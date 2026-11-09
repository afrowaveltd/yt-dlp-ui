# Řešení problémů

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Instalace](../Installation/cs.md)

## Chybí yt-dlp

Otevři **Nástroje** a spusť nové vyhledání. Zkontroluj, že funguje `yt-dlp --version` (ve Windows `yt-dlp.exe --version`). Pokud nástroj není v PATH, nastav přímo cestu ke spustitelnému souboru.

Instalátor Windows má samostatnou volbu **Nainstalovat yt-dlp**; pokud volitelné stahování selže, GUI se neodinstaluje. V Linuxu použij správce balíčků nebo jiný důvěryhodný způsob instalace yt-dlp. Jde o samostatný nástroj, nikoliv součást GUI.

## Chybí FFmpeg nebo nefunguje převod

Prověř `ffmpeg -version` a pro odpovídající operace také `ffprobe -version`. Průvodce Windows instaluje společné ověřené FFmpeg/FFprobe s UAC; karta **Nástroje** může instalovat osobní kopii; Linux zpravidla používá systémové balíčky. Podle potřeby nastav cestu k FFmpeg a zopakuj detekci. Prosté stažení streamu může proběhnout bez FFmpeg, ale spojení či převod často ne.

## YouTube: WARNING o chybějícím JavaScript prostředí

Jde o upozornění ze samotného yt-dlp, nikoliv o problém FFmpeg či GUI. Bez kompatibilního JavaScript prostředí mohou některé formáty YouTube chybět. **Doporučeno je Deno 2.3.0 nebo novější**, které yt-dlp podporuje automaticky.

- Windows: v novém setupu zaškrtni **Nainstalovat Deno**, nebo použij PowerShell: `winget install --id DenoLand.Deno -e`. Pak restartuj GUI.
- Windows i Linux: na kartě **Nástroje** uvidíš stav, verzi a cestu Deno; přes **Nainstalovat Deno pro Windows / Linux** lze nainstalovat ověřenou uživatelskou kopii bez práv správce.
- Linux: grafická aplikace nyní hledá Deno také v `~/.deno/bin`, `~/.local/bin`, `~/bin`, `DENO_INSTALL/bin` a PATH, i když menu nenačte tvůj shell profil. Můžeš Deno nainstalovat rovněž ze zdrojů vydavatele a ověřit `~/.deno/bin/deno --version`.
- GUI automaticky vyhledá Deno i ve společném Windows ProgramData, osobních složkách nebo PATH; umí předat absolutní cestu.
- Pokročilí uživatelé mohou nastavit `--js-runtimes node` (Node 22+) nebo `--js-runtimes deno:/cesta/k/deno`.
- Oficiální samostatná binárka yt-dlp obsahuje `yt-dlp-ejs`; instalace přes pip/distribuci mohou potřebovat `yt-dlp[default]`. Viz [návod autorů yt-dlp](https://github.com/yt-dlp/yt-dlp/wiki/EJS).
- Není vhodné jen skrýt WARNING pomocí `--no-warnings`; problém by tím nezmizel.

## Instalátor Windows nedokázal stáhnout nástroje

- Ověř připojení, DNS a dostupnost serverů vydavatelů — GitHub releases pro yt-dlp a zdroje FFmpeg.
- Podrobnosti hledej v `%ProgramData%\yt-dlp-ui\logs\dependency-install.log`.
- Musí být dostupné také kontrolní součty vydavatele. Nesouhlas SHA-256 má instalaci zastavit místo spuštění poškozené binárky.
- Pro opravu společné instalace spusť znovu Windows **setup s UAC**, nebo použij kartu **Nástroje** pro osobní kopii. Starou uživatelskou instalaci GUI lze odinstalovat, aby nezůstaly dvojí zástupci.

## Video nejde analyzovat nebo stáhnout

- Zkontroluj odkaz v prohlížeči a oprávnění k přístupu.
- Zvaž aktualizaci yt-dlp; weby se mění nezávisle na našem GUI.
- Pokud je potřeba přihlášení, nastav cookies nebo jiné údaje na kartě **Přihlášení**. Soukromé cookies ani hesla nevkládej do veřejného hlášení chyby.
- Na kartě **Pokročilé** zapni podrobný výstup a podívej se do logu. Náhled vytvořeného příkazu pomůže problém zopakovat přímo v yt-dlp.

## Linux hlásí chybu grafiky nebo obrazovky

- Avalonia potřebuje grafické prostředí a nativní knihovny; na čistém textovém serveru okno nezobrazí.
- Ve virtuálních strojích či na problematickém OpenGL zkus `YTDLP_GUI_SOFTWARE_RENDER=1 ./publish/yt-dlp-gui`.
- `YTDLP_GUI_TRACE=1` povolí doplňkovou diagnostiku Avalonia v terminálu.
- Pokud jednosouborové sestavení nemůže rozbalit nativní závislosti, nastav zapisovatelné `DOTNET_BUNDLE_EXTRACT_BASE_DIR` nebo použij standardní složkové sestavení.

## Nevidím nově přidaný jazyk

- Ulož slovník do správného `Locales` adresáře, např. `pt-BR.json`, a znovu otevři 🌐 nabídku jazyků v hlavičce.
- Soubor musí být validní JSON objekt s textovými klíči a hodnotami; poškozený JSON se ignoruje.
- Jazykový kód musí být rozpoznatelný v .NET. Je-li k dispozici jen `fr.json`, volba `fr-CA` může použít nadřazený `fr`.
- Přidaný slovník překládá jen prvky již zapojené do lokalizace; část pokročilých karet může stále zůstat česky.

## Nastavení se neukládá

Umístění konfigurace se liší podle platformy; viz [Nastavení](../Configuration/cs.md). Pokud aplikace nemůže soubor načíst, použije výchozí hodnoty. Neupravuj JSON za běhu, protože ho může přepsat automatické uložení.

## Nahlášení chyby

Použij [GitHub Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues). Uveď systém/architekturu, typ balíčku, postup reprodukce, verze yt-dlp/FFmpeg, očekávané a skutečné chování a **anonymizovaný** log. Nikdy nezveřejňuj cookies, tokeny, hesla ani soukromé odkazy.
