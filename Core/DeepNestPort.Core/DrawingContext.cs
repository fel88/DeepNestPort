using DeepNestLib;
using System.Drawing.Drawing2D;

namespace DeepNestPort.Core
{
    public partial class Form1
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

            public bool EnableWheel = true;
            GraphicsPath getGraphicsPath(NFP nfp)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon(nfp.Points.Select(z => Transform(z.x, z.y)).ToArray());
                if (nfp.children != null)
                {
                    foreach (var item in nfp.children)
                    {
                        gp.AddPolygon(item.Points.Select(z => Transform(z.x, z.y)).ToArray());
                    }
                }
                return gp;
            }
            GraphicsPath getGraphicsPath(RawDetail det)
            {
                GraphicsPath gp = new GraphicsPath();
                foreach (var item in det.Outers)
                {
                    gp.AddPolygon(item.Points.Select(z => Transform(z)).ToArray());
                }
                return gp;
            }

            public float GetLabelHeight()
            {
                return SystemFonts.DefaultFont.GetHeight();
            }
            public GraphicsPath Draw(RawDetail det, Pen pen = null, Brush brush = null)
            {
                var gp = getGraphicsPath(det);
                if (brush != null)
                    gr.FillPath(brush, gp);
                if (pen != null)
                    gr.DrawPath(pen, gp);
                return gp;
            }
            public GraphicsPath Draw(NFP nfp, Pen pen = null, Brush brush = null)
            {
                var gp = getGraphicsPath(nfp);
                if (brush != null)
                    gr.FillPath(brush, gp);
                if (pen != null)
                    gr.DrawPath(pen, gp);
                return gp;
            }

            public SizeF DrawLabel(string text, Brush fontBrush, Color backColor, int x, int y, int opacity = 128)
            {
                var ms = gr.MeasureString(text, SystemFonts.DefaultFont);
                gr.FillRectangle(new SolidBrush(Color.FromArgb(opacity, backColor)), x, y, ms.Width, ms.Height);
                gr.DrawString(text, SystemFonts.DefaultFont, fontBrush, x, y);
                return ms;
            }

            protected virtual void Pb_MouseWheel(object sender, MouseEventArgs e)
            {
                if (!EnableWheel) return;
                float zold = zoom;
                if (e.Delta > 0) { zoom *= 1.5f; ; }
                else { zoom *= 0.5f; }
                if (zoom < 0.08) { zoom = 0.08f; }
                if (zoom > 1000) { zoom = 1000f; }

                var pos = box.PointToClient(Cursor.Position);

                sx = -(pos.X / zold - sx - pos.X / zoom);
                sy = (pos.Y / zold + sy - pos.Y / zoom);
            }

            public bool FocusOnMove = true;
            private void Pb_MouseMove(object sender, MouseEventArgs e)
            {
                if (!FocusOnMove) return;
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

            internal void Clear(System.Drawing.Color color)
            {
                gr.Clear(color);
            }

            float startx, starty;

            internal void Reset()
            {
                gr.ResetTransform();
            }

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
            public virtual PointF BackTransform(PointF p1)
            {
                return new PointF((p1.X / zoom - sx), (InvertY ? (-1) : 1) * (p1.Y / zoom - sy));
            }
            public virtual PointF Transform(double x, double y)
            {
                return new PointF(((float)(x) + sx) * zoom, (InvertY ? (-1) : 1) * ((float)(y) + sy) * zoom);
            }

            private void Pb_SizeChanged(object sender, EventArgs e)
            {
                if (box.Width <= 0 || box.Height <= 0) return;
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

            public void FitToPoints(PointF[] points, int gap = 0)
            {
                var maxx = points.Max(z => z.X) + gap;
                var minx = points.Min(z => z.X) - gap;
                var maxy = points.Max(z => z.Y) + gap;
                var miny = points.Min(z => z.Y) - gap;

                var w = box.Width;
                var h = box.Height;

                var dx = maxx - minx;
                var kx = w / dx;
                var dy = maxy - miny;
                var ky = h / dy;

                var oz = zoom;
                var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
                var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
                zoom = kx;
                if (sz1.Width > w || sz1.Height > h) zoom = ky;

                var x = dx / 2 + minx;
                var y = dy / 2 + miny;

                sx = ((w / 2f) / zoom - x);
                sy = -((h / 2f) / zoom + y);

                var test = Transform(new PointF(x, y));

            }
        }
    }
}