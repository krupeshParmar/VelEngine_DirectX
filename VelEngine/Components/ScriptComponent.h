#pragma once
#include "ComponentsCommon.h"

namespace vel::script
{
	struct init_info
	{
		detail::script_creator script_creator;
	};

	component create(init_info info, game_entity::entity ent);
	void update_scripts(float dt);
	void remove(component c);
}

