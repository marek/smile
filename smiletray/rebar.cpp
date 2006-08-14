#include <windows.h>
#include <strsafe.h>
#include <commctrl.h>
#include "rebar.h"
#include "shared.h"
#include "resource.h"
// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/shellcc/platform/commctls/toolbar/reflist.asp
//http://windowssdk.msdn.microsoft.com/en-us/library/ms650607.aspx
// CreateAToolBar creates a toolbar and adds a set of buttons to it.
// The function returns the handle to the toolbar if successful, 
// or NULL otherwise. 
// hwndParent is the handle to the toolbar's parent window. 

CReBar::CReBar(HWND hwndParent, DWORD style = defaultReBarStyle)
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

   // We may need the parent handle later
   this->hwndParent = hwndParent;
}

LRESULT CReBar::AddToolBar(CToolBar TB)
{
   // Get the height of the toolbar.
   dwBtnSize = SendMessage(hwndTB, TB_GETBUTTONSIZE, 0,0);

   // Set values unique to the band with the toolbar.
   rbBand.lpText     = "Tool Bar";
   rbBand.hwndChild  = hwndTB;
   rbBand.cxMinChild = 0;
   rbBand.cyMinChild = HIWORD(dwBtnSize);
   rbBand.cx         = 0;

   // Add the band that has the toolbar.
   return SendMessage(hwndRB, RB_INSERTBAND, (WPARAM)-1, (LPARAM)&rbBand);
}
