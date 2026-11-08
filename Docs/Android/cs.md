# Android prototyp — vývoj směrem k 0.2.0

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Verzování](../Versioning/cs.md)

Aplikace pro Android má **vlastní dotykové rozhraní Avalonia 12** a od desktopové verze přebírá pouze nezávislé jádro `YtDlpUi.Core`. Mobilní projekt používá Avalonia 12.1.4 nezávisle na desktopové Avalonia 11; sdílené jádro na Avalonia nezávisí. Jde o experimentální **vývojový prototyp 0.2.0**, nikoli o součást vydaného desktopového [v0.1.3](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.3).

## Co už umí

- Samostatný projekt `src/YtDlpUi.Android/` a solution `YtDlpUi.Android.slnx`; desktop není závislý na Android workloadu.
- Jednoduchá dotyková obrazovka: zadání odkazu, výslovné spuštění stahování, stav a nedávné úlohy.
- Příjem URL z androidové nabídky **Sdílet** (`ACTION_SEND` / `text/plain`). Samotné sdílení nikdy **nespustí stahování bez klepnutí uživatele**.
- Pro přímé **HTTPS odkazy na soubory médií** (`.mp4`, `.webm`, `.mp3` a další) využívá systémový **DownloadManager**. Ukládá do veřejné složky **Downloads**; průběh, opakování a oznámení řeší Android.
- Používá společný, automaticky testovaný parser odkazů v `YtDlpUi.Core`.

- **Odkazy na videoportály** nově dostává samostatný lokální adaptér s knihovnou [yt-dlp-android 2.0.2](https://github.com/ffmpegkit-maintained/yt-dlp-android) pod MIT. Python 3.13 a Chaquopy jsou zabalené v APK, dostupné přes Java/.NET binding. **Žádný vzdálený server ani spouštění staženého EXE.**
- **Předvolby bez FFmpeg**: nejlepší společný stream, do 720p, do 480p a původní zvukový stream. Zobrazuje se procentní průběh a tlačítko **Check local yt-dlp engine** pro ověření inicializace na telefonu.
- yt-dlp stahuje do soukromé složky aplikace, dokončené soubory pak publikuje přes **MediaStore** do **Downloads/Afrowave**, bez plošného přístupu k úložišti.

**Omezení ověření:** prototyp už obsahuje skutečnou lokální knihovnu yt-dlp, ale inicializaci Pythonu, stahování z webů a předání souborů do MediaStore **ještě musíme otestovat na skutečném telefonu**. Některé weby vyžadují cookies nebo napodobení prohlížeče. Playlisty, FFmpeg, konverze, spolehlivé rušení a trvalá služba na pozadí ještě hotové nejsou.

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
- **Stahovací engine:** zvlášť Android DownloadManager pro přímé soubory a lokální Java binding zabaleného yt-dlp pro odkazy na videoportály.

Před veřejným vydáním ověříme nativní runtime i licence na skutečném telefonu. Zvolená knihovna je MIT, yt-dlp používá Unlicense, Chaquopy MIT a Python licenci PSF; viz [upstream licence](https://github.com/ffmpegkit-maintained/yt-dlp-android/blob/main/THIRD-PARTY-NOTICES.txt). Podporujeme jen arm64-v8a a x86_64. Žádné aktualizace spouštěním stažených EXE. Dále potřebujeme ověřit reálné stahování, doplnit rušení, mobilní překlady, licenčně prověřit FFmpeg a případně implementovat foreground service.

[Repozitář](https://github.com/afrowaveltd/yt-dlp-ui) · [Desktop release](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.3)
