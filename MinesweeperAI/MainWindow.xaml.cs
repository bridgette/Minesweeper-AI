using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Threading;

namespace MinesweeperAI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

        /// <summary>
        /// Uses a bit of hacky keyboard automation to launch modern app.
        /// Otherwise, not possible without access to app's source code.
        /// </summary>
        /// <param name="modernAppName">name of modern (tile) app to launch</param>
        public void ensureModernAppRunning(string modernAppName)
        {
            SendKeys.SendWait("^{ESC}");
            SendKeys.SendWait(modernAppName);
            SendKeys.SendWait("{ENTER}");
            Thread.Sleep(10000);

            Process[] processes = Process.GetProcessesByName(modernAppName);
            foreach (Process p in processes)
            {
                p.Refresh();
                IntPtr windowHandle = p.MainWindowHandle;
                Trace.TraceInformation("window handle is " + windowHandle.ToString());
            }

            if (processes.Length == 0)
            {
                throw new ModernAppUIAutomationException("Tried and failed to launch " + modernAppName);
            }

            return;
        }

        public Bitmap GetScreenshotModernApp(string modernAppName)
        {
            var rect = new NativeMethods.Rect();
            IntPtr hwd = NativeMethods.GetForegroundWindow();
            NativeMethods.GetWindowRect(hwd, ref rect);

            int width = rect.right - rect.left;
            int height = rect.bottom - rect.top;

            Bitmap bmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.CopyFromScreen(rect.left, rect.top, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

            bmp.Save("c:\\users\\breiche\\pictures\\test.png", ImageFormat.Png);

            return bmp;
        }

        public MainWindow()
        {
            string appName = "minesweeper";
            InitializeComponent();

            ensureModernAppRunning(appName); 
            Bitmap bmp = GetScreenshotModernApp(appName);
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

            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref Rect rect);

            [System.Runtime.InteropServices.DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();
        }
    }
}