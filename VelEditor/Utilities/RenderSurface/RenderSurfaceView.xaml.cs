using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VelEditor.Utilities
{
    /// <summary>
    /// Interaction logic for RenderSurfaceView.xaml
    /// </summary>
    public partial class RenderSurfaceView : UserControl, IDisposable
    {
        private enum Win32Msgs
        {
            WM_SIZE             =   0x0005,
            WM_SIZING           =   0x0214,
            WM_ENTERSIZEMOVE    =   0x0231,
            WM_EXITSIZEMOVE     =   0x0232,
        }

        private RenderSurfaceHost _host = null;
        private bool _canResize = true;
        private bool _moved = false;

        public RenderSurfaceView()
        {
            InitializeComponent();
            Loaded += OnRenderSurfaceViewLoaded;
        }

        private void OnRenderSurfaceViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnRenderSurfaceViewLoaded;

            _host = new RenderSurfaceHost(ActualWidth, ActualHeight);
            _host.MessageHook += new HwndSourceHook(HostMsgFilter);
            Content = _host;

            var window = this.FindVisualParent<Window>();
            Debug.Assert(window != null);

            var helper = new WindowInteropHelper(window);
            if(helper.Handle != null)
            {
                HwndSource.FromHwnd(helper.Handle)?.AddHook(HwndMsgHook);
            }
        }

        private nint HwndMsgHook(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            switch ((Win32Msgs)msg)
            {
                case Win32Msgs.WM_SIZING:
                    _canResize = false;
                    _moved = false;
                    break;
                case Win32Msgs.WM_ENTERSIZEMOVE:
                    _moved = true;
                    break;
                case Win32Msgs.WM_EXITSIZEMOVE:
                    _canResize = true;
                    if(!_moved)
                    {
                        _host.Resize();
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private nint HostMsgFilter(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            switch ((Win32Msgs)msg)
            {
                case Win32Msgs.WM_SIZE:
                    if (_canResize)
                    {
                        _host.Resize();
                    }
                    break;
                case Win32Msgs.WM_SIZING:
                case Win32Msgs.WM_ENTERSIZEMOVE:
                case Win32Msgs.WM_EXITSIZEMOVE:
                    throw new Exception();
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        #region IDisposableSupport
        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_host != null)
                    {
                        _host.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
