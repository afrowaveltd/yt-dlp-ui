# Verzování a pravidla vydávání

[English](en.md) · [Rozcestník dokumentace](../Documentation/cs.md) · [Historie vydání](https://github.com/afrowaveltd/yt-dlp-ui/releases)

U yt-dlp GUI používáme jednoduchý formát **MAJOR.MINOR.PATCH**. Vycházíme ze zásad [Semantic Versioning](https://semver.org/), ale máme jednu jasnou projektovou dohodu: **všechna vydání `0.x.y` jsou na GitHubu označená jako pre-release** a **`1.0.0` bude první stabilní vydání**.

## Co znamenají jednotlivé verze

| Příklad | Význam | Označení na GitHubu |
| --- | --- | --- |
| `0.1.0` | První veřejné preview, první vývojová generace | Pre-release |
| `0.1.1`, `0.1.2` | Opravy chyb a údržba bez nové uživatelské funkcionality | Pre-release |
| `0.2.0` | Druhá vývojová generace s novými nebo výrazně rozšířenými funkcemi | Pre-release |
| `0.2.1` | Opravy ve větvi `0.2` | Pre-release |
| `0.3.0` | Třetí vývojová generace | Pre-release |
| `1.0.0` | První stabilní vydání po splnění přejímacích kritérií | Stable |
| `1.0.1` | Zpětně kompatibilní opravy | Stable |
| `1.1.0` | Nové zpětně kompatibilní funkce stabilní verze | Stable |
| `2.0.0` | Velká změna nebo záměrná nekompatibilita | Stable |

Číslo `0.2.0` znamená **druhou vývojovou generaci**, nikoliv nutně druhé zveřejněné vydání: mezitím mohou vzniknout třeba opravné verze `0.1.1` a `0.1.2`.

## Jak zvolit další číslo verze

1. **PATCH** — `0.1.0 → 0.1.1`: opravy chyb, balíčků, výkonu, správnosti a dokumentace bez přidání uživatelských funkcí. Pokud opravíme jen dokumentaci a nové binární vydání není potřeba, stačí upravit texty v repozitáři.
2. **MINOR před 1.0** — `0.1.x → 0.2.0`: nové funkce, podstatně rozšířené pracovní postupy nebo významné změny UI během vývoje. Číslo PATCH se vrací na nulu.
3. **První stabilní vydání** — `0.x.y → 1.0.0`: nevzniká automaticky. Vydáme ho až po splnění přejímacích kritérií pro instalaci, stahování, testování i dokumentaci.
4. **Od 1.0** — PATCH zvyšujeme pro zpětně kompatibilní opravy, MINOR pro kompatibilní nové funkce a MAJOR pro změny narušující veřejnou kompatibilitu. Nižší čísla se při zvýšení vyššího vrací na nulu.

Před verzí 1.0 máme větší prostor měnit chování, ale každou nekompatibilitu v konfiguraci, datech nebo používání musíme výslovně popsat v poznámkách k vydání. Přidání nové funkce nebudeme maskovat jako drobnou PATCH opravu.

## Životní cyklus vydání

- Jediným zdrojem verze je `Directory.Build.props`; odpovídající číslo musí být také v metadatech Windows instalátoru, názvech balíčků a dokumentaci.
- Každé vydání má vlastní Git tag s prefixem `v`: `v0.1.0`, `v0.1.1`, `v0.2.0`, `v1.0.0`…
- **Zveřejněné tagy a instalační soubory neměníme.** Oprava znamená nové číslo, nikoli přepsání předchozího vydání.
- Každé `0.x.y` označíme jako GitHub **pre-release**; `1.x.y` bude standardně **stable**, pokud výslovně nerozhodneme jinak.
- Před vydáním spustíme CI pro Linux/Windows, ověříme balíčky a kontrolní součty a zveřejníme poznámky o změnách, omezeních i případném postupu aktualizace.
- Aktualizujeme anglické kořenové README a kopii v `Docs/README/en.md`, český README v `Docs/README/cs.md` a obě verze dotčených kapitol `Docs/<Téma>/en.md` / `cs.md`.
- Chyby a návrhy může komunita hlásit přes [GitHub Issues](https://github.com/afrowaveltd/yt-dlp-ui/issues).

## První veřejné vydání

**[v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)** je zveřejněné první preview. **[v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)** je první opravné vydání; **[v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)** opravuje Windows setup; **[v0.1.3](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.3)** doplňuje Deno runtime; **[v0.1.4](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.4)** dokončuje ovládání Deno v desktopové aplikaci a detekci v Linuxu; **[v0.1.5](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5)** opravuje názvy videí v playlistu; **[v0.1.6](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.6)** opravuje záložní vykreslení playlistu a přidává headless UI testy; **[v0.1.7](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.7)** opravuje kontrolní součty v instalátorech yt-dlp a Deno; další opravy mohou použít **v0.1.8**, výraznější nové funkce patří do **v0.2.0**. Přidání nové platformy, například případné androidové aplikace, patří mezi nové funkce, a proto do MINOR vydání (nebo samostatně verzovaného produktu), nikoli do PATCH opravy.

**Technická poznámka:** GitHubův přepínač *pre-release* je nezávislý na číslu verze. My ho záměrně používáme pro všechna `0.x.y` vydání; není to automatické pravidlo samotné specifikace SemVer.
