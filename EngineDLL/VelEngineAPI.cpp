
#include <atlsafe.h>
#include "Common.h"
#include "Content/ContentToEngine.h"
#include "Components/ScriptComponent.h"
#include "Platform/PlatformTypes.h"
#include "Platform/Platform.h"
#include "Graphics/Renderer.h"

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

	utl::vector<graphics::render_surface> surfaces_list;
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

VEL_EDITOR_API u32
CreateRenderSurface(HWND host, s32 width, s32 height)
{
	platform::window_init_info info{ nullptr, host, nullptr, 0, 0, width, height };
	graphics::render_surface surface{ platform::create_window(&info),{} };
	assert(surface.window.is_valid());
	surfaces_list.emplace_back(surface);
	return (u32)surfaces_list.size() - 1;
}

VEL_EDITOR_API void
RemoveRenderSurface(u32 id)
{
	assert(id < surfaces_list.size());
	platform::window& win = surfaces_list[id].window;
	assert(win.is_valid());
	platform::remove_window(win.get_id());
}

VEL_EDITOR_API void
ResizeRenderSurface(u32 id)
{
	assert(id < surfaces_list.size());
	platform::window& win = surfaces_list[id].window;
	assert(win.is_valid());
	win.resize(0, 0);
}

VEL_EDITOR_API id::id_type
CreateResource(u8* data, content::asset_type::type type)
{
	if (type == content::asset_type::mesh)
	{

	}

	if (type == content::asset_type::material)
	{
		
	}

	return id::invalid_id;
}

VEL_EDITOR_API void
DestroyResource(id::id_type id, content::asset_type::type type)
{
}

VEL_EDITOR_API HWND
GetWindowHandle(u32 id)
{
	assert(id < surfaces_list.size());
	platform::window& win = surfaces_list[id].window;
	return (HWND)win.handle();
}
