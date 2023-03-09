using SkiaSharp;
using System.Windows.Media;
using System.Drawing;

namespace DeepNestPort.Core
{
    public abstract class AbstractDrawingContext : IDrawingContext
    {
        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }
        public abstract void SetPen(System.Drawing.Pen pen);

        public void FitToPoints(PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = PictureBox.Control.Width;
            var h = PictureBox.Control.Height;

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
        public float scale = 1;
        public SkiaSharp.SKSurface Surface;
        public object Tag { get; set; }

        public float startx { get; set; }
        public float starty { get; set; }
        public float sx { get; set; }
        public float sy { get; set; }
        public abstract void DrawLineTransformed(PointF point, PointF point2);

        public float origsx, origsy;
        public bool isDrag = false;
        public bool isMiddleDrag { get; set; } = false;
        public bool isLeftDrag { get; set; } = false;
        public bool MiddleDrag { get { return isMiddleDrag; } }

        public float zoom { get; set; } = 1;

        public PointF GetCursor()
        {
            var p = PictureBox.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }
        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;
            isMiddleDrag = false;
            isLeftDrag = false;

            var tt = BackTransform(e.X, e.Y);
            MouseUp?.Invoke(tt.X, tt.Y, e.Button);
        }

        public event Action<float, float, MouseButtons> MouseUp;
        public event Action<float, float, MouseButtons> MouseDown;
        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }
        public virtual PointF Transform(float x, float y)
        {
            return new PointF((x + sx) * zoom, -(y + sy) * zoom);
        }
        public virtual PointF Transform(double x, double y)
        {
            return new PointF((float)((x + sx) * zoom), (float)(-(y + sy) * zoom));
        }


        public virtual PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
        public virtual PointF BackTransform(float x, float y)
        {
            var posx = (x / zoom - sx);
            var posy = (-y / zoom - sy);
            return new PointF(posx, posy);
        }
        public EventWrapperPictureBox PictureBox { get; set; }
        public Action PaintAction { get; set; }

        public void Init(Control pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }
        public MouseButtons DragButton { get; set; } = MouseButtons.Right;
        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == DragButton)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
            if (e.Button == MouseButtons.Middle)
            {
                isMiddleDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
            if (e.Button == MouseButtons.Left)
            {
                //isLeftDrag= true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }

            var tt = BackTransform(e.X, e.Y);
            MouseDown?.Invoke(tt.X, tt.Y, e.Button);
        }

        public void ResetView()
        {
            zoom = 1;
            sx = 0;
            sy = 0;
        }
        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;

            pb.SizeChangedAction = Pb_SizeChanged;
        }

        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            InitGraphics();
        }

        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //zoom *= Math.Sign(e.Delta) * 1.3f;
            //zoom += Math.Sign(e.Delta) * 0.31f;

            var pos = PictureBox.Control.PointToClient(Cursor.Position);
            if (!PictureBox.Control.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }
        public abstract void InitGraphics();

        public abstract void DrawPath(System.Drawing.Pen p, SKPath path);
        public abstract void FillPath(System.Drawing.Brush p, SKPath path);

        public abstract SizeF MeasureString(string text, Font font);

        public abstract void DrawLine(float x0, float y0, float x1, float y1);


        public abstract void DrawLine(PointF pp, PointF pp2);

        public abstract void DrawRectangle(float rxm, float rym, float rdx, float rdy);

        public abstract void Clear(System.Drawing.Color white);

        public abstract Control GenerateRenderControl();

        public abstract void DrawImage(Bitmap image, float x1, float y1, float x2, float y2);

        public abstract void ResetMatrix();

        public abstract void RotateDegress(float deg);

        public abstract void Translate(double x, double y);
        public abstract void Scale(double x, double y);

        public abstract void PushMatrix();

        public abstract void PopMatrix();

        public SKPoint TransformSK(PointF p1)
        {
            return new SKPoint((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);

        }

        public void ZoomOut()
        {
            zoom /= ZoomFactor;

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }
        }

        public void ZoomIn()
        {
            zoom *= ZoomFactor;

            if (zoom < 0.0008) { zoom = 0.0008f; }
            if (zoom > 10000) { zoom = 10000f; }
        }

        public void PanX(int v)
        {
            sx += v;
        }
        public void PanY(int v)
        {
            sy += v;
        }
    }
}