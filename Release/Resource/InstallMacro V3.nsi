; Script generated by the HM NIS Edit Script Wizard.

; HM NIS Edit Wizard helper defines
!define PRODUCT_NAME "Macro"
!define PRODUCT_VERSION "2.5.3"
!define PRODUCT_PUBLISHER "Eomtaewook"
!define PRODUCT_WEB_SITE "https://github.com/EomTaeWook/EmulatorMacro"
!define PRODUCT_DIR_REGKEY "Software\Microsoft\Windows\CurrentVersion\App Paths\Macro.exe"
!define PRODUCT_UNINST_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\${PRODUCT_NAME}"
!define PRODUCT_UNINST_ROOT_KEY "HKLM"

!define BASEPATH "D:\Macro"
!define PATH "D:\Macro\Release\exe"

; MUI 1.67 compatible ------
!include "MUI.nsh"

; MUI Settings
!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

; Welcome page
!insertmacro MUI_PAGE_WELCOME
; License page
!insertmacro MUI_PAGE_LICENSE "${BASEPATH}\LICENSE"
; Directory page
!insertmacro MUI_PAGE_DIRECTORY
; Instfiles page
!insertmacro MUI_PAGE_INSTFILES
; Finish page
!define MUI_FINISHPAGE_RUN "$INSTDIR\Macro.exe"
!insertmacro MUI_PAGE_FINISH

; Uninstaller pages
!insertmacro MUI_UNPAGE_INSTFILES

; Language files
!insertmacro MUI_LANGUAGE "Korean"

; MUI end ------

Name "${PRODUCT_NAME} ${PRODUCT_VERSION}"
OutFile "${BASEPATH}\Release\Install\Setup.exe"
InstallDir "$PROGRAMFILES\Macro"
InstallDirRegKey HKLM "${PRODUCT_DIR_REGKEY}" ""
ShowInstDetails show
ShowUnInstDetails show

Section "MainSection" SEC01
  SetOutPath "$INSTDIR"
  SetOverwrite try
  File "${PATH}\config.json"
  File "${PATH}\ControlzEx.dll"
  File "${PATH}\KosherUtils.dll"
  SetOutPath "$INSTDIR\Datas"
  File "${PATH}\Datas\ApplicationData.json"
  File "${PATH}\Datas\LabelDocument.json"
  File "${PATH}\Datas\MessageDocument.json"
  SetOutPath "$INSTDIR\dll\x64"
  File "${PATH}\dll\x64\OpenCvSharpExtern.dll"
  File "${PATH}\dll\x64\opencv_videoio_ffmpeg453_64.dll"
  SetOutPath "$INSTDIR\dll\x86"
  File "${PATH}\dll\x86\OpenCvSharpExtern.dll"
  File "${PATH}\dll\x86\opencv_videoio_ffmpeg453.dll"
  SetOutPath "$INSTDIR"
  File "${PATH}\Macro.exe"
  File "${PATH}\Macro.exe.config"
  CreateDirectory "$SMPROGRAMS\Macro"
  CreateShortCut "$SMPROGRAMS\Macro\Macro.lnk" "$INSTDIR\Macro.exe"
  CreateShortCut "$DESKTOP\Macro.lnk" "$INSTDIR\Macro.exe"
  File "${PATH}\MahApps.Metro.dll"
  File "${PATH}\MahApps.Metro.IconPacks.Material.dll"
  File "${PATH}\Newtonsoft.Json.dll"
  File "${PATH}\NLog.dll"
  File "${PATH}\OpenCvSharp.dll"
  File "${PATH}\OpenCvSharp.Extensions.dll"
  File "${PATH}\OpenCvSharp.WpfExtensions.dll"
  File "${PATH}\Patcher.exe"
  File "${PATH}\Patcher.exe.config"
  File "${PATH}\System.Buffers.dll"
  File "${PATH}\System.Drawing.Common.dll"
  File "${PATH}\System.Memory.dll"
  File "${PATH}\System.Numerics.Vectors.dll"
  File "${PATH}\System.Runtime.CompilerServices.Unsafe.dll"
  File "${PATH}\System.Security.Principal.Windows.dll"
  File "${PATH}\System.Threading.Tasks.Extensions.dll"
  File "${PATH}\System.ValueTuple.dll"
  File "${PATH}\System.Windows.Interactivity.dll"
  File "${PATH}\Unity.Abstractions.dll"
  File "${PATH}\Unity.Container.dll"
  File "${PATH}\Utils.dll"
SectionEnd

Section -AdditionalIcons
  CreateShortCut "$SMPROGRAMS\Macro\Uninstall.lnk" "$INSTDIR\uninst.exe"
SectionEnd

Section -Post
  WriteUninstaller "$INSTDIR\uninst.exe"
  WriteRegStr HKLM "${PRODUCT_DIR_REGKEY}" "" "$INSTDIR\Macro.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayName" "$(^Name)"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "UninstallString" "$INSTDIR\uninst.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayIcon" "$INSTDIR\Macro.exe"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "DisplayVersion" "${PRODUCT_VERSION}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "URLInfoAbout" "${PRODUCT_WEB_SITE}"
  WriteRegStr ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}" "Publisher" "${PRODUCT_PUBLISHER}"
