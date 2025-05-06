#include "TransformComponent.h"
#include "Entity.h"

namespace vel::transform
{
	namespace
	{
		utl::vector<math::m4x4> to_world;
		utl::vector<math::m4x4> inv_world;
		utl::vector<math::v3>	positions_list;
		utl::vector<math::v4>	rotations_list;
		utl::vector<math::v3>	orientations_list;
		utl::vector<math::v3>	scales_list;
		utl::vector<u8>         has_transform;

		void calculate_transform_matrices(id::id_type index)
		{
			assert(rotations_list.size() >= index);
			assert(positions_list.size() >= index);
			assert(scales_list.size() >= index);

			using namespace DirectX;
			XMVECTOR r{ XMLoadFloat4(&rotations_list[index]) };
			XMVECTOR t{ XMLoadFloat3(&positions_list[index]) };
			XMVECTOR s{ XMLoadFloat3(&scales_list[index]) };

			XMMATRIX world{ XMMatrixAffineTransformation(s, XMQuaternionIdentity(), r, t) };
			XMStoreFloat4x4(&to_world[index], world);

			// NOTE: (F. Luna) Intro to DirectX 12, section 8.2.2
			world.r[3] = XMVectorSet(0.f, 0.f, 0.f, 1.f);
			XMMATRIX inverse_world{ XMMatrixInverse(nullptr, world) };
			XMStoreFloat4x4(&inv_world[index], inverse_world);

			has_transform[index] = 1;
		}

		math::v3 calculate_orientation(math::v4 rotation)
		{
			using namespace DirectX;
			XMVECTOR rotation_quat{ XMLoadFloat4(&rotation) };
			XMVECTOR front{ XMVectorSet(0.f, 0.f, 1.f, 0.f) };
			math::v3 orientation;
			XMStoreFloat3(&orientation, XMVector3Rotate(front, rotation_quat));
			return orientation;
		}

	} // anonymous namespace

	component create(init_info info, game_entity::entity ent)
	{
		assert(ent.is_valid());
		const id::id_type entity_index{ id::index(ent.get_id()) };

		if (positions_list.size() > entity_index)
		{
			math::v4 rotation{ info.rotation };
			rotations_list[entity_index] = rotation;
			orientations_list[entity_index] = calculate_orientation(rotation);
			positions_list[entity_index] = math::v3{ info.position };
			scales_list[entity_index] = math::v3{ info.scale };
			has_transform[entity_index] = 0;
		}
		else
		{
			assert(positions_list.size() == entity_index);
			to_world.emplace_back();
			inv_world.emplace_back();
			rotations_list.emplace_back(info.rotation);
			orientations_list.emplace_back(calculate_orientation(math::v4{ info.rotation }));
			positions_list.emplace_back(info.position);
			scales_list.emplace_back(info.scale);
			has_transform.emplace_back((u8)0);
		}

		// NOTE: each entity has a transform component. Therefor, id's for transform components
		//       are exactly the same as entity ids.

		return component{ transform_id{ ent.get_id()}};
	}
	void remove([[maybe_unused]]component c)
	{
		assert(c.is_valid());
	}
	bool is_alive(component tra)
	{
		return false;
	}

	void get_transform_matrices(const game_entity::entity_id id, math::m4x4& world, math::m4x4& inverse_world)
	{
		assert(game_entity::entity{ id }.is_valid());

		const id::id_type entity_index{ id::index(id) };
		if (!has_transform[entity_index])
		{
			calculate_transform_matrices(entity_index);
		}

		world = to_world[entity_index];
		inverse_world = inv_world[entity_index];
	}


	math::v4 
	component::rotation() const
	{
		assert(is_valid());
		return rotations_list[id::index(_id)];
	}

	math::v3
	component::orientation() const
	{
		assert(is_valid());
		return orientations_list[id::index(_id)];
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