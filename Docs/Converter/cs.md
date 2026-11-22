# Konvertor FFmpeg

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Funkce](../Features/cs.md)

Karta **Konvertovat** převádí soubor, který už máte, do jiného formátu. Je to tenké a srozumitelné rozhraní nad FFmpeg: vy zvolíte, co má vzniknout, stránka ukáže přesný soubor, který se vytvoří, a do stisknutí tlačítka **Konvertovat** se nic nezapisuje.

## Postup

1. Vyberte **zdrojový soubor**. Do pole lze vložit libovolnou cestu; tlačítko **Procházet…** otevře výběr omezený na média.
2. Stránka soubor přečte pomocí **ffprobe** a ukáže jeho souhrn (kodek, rozlišení, délku, velikost).
3. Zvolte **formát**, **kvalitu** a případně **maximální výšku**.
4. Volitelně nastavte **střih** nebo zaškrtněte **Odebrat zvuk**.
5. Volitelně zvolte **cílovou složku**; přesné umístění vidíte ještě před spuštěním.
6. Stiskněte **Konvertovat**. Průběh a rychlost se zobrazují na stránce i v hlavním logu.

## Cílové formáty

| Skupina | Formáty |
|---|---|
| Video | MP4 (H.264 + AAC), MKV, WebM (VP9 + Opus), MOV, AVI, GIF |
| Zvuk | MP3, M4A (AAC), Opus, FLAC, WAV |

MP4 je nejbezpečnější volba pro televize, telefony a prohlížeče a zapisuje se s indexem na začátku (`+faststart`), takže přehrávání může začít dřív, než se soubor celý načte. MKV snese téměř jakýkoli kodek. WebM je svobodný formát, ale pomaleji se kóduje.

Zvukové formáty zachovají jen zvukovou stopu a obraz automaticky vynechají.

## Úrovně kvality

| Úroveň | Video | Zvuk |
|---|---|---|
| Vysoká | CRF 18, pomalý preset kodéru | 320 kbit/s (Opus/WebM: 160) |
| Vyvážená (výchozí) | CRF 23, preset medium | 192 kbit/s (Opus/WebM: 128) |
| Malá | CRF 28, preset veryfast | 128 kbit/s (Opus/WebM: 96) |

Bezztrátové formáty (FLAC, WAV) úroveň kvality ignorují.

## Zmenšování a střih

- **Maximální výška** omezí výšku obrazu. Menší zdroj se nikdy nezvětšuje a volba **Ponechat původní velikost** rozlišení nechá být.
- **Stříhat od** a **Stříhat do** přijímají sekundy (`90`), minuty (`1:30`) nebo hodiny (`00:01:30`). Necháte-li obě pole prázdná, převede se celý soubor. Hledání probíhá před načtením vstupu, takže střih dlouhého souboru je rychlý a přesto přesný na snímek.

## Původní soubor zůstává vždy zachován

Konverze nikdy nepřepíše svůj vlastní zdroj. Při převodu `clip.mp4` do MP4 vznikne `clip (converted).mp4`. Existující výsledek se přepíše jen tehdy, když je zaškrtnuté **Přepsat existující soubor**.

Zrušená konverze po sobě uklidí nedokončený výstup, takže nezůstane rozbitý soubor.

## Požadavky a omezení

- Musí být dostupný **ffmpeg** a **ffprobe**. Stav ukazuje karta **Nástroje**, ve Windows je umí nainstalovat.
- Konverze běží tak rychle, jak zvládne procesor; stránka ukazuje násobek rychlosti a umožňuje zrušení.
- Najednou běží jen jedna konverze; po dobu běhu je tlačítko neaktivní.
- Stránka pokrývá běžné případy. Co tu není, lze při stahování předat yt-dlp přes **Pokročilé → vlastní argumenty**, případně použít FFmpeg přímo.
- Titulky a více stop nejsou vystavené; FFmpeg zachová výchozí stopy.

[Přehrávač](../Player/cs.md) · [Řešení problémů](../Troubleshooting/cs.md)
