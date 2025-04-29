#pragma once
#include "..\Common\CommonHeaders.h"
#include "window.h"

namespace vel::platform
{
	struct window_init_info;
	window create_window(const window_init_info* init_info = nullptr);
	void remove_window(window_id id);
}