#!/usr/bin/env python3
"""
extract_i18n_strings.py - Extract translatable strings from C# Godot source

Scans src/**/*.cs files for hardcoded user-facing strings (Label.Text, Button.Text,
interpolated string assignments, button factory calls, etc.) and generates a
Godot-compatible PO file at locale/en.po with auto-suggested keys.

Usage:
    python3 scripts/extract_i18n_strings.py [--src src] [--output locale/en.po] [--dry-run]

Patterns extracted:
    Text = "..."            (Label / Button / LineEdit etc.)
    Text = $"Score: {var}"  (interpolated, converted to "Score: {0}")
    Title = "..." / Hint = "..." / PlaceholderText = "..."
    MakeButton("...", ...)  (button factory call, extracts 1st string arg)

Patterns skipped:
    Name = "..."            (Node names like TitleScreen, ScoreLabel)
    Color(...) literals
    File paths (res://, assets/, .tscn, .png)
    Asset names (gem_purple, bg_title)
    print/log messages
    Single-char strings, pure numbers

Auto-suggested key naming (per docs/design/i18n-strategy.md §1.2):
    lowercase + dot notation
    screen.subtype.field
    Example: brand.magic_match / title.play / endscreen.you_won / hud.score

Output:
    locale/en.po - PO file with msgid + occurrences + translator comments
    Prints stats to stdout (X strings from Y files, Z potential duplicates)

Dependencies:
    polib (Python PO library) - install via `pip install polib`

Author: Christine (MagicStudio Artist)
Date: 2026-10-06 (D+13)
"""

import argparse
import re
import sys
from pathlib import Path

import polib


# Strings to skip (asset paths, common code patterns)
SKIP_PATTERNS = [
    r'^res://',
    r'^assets/',
    r'\.(png|jpg|jpeg|svg|ttf|otf|tscn|wav|mp3|ogg)$',
    r'^[A-Z][a-zA-Z]*Screen$',
    r'^[A-Z][a-zA-Z]*Label$',
    r'^[A-Z][a-zA-Z]*Button$',
    r'^[A-Z][a-zA-Z]*Container$',
    r'^[A-Z][a-zA-Z]*View$',
    r'^[A-Z][a-zA-Z]*Manager$',
    r'^HUD$',
    r'^[A-Z][a-zA-Z]+Feature$',
]


def is_skippable(text: str) -> bool:
    """Return True if string should be skipped (not user-facing)."""
    if not text or len(text) < 2:
        return True
    for pat in SKIP_PATTERNS:
        if re.match(pat, text, re.IGNORECASE):
            return True
    if text.isdigit() or len(text) == 1:
        return True
    if text.isupper() and ('_' in text or text.isalpha()):
        return True
    return False


def to_snake_case(text: str) -> str:
    """Convert label text to snake_case key component."""
    s = re.sub(r'([a-z])([A-Z])', r'\1_\2', text)
    s = re.sub(r'[^a-zA-Z0-9 ]', '', s)
    return '_'.join(s.lower().split())


def extract_interpolated(text: str) -> tuple[str, int]:
    """Convert interpolated string template to PO format with {0} {1} placeholders.

    Example: "Score: {var.Score}" -> ("Score: {0}", 1)
    """
    placeholder_pattern = re.compile(r'\{[^}]+\}')
    placeholders = placeholder_pattern.findall(text)
    counter = [0]

    def repl(_m):
        result = '{' + str(counter[0]) + '}'
        counter[0] += 1
        return result

    new_text = placeholder_pattern.sub(repl, text)
    return new_text, counter[0]


def normalize_msgid(msgid: str) -> str:
    """Normalize msgid for dedup: 'Score: 0' -> 'Score: {0}'.

    Treats trailing numeric literal after a colon/space as a {0} placeholder,
    since this is a common pattern in HUD initial-display strings
    (e.g., 'Score: 0' before the first runtime update).
    """
    return re.sub(r':\s+\d+\s*$', ': {0}', msgid)


