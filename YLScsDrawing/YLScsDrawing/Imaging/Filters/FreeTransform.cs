//from CodeProject: Free Image Transformation
//YLS CS 
//license : CPOL

using System;
using System.Drawing;
using System.Drawing.Drawing2D;

using ImageTransformation;

namespace YLScsDrawing.Imaging.Filters
{
    public class FreeTransform
    {
        PointF[] vertex = new PointF[4];
        YLScsDrawing.Geometry.Vector AB, BC, CD, DA;
        Rectangle rect = new Rectangle();
        YLScsDrawing.Imaging.ImageData srcCB;
        int srcW = 0;
        int srcH = 0;

        public Bitmap Bitmap
        {
            get
            {
                return GetTransformedBitmap();
            }
            set
            {
                try
                {

                    srcCB = ImageData.CreateFromBitmap(value);
                    srcH = value.Height;
                    srcW = value.Width;
                }
                catch
                {
                    srcW = 0; srcH = 0;
                }
            }
        }

        public Point ImageLocation
        {
            get { return rect.Location; }
            set { rect.Location = value; }

        }

        bool isBilinear = false;
        public bool IsBilinearInterpolation
        {
            get { return isBilinear; }
            set { isBilinear = value; }

        }

        public int ImageWidth
        {
            get { return rect.Width; }
        }

        public int ImageHeight
        {
            get { return rect.Height; }
        }

        public PointF VertexLeftTop
        {
            set { vertex[0] = value; setVertex(); }
            get { return vertex[0]; }
        }

        public PointF VertexTopRight
        {
            get { return vertex[1]; }
            set { vertex[1] = value; setVertex(); }

        }

        public PointF VertexRightBottom
        {
            get { return vertex[2]; }
            set { vertex[2] = value; setVertex(); }

        }

        public PointF VertexBottomLeft
        {
            get { return vertex[3]; }
            set { vertex[3] = value; setVertex(); }

        }

        public PointF[] FourCorners
        {
            get { return vertex; }
            set { vertex = value; setVertex(); }

        }

        private void setVertex()
        {
            float xmin = float.MaxValue;
            float ymin = float.MaxValue;
            float xmax = float.MinValue;
            float ymax = float.MinValue;

            for (int i = 0; i < 4; i++)
            {
                xmax = Math.Max(xmax, vertex[i].X);
                ymax = Math.Max(ymax, vertex[i].Y);
                xmin = Math.Min(xmin, vertex[i].X);
                ymin = Math.Min(ymin, vertex[i].Y);
            }

            rect = new Rectangle((int)xmin, (int)ymin, (int)(xmax - xmin), (int)(ymax - ymin));

            AB = new YLScsDrawing.Geometry.Vector(vertex[0], vertex[1]);
            BC = new YLScsDrawing.Geometry.Vector(vertex[1], vertex[2]);
            CD = new YLScsDrawing.Geometry.Vector(vertex[2], vertex[3]);
            DA = new YLScsDrawing.Geometry.Vector(vertex[3], vertex[0]);

            // get unit vector
            AB /= AB.Magnitude;
            BC /= BC.Magnitude;
            CD /= CD.Magnitude;
            DA /= DA.Magnitude;
        }

        private bool IsOnPlaneABCD(PointF pt) //  including point on border
        {
            if (!YLScsDrawing.Geometry.Vector.IsCCW(pt, vertex[0], vertex[1]))
            {
                if (!YLScsDrawing.Geometry.Vector.IsCCW(pt, vertex[1], vertex[2]))
                {
                    if (!YLScsDrawing.Geometry.Vector.IsCCW(pt, vertex[2], vertex[3]))
                    {
                        if (!YLScsDrawing.Geometry.Vector.IsCCW(pt, vertex[3], vertex[0]))
                            return true;
                    }
                }
            }
            return false;
        }

        Bitmap GetTransformedBitmap()
        {
            if (srcH == 0 || srcW == 0) return null;

            if (isBilinear)
            {
                //return GetTransformedBicubicInterpolation();
                return GetTransformedBicubicInterpolation();
                //return GetTransformedBilinearInterpolation();
            }
            else
            {
                return GetTransformedBitmapNoInterpolation();

            }
        }
        static BicubicInterpolator2 myInterpolator = new BicubicInterpolator2();

