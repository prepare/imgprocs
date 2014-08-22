using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace YLScsDrawing.Imaging
{
    public struct ColorRGBA
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public ColorRGBA(byte b, byte g, byte r, byte a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    /// <summary>
    /// Using InteropServices.Marshal mathods to get image channels (R,G,B,A) byte
    /// </summary>
    public class ImageData : IDisposable
    {

        ColorRGBA[] colorRGBAs;
        private bool _disposed = false;
        byte[] myBuffer;

        int imgWidth, imgHeight;
        int stride;
        public ImageData(int width, int height)
        {
            this.imgWidth = width;
            this.imgHeight = height;
            this.colorRGBAs = new ColorRGBA[width * height];
            this.stride = width * 4;//sample
        }
        public ImageData(Bitmap srcBmp)
        {
            int w = srcBmp.Width;
            int h = srcBmp.Height;
            this.imgWidth = w;
            this.imgHeight = h;

            // Lock the bitmap's bits.  
            System.Drawing.Imaging.BitmapData bmpData = srcBmp.LockBits(new Rectangle(0, 0, w, h),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            this.stride = bmpData.Stride;
            //--------------------

            // Declare an array to hold the bytes of the bitmap.
            int buffLen = bmpData.Stride * srcBmp.Height;
            byte[] rgbValues = new byte[buffLen];
            this.myBuffer = rgbValues;
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbValues, 0, buffLen);
            // Unlock the bits.
            srcBmp.UnlockBits(bmpData);
            //--------------------------------------------------------
            ColorRGBA[] colors = new ColorRGBA[w * h];
            this.colorRGBAs = colors;

            // Copy the RGB values 
            int offset = stride - w * 4;

            int index = 0;
            int cIndex = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {

                    //create color array
                    colors[cIndex] = new ColorRGBA(
                        rgbValues[index], //b
                        rgbValues[index + 1], //g
                        rgbValues[index + 2], //r 
                        rgbValues[index + 3]); //a


                    index += 4;
                    cIndex++;
                }
                index += offset;
            }
        }
        public int Stride
        {
            get
            {
                return this.stride;
            }
        }
        public int Width { get { return this.imgWidth; } }
        public int Height { get { return this.imgHeight; } }

        public static ImageData CreateFromBitmap(Bitmap srcBmp)
        {
            return new ImageData(srcBmp);
        }

        public byte[] GetBuffer()
        {
            return this.myBuffer;
        }

        public Bitmap ToBitmap()
        {
            int width = this.imgWidth;
            int height = this.imgHeight;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = bmpData.Stride * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // set rgbValues
            int offset = bmpData.Stride - width * 4;
            int i = 0;
            int cIndex = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {

                    ColorRGBA color = colorRGBAs[cIndex];

                    rgbValues[i] = color.b;
                    rgbValues[i + 1] = color.g;
                    rgbValues[i + 2] = color.r;
                    rgbValues[i + 3] = color.a;
                    i += 4;
                    cIndex++;
                }
                i += offset;
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);
            return bmp;
        }

        public ColorRGBA GetColorPixel(int x, int y)
        {
            return colorRGBAs[(y * imgWidth) + x];
        }
        public void SetColorPixel(int x, int y, ColorRGBA color)
        {
            colorRGBAs[(y * imgWidth) + x] = color;
        }
        public void Dispose()
        {
            Dispose(true);
            // Use SupressFinalize in case a subclass
            // of this type implements a finalizer.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // If you need thread safety, use a lock around these 
            // operations, as well as in your methods that use the resource.
            if (!_disposed)
            {
                if (disposing)
                {

                }

                // Indicate that the instance has been disposed.
                _disposed = true;
            }
        }
    }
}


