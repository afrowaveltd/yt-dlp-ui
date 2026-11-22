# Přehrávač

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Funkce](../Features/cs.md)

Okno přehrávače slouží k prohlížení toho, co už máte stažené, aniž byste museli opustit aplikaci. Otevřete je tlačítkem **▶** v hlavičce nebo tlačítkem **Přehrát** u hotové položky ve frontě stahování.

## Co okno zobrazuje

| Oblast | Obsah |
|---|---|
| Seznam souborů | všechna videa a zvukové soubory v aktuální složce pro stahování, nejnovější první |
| Náhled | jeden snímek vytažený programem FFmpeg z vybraného místa |
| Posuvník | pohyb v souboru; každá pozice vykreslí nový snímek |
| Podrobnosti | kontejner, video a audio kodek, rozlišení, délka a velikost souboru |
| Akce | **Přehrát v systémovém přehrávači**, **Konvertovat tento soubor**, **Otevřít složku** |

Po novém stažení stiskněte **Obnovit**; okno složku průběžně nesleduje.

## Jak přehrávání funguje

Okno vykresluje **jednotlivé snímky**, nedekóduje video do aplikace. Tlačítko **Přehrát v systémovém přehrávači** otevře soubor v tom přehrávači, který máte v systému nastavený – například VLC, mpv, Celluloid nebo výchozí přehrávač GNOME/KDE.

Je to záměrné rozhodnutí: aplikace nenese ani nevyžaduje nativní přehrávací engine, takže zůstává malá a chová se stejně na každé ploše. Pokud pro daný typ souboru není přidružen žádný přehrávač, okno to oznámí místo tichého selhání.

Zvukové soubory zobrazí místo náhledu notu a otevřou se ve vašem zvukovém přehrávači.

## Konverze přímo z přehrávače

Tlačítko **Konvertovat tento soubor** vybere soubor na kartě **Konvertovat** a přepne na ni, takže to, co jste právě zkontrolovali, můžete jedním krokem převést do jiného formátu. Viz [Konvertor](../Converter/cs.md).

## Požadavky a omezení

- Náhledové snímky potřebují **ffmpeg**; bez něj seznam i podrobnosti fungují, ale obrázek se neobjeví.
- Technické údaje čte **ffprobe**, který obvykle bývá vedle ffmpeg.
- Zobrazují se jen soubory v nastavené složce pro stahování. Tu změníte na kartě **Stáhnout** nebo **Soubory**.
- Okno pracuje s jednou složkou, ne s celým diskem; na soubor se dostanete tlačítkem **Otevřít složku**.
- U velmi velkých nebo neobvyklých kontejnerů se náhled nemusí vytvořit; přehrávání to neovlivní.

[Instalace](../Installation/cs.md) · [Řešení problémů](../Troubleshooting/cs.md)
