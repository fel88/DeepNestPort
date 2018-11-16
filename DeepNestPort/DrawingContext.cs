using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepNestPort
{
    public class DrawingContext
    {
        public float sx, sy;
        public float zoom = 1;
        public Graphics gr;
        public Bitmap bmp;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }

        public void Create(PictureBox pb)
        {
            bmp = new Bitmap(pb.Width, pb.Height);
            pb.SizeChanged += Pb_SizeChanged;
            gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            _pictureBox = pb;
        }
        PictureBox _pictureBox;
        private void Pb_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(_pictureBox.Width, _pictureBox.Height);
            gr = Graphics.FromImage(bmp);
        }
    }
}
