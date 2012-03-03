using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace ProcView
{
    class ImageProcess
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public ImageProcess()
        {

        }


        /// <summary>
        /// Method to pad input image to accomodate kernel size
        /// </summary>
        /// <param name="bitmap">Input Image</param>
        /// <param name="kernelSize">Size of kernel to be used for processing</param>
        /// <returns>Returns bit map with "Black" edges for image processing with kernels</returns>
        private Bitmap PadImage(Bitmap bitmap, int matrixSize)
        {
            int padding = matrixSize-1;
            int offset = (matrixSize-1)/2;
            Bitmap padBitmap = new Bitmap(bitmap.Width+padding, bitmap.Height+padding);
            padBitmap = SetBlack(padBitmap);

            for (int y = 0; y < bitmap.Height; y++)
            {
                for (int x = 0; x < bitmap.Width; x++)
                {
                    padBitmap.SetPixel(x + offset, y + offset, bitmap.GetPixel(x,y));
                }
            }

            return padBitmap;
        }

        /// <summary>
        /// Sets all the pixels of an image to black: ARGB{0,0,0,0}
        /// </summary>
        /// <param name="img">input image</param>
        /// <returns></returns>
        private Bitmap SetBlack(Bitmap img)
        {
            for (int y = 0; y < img.Height; y++)
            {
                for (int x = 0; x < img.Width; x++)
                {
                    //Black sets the ARGB values to {0, 0, 0, 0}
                    img.SetPixel(x, y, System.Drawing.Color.FromName("Black"));
                }
            }
            return img;
        }

        /// <summary>
        /// Algorithm that applies selected kernel to image
        /// </summary>
        /// <param name="paddedBitmap">Image with enough padding to allow for kernel processing</param>
        /// <param name="kernel">kernel matrix to be applied to image</param>
        /// <returns>Returns a processed image</returns>
        public Bitmap FilterImage(Bitmap img, Kernel kernel, System.ComponentModel.BackgroundWorker backgroundWorker)
        {
            int imgSize = img.Width * img.Height;
            int iteration = imgSize / 100;
            int p, alpha, red, green, blue, x, y, i, j;
            int xKMatrix, yKMatrix, kernelElement;
            int size = kernel.matrixSize;
            int matrixSum = kernel.matrixSum;
            string kName = kernel.Name;
            Bitmap padImg = new Bitmap(PadImage(img, size));
            System.Drawing.Color fixPixel, kernelColor;


            for (y = 0; y < img.Height; y++)
            {
                p = y*img.Width;
                for (x = 0; x < img.Width; x++)
                {
                    alpha = 0;
                    red = 0;
                    green = 0;
                    blue = 0;
                        for (i = 0; i < size; i++)
                        {
                            for (j = 0; j < size; j++)
                            {
                                xKMatrix = x + j;
                                yKMatrix = y + i;
                                kernelElement = kernel.matrix[j, i];
                                kernelColor = padImg.GetPixel(xKMatrix, yKMatrix);
                                if (kernelElement > 0)
                                {
                                    alpha += (kernelColor.A * kernelElement);
                                    red += (kernelColor.R * kernelElement);
                                    green += (kernelColor.G * kernelElement);
                                    blue += (kernelColor.B * kernelElement);
                                }
                                else
                                {
                                    alpha += (kernelColor.A-255) * kernelElement;
                                    red += (kernelColor.R-255) * kernelElement;
                                    green += (kernelColor.G-255) * kernelElement;
                                    blue += (kernelColor.B -255) * kernelElement;
                                }                   
                            }
                        }

                    //take absolute values to ensure no negative numbers get passed into component of pixel
                    alpha = Math.Abs(alpha / matrixSum);
                    red = Math.Abs(red / matrixSum);
                    green = Math.Abs(green / matrixSum);
                    blue = Math.Abs(blue / matrixSum);

                    //create pixel out of components
                    fixPixel = System.Drawing.Color.FromArgb(alpha, red, green, blue);

                    //set target pixel to new value
                    img.SetPixel(x, y, fixPixel);

                    p += 1;
                }

                //Report progress
                if (backgroundWorker != null)
                {
                    if (backgroundWorker.WorkerReportsProgress)
                    {
                        backgroundWorker.ReportProgress(p / iteration);
                    }
                }
            }

            if (backgroundWorker != null && backgroundWorker.WorkerReportsProgress)
            {
                backgroundWorker.ReportProgress(100);
            }

            return img;

        }

        /// <summary>
        /// Sum two images. Output is average Value of both
        /// </summary>
        /// <param name="imgA">Image A</param>
        /// <param name="imgB">Image B</param>
        /// <returns></returns>
        public Bitmap AddImages(Bitmap imgA, Bitmap imgB)
        {
            Bitmap addedImage = new Bitmap(imgA.Width, imgA.Height);
            if (imgA.Width == imgB.Width && imgA.Height == imgB.Height)
            {
                for (int y = 0; y < imgA.Height; y++)
                {
                    for (int x = 0; x < imgA.Width; x++)
                    {
                        int alpha = Math.Abs((imgA.GetPixel(x, y).A + imgB.GetPixel(x, y).A) / 2);
                        int red = Math.Abs((imgA.GetPixel(x, y).R + imgB.GetPixel(x, y).R) / 2);
                        int green = Math.Abs((imgA.GetPixel(x, y).G + imgB.GetPixel(x, y).G) / 2);
                        int blue = Math.Abs((imgA.GetPixel(x, y).B + imgB.GetPixel(x, y).B) / 2);
                        System.Drawing.Color addPixel = System.Drawing.Color.FromArgb(alpha, red, green, blue);
                        addedImage.SetPixel(x, y, addPixel);
                    }
                }

                return addedImage;
            }
            else
            {
                MessageBox.Show("The images must be the same size.");
                return imgA;
            }
        }

        /// <summary>
        /// Subract two images
        /// </summary>
        /// <param name="imgA">Image A</param>
        /// <param name="imgB">Image B</param>
        /// <returns></returns>
        public Bitmap SubtractImages(Bitmap imgA, Bitmap imgB)
        {
            Bitmap subImage = new Bitmap(imgA.Width, imgA.Height);
            if (imgA.Width == imgB.Width && imgA.Height == imgB.Height)
            {
                for (int y = 0; y < imgA.Height; y++)
                {
                    for (int x = 0; x < imgA.Width; x++)
                    {
                        int alpha = Math.Abs(imgA.GetPixel(x, y).A - imgB.GetPixel(x, y).A);
                        int red = Math.Abs(imgA.GetPixel(x, y).R - imgB.GetPixel(x, y).R);
                        int green = Math.Abs(imgA.GetPixel(x, y).G - imgB.GetPixel(x, y).G);
                        int blue = Math.Abs(imgA.GetPixel(x, y).B - imgB.GetPixel(x, y).B);
                        System.Drawing.Color addPixel = System.Drawing.Color.FromArgb(alpha, red, green, blue);
                        subImage.SetPixel(x, y, addPixel);
                    }
                }

                return subImage;
            }
            else
            {
                MessageBox.Show("The images must be the same size.");
                return imgA;
            }
        }
    }
}
