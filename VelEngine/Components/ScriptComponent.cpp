#include "ScriptComponent.h"
#include "Entity.h"
#include "TransformComponent.h"

#define USE_TRANSFORM_CACHE_MAP 0

namespace vel::script
{
	namespace
	{
		utl::vector<Scope<entity_script>>			entity_scripts_list;
		utl::vector<id::id_type>					id_mapping;

		utl::vector<id::generation_type>			generations;
		utl::deque<script_id>						free_ids;
		utl::vector<transform::component_cache>		transform_cache;

#if USE_TRANSFORM_CACHE_MAP
		std::unordered_map<id::id_type, u32>    cache_map;
#endif

		using script_registry = std::unordered_map<size_t, detail::script_creator>;

		script_registry& registery()
		{
			/* NOTE:
			*  we put this static variable in a function because of 
			*  the intialization order of static data. This way, we can
			*  be certain that the data is initialized before accessing it.
			*/
			static script_registry reg;
			return reg;
		}

#ifdef USE_WITH_EDITOR
utl::vector<std::string>&
script_names()
{
	/* NOTE:
			*  we put this static variable in a function because of 
			*  the intialization order of static data. This way, we can
			*  be certain that the data is initialized before accessing it.
			*/
	static utl::vector<std::string> names_list;
	return names_list;
}
#endif

		bool exists(script_id id)
		{
			assert(id::is_valid(id));
			const id::id_type index{ id::index(id) };
			assert(index < generations.size() && id_mapping[index] < entity_scripts_list.size());
			assert(generations[index] == id::generation(id));
			return (generations[index] == id::generation(id)) &&
				entity_scripts_list[id_mapping[index]] &&
				entity_scripts_list[id_mapping[index]]->is_valid();
		}

#if USE_TRANSFORM_CACHE_MAP
		transform::component_cache *const get_chage_ptr(const game_entity::entity *const entity)
		{
			assert(game_entity::is_alive((*entity).get_id()));
			const transform::transform_id id{ (*entity).transform().get_id() };

			u32 index{ u32_invalid_id };
			auto pair = cache_map.try_emplace(id, id::invalid_id);

			// cache_map didn't have an entry for this id, new entry inserted
			if (pair.second)
			{
				index = (u32)transform_cache.size();
				transform_cache.emplace_back();
				transform_cache.back().id = id;
				cache_map[id] = index;
			}
			else
			{
				index = cache_map[id];
			}

			assert(index < transform_cache.size());
			return &transform_cache[index];
		}
#else
		transform::component_cache *const get_cache_ptr(const game_entity::entity *const entity)
		{
			assert(game_entity::is_alive((*entity).get_id()));
			const transform::transform_id id{ (*entity).transform().get_id() };

			for (auto& cache : transform_cache)
			{
				if (cache.id == id)
				{
					return &cache;
				}
			}

			transform_cache.emplace_back();
			transform_cache.back().id = id;

			return &transform_cache.back();
		}
#endif

	} // anonymous

	namespace detail
	{
		u8 register_script(size_t tag, script_creator func)
		{
			bool result{ registery().insert(script_registry::value_type{tag,func}).second };
			assert(result);
			return result;
		}
		script_creator get_script_creator(size_t tag)
		{
			auto script = vel::script::registery().find(tag);
			assert(script != vel::script::registery().end() && script->first == tag);
			return script->second;
		}

#ifdef USE_WITH_EDITOR
u8 add_script_name(const char* name)
{
	script_names().emplace_back(name);
	return true;
}
#endif

	} // detail

	component script::create(init_info info, game_entity::entity ent)
	{
		assert(ent.is_valid());
		assert(info.script_creator);

		script_id id{};
		if (free_ids.size() > id::min_deleted_elements)
		{
			id = free_ids.front();
			assert(!exists(id));
			free_ids.pop_front();
			id = script_id{ id::new_generation(id) };
			++generations[id::index(id)];
		}
		else
		{
			id = script_id{ (id::id_type)id_mapping.size() };
			id_mapping.emplace_back();
			generations.push_back(0);
		}

		assert(id::is_valid(id));
		const id::id_type index{ (id::id_type)entity_scripts_list.size()};		// take the index first before adding any new script
		entity_scripts_list.emplace_back(info.script_creator(ent));
		assert(entity_scripts_list.back()->get_id() == ent.get_id());
		id_mapping[id::index(id)] = index;
		return component{ id };
	}

	void update(float dt)
	{
		for (auto& ptr : entity_scripts_list)
		{
			ptr->update(dt);
		}
		if (transform_cache.size())
		{
			transform::update(transform_cache.data(), (u32)transform_cache.size());
			transform_cache.clear();

#if USE_TRANSFORM_CACHE_MAP
			cache_map.clear();
#endif
		}
	}

	void script::remove(component c)
	{
		assert(c.is_valid() && exists(c.get_id()));
		const script_id id{ c.get_id() };
		const id::id_type index{ id_mapping[id::index(id)] };
		const script_id last_id{ entity_scripts_list.back()->script().get_id() };
		utl::erase_unordered(entity_scripts_list, index);
		id_mapping[id::index(last_id)] = index;
		id_mapping[id::index(id)] = id::invalid_id;
	}

	void entity_script::set_rotation(const game_entity::entity *const entity, math::v4 rotation_quaternion)
	{
		transform::component_cache& cache{ *get_cache_ptr(entity) };
		cache.flags |= transform::component_flags::rotation;
		cache.rotation = rotation_quaternion;
	}

	void entity_script::set_orientation(const game_entity::entity *const entity, math::v3 orientation_vector)
	{
		transform::component_cache& cache{ *get_cache_ptr(entity) };
		cache.flags |= transform::component_flags::orientation;
		cache.orientation = orientation_vector;
	}

	void entity_script::set_position(const game_entity::entity *const entity, math::v3 position)
	{
		transform::component_cache& cache{ *get_cache_ptr(entity) };
		cache.flags |= transform::component_flags::position;
		cache.position = position;
	}

	void entity_script::set_scale(const game_entity::entity *const entity, math::v3 scale)
	{
		transform::component_cache& cache{ *get_cache_ptr(entity) };
		cache.flags |= transform::component_flags::scale;
		cache.scale = scale;
	}
}	// script

#ifdef USE_WITH_EDITOR
#include <atlsafe.h>

extern "C" __declspec(dllexport)
LPSAFEARRAY
get_script_names()
{
	const u32 size{ (u32)vel::script::script_names().size() };
	if (!size) return nullptr;
	CComSafeArray<BSTR> names_list(size);
	for (u32 i{ 0 }; i < size; ++i)
	{
		names_list.SetAt(i, A2BSTR(vel::script::script_names()[i].c_str()), false);
	}
	return names_list.Detach();
}
#endif
