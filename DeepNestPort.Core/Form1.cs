using System.Diagnostics;
using static DeepNestPort.Core.RibbonMenuWpf;
using System.IO;
using DeepNestLib;
using System.Windows.Forms;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Drawing.Drawing2D;
using SkiaSharp;

namespace DeepNestPort.Core
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            sheetsInfos.Add(new SheetLoadInfo() { Nfp = NewSheet(), Width = 3000, Height = 1500, Quantity = 10 });

            Load += Form1_Load;
            Form = this;
            ctx3 = new DrawingContext(pictureBox3);

        }
        public Sheet NewSheet(int w = 3000, int h = 1500)
        {
            var tt = new RectangleSheet();
            tt.Name = "rectSheet" + (sheets.Count + 1);
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();

            return tt;
        }

        RibbonMenu menu;
        public static Form1 Form;
        IDrawingContext ctx;
        Control RenderControl;
        private void Form1_Load(object? sender, EventArgs e)
        {

            objectListView1.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Name", Width = 250, AspectName = "Name" });
            objectListView1.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Quantity", IsEditable = true, Width = 100, AspectName = "Quantity" });
            menu = new RibbonMenu();

            tableLayoutPanel1.Controls.Add(menu, 0, 0);
            menu.AutoSize = true;
            menu.Dock = DockStyle.Top;

            ctx = new SkiaGLDrawingContext();
            ctx.PaintAction = () => { Render(); };
            RenderControl = ctx.GenerateRenderControl();
            RenderControl.Visible = false;
            //tableLayoutPanel1. Controls.Add(RenderControl,0,1);
            RenderControl.Dock = DockStyle.Fill;
            ctx.Init(RenderControl);

        }
        void Render()
        {
            var sw = Stopwatch.StartNew();

            ctx.Clear(System.Drawing.Color.White); //same thing but also erases anything else on the canvas first


            ctx.UpdateDrag();
            //subSnapType = SubSnapTypeEnum.None;
            //  updateNearest();
            ctx.SetPen(Pens.Blue);
            ctx.DrawLineTransformed(new PointF(0, 0), new PointF(0, 100));
            ctx.SetPen(Pens.Red);
            ctx.DrawLineTransformed(new PointF(0, 0), new PointF(100, 0));


            //ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;


            //ctx.Reset();

            // ctx.gr.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
            // ctx.gr.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));
            int yy = 0;
            int gap = (int)Font.Size;
            foreach (var item in polygons)
            {
                item.Z = 0;
            }
            foreach (var item in polygons.Union(sheets).OrderBy(z => z.Z))
            {
                //if (!checkBox1.Checked)
                //  continue;

                if (!(item is Sheet))
                    if (!item.fitted) continue;

                SKPath path = new SKPath();
                if (item.Points != null && item.Points.Any())
                {
                    //rotate first;
                    var m = new Matrix();
                    m.Translate((float)item.x, (float)item.y);
                    m.Rotate(item.rotation);

                    var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    m.TransformPoints(pnts);
                    
                    path.AddPoly(pnts.Select(z => ctx.TransformSK(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            path.AddPoly(pnts2.Select(z => ctx.TransformSK(z)).ToArray());

                        }
                    }

                    ctx.ResetMatrix();
                    if (!sheets.Contains(item))
                    {
                        //bool hovered = item == hoveredNfp;
                       // if (hovered || dragNfp == item)
                        //    ctx.FillPath(new SolidBrush(Color.Blue), path);
                       // else
                            ctx.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                    }
                    ctx.DrawPath(Pens.Black, path);

                    if (item is Sheet)
                    {
                        if (nest != null && nest.nests.Any())
                        {
                            var fr = nest.nests.First();
                            double tot1 = 0;
                            double tot2 = 0;
                            bool was = false;
                            foreach (var zitem in fr.placements.First())
                            {
                                var sheetid = zitem.sheetId;
                                if (sheetid != item.id) continue;
                                var sheet = sheets.FirstOrDefault(z => z.id == sheetid);
                                if (sheet != null)
                                {
                                    tot1 += Math.Abs(GeometryUtil.polygonArea(sheet));
                                    was = true;
                                    foreach (var ssitem in zitem.sheetplacements)
                                    {

                                        var poly = polygons.FirstOrDefault(z => z.id == ssitem.id);
                                        if (poly != null)
                                        {
                                            tot2 += Math.Abs(GeometryUtil.polygonArea(poly));
                                        }
                                    }
                                }
                            }
                            var res = Math.Abs(Math.Round((100.0) * (tot2 / tot1), 2));
                            var trans1 = ctx.Transform(new PointF((float)pnts[0].X, (float)pnts[0].Y - 30));
                            /*if (was && isInfoShow)
                            {
                                ctx.gr.DrawString("util: " + res + "%", Font, Brushes.Black, trans1);
                            }*/
                        }
                    }
                }
            }
            //ctx.Setup();
        }
        public void AddDetail()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            //ofd.FilterIndex = lastOpenFilterIndex;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                // lastOpenFilterIndex = ofd.FilterIndex;
                try
                {
                    //try to load
                    if (ofd.FileNames[i].ToLower().EndsWith("dxf"))
                        DxfParser.LoadDxf(ofd.FileNames[i]);

                    if (ofd.FileNames[i].ToLower().EndsWith("svg"))
                        SvgParser.LoadSvg(ofd.FileNames[i]);

                    var fr = Infos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
                    if (fr != null)
                        fr.Quantity++;
                    else
                        Infos.Add(new DetailLoadInfo() { Quantity = 1, Name = new FileInfo(ofd.FileNames[i]).Name, Path = ofd.FileNames[i] });

                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"{ofd.FileNames[i]}: {ex.Message}", "Info");
                }

            }
            UpdateInfos();
        }

        public List<DetailLoadInfo> Infos = new List<DetailLoadInfo>();

        public void RedrawPreview(DrawingContext ctx, object previewObject)
        {
            ctx.Update();
            ctx.Clear(System.Drawing.Color.White);

            //ctx2.gr.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx2.Transform(100, 0));
            //ctx2.gr.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx2.Transform(0, 100));
            ctx.Reset();
            if (previewObject != null)
            {
                RectangleF? bnd = null;
                if (previewObject is RawDetail raw)
                {
                    var _nfp = raw.ToNfp();

                    ctx.Draw(raw, Pens.Black, Brushes.LightBlue);
                    /*  if (drawSimplification)
                      {
                          NFP s = lastSimplifiedResult;
                          if (lastSimplified != raw)
                          {
                              s = SvgNest.simplifyFunction(_nfp, false);
                              lastSimplifiedResult = s;
                              lastSimplified = raw;
                          }

                          ctx.Draw(s, Pens.Red);
                          var pointsChange = $"{_nfp.Points.Length} => {s.Points.Length} points";
                          ctx.DrawLabel(pointsChange, Brushes.Black, Color.Orange, 5, (int)(10 + ctx.GetLabelHeight()));
                      }*/
                    bnd = raw.BoundingBox();
                }
                else if (previewObject is NFP nfp)
                {
                    var g = ctx.Draw(nfp, Pens.Black, System.Drawing.Brushes.LightBlue);
                    bnd = g.GetBounds();
                    bnd = new RectangleF(0, 0, bnd.Value.Width / ctx.zoom, bnd.Value.Height / ctx.zoom);
                }

                var cap = $"{bnd.Value.Width:N2} x {bnd.Value.Height:N2}";
                ctx.DrawLabel(cap, Brushes.Black, Color.LightGreen, 5, 5);
            }
            ctx.Setup();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            RedrawPreview(ctx3, Preview);
            if (!RenderControl.Visible)
                return;

            RenderControl.Refresh();
        }

        NestingContext context = new NestingContext();
        List<SheetLoadInfo> sheetsInfos = new List<SheetLoadInfo>();
        List<NFP> sheets { get { return context.Sheets; } }

        List<NFP> polygons { get { return context.Polygons; } }
        internal void RunNest()
        {
            context = new NestingContext();
            int src = 0;
            foreach (var item in sheetsInfos)
            {
                src = context.GetNextSheetSource();
                for (int i = 0; i < item.Quantity; i++)
                {
                    var ns = Background.clone(item.Nfp);
                    Sheet sheet = new Sheet();
                    sheet.Points = ns.Points;
                    sheet.children = ns.children;
                    sheets.Add(sheet);
                    sheet.Width = sheet.WidthCalculated;
                    sheet.Height = sheet.HeightCalculated;
                    sheet.source = src;
                }
            }

            context.ReorderSheets();

            src = 0;
            foreach (var item in Infos)
            {
                RawDetail det = null;
                if (item.Path.ToLower().EndsWith("dxf"))
                {
                    det = DxfParser.LoadDxf(item.Path);
                }
                else if (item.Path.ToLower().EndsWith("svg"))
                {
                    det = SvgParser.LoadSvg(item.Path);
                }

                for (int i = 0; i < item.Quantity; i++)
                {
                    context.ImportFromRawDetail(det, src);
                }
                src++;
            }
            menu.TabIndex = 1;
            RenderControl.Visible = true;
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel2);
            tableLayoutPanel1.Controls.Add(RenderControl, 0, 1);
            tableLayoutPanel2.Visible = false;
            run();
        }
        void run()
        {
            if (sheets.Count == 0 || polygons.Count == 0)
            {
                MessageBox.Show("There are no sheets or parts", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            stop = false;
            // progressBar1.Value = 0;
            //tabControl1.SelectedTab = tabPage4;
            context.ReorderSheets();
            RunDeepnest();
        }
        Thread th;
        internal void displayProgress(float progress)
        {
            progressVal = progress;


        }
        public void UpdateInfos()
        {
            objectListView1.SetObjects(Infos);
        }
        public float progressVal = 0;
        bool stop = false;
        public void RunDeepnest()
        {
            if (th != null) return;

            th = new Thread(() =>
            {
                context.StartNest();
                UpdateNestsList();
                Background.displayProgress = displayProgress;

                while (true)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    context.NestIterate();
                    UpdateNestsList();
                    displayProgress(1.0f);
                    sw.Stop();
                    toolStripStatusLabel1.Text = "Nesting time: " + sw.ElapsedMilliseconds + "ms";
                    if (stop) break;
                }
                th = null;
            });
            th.IsBackground = true;
            th.Start();

        }
        public SvgNest nest { get { return context.Nest; } }

        public void UpdateNestsList()
        {

            if (nest != null)
            {
                /*listView4.Invoke((Action)(() =>
                {
                    listView4.BeginUpdate();
                    listView4.Items.Clear();
                    foreach (var item in nest.nests)
                    {
                        listView4.Items.Add(new ListViewItem(new string[] { item.fitness == null ? "(null)" : item.fitness.Value.ToString("N5") }) { Tag = item });
                    }
                    listView4.EndUpdate();
                }));*/
            }
        }

        object Preview;
        bool autoFit = true;
        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject == null) return;
            var path = (objectListView1.SelectedObject as DetailLoadInfo).Path.ToLower();
            if (path.EndsWith("dxf"))
            {
                Preview = DxfParser.LoadDxf(path);
            }
            else if (path.EndsWith("svg"))
            {
                Preview = SvgParser.LoadSvg(path);
            }
            if (autoFit)
                fitAll();

        }
        public DrawingContext ctx3;
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
        void fitAll()
        {
            if (Preview == null) return;
            if (Preview is RawDetail raw)
            {
                GraphicsPath gp = new GraphicsPath();
                foreach (var item in raw.Outers)
                {
                    gp.AddPolygon(item.Points.ToArray());
                }
                ctx3.FitToPoints(gp.PathPoints, 5);
            }
            if (Preview is NFP nfp)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon(nfp.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray());
                ctx3.FitToPoints(gp.PathPoints, 5);
            }
        }

        internal void StopNesting()
        {
            stop = true;
        }
    }
}