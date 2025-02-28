#include "TransformComponent.h"
#include "Entity.h"

namespace vel::transform
{
	namespace
	{
		utl::vector<math::v3> positions_list;
		utl::vector<math::v4> rotations_list;
		utl::vector<math::v3> scales_list;
	} // anonymous namespace

	component create_transform(const init_info& info, game_entity::entity ent)
	{
		assert(ent.is_valid());
		const id::id_type entity_index{ id::index(ent.get_id()) };

		if (positions_list.size() > entity_index)
		{
			rotations_list[entity_index] = math::v4(info.rotation);
			positions_list[entity_index] = math::v3(info.position);
			scales_list[entity_index] = math::v3(info.scale);
		}
		else
		{
			assert(positions_list.size() == entity_index);
			rotations_list.emplace_back(info.rotation);
			positions_list.emplace_back(info.position);
			scales_list.emplace_back(info.scale);
		}
		return component(transform_id{ (id::id_type)positions_list.size() - 1 });
	}
	void remove_transform(component c)
	{
		assert(c.is_valid());
	}
	bool is_alive(component tra)
	{
		return false;
	}

	math::v4 
	component::rotation() const
	{
		assert(is_valid());
		return rotations_list[id::index(_id)];
	}
	math::v3 
	component::position() const
	{
		assert(is_valid());
		return positions_list[id::index(_id)];
	}
	math::v3 
	component::scale() const
	{
		assert(is_valid());
		return scales_list[id::index(_id)];
	}
}