SectionEnd


Function un.onUninstSuccess
  HideWindow
  MessageBox MB_ICONINFORMATION|MB_OK "$(^Name)는(은) 완전히 제거되었습니다."
FunctionEnd

Function un.onInit
  MessageBox MB_ICONQUESTION|MB_YESNO|MB_DEFBUTTON2 "$(^Name)을(를) 제거하시겠습니까?" IDYES +2
  Abort
FunctionEnd

Section Uninstall
  Delete "$INSTDIR\uninst.exe"
  Delete "$INSTDIR\Utils.dll"
  Delete "$INSTDIR\Unity.Container.dll"
  Delete "$INSTDIR\Unity.Abstractions.dll"
  Delete "$INSTDIR\System.Windows.Interactivity.dll"
  Delete "$INSTDIR\System.ValueTuple.dll"
  Delete "$INSTDIR\System.Threading.Tasks.Extensions.dll"
  Delete "$INSTDIR\System.Security.Principal.Windows.dll"
  Delete "$INSTDIR\System.Runtime.CompilerServices.Unsafe.dll"
  Delete "$INSTDIR\System.Numerics.Vectors.dll"
  Delete "$INSTDIR\System.Memory.dll"
  Delete "$INSTDIR\System.Drawing.Common.dll"
  Delete "$INSTDIR\System.Buffers.dll"
  Delete "$INSTDIR\Patcher.exe.config"
  Delete "$INSTDIR\Patcher.exe"
  Delete "$INSTDIR\OpenCvSharp.WpfExtensions.dll"
  Delete "$INSTDIR\OpenCvSharp.Extensions.dll"
  Delete "$INSTDIR\OpenCvSharp.dll"
  Delete "$INSTDIR\NLog.dll"
  Delete "$INSTDIR\Newtonsoft.Json.dll"
  Delete "$INSTDIR\MahApps.Metro.IconPacks.Material.dll"
  Delete "$INSTDIR\MahApps.Metro.dll"
  Delete "$INSTDIR\Macro.exe"
  Delete "$INSTDIR\Macro.exe.config"
  Delete "$INSTDIR\dll\x86\opencv_videoio_ffmpeg453.dll"
  Delete "$INSTDIR\dll\x86\OpenCvSharpExtern.dll"
  Delete "$INSTDIR\dll\x64\opencv_videoio_ffmpeg453_64.dll"
  Delete "$INSTDIR\dll\x64\OpenCvSharpExtern.dll"
  Delete "$INSTDIR\Datas\MessageDocument.json"
  Delete "$INSTDIR\Datas\LabelDocument.json"
  Delete "$INSTDIR\Datas\ApplicationData.json"
  Delete "$INSTDIR\KosherUtils.dll"
  Delete "$INSTDIR\ControlzEx.dll"
  Delete "$INSTDIR\config.json"
  Delete "$INSTDIR\Error.log"
  Delete "$INSTDIR\Cache.dll"
  Delete "$INSTDIR\save.dll"
  
  IfFileExists "$INSTDIR\OpenCvSharp.Blob.dll" YES1 NO1
  YES1:
  Delete "$INSTDIR\OpenCvSharp.Blob.dll"
  GOTO END1
  NO1:
  END1:
  
  IfFileExists "$INSTDIR\OpenCvSharp.UserInterface.dll" YES2 NO2
  YES2:
  Delete "$INSTDIR\OpenCvSharp.UserInterface.dll"
  GOTO END2
  NO2:
  END2:
  
  IfFileExists "$INSTDIR\dll\x64\opencv_ffmpeg400_64.dll" YES3 NO3
  YES3:
  Delete "$INSTDIR\dll\x64\opencv_ffmpeg400_64.dll"
  GOTO END3
  NO3:
  END3:
  
  IfFileExists "$INSTDIR\dll\x64\opencv_ffmpeg400.dll" YES4 NO4
  YES4:
  Delete "$INSTDIR\dll\x64\opencv_ffmpeg400.dll"
  GOTO END4
  NO4:
  END4:

  Delete "$SMPROGRAMS\Macro\Uninstall.lnk"
  Delete "$DESKTOP\Macro.lnk"
  Delete "$SMPROGRAMS\Macro\Macro.lnk"

  RMDir "$SMPROGRAMS\Macro"
  RMDir "$INSTDIR\Save"
  RMDir "$INSTDIR\dll\x86"
  RMDir "$INSTDIR\dll\x64"
  RMDir "$INSTDIR\dll"
  RMDir "$INSTDIR\Datas"
  RMDir "$INSTDIR"

  DeleteRegKey ${PRODUCT_UNINST_ROOT_KEY} "${PRODUCT_UNINST_KEY}"
  DeleteRegKey HKLM "${PRODUCT_DIR_REGKEY}"
  SetAutoClose true
SectionEnd