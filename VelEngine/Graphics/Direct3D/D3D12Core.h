#pragma once
#include "D3D12CommonHeaders.h"

namespace vel::graphics::d3d12::core
{
	// Function to initialize Direct3D 12
	bool initialize();
	// Function to shutdown Direct3D 12
	void shutdown();

	template<typename T>
	constexpr void release(T*& ptr)
	{
		if (ptr)
		{
			ptr->Release();
			ptr = nullptr;
		}
	}

} // namespace vel::graphics::d3d12
