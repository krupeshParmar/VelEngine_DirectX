#pragma once

namespace vel::graphics
{
	struct platform_interface;

	namespace d3d12
	{
		void get_platform_interface(platform_interface& interface);
		bool initialize();
		void shutdown();
	} // namespace d3d12
}