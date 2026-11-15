import { execFileSync } from 'node:child_process';
import { existsSync, readFileSync, readdirSync } from 'node:fs';
import { dirname, join } from 'node:path';
import process from 'node:process';
import { fileURLToPath } from 'node:url';

class PackVerifier {
  static allowed = [
    /^package\.json$/,
    /^README\.md$/,
    /^CHANGELOG\.md$/,
    /^LICENSE$/,
    /^dist\/[\w.-]+\.(js|cjs|d\.ts|d\.cts)$/,
    /^schemas\/[\w-]+\.md$/,
    /^schemas\/[\w.-]+\.schema\.json$/,
  ];

  static required = [
    'package.json',
    'README.md',
    'CHANGELOG.md',
    'LICENSE',
    'dist/index.js',
    'dist/index.cjs',
    'dist/index.d.ts',
    'dist/index.d.cts',
    'dist/cli.js',
    'dist/vitest.js',
    'dist/vitest.cjs',
    'dist/vitest.d.ts',
    'dist/vitest.d.cts',
    'schemas/coherent.md',
    'schemas/extract.md',
    'schemas/crucible.config.schema.json',
  ];

  static sharedFile = /^(LICENSE|schemas\/.+)$/;

  static packageDir = dirname(dirname(fileURLToPath(import.meta.url)));

  static repoDir = dirname(PackVerifier.packageDir);

  static vitestEntries = ['dist/vitest.js', 'dist/vitest.cjs'];

  static vitestImport =
    /(from\s*["']vitest["'])|(\bimport\s*\(?\s*["']vitest["'])|(require\(\s*["']vitest["']\s*\))/;

  packedFiles() {
    const output = execFileSync('npm', ['pack', '--dry-run', '--json', '--ignore-scripts'], {
      encoding: 'utf8',
    });
    const [manifest] = JSON.parse(output);
    return manifest.files.map((file) => file.path);
  }

  problems(files) {
    const unexpected = files
      .filter((file) => !PackVerifier.allowed.some((pattern) => pattern.test(file)))
      .map((file) => `unexpected file in tarball: ${file}`);
    const missing = PackVerifier.required
      .filter((file) => !files.includes(file))
      .map((file) => `required file missing from tarball: ${file}`);
    return [
      ...unexpected,
      ...missing,
      ...this.sharedFileProblems(files),
      ...this.vitestProblems(files),
    ];
  }

  sharedFileProblems(files) {
    const stale = files
      .filter((file) => PackVerifier.sharedFile.test(file))
      .filter((file) => !this.matchesRepository(file))
      .map((file) => `${file} differs from the repository's ../${file}; run npm run build`);
    const unshipped = readdirSync(join(PackVerifier.repoDir, 'schemas'))
      .map((name) => `schemas/${name}`)
      .filter((file) => PackVerifier.allowed.some((pattern) => pattern.test(file)))
      .filter((file) => !files.includes(file))
      .map((file) => `repository's ../${file} missing from tarball; run npm run build`);
    return [...stale, ...unshipped];
  }

  matchesRepository(file) {
    const source = join(PackVerifier.repoDir, file);
    return (
      existsSync(source) &&
      readFileSync(join(PackVerifier.packageDir, file)).equals(readFileSync(source))
    );
  }

  vitestProblems(files) {
    const bundles = files.filter((file) => /^dist\/[\w.-]+\.(js|cjs)$/.test(file));
    return bundles.flatMap((file) => {
      const importsVitest = PackVerifier.vitestImport.test(readFileSync(file, 'utf8'));
      const isVitestEntry = PackVerifier.vitestEntries.includes(file);
      if (isVitestEntry && !importsVitest) {
        return [`${file} must import vitest as an external`];
      }
      if (!isVitestEntry && importsVitest) {
        return [`${file} must not import vitest; only the crucible-ai/vitest entry may`];
      }
      return [];
    });
  }

  run() {
    const files = this.packedFiles();
    const problems = this.problems(files);
    if (problems.length > 0) {
      process.stderr.write(`${problems.join('\n')}\n`);
      process.exitCode = 1;
      return;
    }
    process.stdout.write(
      `npm pack contents verified (${files.length} files):\n${files.join('\n')}\n`,
    );
  }
}

new PackVerifier().run();
