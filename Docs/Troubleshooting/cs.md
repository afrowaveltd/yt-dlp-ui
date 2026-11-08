# Řešení problémů

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Instalace](../Installation/cs.md)

## Chybí yt-dlp

Otevři **Nástroje** a spusť nové vyhledání. Zkontroluj, že funguje `yt-dlp --version` (ve Windows `yt-dlp.exe --version`). Pokud nástroj není v PATH, nastav přímo cestu ke spustitelnému souboru.

Instalátor Windows má samostatnou volbu **Nainstalovat yt-dlp**; pokud volitelné stahování selže, GUI se neodinstaluje. V Linuxu použij správce balíčků nebo jiný důvěryhodný způsob instalace yt-dlp. Jde o samostatný nástroj, nikoliv součást GUI.

## Chybí FFmpeg nebo nefunguje převod

Prověř `ffmpeg -version` a pro odpovídající operace také `ffprobe -version`. Windows x64 mohou nainstalovat vlastní ověřenou dvojici na kartě **Nástroje** či v instalačním průvodci; Linux zpravidla používá systémové balíčky. Podle potřeby nastav cestu k FFmpeg a zopakuj detekci. Prosté stažení streamu může proběhnout bez FFmpeg, ale spojení či převod často ne.

## Instalátor Windows nedokázal stáhnout nástroje

- Ověř připojení, DNS a dostupnost serverů vydavatelů — GitHub releases pro yt-dlp a zdroje FFmpeg.
- Podrobnosti hledej v `%LOCALAPPDATA%\yt-dlp-ui\logs\dependency-install.log`.
- Musí být dostupné také kontrolní součty vydavatele. Nesouhlas SHA-256 má instalaci zastavit místo spuštění poškozené binárky.
- Dokonči instalaci GUI a zopakuj pokus na kartě **Nástroje**, případně nainstaluj externí nástroje ručně.

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
