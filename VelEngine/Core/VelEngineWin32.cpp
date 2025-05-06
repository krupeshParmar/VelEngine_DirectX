#include <thread>

#if !defined(SHIPPING) && defined(_WIN64)
#include "../Content/ContentLoader.h"
#include "../Components/ScriptComponent.h"
#include "../Platform/PlatformTypes.h"
#include "../Platform/Platform.h"
#include "../Graphics/Renderer.h"

using namespace vel;

namespace {

	graphics::render_surface game_window{};

	LRESULT win_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
	{
		switch (msg)
		{
		case WM_DESTROY:
		{
			if (game_window.window.is_closed())
			{
				PostQuitMessage(0);
				return 0;
			}
			break;
		}
		case WM_SYSCHAR:
			if (wparam == VK_RETURN && (HIWORD(lparam) & KF_ALTDOWN))
			{
				game_window.window.set_fullscreen(!game_window.window.is_fullscreen());
				return 0;
			}
			break;
		}
		return DefWindowProc(hwnd, msg, wparam, lparam);
	}
}	// annonymous namespace

bool engine_intitalize()
{
	if (!vel::content::load_game()) return false;

	platform::window_init_info info
	{
		&win_proc, nullptr, L"Vel Game"	// TODO: get the name from the loaded game file
	};

	game_window.window = platform::create_window(&info);
	if (!game_window.window.is_valid()) return false;

	return true;
}
void engine_update()
{
	vel::script::update_scripts(10.f);
	std::this_thread::sleep_for(std::chrono::milliseconds(10));
}
void engine_shutdown()
{
	if (!game_window.window.is_valid()) platform::remove_window(game_window.window.get_id());
	vel::content::unload_game();
}
#endif // !defined(SHIPPING)