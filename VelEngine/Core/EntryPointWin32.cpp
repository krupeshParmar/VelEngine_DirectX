#ifdef _WIN64
#include "CommonHeaders.h"
#include <filesystem>

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <crtdbg.h>

namespace {
    // TODO: we might want to have an IO utility header/library and move this function in there.
    std::filesystem::path
        set_current_directory_to_executable_path()
    {
        // set the working directory to the executable path
        wchar_t path[MAX_PATH]{};
        const u32 length{ GetModuleFileName(0, &path[0], MAX_PATH) };
        if (!length || GetLastError() == ERROR_INSUFFICIENT_BUFFER) return {};
        std::filesystem::path p{ path };
        std::filesystem::current_path(p.parent_path());
        return std::filesystem::current_path();
    }

}

#ifndef USE_WITH_EDITOR

extern bool engine_intitalize();
extern void engine_update();
extern void engine_shutdown();

int WINAPI WinMain(HINSTANCE , HINSTANCE , PSTR , int )
{
#if _DEBUG
        _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif  // _DEBUG

    set_current_directory_to_executable_path();

    if (engine_intitalize())
    {
        MSG msg{};
        bool is_running{ true };
        while (is_running)
        {
            while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE))
            {
                TranslateMessage(&msg);
                DispatchMessage(&msg);
                is_running &= (msg.message != WM_QUIT);
            }
            engine_update();
        }
    }
    engine_shutdown();
}
#endif  // USE_WITH_EDITOR
#endif  // _WIN64