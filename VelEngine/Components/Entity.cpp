#include "Entity.h"
#include "TransformComponent.h"
#include "ScriptComponent.h"

namespace vel::game_entity
{
	namespace
	{
		utl::vector<transform::component>	transforms_list;
		utl::vector<script::component>	scripts_list;

		utl::vector<id::generation_type>	generations;
		utl::deque<entity_id>				free_ids;

	}	// annonymous namespace


	entity 
	create(entity_info info)
	{
		assert(info.transform);		// ALl entity have transform component
		if (!info.transform) return entity{};

		entity_id id;

		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();
			assert(!is_alive(id));
			free_ids.pop_front();
			id = entity_id{ id::new_generation(id) };
			++generations[id::index(id)];
		}
		else
		{
			id = entity_id{ (id::id_type)generations.size() };
			generations.push_back(0);

			// Resize Components
			// Note: we don't call resize(), so the number of memory allocations stays low
			transforms_list.emplace_back();
			scripts_list.emplace_back();
		}

		const entity new_entity{ id };
		const id::id_type index{ id::index(id) };

		// create transform component
		assert(!transforms_list[index].is_valid());
		transforms_list[index] = transform::create(*info.transform, new_entity);

		if (!transforms_list[index].is_valid()) return {};
		
		// create script component
		if (info.script && info.script->script_creator)
		{
			assert(!scripts_list[index].is_valid());
			scripts_list[index] = script::create(*info.script, new_entity);;
			assert(scripts_list[index].is_valid());
		}

		return new_entity;
	}

	void 
	remove(entity_id id)
	{
		const id::id_type index{ id::index(id) };
		assert(is_alive(id));
		if (scripts_list[index].is_valid())
		{
			script::remove(scripts_list[index]);
			scripts_list[index] = {};
		}

		transform::remove(transforms_list[index]);
		transforms_list[index] = {};
		free_ids.push_back(id);
	}

	bool 
	is_alive(entity_id id)
	{
		assert(id::is_valid(id));
		const id::id_type index{ id::index(id) };
		assert(index < generations.size());
		return (generations[index] == id::generation(id) && transforms_list[index].is_valid());
	}

	transform::component
		entity::transform() const
	{
		assert(is_alive(this->get_id()));
		const id::id_type index{ id::index(_id) };
		return transforms_list[index];
	}

	script::component
		entity::script() const
	{
		assert(is_alive(this->get_id()));
		const id::id_type index{ id::index(_id) };
		return scripts_list[index];
	}
}
