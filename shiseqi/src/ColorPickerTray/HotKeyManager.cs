using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace ColorPickerTray
{
    public class HotKeyManager : IDisposable
    {
        private readonly IntPtr _windowHandle;
        private readonly Window _window;
        private int _hotKeyId;

        public event EventHandler? HotKeyPressed;

        public HotKeyManager(Window window)
        {
            _window = window;
            var helper = new System.Windows.Interop.WindowInteropHelper(window);
            _windowHandle = helper.EnsureHandle();
            var source = System.Windows.Interop.HwndSource.FromHwnd(_windowHandle);
            if (source != null)
            {
                source.AddHook(WndProc);
            }
            else
            {
                _window.SourceInitialized += OnSourceInitialized;
            }
        }

        private void OnSourceInitialized(object? sender, EventArgs e)
        {
            var source = System.Windows.Interop.HwndSource.FromHwnd(_windowHandle);
            source?.AddHook(WndProc);
        }

        public bool RegisterHotKey(ModifierKeys modifier, Keys key)
        {
            _hotKeyId = GetHashCode();
            return RegisterHotKey(_windowHandle, _hotKeyId, (uint)modifier, (uint)key);
        }

        public void UnregisterHotKey()
        {
            if (_hotKeyId != 0)
            {
                UnregisterHotKey(_windowHandle, _hotKeyId);
                _hotKeyId = 0;
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY && wParam.ToInt32() == _hotKeyId)
            {
                HotKeyPressed?.Invoke(this, EventArgs.Empty);
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            UnregisterHotKey();
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
