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

	component create(init_info info, game_entity::entity ent);
	void remove(component c);
	bool is_alive(component tra);
	void get_transform_matrices(const game_entity::entity_id id, math::m4x4& world, math::m4x4& inverse_world);
}