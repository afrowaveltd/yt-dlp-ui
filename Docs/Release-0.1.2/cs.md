# Opravné vydání v0.1.2 — instalátor Windows s UAC

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [Verzování](../Versioning/cs.md)

**[Stáhnout v0.1.2](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.2)** · [Předchozí v0.1.1](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.1)

Toto opravné vydání řeší administrátorská oprávnění Windows instalátoru a dostupnost volitelných nástrojů. **Nepřidává nové funkce stahování**; Linuxové balíčky ani vývoj Androidu neměníme.

## Co je opravené

- Windows instalátor Inno Setup nyní výslovně vyžaduje **potvrzení správce přes UAC**.
- GUI se instaluje do **Program Files pro všechny uživatele**, včetně společných zástupců v nabídce Start.
- Průvodce stále nabízí dvě nezávislé volby pro yt-dlp a FFmpeg. Vybrané soubory po ověření od vydavatele ukládá do společného **`%ProgramData%\yt-dlp-ui\tools`**, ne do osobního profilu účtu správce.
- Běžný uživatel tyto sdílené nástroje automaticky najde a může je používat bez admin práv. Nastavení a jazyky zůstávají každému zvlášť v **`%LOCALAPPDATA%\yt-dlp-ui`**.
- Instalátor kontroluje i skutečné spuštění nalezených nástrojů. Při neúspěšné výměně yt-dlp zachová původní soubor; FFmpeg/FFprobe se nejprve ověří a připraví stranou.
- Log nyní najdeš v **`%ProgramData%\yt-dlp-ui\logs\dependency-install.log`**.
- CI hlídá administrátorský režim instalátoru, syntaxi skriptu PowerShell i sestavení Windows/Linux.

## Přechod z 0.1.0 a 0.1.1

Starší Windows instalátory ukládaly GUI pouze pro jednoho uživatele do `%LOCALAPPDATA%\Programs\yt-dlp-gui`. Nová verze se instaluje **pro všechny účty**. Původní aplikaci proto nejdřív odinstaluj prostřednictvím Nastavení Windows, aby nevznikly dvojí zástupci. Osobní nastavení a dříve nainstalované uživatelské nástroje zůstanou zachované.

Nový setup může přes UAC žádat heslo správce. Platí to **jen pro instalaci**, nikoli pro běžné používání a stahování. Na kartě Nástroje zůstává možnost spravovat vlastní uživatelské kopie.

Sdílené externí nástroje zůstanou po odinstalaci GUI zachované, protože je mohou používat další účty. Pokud už nejsou potřebné, může je správce odstranit ručně.

## Testování a omezení

Workflow vytvoří a otestuje Windows a Linux sestavení, připraví setup/portable ZIP, DEB/RPM/tar.gz a soubor `SHA256SUMS.txt`. Automatické testy kontrolují nastavení instalátoru a syntaxi PowerShellu, ale **nenahradí reálný dialog UAC a skutečnou instalaci na uživatelském Windows PC**. Tento scénář ještě potřebuje prakticky ověřit.

Zůstává označení **pre-release** a licence MIT pro samotné GUI; yt-dlp a FFmpeg mají vlastní licence. Nepodepsané soubory mohou zobrazovat bezpečnostní upozornění Windows. Android není součástí v0.1.2.

[Hlášení chyb](https://github.com/afrowaveltd/yt-dlp-ui/issues) uvítáme s verzí Windows, očištěným instalačním logem a postupem reprodukce.
