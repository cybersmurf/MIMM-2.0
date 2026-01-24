#!/usr/bin/env python3

import os
import re
from pathlib import Path

def fix_blank_lines_around_fences(content):
    """Opravit MD031: Blank lines around fenced code blocks"""
    lines = content.split('\n')
    result = []
    i = 0
    while i < len(lines):
        line = lines[i]
        
        # Detekovat počátek code fence
        if re.match(r'^```(bash|csharp|python|razor|json|yaml|text|html|dockerfile|sql)?', line):
            # Přidat blank line před fence pokud chybí
            if result and result[-1].strip() != '':
                result.append('')
            
            result.append(line)
            i += 1
            
            # Přidat řádky code bloku
            while i < len(lines) and not lines[i].startswith('```'):
                result.append(lines[i])
                i += 1
            
            # Přidat closing fence
            if i < len(lines) and lines[i].startswith('```'):
                result.append(lines[i])
                i += 1
                
                # Přidat blank line po fence pokud chybí
                if i < len(lines) and lines[i].strip() != '':
                    result.append('')
        else:
            result.append(line)
            i += 1
    
    return '\n'.join(result)

def fix_ordered_list_prefix(content):
    """Opravit MD029: Ordered list item prefix"""
    lines = content.split('\n')
    result = []
    i = 0
    
    while i < len(lines):
        line = lines[i]
        
        # Detekovat ordered list (1., 2., 3., atd.)
        match = re.match(r'^(\s*)(\d+)\.\s+(.+)$', line)
        if match:
            indent = match.group(1)
            # Zjistit první number v sequenci
            first_num = 1
            j = i
            while j > 0 and re.match(r'^' + re.escape(indent) + r'\d+\.\s+', lines[j-1]):
                j -= 1
            
            # Spočítat pozici v seznamu
            list_items = []
            k = j
            while k < len(lines) and (re.match(r'^' + re.escape(indent) + r'\d+\.\s+', lines[k]) or lines[k].strip() == ''):
                if re.match(r'^' + re.escape(indent) + r'\d+\.\s+', lines[k]):
                    list_items.append(k)
                k += 1
            
            # Opravit čísla v seznamu
            for idx, list_item_idx in enumerate(list_items, 1):
                if list_item_idx >= i:
                    old_match = re.match(r'^(\s*)(\d+)(\.\s+.+)$', lines[list_item_idx])
                    if old_match:
                        lines[list_item_idx] = f'{old_match.group(1)}{idx}{old_match.group(3)}'
        
        result.append(lines[i])
        i += 1
    
    return '\n'.join(result)

def wrap_bare_urls(content):
    """Opravit MD034: Bare URLs - obalit do []()"""
    # Vzor pro bare URLs
    pattern = r'(https?://[^\s\)\]]+)(?!\))'
    
    def replace_url(match):
        url = match.group(1)
        # Pokud je již obalena, neopravovat
        return f'[{url}]({url})'
    
    # Najít alle URLs které nejsou již obalené
    result = []
    parts = re.split(r'(\[.+?\]\(.+?\))', content)  # Rozdělit již obalené URLs
    
    for i, part in enumerate(parts):
        if i % 2 == 0:  # Není obalit URL
            part = re.sub(pattern, replace_url, part)
        result.append(part)
    
    return ''.join(result)

def fix_inline_html(content):
    """Opravit MD033: Inline HTML <T>, <EntryList>"""
    # Nahradit <T>, <EntryList>, atd. backticks
    content = re.sub(r'<([A-Z][a-zA-Z]+)>', r'`\1`', content)
    return content

def fix_line_length(content, max_length=120):
    """Opravit MD013: Dlouhé řádky"""
    lines = content.split('\n')
    result = []
    
    for line in lines:
        # Pokud je řádek příliš dlouhý
        if len(line) > max_length:
            # Pokud je to URL nebo link, přeskočit
            if '](http' in line or 'https://' in line or line.strip().startswith('['):
                result.append(line)
            # Pokud je to seznam nebo text, pokusit se zalomit
            elif ' ' in line[max_length-10:max_length+10]:
                # Najít poslední mezeru před max_length
                split_point = line.rfind(' ', 0, max_length)
                if split_point > 0:
                    result.append(line[:split_point])
                    result.append(line[split_point+1:])
                else:
                    result.append(line)
            else:
                result.append(line)
        else:
            result.append(line)
    
    return '\n'.join(result)

def fix_markdown_file(filepath):
    """Opravit markdown soubor"""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    original_content = content
    
    # Aplikovat opravy
    content = fix_blank_lines_around_fences(content)
    content = fix_ordered_list_prefix(content)
    content = wrap_bare_urls(content)
    content = fix_inline_html(content)
    content = fix_line_length(content)
    
    # Opravit trailing spaces
    lines = content.split('\n')
    lines = [line.rstrip() for line in lines]
    content = '\n'.join(lines)
    
    # Přidat single newline na konec
    if content and not content.endswith('\n'):
        content += '\n'
    
    if content != original_content:
        return True, content
    return False, content

def main():
    root_dir = '/Users/petrsramek/AntigravityProjects/MIMM-2.0'
    ignore_paths = {'tools/ExtraTool'}
    
    fixed_files = []
    
    for md_file in Path(root_dir).glob('**/*.md'):
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
