# Vývoj a ověřování

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Kontrolní seznam vydání](../Release-0.1.5/cs.md)

## Co potřebujeme

- **.NET 10 SDK** podle `global.json` (povoleno posunutí v rámci feature band).
- Závislosti Avalonia UI a CommunityToolkit.Mvvm získané běžným NuGet restore.
- Pro ruční spuštění UI na Linuxu grafické prostředí.
- Nástroje balíčkování: `dpkg-deb` (DEB), `rpmbuild` (RPM), `tar` (přenosný Linux), PowerShell a Inno Setup 6 (Windows).
- GUI je desktopová .NET aplikace; **yt-dlp a FFmpeg jsou externí programy**, nikoliv NuGet závislosti nebo součást binárky GUI.

## Překlad a smoke testy

Z kořene repozitáře:

~~~bash
dotnet restore YtDlpUi.slnx
dotnet build YtDlpUi.slnx -c Release
dotnet run --project tests/YtDlpUi.SmokeTests/YtDlpUi.SmokeTests.csproj -c Release
dotnet run --project src/YtDlpUi/YtDlpUi.csproj
~~~

Projekt cílí na `net10.0`. Trimming při publish nezapínej bez úpravy a ověření reflexních vazeb Avalonia.

## Struktura projektu

~~~text
src/YtDlpUi.Core/
  Models/                  nastavení, volby stahování, enumy, metadata
  Services/                procesy, argumenty, analýza, stahování,
                           vyhledávání/instalace nástrojů a ukládání nastavení
  Localization/            SimpleLocalizer a objevování JSON jazyků
  Locales/                 zabalené slovníky en.json a cs.json
src/YtDlpUi/
  Views/Pages/             devět karet Avalonia UI
  ViewModels/              okno, fronta a nástroje
  Localization/            překladová vazba do XAML
  Styles/, Assets/         motivy, ikony a další prostředky
tests/YtDlpUi.SmokeTests/  spustitelné smoke testy
scripts/                   sestavení, DEB, RPM, tar, Windows setup
packaging/windows/         Inno Setup a volitelná instalace závislostí
Docs/<Téma>/en.md, cs.md   dvojjazyčné návody a vydání
~~~

## Aktuální automatické ověřování

Smoke suite také kontroluje detekci JavaScript runtime Deno, přednost ručního nastavení a Windows instalátor Deno. Kontroluje anglický fallback, okamžitý český překlad, přidávání/mazání/přepisování JSON jazyků za běhu, regionální a nadřazené kultury a předání cesty FFmpeg do příkazové řádky. Jsou to **smoke testy**, nikoliv kompletní UI a integrační testy.

[GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) sestavují Linux a Windows a spouštějí smoke testy. Navíc:

- Vytvoří a prověří strukturu `.deb`, `.rpm` a `.tar.gz` na Linuxu.
- Vytvoří portable Windows x64 a instalátor Inno Setup `setup.exe`. Setup vyžaduje UAC, CI navíc testuje nastavení instalátoru a syntaxi PowerShell.
- Nahrají testovací artifacts k ručnímu ověření.

Zelené CI znamená úspěšnou kompilaci a balíčkování, **nikoliv** test každého stažení v průvodci Windows, všech webů yt-dlp ani každé distribuce RPM. Pro reálné ověření je [kontrolní seznam oprav 0.1.5](../Release-0.1.5/cs.md).

## Pravidla lokalizace

- Anglické klíče přidávej do `src/YtDlpUi.Core/Locales/en.json`; anglický text je klíč i hodnota.
- Překlad doplň do `cs.json` a dalších slovníků.
- V XAML používej `{loc:Tr Key='English text'}` pro živé přepínání.
- Jazykové kódy nevkládej natvrdo; slovníky objevuje `SimpleLocalizer`.
- Před merge kontroluj JSON, regionální fallback i živé přepínání.

## Pravidla dokumentace

- Anglický kořenový `README.md` je výchozí stránka GitHubu.
- `Docs/README/en.md` je kopie s upravenými relativními odkazy; `Docs/README/cs.md` je česká varianta.
- Každé téma má `Docs/<Téma>/en.md` **i** `cs.md`. Odkazy piš relativně.
- Pokud funkce není ověřená, označ ji jako **plánovanou**, **neověřenou** nebo **k ručnímu testu**.
- Verze v dokumentaci i příkladech se řídí `Directory.Build.props`. Git tag či release je samostatný krok.
- Při změnách funkcí, konfigurace, skriptů či testů aktualizuj oba jazyky.
- Dodržuj [pravidla MAJOR.MINOR.PATCH](../Versioning/cs.md); všechna vydání 0.x jsou na GitHubu pre-release.

## Doporučené ruční testování

Otestuj minimálně desktop Debian/Ubuntu, jednu distribuci RPM a Windows x64. Vyzkoušej čistý setup se všemi třemi nezávislými volbami (yt-dlp, FFmpeg a Deno), vypnuté volby, existující programy, výpadky internetu, odinstalaci, stahování vlastního ukázkového videa, audio převod, přidání JSON jazyka za běhu, trvalé nastavení, světlý/tmavý motiv, zkratky a cesty s mezerami.

[Zdrojový kód](https://github.com/afrowaveltd/yt-dlp-ui) · [Řešení problémů](../Troubleshooting/cs.md)
