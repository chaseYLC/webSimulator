using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace webSimulator
{
    public class SystemFunctions
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern void keybd_event(byte vk, byte scan, int flags, int extrainfo);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern short VkKeyScan(char ch);


        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public const int KEYEVENTF_KEYUP = 0x02;
        public const byte KEYEVENTF_SILENT = 0X04;


        public static void MouseClick(Window wnd, int x, int y)
        {
            Mouse.Capture(wnd);

            Point screenCoordinates = wnd.PointToScreen(new Point(x, y));
            SetCursorPos((int)screenCoordinates.X, (int)screenCoordinates.Y);

            mouse_event(MOUSEEVENTF_LEFTDOWN, x, y, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, x, y, 0, 0);

            Mouse.Capture(null);
        }

        public static void MouseMove(Window wnd, int x, int y)
        {
            Mouse.Capture(wnd);

            Point screenCoordinates = wnd.PointToScreen(new Point(x, y));
            SetCursorPos((int)screenCoordinates.X, (int)screenCoordinates.Y);

            Mouse.Capture(null);
        }

        public static void Key(Window wnd, string k)
        {
            Mouse.Capture(wnd);

            foreach (char c in k)
            {
                short keyCode = VkKeyScan(c);

                keybd_event((byte)keyCode, 0, KEYEVENTF_SILENT, 0);
                keybd_event((byte)keyCode, 0, KEYEVENTF_SILENT | KEYEVENTF_KEYUP, 0);

                Console.WriteLine("input key:" + c);
            }

            Mouse.Capture(null);
        }

        public static void CKey(Window wnd, byte code)
        {
            Mouse.Capture(wnd);

            keybd_event(code, 0, KEYEVENTF_SILENT, 0);
            keybd_event(code, 0, KEYEVENTF_SILENT | KEYEVENTF_KEYUP, 0);

            Mouse.Capture(null);
        }

        public static void Wait(Window wnd, int ms)
        {
            wnd.Dispatcher.Invoke((System.Threading.ThreadStart)(() => { }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            System.Threading.Thread.Sleep(ms);
        }
    }


    namespace EventHook
    {
        public static class MouseHook
        {
            public static event EventHandler MouseAction = delegate { };

            public static void Start()
            {
                _hookID = SetHook(_proc);
            }
            public static void stop()
            {
                UnhookWindowsHookEx(_hookID);
            }

            private static LowLevelMouseProc _proc = HookCallback;
            private static IntPtr _hookID = IntPtr.Zero;

            private static IntPtr SetHook(LowLevelMouseProc proc)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                using (ProcessModule curModule = curProcess.MainModule)
                {
                    return SetWindowsHookEx(WH_MOUSE_LL, proc,
                        GetModuleHandle(curModule.ModuleName), 0);
                }
            }

            private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

            private static IntPtr HookCallback(
                int nCode, IntPtr wParam, IntPtr lParam)
            {
                if (nCode >= 0 && MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                {
                    MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
                    MouseAction(null, new EventArgs());
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }

            private const int WH_MOUSE_LL = 14;

            private enum MouseMessages
            {
                WM_LBUTTONDOWN = 0x0201,
                WM_LBUTTONUP = 0x0202,
                WM_MOUSEMOVE = 0x0200,
                WM_MOUSEWHEEL = 0x020A,
                WM_RBUTTONDOWN = 0x0204,
                WM_RBUTTONUP = 0x0205
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct POINT
            {
                public int x;
                public int y;
            }

            [StructLayout(LayoutKind.Sequential)]
            private struct MSLLHOOKSTRUCT
            {
                public POINT pt;
                public uint mouseData;
                public uint flags;
                public uint time;
                public IntPtr dwExtraInfo;
            }

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr SetWindowsHookEx(int idHook,
                LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            private static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
                IntPtr wParam, IntPtr lParam);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            private static extern IntPtr GetModuleHandle(string lpModuleName);
        }
    }
}
