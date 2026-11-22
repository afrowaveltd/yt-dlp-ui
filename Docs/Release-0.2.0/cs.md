# Vydání 0.2.0 – vestavěný přehrávač a konvertor FFmpeg

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [GitHub release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.2.0)

Jde o **předběžné vydání s novými funkcemi**, druhou náhledovou generaci. Přidává dvě možnosti, kvůli kterým jste dřív museli aplikaci opustit: prohlédnout si, co jste stáhli, a převést to jinam.

## Okno přehrávače

- Otevřete je tlačítkem **▶** v hlavičce nebo tlačítkem **Přehrát** u hotové položky fronty.
- Vypisuje všechna videa a zvukové soubory ve složce pro stahování, nejnovější první.
- Ukazuje kontejner, kodeky, rozlišení, délku a velikost načtené pomocí **ffprobe**.
- Vykresluje **snímek, kterým lze posouvat** pomocí posuvníku; každá pozice vytvoří nový snímek.
- **Přehrát v systémovém přehrávači** otevře soubor ve vašem přehrávači. Aplikace stále nenese žádný nativní přehrávací engine, takže zůstává malá a přenositelná.

## Karta Konvertovat

- Zdrojový soubor, cílový formát, úroveň kvality, volitelná maximální výška, volitelný střih a volba „odebrat zvuk“.
- Formáty: MP4, MKV, WebM, MOV, AVI, GIF, MP3, M4A, Opus, FLAC, WAV.
- Živé procento a násobek rychlosti z machine-readable výstupu FFmpeg; úlohu lze zrušit.
- **Zdrojový soubor se nikdy nepřepíše.** Při převodu do stejného kontejneru vznikne `<název> (converted).<přípona>`.
- Zrušená konverze po sobě uklidí nedokončený výstup.
- Výsledek lze přehrát nebo zobrazit ve složce přímo ze stránky.

## Lokalizace a dokumentace

- Každý nový ovládací prvek, popisek volby i stavová zpráva má anglický a český záznam; popisky voleb se při změně jazyka překládají za běhu.
- Nová dokumentace: [Přehrávač](../Player/cs.md) a [Konvertor](../Converter/cs.md), aktualizované stránky funkcí, verzování a rozcestníku.

## Testování

- Offline smoke testy pokrývají mapování argumentů FFmpeg (video, zvuk, GIF, zmenšení, střih, `-an`, `-n`), řešení kolize výstupní cesty, parsování času, parsování průběhu, parsování ffprobe JSON (včetně obalu, který se nesmí počítat jako video), procházení složky s médii, mapování formátů a paritu slovníků.
- Headless testy Avalonia vykreslují **skutečnou** stránku konvertoru a **okno přehrávače**, ověřují svázání ovládacích prvků a že se popisky voleb překládají za běhu.
- Strukturální pojistky drží indexy nových karet v souladu s pořadím karet a vyžadují existenci vstupních bodů přehrávače i konvertoru.
- Samotná konverzní pipeline byla navíc spuštěna proti skutečnému FFmpeg 7.1: MP4→MP3, →WebM (zmenšeno), →MKV (bez zvuku), MP4 se střihem, →FLAC, →GIF a převod do stejného kontejneru, který musí původní soubor zachovat.

## Známá omezení

- Okno přehrávače zobrazuje snímky; souvislé přehrávání je záměrně delegováno na systémový přehrávač.
- Najednou běží jedna konverze a titulky či více stop nejsou vystavené.
- Karta Konvertovat zatím není na screenshotách.
- GUI zůstává **nepodepsané a částečně lokalizované** komunitní předběžné vydání; některé pokročilé karty mají popisky jen česky.
- Windows Setup a linuxové balíčky sestavilo CI, ale toto vydání nebylo ručně přeinstalováno na druhém počítači tak jako 0.1.7.

[Instalace](../Installation/cs.md) · [Řešení problémů](../Troubleshooting/cs.md) · [Úplné poznámky](../../RELEASE_NOTES_0.2.0.md)
