using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        Bitmap bmp;
               
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            if (o.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    bmp = new Bitmap(o.FileName);
                    canvas1.CanvasSize = new Size(bmp.Size.Width + 200, bmp.Size.Height + 200);
                    canvas1.ImageLocation = new Point(100, 100);
                    canvas1.CanvasImage = bmp;
                }
                catch
                {
                    bmp = null;
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bmp = canvas1.CanvasImage;

            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "PNG Image|*.png|TIFF Image|*.tiff";
            saveFileDialog1.Title = "Save an Image File";

            // If the file name is not an empty string open it for saving.
            if (saveFileDialog1.ShowDialog() == DialogResult.OK && saveFileDialog1.FileName != "")
            {
                // Saves the Image via a FileStream created by the OpenFile method.
                System.IO.FileStream fs =
                   (System.IO.FileStream)saveFileDialog1.OpenFile();
                // Saves the Image in the appropriate ImageFormat based upon the
                // File type selected in the dialog box.
                // NOTE that the FilterIndex property is one-based.
                switch (saveFileDialog1.FilterIndex)
                {
                    case 1:
                        bmp.Save(fs,
                            System.Drawing.Imaging.ImageFormat.Png);
                        break;

                    case 2:
                        bmp.Save(fs,
                            System.Drawing.Imaging.ImageFormat.Tiff);
                        break;
                }

                fs.Close();
            }
        }

        private void configToolStripMenuItem_Click(object sender, EventArgs e)
        {
            YLScsDrawing.Forms.DialogConfig dia = new YLScsDrawing.Forms.DialogConfig();
            dia.CanvasColor = canvas1.CanvasBackColor;
            dia.CanvasWidth = canvas1.CanvasSize.Width;
            dia.CanvasHeight = canvas1.CanvasSize.Height;
            dia.IsBilineInterpolation = canvas1.IsBilinearInterpolation;
            if (dia.ShowDialog() == DialogResult.OK)
            {
                canvas1.IsBilinearInterpolation = dia.IsBilineInterpolation;
                canvas1.CanvasSize = new Size(dia.CanvasWidth, dia.CanvasHeight);
                canvas1.CanvasBackColor = dia.CanvasColor;
            }
        }
    }
}