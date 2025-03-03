#include "ScriptComponent.h"
#include "Entity.h"

namespace vel::script
{
	namespace
	{
		utl::vector<Scope<entity_script>>	entity_scripts_list;
		utl::vector<id::id_type>			id_mapping;

		utl::vector<id::generation_type>	generations;
		utl::deque<script_id>				free_ids;

		using script_registery = std::unordered_map<size_t, detail::script_creator>;

		script_registery& registery()
		{
			/* NOTE:
			*  we put this static variable in a function because of 
			*  the intialization order of static data. This way, we can
			*  be certain that the data is initialized before accessing it.
			*/
			static script_registery reg;
			return reg;
		}

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
	} // anonymous

	namespace detail
	{
		u8 register_script(size_t tag, script_creator func)
		{
			bool result{ registery().insert(script_registery::value_type{tag,func}).second };
			assert(result);
			return result;
		}

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
		entity_scripts_list.emplace_back(info.script_creator(ent));
		assert(entity_scripts_list.back()->get_id() == ent.get_id());
		const id::id_type index{ (id::id_type)entity_scripts_list.size() };
		id_mapping[id::index(id)] = index;
		return component{};
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
}
