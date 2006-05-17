#pragma once
#include "HookDirect3DDevice9.h"
#ifdef HOOK_EXPORTS
	#define HOOK_API __declspec(dllexport)
#else
	#define HOOK_API __declspec(dllimport)
#endif

// This segment must be defined as SHARED in the .DEF
#pragma data_seg (".HookSection")		
// Shared instance for all processes.
HHOOK hHook = NULL;	
HookDirect3DDevice9 * _device = NULL;
IDirect3DDevice9** ppReturnedDeviceInterface = NULL;
#pragma data_seg ()
#pragma comment(linker, "/section:.HookSection,RWS")

HOOK_API void InstallHook();
HOOK_API void RemoveHook();
HOOK_API void TakeScreenShot(char* file_name);