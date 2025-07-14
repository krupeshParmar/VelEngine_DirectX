#include "ScriptComponent.h"
#include "EntityManager.h"
#include "TransformComponent.h"

#define USE_TRANSFORM_CACHE_MAP 0

namespace vel::script
{
	namespace
	{
		utl::vector<Scope<entity_script>>			entity_scripts_list;
		utl::vector<utl::vector<id::id_type>>		id_mapping;

		utl::vector<id::generation_type>			generations;
		utl::deque<script_id>						free_ids;
		utl::vector<transform::component_cache>		transform_cache;

#if USE_TRANSFORM_CACHE_MAP
		std::unordered_map<id::id_type, u32>    cache_map;
#endif

		using script_registry = std::unordered_map<uint64_t, detail::script_creator>;

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
#if _DEBUG
		bool exists(script_id id)
		{
			assert(id::is_valid(id));
			const id::id_type index = id::index(id);
			if (index >= generations.size() || generations[index] != id::generation(id))
				return false;

			for (const auto& script_ptr : entity_scripts_list)
			{
				if (script_ptr && script_ptr->get_script_id() == id && script_ptr->is_valid())
					return true;
			}
			return false;
		}
#endif
#if USE_TRANSFORM_CACHE_MAP
		transform::component_cache *const get_cache_ptr(const game_entity::entity *const entity)
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
		u8 register_script(uint64_t tag, script_creator func)
		{
			bool result{ registery().insert(script_registry::value_type{tag,func}).second };
			assert(result);
			return result;
		}
		script_creator get_script_creator(uint64_t tag)
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
			id = script_id{ (id::id_type)generations.size() };
			generations.push_back(0);
		}

		assert(id::is_valid(id));

		const id::id_type entity_index = id::index(ent.get_id());
		if (id_mapping.size() <= entity_index)
			id_mapping.resize(entity_index + 1);

		const id::id_type index{ (id::id_type)entity_scripts_list.size()};		// take the index first before adding any new script
		entity_scripts_list.emplace_back(info.script_creator(ent));
		entity_scripts_list.back().get()->set_script_id(id);
		assert(entity_scripts_list.back()->get_id() == ent.get_id());

		id_mapping[entity_index].push_back(index);
		return component{ id };
	}

	void begin_play()
	{
		for (const auto& scripts_list : id_mapping)
		{
			for (const auto& index : scripts_list)
			{
				entity_scripts_list[index]->begin_play();
			}
		}
	}

	void update(f32 dt)
	{
		for (const auto& scripts_list : id_mapping)
		{
			for (const auto& index : scripts_list)
			{
				entity_scripts_list[index]->update(dt);
			}
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

	void script::remove(script_id id)
	{
		assert(id::is_valid(id));

		// Find index of script in the entity_scripts_list
		const id::id_type script_index = [&]() -> id::id_type {
			for (id::id_type i = 0; i < entity_scripts_list.size(); ++i)
			{
				if (entity_scripts_list[i] && entity_scripts_list[i]->get_script_id() == id)
					return i;
			}
			assert(false && "Script ID not found in entity_scripts_list");
			return id::invalid_id;
			}();

		const id::id_type entity_index = id::index(id);

		// Remove script_index from the mapping of the removed entity
		if (entity_index < id_mapping.size())
		{
			auto& vec = id_mapping[entity_index];
			auto it = std::find(vec.begin(), vec.end(), script_index);
			if (it != vec.end())
				vec.erase(it);
		}

		// Handle unordered erase (swap last -> removed slot)
		const id::id_type last_index = (id::id_type)entity_scripts_list.size() - 1;

		if (script_index != last_index)
		{
			// We'll be moving this last script down
			const game_entity::entity_id moved_entity_id = entity_scripts_list[last_index]->get_id(); // entity id
			const id::id_type moved_entity_index = id::index(moved_entity_id);

			// Update moved script’s owner mapping to point to new index (script_index)
			if (moved_entity_index < id_mapping.size())
			{
				auto& moved_vec = id_mapping[moved_entity_index];
				auto it = std::find(moved_vec.begin(), moved_vec.end(), last_index);
				if (it != moved_vec.end())
				{
					*it = script_index;
				}
				else
				{
					assert(false && "Moved script index not found in id_mapping");
				}
			}
		}

		// Now erase from entity_scripts_list
		utl::erase_unordered(entity_scripts_list, script_index);

		// Recycle script_id
		if (generations[id::index(id)] < id::max_generation)
		{
			free_ids.push_back(id);
		}
	}

	void remove_all_scripts_for_entity(game_entity::entity entity)
	{
		const id::id_type index = id::index(entity.get_id());
		if (index >= id_mapping.size())
			return;

		auto& vec = id_mapping[index];
		for (id::id_type script_index : vec)
		{
			if (script_index < entity_scripts_list.size() && entity_scripts_list[script_index])
			{
				const script_id sid = entity_scripts_list[script_index]->get_script_id();
				script::remove(sid);
			}
		}
		vec.clear();
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

	utl::vector<entity_script*> get_scripts(game_entity::entity ent)
	{
		utl::vector<entity_script*> result;
		const id::id_type index = id::index(ent.get_id());
		if (index < id_mapping.size())
		{
			for (id::id_type script_idx : id_mapping[index])
			{
				if (script_idx < entity_scripts_list.size())
					result.push_back(entity_scripts_list[script_idx].get());
			}
		}
		return result;
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
