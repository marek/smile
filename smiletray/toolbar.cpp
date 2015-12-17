#include <windows.h>
#include <strsafe.h>
#include <commctrl.h>
#include "toolbar.h"
#include "shared.h"
#include "resource.h"
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/shellcc/platform/commctls/toolbar/reflist.asp
//http://windowssdk.msdn.microsoft.com/en-us/library/ms650607.aspx
// CreateAToolBar creates a toolbar and adds a set of buttons to it.
// The function returns the handle to the toolbar if successful, 
// or NULL otherwise. 
// hwndParent is the handle to the toolbar's parent window. 

HWND CToolBar::CreateReBar(HWND hwndParent, DWORD style)
{
	INITCOMMONCONTROLSEX icex;
	REBARINFO rbi;
	HWND hwndRB;

	// Ensure that the common control DLL is loaded. 
	icex.dwSize = sizeof(INITCOMMONCONTROLSEX);
	icex.dwICC  = ICC_BAR_CLASSES|ICC_COOL_CLASSES;
	InitCommonControlsEx(&icex);

	// Create the rebar control
	hwndRB = CreateWindowEx(WS_EX_TOOLWINDOW,
		REBARCLASSNAME,
		NULL,
		style,
		0,0,0,0,
		hwndParent,
		NULL,
		hInst,
		NULL);

   if(!hwndRB)
      return NULL;

   // Initialize and send the REBARINFO structure.
   rbi.cbSize = sizeof(REBARINFO); 
   rbi.fMask  = 0;
   rbi.himl   = (HIMAGELIST)NULL;
   if(!SendMessage(hwndRB, RB_SETBARINFO, 0, (LPARAM)&rbi))
      return NULL;
}


