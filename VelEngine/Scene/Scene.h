#pragma once
#include "../Common/CommonHeaders.h"
namespace vel
{
	class scene
	{
	public:
		scene();
		~scene();
		void on_update_runtime();
		void on_update_editor();
	};
}