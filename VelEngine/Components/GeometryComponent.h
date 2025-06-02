#pragma once
#include "ComponentsCommon.h"

namespace vel::geometry {

    struct init_info
    {
        id::id_type     geometry_content_id;
        u32             material_count;
        id::id_type*    material_ids;
    };

    component create(init_info info, game_entity::entity entity);
    void remove(component c);
    void get_render_item_ids(id::id_type *const item_ids, u32 count);

}
