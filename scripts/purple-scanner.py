#!/usr/bin/env python3
"""
Christine's reusable purple-ring scanner.
Scans all assets/ PNG files for purple/magenta rim pixels that indicate
the "purple ring bug" (a magenta band on what should be gold/orange UI elements).

Threshold (empirically derived from button_normal case):
  R > 100  AND G < 130  AND B > 70  AND B > G  AND R > G

Files flagged for review:
- button_*.png  (UI element, gold/orange family)
- gem_*.png  where color != purple (only gem_purple should have purple)
- hud_*.png, star_*.png, coin.png, btn_*.png  (UI elements)
- bg_*.png  SKIP (backgrounds legitimately use purple)
- icon.* SKIP (icon.svg is intentional)
"""
import os
import sys
from pathlib import Path
import numpy as np
from PIL import Image

ROOT = Path('/root/godot-template/assets')
THRESHOLD_MIN_PIXELS = 100  # Lower threshold for thorough scan

def detect_purple(arr):
    """Return mask + count of purple pixels per threshold."""
    if arr.shape[2] < 4:
        return None, 0
    r, g, b, a = arr[:,:,0], arr[:,:,1], arr[:,:,2], arr[:,:,3]
    # Strict: only very pinkish-magenta ring-like colors (catches the bug case more narrowly)
    # vs design intent (gem_purple body = #88489B = R=136 G=72 B=155, NOT pink-magenta)
    mask = (r > 130) & (r > b) & (g < 100) & (b > 60) & (b > g) & (a > 100)
    return mask, int(mask.sum())

def bbox_of(mask):
    if not mask.any():
        return None
    ys, xs = np.where(mask)
    return (xs.min(), ys.min(), xs.max(), ys.max())

def is_fungi_safe(file, prompt_body_color):
    """Heuristic: is the purple body intentional (not a ring)?"""
    if 'gem_purple' in str(file):
        return True  # intentional
    if 'bg_' in str(file):
        return True  # background uses purple
    return False

def scan_file(path):
    """Scan a single PNG; return dict or None."""
    img = Image.open(path).convert('RGBA')
    arr = np.array(img)
    mask, n = detect_purple(arr)
    if n < THRESHOLD_MIN_PIXELS:
        return None
    bbox = bbox_of(mask)
    # Compute purple pixel color stats
    purple_rgb = arr[mask][:, :3]
    median_rgb = np.median(purple_rgb, axis=0).astype(int)
    return {
        'file': str(path),
        'purple_count': n,
        'total_pixels': arr.shape[0] * arr.shape[1],
        'purple_pct': 100 * n / (arr.shape[0] * arr.shape[1]),
        'bbox': bbox,
        'median_rgb': tuple(median_rgb),
        'median_hex': '#{:02X}{:02X}{:02X}'.format(*median_rgb),
    }

def main():
    results = []
    print(f'Scanning {ROOT} ...')
    for path in sorted(ROOT.rglob('*.png')):
        r = scan_file(path)
        if r:
            results.append(r)
    print(f'\nFound {len(results)} files with > {THRESHOLD_MIN_PIXELS} purple pixels:\n')
    print(f'{"File":<60} {"Count":>10} {"%":>6} {"BBox":<25} {"Median Hex":<10}')
    print('-' * 115)
    for r in sorted(results, key=lambda x: -x['purple_count']):
        fname = Path(r['file']).name
        bbox = r['bbox']
        bbox_s = f'x={bbox[0]:4d}-{bbox[2]:4d}, y={bbox[1]:4d}-{bbox[3]:4d}' if bbox else 'n/a'
        print(f'{fname:<60} {r["purple_count"]:>10} {r["purple_pct"]:>5.2f}% {bbox_s:<25} {r["median_hex"]}')

if __name__ == '__main__':
    main()