using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using IWin32Window = System.Windows.Forms.IWin32Window;

namespace PHmiClient.Utils {
    public static class WinFormsExtentions {
        public static IWin32Window GetIWin32Window(this Visual visual) {
            var source = PresentationSource.FromVisual(visual) as HwndSource;
            Debug.Assert(source != null, "source != null");
            IWin32Window win = new OldWindow(source.Handle);
            return win;
        }

        private class OldWindow : IWin32Window {
            private readonly IntPtr _handle;

            public OldWindow(IntPtr handle) {
                _handle = handle;
            }

            IntPtr IWin32Window.Handle {
                get { return _handle; }
            }
        }
    }
}