def suggest_key(text: str, file_path: Path) -> str:
    """Auto-suggest i18n key based on content + file context."""
    text_lower = text.lower().strip()
    text_clean = text_lower.replace('{0}', '').strip()

    # Specific keywords (per docs/design/i18n-strategy.md §1.1)
    if 'magic match' in text_clean or 'magicmatch' in text_clean:
        return 'brand.magic_match'
    if 'play' in text_clean and 'again' in text_clean:
        return 'endscreen.play_again'
    if 'play' in text_clean:
        return 'title.play'
    if 'quit' in text_clean:
        return 'title.quit'
    if 'main menu' in text_clean:
        return 'endscreen.main_menu'
    if 'won' in text_clean and '{' not in text_clean:
        return 'endscreen.you_won'
    if 'game over' in text_clean:
        return 'endscreen.game_over'
    if 'score' in text_clean:
        return 'hud.score'
    if 'moves' in text_clean:
        return 'hud.moves'

    # Generic fallback: filename + first 20 chars
    file_key = file_path.stem.lower().replace('.cs', '')
    text_key = to_snake_case(text[:20])
    return f'{file_key}.{text_key}'


def scan_file(file_path: Path) -> list:
    """Scan a single .cs file and extract hardcoded strings."""
    entries = []
    with open(file_path, 'r', encoding='utf-8') as f:
        lines = f.readlines()

    for i, line in enumerate(lines, start=1):
        stripped = line.strip()
        # Skip comment-only lines (//, ///, /*, *)
        if stripped.startswith('//') or stripped.startswith('*') or stripped.startswith('/*'):
            continue

        # Pattern 1: .Text = "..." or Text = "..." (object initializer or method call)
        # Match both `.Text = "..."` (method call) and `Text = "..."` (initializer)
        # Also handle ternary like: Text = cond ? "Yes" : "No" -> extract both strings
        m = re.search(r'(?:\.|^|\{)\s*Text\s*=\s*(.+)', line)
        if m:
            rhs = m.group(1).rstrip(',').rstrip(';').rstrip()
            # Find all string literals in the right-hand side
            string_literals = re.findall(r'\$?"((?:[^"\\]|\\.)*)"', rhs)
            for raw_text in string_literals:
                # Skip if preceded by $ (interpolated)
                # (polib doesn't need to know about interpolation in the source)
                text, placeholders = extract_interpolated(raw_text)
                if not is_skippable(text):
                    entries.append({
                        'msgid': text,
                        'file': str(file_path),
                        'line': i,
                        'context': 'Text assignment',
                        'placeholders': placeholders,
                    })
            continue

        # Pattern 2: Title = "..." / Hint = "..." / PlaceholderText = "..."
        m = re.search(r'\b(Title|Hint|PlaceholderText|Placeholder)\s*=\s*"([^"]*)"', line)
        if m:
            text = m.group(2)
            if not is_skippable(text):
                entries.append({
                    'msgid': text,
                    'file': str(file_path),
                    'line': i,
                    'context': f'{m.group(1)} assignment',
                    'placeholders': 0,
                })
            continue

        # Pattern 3: MakeButton("...", ...)
        m = re.search(r'MakeButton\(\s*"([^"]+)"', line)
        if m:
            text = m.group(1)
            if not is_skippable(text):
                entries.append({
                    'msgid': text,
                    'file': str(file_path),
                    'line': i,
                    'context': 'Button factory call',
                    'placeholders': 0,
                })
            continue

        # Pattern 4: Tr("...", ...) or Tr($"...{var}...", ...) — added v1.1
        # when scenes migrated from `Text = "..."` to `Text = Tr("...")`
        # (per Godot TranslationServer API). Captures the first string
        # literal argument. Multiple Tr() on same line (ternary) handled by
        # the broader regex scan; this catches the common case.
        m = re.search(r'\bTr\s*\(\s*\$?\"((?:[^\"\\]|\\.)*)\"', line)
        if m:
            raw_text = m.group(1)
            text, placeholders = extract_interpolated(raw_text)
            if not is_skippable(text):
                entries.append({
                    'msgid': text,
                    'file': str(file_path),
                    'line': i,
                    'context': 'Tr() translation call',
                    'placeholders': placeholders,
                })
            continue

    return entries


