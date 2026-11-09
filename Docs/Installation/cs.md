# Instalace a distribuce

[English](en.md) · [Rozcestník](../Documentation/cs.md) · [První kroky](../Getting-Started/cs.md)

**[Veřejné opravné vydání 0.1.5](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5)** už obsahuje instalační soubory i jejich SHA-256 součty. Artifacts v [GitHub Actions](https://github.com/afrowaveltd/yt-dlp-ui/actions) slouží k průběžnému vývoji; veřejné preview binárky **nejsou digitálně podepsané**. Návody na vlastní sestavení spouštěj z kořene repozitáře.

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

Z [v0.1.5](https://github.com/afrowaveltd/yt-dlp-ui/releases/tag/v0.1.5) stáhni Windows instalační EXE a spusť `yt-dlp-gui-setup-0.1.5-win-x64.exe`.

Průvodce Inno Setup:
1. **Vyžádá souhlas správce prostřednictvím UAC** a nainstaluje program pro všechny uživatele do `%ProgramFiles%\yt-dlp-gui`.
2. Vytvoří zástupce v nabídce Start a nabídne zástupce na ploše a tři **nezávislé výchozí zaškrtnuté** volby: instalovat yt-dlp / instalovat FFmpeg / instalovat Deno (pro YouTube).
3. Vybrané yt-dlp, FFmpeg a Deno stáhne přes HTTPS, ověří SHA-256 od vydavatele a nainstaluje je do `%ProgramData%\yt-dlp-ui\tools`. Následně ověří jejich spustitelnost a u Deno také minimální verzi 2.3.0.
4. Každý uživatel má dál vlastní nastavení v `%LOCALAPPDATA%\yt-dlp-ui`. **Samotné GUI a stahování** nevyžadují práva správce.
5. Pokud doplněk selže, GUI zůstává nainstalované. Diagnostika: `%ProgramData%\yt-dlp-ui\logs\dependency-install.log`. Společnou opravu proveď opakováním setupu s UAC; karta Nástroje může stále instalovat vlastní uživatelskou kopii.
6. Standardní odinstalace nesmaže společné externí programy, které mohou využívat další uživatelé.

**Přechod z v0.1.0/v0.1.1:** starší setup používal `%LOCALAPPDATA%\Programs\yt-dlp-gui`. Před novou systémovou instalací doporučujeme tuto *uživatelskou* kopii odebrat v Nastavení Windows, jinak mohou vzniknout dvojí zástupci. Osobní nastavení a dříve nainstalované externí nástroje se tím nemažou. Pokud původní aplikaci instaloval jiný uživatel, měl by ji odinstalovat on.

Deno je volitelné a stahuje se jen po výběru položky v průvodci. Nejde o součást vlastní binárky GUI. Instalátor tedy vyžaduje UAC; samotnou aplikaci však běžně nespouštěj jako správce. Nejde o MSI ani instalátor ARM64.

### Přenosná verze Windows

Soubor `yt-dlp-gui_0.1.5_win-x64_portable.zip` obsahuje kompletní složku včetně `yt-dlp-gui.exe`. Rozbal celou složku a spusť EXE; nepřesouvej samotný EXE bez knihoven.

### Sestavení ve Windows

~~~powershell
.\scripts\build-windows.ps1
.\publish\win-x64\yt-dlp-gui.exe
~~~

S nainstalovaným [Inno Setup 6](https://jrsoftware.org/isinfo.php):

~~~powershell
.\scripts\build-windows-installer.ps1
~~~

Vznikne `artifacts/yt-dlp-gui-setup-0.1.5-win-x64.exe`. Pro přenosnou verzi ARM64:

~~~powershell
.\scripts\build-windows.ps1 -Runtime win-arm64
~~~

## Debian / Ubuntu

~~~bash
./scripts/package-deb.sh
sudo apt install ./artifacts/yt-dlp-gui_0.1.5_amd64.deb
~~~

Balíček instaluje program do `/opt/yt-dlp-gui`, spouštěč `/usr/bin/yt-dlp-gui` a položku nabídky aplikací. **Doporučuje** balíčky `yt-dlp` a `ffmpeg`; podle nastavení může APT doporučené balíčky přidat. Instalační hook nestahuje žádný externí program ze sítě. **Od v0.1.5** je Deno volitelné: na kartě Nástroje → Deno → Nainstalovat Deno pro Windows / Linux stáhneš uživatelskou kopii ověřenou pomocí SHA-256 do `~/.deno/bin` (nebo `$DENO_INSTALL/bin`). Bez `sudo`, změny správce balíčků či PATH.

Odinstalace: `sudo apt remove yt-dlp-gui`.

## Fedora / openSUSE / distribuce RPM

~~~bash
./scripts/package-rpm.sh              # potřebuje rpmbuild
sudo dnf install ./artifacts/yt-dlp-gui-0.1.5-1.x86_64.rpm
~~~

Na openSUSE lze použít odpovídající `zypper install ./...rpm`. Aplikace se umístí do `/opt/yt-dlp-gui` se spouštěčem v `/usr/bin`; balíček doporučuje yt-dlp a FFmpeg. RPM se **vytváří na Ubuntu runneru**, proto před vydáním ověř instalaci také na skutečné cílové distribuci.

## Přenosný Linux a uživatelská instalace

~~~bash
./scripts/package-linux-tar.sh
tar -xzf artifacts/yt-dlp-gui_0.1.5_linux-x86_64.tar.gz
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
