import { readFile, writeFile } from 'fs/promises';

function isValidSemver(v: string): boolean {
  return /^[0-9]+\.[0-9]+\.[0-9]+(?:\+[0-9A-Za-z.-]+)?$/.test(v);
}

function convertDotNetVersion(v: string): string {
  const parts = v.split('.');
  if (parts.length === 4) {
    const [major, minor, patch, build] = parts.map((p) => p.trim());
    if ([major, minor, patch, build].some((p) => isNaN(Number(p)))) {
      throw new Error(`Invalid .NET version segment in "${v}"`);
    }
    return `${major}.${minor}.${patch}+${build}`;
  }

  if (parts.length === 3) {
    if (parts.some((p) => isNaN(Number(p.trim())))) {
      throw new Error(`Invalid version segment in "${v}"`);
    }
    return v;
  }

  throw new Error(`Invalid version format "${v}". Expected X.Y.Z or X.Y.Z.W`);
}

async function updatePackageVersion(input: string): Promise<void> {
  const pkgPath = './package.json';
  const pkgData = await readFile(pkgPath, 'utf8');
  const pkg = JSON.parse(pkgData) as { version?: string };

  const formatted = convertDotNetVersion(input);

  if (!isValidSemver(formatted)) {
    throw new Error(`Resulting version "${formatted}" is not valid semver`);
  }

  pkg.version = formatted;
  await writeFile(pkgPath, JSON.stringify(pkg, null, 2) + '\n', 'utf8');

  console.log(`Updated package version to: ${pkg.version}`);
}

async function main(): Promise<void> {
  const input = process.argv[2];
  if (!input) {
    console.error('Usage: bun scripts/set-version.ts <version>');
    process.exit(1);
  }

  try {
    await updatePackageVersion(input.trim());
  } catch (err) {
    console.error(`${(err as Error).message}`);
    process.exit(1);
  }
}

main();
