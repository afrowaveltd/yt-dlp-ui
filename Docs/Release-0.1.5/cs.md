# Vydání 0.1.5 — oprava zobrazení playlistu

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5)

**Opravné předběžné vydání desktopové aplikace pro Windows a Linux.**

## Co se opravilo

Po analýze playlistu se místo názvů videí zobrazoval text `YtDlpUi.Core.Models.MediaEntry`. Šablona seznamu v Avalonia UI byla omylem určena jinému modelu (`LogLine`).

- Šablona položek nyní odpovídá skutečnému typu **`MediaEntry`**.
- Každý řádek zobrazuje **pořadové číslo a název videa**, případně také **délku**.
- Přibyl regresní smoke test, který kontroluje správný typ šablony, XML namespace i vazby na název a délku.
- Analýza playlistu, stahování, volby formátu a fronta zůstávají **beze změny**.

Jde pouze o opravu zobrazení. Není nutná migrace ani změna nastavení. Funkce Deno z verzí v0.1.3 a v0.1.4 zůstávají zachovány.

## Ověření

Před publikací běží sestavení, smoke testy, kontroly instalačních balíčků a SHA-256 v GitHub Actions pro Windows i Linux. Automatický test zachytí chybně zapojenou šablonu, ale nenahrazuje ruční test každého playlistu či webu.

Nadále jde o **nepodepsaný pre-release** s neúplnou lokalizací. Experimentální aplikace pro Android není součástí desktopového vydání.

[Instalace](../Installation/cs.md) · [Funkce](../Features/cs.md) · [Nahlásit chybu](https://github.com/afrowaveltd/yt-dlp-ui/issues)
