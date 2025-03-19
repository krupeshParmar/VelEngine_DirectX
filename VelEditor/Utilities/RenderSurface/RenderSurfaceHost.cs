using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using VelEditor.DLLWrapper;

namespace VelEditor.Utilities
{
    class RenderSurfaceHost : HwndHost
    {
        private readonly int VK_LBUTTON = 0x01;
        private IntPtr _winHandle = IntPtr.Zero;
        private readonly int _width = 800;
        private readonly int _height = 600;
        private DelayEventTimer _resizeTimer;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public int SurfaceId { get; private set; } = ID.INVALID_ID;

        private void Resize(object? sender, DelayEventTimerArgs e)
        {
            e.RepeatEvent = GetAsyncKeyState(VK_LBUTTON) < 0;
            if (!e.RepeatEvent)
            {
                VelAPI.ResizeRenderSurface(SurfaceId);
            }
        }

        public RenderSurfaceHost(double width, double height)
        {
            _width = (int)width;
            _height = (int)height;
            _resizeTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250.0));
            _resizeTimer.Triggered += Resize;

            SizeChanged += (s, e) => _resizeTimer.Trigger();
        }


        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            SurfaceId = VelAPI.CreateRenderSurface(hwndParent.Handle, _width, _height);
            Debug.Assert(ID.IsValid(SurfaceId));
            _winHandle = VelAPI.GetWindowHandle(SurfaceId);
            Debug.Assert(_winHandle != IntPtr.Zero);

            return new HandleRef(this, _winHandle);
        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            VelAPI.RemoveRenderSurface(SurfaceId);
            SurfaceId = ID.INVALID_ID;
            _winHandle = IntPtr.Zero;
        }
    }
}
