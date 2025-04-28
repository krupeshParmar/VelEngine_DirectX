#pragma once
#include "D3D12CommonHeaders.h"

namespace vel::graphics::d3d12 {
	class descriptor_heap;
}

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

	id3d12_device* const device();
	descriptor_heap& rtv_heap();
	descriptor_heap& dsv_heap();
	descriptor_heap& srv_heap();
	descriptor_heap& uav_heap();
	DXGI_FORMAT default_render_target_format();
	u32 current_frame_index();
	void set_deferred_releases_flag();

	surface create_surface(platform::window window);
	void remove_surface(surface_id id);
	void resize_surface(surface_id id, u32, u32);
	u32 surface_width(surface_id id);
	u32 surface_height(surface_id id);
	void render_surface(surface_id id);

} // namespace vel::graphics::d3d12
