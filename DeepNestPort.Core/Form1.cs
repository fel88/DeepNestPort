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
            sheetsInfos.Add(new SheetLoadInfo() { Nfp = NewSheet(), Width = 3000, Height = 1500, Quantity = 1 });

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

            objectListView2.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Width", IsEditable = true, Width = 100, AspectName = "Width" });
            objectListView2.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Height", IsEditable = true, Width = 100, AspectName = "Height" });
            objectListView2.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Quantity", IsEditable = true, Width = 100, AspectName = "Quantity" });
            objectListView2.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Info", IsEditable = true, Width = 100, AspectName = "Info" });
            menu = new RibbonMenu();
            menu.TabChanged += Menu_TabIndexChanged;


            tableLayoutPanel1.Controls.Add(menu, 0, 0);
            menu.AutoSize = true;
            menu.Dock = DockStyle.Top;

            ctx = new SkiaGLDrawingContext();


            updateSheetInfos();
            ctx.PaintAction = () => { Render(); };
            RenderControl = ctx.GenerateRenderControl();
            RenderControl.Visible = false;
            //tableLayoutPanel1. Controls.Add(RenderControl,0,1);
            RenderControl.Dock = DockStyle.Fill;
            ctx.Init(RenderControl);
            /*ctx.FitToPoints(new PointF[] {
                ctx.Transform( new PointF(0, 0)),
                ctx.Transform( new PointF(0, 1500)),
                ctx.Transform( new PointF(3000, 1500)),
                ctx.Transform( new PointF(3000, 0) ) });*/

            (ctx as SkiaGLDrawingContext).sx = 650;
            (ctx as SkiaGLDrawingContext).sy = -1570;
            (ctx as SkiaGLDrawingContext).zoom = 0.28f;
        }

        NFP dragNfp = null;
        NFP hoveredNfp = null;
        void updateSheetInfos()
        {
            objectListView2.SetObjects(sheetsInfos);
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
                    if (!item.fitted)
                        continue;

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
                            path.AddPoly(pnts2.Select(z => ctx.TransformSK(z)).Reverse().ToArray());                            
                        }
                    }

                    ctx.ResetMatrix();
                    if (!sheets.Contains(item))
                    {
                        bool hovered = item == hoveredNfp;
                        if (hovered || dragNfp == item)
                            ctx.FillPath(new SolidBrush(Color.Blue), path);
                        else
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
            toolStripProgressBar1.Value = (int)Math.Round(progressVal * 100f);

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


            menu.SetTab(menu.NestTab);
            /*RenderControl.Visible = true;
            tableLayoutPanel1.Controls.Remove(tableLayoutPanel2);
            tableLayoutPanel1.Controls.Add(RenderControl, 0, 1);
            tableLayoutPanel2.Visible = false;*/
            run();
        }

        private void Menu_TabIndexChanged()
        {
            if (menu.GeneralTab.IsSelected && RenderControl.Visible)
            {
                RenderControl.Visible = false;
                tableLayoutPanel1.Controls.Remove(RenderControl);
                tableLayoutPanel1.Controls.Add(panel1, 0, 1);
                panel1.Visible = true;
            }
            else
            if (menu.NestTab.IsSelected && !RenderControl.Visible)
            {
                RenderControl.Visible = true;
                tableLayoutPanel1.Controls.Remove(panel1);
                tableLayoutPanel1.Controls.Add(RenderControl, 0, 1);
                panel1.Visible = false;
            }
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
        public int MaxNestSeconds = 5;
        public void RunDeepnest()
        {
            if (th != null) return;

            th = new Thread(() =>
            {
                context.StartNest();
                UpdateNestsList();
                Background.displayProgress = displayProgress;
                Stopwatch sww = new Stopwatch();
                while (true)
                {
                    /*if (sww.Elapsed.TotalSeconds > MaxNestSeconds)
                    {
                        break;
                    }*/
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    context.NestIterate();
                    UpdateNestsList();
                    displayProgress(1.0f);
                    sw.Stop();
                    toolStripStatusLabel1.Text = "Nesting time: " + sw.ElapsedMilliseconds + "ms";
                    //if (stop)
                        break;

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

        internal void Export()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            //sfd.FilterIndex = lastSaveFilterIndex;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // lastSaveFilterIndex = sfd.FilterIndex;
                if (sfd.FilterIndex == 1)
                    DxfParser.Export(sfd.FileName, polygons.ToArray(), sheets.ToArray());

                if (sfd.FilterIndex == 2)
                    SvgParser.Export(sfd.FileName, polygons.ToArray(), sheets.ToArray());
            }
        }
        public DialogResult ShowQuestion(string text)
        {
            return MessageBox.Show(text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }
        void deleteParts()
        {
            if (objectListView1.SelectedObjects.Count == 0) return;
            if (ShowQuestion($"Are you to sure to delete {objectListView1.SelectedObjects.Count} items?") == DialogResult.No) return;
            foreach (var item in objectListView1.SelectedObjects)
            {
                if (Preview != null && (item as DetailLoadInfo).Path == (Preview as RawDetail).Name) Preview = null;
                Infos.Remove(item as DetailLoadInfo);
            }
            objectListView1.SetObjects(Infos);
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteParts();

        }

        private void objectListView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                deleteParts();
            }
        }
        public void ShowMessage(string text, MessageBoxIcon type)
        {
            MessageBox.Show(text, Text, MessageBoxButtons.OK, type);
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (Infos.Count == 0) { ShowMessage("There are no parts.", MessageBoxIcon.Warning); return; }
            if (ShowQuestion("Are you to sure to delete all items?") == DialogResult.No) return;
            Infos.Clear();
            objectListView1.SetObjects(Infos);
            Preview = null; 
        }

        private void detailToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddDetail();
        }

        private void setToToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObjects.Count == 0) return;
            QntDialog q = new QntDialog();
            q.ShowDialog();

            foreach (var item in objectListView1.SelectedObjects)
            {
                (item as DetailLoadInfo).Quantity = q.Qnt;
            }
            objectListView1.RefreshObjects(objectListView1.SelectedObjects);
        }
    }
}