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

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;

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

        // todo: implement this
        //public IBlobsFilter BlobsFilter


        /// <summary>
        /// uses AForge to detect square (or rectangular, if tile is obscured) tiles in the image
        /// </summary>
        /// <param name="img"></param>
        public void DetectBlobsInImage(Bitmap img)
        {
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.ProcessImage(img);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            Graphics g = Graphics.FromImage(img);
            System.Drawing.Pen bluePen = new System.Drawing.Pen(System.Drawing.Color.Blue, 10);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                try
                {
                    List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                    List<IntPoint> corners = PointsCloud.FindQuadrilateralCorners(edgePoints);

                    SimpleShapeChecker shapeChecker = new SimpleShapeChecker();
                    PolygonSubType subType = shapeChecker.CheckPolygonSubType(corners);
                    if (subType == PolygonSubType.Square)
                    {
                        g.DrawPolygon(bluePen, PointsListToArray(corners));
                    }
                }
                catch (Exception) { }
            }

            bluePen.Dispose();
            g.Dispose();
        }

        // Convert list of AForge.NET's IntPoint to array of .NET's Point
        private static System.Drawing.Point[] PointsListToArray(List<IntPoint> list)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[list.Count];

            for (int i = 0, n = list.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(list[i].X, list[i].Y);
            }

            return array;
        }

        /// 
        ///
        public System.Drawing.Image CropHeaderFooter(System.Drawing.Image img, int headerHeight, int FooterHeight) {

            int croppedWidth = img.Width;
            int croppedHeight = img.Height - headerHeight - FooterHeight;
            try
            {
                Bitmap target = new Bitmap(croppedWidth, croppedHeight);
                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(img,
                      new RectangleF(0, 0, croppedWidth, croppedHeight),
                      new RectangleF(0, headerHeight, croppedWidth, croppedHeight),
                      GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(
                  string.Format("Couldn't crop header and footer. Header height = {0}, footer height = {1}", headerHeight, FooterHeight), ex);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        public GameLogic ScrapeBoardFromImage(Bitmap img)
        {
            // filter board's color to better identify tiles
            Blur blur = new Blur();
            blur.Threshold = 1;
            blur.ApplyInPlace(img);
            BrightnessCorrection brightness_filter = new BrightnessCorrection(-30);
            brightness_filter.ApplyInPlace(img);
            HSLFiltering luminance_filter = new HSLFiltering();
            luminance_filter.Luminance = new Range(0.50f, 1);
            luminance_filter.ApplyInPlace(img);
            img.Save("c:\\users\\breiche\\pictures\\filtered.bmp", ImageFormat.Bmp);

            DetectBlobsInImage(img);
            img.Save("c:\\users\\breiche\\pictures\\blobs.bmp", ImageFormat.Bmp);

            return new GameLogic(0, 0);
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
