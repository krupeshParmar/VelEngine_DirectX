#include "EntityManager.h"
#include "TransformComponent.h"
#include "ScriptComponent.h"
#include "GeometryComponent.h"

namespace vel::game_entity
{
	namespace
	{
		utl::vector<transform::component>									transforms_list;
		//utl::unordered_map<id::id_type, utl::vector<script::component>>		scripts_list;
		utl::vector<geometry::component>									geometries_list;

		utl::vector<id::generation_type>	generations;
		utl::deque<entity_id>				free_ids;

	}	// annonymous namespace

	entity
	create()
	{
		entity_id id{};
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
			//scripts_list[id::index(id)].emplace_back();
			geometries_list.emplace_back();
		}

		return entity{ id };
	}

	entity 
	create(entity_info info)
	{
		assert(info.transform);		// ALl entity have transform component
		if (!info.transform) return {};
		const entity new_entity =  create();
		entity_id id = new_entity.get_id();
		const id::id_type index{ id::index(id) };

		// create transform component
		assert(!transforms_list[index].is_valid());
		transforms_list[index] = transform::create(*info.transform, new_entity);
		assert(transforms_list[index].get_id() == id);

		if (!transforms_list[index].is_valid()) return {};
		
		// create script component
		if (info.script && info.script->script_creator)
		{
			script::create(*info.script, new_entity);
			//scripts_list[index].push_back(script::create(*info.script, new_entity));
		}

		// Create geometry component
		if (info.geometry)
		{
			assert(!geometries_list[index].is_valid());
			geometries_list[index] = geometry::create(*info.geometry, new_entity);
			assert(geometries_list[index].is_valid());
		}

		return new_entity;
	}

	void 
	remove(entity_id id)
	{
		game_entity::entity entity{ id };
		const id::id_type index{ id::index(id) };
		assert(is_alive(id));
		if (geometries_list[index].is_valid())
		{
			geometry::remove(geometries_list[index]);
			geometries_list[index] = {};
		}

		script::remove_all_scripts_for_entity(entity);
		/*for (int i = 0; i < scripts_list[index].size(); ++i)
		{
			if (!scripts_list[index][i].is_valid())
				continue;
			script::remove(scripts_list[index][i], entity);
			scripts_list[index] = {};
		}*/

		transform::remove(transforms_list[index]);
		transforms_list[index] = {};
		if (generations[index] < id::max_generation)
		{
			free_ids.push_back(id);
		}
	}

	bool 
	is_alive(entity_id id)
	{
		assert(id::is_valid(id));
		const id::id_type index{ id::index(id) };
		assert(index < generations.size());
		return generations[index] == id::generation(id) && transforms_list[index].is_valid();
	}

	bool add_transform(const entity& new_entity,const transform::init_info& info)
	{
		const entity_id id = new_entity.get_id();
		const id::id_type index{ id::index(id) };
		// create transform component
		assert(!transforms_list[index].is_valid());
		transforms_list[index] = transform::create(info, new_entity);
		assert(transforms_list[index].get_id() == id);

		if (!transforms_list[index].is_valid()) return false;
		return true;
	}

	bool add_script(const entity& new_entity,const script::init_info& info)
	{
		const entity_id id = new_entity.get_id();
		const id::id_type index{ id::index(id) };
		// create script component
		if (info.script_creator)
		{
			script::create(info, new_entity);
			//scripts_list[index].push_back(script::create(info, new_entity));
			return true;
		}
		return false;
	}
	bool add_geometry(const entity& new_entity,const geometry::init_info& info)
	{
		const entity_id id = new_entity.get_id();
		const id::id_type index{ id::index(id) };
		// Create geometry component
		assert(!geometries_list[index].is_valid());
		geometries_list[index] = geometry::create(info, new_entity);
		assert(geometries_list[index].is_valid());
		return true;
	}

	transform::component entity::transform() const
	{
		assert(is_alive(this->get_id()));
		return transforms_list[id::index(_id)];
	}

	/*utl::vector<script::component> entity::script() const
	{
		assert(is_alive(this->get_id()));
		return script::get_all_scripts_for_entity(_id);
	}*/

	geometry::component entity::geometry() const
	{
		assert(is_alive(_id));
		return geometries_list[id::index(_id)];
	}
}
