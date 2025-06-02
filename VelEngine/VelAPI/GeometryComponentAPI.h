#pragma once
#include "../Components/ComponentsCommon.h"

namespace vel::geometry {

    DEFINE_TYPE_ID(geometry_id);

    class component final
    {
    public:
        constexpr explicit component(geometry_id id) : _id{ id } {}
        constexpr component() : _id{ id::invalid_id } {}
        constexpr geometry_id get_id() const { return _id; }
        constexpr bool is_valid() const { return id::is_valid(_id); }

    private:
        geometry_id _id;
    };
}