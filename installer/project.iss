#include "isxdl.iss"

[Setup]
AppName=Smile!
AppID={code:GetAppID}
AppVerName=Smile! v{code:GetAppVer}
AppVersion={code:GetAppVer}
AppPublisher=Kudlacz
AppPublisherURL=http://www.kudlacz.com
AppSupportURL=http://www.kudlacz.com/?/section/contact
AppUpdatesURL=http://www.kudlacz.com/?/section/projects
DefaultDirName={pf}\Kudlacz\Smile!
DefaultGroupName=Kudlacz\Smile!
AllowNoIcons=yes
InfoBeforeFile=install info.txt
OutputDir=compiled
OutputBaseFilename=Smile! v{code:GetAppVer} Installer
Compression=lzma/ultra
InternalCompressLevel=ultra
SolidCompression=yes
LicenseFile=source\LICENSE.TXT
UninstallDisplayIcon={app}\smiletray.exe
AppModifyPath="{app}\UninsHs.exe" /m0=Smile!
PrivilegesRequired=admin
DisableStartupPrompt=true

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"

[_ISTool]
EnableISX=true

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "source\smiletray.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "source\LICENSE.TXT"; DestDir: "{app}";  Flags: isreadme
Source: "source\ReadMe.txt"; DestDir: "{app}";  Flags: isreadme
Source: "UninsHs.exe"; DestDir: "{app}"; Flags: restartreplace
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[INI]
Filename: "{app}\Kudlacz.url"; Section: "InternetShortcut"; Key: "URL"; String: "http://www.Kudlacz.com"

[Icons]
Name: "{group}\Smile!"; Filename: "{app}\smiletray.exe"
Name: "{group}\Visit Kudlacz.com"; Filename: "{app}\Kudlacz.url"
Name: "{group}\View README"; Filename: "{app}\README.txt"
Name: "{group}\View LICENSE"; Filename: "{app}\LICENSE.txt"
Name: "{group}\Uninstall Smile!"; Filename: "{app}\UninsHs.exe"; Parameters: /u0={code:GetAppID}
Name: "{userdesktop}\Smile!"; Filename: "{app}\smiletray.exe"; Tasks: desktopicon
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\Smile!"; Filename: "{app}\smiletray.exe"; Tasks: quicklaunchicon

[Run]
Filename: "{app}\smiletray.exe"; Description: "{cm:LaunchProgram,smile}"; Flags: nowait postinstall skipifsilent
Filename: {app}\UninsHs.exe; Parameters: /r={code:GetAppID},{language},{srcexe},{app}\setup.exe; Flags: runminimized runhidden nowait
Filename: {ini:{tmp}\dep.ini,install,sp6a}; Parameters: /U /X:{tmp}\sp6a; Description: NT4 SP6a Extract; StatusMsg: Extracting NT4 Service Pack 6a Files...; Flags: skipifdoesntexist
Filename: {tmp}\sp6a\update\update.exe; Parameters: -u -z; Description: NT4 SP6a Install; StatusMsg: Installing NT4 Service Pack 6a...; Flags: skipifdoesntexist
Filename: {ini:{tmp}\dep.ini,install,ie}; Description: IE 6; StatusMsg: Installing Internet Explorer 6... (This may take a few minutes); Flags: skipifdoesntexist; Parameters: "/Q /C:""ie5wzd /QU /R:N /S:#e"""
Filename: {ini:{tmp}\dep.ini,install,mdac}; Parameters: "/Q /C:""setup /QNT"""; Description: MDAC 2.7; StatusMsg: Installing Microsoft Data Access Components 2.7... (This may take a few minutes); Flags: skipifdoesntexist
Filename: {ini:{tmp}\dep.ini,install,jet}; Description: JET 4; StatusMsg: Installing Jet 4.0 Database Components...; Flags: skipifdoesntexist
Filename: {ini:{tmp}\dep.ini,install,dotnetfx}; Parameters: /Q /T:{tmp}\dotnetfx; Description: .NET Extract; Flags: skipifdoesntexist; StatusMsg: Extracting Microsoft .NET...
Filename: {tmp}\dotnetfx\dotnetfx.exe; Parameters: "/Q /C:""install /q"""; Description: .NET Install; StatusMsg: Installing Microsoft .NET... (This may take a few minutes); Flags: skipifdoesntexist

[UninstallDelete]
Type: files; Name: "{app}\kudlacz.url"
Type: files; Name: "{app}\setup.exe"

[Code]
var
  iePath, sp6aPath, mdacPath, jetPath, dotnetfxPath: string;
  downloadNeeded: boolean;
  exclusiveNeeded: boolean;
  // SP6a must be installed exclusively or IE 6 install will fail.
  // IE 6 must be installed exclusively or netfx will error with: "urlmon.dll entry point error"

  memoDependenciesNeeded: string;

