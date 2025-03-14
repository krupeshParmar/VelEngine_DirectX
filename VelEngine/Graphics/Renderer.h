#pragma once
#include "..\Common\CommonHeaders.h"
#include "..\Platform\window.h"

namespace vel::graphics
{
	class surface
	{

	};

	struct render_surface
	{
		platform::window window{};
		surface surface{};
	};
}