#pragma once
#include "ComponentsCommon.h"

namespace vel::script
{
	struct init_info
	{
		uint64_t script_name_hash;
		detail::script_creator script_creator;
	};

	component create(init_info info, game_entity::entity ent);
	void begin_play();
	void update(f32 dt);
	void remove(script_id id);
	void remove_all_scripts_for_entity(game_entity::entity entity);
}

