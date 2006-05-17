#include <iostream>
#include <windows.h>
#include <TCHAR.h>
#include "..\smile-hook\main.h"

int _tmain(int argc, TCHAR* argv[])
{
	std::cout<< "Hooking Direct3D...";
	InstallHook();
	std::cout<< "Done."<< std::endl
		<< "Press any key to take a screenshot."<< std::endl;
	std::cin.ignore();
	TakeScreenShot("test.bmp");
	std::cout<< "Done."<< std::endl
		<< "Press enter to unhook and exit."<< std::endl;
	std::cin.ignore();
	
	RemoveHook();

	return 0;
}