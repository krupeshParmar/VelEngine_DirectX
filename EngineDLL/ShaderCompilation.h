#pragma once
#include "CommonHeaders.h"
#include "Graphics/Renderer.h"

struct shader_file_info
{
    const char*         file_name;
    const char*         function;
    vel::graphics::shader_type::type   type;
};

vel::Scope<u8[]> compile_shader(shader_file_info info, u8* code, u32 code_size, vel::utl::vector<std::wstring>& extra_args,
    bool include_errors_and_disassembly = false);
vel::Scope<u8[]> compile_shader(shader_file_info info, const char* file_path, vel::utl::vector<std::wstring>& extra_args,
    bool include_errors_and_disassembly = false); 
bool compile_shaders();