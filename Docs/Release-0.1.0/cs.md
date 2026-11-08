# První veřejné preview — v0.1.0

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Vývoj a ověřování](../Development-and-Verification/cs.md)

**Stav: [v0.1.0 je veřejně vydaná](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) jako pre-release dne 8. listopadu 2026.** Tag zůstává neměnný; následující seznam sleduje zbývající ruční testy a případné budoucí opravy.

## Zamýšlený obsah vydání

- Avalonia/.NET 10 GUI nad yt-dlp s devíti kartami, analýzou odkazu, profily, frontou, náhledem příkazu a stavem nástrojů.
- Světlý/tmavý motiv, podpora schránky a přepínání jazyků v horní liště hlavního okna.
- Dictionaryless JSON slovníky v angličtině/češtině, automaticky nalezené z vestavěné/systémové/uživatelské složky `Locales` a anglický fallback.
- Windows x64 instalátor Inno Setup se dvěma nezávislými volbami yt-dlp a FFmpeg, instalací pro uživatele a standardní odinstalací; portable Windows x64.
- Linux DEB, RPM a portable tar.gz; správci balíčků řeší externí závislosti bez jejich zabalení do GUI.
- Anglický/český README a dvojice `Docs/<Téma>/{en,cs}.md`.
- Smoke testy překladů a vyhledávání JSON, GitHub Actions sestavení Windows/Linux.

## Publikované balíčky a ověření v CI

- [x] Sestavení a testy pro Windows a Linux: [úspěšný publikační workflow](https://github.com/afrowaveltd/yt-dlp-ui/actions/runs/42517322864).
- [x] Vytvořit a prověřit obsah DEB, RPM, přenosného Linuxu, Windows setup i portable ZIP.
- [x] Ověřit dvojjazyčnou dokumentaci, verzi 0.1.0 a místní odkazy ve smoke testech.

Publikační workflow nahrál všech pět balíčků a `SHA256SUMS.txt`. Ruční integrační testy zůstávají samostatným úkolem.

## Ruční přejímací testy

- [ ] Čistá instalace a odinstalace Windows x64; Start menu a zástupce.
- [ ] Obě volby závislostí: obě zapnuté, jen jedna, žádná a již instalované programy.
- [ ] Výpadek internetu/chyba stažení, odmítnutí neplatného SHA-256 a opakování na kartě Nástroje.
- [ ] Spuštění Windows portable bez instalace; spuštění na Debianu/Ubuntu.
- [ ] Instalace/odinstalace DEB na skutečném desktopu Debian/Ubuntu; menu, ikona a závislosti.
- [ ] Instalace/odinstalace RPM na Fedoře/openSUSE, nejen jeho vytvoření v Ubuntu CI.
- [ ] Základní stažení a spojování/převod médií, k nimž máme oprávnění.
- [ ] Průběh, fronta, opakování/zrušení, cesty s mezerami a neASCII názvy.
- [ ] Zabalené i vlastní jazyky (`cs`, `en`, `pt-BR`), změny za běhu a uložení výběru.
- [ ] Kontrola logů a přihlašovacích údajů, zda neunikají citlivá data.

## Známá omezení

- Lokalizované prvky zatím tvoří jen část zpráv a pokročilých karet.
- Windows instalátor je jen x64. ARM64 lze sestavit ručně; ověřený ARM64 setup ani macOS balíček do prvního vydání nepatří.
- Automatické testy ověřují vybrané funkce a tvorbu balíčků, **nikoliv** všechny weby či instalátory end-to-end.
- Externí programy se instalují zvlášť a mají vlastní licenční podmínky; GUI je MIT.
- Veřejné binárky nejsou digitálně podepsané; součástí vydání jsou součty SHA-256.

## Splněné kroky a co nás čeká

1. **Hotovo:** verze 0.1.0, dvojjazyčná dokumentace, zelené buildy i smoke testy pro Linux/Windows.
2. **Hotovo:** tag `v0.1.0` a [veřejný pre-release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) s pěti balíčky a kontrolními součty.
3. **Dále:** sbírat hlášení komunity a provést zbývající ruční testy uvedené výše.
4. **Případné opravy:** vydávat pod novou verzí a tagem, nepřepisovat zveřejněný `v0.1.0`.

[Repozitář](https://github.com/afrowaveltd/yt-dlp-ui) · [Instalace](../Installation/cs.md)
