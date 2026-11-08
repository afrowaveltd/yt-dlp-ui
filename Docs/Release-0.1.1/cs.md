# Opravné předběžné vydání v0.1.1

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Verzování](../Versioning/cs.md)

**[Stáhnout v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)** · [Předchozí v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)

První **PATCH** vydání veřejného desktopového preview. **Nepřidává nové uživatelské funkce.** Cesty ke konfiguraci a identita aplikace zůstávají stejné.

## Opravy

1. **Globus u jazyků:** ořezávané emoji jsme nahradili vektorovou ikonou. Samostatný sloupec a mezera 18 px ji oddělují od nabídky na Linuxu i Windows.
2. **Fronta:** při zařazení každé položky se uchová její formát, cílová složka a dočasné přihlašovací údaje. Pozdější změny v GUI už neovlivňují čekající stahování. Opakování používá původní volby.
3. **Ovládání fronty:** právě stahovanou položku nelze odebrat ze seznamu bez jejího zrušení.
4. **Instalace yt-dlp:** před výměnou se ověří SHA-256 konkrétní binárky podle oficiálního souboru `SHA2-256SUMS`. Při neúspěchu se odstraní dočasné soubory a stará funkční instalace zůstane zachována.
5. **Instalace FFmpeg:** nové ověřené FFmpeg/FFprobe se připraví zvlášť, stará složka se dočasně zazálohuje a při neúspěšné výměně obnoví.
6. **Nastavení:** JSON se zapisuje kompletně do dočasného souboru a pak se atomicky přejmenuje. Vypnutí pamatování voleb odstraní starý uložený snímek.
7. **Soukromí:** do logu se již automaticky nepřidávají celé příkazy, které by mohly obsahovat hesla, cookies nebo tokeny. Ruční náhled příkazu zůstává dostupný.

## Testování a balíčky

Offline testy prověřují přesné i duplicitní SHA-256 záznamy, simulované úspěšné a zamítnuté instalace, zachování původní binárky, nezávislé parametry fronty, výměnu složek a ukládání nastavení. Zůstávají také testy překladů a příkazové řádky.

CI připravuje Windows x64 setup a portable ZIP, Linux x64 DEB/RPM/tar.gz a soubor `SHA256SUMS.txt`. K publikaci přistoupí teprve po úspěšném Linux/Windows CI. Zelený build **nenahrazuje** praktickou instalaci na každé distribuci ani otestování všech videoportálů.

## Aktualizace

- Nastavení a externí nástroje zůstávají na svých místech. Pro aktualizaci samotného GUI není nutné přeinstalovat yt-dlp nebo FFmpeg.
- Windows: spusť nový instalátor nebo nahraď celou portable složku. Debian/Ubuntu: instaluj nový DEB pomocí APT. Distribuce RPM: použij svého správce balíčků.
- Experimentální Android není součástí tohoto desktopového PATCH vydání; vyvíjí se pro generaci `0.2.0`.
- GUI zůstává MIT, yt-dlp a FFmpeg mají své licence. Balíčky nejsou digitálně podepsané. Část pokročilého rozhraní zůstává nepřeložená.

[GitHub Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues) slouží k hlášení chyb a nápadů. Uveď systém, verze nástrojů a očištěný log, nikdy hesla či cookies. Historický tag `v0.1.0` zůstává beze změny.
