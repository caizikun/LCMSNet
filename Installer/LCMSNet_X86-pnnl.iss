; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!
                                                           
#define MyAppVerName "4.3.0"         
#define MySource "..\LcmsNet\lcms\LCMSnet\"
#define MyLib    "..\..\lib"
#define MyPlugins "..\..\..\PluginDlls"
#define MyAppName "LCMSNet"
#define MyAppVis  "PNNL"
#define MyAppPublisher "Battelle"
#define MyAppExeName "LcmsNet.exe"  
#define MyDateTime GetDateTimeString('mm_dd_yyyy', "_","_");

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{7A59E1E4-236D-47F7-996E-F9888D99F017}
AppName={#MyAppName}
AppVerName={#MyAppVerName}
AppPublisher={#MyAppPublisher}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputDir=..\..\..\Installer\Installers\{#MyDateTime}
OutputBaseFilename={#MyAppName}_{#MyAppVis}_{#MyAppVerName}_{#MyDateTime}
SourceDir={#MySource}
Compression=lzma
SolidCompression=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Dirs]        
Name: "{userappdata}\{#MyAppName}\Log"
Name: "{userappdata}\{#MyAppName}\dmsExtensions"
Name: "{userappdata}\{#MyAppName}\SampleValidators"
Name: "{app}\LCMethods"
Name: "{app}\Plugins"
Name: "{app}\cy-GB"

[Files]
; Exe
Source: LCMSNetProg\bin\x86\PNNLRelease\LcmsNet.exe;                                                   DestDir: "{app}";          Flags: ignoreversion
; Internal Libraries

; dmstools
Source: "..\..\..\LcmsNetDmsTools\LCmsNetDmsTools\bin\x86\PNNLRelease\*.dll";        DestDir: "{userappdata}\{#MyAppName}\dmsExtensions";      Flags: ignoreversion
Source: "..\..\..\LcmsNetDmsTools\LCmsNetDmsTools\*.config";                         DestDir: "{userappdata}\{#MyAppName}\dmsExtensions";      Flags: ignoreversion

;SDK
Source: "{#MyLib}\*"; Excludes:"FluidicsPack.dll";                                   DestDir: "{app}";          Flags: ignoreversion
Source: "{#MyLib}\FluidicsPack.dll";                                                 DestDir:"{app}\Plugins";   Flags: ignoreversion

;Core sample validator    
Source: "..\..\SDK\CoreSampleValidator\bin\x86\PNNLRelease\*.dll";                                     DestDir: "{userappdata}\{#MyAppName}\SampleValidators\"; Flags:ignoreversion

;Plugins
Source: "{#MyPlugins}\*";                                                                              DestDir: "{app}\Plugins\"; Flags: ignoreversion
Source: "..\..\..\lcmsnetPlugins\PALAutoSampler\paldriv.exe";                                          DestDir: "{sys}";          Flags: ignoreversion

;PAL Validator
Source: "..\..\..\lcmsnetPlugins\PalValidator\bin\x86\PNNLRelease\*.dll";                              DestDir: "{userappdata}\{#MyAppName}\SampleValidators\"; Flags: ignoreversion

;SQLite Database Log Viewer program
Source: "..\..\..\ExternalApplications\LogViewer\bin\x86\PNNLRelease\*";                                DestDir: "{app}";          Flags: ignoreversion

; SETTINGS FILE-------------------------------------------------------------------------------------------------------------------------------------------------------------
; **WARNING**: Changing the Settings.settings file in visual studio DOES NOT change the 
; config.default! If you want to make changes to the deployed defaults, change the 
; Settings.settings file, compile, and then copy LcmsNet.exe.config into the proper 
; directory and rename it LcmsNet.exe.config.default before running this script! 
Source: "..\..\..\Installer\LcmsNet.exe.config.default";                               DestName: "LcmsNet.exe.config";    DestDir: "{app}";          Flags: ignoreversion
; END SETTINGS FILE---------------------------------------------------------------------------------------------------------------------------------------------------------


[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon
;Name: "{commondesktop}\{#MyAppName}-LogViewer"; Filename: "{app}\LogViewer.exe"; WorkingDir:"{userappdata}\LCMSNet\Log"; Tasks: desktopicon

[Run]
;Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent
Filename: "{sys}\paldriv.exe"; Flags: nowait postinstall skipifsilent

[Code]
function InitializeSetup(): Boolean;
var
    NetFrameWorkInstalled : Boolean;
begin
	NetFrameWorkInstalled := RegKeyExists(HKLM,'SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0');
	if NetFrameWorkInstalled =true then
	begin
		Result := true;
	end;

	if NetFrameWorkInstalled =false then
	begin
		MsgBox('This setup requires the .NET Framework 4.0. Please install the .NET Framework and run this setup again.',
			mbInformation, MB_OK);
		Result:=false;
	end;
end;


