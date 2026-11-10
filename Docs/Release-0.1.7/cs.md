# Vydání 0.1.7 — Oprava instalace yt-dlp a Deno

[English](en.md) · [Dokumentace](../Documentation/cs.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.7)

Ve v0.1.6 Windows instalátor nenacházel kontrolní součet yt-dlp.exe a Deno odmítalo oficiální soubor s kontrolním součtem. FFmpeg se instaloval správně.

- Instalační skript nyní stahuje `SHA2-256SUMS` jako soubor a čte jej jako UTF-8, bez chybné konverze binární HTTP odpovědi v PowerShellu.
- Společná i uživatelská instalace Deno podporuje oficiální výstup Windows PowerShellu `Get-FileHash` i GNU formát.
- Ověření SHA-256 od vydavatele je stále povinné a odmítá chybné, cizí či duplicitní záznamy.
- Přidány testy parsování a dekódování v PowerShellu i C#.

CI pro Windows i Linux u opravy prošlo a úspěšnou instalaci všech tří volitelných nástrojů na druhém Windows PC uživatel ručně potvrdil. Nejde o ověření všech síťových prostředí a linuxových správců balíčků.

Nadále jde o nepodepsané komunitní předběžné vydání s částečnou lokalizací; Android není součástí. Funkce FFmpeg, playlistů a stahování se jinak nemění.

[Instalace](../Installation/cs.md) · [Řešení problémů](../Troubleshooting/cs.md) · [Úplné poznámky](../../RELEASE_NOTES_0.1.7.md)
