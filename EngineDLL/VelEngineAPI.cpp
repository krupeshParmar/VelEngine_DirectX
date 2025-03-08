
#include <atlsafe.h>
#include "Common.h"
#include "..\VelEngine\Components\ScriptComponent.h"

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif

#include <Windows.h>
using namespace vel;

namespace
{
	HMODULE game_code_dll{ nullptr };
	using _get_script_creator = script::detail::script_creator(*)(size_t);
	_get_script_creator get_script_creator{ nullptr };
	using _get_script_names = LPSAFEARRAY(*)(void);
	_get_script_names get_script_names{ nullptr };
} // annonymous namespace

VEL_EDITOR_API u32
LoadGameCodeDll(const char* dll_path)
{
	if (game_code_dll) return 0;
	game_code_dll = LoadLibraryA(dll_path);
	assert(game_code_dll);

	get_script_creator =
		(_get_script_creator)GetProcAddress(game_code_dll, "get_script_creator");

	get_script_names =
		(_get_script_names)GetProcAddress(game_code_dll, "get_script_names");

	return (game_code_dll && get_script_creator && get_script_names) ? TRUE : FALSE;
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

VEL_EDITOR_API script::detail::script_creator
GetScriptCreator(const char* name)
{
	return (game_code_dll && get_script_creator) ? get_script_creator(string_hash()(name)) : nullptr;
}

VEL_EDITOR_API LPSAFEARRAY
GetScriptNames()
{
	return (game_code_dll && get_script_names) ? get_script_names() : nullptr;
}
