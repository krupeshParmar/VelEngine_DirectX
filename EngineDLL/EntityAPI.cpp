#include "Common.h"
#include "Components/EntityManager.h"
#include "Components/TransformComponent.h"
#include "Components/ScriptComponent.h"
#include "Components/GeometryComponent.h"

using namespace vel;

namespace
{
	enum class component_type_id : u32 {
		transform = 0,
		script = 1,
		geometry = 2,
	};

	struct component_descriptor
	{
		int type_id;
		void* data;
	};

	struct transform_component
	{
		f32 position[3];
		f32 rotation[3];	// Euler Angles
		f32 scale[3];

		transform::init_info to_init_info()
		{
			using namespace DirectX;

			transform::init_info info{};
			memcpy(&info.position[0], &position[0], sizeof(position));
			memcpy(&info.scale[0], &scale[0], sizeof(position));
			XMFLOAT3A rot{ &rotation[0] };
			XMVECTOR quat{ XMQuaternionRotationRollPitchYawFromVector(XMLoadFloat3A(&rot)) };
			// quaternion_rotation_roll_pitch_yaw_from_vector()
			XMFLOAT4 rot_quat{};
			XMStoreFloat4(&rot_quat, quat);
			memcpy(&info.rotation[0], &rot_quat.x, sizeof(info.rotation));
			return info;
		}
	};

	struct script_component
	{
		uint64_t script_name_hash;
		script::detail::script_creator script_creator;
		script::init_info to_init_info()
		{
			script::init_info info{};
			info.script_name_hash = script_name_hash;
			info.script_creator = script_creator;
			return info;
		}
	};

	struct geometry_component
	{
		id::id_type     geometry_content_id;
		u32             material_count;
		id::id_type*    material_ids;

		geometry::init_info to_init_info()
		{
			geometry::init_info info{};
			info.geometry_content_id = geometry_content_id;
			info.material_count = material_count;
			info.material_ids = material_ids;
			return info;
		}
	};

	struct game_entity_descriptor
	{
		component_descriptor* components_list;
		int component_count;
	};

	game_entity::entity entity_from_id(id::id_type id)
	{
		return game_entity::entity{ game_entity::entity_id{id} };
	}
} // anonymous namespace

VEL_EDITOR_API id::id_type CreateGameEntity(game_entity_descriptor* e)
{
	assert(e);
	game_entity_descriptor& desc{ *e };
	vel::game_entity::entity entity = game_entity::create();
	for (int i = 0; i < desc.component_count; ++i)
	{
		const auto& comp = desc.components_list[i];
		switch (static_cast<component_type_id>(comp.type_id))
		{
		case component_type_id::transform:
		{
			transform::init_info transform = reinterpret_cast<transform_component*>(comp.data)->to_init_info();
			game_entity::add_transform(entity, transform);
			break;
		}
		case component_type_id::script:
		{
			if (comp.data == nullptr)
				break;
			script::init_info script{ reinterpret_cast<script_component*>(comp.data)->to_init_info() };
			game_entity::add_script(entity, script);
			break;
		}
		case component_type_id::geometry:
		{
			geometry::init_info geometry = reinterpret_cast<geometry_component*>(comp.data)->to_init_info();
			game_entity::add_geometry(entity, geometry);
			break;
		}
		default:
			break;
		}
	}
	/*transform::init_info transform_info{ desc.transform.to_init_info() };
	script::init_info script_info{ desc.script.to_init_info() };
	geometry::init_info geometry_info{ desc.geometry.to_init_info() };
	game_entity::entity_info entity_info
	{
		&transform_info,
		&script_info,
		id::is_valid(desc.geometry.geometry_content_id) ? &geometry_info : nullptr,
	};*/
	return entity.get_id();
}

VEL_EDITOR_API void
RemoveGameEntity(id::id_type id)
{
	assert(id::is_valid(id));
	game_entity::remove(game_entity::entity_id{ id });
}