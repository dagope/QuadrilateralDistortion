//http://www.vcskicks.com/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsApplication1
{
    public partial class frmSample : Form
    {
        public frmSample()
        {
            InitializeComponent();

            pImage.BackColor = Color.Transparent;
        }
        Point[] corners;
        Bitmap distorted;
        Bitmap source;
        int moveIndex = -1;

        private void updateImage()
        {
            if (source == null)
                return; //no image selected yet

            if (distorted == null)
                distorted = new Bitmap(pImage.Width, pImage.Height);

            distorted.MakeTransparent();

            //The final display buffer
            Bitmap display = new Bitmap(distorted.Width + 5, distorted.Height + 5);
            display.MakeTransparent();

            Graphics g = Graphics.FromImage(display);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
            
            //Draw a rectangle around each corner
            g.DrawImage(distorted, 0, 0);

            foreach (Point corner in corners)
            {
                int width = 5;
                Rectangle rect = new Rectangle(corner.X - width, corner.Y - width, width * 2, width * 2);                
                g.DrawRectangle(Pens.Red, rect);
            }

            pImage.Image = display as Image; //update display

            g.Dispose(); //clean up
        }

        private double GetDistance(Point A, Point B)
        {
            double a = (double)A.X - (double)B.X;
            double b = (double)A.Y - (double)B.Y;
            return Math.Sqrt(a * a + b * b);
        }

        private void pImage_MouseDown(object sender, MouseEventArgs e)
        {
            if (corners == null)
                return; //no image loaded

            //Find the closest of the four corner points
            Dictionary<double, Point> values = new Dictionary<double, Point>();

            Point mouse = new Point(e.X, e.Y);

            //Adds all the distances to a list
            foreach (Point corner in corners)
            {
                values.Add(GetDistance(mouse, corner), corner);
            }

            List<double> distances = new List<double>(values.Keys);
            distances.Sort(); //Sort to make the first element the smallest distance

            if (distances[0] > 20) return; //too far

            Point closetPoint = values[distances[0]];
            
            //find the point index
            for (int i = 0; i < corners.Length; i++)
            {
                if (corners[i].X == closetPoint.X && corners[i].Y == closetPoint.Y)
                {
                    moveIndex = i;
                    break;
                }
            }
        }

        private void pImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (moveIndex != -1)
                {
                    corners[moveIndex].X = e.X;
                    corners[moveIndex].Y = e.Y;

                    //Distort the image
                    //Fast interpolation
                    this.Cursor = Cursors.WaitCursor;
                    distorted = QuadrilateralDistortion.QuadDistort.Distort(source, corners[0], corners[1], corners[2], corners[3], 1);
                    this.Cursor = Cursors.Default;

                    updateImage();
                }
            }
        }

        private void pImage_MouseUp(object sender, MouseEventArgs e)
        {
            if (source == null)
                return; //no image loaded yet

            //Release the point
            moveIndex = -1;

            //Smooth the distortion
            //Better interpolation
            this.Cursor = Cursors.WaitCursor;
            distorted = QuadrilateralDistortion.QuadDistort.Distort(source, corners[0], corners[1], corners[2], corners[3], 3);
            updateImage();
            this.Cursor = Cursors.Default;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.vcskicks.com/");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                Bitmap test = new Bitmap(openFile.FileName);
                //source = new Bitmap(openFile.FileName);
                test.MakeTransparent();
                source = test.Clone(new Rectangle(0, 0, test.Width, test.Height), System.Drawing.Imaging.PixelFormat.Format32bppArgb);                
                
                source.MakeTransparent();
                corners = new Point[4];
                corners[0] = new Point(0, 0);
                corners[1] = new Point(source.Width, 0);
                corners[2] = new Point(0, source.Height);
                corners[3] = new Point(source.Width, source.Height);

                updateImage();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                distorted.Save(saveFile.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Picture Saved!");
            }
        } 
    }
}