HWND CToolBar::CreateToolBar(HWND hwndParent, HWND hRB) 
{ 
	TBBUTTON tbb[3]; 
	char szBuf[16]; 
	int iCut, iCopy, iPaste;
	INITCOMMONCONTROLSEX icex;
	HRESULT hr;
	size_t cch;
	REBARBANDINFO rbBand;
	HWND   hwndTB;
	DWORD  dwBtnSize;

	// Ensure that the common control DLL is loaded. 
	icex.dwSize = sizeof(INITCOMMONCONTROLSEX);
	icex.dwICC  = ICC_BAR_CLASSES|ICC_COOL_CLASSES;
	InitCommonControlsEx(&icex);

	// Create the rebar control
	hwndRB = CreateWindowEx(WS_EX_TOOLWINDOW,
		REBARCLASSNAME,
		NULL,
		WS_CHILD|WS_BORDER|WS_VISIBLE|WS_CLIPSIBLINGS|
		WS_CLIPCHILDREN|RBS_VARHEIGHT|CCS_NODIVIDER|CCS_NOPARENTALIGN|RBS_DBLCLKTOGGLE|
		RBS_BANDBORDERS|RBS_REGISTERDROP,
		0,0,0,0,
		hwndParent,
		NULL,
		hInst,
		NULL);

   if(!hwndRB)
      return NULL;

   // Initialize and send the REBARINFO structure.
   rbi.cbSize = sizeof(REBARINFO); 
   rbi.fMask  = 0;
   rbi.himl   = (HIMAGELIST)NULL;
   if(!SendMessage(hwndRB, RB_SETBARINFO, 0, (LPARAM)&rbi))
      return NULL;

   // Initialize structure members that both bands will share.
   rbBand.cbSize = sizeof(REBARBANDINFO);  // Required
   rbBand.fMask  = RBBIM_COLORS | RBBIM_TEXT | 
                   RBBIM_STYLE | RBBIM_CHILD  | RBBIM_CHILDSIZE | 
                   RBBIM_SIZE | RBBS_GRIPPERALWAYS;
   rbBand.fStyle = RBBS_BREAK | RBBS_CHILDEDGE;
   rbBand.clrBack = GetSysColor(COLOR_MENU);
  // rbBand.hbmBack = LoadBitmap(hInst, MAKEINTRESOURCE(IDB_BACKGRND));   


	// Create a toolbar. 
	hwndTB = CreateWindowEx(0, TOOLBARCLASSNAME, (LPSTR) NULL, 
		WS_CHILD | CCS_ADJUSTABLE | WS_BORDER | WS_VISIBLE | TBSTYLE_TOOLTIPS | TBSTYLE_FLAT, 0, 0, 0, 0, hwndParent, 
		(HMENU) ID_TOOLBAR, hInst, NULL); 

	// Send the TB_BUTTONSTRUCTSIZE message, which is required for 
	// backward compatibility. 
	SendMessage(hwndTB, TB_BUTTONSTRUCTSIZE, (WPARAM) sizeof(TBBUTTON), 0); 
	
	// Add the button strings to the toolbar's internal string list. 
	LoadString(hInst, IDS_CUT, szBuf, MAX_LEN-1); 
	//Save room for second terminating null character.
	hr = StringCchLength(szBuf, MAX_LEN, &cch);
	if(SUCCEEDED(hr))
	{
		szBuf[cch + 2] = 0;  //Double-null terminate.
	}
	else
	{
		// TODO: Write error handler.
	} 
	iCut = SendMessage(hwndTB, TB_ADDSTRING, 0, (LPARAM) (LPSTR) szBuf); 
	LoadString(hInst, IDS_COPY, szBuf, MAX_LEN-1);  
	//Save room for second terminating null character.
	hr = StringCchLength(szBuf, MAX_LEN, &cch);
	if(SUCCEEDED(hr))
	{
		szBuf[cch + 2] = 0;  //Double-null terminate.
	}
	else
	{
		// TODO: Write error handler.
	} 
	iCopy = SendMessage(hwndTB, TB_ADDSTRING, (WPARAM) 0, 
		(LPARAM) (LPSTR) szBuf); 
	LoadString(hInst, IDS_PASTE, szBuf, MAX_LEN-1);  
	//Save room for second terminating null character.
	hr = StringCchLength(szBuf, MAX_LEN, &cch);
	if(SUCCEEDED(hr))
	{
		szBuf[cch + 2] = 0;  //Double-null terminate.
	}
	else
	{
		// TODO: Write error handler.
	} 
	iPaste = SendMessage(hwndTB, TB_ADDSTRING, (WPARAM) 0, 
		(LPARAM) (LPSTR) szBuf); 

	// Fill the TBBUTTON array with button information, and add the 
	// buttons to the toolbar. The buttons on this toolbar have text 
	// but do not have bitmap images. 
	tbb[0].iBitmap = I_IMAGENONE; 
	tbb[0].idCommand = IDS_CUT; 
	tbb[0].fsState = TBSTATE_ENABLED; 
	tbb[0].fsStyle = BTNS_BUTTON; 
	tbb[0].dwData = 0; 
	tbb[0].iString = iCut; 

	tbb[1].iBitmap = I_IMAGENONE; 
	tbb[1].idCommand = IDS_COPY; 
	tbb[1].fsState = TBSTATE_ENABLED; 
	tbb[1].fsStyle = BTNS_BUTTON; 
	tbb[1].dwData = 0; 
	tbb[1].iString = iCopy; 

	tbb[2].iBitmap = I_IMAGENONE; 
	tbb[2].idCommand = IDS_PASTE; 
	tbb[2].fsState = TBSTATE_ENABLED; 
	tbb[2].fsStyle = BTNS_BUTTON; 
	tbb[2].dwData = 0; 
	tbb[2].iString = iPaste; 

	int NUM_BUTTONS = 3;

	SendMessage(hwndTB, TB_ADDBUTTONS, (WPARAM) NUM_BUTTONS, 
		(LPARAM) (LPTBBUTTON) &tbb); 

	SendMessage(hwndTB, TB_AUTOSIZE, 0, 0); 
	SendMessage(hwndTB, TB_SETBUTTONSIZE, 0, (LPARAM)MAKELONG (128, 128)); 

	ShowWindow(hwndTB, SW_SHOW); 
   // Get the height of the toolbar.
   dwBtnSize = SendMessage(hwndTB, TB_GETBUTTONSIZE, 0,0);

   // Set values unique to the band with the toolbar.
   rbBand.lpText     = "Tool Bar";
   rbBand.hwndChild  = hwndTB;
   rbBand.cxMinChild = 0;
   rbBand.cyMinChild = HIWORD(dwBtnSize);
   rbBand.cx         = 250;

   // Add the band that has the toolbar.
   SendMessage(hwndRB, RB_INSERTBAND, (WPARAM)-1, (LPARAM)&rbBand);

	return hwndRB; 
} 