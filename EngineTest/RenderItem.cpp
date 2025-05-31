#include <filesystem>
#include "CommonHeaders.h"
#include "Content/ContentToEngine.h"
#include "ShaderCompilation.h"
#include "Components/Entity.h"
#include "Graphics/Renderer.h"
#include "../ContentTools/Geometry.h"

using namespace vel;

game_entity::entity create_one_game_entity(math::v3 position, math::v3 rotation, const char* script_name);
void remove_game_entity(game_entity::entity_id id);

bool read_file(std::filesystem::path, Scope<u8[]>&, u64&);

namespace {

    id::id_type sword_model_id{ id::invalid_id };
    id::id_type maria_model_id{ id::invalid_id };
    id::id_type strip_club_model_id{ id::invalid_id };
    
    id::id_type sword_item_id{ id::invalid_id };
    id::id_type maria_item_id{ id::invalid_id };
    id::id_type strip_club_item_id{ id::invalid_id };

    game_entity::entity_id sword_entity_id{ id::invalid_id };
    game_entity::entity_id maria_entity_id{ id::invalid_id };
    game_entity::entity_id strip_club_entity_id{ id::invalid_id };
    struct texture_usage {
            enum usage : u32 {
            ambient_occlusion = 0,
            base_color,
            emissive,
            metal_rough,
            normal,

            count
        };
    };

    id::id_type texture_ids[texture_usage::count];

    id::id_type vs_id{ id::invalid_id };
    id::id_type ps_id{ id::invalid_id };
    id::id_type textured_ps_id{ id::invalid_id };
    id::id_type default_mtl_id{ id::invalid_id };
    id::id_type maria_mtl_id{ id::invalid_id };

    std::unordered_map<id::id_type, game_entity::entity_id> render_item_entity_map;

    [[nodiscard]] id::id_type load_model(const char* path)
    {
        Scope<u8[]> model;
        u64 size{ 0 };
        read_file(path, model, size);

        const id::id_type model_id{ content::create_resource(model.get(), content::asset_type::mesh) };
        assert(id::is_valid(model_id));
        return model_id;
    }

    [[nodiscard]] id::id_type load_texture(const char* path)
    {
        // load test texture
        std::unique_ptr<u8[]> texture;
        u64 size{ 0 };
        read_file(path, texture, size);

        const id::id_type texture_id{ content::create_resource(texture.get(), content::asset_type::texture) };
        assert(id::is_valid(texture_id));
        return texture_id;
    }

    void load_shaders()
    {
        // Let's say our material uses a vertex shader and a pixel shader.
        shader_file_info info{};
        info.file_name = "TestShader.hlsl";
        info.function = "TestShaderVS";
        info.type = shader_type::vertex;

        const char* shader_path{ "..\\..\\enginetest\\" };

        std::wstring defines[]{ L"ELEMENTS_TYPE=1", L"ELEMENTS_TYPE=3" };
        utl::vector<u32> keys;
        keys.emplace_back(tools::elements::elements_type::static_normal);
        keys.emplace_back(tools::elements::elements_type::static_normal_texture);

        utl::vector<std::wstring> extra_args{};
        utl::vector<Scope<u8[]>> vertex_shaders;
        utl::vector<const u8*> vertex_shader_pointers;
        for (u32 i{ 0 }; i < _countof(defines); ++i)
        {
            extra_args.clear();
            extra_args.emplace_back(L"-D");
            extra_args.emplace_back(defines[i]);
            vertex_shaders.emplace_back(std::move(compile_shader(info, shader_path, extra_args)));
            assert(vertex_shaders.back().get());
            vertex_shader_pointers.emplace_back(vertex_shaders.back().get());
        }

        extra_args.clear();

        info.function = "TestShaderPS";
        info.type = shader_type::pixel;
        utl::vector<Scope<u8[]>> pixel_shaders;

        pixel_shaders.emplace_back(compile_shader(info, shader_path, extra_args));
        assert(pixel_shaders.back().get());

        defines[0] = L"TEXTURED_MTL=1";
        extra_args.emplace_back(L"-D");
        extra_args.emplace_back(defines[0]);

        pixel_shaders.emplace_back(compile_shader(info, shader_path, extra_args));
        assert(pixel_shaders.back().get());

        vs_id = content::add_shader_group(vertex_shader_pointers.data(), (u32)vertex_shader_pointers.size(), keys.data());

        const u8* pixel_shader_pointers[]{ pixel_shaders[0].get() };
        ps_id = content::add_shader_group(pixel_shader_pointers, 1, &u32_invalid_id);

        pixel_shader_pointers[0] = pixel_shaders[1].get();
        textured_ps_id = content::add_shader_group(pixel_shader_pointers, 1, &u32_invalid_id);
    }
    void create_material()
    {
        assert(id::is_valid(vs_id) && id::is_valid(ps_id));
        graphics::material_init_info info{};
        info.shader_ids[graphics::shader_type::vertex] = vs_id;
        info.shader_ids[graphics::shader_type::pixel] = ps_id;
        info.type = graphics::material_type::opaque;
        default_mtl_id = content::create_resource(&info, content::asset_type::material);

        info.shader_ids[graphics::shader_type::pixel] = textured_ps_id;
        info.texture_count = texture_usage::count;
        info.texture_ids = &texture_ids[0];
        maria_mtl_id = content::create_resource(&info, content::asset_type::material);
    }

