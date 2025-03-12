#include "Platform.h"
#include "PlatformTypes.h"

namespace vel::platform
{

#ifdef _WIN64
	namespace
	{
		struct window_info
		{
			HWND	hwnd{ nullptr };
			RECT	client_area{ 0, 0, 1920, 1080 };
			RECT	full_screen_area{};
			POINT	top_left{ 0,0 };
			DWORD	style{ WS_VISIBLE };
			bool	is_fullscreen{ false };
			bool	is_closed{ false };
		};

		utl::vector<window_info> windows_list;
		//////////////////////////////////////////////////////////////////
		// TODO: This part will be handled by a free-list container later
		utl::vector<u32> available_slots;
		u32 add_to_windows(window_info info)
		{
			u32 id{ u32_invalid_id };
			if (available_slots.empty())
			{
				id = (u32)windows_list.size();
				windows_list.emplace_back(info);
			}
			else
			{
				id = available_slots.back();
				available_slots.pop_back();
				assert(id != u32_invalid_id);
				windows_list[id] = info;
			}
			return id;
		}

		void remove_window_info(u32 id)
		{
			assert(id < windows_list.size());
			available_slots.emplace_back(id);
		}
		//////////////////////////////////////////////////////////////////
		window_info& get_window_info_from_id(window_id id)
		{
			assert(id < windows_list.size());
			assert(windows_list[id].hwnd);
			return windows_list[id];
		}

		window_info& get_window_info_from_handle(HWND hwnd)
		{
			const window_id id{ (id::id_type)GetWindowLongPtr(hwnd, GWLP_USERDATA) };
			return get_window_info_from_id(id);
		}


		LRESULT CALLBACK internal_window_proc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam)
		{
			window_info* info{ nullptr };
			switch (msg)
			{
			case WM_DESTROY:
				get_window_info_from_handle(hwnd).is_closed = true;
				break;
			case WM_EXITSIZEMOVE:
				info = &get_window_info_from_handle(hwnd);
				break;
			case WM_SIZE:
				if (wparam == SIZE_MAXIMIZED)
				{
					info = &get_window_info_from_handle(hwnd);
				}
				break;
			case WM_SYSCOMMAND:
				if (wparam == SC_RESTORE)
				{
					info = &get_window_info_from_handle(hwnd);
				}
				break;
			default: break;
			}

			if (info)
			{
				assert(info->hwnd);
				GetClientRect(info->hwnd, info->is_fullscreen ? &info->full_screen_area : &info->client_area);
			}

			LONG_PTR long_ptr{ GetWindowLongPtr(hwnd, 0) };
			return long_ptr
				? ((window_proc)long_ptr)(hwnd, msg, wparam, lparam)
				: DefWindowProc(hwnd, msg, wparam, lparam);
		}

		void resize_window(const window_info info, const RECT& area)
		{
			// adjust the window size for the device
			RECT window_rect{ area };
			AdjustWindowRect(&window_rect, info.style, FALSE);

			const s32 width{ window_rect.right - window_rect.left };
			const s32 height{ window_rect.bottom - window_rect.top };

			MoveWindow(info.hwnd, info.top_left.x, info.top_left.y, width, height, true);
		}

		void set_window_fullscreen(window_id id, bool isfullscreen)
		{
			window_info& info{ get_window_info_from_id(id) };
			if (info.is_fullscreen != isfullscreen)
			{
				info.is_fullscreen = isfullscreen;
				if (isfullscreen)
				{
					// store the current window dimensions so they can be restored
					// when switching out of fullscreen state.
					GetClientRect(info.hwnd, &info.client_area);
					RECT rect;
					GetWindowRect(info.hwnd, &rect);
					info.top_left.x = rect.left;
					info.top_left.y = rect.top;
					info.style = 0;
					SetWindowLongPtr(info.hwnd, GWL_STYLE, info.style);
					ShowWindow(info.hwnd, SW_MAXIMIZE);
				}
				else
				{
					info.style = WS_VISIBLE | WS_OVERLAPPEDWINDOW;
					SetWindowLongPtr(info.hwnd, GWL_STYLE, info.style);
					resize_window(info, info.client_area);
					ShowWindow(info.hwnd, SW_SHOWNORMAL);
				}
			}
		}

		bool is_window_fullscreen(window_id id)
		{
			return get_window_info_from_id(id).is_fullscreen;
		}

		void* get_window_handle(window_id id)
		{
			return get_window_info_from_id(id).hwnd;
		}

		void set_window_caption(window_id id, const wchar_t* caption)
		{
			window_info& info{ get_window_info_from_id(id) };
			SetWindowText(info.hwnd, caption);
		}

