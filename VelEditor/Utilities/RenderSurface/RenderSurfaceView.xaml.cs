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
        }

        private nint HostMsgFilter(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
        {
            switch ((Win32Msgs)msg)
            {
                case Win32Msgs.WM_SIZE:
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
