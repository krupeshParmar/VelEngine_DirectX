#pragma once
#include "D3D12CommonHeaders.h"

namespace vel::graphics::d3d12::core
{

	namespace detail
	{
		void deferred_release(IUnknown* ptr);
	}

	// Function to initialize Direct3D 12
	bool initialize();
	// Function to shutdown Direct3D 12
	void shutdown();
	void render();

	template<typename T>
	constexpr void release(T*& ptr)
	{
		if (ptr)
		{
			ptr->Release();
			ptr = nullptr;
		}
	}

	template<typename T>
	constexpr void deferred_release(T*& ptr)
	{
		if (ptr)
		{
			detail::deferred_release(ptr);
			ptr = nullptr;
		}
	}

	ID3D12Device* const device();
	u32 current_frame_index();
	void set_deferred_releases_flag();

} // namespace vel::graphics::d3d12
