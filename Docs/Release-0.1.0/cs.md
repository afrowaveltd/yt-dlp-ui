# Příprava prvního vydání — 0.1.0

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Vývoj a ověřování](../Development-and-Verification/cs.md)

**Stav: připravujeme 0.1.0. Netvrdíme, že existuje zveřejněný GitHub Release nebo neměnný tag.** Tento soubor popisuje plánovaný obsah první preview verze a co ještě musíme otestovat.

## Zamýšlený obsah vydání

- Avalonia/.NET 10 GUI nad yt-dlp s devíti kartami, analýzou odkazu, profily, frontou, náhledem příkazu a stavem nástrojů.
- Světlý/tmavý motiv, podpora schránky a přepínání jazyků v horní liště hlavního okna.
- Dictionaryless JSON slovníky v angličtině/češtině, automaticky nalezené z vestavěné/systémové/uživatelské složky `Locales` a anglický fallback.
- Windows x64 instalátor Inno Setup se dvěma nezávislými volbami yt-dlp a FFmpeg, instalací pro uživatele a standardní odinstalací; portable Windows x64.
- Linux DEB, RPM a portable tar.gz; správci balíčků řeší externí závislosti bez jejich zabalení do GUI.
- Anglický/český README a dvojice `Docs/<Téma>/{en,cs}.md`.
- Smoke testy překladů a vyhledávání JSON, GitHub Actions sestavení Windows/Linux.

## Ověřeno v CI (po úspěšném běhu aktuální verze)

- [ ] Zkontrolovat finální SHA commitu a zelený Linux/Windows běh v [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions).
- [ ] Ověřit metadata/obsah DEB a RPM, archiv Linuxu a Windows setup/portable artifacts.
- [ ] Ověřit dvojjazyčné dokumenty, shodu verze a všechny lokální odkazy.

Tyto kroky mají být zaškrtnuty teprve po úspěšném sestavení finálního kódu verze `0.1.0`.

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
- Netvrdíme, že máme certifikát pro podpis kódu nebo vydaný podepsaný release.

## Postup vydání (zatím neprovedeno)

1. Opravit chyby z přejímacího seznamu a aktualizovat oba jazyky dokumentace.
2. Ověřit, že `Directory.Build.props`, náhradní verze Inno Setup, názvy balíčků a příklady uvádějí **0.1.0**.
3. Ověřit zelené CI pro přesný commit určený k vydání.
4. **Až po přejímce** založit neměnný Git tag `v0.1.0`.
5. Zveřejnit GitHub Release s ověřenými balíčky a odpovídajícími kontrolními součty.
6. Oznamovat pouze skutečně publikované balíčky a udržovat poznámky pro pozdější opravy.

[Repozitář](https://github.com/afrowaveltd/yt-dlp-ui) · [Instalace](../Installation/cs.md)
