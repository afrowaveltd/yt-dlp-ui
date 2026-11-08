; Per-user installer for yt-dlp GUI. Compiled by scripts/build-windows-installer.ps1.
#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif
#define RepoRoot AddBackslash(SourcePath) + "..\.."

[Setup]
AppId={{9B30F75E-6224-4E18-87C0-7A03BFC1B68D}
AppName=yt-dlp GUI
AppVersion={#AppVersion}
AppVerName=yt-dlp GUI {#AppVersion}
AppPublisher=Afrowave contributors
AppPublisherURL=https://github.com/afrowaveltd/yt-dlp-ui
AppSupportURL=https://github.com/afrowaveltd/yt-dlp-ui/issues
AppUpdatesURL=https://github.com/afrowaveltd/yt-dlp-ui/releases
DefaultDirName={localappdata}\Programs\yt-dlp-gui
DefaultGroupName=yt-dlp GUI
PrivilegesRequired=lowest
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
MinVersion=10.0
SetupIconFile={#RepoRoot}\src\YtDlpUi\Assets\yt-dlp-gui.ico
UninstallDisplayIcon={app}\yt-dlp-gui.exe
OutputDir={#RepoRoot}\artifacts
OutputBaseFilename=yt-dlp-gui-setup-{#AppVersion}-win-x64
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"

[CustomMessages]
english.OptionalTools=Optional tools (installed only if selected)
czech.OptionalTools=Volitelné nástroje (instalují se jen při zaškrtnutí)
english.InstallYtDlp=Install yt-dlp (required to download videos)
czech.InstallYtDlp=Nainstalovat yt-dlp (nutný pro stahování videí)
english.InstallFfmpeg=Install FFmpeg (recommended for merging and converting)
czech.InstallFfmpeg=Nainstalovat FFmpeg (doporučený pro spojování a převod)
english.CreateDesktopShortcut=Create a desktop shortcut
czech.CreateDesktopShortcut=Vytvořit zástupce na ploše
english.ToolInstallFailed=Could not install %1. Please open the Tools tab in yt-dlp GUI to retry. Details are recorded in the application data folder.
czech.ToolInstallFailed=Instalace nástroje %1 se nezdařila. Zkuste ji zopakovat na kartě Nástroje. Záznam je v uživatelských datech aplikace.
english.ToolStatus=Installing optional tool: %1
czech.ToolStatus=Instaluje se volitelný nástroj: %1

[Tasks]
Name: "installYtDlp"; Description: "{cm:InstallYtDlp}"; GroupDescription: "{cm:OptionalTools}"; Flags: checkedonce
Name: "installFfmpeg"; Description: "{cm:InstallFfmpeg}"; GroupDescription: "{cm:OptionalTools}"; Flags: checkedonce
Name: "desktopIcon"; Description: "{cm:CreateDesktopShortcut}"; Flags: unchecked

[Files]
Source: "{#RepoRoot}\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#RepoRoot}\packaging\windows\install-dependencies.ps1"; DestDir: "{app}\support"; Flags: ignoreversion
Source: "{#RepoRoot}\LICENSE"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\yt-dlp GUI"; Filename: "{app}\yt-dlp-gui.exe"
Name: "{autodesktop}\yt-dlp GUI"; Filename: "{app}\yt-dlp-gui.exe"; Tasks: desktopIcon

[Run]
Filename: "{app}\yt-dlp-gui.exe"; Description: "{cm:LaunchProgram,yt-dlp GUI}"; Flags: nowait postinstall skipifsilent

[Code]
procedure InstallOptionalTool(Tool, Display: string);
var
  PowerShellExe: string;
  Params: string;
  ExitCode: Integer;
  Started: Boolean;
begin
  PowerShellExe := ExpandConstant('{sys}\WindowsPowerShell\v1.0\powershell.exe');
  Params := '-NoProfile -NonInteractive -ExecutionPolicy Bypass -File "' +
    ExpandConstant('{app}\support\install-dependencies.ps1') + '" -Tool ' + Tool;
  WizardForm.StatusLabel.Caption := FmtMessage(CustomMessage('ToolStatus'), [Display]);
  WizardForm.Update();
  Log('Installing optional dependency: ' + Tool);
  ExitCode := -1;
  Started := Exec(PowerShellExe, Params, '', SW_HIDE, ewWaitUntilTerminated, ExitCode);
  if (not Started) or (ExitCode <> 0) then
  begin
    Log('Dependency installation failed: ' + Tool + ' (code ' + IntToStr(ExitCode) + ')');
    MsgBox(FmtMessage(CustomMessage('ToolInstallFailed'), [Display]), mbInformation, MB_OK);
  end;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if WizardIsTaskSelected('installYtDlp') then
      InstallOptionalTool('yt-dlp', 'yt-dlp');
    if WizardIsTaskSelected('installFfmpeg') then
      InstallOptionalTool('ffmpeg', 'FFmpeg');
  end;
end;
