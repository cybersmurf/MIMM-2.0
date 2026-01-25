import fs from 'node:fs';
import path from 'node:path';

const reportPath = path.resolve(process.cwd(), 'playwright-report', 'report.json');
if (!fs.existsSync(reportPath)) {
  console.error('Report JSON not found at', reportPath);
  process.exit(1);
}

const json = JSON.parse(fs.readFileSync(reportPath, 'utf-8'));

let total = 0, passed = 0, failed = 0, skipped = 0, durationMs = 0;
const failedTests = [];

for (const project of json.projects || []) {
  for (const file of project.suites || []) {
    for (const suite of file.suites || []) {
      for (const test of suite.tests || []) {
        total += 1;
        durationMs += (test.results?.[0]?.duration || 0);
        const status = test.results?.[0]?.status || test.outcome;
        if (status === 'passed') passed += 1;
        else if (status === 'skipped') skipped += 1;
        else {
          failed += 1;
          const err = test.results?.[0]?.error?.message || test.results?.[0]?.error?.stack || '';
          failedTests.push({ title: test.title, file: file?.title, error: (err || '').slice(0, 500) });
        }
      }
    }
  }
}

const secs = (durationMs / 1000).toFixed(1);

const FRONTEND_URL = process.env.FRONTEND_URL || 'http://localhost:5000';
const BACKEND_URL = process.env.BACKEND_URL || 'http://localhost:5001';

let md = '';
md += `# Playwright E2E Summary\n\n`;
md += `- Total: **${total}**\n`;
md += `- Passed: **${passed}**\n`;
md += `- Failed: **${failed}**\n`;
md += `- Skipped: **${skipped}**\n`;
md += `- Duration: **${secs}s**\n`;
md += `- Env: FRONTEND_URL=${FRONTEND_URL}, BACKEND_URL=${BACKEND_URL}\n`;
md += `- Full HTML report: available as CI artifact \"playwright-report\" or GitHub Pages on main\n`;

if (failedTests.length) {
  md += `\n## Failures\n`;
  for (const f of failedTests) {
    md += `- ${f.title} (${f.file})\n`;
    if (f.error) md += `  \n  \n  \n> ${f.error.replace(/\n/g, '\n> ')}\n`;
  }
}

console.log(md);