        Bitmap GetTransformedBitmapNoInterpolation()
        {
            ImageData destCB = new ImageData(rect.Width, rect.Height);
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;

            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;

            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {

                    Point srcPt = new Point(x, y);
                    srcPt.Offset(this.rect.Location);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;
                    destCB.SetColorPixel(x, y, srcCB.GetColorPixel(x1, y1));

                    //-------------------------------------
                    dab = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[0], srcPt)).CrossProduct(ab_vec));
                    dbc = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[1], srcPt)).CrossProduct(bc_vec));
                    dcd = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[2], srcPt)).CrossProduct(cd_vec));
                    dda = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[3], srcPt)).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));
                }
            }
            return destCB.ToBitmap();
        }
        Bitmap GetTransformedBilinearInterpolation()
        {
            //4 points sampling
            //weight between four point

            ImageData destCB = new ImageData(rect.Width, rect.Height);
            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;

            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;

            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {

                    Point srcPt = new Point(x, y);
                    srcPt.Offset(this.rect.Location);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    //-------------------------------------
                    dab = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[0], srcPt)).CrossProduct(ab_vec));
                    dbc = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[1], srcPt)).CrossProduct(bc_vec));
                    dcd = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[2], srcPt)).CrossProduct(cd_vec));
                    dda = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[3], srcPt)).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));

                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;

                    if (x1 >= 0 && x1 < srcW && y1 >= 0 && y1 < srcH)
                    {

                        //bilinear interpolation *** 
                        x2 = (x1 == srcW - 1) ? x1 : x1 + 1;
                        y2 = (y1 == srcH - 1) ? y1 : y1 + 1;

                        dx1 = ptInPlane.X - (float)x1;
                        if (dx1 < 0) dx1 = 0;
                        dx1 = 1f - dx1;
                        dx2 = 1f - dx1;
                        dy1 = ptInPlane.Y - (float)y1;
                        if (dy1 < 0) dy1 = 0;
                        dy1 = 1f - dy1;
                        dy2 = 1f - dy1;

                        dx1y1 = dx1 * dy1;
                        dx1y2 = dx1 * dy2;
                        dx2y1 = dx2 * dy1;
                        dx2y2 = dx2 * dy2;

                        //use 4 points
                        var x1y1Color = srcCB.GetColorPixel(x1, y1);
                        var x2y1Color = srcCB.GetColorPixel(x2, y1);
                        var x1y2Color = srcCB.GetColorPixel(x1, y2);
                        var x2y2Color = srcCB.GetColorPixel(x2, y2);

                        float a = (x1y1Color.a * dx1y1) + (x2y1Color.a * dx2y1) + (x1y2Color.a * dx1y2) + x2y2Color.a * dx2y2;
                        float b = (x1y1Color.b * dx1y1) + (x2y1Color.b * dx2y1) + (x1y2Color.b * dx1y2) + x2y2Color.b * dx2y2;
                        float g = (x1y1Color.g * dx1y1) + (x2y1Color.g * dx2y1) + (x1y2Color.g * dx1y2) + x2y2Color.g * dx2y2;
                        float r = (x1y1Color.r * dx1y1) + (x2y1Color.r * dx2y1) + (x1y2Color.r * dx1y2) + x2y2Color.r * dx2y2;

                        destCB.SetColorPixel(x, y, new ColorRGBA((byte)b, (byte)g, (byte)r, (byte)a));
                    }

                }
            }
            return destCB.ToBitmap();
        }
        Bitmap GetTransformedBicubicInterpolation()
        {
            //4 points sampling
            //weight between four point


            PointF ptInPlane = new PointF();
            int x1, x2, y1, y2;
            double dab, dbc, dcd, dda;
            float dx1, dx2, dy1, dy2, dx1y1, dx1y2, dx2y1, dx2y2;
            int rectWidth = rect.Width;
            int rectHeight = rect.Height;

            var ab_vec = this.AB;
            var bc_vec = this.BC;
            var cd_vec = this.CD;
            var da_vec = this.DA;

            byte[] buffer = srcCB.GetBuffer();
            int stride = srcCB.Stride;
            int bmpWidth = srcCB.Width;
            int bmpHeight = srcCB.Height;

            BufferReader4 reader = new BufferReader4(buffer, stride, bmpWidth, bmpHeight);
            MyColor[] pixelBuffer = new MyColor[16];
            byte[] sqPixs = new byte[16];



            //Bitmap outputbmp = new Bitmap(rectWidth, rectHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            ////-----------------------------------------------
            //var bmpdata2 = outputbmp.LockBits(new Rectangle(0, 0, rectWidth, rectHeight),
            //    System.Drawing.Imaging.ImageLockMode.ReadWrite, outputbmp.PixelFormat);
            ////-----------------------------------------
            ImageData destCB = new ImageData(rect.Width, rect.Height);
            //PointF ptInPlane = new PointF();

            //int stride2 = bmpdata2.Stride;
            //byte[] outputBuffer = new byte[stride2 * outputbmp.Height];

            // int targetPixelIndex = 0;
            // int startLine = 0;


            for (int y = 0; y < rectHeight; ++y)
            {
                for (int x = 0; x < rectWidth; ++x)
                {

                    Point srcPt = new Point(x, y);
                    srcPt.Offset(this.rect.Location);
                    if (!IsOnPlaneABCD(srcPt))
                    {
                        continue;
                    }
                    //-------------------------------------
                    dab = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[0], srcPt)).CrossProduct(ab_vec));
                    dbc = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[1], srcPt)).CrossProduct(bc_vec));
                    dcd = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[2], srcPt)).CrossProduct(cd_vec));
                    dda = Math.Abs((new YLScsDrawing.Geometry.Vector(vertex[3], srcPt)).CrossProduct(da_vec));
                    ptInPlane.X = (float)(srcW * (dda / (dda + dbc)));
                    ptInPlane.Y = (float)(srcH * (dab / (dab + dcd)));

                    x1 = (int)ptInPlane.X;
                    y1 = (int)ptInPlane.Y;

                    if (x1 >= 2 && x1 < srcW - 2 && y1 >= 2 && y1 < srcH - 2)
                    {


                        reader.SetStartPixel(x1, y1);
                        reader.Read16(pixelBuffer);
                        //do interpolate

                        //find src pixel and approximate  
                        MyColor color = GetApproximateColor_Bicubic(reader,
                           ptInPlane.X,
                           ptInPlane.Y);

                        //outputBuffer[targetPixelIndex] = (byte)color.b;
                        //outputBuffer[targetPixelIndex + 1] = (byte)color.g;
                        //outputBuffer[targetPixelIndex + 2] = (byte)color.r;
                        //outputBuffer[targetPixelIndex + 3] = (byte)color.a;
                        //targetPixelIndex += 4;

                        destCB.SetColorPixel(x, y, new ColorRGBA(color.b, color.g, color.r, color.a));
                    }

                }
                //newline
                // startLine += stride2;
                //targetPixelIndex = startLine;
            }
            //------------------------
            //System.Runtime.InteropServices.Marshal.Copy(
            //outputBuffer, 0,
            //bmpdata2.Scan0, outputBuffer.Length);
            //outputbmp.UnlockBits(bmpdata2);
            ////outputbmp.Save("d:\\WImageTest\\n_lion_bicubic.png");
            //return outputbmp;
            return destCB.ToBitmap();

        }
        static void SeparateByChannel(MyColor[] myColors, byte[] rBuffer, byte[] gBuffer, byte[] bBuffer, byte[] aBuffer)
        {
            for (int i = 0; i < 16; ++i)
            {
                MyColor m = myColors[i];
                rBuffer[i] = m.r;
                gBuffer[i] = m.g;
                bBuffer[i] = m.b;
                aBuffer[i] = m.a;
            }
        }
        static MyColor GetApproximateColor_Bicubic(BufferReader4 reader, double cx, double cy)
        {
            byte[] rBuffer = new byte[16];
            byte[] gBuffer = new byte[16];
            byte[] bBuffer = new byte[16];
            byte[] aBuffer = new byte[16];

            //nearest neighbor
            if (reader.CurrentX > 2 && reader.CurrentY > 2 &&
                reader.CurrentX < reader.Width - 2 &&
                reader.CurrentY < reader.Height - 2)
            {
                //read 4 point sample
                MyColor[] colors = new MyColor[16];
                reader.SetStartPixel((int)cx, (int)cy);
                reader.Read16(colors);

                double x0 = (int)cx;
                double x1 = (int)(cx + 1);
                double xdiff = cx - x0;

                double y0 = (int)cy;
                double y1 = (int)(cy + 1);
                double ydiff = cy - y0;


                SeparateByChannel(colors, rBuffer, gBuffer, bBuffer, aBuffer);

                double result_B = myInterpolator.getValueBytes(bBuffer, xdiff, ydiff);
                double result_G = myInterpolator.getValueBytes(gBuffer, xdiff, ydiff);
                double result_R = myInterpolator.getValueBytes(rBuffer, xdiff, ydiff);
                double result_A = myInterpolator.getValueBytes(aBuffer, xdiff, ydiff);

                //clamp
                if (result_B > 255)
                {
                    result_B = 255;
                }
                else if (result_B < 0)
                {
                    result_B = 0;
                }

                if (result_G > 255)
                {
                    result_G = 255;
                }
                else if (result_G < 0)
                {
                    result_G = 0;
                }

                if (result_R > 255)
                {
                    result_R = 255;
                }
                else if (result_R < 0)
                {
                    result_R = 0;
                }

                if (result_A > 255)
                {
                    result_A = 255;
                }
                else if (result_A < 0)
                {
                    result_A = 0;
                }

                return new MyColor((byte)result_R, (byte)result_G, (byte)result_B, (byte)result_A);


            }
            else
            {
                return reader.ReadOnePixel();
            }
        }
    }
}