# Lokalizace — dictionaryless JSON

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Nastavení](../Configuration/cs.md)

## Princip

**Anglický text je přímo klíčem.** Soubor `en.json` obsahuje anglické klíče a hodnoty, `cs.json` jejich české překlady. Například:

~~~json
{
  "Download": "Baixar",
  "About": "Sobre"
}
~~~

Tento příklad lze uložit jako `pt-BR.json`. Klíče rozlišují velká a malá písmena. Pro jednoduché statické texty nejsou potřeba placeholdery; lehký lokalizátor zatím záměrně **neimplementuje** rozšířené hinty, placeholdery ani providery UnderstandMe.

## Načítání jazyků a priorita

`SimpleLocalizer` vyhledává platné slovníky `*.json` automaticky v tomto pořadí (pozdější zdroj přepíše předchozí slovník téhož jazyka):

1. Zabalené slovníky z `src/YtDlpUi.Core/Locales/*.json`.
2. `Locales/` vedle spustitelného souboru (také ve výstupu publish).
3. Uživatelská konfigurace:
   - Linux: `~/.config/yt-dlp-ui/Locales/` nebo `$XDG_CONFIG_HOME/yt-dlp-ui/Locales/`
   - Windows: `%LOCALAPPDATA%\yt-dlp-ui\Locales\`

Název souboru odpovídá jazykovému kódu, např. `cs`, `fr` nebo `pt-BR`. Neplatný či nečitelný JSON se ignoruje. Pokud přesný jazyk není k dispozici, zkouší se nadřazený (např. `fr-CA` -> `fr`), potom angličtina. Chybějící nebo prázdné překlady vrací původní anglický klíč.

## Přidání nového jazyka

1. Vytvoř platný JSON se jménem jazyka, například `pt-BR.json`.
2. Přidej požadované položky podle anglických klíčů v `src/YtDlpUi.Core/Locales/en.json`.
3. Ulož jej do uživatelské složky `Locales/` bez nutnosti kompilace; pro zabalení do nového releasu soubor commitni do `src/YtDlpUi.Core/Locales/`.
4. Otevři 🌐 nabídku jazyků v horní pravé části okna. Při otevření se seznam znovu načítá.
5. Vyber jazyk. Již lokalizované texty se změní bez restartu a volba se uloží do `settings.json`.

Výchozí jazyk odpovídá systémovému nastavení, pokud existuje jeho slovník, jinak se použije angličtina. **Přidání JSON** samo o sobě neznamená **překlad všech obrazovek**: automaticky se mění prvky používající `TrExtension`, zatímco některé starší české zprávy ještě čekají na převod.

## XAML a vývoj

Pro nový text přidej anglický klíč do `en.json`, českou hodnotu do `cs.json` a použij vazbu:

~~~xml
<TextBlock Text="{loc:Tr Key='Download'}" />
~~~

`TrExtension` vrací pozorovatelnou vazbu, takže text reaguje na přepnutí jazyka bez nového okna. Dynamické věty raději neskládej spojením fragmentů. Pokud budou potřeba placeholdery, přidej je promyšleně a validuj slovníky před přechodem na UnderstandMe.

## Co testujeme

Smoke testy pokrývají zabalené i nově přidané JSON soubory, neplatný JSON, `pt-BR`, nadřazený `fr` při volbě `fr-CA`, změnu/odstranění za běhu, fallback na angličtinu a uživatelské přepsání českého slovníku. Nejde o tvrzení, že je celé UI přeložené.

[Vývoj a testování](../Development-and-Verification/cs.md)
