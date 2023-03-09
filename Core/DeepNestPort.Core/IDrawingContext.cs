using SkiaSharp;

namespace DeepNestPort.Core
{
    public interface IDrawingContext
    {
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
        void ResetMatrix();
        
        void FitToPoints(PointF[] points, int gap = 0);
        void DrawLineTransformed(PointF point, PointF point2);        
        void DrawLine(float x0, float y0, float x1, float y1);
        void Clear(Color white);
        void DrawLine(PointF pp, PointF pp2);
        void ZoomOut();
        void ZoomIn();
        void PanX(int v);
        void PanY(int v);
    }
}