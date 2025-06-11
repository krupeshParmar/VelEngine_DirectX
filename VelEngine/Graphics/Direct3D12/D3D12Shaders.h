#pragma once
#include "D3D12CommonHeaders.h"

namespace vel::graphics::d3d12::shaders
{
    struct engine_shader {
        enum id : u32 {
            fullscreen_triangle_vs = 0,
            fill_color_ps, 
            post_process_ps,
            grid_frustums_cs,
            light_culling_cs,

            count
        };
    };

    bool initialize();
    void shutdown();

    D3D12_SHADER_BYTECODE get_engine_shader(engine_shader::id id);
}