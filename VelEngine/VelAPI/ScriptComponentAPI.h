#pragma once
#include "..\Components\ComponentsCommon.h"

namespace vel::script
{
	DEFINE_TYPE_ID(script_id);

	class component final
	{
	public:
		constexpr explicit component(script_id id) : _id{ id } {}
		constexpr component() : _id{ id::invalid_id } {}
		constexpr script_id get_id() const { return _id; }
		constexpr bool is_valid() const { return id::is_valid(_id); }
		component(const component& c)
		{
			_id = c.get_id();
		}

	private:
		script_id _id;
	};
}
