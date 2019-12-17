namespace PerMonitorDpi
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Runtime.InteropServices;
    using System.Windows.Interactivity;

    /// <summary>
    /// Определяет поведение при изменении DPI на мониторе
    /// </summary>
    /// <remarks>https://github.com/anaisbetts/PerMonitorDpi</remarks>
    public class PerMonitorDpiBehavior : Behavior<FrameworkElement>
    {    
        private HwndSource _hwndSource;
        private double _currentDpiRatio;

        static PerMonitorDpiBehavior()
        {
            if (MonitorDpi.IsHighDpiMethodSupported()) 
            {
                // NB: We need to call this early before we start doing any 
                // fiddling with window coordinates / geometry
                SafeNativeMethods.SetProcessDpiAwareness(PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE);
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded += AssociatedObjectOnUnloaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            
            AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
            AssociatedObject.Unloaded -= AssociatedObjectOnUnloaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject.IsInitialized)
                AddHwndHook();
            else
                AssociatedObject.Initialized += AssociatedObject_SourceInitialized;

            // NB: This allows us to drag-drop URLs from IE11, which would 
            // normally fail because we run at Medium integrity and most of
            // IE runs at Low or AppContainer level.
            EnableDragDropFromLowPrivUIPIProcesses();
        }

        private void AssociatedObjectOnUnloaded(object sender, RoutedEventArgs e)
        {
            RemoveHwndHook();
        }

        private void AddHwndHook()
        {
            _hwndSource = PresentationSource.FromVisual(AssociatedObject) as HwndSource;
            _hwndSource?.AddHook(HwndHook);
        }

        private void RemoveHwndHook()
        {
            AssociatedObject.Initialized -= AssociatedObject_SourceInitialized;
            _hwndSource.RemoveHook(HwndHook);
        }

        private void AssociatedObject_SourceInitialized(object sender, EventArgs e)
        {
            AddHwndHook();

            UpdateDpiScaling(_currentDpiRatio);
        }

        static void EnableDragDropFromLowPrivUIPIProcesses()
        {
            // UIPI was introduced on Vista
            if (Environment.OSVersion.Version.Major < 6) 
                return;

            var msgs = new uint[] 
            {
                0x233,      // WM_DROPFILES
                0x48,       // WM_COPYDATA
                0x49,       // NOBODY KNOWS BUT EVERYONE SAYS TO DO IT
            };

            foreach (var msg in msgs) 
                SafeNativeMethods.ChangeWindowMessageFilter(msg, ChangeWindowMessageFilterFlags.Add);
        }

        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "GitHub.Extensions.Windows.Native.UnsafeNativeMethods.DwmExtendFrameIntoClientArea(System.IntPtr,GitHub.Extensions.Windows.Native.MARGINS@)")]
        private IntPtr HwndHook(IntPtr hWnd, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (message)
            {
                case NativeConstants.WM_DPICHANGED:
                    var rect = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));

                    SafeNativeMethods.SetWindowPos(hWnd, IntPtr.Zero,
                        rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top,
                        SetWindowPosFlags.DoNotChangeOwnerZOrder | SetWindowPosFlags.DoNotActivate | SetWindowPosFlags.IgnoreZOrder);

                    var newDpiRatio = MonitorDpi.GetScaleRatioForWindow(AssociatedObject);
                    if (newDpiRatio != _currentDpiRatio) 
                        UpdateDpiScaling(newDpiRatio);

                    break;
            }

            return IntPtr.Zero;
        }

        private void UpdateDpiScaling(double newDpiRatio)
        {
            _currentDpiRatio = newDpiRatio;

            var firstChild = (Visual)VisualTreeHelper.GetChild(AssociatedObject, 0);
            firstChild.SetValue(FrameworkElement.LayoutTransformProperty, new ScaleTransform(_currentDpiRatio, _currentDpiRatio));
        }
    }
}
