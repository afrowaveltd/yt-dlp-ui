# yt-dlp GUI

Přívětivé **multiplatformní grafické rozhraní pro [yt-dlp](https://github.com/yt-dlp/yt-dlp)** vytvořené v [Avalonia UI](https://avaloniaui.net/) a .NET 10. Stačí vložit odkaz na video nebo playlist, vybrat profil a spravovat stahování bez memorování přepínačů příkazové řádky. Pokročilé volby yt-dlp přitom zůstávají dostupné.

**Stav vydání:** **[v0.1.0 – první veřejné preview](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** je dostupné pro Windows x64 a Linux x64. Jde o komunitní pre-release; uvítáme zpětnou vazbu a hlášení chyb.

**Dokumentace:** [English](en.md) · Česky (tato stránka) · [Rozcestník](../Documentation/cs.md) · [Documentation index](../Documentation/en.md)

## Co aplikace umí

- **Jednoduché stahování:** načítání odkazů ze schránky, přetažení odkazu, analýza videí a playlistů, náhledy, předvolby kvality (až 4K), pouze zvuk, fronta a průběh.
- **Devět karet:** Stáhnout, Formát, Playlist, Titulky a metadata, Síť, Soubory, Přihlášení, Pokročilé a Nástroje.
- **Pokročilé funkce:** výběr formátu, převody, titulky, metadata, filtry, proxy, šablony názvů, cookies a přihlášení, náhled příkazu yt-dlp.
- **Externí nástroje:** detekce a aktualizace yt-dlp, zjištění FFmpeg; volitelný instalátor Windows nabízí stažení yt-dlp a FFmpeg s ověřením SHA-256.
- **Jazyky:** výběr vpravo nahoře u stavu nástrojů. Anglický a český slovník, automatické načítání `Locales/*.json`, okamžitý přepínač a anglický fallback.
- **Distribuce:** Windows x64 `setup.exe` a portable; Linux `.deb`, `.rpm` a přenosný `.tar.gz`.

Část pokročilých karet, zpráv a dialogů dosud obsahuje české texty. **Lokalizace je v kandidátovi 0.1.0 jen částečná.** Úspěšné sestavení balíčku ještě neznamená, že byly ručně otestovány všechny scénáře instalace a stahování.

## Ukázky

[![Hlavní obrazovka s odkazem a profily](../../screenshots/main_screen.png)](../../screenshots/main_screen.png)

| | |
|---|---|
| [![Formát](../../screenshots/format_screen.png)](../../screenshots/format_screen.png) **Formát** | [![Playlist](../../screenshots/playlist_screen.png)](../../screenshots/playlist_screen.png) **Playlist** |
| [![Titulky a metadata](../../screenshots/titulky_screen.png)](../../screenshots/titulky_screen.png) **Titulky a metadata** | [![Síť](../../screenshots/sit_screen.png)](../../screenshots/sit_screen.png) **Síť** |
| [![Soubory](../../screenshots/soubory_screen.png)](../../screenshots/soubory_screen.png) **Soubory** | [![Přihlášení](../../screenshots/prihlaseni_screen.png)](../../screenshots/prihlaseni_screen.png) **Přihlášení** |
| [![Pokročilé](../../screenshots/pokrocile_screen.png)](../../screenshots/pokrocile_screen.png) **Pokročilé** | [![Nástroje](../../screenshots/nastroje_screen.png)](../../screenshots/nastroje_screen.png) **Nástroje** |

Snímky představují stav aplikace při pořízení a nemusí obsahovat nejnovější ovládací prvky. Kliknutím otevřeš plné rozlišení.

## Instalace a spuštění

### Windows 10/11 (x64)

Z [prvního vydání v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) stáhni **Windows instalační EXE** a spusť `yt-dlp-gui-setup-0.1.0-win-x64.exe`. Průvodce vytvoří zástupce v nabídce Start, umožní běžnou odinstalaci a nabídne dvě **nezávislé, ve výchozím stavu zaškrtnuté** položky: nainstalovat yt-dlp a FFmpeg. Stahování vyžaduje internet a používá kontrolní součty SHA-256 od vydavatelů. Obě volby lze vypnout a nástroje doplnit později. Instalace probíhá pro aktuálního uživatele bez zvýšených oprávnění.

V release najdeš i přenosný balíček **`yt-dlp-gui_0.1.0_win-x64_portable.zip`**. Windows ARM64 mají samostatný ruční publish postup, nikoli ověřený instalační program v CI.

### Debian / Ubuntu, Fedora / openSUSE, další Linux

Stáhni odpovídající balíček DEB nebo RPM z [v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0) a použij správce balíčků distribuce:

~~~bash
sudo apt install ./yt-dlp-gui_0.1.0_amd64.deb    # Debian/Ubuntu x64
sudo dnf install ./yt-dlp-gui-0.1.0-1.x86_64.rpm # Fedora / RPM
~~~

Názvy balíčků se liší podle verze a architektury. DEB/RPM **doporučují** systémové balíčky `yt-dlp` a `ffmpeg`, ale nepřibalují je a nespouštějí skryté skripty pro stahování. Rozhoduje politika správce balíčků. Na jiných distribucích rozbal přenosný `tar.gz` a spusť `./yt-dlp-gui/yt-dlp-gui`. Sestavení obsahují vlastní .NET runtime.

Pro uživatelskou instalaci z kódu je tu `./scripts/install.sh`, odinstalace `./scripts/install.sh --uninstall`.

**v0.1.0 je veřejný, nepodepsaný pre-release.** Podrobnosti najdeš v [Instalaci](../Installation/cs.md).

## Sestavení ze zdrojových kódů

Nainstaluj **.NET 10 SDK** podle `global.json`:

~~~bash
git clone https://github.com/afrowaveltd/yt-dlp-ui.git
cd yt-dlp-ui
dotnet restore YtDlpUi.slnx
dotnet build YtDlpUi.slnx -c Release
dotnet run --project src/YtDlpUi/YtDlpUi.csproj
dotnet run --project tests/YtDlpUi.SmokeTests/YtDlpUi.SmokeTests.csproj -c Release
~~~

Pro samotné stahování je nutný `yt-dlp`; pro spojování a mnoho převodů také `ffmpeg` / `ffprobe`. Na kartě **Nástroje** zjistíš jejich stav a nastavíš vlastní cesty.

## Jak přidat jazyk

Výběr jazyka je **vpravo nahoře** vedle stavu yt-dlp/FFmpeg. Přepnutí okamžitě změní už lokalizované prvky.

Chceš-li nový jazyk bez překompilování, vytvoř třeba `pt-BR.json` v adresáři:

- Linux: `~/.config/yt-dlp-ui/Locales/` (nebo `$XDG_CONFIG_HOME/yt-dlp-ui/Locales/`)
- Windows: `%LOCALAPPDATA%\yt-dlp-ui\Locales\`
- Případně `Locales/` vedle spustitelného souboru

~~~json
{
  "Download": "Baixar",
  "About": "Sobre"
}
~~~

Při opětovném otevření rozbalovací nabídky se soubory načtou. Názvy souborů odpovídají jazykům (např. `fr.json`, `pt-BR.json`); klíči jsou anglické texty UI. Chybějící nebo prázdné překlady vrací anglický klíč. Uživatelský slovník má přednost před zabaleným. Více: [Lokalizace](../Localization/cs.md).

## Dokumentace

| Téma | Česky | English |
|---|---|---|
| Rozcestník | [Dokumentace](../Documentation/cs.md) | [Index](../Documentation/en.md) |
| První spuštění | [Průvodce](../Getting-Started/cs.md) | [Getting started](../Getting-Started/en.md) |
| Instalace a balíčky | [Instalace](../Installation/cs.md) | [Installation](../Installation/en.md) |
| Funkce a používání | [Funkce](../Features/cs.md) | [Features](../Features/en.md) |
| Lokalizace | [Jazyky](../Localization/cs.md) | [Localization](../Localization/en.md) |
| Nastavení a úložiště | [Nastavení](../Configuration/cs.md) | [Configuration](../Configuration/en.md) |
| Řešení problémů | [Nápověda](../Troubleshooting/cs.md) | [Troubleshooting](../Troubleshooting/en.md) |
| Vývoj a ověřování | [Vývoj](../Development-and-Verification/cs.md) | [Development](../Development-and-Verification/en.md) |
| Příprava 0.1.0 | [Checklist](../Release-0.1.0/cs.md) | [Release checklist](../Release-0.1.0/en.md) |

## Licence, soukromí a poděkování

**Zdrojový kód našeho GUI** je pod licencí [MIT](../../LICENSE). [yt-dlp](https://github.com/yt-dlp/yt-dlp) a [FFmpeg](https://ffmpeg.org/) jsou samostatné projekty s **vlastními licencemi**; instalace jejich binárek nemění licenci GUI. Aplikace neobsahuje reklamy ani telemetrii. Při analýze metadat, stahování, instalaci nástrojů a kontrole aktualizací však může docházet k připojení k odpovídajícím službám.

Zapamatované volby stahování záměrně vynechávají podporovaná pole hesel a 2FA. Se staženými soubory, cookies a logy nakládej podle svých požadavků na soukromí. Stahuj jen obsah, ke kterému máš oprávnění, a respektuj podmínky služeb.

**[v0.1.0 už je veřejně vydaná](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** jako první komunitní preview. Viz [stav vydání a zbývající testy](../Release-0.1.0/cs.md).