def dedupe_keys(entries: list) -> dict:
    """Group entries by suggested key, dedupe msgid; resolve conflicts.

    Uses normalize_msgid() to merge entries with same UI intent but different
    msgid forms (e.g., 'Score: 0' literal vs 'Score: {0}' interpolated template).
    """
    by_key = {}
    for entry in entries:
        file_path = Path(entry['file'])
        key = suggest_key(entry['msgid'], file_path)
        # Normalize both msgids for comparison
        existing_normalized = normalize_msgid(by_key[key]['msgid']) if key in by_key else None
        new_normalized = normalize_msgid(entry['msgid'])

        if key not in by_key:
            by_key[key] = entry
        elif by_key[key]['msgid'] == entry['msgid']:
            # Duplicate — merge occurrences
            by_key[key]['file'] = f"{by_key[key]['file']}, {entry['file']}:{entry['line']}"
        elif existing_normalized == new_normalized:
            # Same UI intent but different msgid form (e.g., 'Score: 0' vs 'Score: {0}')
            # Prefer the template form (with placeholders) as canonical msgid
            if '{' in entry['msgid']:
                by_key[key]['msgid'] = entry['msgid']
            by_key[key]['file'] = f"{by_key[key]['file']}, {entry['file']}:{entry['line']}"
        else:
            # Different msgid for same key — append counter
            base_key = key
            counter = 2
            while f'{base_key}_{counter}' in by_key:
                counter += 1
            new_key = f'{base_key}_{counter}'
            by_key[new_key] = entry
    return by_key


def generate_po(by_key: dict, output_path: Path, language: str = 'en') -> None:
    """Generate Godot-compatible PO file."""
    po = polib.POFile()
    po.metadata = {
        'Project-Id-Version': 'Magic Match v1.1',
        'Content-Type': 'text/plain; charset=UTF-8',
        'Language': language,
        'MIME-Version': '1.0',
        'Content-Transfer-Encoding': '8bit',
        'Generated-By': 'scripts/extract_i18n_strings.py (Christine D+13)',
    }

    for key in sorted(by_key.keys()):
        entry = by_key[key]
        po_entry = polib.POEntry(
            msgid=entry['msgid'],
            msgstr=entry['msgid'] if language == 'en' else '',
            occurrences=[(entry['file'], str(entry['line']))],
            comment=entry['context'],
            tcomment=f'i18n key: {key}',
        )
        if entry.get('placeholders', 0) > 0:
            po_entry.tcomment += f' ({entry["placeholders"]} placeholder{"s" if entry["placeholders"] != 1 else ""})'
        po.append(po_entry)

    output_path.parent.mkdir(parents=True, exist_ok=True)
    po.save(str(output_path))


def main():
    parser = argparse.ArgumentParser(description='Extract translatable strings from C# Godot source')
    parser.add_argument('--src', default='src', help='Source directory (default: src)')
    parser.add_argument('--output', default='locale/en.po', help='Output PO file (default: locale/en.po)')
    parser.add_argument('--language', default='en', help='Target language code (default: en)')
    parser.add_argument('--dry-run', action='store_true', help='Print to stdout instead of writing')
    args = parser.parse_args()

    src_path = Path(args.src)
    output_path = Path(args.output)

    if not src_path.exists():
        print(f'ERROR: source directory {src_path} does not exist', file=sys.stderr)
        sys.exit(1)

    cs_files = sorted(src_path.rglob('*.cs'))
    if not cs_files:
        print(f'WARNING: no .cs files found in {src_path}', file=sys.stderr)

    all_entries = []
    for cs_file in cs_files:
        entries = scan_file(cs_file)
        all_entries.extend(entries)

    by_key = dedupe_keys(all_entries)

    print('=== i18n String Extraction Report ===')
    print(f'Source: {src_path} ({len(cs_files)} .cs files)')
    print(f'Extracted: {len(all_entries)} string occurrences')
    print(f'Unique keys: {len(by_key)}')
    print(f'Output: {output_path}')
    print(f'Language: {args.language}')
    print()
    print('--- Strings by i18n key ---')
    for key in sorted(by_key.keys()):
        entry = by_key[key]
        placeholders = entry.get('placeholders', 0)
        ph_str = f' ({placeholders} placeholder{"s" if placeholders != 1 else ""})' if placeholders else ''
        print(f'  {key:35s} = "{entry["msgid"]}"{ph_str}')
        print(f'    {entry["context"]} @ {entry["file"]}:{entry["line"]}')

    if not args.dry_run:
        generate_po(by_key, output_path, args.language)
        print(f'\n✅ Generated {output_path} ({len(by_key)} entries)')


if __name__ == '__main__':
    main()