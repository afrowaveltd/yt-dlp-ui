# Vydání 0.1.4 — ovládání Deno a detekce v Linuxu

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.4)

Jde o **opravné předběžné vydání desktopové aplikace**, které dokončuje integraci prostředí Deno započatou ve v0.1.3.

## Co přibylo

- V horní liště jsou **tři kontrolky**: **yt-dlp, FFmpeg a Deno**. Kontrolka Deno ukazuje dostupnost kompatibilního prostředí; bublina po najetí myší ukáže nalezenou cestu.
- **Nástroje → Deno** zobrazují dostupnost, cestu a verzi (vyžaduje Deno 2.3+). Na podporovaných systémech lze spustit **Nainstalovat Deno pro Windows / Linux** a přes **Zkontrolovat znovu** obnovit stav.
- Linux hledá Deno v `$DENO_INSTALL/bin`, `~/.deno/bin`, `~/.local/bin`, `~/bin` i PATH, a to také při spuštění z grafické nabídky, která nenačítá profil shellu.
- Uživatelský instalátor ověří oficiální archiv pomocí SHA-256 a zkontroluje spustitelnost. **Nevyžaduje sudo ani UAC**, nemění PATH a při neúspěšném ověření zachová funkční verzi.
- Windows setup dál nabízí samostatné výchozí zaškrtnuté volby yt-dlp / FFmpeg / Deno; společná instalace vyžaduje UAC.
- Smoke testy hlídají Deno v rozhraní a cíl instalace v Linuxu.

Deno není povinné pro samotné GUI, ale doporučuje se kvůli kompatibilitě YouTube s aktuálním yt-dlp. Ručně nastavené JavaScript prostředí má přednost.

## Instalační balíčky

Windows x64 instalační EXE a přenosný ZIP; Debian/Ubuntu x64 DEB; x64 RPM; Linux x64 přenosný tar.gz a SHA-256 kontrolní součty. GitHub Actions vše sestavuje ze **stejného commitu**. Viz [Instalace](../Installation/cs.md).

## Ověření a zbývající testy

Instalátor Windows a zobrazení Deno byly ručně potvrzeny testerem projektu. CI ověřuje sestavení, smoke testy, balíčky a kontrolní součty; **nenahrazuje skutečné instalace a stahování** ve všech distribucích. Uvítáme testy na Debianu/Ubuntu a RPM distribuci, přerušenou instalaci Deno, starší verzi Deno a skutečné stahování vlastního videa z YouTube.

**Omezení:** nepodepsané komunitní preview, neúplná lokalizace, závislost na externích službách a žádná androidová aplikace ve vydání. Android zůstává samostatným experimentálním projektem.

[Verzování](../Versioning/cs.md) · [Řešení problémů](../Troubleshooting/cs.md) · [Nahlásit chybu](https://github.com/afrowaveltd/yt-dlp-ui/issues)