const
  appVer = '1.9';
  appID = 'smiletray-{F1D19AEB-8EF6-41b9-AD38-3A84AE64AF53}';
  ieURL = 'http://download.microsoft.com/download/ie6sp1/finrel/6_sp1/W98NT42KMeXP/EN-US/ie6setup.exe';
  sp6aURL = 'http://download.microsoft.com/download/winntsp/SP/6.0a-128/NT4/EN-US/sp6i386.exe';
  mdacURL = 'http://download.microsoft.com/download/MDAC26/Refresh/2.0/W98NT42KMeXP/EN-US/MDAC_TYP.EXE';
  jetURL = 'http://www.ase-systems.com/downloads/jet40sp3_comp.exe';
  dotnetfxx86URL = 'http://download.microsoft.com/download/5/6/7/567758a3-759e-473e-bf8f-52154438565a/dotnetfx.exe';
  dotnetfxx64URL = 'http://download.microsoft.com/download/a/3/f/a3f1bf98-18f3-4036-9b68-8e6de530ce0a/NetFx64.exe';
  dotnetfxIA64URL = 'http://download.microsoft.com/download/f/8/6/f86148a4-e8f7-4d08-a484-b4107f238728/NetFx64.exe';
  WM_LBUTTONDOWN = 513;
  WM_LBUTTONUP = 514;
  
function GetAppVer(Param: String): String;
begin
 Result := appVer;
end;

function GetAppID(Param: String): String;
begin
 Result := appID;
end;

function GetInstalledVersion(Ver: String): Boolean;
begin
  Ver := '';
  Result := True;
  if not RegQueryStringValue(HKLM,
    'Software\Microsoft\Windows\CurrentVersion\Uninstall\'+{code:GetAppID}+'_is1',
    'DisplayVersion', Ver) then
    if not RegQueryStringValue(HKCU, 'Software\Microsoft\Windows\CurrentVersion\Uninstall\'+{code:GetAppID}+'_is1' ,
      'DisplayVersion', Ver) then
        Result := False;
end;

function ChrCount(const substr: Char; const S: String): Integer;
var
  i: Integer;
begin
  i := 1;
  Result := 0;
  while i <= Length(S) do begin
    if S[i] = substr then
      Result := Result + 1;
    i := i + CharLength(S, i);
  end;
end;

// Return 1 - Left > Right
// Return 0 - Left = Right
// Return -1 - Left < Right
function CompareVersions(const Left, Right: String): Integer;
var
   len,i,pl,pr,l,r,n: Integer;
begin
  Result := 0;
  i := 1;
  pl := 1;
  pr := 1;

  n := ChrCount ('.', Right);
  len := ChrCount('.', Left);
  if n > len then
    len := n;
  while i <= len do
  begin
    if i > Length(Left) then begin
      l := 0;
    end else begin
      n := Pos('.', Copy(Left, pl, Length(Left)));
      if n = 0 then
        n := Length(Left)-pl;
      l := StrToIntDef(Copy(Left, pl, n+1), 0);
      pl := n+1;
    end;
    
    if i > Length(Right) then begin
      r := 0;
    end else begin
      n := Pos('.', Copy(Right, pr, Length(Right)));
      if n = 0 then
        n := Length(Right)-pr;
      r := StrToIntDef(Copy(Right, pr, n+1), 0);
      pr := n+1;
    end;
    
    if l > r then begin
      Result := 1;
    end else if l < r then begin
      Result := -1;
    end else begin
      Result := 0;
    end;
      
    // Go to the next character. But do not simply increment I by 1.
    // Increment by CharLength() in case Result[I] is a double-byte character.
    i := i + 1;
  end;

end;

procedure InitializeWizard();
begin
  if (Pos('/SP-', UpperCase(GetCmdTail)) > 0) then
  begin
    PostMessage(WizardForm.NextButton.Handle,WM_LBUTTONDOWN,0,0);
    PostMessage(WizardForm.NextButton.Handle,WM_LBUTTONUP,0,0);
  end;
end;

procedure CurPageChanged(CurPageID: Integer);
begin
  if (Pos('/SP-', UpperCase(GetCmdTail)) > 0) and
     (CurPageID = wpSelectComponents) then
    WizardForm.BackButton.Visible := False;
end;

function InitializeSetup(): Boolean;
var
  sRet: string;
  nRet: Integer;

