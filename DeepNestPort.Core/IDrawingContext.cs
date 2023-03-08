using SkiaSharp;

namespace DeepNestPort.Core
{
    public interface IDrawingContext
    {
        /*event Action<float, float, MouseButtons> MouseUp;
        event Action<float, float, MouseButtons> MouseDown;
        void DrawPolygon(Pen p, PointF[] pointFs);*/
        void FillPath(Brush b, SKPath path);
        void DrawPath(Pen b, SKPath path);
        
        bool MiddleDrag { get; }
        Action PaintAction { get; set; }
        void SetPen(Pen pen);

        MouseButtons DragButton { get; set; }
        PointF GetCursor();
        void Init(Control pb);

        PointF Transform(PointF p1);
        SKPoint TransformSK(PointF p1);
        PointF Transform(float x, float y);
        PointF Transform(double x, double y);
        //PointF Transform(Vector2d p1);
        
        void UpdateDrag();
        Control GenerateRenderControl();
        void ResetView();
        void ResetMatrix();/*
        void RotateDegress(float deg);
        void Translate(double x, double y);
        void Scale(double x, double y);
        void PushMatrix();
        void PopMatrix();
        object Tag { get; set; }
        //EventWrapperPictureBox PictureBox { get; set; }
        void InitGraphics();
        bool isMiddleDrag { get; set; }
        bool isLeftDrag { get; set; }
        float startx { get; set; }
        float starty { get; set; }
        float sx { get; set; }
        float sy { get; set; }
        */
        void FitToPoints(PointF[] points, int gap = 0);

        void DrawLineTransformed(PointF point, PointF point2);
        /*
        PointF BackTransform(PointF p1);
        PointF BackTransform(float x, float y);
        float zoom { get; set; }
        void FillCircle(Brush brush, float v1, float v2, int rad);
        void DrawCircle(Pen pen, float v1, float v2, float rad);
        void DrawCircle(Pen pen, float v1, float v2, float rad, int angles, float startAngle);
        SizeF MeasureString(string text, Font font);
        void DrawString(string text, Font font, Brush brush, PointF position);
        void DrawString(string text, Font font, Brush brush, float x, float y);
        */
        void DrawLine(float x0, float y0, float x1, float y1);
        void Clear(Color white);
        void DrawLine(PointF pp, PointF pp2);/*
        void FillRoundRectangle(Brush blue, SKRoundRect rr);
        void DrawArrowedLine(Pen p, PointF tr0, PointF tr1, int v);
        void DrawRoundRectangle(Pen pen, SKRoundRect rect);
        void DrawRectangle(float rxm, float rym, float rdx, float rdy);

        void FillRectangle(Brush blue, float v1, float v2, float v3, float v4);
        void DrawImage(Bitmap image, float x1, float y1, float x2, float y2);
   */ }
}