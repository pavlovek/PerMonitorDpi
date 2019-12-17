namespace PerMonitorDpi
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    
    /// <summary>
    /// This class suppresses stack walks for unmanaged code permission. 
    /// (System.Security.SuppressUnmanagedCodeSecurityAttribute is applied to this class.) 
    /// This class is for methods that are safe for anyone to call. Callers of these methods 
    /// are not required to do a full security review to ensure that the usage is secure 
    /// because the methods are harmless 
    /// </summary>
    /// <remarks>
    /// Methods that simply query for information or state that isn't sensitive can be moved 
    /// here.
    /// </remarks>
    [SuppressUnmanagedCodeSecurity]
    internal static class SafeNativeMethods
    {
        [DllImport("user32.dll")]
        internal static extern bool ChangeWindowMessageFilter(uint msg, ChangeWindowMessageFilterFlags flags);

        [DllImport("shcore.dll")]
        internal static extern uint SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("gdi32.dll")]
        internal static extern int GetDeviceCaps(IntPtr hdc, DeviceCaps nIndex);

        [DllImport("user32.dll")]
        internal static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOpts dwFlags);

        [DllImport("shcore.dll")]
        internal static extern uint GetDpiForMonitor(IntPtr hmonitor, MonitorDpiType dpiType, out uint dpiX, out uint dpiY);


        // Note: this methods are taken from https://raw.githubusercontent.com/Microsoft/referencesource/9da503f9ef21e8d1f2905c78d4e3e5cbb3d6f85a/mscorlib/microsoft/win32/win32native.cs

        // Note - do NOT use this to call methods.  Use P/Invoke, which will
        // do much better things w.r.t. marshaling, pinning memory, security 
        // stuff, better interactions with thread aborts, etc.  This is used
        // solely by DoesWin32MethodExist for avoiding try/catch EntryPointNotFoundException
        // in scenarios where an OS Version check is insufficient
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, BestFitMapping = false, SetLastError = true, ExactSpelling = true)]
        [ResourceExposure(ResourceScope.None)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String methodName);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        private static extern IntPtr LoadLibrary(string libFilename);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [ResourceExposure(ResourceScope.Process)]
        private static extern bool FreeLibrary(IntPtr hModule);
        

        [System.Security.SecurityCritical]  // auto-generated
        internal static bool DoesWin32MethodExist(String moduleName, String methodName)
        {
            IntPtr hModule = LoadLibrary(moduleName);
          
            if (hModule == IntPtr.Zero)
            {
                Debug.Assert(hModule != IntPtr.Zero, "LoadLibrary failed. API must not be available");
                return false;
            }

            IntPtr functionPointer = GetProcAddress(hModule, methodName);

            FreeLibrary(hModule);
            return (functionPointer != IntPtr.Zero);
        }
    }
}