#!/usr/bin/env python3

import re
from pathlib import Path

# Data o dlouhých řádcích a jejich opravách
LONG_LINES_FIXES = [
    (".github/agents/dotnet-blazor-specialist-agent.agent.md", 8,
     "description: Enterprise-grade AI developer pro .NET 9, ASP.NET Core, Blazor WASM a PostgreSQL",
     ["description: Enterprise-grade AI developer pro .NET 9, ASP.NET Core,",
      "             Blazor WASM a PostgreSQL"]),
    
    ("ACTION_3_COMPLETION_REPORT.md", 60,
     "- **Paginated table** with MudBlazor components\n- **Columns:**\n  - Album art (MudAvatar with cover image or music note icon)",
     ["- **Paginated table** with MudBlazor components",
      "- **Columns:**",
      "  - Album art (MudAvatar with cover image or music note",
      "    icon)"]),
]

def apply_line_fix(filepath, line_number, old_text, new_lines):
    """Aplikuj opravu na dlouhý řádek"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Pokus se najít a nahradit text
    if old_text in content:
        content = content.replace(old_text, '\n'.join(new_lines), 1)
        with open(filepath, 'w', encoding='utf-8') as f:
            f.write(content)
        return True
    return False

def main():
    root_dir = '/Users/petrsramek/AntigravityProjects/MIMM-2.0'
    
    for filename, line_num, old_text, new_lines in LONG_LINES_FIXES:
        filepath = Path(root_dir) / filename
        
        if not filepath.exists():
            print(f"✗ File not found: {filepath}")
            continue
        
        success = apply_line_fix(str(filepath), line_num, old_text, new_lines)
        
        if success:
            print(f"✓ {filename}:{line_num} - Wrapped long line")
        else:
            print(f"- {filename}:{line_num} - Could not find text")

if __name__ == '__main__':
    main()
