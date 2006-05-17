#include <windows.h>

bool WINAPI DllMain(HMODULE hDll, DWORD dwReason, PVOID pvReserved)
{
	if(dwReason == DLL_PROCESS_ATTACH)
	{
		DisableThreadLibraryCalls(hDll);

		HMODULE hMod = LoadLibrary("d3d9.dll");		
	
		oDirect3DCreate9 = (tDirect3DCreate9)DetourFunc(
			(BYTE*)GetProcAddress(hMod, "Direct3DCreate9"),
			(BYTE*)hkDirect3DCreate9, 
			5);

		return TRUE;
	}
	else if(dwReason == DLL_PROCESS_DETACH)
	{
		if(ofile) { ofile.close(); }
	}
    return FAlSE;
}