		math::u32v4 get_window_size(window_id id)
		{
			window_info& info{ get_window_info_from_id(id) };
			RECT area{ info.is_fullscreen ? info.full_screen_area : info.client_area };
			return { (u32)area.left, (u32)area.top, (u32)area.right, (u32)area.bottom };
		}

		void resize_window(window_id id, u32 width, u32 height)
		{
			window_info& info{ get_window_info_from_id(id) };

			// we may also resize while in fullscreen
			RECT area{ info.is_fullscreen ? info.full_screen_area : info.client_area };
			area.bottom = area.top + height;
			area.right = area.left + width;
			resize_window(info, area);
		}

		bool is_window_closed(window_id id)
		{
			return get_window_info_from_id(id).is_closed;
		}

	}	// annonymous

	window create_window(const window_init_info* const init_info)
	{
		window_proc callback{ init_info ? init_info->callback : nullptr };
		window_handle parent{ init_info ? init_info->parent : nullptr };

		// Setup a window class
		WNDCLASSEX wc;
		ZeroMemory(&wc, sizeof(wc));
		wc.cbSize = sizeof(WNDCLASSEX);
		wc.style = CS_HREDRAW | CS_VREDRAW;
		wc.lpfnWndProc = internal_window_proc;
		wc.cbClsExtra = 0;
		wc.cbWndExtra = callback ? sizeof(callback) : 0;
		wc.hInstance = 0;
		wc.hIcon = LoadIcon(NULL, IDI_APPLICATION);
		wc.hCursor = LoadCursor(NULL, IDC_ARROW);
		wc.hbrBackground = CreateSolidBrush(RGB(26, 48, 76));
		wc.lpszMenuName = NULL;
		wc.lpszClassName = L"VelWindow";
		wc.hIconSm = LoadIcon(NULL, IDI_APPLICATION);

		// Register the window class
		RegisterClassEx(&wc);

		// adjust the window size for the correct device size
		window_info info{};
		RECT rc{ info.client_area };

		AdjustWindowRect(&rc, info.style, FALSE);
		const wchar_t* caption{ (init_info && init_info->caption) ? init_info->caption : L"Vel Game" };
		const s32 left{ (init_info && init_info->left) ? init_info->left : info.client_area.left };
		const s32 top{ (init_info && init_info->top) ? init_info->top :  info.client_area.top };
		const s32 width{ (init_info && init_info->width) ? init_info->width : rc.right - rc.left };
		const s32 height{ (init_info && init_info->height) ? init_info->height : rc.bottom - rc.top };

		info.style |= parent ? WS_CHILD : WS_OVERLAPPEDWINDOW;

		// Create an instance of the window class
		info.hwnd = CreateWindowEx(
			0,					// extended style
			wc.lpszClassName,	// window class name
			caption,			// instance title
			info.style,			// window style
			left,
			top,				// initial window position (x , y)
			width,				// width of the window
			height,				// height of the window
			parent,				// handle to the parent of the instance
			NULL,				// handle to menu
			NULL,				// instance of this application
			NULL				// extra creation parameters
		);

		if (info.hwnd)
		{
			SetLastError(0);
			window_id id{ add_to_windows(info) };
			SetWindowLongPtr(info.hwnd, GWLP_USERDATA, (LONG_PTR)id);

			if(callback) SetWindowLongPtr(info.hwnd, 0, (LONG_PTR)callback);
			assert(GetLastError() == 0);

			ShowWindow(info.hwnd, SW_SHOWNORMAL);
			UpdateWindow(info.hwnd);
			return window{ id };
		}

		return {};
	}

	void remove_window(window_id id)
	{
		window_info& info{ get_window_info_from_id(id) };
		DestroyWindow(info.hwnd);
		remove_window_info(id);
	}
#elif _X124xsfas
#else
#error "must implement at least one platform"
#endif

	void window::set_fullscreen(bool isfullscreen) const
	{
		assert(is_valid());
		set_window_fullscreen(_id, isfullscreen);
	}
	bool window::is_fullscreen() const
	{
		assert(is_valid());
		return is_window_fullscreen(_id);
	}
	void* window::handle() const
	{
		assert(is_valid());
		return get_window_handle(_id);
	}
	void window::set_caption(const wchar_t* caption) const
	{
		assert(is_valid());
		set_window_caption(_id, caption);
	}
	const math::u32v4 window::size() const
	{
		assert(is_valid());
		return get_window_size(_id);
	}
	void window::resize(u32 width, u32 height) const
	{
		assert(is_valid());
		resize_window(_id, width, height);
	}
	const u32 window::width() const
	{
		assert(is_valid());
		math::u32v4 s{ size() };
		return s.z - s.x;
	}
	const u32 window::height() const
	{
		math::u32v4 s{ size() };
		return s.w - s.y;
	}
	bool window::is_closed() const
	{
		assert(is_valid());
		return is_window_closed(_id);
	}
}