#include <windows.h>
#include "toolbar.h"

#ifndef _REBAR_H_
#define _REBAR_H_

class CReBar
{
	private:
		HWND hwndParent;
	public:
		static const DWORD defaultStyle = WS_CHILD|WS_BORDER|WS_VISIBLE|WS_CLIPSIBLINGS|
			WS_CLIPCHILDREN|RBS_VARHEIGHT|CCS_NODIVIDER|CCS_NOPARENTALIGN|RBS_DBLCLKTOGGLE|
			RBS_BANDBORDERS|RBS_REGISTERDROP;

		CReBar(HWND hwndParent, DWORD style);
		LRESULT AddToolBar(CToolBar TB);
};


#endif