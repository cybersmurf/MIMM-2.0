#!/usr/bin/env python3

import re
from pathlib import Path

# Data o chybách a jejich opravách
FIXES = [
    # (file, line_number, pattern, replacement, description)
    ("ACTION_1_COMPLETION.md", 48, r'^```$', '```bash', "MD040: Add bash language"),
    ("ACTION_1_COMPLETION.md", 79, r'^```$', '```text', "MD040: Add text language"),
    ("ACTION_1_COMPLETION.md", 89, r'^```$', '```bash', "MD040: Add bash language"),
    ("ACTION_1_COMPLETION.md", 98, r'^```$', '```text', "MD040: Add text language"),
    ("ACTION_2_E2E_TEST.md", 48, r'^```$', '```bash', "MD040: Add bash language"),
    ("ACTION_2_E2E_TEST.md", 71, r'^```$', '```yaml', "MD040: Add yaml language"),
    ("ACTION_3_COMPLETION_REPORT.md", 120, r'^```$', '```csharp', "MD040: Add csharp language"),
    ("DEPLOYMENT_PLAN.md", 30, r'^```$', '```bash', "MD040: Add bash language"),
    ("DEPLOYMENT_PLAN.md", 51, r'^```$', '```bash', "MD040: Add bash language"),
    ("E2E_TEST_EXECUTION.md", 12, r'^```$', '```bash', "MD040: Add bash language"),
    ("E2E_TEST_EXECUTION.md", 37, r'^```$', '```bash', "MD040: Add bash language"),
    ("E2E_TEST_EXECUTION.md", 52, r'^```$', '```yaml', "MD040: Add yaml language"),
    ("MIGRATION_GUIDE.md", 162, r'^```$', '```python', "MD040: Add python language"),
    ("PROJECT_ANALYSIS_2026.md", 20, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 84, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 101, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 121, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 139, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 173, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 202, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 231, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 260, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 278, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 302, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 319, r'^```$', '```text', "MD040: Add text language"),
    ("PROJECT_ANALYSIS_2026.md", 336, r'^```$', '```bash', "MD040: Add bash language"),
    ("PROJECT_ANALYSIS_2026.md", 347, r'^```$', '```text', "MD040: Add text language"),
    ("READY_FOR_GITHUB.md", 16, r'^```$', '```bash', "MD040: Add bash language"),
    ("SETUP_GUIDE.md", 519, r'^```$', '```bash', "MD040: Add bash language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 33, r'^```$', '```bash', "MD040: Add bash language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 148, r'^```$', '```text', "MD040: Add text language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 159, r'^```$', '```bash', "MD040: Add bash language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 170, r'^```$', '```text', "MD040: Add text language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 183, r'^```$', '```bash', "MD040: Add bash language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 193, r'^```$', '```text', "MD040: Add text language"),
    ("SPRINT_1_DAY_1_SUMMARY.md", 201, r'^```$', '```bash', "MD040: Add bash language"),
    ("SPRINT_TIMELINE.md", 3, r'^```$', '```text', "MD040: Add text language"),
    ("STRATEGIC_ACTION_PLAN_2026.md", 353, r'^```$', '```bash', "MD040: Add bash language"),
]

def apply_fix(filepath, line_number, pattern, replacement):
    """Aplikuj opravu na konkrétní řádek"""
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    # Line numbers jsou 1-based
    line_idx = line_number - 1
    if line_idx < len(lines):
        old_line = lines[line_idx]
        new_line = re.sub(pattern, replacement, old_line.rstrip()) + '\n'
        if old_line != new_line:
            lines[line_idx] = new_line
            return True, lines
    return False, lines

def main():
    root_dir = '/Users/petrsramek/AntigravityProjects/MIMM-2.0'
    
    fixed_files = set()
    
    for filename, line_num, pattern, replacement, description in FIXES:
        filepath = Path(root_dir) / filename
        
        if not filepath.exists():
            print(f"✗ File not found: {filepath}")
            continue
        
        changed, lines = apply_fix(str(filepath), line_num, pattern, replacement)
        
        if changed:
            with open(str(filepath), 'w', encoding='utf-8') as f:
                f.writelines(lines)
            fixed_files.add(filename)
            print(f"✓ {filename}:{line_num} - {description}")
        else:
            print(f"- {filename}:{line_num} - No change")
    
    print(f"\n✓ Fixed {len(fixed_files)} files")

if __name__ == '__main__':
    main()
