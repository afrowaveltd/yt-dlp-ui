# Instalace a distribuce

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [První kroky](../Getting-Started/cs.md)

Připravujeme **první vydání 0.1.0**. Dokud není zveřejněný GitHub Release, jsou soubory v [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) **testovací artifacts**, nikoliv podepsané finální balíčky. Návody na vlastní sestavení spouštěj z kořene repozitáře.

## Podporované platformy

| Platforma | Formát | Aktuální ověření |
|---|---|---|
| Windows 10/11 x64 | Setup.exe (Inno Setup) | V CI sestaveno a zveřejněno; interaktivní stahování závislostí je třeba ověřit ručně |
| Windows 10/11 x64 | Přenosná složka | V CI vytvořeno a zveřejněno |
| Windows ARM64 | Přenosná složka | Existuje `build-windows.ps1 -Runtime win-arm64`; aktuální CI ji nesestavuje |
| Debian / Ubuntu x64 | DEB | V CI vytvořeno a prověřena struktura |
| Fedora / openSUSE a podobné x64 | RPM | V CI vytvořeno a prověřena struktura; na cílových distribucích neinstalováno |
| Ostatní Linux x64 | Přenosný tar.gz | V CI vytvořeno a prověřen obsah |
| Linux arm64 | Skripty DEB/RPM/tar | Podpora ve skriptech; CI prověřuje jen x64 |
| macOS | Zdrojové kódy / ruční publish | Bez ověřeného instalátoru a vydaného balíčku |

### Úloha externích programů

- `yt-dlp` skutečně stahuje média; GUI bez něj nestáhne video.
- `ffmpeg` a obvykle `ffprobe` jsou potřeba ke spojování streamů a mnoha převodům.
- GUI tyto binárky **nezabaluje** do DEB/RPM/Windows Setup ani portable archivu. Platí samostatné licence externích projektů.
- Nástroje může zajistit systém nebo vlastní cesta na kartě **Nástroje**.

## Windows x64: instalační průvodce

Z příslušného [CI běhu](https://github.com/afrowaveltd/yt-dlp-ui/actions) stáhni **`yt-dlp-gui-windows-setup`**, rozbal ZIP a spusť `yt-dlp-gui-setup-0.1.0-win-x64.exe`.

Průvodce Inno Setup:
1. Instaluje pro aktuálního uživatele do `%LOCALAPPDATA%\Programs\yt-dlp-gui` bez požadavku na UAC.
2. Vytvoří položku v nabídce Start a nabídne volitelného zástupce na ploše.
3. Zobrazí dvě **nezávislé, výchozí zaškrtnuté** volby **Nainstalovat yt-dlp** a **Nainstalovat FFmpeg**. Každou lze vypnout.
4. Vybrané chybějící nástroje stáhne od vydavatele přes HTTPS a ověří zveřejněný kontrolní součet SHA-256. Je nutný internet.
5. Pokud stažení závislosti selže, GUI zůstane nainstalované a uživatel dostane upozornění. Pokus lze opakovat na kartě **Nástroje**.
6. Nabízí standardní odinstalaci. Vlastní externí nástroje v `%LOCALAPPDATA%\yt-dlp-ui\tools` po odinstalaci GUI zůstávají, aby nebyla omylem smazána samostatně využívaná data.

Log instalace volitelných nástrojů: `%LOCALAPPDATA%\yt-dlp-ui\logs\dependency-install.log`. Nejde o firemní MSI ani instalátor pro Windows ARM64.

### Přenosná verze Windows

Artifact `yt-dlp-gui-windows-x64` obsahuje kompletní složku včetně `yt-dlp-gui.exe`. Rozbal celou složku a spusť EXE; nepřesouvej samotný EXE bez knihoven.

### Sestavení ve Windows

~~~powershell
.\scripts\build-windows.ps1
.\publish\win-x64\yt-dlp-gui.exe
~~~

S nainstalovaným [Inno Setup 6](https://jrsoftware.org/isinfo.php):

~~~powershell
.\scripts\build-windows-installer.ps1
~~~

Vznikne `artifacts/yt-dlp-gui-setup-0.1.0-win-x64.exe`. Pro přenosnou verzi ARM64:

~~~powershell
.\scripts\build-windows.ps1 -Runtime win-arm64
~~~

## Debian / Ubuntu

~~~bash
./scripts/package-deb.sh
sudo apt install ./artifacts/yt-dlp-gui_0.1.0_amd64.deb
~~~

Balíček instaluje program do `/opt/yt-dlp-gui`, spouštěč `/usr/bin/yt-dlp-gui` a položku nabídky aplikací. **Doporučuje** balíčky `yt-dlp` a `ffmpeg`; podle nastavení může APT doporučené balíčky přidat. Instalační hook nestahuje žádný externí program ze sítě.

Odinstalace: `sudo apt remove yt-dlp-gui`.

## Fedora / openSUSE / distribuce RPM

~~~bash
./scripts/package-rpm.sh              # potřebuje rpmbuild
sudo dnf install ./artifacts/yt-dlp-gui-0.1.0-1.x86_64.rpm
~~~

Na openSUSE lze použít odpovídající `zypper install ./...rpm`. Aplikace se umístí do `/opt/yt-dlp-gui` se spouštěčem v `/usr/bin`; balíček doporučuje yt-dlp a FFmpeg. RPM se **vytváří na Ubuntu runneru**, proto před vydáním ověř instalaci také na skutečné cílové distribuci.

## Přenosný Linux a uživatelská instalace

~~~bash
./scripts/package-linux-tar.sh
tar -xzf artifacts/yt-dlp-gui_0.1.0_linux-x86_64.tar.gz
./yt-dlp-gui/yt-dlp-gui
~~~

Archiv obsahuje runtime, ale cílový systém stále potřebuje běžné nativní a grafické knihovny. Systémové balíčky se nemění.

Pro instalaci ze zdrojových kódů do `~/.local/`:

~~~bash
./scripts/install.sh
yt-dlp-gui
./scripts/install.sh --uninstall
~~~

Instalace vytvoří `~/.local/opt/yt-dlp-gui`, uživatelský spouštěč, ikonu a položku v menu. Ujisti se, že `~/.local/bin` je v PATH. Tato cesta není totéž co systémová instalace DEB/RPM.

## Co potřebuje vývojář

- **.NET 10 SDK** odpovídající `global.json`.
- Pro DEB `dpkg-deb`, pro RPM `rpmbuild`, pro archiv `tar`.
- Windows instalátor potřebuje Inno Setup 6 a PowerShell.
- Pro běh GUI mohou být potřeba nativní grafické knihovny závislé na desktopovém prostředí.

Skripty odvozují verzi z `Directory.Build.props`. Dokumentace ani soubory ke stažení proto nesmí uvádět jiné číslo verze.

[Ověřování](../Development-and-Verification/cs.md) · [Řešení problémů](../Troubleshooting/cs.md)
