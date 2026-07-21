import { cp, mkdtemp, readdir, rm } from "node:fs/promises";
import { tmpdir } from "node:os";
import { join, basename } from "node:path";
import { promisify } from "node:util";
import { execFile } from "node:child_process";

const execFileAsync = promisify(execFile);
const repositoryRoot = new URL("..", import.meta.url).pathname.replace(/^\/(?=\w:)/, "");
const destinationRoot = join(repositoryRoot, ".codex", "skills");
const force = process.argv.includes("--force");

const skillSources = [
  {
    repository: "https://github.com/angular/skills.git",
    paths: ["angular-developer", "angular-new-app"],
  },
  {
    repository: "https://github.com/vercel-labs/skills.git",
    paths: ["skills/find-skills"],
  },
  {
    repository: "https://github.com/vercel-labs/agent-skills.git",
    paths: [
      "skills/react-best-practices",
      "skills/react-native-skills",
      "skills/react-view-transitions",
      "skills/web-design-guidelines",
      "skills/writing-guidelines",
    ],
  },
];

async function runGit(args, cwd) {
  await execFileAsync("git", args, { cwd, stdio: "inherit" });
}

async function installSource(source, temporaryRoot) {
  const checkoutRoot = join(temporaryRoot, basename(source.repository, ".git"));

  await runGit(
    ["clone", "--depth", "1", "--filter=blob:none", "--no-checkout", source.repository, checkoutRoot],
    temporaryRoot,
  );
  await runGit(["sparse-checkout", "init", "--cone"], checkoutRoot);
  await runGit(["sparse-checkout", "set", ...source.paths], checkoutRoot);
  await runGit(["checkout"], checkoutRoot);

  for (const sourcePath of source.paths) {
    const skillName = basename(sourcePath);
    const sourceDirectory = join(checkoutRoot, sourcePath);
    const destinationDirectory = join(destinationRoot, skillName);
    const exists = await readdir(destinationDirectory).then(() => true).catch(() => false);

    if (exists && !force) {
      console.log(`Skipping ${skillName}; already exists. Use --force to replace it.`);
      continue;
    }

    if (exists) {
      await rm(destinationDirectory, { recursive: true, force: true });
    }

    await cp(sourceDirectory, destinationDirectory, { recursive: true });
    console.log(`Installed ${skillName}`);
  }
}

const temporaryRoot = await mkdtemp(join(tmpdir(), "sitewatch-skills-"));

try {
  for (const source of skillSources) {
    await installSource(source, temporaryRoot);
  }
} finally {
  await rm(temporaryRoot, { recursive: true, force: true });
}
