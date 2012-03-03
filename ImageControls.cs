using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ProcView
{
    /// <summary>
    /// This class provides basic zooming and panning for displayed images.  Functions are repeated to synchronize operations between images.
    /// User would like to zoom in and out on one image and view the corresponding zoom on the other image
    /// </summary>
    class ImageControls
    {

        //Initialize variables
        private Image image;
        private Canvas imageCanvas;

        private Image procImage;
        private Canvas procImageCanvas;

        private System.Windows.Point origin;
        private System.Windows.Point start;

        private System.Windows.Point originO;
        private System.Windows.Point startO;

        /// <summary>
        /// ImageControls constructor
        /// </summary>
        /// <param name="uiImage">User's Image</param>
        /// <param name="uiImageCanvas">Canvas the image is displayed on</param>
        /// <param name="uiProcImage">Processed Image</param>
        /// <param name="uiProcImageCanvas">Canvas that the processed image is displayed on</param>
        public ImageControls(Image uiImage, Canvas uiImageCanvas, Image uiProcImage, Canvas uiProcImageCanvas)
        {
            image = uiImage;
            imageCanvas = uiImageCanvas;

            procImage = uiProcImage;
            procImageCanvas = uiProcImageCanvas;

            //Create transform group that includes ScaleTransform (zooming) and TranslateTransform (panning)
            TransformGroup group = new TransformGroup();
            ScaleTransform xform = new ScaleTransform();
            group.Children.Add(xform);
            TranslateTransform tt = new TranslateTransform();
            TranslateTransform ttp = new TranslateTransform();
            group.Children.Add(tt);
            group.Children.Add(ttp);

            //Add transform group to both images. Cant add to canvas because there is no boundry for the canvas, thus the images will expand outside the GUI
            image.RenderTransform = group;
            image.MouseWheel += image_MouseWheel;
            image.MouseLeftButtonDown += image_MouseLeftButtonDown;       
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;

            procImage.RenderTransform = group;
            procImage.MouseWheel += procImage_MouseWheel;
            procImage.MouseLeftButtonDown += procImage_MouseLeftButtonDown;
            procImage.MouseLeftButtonUp += procImage_MouseLeftButtonUp;
            procImage.MouseMove += procImage_MouseMove;

        }

        /// <summary>
        /// Release panning mode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            image.ReleaseMouseCapture();
        }

        /// <summary>
        /// Pan the image by moving the mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            var ttp = (TranslateTransform)((TransformGroup)procImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(imageCanvas);
            Vector vO = startO - e.GetPosition(procImageCanvas);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
            ttp.X = originO.X - vO.X;
            ttp.Y = originO.Y - vO.Y;
        }

        /// <summary>
        /// Click and hold the LMouse button to begin panning
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            var ttp = (TranslateTransform)((TransformGroup)procImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(imageCanvas);
            startO = e.GetPosition(procImageCanvas);
            origin = new System.Windows.Point(tt.X, tt.Y);
            originO = new System.Windows.Point(ttp.X, ttp.Y);
        }

        /// <summary>
        /// Scroll Wheel up or down to zoom in or out
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)image.RenderTransform;
            TransformGroup transformGroupP = (TransformGroup)procImage.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];
            ScaleTransform transformP = (ScaleTransform)transformGroupP.Children[0];

            double zoom = e.Delta > 0 ? .2 : -.2;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
            transformP.ScaleX += zoom;
            transformP.ScaleY += zoom;
        }

        //*************************************************************************************************
        // Repeat Function on processedImage display for Synchornization
        //*************************************************************************************************
        private void procImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            procImage.ReleaseMouseCapture();
        }

        private void procImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (!procImage.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            var ttp = (TranslateTransform)((TransformGroup)procImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(imageCanvas);
            Vector vO = startO - e.GetPosition(procImageCanvas);
            tt.X = origin.X - v.X;
            tt.Y = origin.Y - v.Y;
            ttp.X = originO.X - vO.X;
            ttp.Y = originO.Y - vO.Y;
        }

        private void procImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            procImage.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            var ttp = (TranslateTransform)((TransformGroup)procImage.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(imageCanvas);
            startO = e.GetPosition(procImageCanvas);
            origin = new System.Windows.Point(tt.X, tt.Y);
            originO = new System.Windows.Point(ttp.X, ttp.Y);
        }

        private void procImage_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TransformGroup transformGroup = (TransformGroup)image.RenderTransform;
            TransformGroup transformGroupP = (TransformGroup)procImage.RenderTransform;
            ScaleTransform transform = (ScaleTransform)transformGroup.Children[0];
            ScaleTransform transformP = (ScaleTransform)transformGroupP.Children[0];

            double zoom = e.Delta > 0 ? .2 : -.2;
            transform.ScaleX += zoom;
            transform.ScaleY += zoom;
            transformP.ScaleX += zoom;
            transformP.ScaleY += zoom;
        }
        //*************************************************************************************************
        //*************************************************************************************************

    }
}
