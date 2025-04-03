#pragma comment(lib,"VelEngine.lib")
#define TEST_ENTITY_COMPONENTS 0
#define TEST_WINDOWS 0
#define TEST_RENDERER 1

#if TEST_ENTITY_COMPONENTS
#include "TestEntityComponents.h"
#elif TEST_WINDOWS
#include "WindowTest.h"
#elif TEST_RENDERER
#include "TestRenderer.h"
#else
#error One of the tests needs to be enabled
#endif

#ifdef _WIN64
#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif // WIN32_LEAN_AND_MEAN
#include <Windows.h>
int WINAPI WinMain(HINSTANCE, HINSTANCE, PSTR, int)
{
#if _DEBUG
    _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif  // _DEBUG
    engine_test test{};
    if (test.initialize())
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
            test.run();
        }
    }
    test.shutdown();
    return 0;
}
#else
int main()
{
#if _DEBUG
	_CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
#endif
	engine_test test{};
	if (test.initialize())
	{
		test.run();
	}

	test.shutdown();
}
#endif  // _WIN64