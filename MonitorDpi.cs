namespace PerMonitorDpi
{
    using System;
    using System.Windows;
    using System.Windows.Interop;

    /// <summary>
    /// Определение DPI монитора
    /// </summary>
    /// <remarks>https://github.com/anaisbetts/PerMonitorDpi</remarks>
    internal static class MonitorDpi
    { 
        static bool? _isHighDpiMethodSupported = null;

        public static bool IsHighDpiMethodSupported()
        {
            if (_isHighDpiMethodSupported != null) return _isHighDpiMethodSupported.Value;

            _isHighDpiMethodSupported = SafeNativeMethods.DoesWin32MethodExist("shcore.dll", "SetProcessDpiAwareness");

            return _isHighDpiMethodSupported.Value;
        }

        public static double GetScaleRatioForWindow(IntPtr hWnd)
        {
            var wpfDpi = 96.0 * PresentationSource.FromVisual(Application.Current.MainWindow).CompositionTarget.TransformToDevice.M11;

            if (IsHighDpiMethodSupported() == false) 
            {
                // Use System DPI
                return wpfDpi / 96.0;
            } 
            else 
            {
                var monitor = SafeNativeMethods.MonitorFromWindow(hWnd, MonitorOpts.MONITOR_DEFAULTTONEAREST);

                uint dpiX; uint dpiY;
                SafeNativeMethods.GetDpiForMonitor(monitor, MonitorDpiType.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);

                return ((double)dpiX) / wpfDpi;
            }
        }

        public static double GetScaleRatioForWindow(FrameworkElement This)
        {
            var hwndSource = PresentationSource.FromVisual(This) as HwndSource;
            return GetScaleRatioForWindow(hwndSource.Handle);
        }
    }
}
