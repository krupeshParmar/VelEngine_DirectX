#include "Entity.h"

namespace vel::game_entity
{
	namespace
	{
		utl::vector<id::generation_type> generations;


	}	// annonymous namespace


	entity_id 
	create_game_entity(const entity_info& info)
	{
		assert(info.transform);		// ALl entity have transform component
	}

	void 
	remove_game_entity(entity_id id)
	{
	}

	bool 
	is_alive(entity_id id)
	{
		return false;
	}

}
