// main.cpp : Defines the entry point for the DLL application.
#include <windows.h>
#include <d3d9.h>
#include <d3dx9tex.h>
#include <shlwapi.h>
#include "hookapi.h"
#include "main.h"
#include "HookDirect3D9.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

/*
// This segment must be defined as SHARED in the .DEF
#pragma data_seg (".HookSection")		
// Shared instance for all processes.
HHOOK hHook = NULL;	
LPDIRECT3DDEVICE9 _device = NULL;
#pragma data_seg ()
#pragma comment(linker, "/section:.HookSection,RWS")
*/

HINSTANCE hDLL;

// Function pointer types.
typedef IDirect3D9* (WINAPI *Direct3DCreate9_t)(UINT sdk_version);

// Function prototypes.
IDirect3D9* WINAPI HookDirect3DCreate9(UINT sdk_version);

// Direct3D 9 Hook structure.
enum
{
	D3DFN_Direct3DCreate9 = 0
};

SDLLHook D3D9Hook = 
{
	"D3D9.DLL",
	false, NULL,		// Default hook disabled, NULL function pointer.
	{
		{ "Direct3DCreate9", HookDirect3DCreate9},
		{ NULL, NULL }
	}
};

// Hook function.
IDirect3D9* WINAPI HookDirect3DCreate9(UINT sdk_version)
{
	// Let the world know we're working.
	MessageBeep(MB_ICONINFORMATION);
	OutputDebugString( "smile-hook: HookDirect3DCreate9 called.\n" );

	Direct3DCreate9_t old_func = (Direct3DCreate9_t) D3D9Hook.Functions[D3DFN_Direct3DCreate9].OrigFn;
	IDirect3D9* d3d = old_func(sdk_version);
	
	return d3d ? new HookDirect3D9(d3d) : 0;
}

BOOL APIENTRY DllMain( HMODULE hModule,
					   DWORD  ul_reason_for_call,
					   LPVOID lpReserved
					 )
{
	if(ul_reason_for_call == DLL_PROCESS_ATTACH)  // When initializing....
	{
		hDLL = hModule;
		// We don't need thread notifications for what we're doing.  Thus, get
		// rid of them, thereby eliminating some of the overhead of this DLL

		HookAPICalls(&D3D9Hook);
	}

	return TRUE;
}

//HOOK_API void TakeScreenShot(LPDIRECT3DDEVICE9 _device, char* file_name) 
HOOK_API void TakeScreenShot(char* file_name) 
{  
	if(_device == NULL)
	{
		MessageBox(NULL, "tss", "tss", MB_OK);
		return;
	}
	D3DDEVICE_CREATION_PARAMETERS dcp; 
	dcp.AdapterOrdinal = D3DADAPTER_DEFAULT; 
	_device->GetCreationParameters(&dcp); 

	D3DDISPLAYMODE dm; 
	ZeroMemory(&dm, sizeof(D3DDISPLAYMODE)); 

	LPDIRECT3D9 lpD3D = NULL; 
	_device->GetDirect3D(&lpD3D); 

	if (lpD3D) 
	{ 
		// query the screen dimensions of the current adapter 
		lpD3D->GetAdapterDisplayMode(dcp.AdapterOrdinal, &dm); 
		lpD3D->Release(); 
	} 

	LPDIRECT3DSURFACE9 lpSurface = NULL; 

	_device->CreateOffscreenPlainSurface(dm.Width, dm.Height,D3DFMT_A8R8G8B8,D3DPOOL_SCRATCH,&lpSurface,NULL); 
	_device->GetFrontBufferData(NULL,lpSurface); 
	if(lpSurface!=NULL)    
		D3DXSaveSurfaceToFile(file_name,D3DXIFF_BMP,lpSurface,NULL,NULL); 

	lpSurface->Release(); 
}

HOOK_API LRESULT CALLBACK HookProc(int nCode, WPARAM wParam, LPARAM lParam) 
{
    return CallNextHookEx( hHook, nCode, wParam, lParam); 
}

HOOK_API void InstallHook()
{
	OutputDebugString( "smile-hook: capture hook installed.\n" );
    hHook = SetWindowsHookEx( WH_CBT, HookProc, hDLL, 0 ); 
}

HOOK_API void RemoveHook()
{
	OutputDebugString( "smile-hook: capture hook removed.\n" );
    UnhookWindowsHookEx( hHook );
}


#ifdef _MANAGED
#pragma managed(pop)
#endif

