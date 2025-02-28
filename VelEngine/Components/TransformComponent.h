#pragma once
#include "ComponentsCommon.h"

namespace vel::transform
{
	struct init_info
	{
		f32 position[3]{};
		f32 rotation[4]{};
		f32 scale[3]{1.f,1.f,1.f};
	};

	component create_transform(const init_info& info, game_entity::entity ent);
	void remove_transform(component c);
	bool is_alive(component tra);
}