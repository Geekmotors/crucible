import { copyFileSync, existsSync, mkdirSync, readdirSync, rmSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

class SharedFileSync {
  static packageDir = dirname(dirname(fileURLToPath(import.meta.url)));

  static repoDir = dirname(SharedFileSync.packageDir);

  static schemaFile = /^([\w-]+\.md|[\w.-]+\.schema\.json)$/;

  syncSchemas() {
    const source = join(SharedFileSync.repoDir, 'schemas');
    const target = join(SharedFileSync.packageDir, 'schemas');
    rmSync(target, { recursive: true, force: true });
    mkdirSync(target);
    readdirSync(source)
      .filter((file) => SharedFileSync.schemaFile.test(file))
      .forEach((file) => copyFileSync(join(source, file), join(target, file)));
  }

  syncLicense() {
    copyFileSync(
      join(SharedFileSync.repoDir, 'LICENSE'),
      join(SharedFileSync.packageDir, 'LICENSE'),
    );
  }

  requireSources() {
    const missing = ['schemas', 'LICENSE']
      .map((name) => join(SharedFileSync.repoDir, name))
      .filter((path) => !existsSync(path));
    if (missing.length > 0) {
      throw new Error(
        `shared files not found (build from a full repository checkout): ${missing.join(', ')}`,
      );
    }
  }

  run() {
    this.requireSources();
    this.syncSchemas();
    this.syncLicense();
  }
}

new SharedFileSync().run();
