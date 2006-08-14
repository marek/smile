#include <windows.h>
#include <string>

#ifndef _TOOLBAR_H_
#define _TOOLBAR_H_

class CToolBar
{
	private
		HWND hwndParent;
		std::string strName;
	public:
		static const DWORD defaultStyle = WS_CHILD|WS_BORDER|WS_VISIBLE|WS_CLIPSIBLINGS|
			WS_CLIPCHILDREN|RBS_VARHEIGHT|CCS_NODIVIDER|CCS_NOPARENTALIGN|RBS_DBLCLKTOGGLE|
			RBS_BANDBORDERS|RBS_REGISTERDROP;

		CToolBar(HWND hwndParent, DWORD style);
		void SetName(std::string s);
		std::string GetName();
};


#endif