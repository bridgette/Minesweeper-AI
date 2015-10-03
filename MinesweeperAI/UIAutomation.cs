using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Runtime.Serialization;

namespace MinesweeperAI
{
    public class UIAutomation
    {
        public IntPtr mainWindowHandle { get; private set; }
        public string modernAppName { get; private set; }

        public UIAutomation(string appName)
        {
            modernAppName = appName;
            LaunchModernApp();
            mainWindowHandle = NativeMethods.GetForegroundWindow();
        }

        public Bitmap GetScreenshotModernApp()
        {
            var rect = new NativeMethods.Rect();
            NativeMethods.GetWindowRect(mainWindowHandle, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            try
            {
                Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics graphics = Graphics.FromImage(bmp);
                graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

                bmp.Save("c:\\users\\breiche\\pictures\\minesweeperWindow.png", ImageFormat.Png);
                return bmp;
            }
            catch (Exception e)
            {
                throw new ModernAppUIAutomationException("Failed to get a screenshot from " + modernAppName, e.InnerException);
            }
        }

        private bool FocusOnWindow()
        {
            bool wasSuccessful = false;
            try
            {
                wasSuccessful = NativeMethods.BringWindowToTop(mainWindowHandle);
            }
            catch (Exception e)
            {
                throw new ModernAppUIAutomationException("Failed to bring window to front " + modernAppName, e.InnerException);
            }

            return wasSuccessful;
        }

        /// <summary>
        /// Uses a bit of hacky keyboard automation to launch modern app.
        /// Otherwise, not possible without access to app's source code.
        /// </summary>
        private void LaunchModernApp()
        {
            SendKeys.SendWait("^{ESC}");
            SendKeys.SendWait(modernAppName);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(5000);

            Process[] processes = Process.GetProcessesByName(modernAppName);

            if (processes.Length == 0)
            {
                throw new ModernAppUIAutomationException("Failed to launch " + modernAppName);
            }

            return;
        }


        /// <summary>
        /// Class to wrap C++ user32.dll
        /// </summary>
        private class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct Rect
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern bool BringWindowToTop(IntPtr hWnd);
        }
    }

    /// <summary>
    /// Exceptions when dealing with UI automation of modern app.
    /// </summary>
    [Serializable]
    public class ModernAppUIAutomationException : Exception
    {
        public ModernAppUIAutomationException()
        { }

        public ModernAppUIAutomationException(string message)
            : base(message)
        { }

        public ModernAppUIAutomationException(string message, Exception innerException)
            : base(message, innerException)
        { }

        protected ModernAppUIAutomationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
