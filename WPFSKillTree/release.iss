; Inno Setup (5.5.3+) Script
; See http://www.jrsoftware.org/ishelp/

#define AppExeName AssemblyName + ".exe"
#define AppDataFolderName ProductName
#define DistDir ProjectDir + "\dist"
#define BuildOutputDir DistDir + "\PoESkillTree"

[Setup]
AppId={#AppId}
AppMutex={#AppId}
AppName={#ProductName}
AppVersion={#ProductVersion}
AppVerName={#ProductName} {#ProductVersion}
AppPublisher={#AssemblyCompany}
AppPublisherURL={#ProductURL}
AppSupportURL={#ProductURL}
AppUpdatesURL={#ProductURL}
AppCopyright={#AssemblyCopyright}
DefaultDirName={pf}\{#ProductName}
DefaultGroupName={#ProductName}
UninstallDisplayName={#ProductName}
UninstallDisplayIcon={app}\{#AppExeName},0
;InfoBeforeFile="Release-Notes.txt"
LicenseFile={#BuildOutputDir}\LICENSE.txt
OutputDir={#DistDir}
OutputBaseFilename={#PackageName}-{#ProductVersion}
VersionInfoVersion={#FileVersion}
SetupIconFile={#ProjectDir}\logo.ico
Compression=lzma
SolidCompression=yes

[Languages]
; Name must be set to supported culture name with all '-' characters replaced with '_'.
; Note: Name cannot contain '-' (minus) character.
Name: "en_US"; MessagesFile: "compiler:Default.isl"
Name: "sk"; MessagesFile: "compiler:Languages\Slovak.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Dirs]
Name: "{userappdata}\{#AppId}"

[Files]
; Program Files
Source: "{#BuildOutputDir}\*.exe"; DestDir: "{app}"
Source: "{#BuildOutputDir}\*.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
; Application Data
Source: "{#BuildOutputDir}\Data\*"; DestDir: "{userappdata}\{#AppDataFolderName}\Data"; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\Locale\*"; DestDir: "{userappdata}\{#AppDataFolderName}\Locale"; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\Items.xml"; DestDir: "{userappdata}\{#AppDataFolderName}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\PersistentData.xml"; DestDir: "{userappdata}\{#AppDataFolderName}"; AfterInstall: SetLanguage; Flags: ignoreversion

[Icons]
Name: "{group}\{#ProductName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#ProductName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#ProductName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ProductName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(ProductName, '&', '&&')}}"; Flags: unchecked nowait postinstall skipifsilent

[Code]
procedure SetLanguage();
var
    AnsiFileData: AnsiString;
	ExpandedFile: String;
	FileData: String;
	Language: String;
begin
	ExpandedFile := ExpandConstant(CurrentFileName);
	Language := ExpandConstant('{language}');
	StringChangeEx(Language, '_', '-', False);
    LoadStringFromFile(ExpandedFile, AnsiFileData);
	FileData := String(AnsiFileData);
    StringChangeEx(FileData, '%LANGUAGE%', Language, True);
    SaveStringToFile(ExpandedFile, AnsiString(FileData), False);
end;
