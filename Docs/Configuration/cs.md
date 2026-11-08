# Nastavení, soubory a soukromí

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Lokalizace](../Localization/cs.md)

Předvolby aplikace jsou v souboru JSON, odděleně od binárek yt-dlp a FFmpeg. Zdrojem pravdy je `AppSettings`, `SettingsStore` a `UserPaths` v `src/YtDlpUi.Core/`.

## Umístění souborů

| Položka | Linux | Windows |
|---|---|---|
| Nastavení aplikace | `~/.config/yt-dlp-ui/settings.json` nebo `$XDG_CONFIG_HOME/yt-dlp-ui/settings.json` | `%LOCALAPPDATA%\yt-dlp-ui\settings.json` |
| Vlastní překlady | `~/.config/yt-dlp-ui/Locales/*.json` | `%LOCALAPPDATA%\yt-dlp-ui\Locales\*.json` |
| Cache aplikace | `~/.cache/yt-dlp-ui/` nebo `$XDG_CACHE_HOME/yt-dlp-ui/` | `%LOCALAPPDATA%\yt-dlp-ui\` |
| yt-dlp spravované aplikací | `~/.local/bin/yt-dlp` | `%LOCALAPPDATA%\yt-dlp-ui\tools\yt-dlp.exe` |
| FFmpeg spravovaný aplikací | Zpravidla instalovaný distribucí | `%LOCALAPPDATA%\yt-dlp-ui\tools\ffmpeg\ffmpeg.exe` a `ffprobe.exe` |
| Sdílené nástroje ze setupu | Nevztahuje se | `%ProgramData%\yt-dlp-ui\tools\yt-dlp.exe`, `tools\ffmpeg\ffmpeg.exe`, `ffprobe.exe` a `tools\deno.exe` |
| Log Windows instalátoru | Nevztahuje se na DEB/RPM | `%ProgramData%\yt-dlp-ui\logs\dependency-install.log` |

**Cílová složka stahování** je uživatelsky nastavitelná. Na Linuxu se nejdřív hledá `user-dirs.dirs`, jinak `~/Downloads` nebo domovský adresář. Ne všichni uživatelé proto budou stahovat do stejné složky.

## Uložené předvolby

`AppSettings` obsahuje vlastní cesty pro yt-dlp a FFmpeg, výchozí složku, sledování schránky, automatickou analýzu po vložení, kontrolu aktualizací yt-dlp, pamatování voleb, jazyk, motiv, souběžnost a stav prvního spuštění i rozměry okna. Většina změn se uloží po krátké prodlevě nebo při běžném ukončení.

Volba jazyka `system` používá systémové prostředí, pokud existuje odpovídající JSON, jinak angličtinu. Ručně vybraný jazyk se ukládá jako kód.

Pro YouTube GUI automaticky vyhledá Deno ve společné složce nástrojů, uživatelské složce či PATH a předá jej programu yt-dlp. Výslovně nastavený JavaScript runtime na kartě Síť nebo další parametry mají přednost. Oficiální samostatné yt-dlp.exe už obsahuje EJS skripty.

Na kartě **Nástroje** lze nastavit vlastní cesty pro programy mimo běžné složky. V hlavičce je jejich detekovaný stav. Aplikace se snaží ověřit, že je program opravdu spustitelný; nekontroluje jen existenci souboru.

## Bezpečnost a soukromí

- Podporovaná pole hesel a 2FA se před uložením **posledních voleb stahování** odstraní. Nejde ale o šifrovaný trezor hesel.
- Cookies, příkazové argumenty, externí procesy a logy mohou přesto obsahovat citlivé údaje. Zkontroluj, co sdílíš, zejména u podrobného logu a náhledu příkazu.
- GUI záměrně neobsahuje reklamu ani telemetrii. Připojuje se však k internetu kvůli metadatům/stahování a může komunikovat kvůli aktualizacím či instalaci nástrojů.
- Balíčky DEB/RPM používají doporučené závislosti, nikoliv automatické stahování po instalaci. Windows průvodce žádá UAC, má samostatné volby a používá SHA-256 od vydavatele pro společné nástroje.
- Odinstalace GUI ve Windows automaticky nemaže samostatně nainstalované uživatelské ani sdílené nástroje.

## Bezpečná změna konfigurace

1. Pro běžné úpravy upřednostni kartu **Nástroje**.
2. Pokud chceš upravit `settings.json` ručně, zavři nejdřív aplikaci, ať změny nepřepíše automatické uložení.
3. Před experimentováním s pokročilými volbami soubor zazálohuj.
4. Při neplatném či nečitelném JSON aplikace používá výchozí nastavení; automatická záchrana původního souboru není zaručená.
5. Nový překlad vlož jako jazykový JSON do `Locales` a znovu otevři nabídku jazyků. Viz [Lokalizace](../Localization/cs.md).

[Instalace](../Installation/cs.md) · [Řešení problémů](../Troubleshooting/cs.md)
