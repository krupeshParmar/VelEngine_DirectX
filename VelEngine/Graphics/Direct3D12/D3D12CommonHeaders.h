#pragma once
#include "CommonHeaders.h"
#include "Graphics/Renderer.h"
#include "Platform/Window.h"

// Skip definition of min/max macros in windows.h
#ifndef NOMINMAX
#define NOMINMAX
#endif // !NOMINMAX

#include <dxgi1_6.h>
#include <d3d12.h>
#include <wrl.h>
#include <mutex>

#pragma comment(lib, "dxgi.lib")
#pragma	comment(lib, "d3d12.lib")

namespace vel::graphics::d3d12
{
	constexpr u32 frame_buffer_count{ 3 }; // number of back buffers
    using id3d12_device = ID3D12Device8;
    using id3d12_graphics_command_list = ID3D12GraphicsCommandList6;
}

// Assert that COM call to D3D API succeeded
#ifdef _DEBUG
#ifndef DXCall
#define DXCall(x)												\
if(FAILED(x)) {													\
     char line_number[32];					                    \
     sprintf_s(line_number, "%d", __LINE__);	                \
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
#define NAME_D3D12_OBJECT_INDEXED(obj, indx, name)              \
{                                                               \
wchar_t name_buffer[128];                                       \
if(swprintf_s(name_buffer, L"%s[%u]", name, indx) > 0){         \
    obj->SetName(name_buffer);                                  \
    OutputDebugString(L"::D3D12 Object Created: ");             \
    OutputDebugString(name_buffer);                             \
    OutputDebugString(L"\n");                                   \
}}

#else
#ifndef DXCall
#define DXCall(x) x
#endif // !DXCall
#define NAME_D3D12_OBJECT(obj, name)
#define NAME_D3D12_OBJECT_INDEXED(obj, indx, name)
#endif // _DEBUG

#include "D3D12Helpers.h"
#include "D3D12Resources.h"
