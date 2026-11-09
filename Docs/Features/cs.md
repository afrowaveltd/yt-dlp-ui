# Funkce a práce s aplikací

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [První kroky](../Getting-Started/cs.md)

Rozhraní usnadňuje první stažení, ale nechává přístupné pokročilé volby yt-dlp. **Opravné preview 0.1.4** má devět karet:

| Karta | Účel |
|---|---|
| **Stáhnout** | Odkaz, schránka a přetažení, analýza a náhled, profil, složka, spuštění/fronta |
| **Formát** | Dostupné streamy, výběr a řazení, kontejner, extrakce zvuku, převody/remux, časové úseky |
| **Playlist** | Režimy playlistu, rozsahy/pořadí, archiv, filtry, chyby a živé přenosy |
| **Titulky a metadata** | Jazyky a formáty titulků, vložení, obrázky, metadata, kapitoly, SponsorBlock |
| **Síť** | Proxy, timeouty, opakování, IPv4/IPv6, rychlostní limity, hlavičky a externí stahovač |
| **Soubory** | Šablony názvů, cílové/dočasné složky, přepisování, cache a volby názvů |
| **Přihlášení** | Cookies ze souboru/prohlížeče, přihlašovací údaje, `.netrc` a certifikáty |
| **Pokročilé** | Náhled vytvořeného příkazu, dodatečné argumenty, diagnostika a reset |
| **Nástroje** | Vyhledání/instalace/aktualizace yt-dlp, detekce a instalace FFmpeg a Deno na podporovaných systémech Windows/Linux, nastavení aplikace |

Některé speciální přepínače yt-dlp zůstávají v poli **vlastní argumenty** místo samostatného ovládacího prvku. GUI předává argumenty externímu yt-dlp; podporu webových služeb určuje jeho aktuální verze.

## Rychlá cesta

1. Vlož odkaz nebo ho přetáhni do okna.
2. Případně stiskni **Analyzovat** a prohlédni údaje o médiu.
3. Vyber profil kvality nebo **pouze zvuk**.
4. Zvol cílovou složku, klikni na **Stáhnout** či **Přidat do fronty**.
5. Sleduj stav a průběh; při problému zkontroluj log.

Sledování schránky a automatickou analýzu lze vypnout či zapnout v nastavení aplikace.

## Pokročilá cesta

Karty **Formát**, **Playlist**, **Titulky a metadata**, **Síť**, **Soubory** a **Přihlášení** umožňují podrobněji nastavit yt-dlp. Karta **Pokročilé** zobrazuje kopírovatelný příkaz a dovoluje zadat vlastní argumenty. S hesly, cookies a dalšími údaji zacházej opatrně.

Na pravé straně je fronta stahování, průběh a ovládání opakování/odstranění. Log pomáhá s diagnostikou neúspěšných stahování.

## Horní lišta a zkratky

Nahoře je **stav yt-dlp, FFmpeg a Deno**. Vedle indikátorů je 🌐 **výběr jazyka**, následovaný ovladačem tmavého/světlého motivu. Nabídka vzniká z JSON slovníků, nikoliv pevně daných dvou jazyků. Viz [Lokalizace](../Localization/cs.md).

| Zkratka | Akce |
|---|---|
| `Ctrl+L` | Přejít do pole odkazu |
| `F5` | Analyzovat odkaz |
| `Ctrl+Enter` | Spustit stahování |

## Známá omezení

- Některé dialogy, stavové zprávy a pokročilé volby zatím nemají úplnou lokalizaci.
- Pro macOS není připraven samostatný instalátor ani CI sestavení.
- Stahování ovlivňují dostupné služby, externí programy i připojení.
- Smoke testy neověřují všechny přepínače yt-dlp ani všechny webové servery.

[Nápověda](../Troubleshooting/cs.md) · [Příprava vydání](../Release-0.1.0/cs.md)
