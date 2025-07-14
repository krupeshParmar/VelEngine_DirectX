#pragma once
#include "../Components/ComponentsCommon.h"
#include "TransformComponentAPI.h"
#include "ScriptComponentAPI.h"
#include "GeometryComponentAPI.h"

namespace vel::game_entity
{
	DEFINE_TYPE_ID(entity_id);
	class entity
	{
	public:
		constexpr entity() : _id{ id::invalid_id } {}
		constexpr explicit entity(entity_id id) : _id{ id } {}
		[[nodiscard]] constexpr entity_id get_id() const { return _id; }
		[[nodiscard]] constexpr bool is_valid() const { return id::is_valid(_id); }

		[[nodiscard]] transform::component transform() const;
		[[nodiscard]] utl::vector<script::component> script() const { return {};};
		[[nodiscard]] geometry::component geometry() const;

		[[nodiscard]] math::v4 rotation() const { return transform().rotation(); }
		[[nodiscard]] math::v3 orientation() const { return transform().orientation(); }
		[[nodiscard]] math::v3 position() const { return transform().position(); }
		[[nodiscard]] math::v3 scale() const { return transform().scale(); }
	private:
		entity_id _id;
	};
} // namespace game_entity

namespace vel::script
{
	class entity_script : public game_entity::entity
	{
	public:
		virtual ~entity_script() = default;
		virtual void begin_play() {};
		virtual void update(f32) {};
	public:
		constexpr explicit entity_script(game_entity::entity entity)
			: game_entity::entity{ entity.get_id() }, _script_id(id::invalid_id) {};

		void set_script_id(script_id id) { _script_id = id; }
		script_id get_script_id() const { return _script_id; }
		void set_rotation(math::v4 rotation_quaternion) const { set_rotation(this, rotation_quaternion); }
		void set_orientation(math::v3 orientation_vector) const { set_orientation(this, orientation_vector); }
		void set_position(math::v3 position) const { set_position(this, position); }
		void set_scale(math::v3 scale) const { set_scale(this, scale); }
		utl::vector<entity_script*> get_scripts(game_entity::entity ent);

		static void set_rotation(const game_entity::entity *const entity, math::v4 rotation_quaternion);
		static void set_orientation(const game_entity::entity *const entity, math::v3 orientation_vector);
		static void set_position(const game_entity::entity *const entity, math::v3 position);
		static void set_scale(const game_entity::entity *const entity, math::v3 scale);

		template<typename T>
		static bool has_script(game_entity::entity ent)
		{
			auto scripts = get_scripts(ent);
			for (auto* script : scripts)
			{
				if (dynamic_cast<T*>(script))
					return true;
			}
			return false;
		}

	private:
		script_id _script_id;
	};

	namespace detail
	{
		using script_creator = Scope<entity_script>(*)(game_entity::entity entity);

		u8 register_script(size_t, script_creator);

#ifdef USE_WITH_EDITOR
		extern "C" __declspec(dllexport)
#endif
			script_creator get_script_creator(size_t tag);

		template<class script_class>
		Scope<entity_script> create_script(game_entity::entity entity)
		{
			assert(entity.is_valid());
			return CreateScope<script_class>(entity);
		}

#ifdef USE_WITH_EDITOR
		u8 add_script_name(const char* name);

#define REGISTER_SCRIPT(TYPE)													\
		namespace {																\
		const u8 _reg_##TYPE													\
		{ vel::script::detail::register_script(									\
					vel::string_hash()(#TYPE),									\
					&vel::script::detail::create_script<TYPE>) };				\
		const u8 _name_##TYPE													\
		{ vel::script::detail::add_script_name(#TYPE) };						\
		}
#else
#define REGISTER_SCRIPT(TYPE)													\
		namespace {																\
		const u8 _reg##TYPE														\
		{ vel::script::detail::register_script(									\
					vel::string_hash()(#TYPE),									\
					&vel::script::detail::create_script<TYPE>) };				\
		}
#endif

	} // namespace detail																	

} // namespace script
