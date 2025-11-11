import * as core from '@actions/core';
import { getOctokit, context } from '@actions/github';
import { Buffer } from 'node:buffer';
import * as fs from 'node:fs';
import * as path from 'node:path';
import { glob } from 'glob';

type GitHub = ReturnType<typeof getOctokit>;

interface Release {
  id: number;
  tag_name: string;
  name: string;
  body: string;
  draft: boolean;
  prerelease: boolean;
  [key: string]: any;
}

export class GitHubService {
  constructor(private octokit: GitHub, private ctx: typeof context) {}

  async getReleaseByTag(tag: string): Promise<Release | null> {
    try {
      const response = await this.octokit.rest.repos.getReleaseByTag({
        ...this.ctx.repo,
        tag
      });
      return response.data as Release;
    } catch (error: any) {
      if (error.status === 404) return null;
      throw error;
    }
  }

  async createRelease(params: {
    tag_name: string;
    name: string;
    body: string;
    draft: boolean;
    prerelease: boolean;
    target_commitish: string;
  }): Promise<Release> {
    const response = await this.octokit.rest.repos.createRelease({
      ...this.ctx.repo,
      ...params
    });
    return response.data as Release;
  }

  async updateRelease(params: {
    release_id: number;
    tag_name?: string;
    name?: string;
    body?: string;
    draft?: boolean;
    prerelease?: boolean;
  }): Promise<Release> {
    const response = await this.octokit.rest.repos.updateRelease({
      ...this.ctx.repo,
      ...params
    });
    return response.data as Release;
  }

  async uploadAsset(
    releaseId: number,
    assetName: string,
    assetData: Buffer
  ): Promise<void> {
    await this.octokit.rest.repos.uploadReleaseAsset({
      ...this.ctx.repo,
      release_id: releaseId,
      name: assetName,
      data: assetData as any
    });
  }

  async listReleaseAssets(releaseId: number): Promise<any[]> {
    const response = await this.octokit.rest.repos.listReleaseAssets({
      ...this.ctx.repo,
      release_id: releaseId
    });
    return response.data;
  }

  async deleteReleaseAsset(assetId: number): Promise<void> {
    await this.octokit.rest.repos.deleteReleaseAsset({
      ...this.ctx.repo,
      asset_id: assetId
    });
  }
}

export class ReleaseManager {
  constructor(private githubService: GitHubService) {}

  async createOrUpdate(version: string, sha: string): Promise<Release> {
    const tagName = `v${version}`;
    const existingRelease = await this.githubService.getReleaseByTag(tagName);

    if (existingRelease) {
      core.info(`Release ${tagName} already exists. Updating...`);
      return await this.githubService.updateRelease({
        release_id: existingRelease.id,
        tag_name: tagName,
        name: `Release ${version}`,
        body: `Release for version ${version}`,
        draft: false,
        prerelease: false
      });
    } else {
      core.info(`Creating new release ${tagName}...`);
      return await this.githubService.createRelease({
        tag_name: tagName,
        name: `Release ${version}`,
        body: `Release for version ${version}`,
        draft: false,
        prerelease: false,
        target_commitish: sha
      });
    }
  }
}

export class FileManager {
  constructor(private githubService: GitHubService) {}

  private parseFilePatterns(filesInput: string): string[] {
    return filesInput
      .split('\n')
      .map(line => line.trim())
      .filter(line => line.length > 0 && !line.startsWith('#'));
  }

  async uploadFiles(
    releaseId: number,
    filesInput: string
  ): Promise<void> {
    const patterns = this.parseFilePatterns(filesInput);
    
    if (patterns.length === 0) {
      core.warning('No file patterns provided');
      return;
    }

    core.info(`Processing ${patterns.length} file patterns...`);

    const allFiles: string[] = [];
    for (const pattern of patterns) {
      const matches = await glob(pattern, { nodir: true });
      if (matches.length === 0) {
        core.warning(`No files found for pattern: ${pattern}`);
      } else {
        core.info(`Found ${matches.length} files for pattern: ${pattern}`);
        allFiles.push(...matches);
      }
    }

    if (allFiles.length === 0) {
      core.warning('No files found matching any patterns');
      return;
    }

    core.info(`Total files to upload: ${allFiles.length}`);

    // Get existing assets to avoid duplicates
    const existingAssets = await this.githubService.listReleaseAssets(releaseId);

    for (const filePath of allFiles) {
      try {
        const fileName = path.basename(filePath);
        core.info(`Processing file: ${fileName}`);

        // Check if asset already exists
        const existingAsset = existingAssets.find(
          asset => asset.name === fileName
        );

        if (existingAsset) {
          core.info(`Asset ${fileName} already exists. Deleting old version...`);
          await this.githubService.deleteReleaseAsset(existingAsset.id);
        }

        // Read file
        const fileBuffer = fs.readFileSync(filePath);
        const fileSize = (fileBuffer.length / (1024 * 1024)).toFixed(2);
        core.info(`Uploading ${fileName} (${fileSize} MB)...`);

        // Upload to release
        await this.githubService.uploadAsset(
          releaseId,
          fileName,
          fileBuffer
        );

        core.info(`✓ Uploaded ${fileName}`);
      } catch (error) {
        core.error(`Failed to process file ${filePath}: ${error}`);
        throw error;
      }
    }
  }
}

export async function run(): Promise<void> {
  try {
    const version = core.getInput('version', { required: true });
    const token = core.getInput('github-token', { required: true });
    const files = core.getInput('files', { required: false });

    if (!files) {
      core.setFailed('No files input provided');
      return;
    }

    core.info(`Creating/updating release for version ${version}`);

    const octokit = getOctokit(token);
    const githubService = new GitHubService(octokit, context);
    const releaseManager = new ReleaseManager(githubService);
    const fileManager = new FileManager(githubService);

    const release = await releaseManager.createOrUpdate(version, context.sha);
    core.info(`Release ID: ${release.id}`);

    await fileManager.uploadFiles(release.id, files);

    core.info('Release process completed successfully.');
  } catch (error) {
    if (error instanceof Error) {
      core.setFailed(error.message);
    } else {
      core.setFailed('An unknown error occurred.');
    }
  }
}

// Execute if running as a script
if (require.main === module) {
  run();
}