#include "Renderer.h"
#include "GraphicsPlatformInterface.h"
#include "Direct3D/D3D12Interface.h"
namespace vel::graphics
{
	namespace {
		platform_interface gfx{};

		bool set_platform_interface(graphics_platform platform)
		{
			switch (platform)
			{
			case graphics_platform::direct3d12:
			{
				d3d12::get_platform_interface(gfx);
			}
				return true;

			case graphics_platform::vulkan:

			case graphics_platform::opengl:

			default:
				
				return false;
			}
		}

	} // annonymous namespace

	bool initialize(graphics_platform platform)
	{
		return set_platform_interface(platform) && gfx.initialize();
	}

	void shutdown()
	{
		gfx.shutdown();
	}
}