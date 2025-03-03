#pragma once
#include "..\Components\ComponentsCommon.h"
#include "TransformComponentAPI.h"
#include "ScriptComponentAPI.h"

namespace vel
{
	namespace game_entity
	{
		DEFINE_TYPE_ID(entity_id);
		class entity
		{
		public:
			constexpr entity() : _id{ id::invalid_id } {}
			constexpr explicit entity(entity_id id) : _id{ id } {}
			constexpr entity_id get_id() const { return _id; }
			constexpr bool is_valid() const { return id::is_valid(_id); }

			transform::component transform() const;
			script::component script() const;
		private:
			entity_id _id;
		};
	} // namespace game_entity

	namespace script
	{
		class entity_script : public game_entity::entity
		{
		public:
			entity_script() {};
			virtual ~entity_script() = default;
			virtual void begin_play() {}
			virtual void update(float) {}
		protected:
			constexpr explicit entity_script(game_entity::entity entity)
				: game_entity::entity{ entity.get_id()}{};
		};

		namespace detail
		{
			using script_creator = Scope<entity_script>(*)(game_entity::entity entity);

			u8 register_script(size_t, script_creator);

			template<class script_class>
			Scope<entity_script> create_script(game_entity::entity entity)
			{
				assert(entity.is_valid());
				return CreateScope<entity_script>();
			}

#define REGISTER_SCRIPT(TYPE)													\
		class TYPE;																\
		namespace {																\
		const u8 _reg##TYPE														\
		{ vel::script::detail::register_script(									\
					vel::string_hash()(#TYPE),									\
					&vel::script::detail::create_script<TYPE>) };				\
		}

		} // namespace detail																	

	} // namespace script
}