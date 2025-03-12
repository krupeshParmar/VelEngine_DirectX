#pragma once
#include "..\Common\CommonHeaders.h"

namespace vel::platform
{
	DEFINE_TYPE_ID(window_id);

	class window
	{
	public:
		constexpr explicit window(window_id id) : _id{ id } {}
		constexpr window() : _id{ id::invalid_id } {}
		constexpr window_id get_id() const { return _id; }
		constexpr bool is_valid() const { return id::is_valid(_id); }

		void set_fullscreen(bool isfullscreen) const;
		bool is_fullscreen() const;
		void* handle() const;
		void set_caption(const wchar_t* caption) const;
		const math::u32v4 size() const;
		void resize(u32 width, u32 height) const;
		const u32 width() const;
		const u32 height() const;
		bool is_closed() const;

		window(const window& w)
		{
			_id = w.get_id();
		}

	private:
		window_id _id{ id::invalid_id };
	};
}