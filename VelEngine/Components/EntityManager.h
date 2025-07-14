#pragma once

#include "ComponentsCommon.h"

namespace vel
{
#define INIT_INFO(component) namespace component {struct init_info;}
	INIT_INFO(transform);
	INIT_INFO(script)
	INIT_INFO(geometry);
#undef INIT_INFO

	namespace game_entity
	{
		struct entity_info
		{
			transform::init_info* transform{ nullptr };
			script::init_info* script{ nullptr };
			geometry::init_info* geometry{ nullptr };
		};

		entity create();
		entity create(entity_info info);
		void remove(entity_id id);
		bool is_alive(entity_id id);

		// componets
		bool add_transform(const entity& new_entity,const transform::init_info&);
		bool add_script(const entity& new_entity,const script::init_info&);
		bool add_geometry(const entity& new_entity,const geometry::init_info&);
	}
}