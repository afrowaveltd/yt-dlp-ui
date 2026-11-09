# První kroky

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Instalace](../Installation/cs.md)

## 1. Co potřebuješ

- Windows 10/11 x64 nebo podporované desktopové prostředí Linuxu.
- Nainstalovaný `yt-dlp` nebo jeho zaškrtnutí v instalátoru Windows; právě on provádí stahování.
- Doporučený `ffmpeg` / `ffprobe` pro spojování obrazu se zvukem a převody. Oba nástroje mají vlastní licence.
- Připojení k internetu pro metadata a stahování.

Balíčky Windows a Linux DEB/RPM/tar obsahují vlastní runtime .NET. K běhu tedy není nutná samostatná instalace .NET; pro vývoj ze zdrojového kódu potřebuješ .NET 10 SDK.

## 2. Volba instalace

- **Windows x64:** z [v0.1.6](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.6) stáhni instalační EXE a spusť průvodce. Pro nejjednodušší start nech zaškrtnuté tři nezávislé volby **Instalovat yt-dlp**, **Instalovat FFmpeg** a **Instalovat Deno**.
- **Debian/Ubuntu:** odpovídající `.deb` nainstaluj přes `sudo apt install ./soubor.deb`; správce balíčků může nainstalovat i doporučené závislosti.
- **Fedora/openSUSE:** použij `.rpm` a správce balíčků příslušné distribuce.
- **Jiné distribuce Linuxu:** rozbal `.tar.gz`, spusť obsažený program a podle potřeby doplň externí nástroje.
- **Vývojáři:** naklonuj repozitář a použij postup v [Vývoji a ověřování](../Development-and-Verification/cs.md).

Pro běžné stažení používej zveřejněný **[pre-release v0.1.6](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.6)**. Artifacts v GitHub Actions slouží k vývoji a testování.

## 3. První stažení

1. Zkopíruj přímý odkaz na video či playlist (nebo jej přetáhni do okna).
2. Sledování schránky ho může vyplnit samo; jinak ho vlož na kartě **Stáhnout**.
3. Stiskni **Analyzovat** a prohlédni si název, náhled a dostupné formáty.
4. Vyber profil, například Nejlepší kvalita, Full HD či Jen zvuk; případně nastav pokročilé karty.
5. Vyber cílovou složku a stiskni **Stáhnout**, případně přidej další položku **do fronty**.
6. Sleduj průběh; po dokončení použij **Otevřít složku**.

V horní liště je stav yt-dlp, FFmpeg a Deno. Pokud některý chybí, otevři kartu **Nástroje**. Vedle indikátorů je také **nabídka jazyků**.

## 4. Klávesové zkratky

| Zkratka | Akce |
|---|---|
| `Ctrl+L` | Fokus na vstup odkazu |
| `F5` | Analyzovat aktuální odkaz |
| `Ctrl+Enter` | Spustit stahování |

## 5. Co dál

- [Devět karet aplikace](../Features/cs.md)
- [Nastavení a ukládání](../Configuration/cs.md)
- [Řešení problémů](../Troubleshooting/cs.md)

**Poznámka:** výsledky závisí na podporovaných serverech, cookies, webové službě a verzi yt-dlp. Stahuj pouze obsah, k němuž máš oprávnění.
