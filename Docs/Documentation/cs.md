# Dokumentace — yt-dlp GUI

**Jazyk:** [English](en.md) · Česky · [README repozitáře](../../README.md)

Rozcestník popisuje veřejnou **komunitní opravnou předběžnou verzi 0.1.5** a odděluje hotové funkce, ověřené sestavení a ruční testy, které ještě zbývají. Každé téma má v `Docs/<Téma>/` soubory `en.md` a `cs.md`.

## Pro uživatele

1. [První kroky](../Getting-Started/cs.md) — spuštění, první video a základní ovládání.
2. [Instalace a distribuce](../Installation/cs.md) — Windows setup/portable, DEB, RPM, přenosný Linux a volba závislostí.
3. [Funkce](../Features/cs.md) — všech devět karet, fronta, formáty, příkaz a klávesové zkratky.
4. [Lokalizace](../Localization/cs.md) — dictionaryless JSON a přidání jazyka za běhu.
5. [Nastavení a úložiště](../Configuration/cs.md) — konfigurace, složky a soukromí.

## Správa a vývoj

6. [Řešení problémů](../Troubleshooting/cs.md) — chybějící nástroje, instalátory, síť, grafika Linuxu a logy.
7. [Vývoj a ověřování](../Development-and-Verification/cs.md) — .NET 10, projekty, smoke testy a CI.
8. [Verzování a vydávání](../Versioning/cs.md) — PATCH opravy, MINOR preview, stabilní 1.x, tagy a pravidla vydávání.
9. [Android prototyp](../Android/cs.md) — vlastní mobilní rozhraní, sdílené jádro, přímé HTTPS soubory; není součástí v0.1.0.
10. [Oprava ve verzi 0.1.5](../Release-0.1.5/cs.md) — názvy a délky videí v náhledu playlistu.
11. [Opravy ve verzi 0.1.4](../Release-0.1.4/cs.md) — kontrolky Deno, karta Nástroje a vyhledávání v Linuxu.
12. [Opravy ve verzi 0.1.3](../Release-0.1.3/cs.md) — JavaScript runtime YouTube a volitelná instalace Deno.
13. [Opravy ve verzi 0.1.2](../Release-0.1.2/cs.md) — UAC a společné nástroje Windows.
14. [Opravy ve verzi 0.1.1](../Release-0.1.1/cs.md) — historické opravné vydání a testy.
15. [Stav vydání 0.1.0](../Release-0.1.0/cs.md) — **[zveřejněný pre-release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)**, známé nedodělky a kontrolní seznam.

## README v obou jazycích

- [Anglická kopie README](../README/en.md) — odpovídá kořenovému README s upravenými odkazy.
- [Česká kopie README](../README/cs.md) — česká varianta kořenového README.
- [Hlavní README](../../README.md) — kanonická vstupní stránka GitHubu v angličtině.

## Pravidla pro dokumentaci

- **Angličtina jako první:** nové téma dostane `Docs/Tema/en.md` i `Docs/Tema/cs.md`.
- Popisujeme **to, co skutečně funguje v kódu**, ne budoucí plány jako hotové funkce.
- Při změně funkcí, konfiguračních cest či balíčků aktualizujeme README, oba jazyky i stav vydání.
- Ověřujeme místní odkazy a příklady. Úspěšná kompilace v CI nenahrazuje instalaci na skutečných Windows/Linux.
- Snímky obrazovky zůstávají ve složce `screenshots/`; odkazy se píšou relativně ke konkrétnímu dokumentu.

[Repozitář](https://github.com/afrowaveltd/yt-dlp-ui) · [Sestavení CI](https://github.com/afrowaveltd/yt-dlp-ui/actions)