begin
  Result := true;

  // Check for required SP6a installation
  RegQueryStringValue(HKLM, 'Software\Microsoft\Windows NT\CurrentVersion', 'CurrentVersion', sRet);
  if sRet < '5' then begin
    if not RegKeyExists(HKLM, 'Software\Microsoft\Windows NT\CurrentVersion\Hotfix\Q246009') then begin
      MsgBox('Windows NT4 Service Pack 6a must be installed before Setup can install the application.  Setup will attempt to install SP6a for you.  After the computer reboots, please run Setup again.', mbInformation, MB_OK);
      exclusiveNeeded := true;
      memoDependenciesNeeded := memoDependenciesNeeded + '      NT4 Service Pack 6a' #13;
      sp6aPath := '\dependencies\sp6i386.exe';
      if not FileExists(sp6aPath) then begin
        sp6aPath := ExpandConstant('{tmp}\sp6i386.exe');
        if not FileExists(sp6aPath) then begin
          isxdl_AddFile(sp6aURL, sp6aPath);
          downloadNeeded := true;
        end;
      end;
    end;
    SetIniString('install', 'sp6a', sp6aPath, ExpandConstant('{tmp}\dep.ini'));
  end;

  // Check for required IE installation
  // Note that if IE 6 is downloaded, the express setup will be downloaded, however it is the same
  // ie6setup.exe that would be available in the ie5full folder.  The only difference is that the
  // user will be presented with an option as to where to download IE 6 and a progress dialog.
  // Most common components will still be installed automatically.
  sRet := '';
  RegQueryStringValue(HKLM, 'Software\Microsoft\Internet Explorer', 'Version', sRet);
  if (not exclusiveNeeded) and (sRet < '6') then begin
    exclusiveNeeded := true;
    MsgBox('Internet Explorer 6 must be installed before Setup can install the application.  Setup will attempt to install IE 6 for you.  After the computer reboots, please run Setup again.', mbInformation, MB_OK);
    memoDependenciesNeeded := memoDependenciesNeeded + '      Internet Explorer 6' #13;
    iePath := '\dependencies\ie5full\ie6setup.exe';
    if not FileExists(iePath) then begin
      iePath := ExpandConstant('{tmp}\ie6setup.exe');
      if not FileExists(iePath) then begin
        isxdl_AddFile(ieURL, iePath);
        downloadNeeded := true;
      end;
    end;
    SetIniString('install', 'ie', iePath, ExpandConstant('{tmp}\dep.ini'));
  end;

  // Check for required MDAC installation
  sRet := '';
  RegQueryStringValue(HKLM, 'Software\Microsoft\DataAccess', 'FullInstallVer', sRet);
  if (not exclusiveNeeded) and (sRet < '2.7') then begin
    memoDependenciesNeeded := memoDependenciesNeeded + '      Microsoft Data Access Components 2.7' #13;
    mdacPath := '\dependencies\MDAC_TYP.EXE';
    if not FileExists(mdacPath) then begin
      mdacPath := ExpandConstant('{tmp}\MDAC_TYP.EXE');
      if not FileExists(mdacPath) then begin
        isxdl_AddFile(mdacURL, mdacPath);
        downloadNeeded := true;
      end;
    end;
    SetIniString('install', 'mdac', mdacPath, ExpandConstant('{tmp}\dep.ini'));
  end;

  // Check for required Jet installation
  // Jet is not included in MDAC 2.5+.  It will be needed on NT4 installations.
  if (not exclusiveNeeded) and (not RegKeyExists(HKLM, 'Software\Microsoft\Jet\4.0')) then begin
    memoDependenciesNeeded := memoDependenciesNeeded + '      Jet 4.0 Database Components' #13;
    jetPath := '\dependencies\Jet40Sp3_Comp.exe';
    if not FileExists(jetPath) then begin
      jetPath := ExpandConstant('{tmp}\Jet40Sp3_Comp.exe');
      if not FileExists(jetPath) then begin
        isxdl_AddFile(jetURL, jetPath);
        downloadNeeded := true;
      end;
    end;
    SetIniString('install', 'jet', jetPath, ExpandConstant('{tmp}\dep.ini'));
  end;

  // Check for required netfx installation
  if (not exclusiveNeeded) and (not RegKeyExists(HKLM, 'Software\Microsoft\.NETFramework\policy\v2.0')) then begin
    memoDependenciesNeeded := memoDependenciesNeeded + '      .NET Framework' #13;
    dotnetfxPath := '\dependencies\dotnetfx.exe';
    if not FileExists(dotnetfxPath) then begin
      dotnetfxPath := ExpandConstant('{tmp}\dotnetfx.exe');
      if not FileExists(dotnetfxPath) then begin
		    case ProcessorArchitecture of
			     paX86: isxdl_AddFile(dotnetfxx86URL, dotnetfxPath);
			     paX64: isxdl_AddFile(dotnetfxx64URL, dotnetfxPath);
			     paIA64: isxdl_AddFile(dotnetfxIA64URL, dotnetfxPath);
		    else
			     isxdl_AddFile(dotnetfxx86URL, dotnetfxPath);
		    end;
        downloadNeeded := true;
      end;
    end;
    SetIniString('install', 'dotnetfx', dotnetfxPath, ExpandConstant('{tmp}\dep.ini'));
  end;

  sRet := '';
  RegQueryStringValue(HKLM, 'Software\Microsoft\Internet Explorer', 'Version', sRet);
  if downloadNeeded and (sRet < '3') then begin
    // Downloads are needed and isxdl can't initialize to download them, abort
    Result := false;
    if FileExists(iePath) then begin
      MsgBox('Internet Explorer 6 (or later) must be installed manually before Setup can continue.' #13#13 'Setup will now exit and begin the IE 6 installation.', mbInformation, MB_OK);
      Exec(iePath, '', '', SW_SHOWNORMAL, ewNoWait, nRet);
    end else begin
      if MsgBox('Internet Explorer 6 (or later) must be installed manually before Setup can continue.' #13#13 'Would you like to visit the download site now?', mbConfirmation, MB_OKCANCEL) = IDOK then
        ShellExec('open', 'http://www.microsoft.com/windows/ie/ie6/downloads/critical/ie6sp1/default.mspx', '', '', SW_SHOWNORMAL, ewNoWait, nRet);
    end;
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  hWnd, nRet: Integer;
  path, dir, Ver: String;

begin
  Result := true;

  if CurPageID = wpSelectDir then begin
    if GetInstalledVersion(Ver) then begin
      nRet := CompareVersions(Ver, ''+{code:GetAppVer}+'');
      if nRet = -1 then begin
        // Do Update
        if MsgBox('A previous version of Smile! (' + Ver + ') is already installed. Would you like to Upgrade? Yes to continue, No to exit this setup application', mbConfirmation, MB_YESNO) = IDYES then begin
          dir := ExpandConstant('{app}');
          path := ExpandConstant('{uninstallexe}');
          MsgBox(path, mbInformation, MB_OK);
          if FileExists(path) and Exec(path, '', dir, SW_SHOWNORMAL, ewWaitUntilTerminated, nRet) then begin
            Exec(path, '/SILENT /SUPPRESSMSGBOXES', dir, SW_SHOWNORMAL, ewWaitUntilTerminated, nRet);
          end else begin
            Result := False;
          end;
        end;
      end else if nRet = 1 then begin
        // Do Downgrade
        if MsgBox('A previous version of Smile! (' + Ver + ') is already installed. This is newer than is available with this installer. Are you sure you want to install an older version? Yes to continue, No to exit this setup application', mbConfirmation, MB_YESNO) = IDYES then begin
        dir := ExpandConstant('{app}');
        path := ExpandConstant('{uninstallexe}');
        MsgBox(path, mbInformation, MB_OK);
        if FileExists(path) and Exec(path, '', dir, SW_SHOWNORMAL, ewWaitUntilTerminated, nRet) then begin
           Exec(path, '/SILENT /SUPPRESSMSGBOXES', dir, SW_SHOWNORMAL, ewWaitUntilTerminated, nRet);
        end else begin
           Result := False;
        end;
      end;
    end;
  end else if CurPageID = wpReady then begin
    hWnd := StrToInt(ExpandConstant('{wizardhwnd}'));

    // don't try to init isxdl if it's not needed because it will error on < ie 3
    if downloadNeeded then
      if isxdl_DownloadFiles(hWnd) = 0 then Result := false;
    end;
  end;
end;

function ShouldSkipPage(CurPage: Integer): Boolean;
begin
   if Pos('/SP-', UpperCase(GetCmdTail)) > 0 then
    case CurPage of
      wpLicense, wpPassword, wpInfoBefore, wpUserInfo,
      wpSelectDir, wpSelectProgramGroup, wpInfoAfter:
        Result := True;
    end;
  if (CurPage = wpSelectDir) and exclusiveNeeded then Result := true;
end;

function UpdateReadyMemo(Space, NewLine, MemoUserInfoInfo, MemoDirInfo, MemoTypeInfo, MemoComponentsInfo, MemoGroupInfo, MemoTasksInfo: String): String;
var
  s: string;

begin
  if memoDependenciesNeeded <> '' then s := s + 'Dependencies to install:' + NewLine + memoDependenciesNeeded + NewLine;
  s := s + MemoDirInfo + NewLine + NewLine;

  Result := s
end;


