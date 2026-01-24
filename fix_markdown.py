#!/usr/bin/env python3

import os
import re
import sys
from pathlib import Path

# Heuristika pro určení programovacího jazyka
def detect_language(code_block):
    """Detekuje programovací jazyk na základě obsahu kódu"""
    code_lines = code_block.strip().split('\n')
    if not code_lines:
        return 'text'
    
    first_line = code_lines[0]
    
    # Bash/shell commands
    if any(cmd in code_block for cmd in ['dotnet ', 'docker', 'npm ', 'git ', 'curl', 'bash', '#!/bin', 'echo ', 'mkdir ', 'cd ']):
        return 'bash'
    
    # C# code
    if any(pattern in code_block for pattern in ['public class', 'public async', 'using ', 'namespace ', '.csproj', 'GetEntriesAsync', 'Task<', 'CreateEntryModel']):
        return 'csharp'
    
    # Razor components
    if any(pattern in code_block for pattern in ['@page', '@inject', '@implements', '@foreach', '<EntryList', '<MudBlazor', '.razor']):
        return 'razor'
    
    # JSON
    if first_line.strip().startswith('{') or code_block.strip().startswith('['):
        return 'json'
    
    # YAML
    if first_line.strip().endswith(':'):
        return 'yaml'
    
    # Dockerfile
    if any(pattern in code_block for pattern in ['FROM ', 'RUN ', 'COPY ', 'CMD ', 'EXPOSE ', 'ENV ']):
        return 'dockerfile'
    
    # HTML/XML
    if code_block.strip().startswith('<') or '</html>' in code_block or '<?xml' in code_block:
        return 'html'
    
    # XML
    if '<?xml' in code_block or code_block.strip().startswith('<') and ('</' in code_block or '/>' in code_block):
        return 'xml'
    
    # SQL
    if any(pattern in code_block.upper() for pattern in ['SELECT ', 'INSERT ', 'UPDATE ', 'CREATE TABLE', 'DELETE ']):
        return 'sql'
    
    # Python
    if any(pattern in code_block for pattern in ['def ', 'import ', 'if __name__', 'print(', 'class ']):
        return 'python'
    
    # Default to text
    return 'text'

def fix_markdown_file(filepath):
    """Opravit markdown soubor"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Opravit prázdné code blocky bez jazyka
    # Vzor: ``` (bez jazyka) ... ```
    pattern = r'```\n(.*?)\n```'
    
    def replace_code_block(match):
        code_block = match.group(1)
        if code_block.strip():
            lang = detect_language(code_block)
            return f'```{lang}\n{code_block}\n```'
        return match.group(0)
    
    content = re.sub(pattern, replace_code_block, content, flags=re.DOTALL)
    
    # Opravit MD033: Inline HTML tagy <T>, <EntryList> - nahradit backticks
    content = re.sub(r'<([A-Z][a-zA-Z]+)>', r'`\1`', content)  # <T> → `T`, <EntryList> → `EntryList`
    
    # Opravit trailing spaces (MD009)
    lines = content.split('\n')
    lines = [line.rstrip() for line in lines]
    content = '\n'.join(lines)
    
    # Přidat single newline na konec (MD047)
    if content and not content.endswith('\n'):
        content += '\n'
    
    # Zalogovat změny
    if content != original_content:
        return True, content
    return False, content

def main():
    root_dir = '/Users/petrsramek/AntigravityProjects/MIMM-2.0'
    
    # Ignorovat soubory
    ignore_paths = {'tools/ExtraTool'}
    
    fixed_files = []
    error_count = 0
    
    for md_file in Path(root_dir).glob('**/*.md'):
        # Přeskočit ignorované soubory
        if any(ignore_str in str(md_file) for ignore_str in ignore_paths):
            continue
        
        changed, new_content = fix_markdown_file(str(md_file))
        
        if changed:
            with open(str(md_file), 'w', encoding='utf-8') as f:
                f.write(new_content)
            fixed_files.append(str(md_file))
            print(f"✓ {md_file.relative_to(root_dir)}")
    
    print(f"\n✓ Opraveno {len(fixed_files)} souborů")

if __name__ == '__main__':
    main()