    void remove_item(game_entity::entity_id entity_id, id::id_type item_id, id::id_type model_id)
    {
        if (id::is_valid(item_id))
        {
            graphics::remove_render_item(item_id);
            auto pair = render_item_entity_map.find(item_id);
            if (pair != render_item_entity_map.end())
            {
                remove_game_entity(pair->second);
            }

            if (id::is_valid(model_id))
            {
                content::destroy_resource(model_id, content::asset_type::mesh);
            }
        }
    }

} // anonymous namespace

void
create_render_items()
{
    assert(std::filesystem::exists("..\\..\\x64\\stripclub_interior2.model"));
    assert(std::filesystem::exists("..\\..\\x64\\sword.model"));
    assert(std::filesystem::exists("..\\..\\x64\\maria.model"));

    memset(&texture_ids[0], 0xff, sizeof(id::id_type) * _countof(texture_ids));

        std::thread threads[]{
            std::thread{ [] { texture_ids[texture_usage::ambient_occlusion] = load_texture("..\\..\\x64\\ambient_occlusion.texture"); }},
            std::thread{ [] { texture_ids[texture_usage::base_color] = load_texture("..\\..\\x64\\base_color.texture"); }},
            std::thread{ [] { texture_ids[texture_usage::emissive] = load_texture("..\\..\\x64\\emissive.texture"); }},
            std::thread{ [] { texture_ids[texture_usage::metal_rough] = load_texture("..\\..\\x64\\metal_rough.texture"); }},
            std::thread{ [] { texture_ids[texture_usage::normal] = load_texture("..\\..\\x64\\normal.texture"); }},

            std::thread{ [] { strip_club_model_id = load_model("..\\..\\x64\\stripclub_interior2.model"); } },
            std::thread{ [] { sword_model_id = load_model("..\\..\\x64\\sword.model"); } },
            std::thread{ [] { maria_model_id = load_model("..\\..\\x64\\maria.model"); } },
            std::thread{ [] { load_shaders(); } },
    };

    for (auto& t : threads)
    {
        t.join();
    }

    strip_club_entity_id = create_one_game_entity({ -5.f, -1.f, 0.f }, {}, nullptr).get_id();
    sword_entity_id = create_one_game_entity({ 2.f, 1.3f, -6.6f }, {}, "wibbly_wobbly_script").get_id();
    maria_entity_id = create_one_game_entity({ 0.f, 0.f, -11.6f }, {}, nullptr).get_id();

    // NOTE: we need shaders to be ready before creating materials
    create_material();
    id::id_type materials[]{ default_mtl_id };
    id::id_type maria_materials[]{ maria_mtl_id, maria_mtl_id };

    strip_club_item_id = graphics::add_render_item(strip_club_entity_id, strip_club_model_id, _countof(materials), &materials[0]);
    sword_item_id = graphics::add_render_item(sword_entity_id, sword_model_id, _countof(materials), &materials[0]);
    maria_item_id = graphics::add_render_item(maria_entity_id, maria_model_id, _countof(maria_materials), &maria_materials[0]);

    render_item_entity_map[strip_club_item_id] = strip_club_entity_id;
    render_item_entity_map[sword_item_id] = sword_entity_id;
    render_item_entity_map[maria_item_id] = maria_entity_id;
}

void
destroy_render_items()
{
    remove_item(strip_club_entity_id, strip_club_item_id, strip_club_model_id);
    remove_item(sword_entity_id, sword_item_id, sword_model_id);
    remove_item(maria_entity_id, maria_item_id, maria_model_id);

    // remove material
    if (id::is_valid(default_mtl_id))
    {
        content::destroy_resource(default_mtl_id, content::asset_type::material);
    }

    if (id::is_valid(maria_mtl_id))
    {
        content::destroy_resource(maria_mtl_id, content::asset_type::material);
    }

    // remove textures
    for (id::id_type id : texture_ids)
    {
        if (id::is_valid(id))
        {
            content::destroy_resource(id, content::asset_type::texture);
        }
    }

    // remove shaders and textures
    if (id::is_valid(vs_id))
    {
        content::remove_shader_group(vs_id);
    }

    if (id::is_valid(ps_id))
    {
        content::remove_shader_group(ps_id);
    }

    if (id::is_valid(textured_ps_id))
    {
        content::remove_shader_group(textured_ps_id);
    }
}

void
get_render_items(id::id_type* items, [[maybe_unused]] u32 count)
{
    //assert(count == 3);
    items[0] = strip_club_item_id;
    items[1] = sword_item_id;
    items[2] = maria_item_id;
}