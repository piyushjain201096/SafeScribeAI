; Inno Setup Script for SafeScribe AI (Windows .NET 10 Avalonia Desktop App)
#define MyAppName "SafeScribe AI"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "SafeScribe AI"
#define MyAppURL "https://github.com/piyushjain201096/SafeScribeAI"
#define MyAppExeName "SafeScribe.UI.Avalonia.exe"

[Setup]
AppId={{D37E88F0-45A1-4A59-8692-0A1D9C5E4821}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\SafeScribe AI
DefaultGroupName={#MyAppName}
AllowNoGroup=yes
OutputDir=..\dist
OutputBaseFilename=SafeScribeAI-Setup-v{#MyAppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#MyAppExeName}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent
