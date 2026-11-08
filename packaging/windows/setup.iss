; Machine-wide installer for yt-dlp GUI, with UAC elevation. Compiled by scripts/build-windows-installer.ps1.
#ifndef AppVersion
  #define AppVersion "0.1.2"
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
DefaultDirName={autopf}\yt-dlp-gui
UsePreviousAppDir=no
DefaultGroupName=yt-dlp GUI
PrivilegesRequired=admin
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
english.OptionalTools=Optional shared tools (installed for all users if selected)
czech.OptionalTools=Volitelné společné nástroje (při zaškrtnutí pro všechny uživatele)
english.InstallYtDlp=Install yt-dlp (required to download videos)
czech.InstallYtDlp=Nainstalovat yt-dlp (nutný pro stahování videí)
english.InstallFfmpeg=Install FFmpeg (recommended for merging and converting)
czech.InstallFfmpeg=Nainstalovat FFmpeg (doporučený pro spojování a převod)
english.CreateDesktopShortcut=Create a desktop shortcut
czech.CreateDesktopShortcut=Vytvořit zástupce na ploše
english.ToolInstallFailed=Could not install %1 for all users. See the installation log under ProgramData\yt-dlp-ui\logs. You can retry with administrator privileges.
czech.ToolInstallFailed=Instalace nástroje %1 pro všechny uživatele se nezdařila. Podrobnosti jsou v ProgramData\yt-dlp-ui\logs. Zkuste instalátor spustit znovu jako správce.
english.DependencyExplanation=Setup requires administrator approval. The selected tools will be verified using SHA-256 and installed for all Windows users. Personal app settings remain per user. Details: README / Tools tab.
czech.DependencyExplanation=Instalátor vyžaduje oprávnění správce. Vybrané nástroje se ověří pomocí SHA-256 a nainstalují pro všechny uživatele. Nastavení aplikace zůstává osobní.
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
Source: "{#RepoRoot}\README.md"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\yt-dlp GUI"; Filename: "{app}\yt-dlp-gui.exe"
Name: "{autodesktop}\yt-dlp GUI"; Filename: "{app}\yt-dlp-gui.exe"; Tasks: desktopIcon

[Run]
Filename: "{app}\yt-dlp-gui.exe"; Description: "{cm:LaunchProgram,yt-dlp GUI}"; Flags: nowait postinstall skipifsilent runasoriginaluser

[Code]
procedure InitializeWizard();
var
  HelpText: TNewStaticText;
begin
  WizardForm.TasksList.Height := WizardForm.TasksList.Height - ScaleY(64);
  HelpText := TNewStaticText.Create(WizardForm);
  HelpText.Parent := WizardForm.TasksList.Parent;
  HelpText.Left := WizardForm.TasksList.Left;
  HelpText.Top := WizardForm.TasksList.Top + WizardForm.TasksList.Height + ScaleY(10);
  HelpText.Width := WizardForm.TasksList.Width;
  HelpText.Height := ScaleY(54);
  HelpText.WordWrap := True;
  HelpText.Caption := CustomMessage('DependencyExplanation');
end;

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
