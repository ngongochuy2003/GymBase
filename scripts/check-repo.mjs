import { readFileSync, statSync } from 'node:fs';
import { fileURLToPath } from 'node:url';

const root = new URL('../', import.meta.url);
const errors = [];
const required = [
  'README.md', '.gitignore', '.gitattributes', '.editorconfig', '.env.example',
  'package.json', 'pnpm-workspace.yaml',
  'apps/README.md', 'apps/mobile/README.md', 'apps/api/README.md',
  'packages/README.md', 'database/README.md',
  'database/diagrams/README.md', 'database/scripts/README.md', 'database/seeds/README.md',
  'infrastructure/README.md', 'infrastructure/docker/README.md',
  'deploy/README.md', 'deploy/development/README.md',
  'docs/architecture/README.md', 'docs/architecture/decisions.md',
  'docs/architecture/roadmap.md', 'docs/business/README.md', 'docs/api/README.md',
  'docs/onboarding/README.md', 'docs/onboarding/repo-01-handoff.md',
  '.github/CODEOWNERS', '.github/workflows/README.md', 'scripts/README.md',
];

for (const path of required) {
  try {
    const stat = statSync(new URL(path, root));
    if (!stat.isFile() || stat.size === 0) errors.push(`Empty or invalid file: ${path}`);
  } catch {
    errors.push(`Missing file: ${path}`);
  }
}

try {
  const manifest = JSON.parse(readFileSync(new URL('package.json', root), 'utf8'));
  if (manifest.name !== 'gymbase' || manifest.private !== true) {
    errors.push('Root package must be named gymbase and private.');
  }
  if (manifest.scripts?.['check:repo'] !== 'node scripts/check-repo.mjs') {
    errors.push('Missing or incorrect check:repo script.');
  }
  const workspace = readFileSync(new URL('pnpm-workspace.yaml', root), 'utf8');
  if (!/^packages:\s*$/m.test(workspace)) errors.push('Missing workspace packages list.');
  for (const pattern of ['apps/*', 'packages/*']) {
    if (!workspace.split(/\r?\n/).some((line) =>
      [`- '${pattern}'`, `- "${pattern}"`, `- ${pattern}`].includes(line.trim()))) {
      errors.push(`Missing workspace pattern: ${pattern}`);
    }
  }
} catch (error) {
  errors.push(`Configuration could not be read: ${error.message}`);
}

if (errors.length) {
  console.error(errors.join('\n'));
  process.exitCode = 1;
} else {
  console.log(`PASS: REPO-01 structure (${required.length} files) at ${fileURLToPath(root)}`);
  console.log('Structure check only. Run pnpm mobile:check for Mobile lint, types and tests.');
}
