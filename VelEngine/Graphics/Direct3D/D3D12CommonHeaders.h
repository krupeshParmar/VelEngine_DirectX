#pragma once
#include "CommonHeaders.h"
#include "Graphics/Renderer.h"

#include <dxgi1_6.h>
#include <d3d12.h>
#include <wrl.h>

#pragma comment(lib, "dxgi.lib")
#pragma	comment(lib, "d3d12.lib")

// Assert that COM call to D3D API succeeded
#ifdef _DEBUG
#ifndef DXCall
#define DXCall(x)												\
if(FAILED(x)) {													\
     char line_number[32];					                    \
     sprintf(line_number, "%d", __LINE__);	                    \
     OutputDebugStringA("Error in: ");                          \
     OutputDebugStringA(__FILE__);                              \
     OutputDebugStringA("Line: ");                              \
     OutputDebugStringA(line_number);                           \
     OutputDebugStringA("\n");                                  \
     OutputDebugStringA(#x);                                    \
     OutputDebugStringA("\n");                                  \
     __debugbreak();                                            \
}
#endif // !DXCall
// Sets the name of the COM object and outputs it to the debug console
#define NAME_D3D12_OBJECT(obj, name) obj->SetName(name); OutputDebugString(L"::D3D12 Object Created: "); OutputDebugString(name); OutputDebugString(L"\n");

#else
#ifndef DXCall
#define DXCall(x) x
#endif // !DXCall
#define NAME_D3D12_OBJECT(obj, name)
#endif // _DEBUG
