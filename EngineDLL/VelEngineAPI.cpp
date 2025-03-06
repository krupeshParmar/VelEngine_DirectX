
#include "Common.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <Windows.h>
using namespace vel;

namespace
{
	HMODULE game_code_dll{ nullptr };
} // annonymous

VEL_EDITOR_API u32
LoadGameCodeDll(const char* dll_path)
{
	if (game_code_dll) return 0;
	game_code_dll = LoadLibraryA(dll_path);
	assert(game_code_dll);

	return game_code_dll ? TRUE : FALSE;
}

VEL_EDITOR_API u32
UnloadGameCodeDll()
{
	if (!game_code_dll) return FALSE;
	assert(game_code_dll);
	int result{ FreeLibrary(game_code_dll) };
	assert(result);
	game_code_dll = nullptr;
	return TRUE;
}
