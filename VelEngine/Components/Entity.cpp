#include "Entity.h"
#include "TransformCompnent.h"

namespace vel::game_entity
{
	namespace
	{
		utl::vector<transform::component>	transforms_list;
		utl::vector<id::generation_type>	generations;
		utl::deque<entity_id>				free_ids;

	}	// annonymous namespace


	entity 
	create_game_entity(const entity_info& info)
	{
		assert(info.transform);		// ALl entity have transform component
		if (!info.transform) return entity{};

		entity_id id;

		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();
			assert(!is_alive(entity{ id }));
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
		}

		const entity new_entity{ id };
		const id::id_type index{ id::index(id) };

		// create transform component
		assert(!transforms_list[index].is_valid());
		transforms_list[index] = transform::create_transform(*info.transform, new_entity);

		if (!transforms_list[index].is_valid()) return entity{};

		return new_entity;
	}

	void 
	remove_game_entity(entity en)
	{
		const entity_id id{ en.get_id() };
		const id::id_type index{ id::index(id) };
		assert(is_alive(en));
		if (!is_alive(en))
		{
			free_ids.push_back(id);
		}
	}

	bool 
	is_alive(entity en)
	{
		assert(en.is_valid());
		const entity_id id{ en.get_id() };
		const id::id_type index{ id::index(id) };
		assert(index < generations.size());
		assert(generations[index] == id::generation(id));
		return (generations[index] == id::generation(id));
	}

}
