# Credits & Attribution — Magic Match v1.1

> **Purpose**：attribution for third-party assets (fonts + libraries) used in Magic Match
> **Audience**：Google Play Store listing + splash/settings UI credits (future)
> **License summary**：all assets are permissively licensed (OFL 1.1 for fonts, MIT for libraries)

---

## Fonts

### Fredoka (Bold 700, SemiBold 600)

- **Author**: Milena Brandão
- **Source**: https://fonts.google.com/specimen/Fredoka
- **License**: SIL Open Font License, Version 1.1
- **Usage**: splash screen title ("Magic Match") + button labels + app brand display
- **Files**:
  - `assets/fonts/Fredoka-Bold.ttf` (subset, ~33 KB)
  - `assets/fonts/Fredoka-SemiBold.ttf` (subset, ~33 KB)

### Nunito (Regular 400, Medium 500, Bold 700)

- **Authors**: Vernon Adams (2014), Manvel Shmavonyan (2014), Jacques Le Bailly (2016)
- **Source**: https://fonts.google.com/specimen/Nunito
- **License**: SIL Open Font License, Version 1.1
- **Usage**: body text + UI labels + HUD numbers + i18n strings
- **Files**:
  - `assets/fonts/Nunito-Regular.ttf` (subset, ~39 KB)
  - `assets/fonts/Nunito-Medium.ttf` (subset, ~39 KB)
  - `assets/fonts/Nunito-Bold.ttf` (subset, ~39 KB)

Both fonts are subset to **Latin + Latin Extended-A** (~319 chars) via fontTools pyftsubset (2026-10-06 by Christine).

Full license text: `assets/fonts/OFL.txt`

---

## Libraries

### Godot Engine

- **Authors**: Juan Linietsky + Ariel Manzur + Godot contributors
- **Source**: https://godotengine.org/
- **License**: MIT License
- **Usage**: game engine + scene framework

### C# / .NET 9

- **Authors**: Microsoft + .NET contributors
- **Source**: https://dotnet.microsoft.com/
- **License**: MIT License
- **Usage**: game logic scripts (Match3 / Counter / Splash / etc.)

---

## Background images (AI-generated, custom)

### bg_title.png + bg_game.png

- **Generator**: MiniMax text-to-image (image-01 model)
- **Author**: Christine (MagicStudio Artist)
- **License**: Custom (project-owned)
- **Generated**: 2026-09-29 (D+1)
- **Re-generated**: 2026-10-01 (D+3) for cosmic polish

---

## Gem sprites (12 colors)

- **Style**: dark outline + semi-realistic facets (per ART_SPEC §6)
- **Author**: Christine (MagicStudio Artist)
- **License**: Custom (project-owned)
- **Variant**: 6 gem family + 6 polish variants (D+5)

---

## i18n translations

### Spanish (Latin American) + Portuguese (Brazilian)

- **Source**: en.po (English baseline)
- **Translation method**: DeepL Pro + manual review (per Mark D+12 approval)
- **Style**: informal "tú" (Spanish) + informal "você" (Brazilian Portuguese)
- **Files**:
  - `locale/es.po` (Spanish, 9 strings filled)
  - `locale/pt-BR.po` (Brazilian Portuguese, 9 strings filled)

---

## Future credits (v2.0+)

### CJK fonts (Noto Sans SC/JP/KR)

- **License**: SIL OFL 1.1
- **Subset target**: per locale (`cmap` subsetting)
- **Estimated v2.0 file size**: ~2.7 MB total for SC + JP + KR

### App icon variants (per platform)

- **iOS**: App Store Connect (Apple standard)
- **Android**: Google Play Console (Google standard)
- **Web**: PWA manifest (W3C standard)

---

## License compliance

| Asset | License | Attribution required? |
|-------|---------|---------------------|
| Fredoka | OFL 1.1 | Recommended (this file + OFL.txt) |
| Nunito | OFL 1.1 | Recommended (this file + OFL.txt) |
| Godot | MIT | Optional (standard) |
| C# / .NET | MIT | Optional (standard) |
| Custom assets (bg / gems / UI) | Custom | N/A (project-owned) |
| i18n translations | Custom | N/A (project-owned) |

**Compliance status**: ✅ all OFL fonts attribution included; MIT libraries use is permitted without attribution; custom assets are project-owned.

---

**Maintainer**: Christine (Artist) + Mark (Leader)
**Last updated**: 2026-10-06 (D+16)
**Repository**: git@github.com:rongxianzhuo/godot-template.git
