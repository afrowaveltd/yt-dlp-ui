# Vydání 0.1.6 — spolehlivé zobrazení názvů videí v playlistu

[English](en.md) · [Rozcestník dokumentace](../Documentation/cs.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.6)

**Opravné předběžné vydání pro desktopový Windows/Linux.**

Ve v0.1.5 se v náhledu playlistu stále mohl místo názvů videí zobrazovat text `YtDlpUi.Core.Models.MediaEntry`. Nyní opravujeme jak šablonu Avalonia UI, tak záložní zobrazování.

- Šablona `ListBox.ItemTemplate` na kartě Stáhnout se použije **bez podmínky `DataType`**.
- Když šablonu nelze použít, `MediaEntry.ToString()` vrátí srozumitelné **pořadí, název a délku videa**. Chybějící název má čitelný zástupný text.
- Při správném vykreslení šablony zůstává zvlášť sloupec s názvem a délkou.
- Nový projekt `YtDlpUi.UiSmokeTests` spustí opravdovou stránku aplikace v prostředí **Avalonia.Headless** a ověří text položek na CI pro Windows i Linux. Dřívější testy modelu a XAML zůstávají.
- Parser playlistu, stahování, přihlašování ani formáty se nemění.

## Ověření a omezení

Obě platformy provádějí také nové UI smoke testy. Před zveřejněním instalačních balíčků je GitHub Actions znovu spustí. Test potvrzuje vykreslení v headless Avalonia, nikoli úplné chování na každém počítači a webu.

Jde stále o nepodepsané komunitní předběžné vydání s částečnou lokalizací. Experimentální Android není součástí balíčků.

[Instalace](../Installation/cs.md) · [Funkce](../Features/cs.md) · [Nahlásit problém](https://github.com/afrowaveltd/yt-dlp-ui/issues)
