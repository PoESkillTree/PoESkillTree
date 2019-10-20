; Inno Setup (5.5.3+) Script
; See http://www.jrsoftware.org/ishelp/
;
; Add languages for setup wizard to [Languages] section.
; The Inno Setup language (ISL) files can be found in Tools\isetup\Languages folder.
; If certain language cannot be found there, try looking on Internet for its ISL file. Probably someone already made one.
; Note that, ISL files with ".isl" extension use ANSI encoding in LanguageCodePage specified in file. ISL files with ".islu" extension use UTF-8 encoding.

#define AppExeName AssemblyName + ".exe"
#define AppDataFolderName ProductName
#define DistDir ProjectDir + "\dist"
#define BuildOutputDir DistDir + "\PoESkillTree"
#define PortableIniFileName "Portable.ini"

[Setup]
AppId={#ProductName}
AppMutex={#ProductName}
AppName={#ProductName}
AppVersion={#Version}
AppPublisher={#Company}
AppPublisherURL={#ProductURL}
AppSupportURL={#ProductURL}
AppUpdatesURL={#ProductURL}
AppCopyright={#Copyright}
DefaultDirName={pf}\{#ProductName}
DefaultGroupName={#ProductName}
Uninstallable=not CheckPortableMode
UninstallDisplayName={#ProductName}
UninstallDisplayIcon={app}\{#AppExeName},0
LicenseFile={#BuildOutputDir}\LICENSE.txt
OutputDir={#DistDir}
OutputBaseFilename={#ArtifactFileName}
VersionInfoVersion={#FileVersion}
SetupIconFile={#ProjectDir}\logo.ico
Compression=lzma
SolidCompression=yes
ChangesAssociations=yes

[Languages]
; Name must be set to supported culture name with all '-' characters replaced with '_'.
; Note: Name cannot contain '-' (minus) character.
Name: "en_US"; MessagesFile: "compiler:Default.isl"
Name: "sk"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "ru"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "de_DE"; MessagesFile: "compiler:Languages\German.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Check: CheckStandardMode; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Check: CheckStandardMode; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Dirs]
Name: "{userappdata}\{#ProductName}"; Check: CheckStandardMode

[Files]
; Program Files
Source: "{#BuildOutputDir}\*.exe"; DestDir: "{app}"
Source: "{#BuildOutputDir}\*.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\*.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#BuildOutputDir}\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
; Application Data (Standard mode)
Source: "{#BuildOutputDir}\Data\*"; DestDir: "{userappdata}\{#AppDataFolderName}\Data"; Check: CheckStandardMode; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\Locale\*"; DestDir: "{userappdata}\{#AppDataFolderName}\Locale"; Check: CheckStandardMode; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\PersistentData.xml"; DestDir: "{userappdata}\{#AppDataFolderName}"; Check: CheckStandardMode; AfterInstall: SetLanguage; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall
; Application Data (Portable mode)
Source: "{#BuildOutputDir}\Data\*"; DestDir: "{app}\Data"; Check: CheckPortableMode; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\Locale\*"; DestDir: "{app}\Locale"; Check: CheckPortableMode; Flags: ignoreversion recursesubdirs
Source: "{#BuildOutputDir}\PersistentData.xml"; DestDir: "{app}"; Check: CheckPortableMode; AfterInstall: SetLanguage; Flags: ignoreversion onlyifdoesntexist uninsneveruninstall

[INI]
Filename: "{app}\{#PortableIniFileName}"; Section: "Setup"; Key: "Language"; String: "{language}"; Check: CheckPortableMode

[Icons]
Name: "{group}\{#ProductName}"; Filename: "{app}\{#AppExeName}"; Check: CheckStandardMode
Name: "{group}\{cm:UninstallProgram,{#ProductName}}"; Filename: "{uninstallexe}"; Check: CheckStandardMode
Name: "{commondesktop}\{#ProductName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\{#ProductName}"; Filename: "{app}\{#AppExeName}"; Tasks: quicklaunchicon

[Run]
; Launch program checkbox
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(ProductName, '&', '&&')}}"; Flags: unchecked nowait postinstall skipifsilent
; Run application once silent update finished
Filename: "{app}\{#AppExeName}"; Flags: nowait skipifnotsilent

[Registry]
Root: HKCR; Subkey: ".pbuild";                          ValueData: "{#ProductName}";          Flags: uninsdeletevalue; ValueType: string; Check: CheckStandardMode;
Root: HKCR; Subkey: "{#ProductName}";                   ValueData: "Program {#ProductName}";  Flags: uninsdeletekey;   ValueType: string; Check: CheckStandardMode;
Root: HKCR; Subkey: "{#ProductName}\DefaultIcon";       ValueData: "{app}\{#AppExeName},0";                            ValueType: string; Check: CheckStandardMode;
Root: HKCR; Subkey: "{#ProductName}\shell\open\command";ValueData: """{app}\{#AppExeName}"" ""%1""";                   ValueType: string; Check: CheckStandardMode;

[Code]
var
	PortabilityPage: TInputOptionWizardPage;
	IsPortable: Boolean;
	IsSilent: Boolean;

function CheckPortableMode: Boolean;
begin
	Result := IsPortable;
end;

function CheckStandardMode: Boolean;
begin
	Result := not IsPortable;
end;

procedure InitializeWizard;
var 
	i: Integer;
begin
	{ Create custom page }
	PortabilityPage := CreateInputOptionPage(wpLicense, CustomMessage('InstallationTypeTitle'), CustomMessage('InstallationTypeDesc'),
											 CustomMessage('InstallationTypeLabel'), True, False);
	PortabilityPage.Add(CustomMessage('InstallationTypeStandardLabel'));
	PortabilityPage.Add(CustomMessage('InstallationTypePortableLabel'));

	{ Select standard mode, unless launched with /PORTABLE=1 argument }
	if (ExpandConstant('{param:portable|0}') = '1') then
		IsPortable := True
	else
		IsPortable := False;

	IsSilent := False;
	for i := 1 to ParamCount do
		if CompareText(ParamStr(i), '/SILENT') = 0 then
		begin
			IsSilent := True;
			Break;
		end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
	InstallLocation: String;
begin
	{ Determine portable mode }
	if (CurPageID = PortabilityPage.ID) then
	begin
		IsPortable := not (PortabilityPage.SelectedValueIndex = 0);
		{ Change DefaultDirName in wizard form }
		if IsPortable then
			WizardForm.DirEdit.Text := ExpandConstant('{sd}\{#ProductName}')
		else
			begin
				{ Use InstallLocation from registry if possible }
				if RegQueryStringValue(HKEY_LOCAL_MACHINE, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\' + ExpandConstant('{#ProductName}') + '_is1', 'InstallLocation', InstallLocation) then
					WizardForm.DirEdit.Text := RemoveBackslashUnlessRoot(InstallLocation)
				else
					WizardForm.DirEdit.Text := ExpandConstant('{pf}\{#ProductName}')
			end;
	end;
	{ Return True to continue with setup }
	Result := True; 
end;

procedure SetLanguage;
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

function ShouldSkipPage(PageID: Integer): Boolean;
begin
	Result := False;

	{ Don't ask Start Menu group name if in portable mode }
	if PageID = wpSelectProgramGroup then
	begin
		Result := IsPortable;
	end;

	if IsSilent then
	begin
		if PageID = PortabilityPage.ID then
		begin
			result := True;
		end;
	end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
	if CurUninstallStep = usPostUninstall then
	begin
		{ Ask user if program created files in AppData should be removed. }
		if MsgBox(CustomMessage('UninstallAskForPersistentDataRemove'), mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES then
		begin
			DeleteFile(ExpandConstant('{userappdata}\{#AppDataFolderName}\PersistentData.xml'))
			DeleteFile(ExpandConstant('{userappdata}\{#AppDataFolderName}\PersistentData.bak'))
			DeleteFile(ExpandConstant('{userappdata}\{#AppDataFolderName}\stash.json'))
			DelTree(ExpandConstant('{userappdata}\{#AppDataFolderName}\Builds'), True, True, True);
			DelTree(ExpandConstant('{userappdata}\{#AppDataFolderName}\Data'), True, True, True);
			DelTree(ExpandConstant('{userappdata}\{#AppDataFolderName}\Filters'), True, True, True);
			DelTree(ExpandConstant('{userappdata}\{#AppDataFolderName}\Settings'), True, True, True);
		end;
	end;
end;
