; Pictures.nsi
; NSIS installation script for the Pictures Screensaver

; The name of the installer
Name "Pictures Screensaver"

; The file to write
OutFile "PicturesInstaller.exe"

; The default installation directory
InstallDir "$WINDIR\system32"

; Request application privileges for Windows Vista
RequestExecutionLevel admin

; Installation
Section "Install"

  ; Set output path to the installation directory.
  SetOutPath "$WINDIR\system32"
  
  ; Put file there
  File "/oname=Pictures.scr" "Pictures\bin\Release\Pictures.exe"
  File "/oname=sqlite3.dll" "Pictures\bin\Release\sqlite3.dll"
  
  ; Uninstall stuff
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "DisplayName" "Pictures Screensaver"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "UninstallString" "$WINDIR\system32\PicturesUninstall.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "Publisher" "gdroud@gmail.com"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "DisplayVersion" "1.0.0"
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver" "NoRepair" 1
  WriteUninstaller "PicturesUninstall.exe"

  ; Set the active screensaver - BREAKS CONTROL PANEL
  ;WriteRegStr HKCU "Control Panel\desktop" "SCRNSAVE.EXE" "Pictures.scr"
  ;WriteRegStr HKCU "Control Panel\desktop" "ScreenSaveActive" "1"
  ;System::Call 'user32.dll::SystemParametersInfo(17, 1, 0, 2)'
  
  ; Start configuration - HANDLED BY CONTROL PANEL
  ;Exec "$WINDIR\system32\Pictures.scr /c"
  
  ; Open control panel so they can set screen saver
  Exec "RunDll32.exe shell32.dll,Control_RunDLL desk.cpl,,1"
SectionEnd

; Uninstallation
Section "Uninstall"
  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\PicturesScreensaver"
  DeleteRegKey HKCU "Software\droud\Pictures"

  ; Remove files and uninstaller
  Delete "$WINDIR\system32\Pictures.scr"
  Delete "$WINDIR\system32\PicturesUninstall.exe"

SectionEnd