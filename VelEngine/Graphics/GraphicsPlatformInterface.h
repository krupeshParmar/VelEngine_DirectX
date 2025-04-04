#pragma once

#include "..\Common\CommonHeaders.h"
#include "Renderer.h"

namespace vel::graphics
{
	struct platform_interface
	{
		bool(*initialize)(void);
		void(*shutdown)(void);
		void(*render)(void);
	};
}