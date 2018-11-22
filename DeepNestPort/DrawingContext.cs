using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DeepNestPort
{
    public class DrawingContext
    {
        public DrawingContext(PictureBox pb)
        {
            box = pb;

            bmp = new Bitmap(pb.Width, pb.Height);
            pb.SizeChanged += Pb_SizeChanged;
            gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = SmoothingMode.AntiAlias;
            box.Image = bmp;

            pb.MouseDown += PictureBox1_MouseDown;
            pb.MouseUp += PictureBox1_MouseUp;
            pb.MouseMove += Pb_MouseMove;
            sx = box.Width / 2;
            sy = -box.Height / 2;
            pb.MouseWheel += Pb_MouseWheel;
        }

        private void Pb_MouseWheel(object sender, MouseEventArgs e)
        {
            float zold = zoom;
            if (e.Delta > 0) { zoom *= 1.5f; ; }
            else { zoom *= 0.5f; }
            if (zoom < 0.08) { zoom = 0.08f; }
            if (zoom > 1000) { zoom = 1000f; }

            var pos = box.PointToClient(Cursor.Position);

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        private void Pb_MouseMove(object sender, MouseEventArgs e)
        {
            box.Focus();
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;

            var p = box.PointToClient(Cursor.Position);
            var pos = box.PointToClient(Cursor.Position);
            var posx = (pos.X / zoom - sx);
            var posy = (-pos.Y / zoom - sy);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = box.PointToClient(Cursor.Position);
            var p = Transform(pos);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;
                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }
        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;

        PictureBox box;
        public float sx, sy;
        public float zoom = 1;
        public Graphics gr;
        public Bitmap bmp;
        public bool InvertY = true;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, (InvertY ? (-1) : 1) * (p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF(((float)(x) + sx) * zoom, (InvertY ? (-1) : 1) * ((float)(y) + sy) * zoom);
        }
        

        private void Pb_SizeChanged(object sender, EventArgs e)
        {
            bmp = new Bitmap(box.Width, box.Height);
            gr = Graphics.FromImage(bmp);

            box.Image = bmp;
        }

        public PointF GetPos()
        {
            var pos = box.PointToClient(Cursor.Position);
            var posx = (pos.X / zoom - sx);
            var posy = (-pos.Y / zoom - sy);

            return new PointF(posx, posy);
        }
        public void Update()
        {
            if (isDrag)
            {
                var p = box.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }

        public void Setup()
        {
            box.Invalidate();
        }
    }
}
