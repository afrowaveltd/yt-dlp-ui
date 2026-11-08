# yt-dlp GUI v0.1.3 — JavaScript prostředí pro YouTube

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Verzování](../Versioning/cs.md)

**[Stáhnout v0.1.3](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.3)** · [Předchozí v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)

Toto opravné vydání řeší opakované upozornění YouTube **„No supported JavaScript runtime could be found“**, kvůli kterému mohou v yt-dlp chybět některé formáty nebo časem přestat fungovat části stahování.

## Co jsme opravili

- Při analýze metadat i stahování GUI **automaticky najde Deno**, i když není v PATH. Ve Windows hledá ve společném ProgramData, osobní složce nástrojů, uživatelské instalaci Deno a PATH. Předá yt-dlp správnou absolutní cestu přes `--js-runtimes deno:CESTA`.
- Instalátor Windows nabízí **tři nezávislé, předem zaškrtnuté** volby: yt-dlp (nutné), FFmpeg (doporučeno) a **Deno (doporučeno pro JavaScript výzvy YouTube)**. Každou lze vypnout.
- Instalace Deno stahuje oficiální Windows x64 ZIP s odpovídajícím SHA-256 součtem, rozbalí jen očekávaný `deno.exe`, ověří minimální verzi **2.3.0** a při chybě výměny zachová předchozí funkční kopii.
- Ručně zvolený runtime (např. Node 22+) a pokročilé parametry yt-dlp mají před automatickou detekcí přednost. Bez volby v instalátoru se žádný runtime sám nestahuje.
- Přidali jsme smoke testy přednosti parametrů a cest a kontroly instalátoru v CI Windows.

## Instalace a kompatibilita

- Instalátor Windows nadále vyžaduje UAC a instaluje pro všechny uživatele. Deno ukládá do `%ProgramData%\yt-dlp-ui\tools\deno.exe`; běžné spuštění GUI práva správce nepotřebuje.
- Pokud **Deno už máš**, nemusíš ho instalovat znovu. Pro ruční instalaci ve Windows použij `winget install --id DenoLand.Deno -e`, restartuj GUI a ověř `deno --version`.
- V Linuxu nainstaluj aktuální Deno z oficiálních zdrojů či správce balíčků. GUI ho najde v PATH.
- Oficiální samostatný yt-dlp má skripty `yt-dlp-ejs` přibalené. Instalace přes Python/pip či distribuci mohou vyžadovat odpovídající EJS balíček; viz [návod yt-dlp](https://github.com/yt-dlp/yt-dlp/wiki/EJS).
- Samostatné Deno je pod vlastní licencí MIT, GUI zůstává MIT a yt-dlp i FFmpeg mají své licence.
- Žádné další funkce stahování nepřibyly, experimentální Android v tomto vydání není.

## Testování a známá omezení

CI sestaví Windows i Linux, spustí smoke testy, ověří balíčky a vytvoří pět distribučních souborů se SHA256SUMS. Skripty kontrolujeme staticky, ale **skutečné potvrzení UAC, instalaci Deno a stahování z YouTube musí ještě prověřit uživatel na Windows PC**. YouTube se navíc mění nezávisle na našem GUI. Balíčky nejsou podepsané a vydání je stále pre-release.

[Hlášení chyb](https://github.com/afrowaveltd/yt-dlp-ui/issues) uvítáme s verzí OS, GUI, yt-dlp a Deno, postupem a očištěným logem.
