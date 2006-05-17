/////////////////////////////////////////////////////////////////////////////
//
// smiletray.cpp -- Compact Check Count Tray Appliaction
// v0.01
// Written by Marek Kudlacz
// Copyright (c)2005
//
/////////////////////////////////////////////////////////////////////////////

// Headers
#include <windows.h>
#include <tchar.h>
#include <commctrl.h>
#include <limits.h>
#include <time.h>
#include "resource.h"
using namespace System::Text::RegularExpressions;

// Libs
#pragma comment(lib, "comctl32.lib")

// Various consts & Defs
#define	WM_USER_SHELLICON WM_USER + 1 
#define WM_TASKBAR_CREATE RegisterWindowMessage(_T("TaskbarCreated"))
#define ID_TIMER 1
#define TIMER_DELAY 10			// timer will run every x milliseconds

struct settings_s
{
	unsigned int delay;		// Do work every x times
};

// Globals
HWND hWnd;
HINSTANCE hInst;
NOTIFYICONDATA structNID;
BOOL Enabled;
char szDatPath[MAX_PATH];
char szName[256];
struct settings_s structSettings;


/* ================================================================================================================== */

/*
Name: ... SaveSettings(...)
Desc: SaveSettings
*/
BOOL SaveSettings()
{
	HANDLE hFile;
	if ((hFile = CreateFile(szDatPath, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, FILE_ATTRIBUTE_NORMAL, NULL)) != INVALID_HANDLE_VALUE) {
		DWORD dwWritten;
		if(!WriteFile(hFile, &structSettings, sizeof(structSettings), &dwWritten, NULL))
			MessageBox(NULL, "Writing new settings failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		CloseHandle(hFile);
	} else {
		MessageBox(NULL, "Cannot open/create settings file!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		return FALSE;
	}
	return TRUE;
}

/* ================================================================================================================== */

/*
Name: ... GetIndentityKeyHandle(...)
Desc: Return a Registry handle to the appropriate Identity
*/
int GetLastIndentity(char * szName)
{
	HKEY hKey;
	char szSteamEXE[MAX_PATH];
	char szSteamDIR[MAX_PATH];
	DWORD dwValSize = MAX_PATH;
	DWORD dwType = REG_SZ;

	// Find steam exe path
	if(RegOpenKeyEx(HKEY_CURRENT_USER, "\\Software\\Valve\\Steam", 0, KEY_QUERY_VALUE, &hKey) != ERROR_SUCCESS)
		return 0;

	if((RegQueryValueEx(hKey, "SteamExe", NULL, &dwType, (LPBYTE)szSteamEXE, &dwValSize) != ERROR_SUCCESS) || (dwValSize > MAX_PATH)) {
		RegCloseKey(hKey);
		return 0;
	}
	RegCloseKey(hKey);

	// Get the SteamEXE's directory name
	strncpy(szSteamDIR, szSteamEXE, strrchr(szSteamEXE, '\\') - szSteamEXE+1);


	return 1;
}

/* ================================================================================================================== */

/*
Name: ... UpdateTrayNotify(...)
Desc: Update Notify with specific tray message
*/
BOOL UpdateTrayNotify()
{
	TCHAR buff[128];
	if(Enabled)
		strcpy(buff, "SmileTray: Enabled.");
	else
		strcpy(buff, "SmileTray: Disabled.");
	strcpy(structNID.szTip, buff);	
	if(!Shell_NotifyIcon(NIM_MODIFY, &structNID));
		return FALSE;
	return TRUE;
}

/* ================================================================================================================== */

/*
Name: ... SetIdentityCCC(...)
Desc: Set the compact check count in the registry to specified value
*/
BOOL SetIdentityCCC(HKEY hKey, DWORD dwValue)
{
	if(RegSetValueEx(hKey, "Compact Check Count", 0, REG_DWORD, (BYTE *)&dwValue, sizeof(DWORD)) != ERROR_SUCCESS)
		return FALSE;
	return TRUE;
}

/* ================================================================================================================== */

/*
Name: ... AboutDlgProc(...)
Desc: proccess the about dialog
*/
BOOL CALLBACK AboutDlgProc(HWND hwnd, UINT Message, WPARAM wParam, LPARAM lParam)
{
    switch(Message)
    {
        case WM_INITDIALOG:
			{
				return TRUE;
			}
			break;
        case WM_COMMAND:
			{
				switch(LOWORD(wParam))
				{
					case IDOK:
						{
							EndDialog(hwnd, IDOK);
						}
						break;
				}
			}
			break;
        default:
            return FALSE;
    }
    return TRUE;
}


/* ================================================================================================================== */

/*
Name: ... OptionsDlgProc(...)
Desc: proccess the options dialog
*/
BOOL CALLBACK OptionsDlgProc(HWND hwnd, UINT Message, WPARAM wParam, LPARAM lParam)
{
    switch(Message)
    {
        case WM_INITDIALOG:
			{
				TCHAR buff[100];

				// Set spin(up-down) control options
				UDACCEL upaccel[3];
				upaccel[0].nInc = 1;
				upaccel[0].nSec = 0;
				upaccel[1].nInc = 100;
				upaccel[1].nSec = 4;
				upaccel[2].nInc = 1000;
				upaccel[2].nSec = 10;
				SendDlgItemMessage(hwnd, IDC_DELAY, UDM_SETACCEL, (WPARAM)(sizeof(upaccel)/sizeof(upaccel[0])), (LPARAM)&upaccel);
				SendDlgItemMessage(hwnd, IDC_DELAY, UDM_SETBASE, (WPARAM)10, 0);
				SendDlgItemMessage(hwnd, IDC_DELAY, UDM_SETRANGE32, (WPARAM)0, (LPARAM)INT_MAX);
				SendDlgItemMessage(hwnd, IDC_DELAY, UDM_SETPOS32, 0, (LPARAM)structSettings.delay);

				// Display current values
				wsprintf((LPTSTR) buff, "%d", structSettings.delay);
				SetDlgItemText(hwnd, IDC_DELAY, buff);
			}
			break;
        case WM_COMMAND:
			{
				switch(LOWORD(wParam))
				{
					case IDOK:
						{
							// Grab Values
							structSettings.delay = SendDlgItemMessage(hwnd, IDC_DELAY, UDM_GETPOS32, 0, (LPARAM)NULL);

							// Kill dialog
							EndDialog(hwnd, IDOK);

							// Save settings
							SaveSettings();
						}
						break;
					case IDCANCEL:
						{
							EndDialog(hwnd, IDOK);
						}
						break;
				}
			}
			break;
		case WM_NOTIFY:
			{
				switch(wParam)
				{
					case IDC_SPINDELAY:
						{
							LPNMUPDOWN lpnmud = (LPNMUPDOWN)lParam;
							switch(lpnmud->hdr.code)
							{
								case UDN_DELTAPOS:
									{
										int val;
										TCHAR buff[100];

										if((lpnmud->iPos + lpnmud->iDelta) < 0)
											val = 0;
										else 
											val = lpnmud->iPos + lpnmud->iDelta;

										wsprintf((LPTSTR) buff, "%d", val);
										SetDlgItemText(hwnd, IDC_DELAY, buff);
									}
								break;
							}
						}
						break;
				}
			}
			break;
        default:
            return FALSE;
    }
    return TRUE;
}

/* ================================================================================================================== */

/*
Name: ... WndProc(...)
Desc: Main hidden "Window" that handles the messaging for our system tray
*/
LRESULT CALLBACK WndProc(HWND hwnd, UINT Message, WPARAM wParam, LPARAM lParam)
{
	POINT lpClickPoint;

	if(Message == WM_TASKBAR_CREATE) {			// Taskbar has been recreated (Explorer crashed?)
		// Display tray icon
		if(!Shell_NotifyIcon(NIM_ADD, &structNID)) {
			MessageBox(NULL, "Systray Icon Creation Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
			DestroyWindow(hWnd);
			return -1;
		}
	}

	switch(Message)
	{
		case WM_CREATE:
			{
				// Start out timer
				if(!SetTimer(hwnd, ID_TIMER, TIMER_DELAY, NULL)) {
					MessageBox(hwnd, "Could Not Start Timer!", "Error", MB_OK | MB_ICONEXCLAMATION);
					return -1;	// Creation Failed
				}
			}
			break;
		case WM_DESTROY:
			{
				Shell_NotifyIcon(NIM_DELETE, &structNID);
				KillTimer(hWnd, ID_TIMER);
				PostQuitMessage(0);
			}
			break;
		case WM_USER_SHELLICON:			// sys tray icon Messages
			{
				switch(LOWORD(lParam)) 
				{ 
					case WM_RBUTTONDOWN: 
						{
							HMENU hMenu, hSubMenu;
							// get mouse cursor position x and y as lParam has the Message itself 
							GetCursorPos(&lpClickPoint);
							
							// Load menu resource
							hMenu = LoadMenu(hInst, MAKEINTRESOURCE(IDR_POPUP_MENU));
							if(!hMenu)
								return -1;	// !0, message not successful?
							
							// Select the first submenu
							hSubMenu = GetSubMenu(hMenu, 0);
							if(!hSubMenu) {
								DestroyMenu(hMenu);        // Be sure to Destroy Menu Before Returning
								return -1;
							}

							// Set Enabled State
							if(Enabled)
								CheckMenuItem(hMenu, ID_POPUP_ENABLE, MF_BYCOMMAND | MF_CHECKED);
							else
								CheckMenuItem(hMenu, ID_POPUP_ENABLE, MF_BYCOMMAND | MF_UNCHECKED);

							// Display menu
							SetForegroundWindow(hWnd);
							TrackPopupMenu(hSubMenu, TPM_LEFTALIGN | TPM_LEFTBUTTON | TPM_BOTTOMALIGN, lpClickPoint.x, lpClickPoint.y, 0, hWnd, NULL);
							SendMessage(hWnd, WM_NULL, 0, 0);

							// Kill off objects we're done with
							DestroyMenu(hMenu);
						}
						break;
				} 
			}
			break;
		case WM_CLOSE: 
				{
					DestroyWindow(hWnd);
				}
				break;

		case WM_COMMAND: 
			{
				switch(LOWORD(wParam))
				{
					case ID_POPUP_EXIT:
						{
							// Remove Shell Icon and quit;
							DestroyWindow(hWnd);
						}
						break;
					case ID_POPUP_ABOUT:	// Open about box
						{
							DialogBox(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_ABOUT), hwnd, (DLGPROC)AboutDlgProc);
						}
						break;
					case ID_POPUP_OPEN:		// Open options box (and maybe save settings after)
						{
							DialogBox(GetModuleHandle(NULL), MAKEINTRESOURCE(IDD_OPTIONS), hwnd, (DLGPROC)OptionsDlgProc);
						}
						break;
					case ID_POPUP_ENABLE:
						{	
							if(Enabled)
								KillTimer(hWnd, ID_TIMER);
							else {
								if(!SetTimer(hwnd, ID_TIMER, TIMER_DELAY, NULL)) {
									MessageBox(hwnd, "Could Not Start Timer!", "Error", MB_OK | MB_ICONEXCLAMATION);
									return -1;			// Creation Failed
								}
							}
							Enabled = !Enabled;
							UpdateTrayNotify();			// Update Notify Tip
						}	
						break;
				}
			}
			break;
		case WM_TIMER:
			{
				switch(wParam)
				{
					case ID_TIMER:
						{



						}
						break;
				}
			}
			break;
		default:
			return DefWindowProc(hwnd, Message, wParam, lParam);
	}
	return 0;		// Return 0 = Message successfully proccessed
}

/* ================================================================================================================== */

/*
Name: ... WinMain(...)
Desc: Main Entry point
*/
int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPSTR lpCmdLine, int nCmdShow)
{
	MSG Msg;
	WNDCLASSEX wc;
	HANDLE hMutexInstance, hFile;
	INITCOMMONCONTROLSEX iccex;
	char szAppPath[MAX_PATH];

	// Check for single instance
	hMutexInstance = CreateMutex(NULL, FALSE,_T("smiletray-{B3B4F756-58FA-4131-92F9-3CA746518614}"));
	if(GetLastError() == ERROR_ALREADY_EXISTS || GetLastError() == ERROR_ACCESS_DENIED)
		return 0;

	// Copy instance so it can be used globally in other methods
	hInst = hInstance;

	// Create path to settings file
	GetModuleFileName(NULL, szAppPath, MAX_PATH);
	strncpy(szDatPath, szAppPath, strrchr(szAppPath, '\\') - szAppPath+1);
	strcat(szDatPath, "smiletray.dat");

	// Read Options
    if((hFile = CreateFile(szDatPath, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL)) != INVALID_HANDLE_VALUE) {
		DWORD dwRead;
        if(!ReadFile(hFile, &structSettings, sizeof(structSettings), &dwRead, NULL)) {
			CloseHandle(hFile);
			MessageBox(NULL, "Reading Settings failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
			return 0;
        }
		CloseHandle(hFile);
	} else {
		// Create default options
		structSettings.delay = 0;
		if(!SaveSettings()) {
				MessageBox(NULL, "Cannot open/create settings file!", "Error!", MB_ICONEXCLAMATION | MB_OK);
				return 0;
		}
	}

	// Init common controls
	iccex.dwSize = sizeof(INITCOMMONCONTROLSEX);
	iccex.dwICC = ICC_UPDOWN_CLASS;
	if(!InitCommonControlsEx(&iccex)) {
		MessageBox(NULL, "Cannot Initialize Common Controls!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
	}

	// Window "class"
	wc.cbSize =			sizeof(WNDCLASSEX);
	wc.style =			CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc =	WndProc;
	wc.cbClsExtra =		0;
	wc.cbWndExtra =		0;
	wc.hInstance =		hInstance;
	wc.hIcon =			LoadIcon(hInstance, (LPCTSTR)MAKEINTRESOURCE(IDI_TRAYICON));
	wc.hCursor =		LoadCursor(NULL, IDC_ARROW);
	wc.hbrBackground =	(HBRUSH)GetStockObject(WHITE_BRUSH);
	wc.lpszMenuName =	NULL;
	wc.lpszClassName =	"smiletray";
	wc.hIconSm		 =	LoadIcon(hInstance, (LPCTSTR)MAKEINTRESOURCE(IDI_TRAYICON));
	if(!RegisterClassEx(&wc)) {
		MessageBox(NULL, "Window Registration Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
	}

	// Create the hidden window
	hWnd = CreateWindowEx(
		WS_EX_CLIENTEDGE,
		"smiletray",
		"Smile! Screenshot Tray Utility ",
		WS_OVERLAPPEDWINDOW,
		CW_USEDEFAULT, 
		CW_USEDEFAULT, 
		CW_USEDEFAULT, 
		CW_USEDEFAULT,
		NULL, 
		NULL, 
		hInstance, 
		NULL);
	if(hWnd == NULL) {
		MessageBox(NULL, "Window Creation Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
	}

	// tray icon settings
	structNID.cbSize = sizeof(NOTIFYICONDATA);				
	structNID.hWnd = (HWND)hWnd;								 
	structNID.uID = IDI_TRAYICON;								 
	structNID.uFlags = NIF_ICON | NIF_MESSAGE | NIF_TIP;		
	strcpy(structNID.szTip, "smiletray");							 
	structNID.hIcon = LoadIcon(hInstance, (LPCTSTR)MAKEINTRESOURCE(IDI_TRAYICON));								 
	structNID.uCallbackMessage = WM_USER_SHELLICON;				 

	// Display tray icon
	if(!Shell_NotifyIcon(NIM_ADD, &structNID)) {
		MessageBox(NULL, "Systray Icon Creation Failed!", "Error!", MB_ICONEXCLAMATION | MB_OK);
		return 0;
	}

	// Set mode to enabled
	Enabled = TRUE;

	// Message Loop
	while(GetMessage(&Msg, NULL, 0, 0))
	{
		TranslateMessage(&Msg);
		DispatchMessage(&Msg);
	}
	return Msg.wParam;
}
