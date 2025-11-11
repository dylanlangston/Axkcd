import * as core from '@actions/core';
import { getOctokit, context } from '@actions/github';
import { Buffer } from 'node:buffer';

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

  async listArtifacts(): Promise<any[]> {
    const response = await this.octokit.rest.actions.listArtifactsForRepo({
      ...this.ctx.repo,
      per_page: 100
    });
    return response.data.artifacts;
  }

  async downloadArtifact(artifactId: number): Promise<ArrayBuffer> {
    const response = await this.octokit.rest.actions.downloadArtifact({
      ...this.ctx.repo,
      artifact_id: artifactId,
      archive_format: 'zip'
    });
    return response.data as ArrayBuffer;
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

export class ArtifactManager {
  constructor(private githubService: GitHubService) {}

  async downloadAndUpload(
    releaseId: number,
    version: string,
    assetPrefix: string
  ): Promise<void> {
    const artifacts = await this.githubService.listArtifacts();
    const matchingArtifacts = artifacts.filter(artifact =>
      artifact.name.startsWith(assetPrefix)
    );

    if (matchingArtifacts.length === 0) {
      core.warning(`No artifacts found with prefix "${assetPrefix}"`);
      return;
    }

    core.info(`Found ${matchingArtifacts.length} matching artifacts`);

    // Get existing assets to avoid duplicates
    const existingAssets = await this.githubService.listReleaseAssets(releaseId);

    for (const artifact of matchingArtifacts) {
      try {
        core.info(`Processing artifact: ${artifact.name}`);

        // Check if asset already exists
        const existingAsset = existingAssets.find(
          asset => asset.name === artifact.name
        );

        if (existingAsset) {
          core.info(`Asset ${artifact.name} already exists. Deleting old version...`);
          await this.githubService.deleteReleaseAsset(existingAsset.id);
        }

        // Download artifact
        const artifactData = await this.githubService.downloadArtifact(artifact.id);
        const buffer = Buffer.from(artifactData);

        // Upload to release
        await this.githubService.uploadAsset(
          releaseId,
          artifact.name,
          buffer
        );

        core.info(`Uploaded ${artifact.name} to release`);
      } catch (error) {
        core.error(`Failed to process artifact ${artifact.name}: ${error}`);
        throw error;
      }
    }
  }
}

export async function run(): Promise<void> {
  try {
    const version = core.getInput('version', { required: true });
    const token = core.getInput('github-token', { required: true });
    const assetPrefix = core.getInput('asset-prefix');

    core.info(`Creating/updating release for version ${version}`);
    core.info(`Asset prefix: ${assetPrefix}`);

    const octokit = getOctokit(token);
    const githubService = new GitHubService(octokit, context);
    const releaseManager = new ReleaseManager(githubService);
    const artifactManager = new ArtifactManager(githubService);

    const release = await releaseManager.createOrUpdate(version, context.sha);
    core.info(`Release ID: ${release.id}`);

    await artifactManager.downloadAndUpload(release.id, version, assetPrefix);

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