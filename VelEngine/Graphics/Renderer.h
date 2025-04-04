#pragma once
#include "..\Common\CommonHeaders.h"
#include "..\Platform\window.h"

namespace vel::graphics
{
	enum class graphics_platform
	{
		direct3d12 = 0,
		vulkan = 1,
		opengl = 2,
	};

	class surface
	{

	};

	struct render_surface
	{
		platform::window window{};
		surface surface{};
	};

	bool initialize(graphics_platform platform);

	void shutdown();

	void render();
}