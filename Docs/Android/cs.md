# Android prototyp — vývoj směrem k 0.2.0

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Verzování](../Versioning/cs.md)

Aplikace pro Android má **vlastní dotykové rozhraní Avalonia** a od desktopové verze přebírá pouze nezávislé jádro `YtDlpUi.Core`. Jde o experimentální **vývojový prototyp 0.2.0**, nikoli o součást vydaného desktopového [v0.1.0](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0).

## Co už umí

- Samostatný projekt `src/YtDlpUi.Android/` a solution `YtDlpUi.Android.slnx`; desktop není závislý na Android workloadu.
- Jednoduchá dotyková obrazovka: zadání odkazu, výslovné spuštění stahování, stav a nedávné úlohy.
- Příjem URL z androidové nabídky **Sdílet** (`ACTION_SEND` / `text/plain`). Samotné sdílení nikdy **nespustí stahování bez klepnutí uživatele**.
- Pro přímé **HTTPS odkazy na soubory médií** (`.mp4`, `.webm`, `.mp3` a další) využívá systémový **DownloadManager**. Ukládá do veřejné složky **Downloads**; průběh, opakování a oznámení řeší Android.
- Používá společný, automaticky testovaný parser odkazů v `YtDlpUi.Core`.

**Zatím nejde o plný yt-dlp engine pro Android:** odkazy na stránky YouTube/Vimeo nejsou přímými soubory. Prototyp srozumitelně vysvětlí, že takové odkazy ještě nestahuje. Analýza webů, kvality, playlisty a FFmpeg potřebují příští etapu.

## Oprávnění — jen nezbytná

Deklarujeme pouze běžné oprávnění `android.permission.INTERNET`. Android DownloadManager umí na Androidu 10+ ukládat do systémových Downloads bez plošného přístupu k úložišti. **Nežádáme** `MANAGE_EXTERNAL_STORAGE`, galerii, kameru, mikrofon, kontakty, polohu ani root. Internet je běžné instalační oprávnění, takže obvykle nevyžaduje dodatečný potvrzovací dialog.

Pokud další funkce bude skutečně potřebovat nové oprávnění (například vlastní oznámení služby na pozadí), požádáme až při použití a vysvětlíme proč.

## Sestavení testovacího APK

Potřebuješ **.NET 10 SDK**, Android workload, Android SDK platformu 36 a JDK 17:

~~~bash
dotnet workload install android
dotnet build YtDlpUi.Android.slnx -c Debug -p:AndroidPackageFormat=apk
~~~

[Android CI](https://github.com/afrowaveltd/yt-dlp-ui/actions/workflows/android-preview.yml) vytvoří **pouze vývojové debug APK**, nikoli veřejný produkční release ani balíček připravený do obchodu. Ještě je třeba otestovat instalaci, sdílení i stahování na skutečném telefonu. V nastavení projektu je minimum Android 10 (API 29), ale podporu verzí **ještě nemáme ručně ověřenou**.

## Architektura a další kroky

- **Core:** společné zpracování odkazů, volby a budoucí kontrakty fronty.
- **Android host:** aktivita, sdílení, oprávnění a systémové Downloads.
- **Mobilní UI:** vlastní pohledy a ViewModely Avalonia; žádná kopie devíti desktopových karet.
- **Stahovací engine:** oddělený adaptér, zatím **jen pro přímé soubory**.

Před plnohodnotným yt-dlp prověříme možnosti zabalení Pythonu/yt-dlp či použití nativní knihovny, omezení architektur a licence externích komponent (některé možnosti používají GPL). Nikdy nebudeme spouštět libovolné programy stažené do zapisovatelné složky aplikace. Systémový výběr umístění, skutečnou frontu, průběh/rušení, mobilní lokalizaci a služby na pozadí doplníme, až budou potřebné.

[Repozitář](https://github.com/afrowaveltd/yt-dlp-ui) · [Desktop release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.0)
