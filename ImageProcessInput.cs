using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ProcView
{
    /// <summary>
    /// Wraps the input required by the ImageProcess class into one object to use in threads.
    /// </summary>
    class ImageProcessInput
    {
        public Bitmap Img
        { get; set; }

        public Bitmap Img2
        { get; set; }

        public Kernel Kernel
        { get; set; }


        /// <summary>
        /// Used for applying a filter to an image
        /// </summary>
        /// <param name="img">image</param>
        /// <param name="kernel">kernel</param>
        public ImageProcessInput(Bitmap img, Kernel kernel)
        {
            Img = img;
            Kernel = kernel;
        }

        /// <summary>
        /// Used for adding or subtracting two images. Images must be same size.
        /// </summary>
        /// <param name="imga">first image</param>
        /// <param name="imgb">second image</param>
        public ImageProcessInput(Bitmap imga, Bitmap imgb)
        {
            Img = imga;
            Img2 = imgb;
        }
    }
}
