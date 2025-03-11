
#ifdef _WIN64

#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // !WIN32_LEAN_AND_MEAN

#include <Windows.h>
#include <crtdbg.h>

#ifndef USE_WITH_EDITOR

extern bool engine_intitalize();
extern void engine_update();
extern void engine_shutdown();

int WINAPI WinMain(HINSTANCE , HINSTANCE , PSTR , int )
{
#if _DEBUG
        _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif  // _DEBUG
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