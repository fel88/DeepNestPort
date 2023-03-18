using System.Diagnostics;
using static DeepNestPort.Core.RibbonMenuWpf;
using System.IO;
using DeepNestLib;
using System.Windows.Forms;
using System.Text;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Drawing.Drawing2D;
using SkiaSharp;
using BrightIdeasSoftware;
using System.Windows.Media;
using OpenTK;
using IxMilia.Dxf.Entities;
using System.Xml.Linq;

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
            Shown += Form1_Shown;
            FormClosing += Form1_FormClosing;
            stgControl.Visible = false;
            stgControl.Close += StgControl_Close;
            panel1.Controls.Add(stgControl);
            stgControl.Dock = DockStyle.Fill;

            List<System.Drawing.Color> clrs = new List<System.Drawing.Color>();
            foreach (var item in Enum.GetValues(typeof(KnownColor)))
            {
                var cc = System.Drawing.Color.FromKnownColor((KnownColor)item);
                if (cc.R < 32 && cc.G < 32 && cc.B < 32)
                    continue;

                if (cc.R > 228 && cc.G > 228 && cc.B > 228)
                    continue;

                bool good = true;
                foreach (var color in clrs)
                {
                    if (((new Vector3(color.R, color.G, color.B)) - new Vector3(cc.R, cc.G, cc.B)).Length < 32) { good = false; break; }
                }
                if (!good)
                    continue;

                clrs.Add(cc);
            }

            Random rrr = new Random(234);
            clrs = clrs.OrderBy(z => rrr.Next(100000)).ToList();
            Colors = clrs.ToArray();
        }

        private void Form1_FormClosing(object? sender, FormClosingEventArgs e)
        {
            SaveSettings();
        }

        public void LoadSettings()
        {
            if (!File.Exists("settings.xml"))
                return;

            var doc = XDocument.Load("settings.xml");
            foreach (var item in doc.Descendants("setting"))
            {
                try
                {
                    var nm = item.Attribute("name").Value;
                    var vl = item.Attribute("value").Value;
                    switch (nm)
                    {
                        case "spacing":
                            SvgNest.Config.spacing = vl.ToDouble();
                            break;
                        case "sheetSpacing":
                            SvgNest.Config.sheetSpacing = vl.ToDouble();
                            break;
                        case "maxNestSeconds":
                            MaxNestSeconds = int.Parse(vl);
                            break;
                        case "useNestTimeLimit":
                            UseNestTimeLimit = bool.Parse(vl);
                            break;
                        case "borderScroll":
                            BorderScrollEnabled = bool.Parse(vl);
                            break;
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void SaveSettings()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine("<root>");
            sb.AppendLine($"<setting name=\"spacing\" value=\"{SvgNest.Config.spacing}\"/>");
            sb.AppendLine($"<setting name=\"sheetSpacing\" value=\"{SvgNest.Config.sheetSpacing}\"/>");
            sb.AppendLine($"<setting name=\"maxNestSeconds\" value=\"{MaxNestSeconds}\"/>");
            sb.AppendLine($"<setting name=\"useNestTimeLimit\" value=\"{UseNestTimeLimit}\"/>");
            sb.AppendLine($"<setting name=\"borderScroll\" value=\"{BorderScrollEnabled}\"/>");
            sb.AppendLine("</root>");
            File.WriteAllText("settings.xml", sb.ToString());
        }

        private void Form1_Shown(object? sender, EventArgs e)
        {
            LoadSettings();
            stgControl.InitValues();
            menu.ApplySettings();
        }

        System.Drawing.Color[] Colors;

        private void StgControl_Close()
        {
            SwitchSettingsPanel();
        }

        Settings stgControl = new Settings();
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
            //objectListView2.Columns.Add(new BrightIdeasSoftware.OLVColumn() { Text = "Info", IsEditable = true, Width = 100, AspectName = "Info" });

            ((OLVColumn)objectListView2.Columns[0]).AspectPutter = (e, x) =>
            {
                var w = (float)(double)x;
                if (w < 10)
                    return;

                ((SheetLoadInfo)e).Width = w;
            };

            ((OLVColumn)objectListView2.Columns[1]).AspectPutter = (e, x) =>
            {
                var h = (float)(double)x;
                if (h < 10)
                    return;

                ((SheetLoadInfo)e).Height = h;
            };
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
            panel1.Controls.Add(RenderControl);
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

        public bool UseColors { get; set; } = false;
        void updateSheetInfos()
        {
            objectListView2.SetObjects(sheetsInfos);
        }

        const int zoomSpeed = 40;
        void Render()
        {
            var pos = RenderControl.PointToClient(Cursor.Position);

            if (BorderScrollEnabled && ClientRectangle.Contains(pos))
            {
                if (pos.X >= 0 && pos.X < 15)
                    ctx.PanX(zoomSpeed);

                if (pos.X <= RenderControl.Width && pos.X > (RenderControl.Width - 15))
                    ctx.PanX(-zoomSpeed);

                if (pos.Y >= 0 && pos.Y < 5)
                    ctx.PanY(-zoomSpeed);

                if (pos.Y <= RenderControl.Height && pos.Y > (RenderControl.Height - 15))
                    ctx.PanY(zoomSpeed);
            }

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
            var nms = polygons.GroupBy(z => z.source.Value).ToArray();

            Dictionary<int, System.Drawing.Color> colors = new System.Collections.Generic.Dictionary<int, System.Drawing.Color>();
            if (UseColors)
            {
                for (int i = 0; i < nms.Length; i++)
                {
                    colors.Add(nms[i].Key, Colors[i]);
                }
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
                    var m = new System.Drawing.Drawing2D.Matrix();
                    m.Translate((float)item.x, (float)item.y);
                    m.Rotate(item.rotation);

                    var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    var sign1 = Math.Sign(GeometryUtil.polygonArea(item));
                    m.TransformPoints(pnts);

                    path.AddPoly(pnts.Select(z => ctx.TransformSK(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            var sign = Math.Sign(GeometryUtil.polygonArea(citem));
                            var pp = pnts2.Select(z => ctx.TransformSK(z));
                            if (sign == sign1)
                            {
                                pp = pp.Reverse();
                            }
                            path.AddPoly(pp.ToArray());
                        }
                    }

                    ctx.ResetMatrix();
                    if (!sheets.Contains(item))
                    {
                        bool hovered = item == hoveredNfp;
                        if (hovered || dragNfp == item)
                            //ctx.FillPath(new SolidBrush(System.Drawing.Color.Blue), path);
                            ctx.FillPath(new SolidBrush(UseColors ? colors[item.source.Value] : System.Drawing.Color.Blue), path);
                        else
                            //ctx.FillPath(new SolidBrush(System.Drawing.Color.FromArgb(128, System.Drawing.Color.LightBlue)), path);
                            ctx.FillPath(new SolidBrush(System.Drawing.Color.FromArgb(128, UseColors ? colors[item.source.Value] : System.Drawing.Color.LightBlue)), path);
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

        int lastOpenFilterIndex = 1;
        public void AddDetail()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            ofd.FilterIndex = lastOpenFilterIndex;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                lastOpenFilterIndex = ofd.FilterIndex;
                try
                {
                    //try to load
                    RawDetail[] d = null;
                    if (ofd.FileNames[i].ToLower().EndsWith("dxf"))
                        d = DxfParser.LoadDxf(ofd.FileNames[i], true);

                    if (ofd.FileNames[i].ToLower().EndsWith("svg"))
                        d = SvgParser.LoadSvg(ofd.FileNames[i]);

                    var fr = Infos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
                    if (fr != null)
                        fr.Quantity++;
                    else
                    {
                        bool split = d.Length > 1;
                        if (SplitMode == LoadDetailSplitMode.Ask && split && ShowQuestion($"File {ofd.FileNames[i]} contains {d.Length} separate parts. Do you want to split it to separate parts during nesting?") == DialogResult.No)
                        {
                            split = false;
                        }
                        Infos.Add(new DetailLoadInfo()
                        {
                            Quantity = 1,
                            SplitOnLoad = split,
                            Name = new FileInfo(ofd.FileNames[i]).Name,
                            Path = ofd.FileNames[i]
                        });
                    }

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

                    ctx.Draw(raw, Pens.Black, System.Drawing.Brushes.LightBlue);
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
                ctx.DrawLabel(cap, System.Drawing.Brushes.Black, System.Drawing.Color.LightGreen, 5, 5);
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

            dxfCache.Clear();

            src = 0;
            foreach (var item in Infos)
            {
                RawDetail[] det = null;
                if (item.Path.ToLower().EndsWith("dxf"))
                {
                    det = DxfParser.LoadDxf(item.Path, item.SplitOnLoad);
                    foreach (var ditem in det)
                    {
                        var t1 = ditem.Outers.Where(z => z.Tag is DxfEntity[]).Select(z => z.Tag as DxfEntity[]).SelectMany(z => z).ToArray();
                        foreach (var outter in ditem.Outers)
                        {
                            t1 = t1.Union(outter.Childrens.Where(z => z.Tag is DxfEntity[]).Select(z => z.Tag as DxfEntity[]).SelectMany(z => z).ToArray()).ToArray();
                        }
                        dxfCache.Add(ditem.Name, t1);
                    }

                }
                else if (item.Path.ToLower().EndsWith("svg"))
                {
                    det = SvgParser.LoadSvg(item.Path);
                }

                foreach (var r in det)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        context.ImportFromRawDetail(r, src);
                    }
                    src++;
                }
            }

            if (sheets.Count == 0 || polygons.Count == 0)
            {
                MessageBox.Show("There are no sheets or parts", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            menu.SetTab(menu.NestTab);
            run();
        }

        private void Menu_TabIndexChanged()
        {
            if (menu.GeneralTab.IsSelected && RenderControl.Visible)
            {
                RenderControl.Visible = false;
                //tableLayoutPanel1.Controls.Remove(RenderControl);
                //tableLayoutPanel1.Controls.Add(panel1, 0, 1);
                tableLayoutPanel2.Visible = true;
                stgControl.Visible = false;
            }
            else
            if (menu.NestTab.IsSelected && !RenderControl.Visible)
            {
                RenderControl.Visible = true;
                //tableLayoutPanel1.Controls.Remove(panel1);
                //tableLayoutPanel1.Controls.Add(RenderControl, 0, 1);
                tableLayoutPanel2.Visible = false;
                stgControl.Visible = false;
            }
        }

        void run()
        {
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
        public int MaxNestSeconds = 10;
        public bool UseNestTimeLimit = false;
        public void RunDeepnest()
        {
            if (th != null) return;

            th = new Thread(() =>
            {
                context.StartNest();
                UpdateNestsList();
                Background.displayProgress = displayProgress;
                Stopwatch sww = Stopwatch.StartNew();
                while (true)
                {
                    Stopwatch sw = Stopwatch.StartNew();
                    sw.Start();
                    displayProgress(0.0f);
                    context.NestIterate();
                    UpdateNestsList();
                    displayProgress(1.0f);
                    sw.Stop();
                    if (UseNestTimeLimit)
                    {
                        toolStripStatusLabel1.Text = $"Total nesting time: {Math.Round(sww.Elapsed.TotalSeconds, 2),6} / {MaxNestSeconds}s   ";
                    }
                    else
                    {
                        toolStripStatusLabel1.Text = $"Total nesting time: {Math.Round(sww.Elapsed.TotalSeconds, 2),6}s   Total nests: {nest.nests.Count} ";
                    }
                    //toolStripStatusLabel1.Text = $"Total nesting time: {Math.Round(sww.Elapsed.TotalSeconds, 2),6} / {MaxNestSeconds}s    Last nesting time: {sw.ElapsedMilliseconds} ms";

                    if (stop)
                        break;

                    if (UseNestTimeLimit && sww.Elapsed.TotalSeconds > MaxNestSeconds)
                        break;
                }
                toolStripStatusLabel1.Text = $"Nesting complete. Total nests: {nest.nests.Count}";

                th = null;
            });
            th.IsBackground = true;
            th.Start();

        }
        public SvgNest nest { get { return context.Nest; } }

        public LoadDetailSplitMode SplitMode { get; set; } = LoadDetailSplitMode.Ask;

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

        RawDetail Preview;
        bool autoFit = true;

        private void objectListView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (objectListView1.SelectedObject == null) return;
            var path = (objectListView1.SelectedObject as DetailLoadInfo).Path.ToLower();
            if (path.EndsWith("dxf"))
            {
                Preview = DxfParser.LoadDxf(path).FirstOrDefault();
            }
            else if (path.EndsWith("svg"))
            {
                Preview = SvgParser.LoadSvg(path).FirstOrDefault();
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
            /*if (Preview is NFP nfp)
            {
                GraphicsPath gp = new GraphicsPath();
                gp.AddPolygon(nfp.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray());
                ctx3.FitToPoints(gp.PathPoints, 5);
            }*/
        }

        internal void StopNesting()
        {
            stop = true;
        }

        public static Dictionary<string, DxfEntity[]> dxfCache = new Dictionary<string, DxfEntity[]>();

        internal void Export()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            //sfd.FilterIndex = lastSaveFilterIndex;


            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            foreach (var item in polygons)
            {
                item.Tag = dxfCache[item.Name];
            }

            // lastSaveFilterIndex = sfd.FilterIndex;
            if (sfd.FilterIndex == 1)
                DxfParser.Export(sfd.FileName, polygons.Where(z => z.sheet == sheets[currentSheet]).ToArray(), new[] { sheets[currentSheet] }.ToArray());

            if (sfd.FilterIndex == 2)
                SvgParser.Export(sfd.FileName, polygons.Where(z => z.sheet == sheets[currentSheet]).ToArray(), new[] { sheets[currentSheet] }.ToArray());
        }

        public DialogResult ShowQuestion(string text)
        {
            return MessageBox.Show(text, Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        void deleteParts()
        {
            if (objectListView1.SelectedObjects.Count == 0)
                return;

            if (ShowQuestion($"Are you to sure to delete {objectListView1.SelectedObjects.Count} items?") == DialogResult.No)
                return;

            foreach (var item in objectListView1.SelectedObjects)
            {
                if (Preview != null && (item as DetailLoadInfo).Path == (Preview as RawDetail).Name) Preview = null;
                var di = item as DetailLoadInfo;
                dxfCache.Remove(di.Name);
                Infos.Remove(di);
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
            if (Infos.Count == 0)
            {
                ShowMessage("There are no parts.", MessageBoxIcon.Warning);
                return;
            }

            if (ShowQuestion("Are you to sure to delete all items?") == DialogResult.No)
                return;

            Infos.Clear();
            objectListView1.SetObjects(Infos);
            dxfCache.Clear();
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
        void deleteSheet()
        {
            if (objectListView2.SelectedObjects.Count == 0)
                return;

            if (ShowQuestion($"Are you to sure to delete {objectListView2.SelectedObjects.Count} sheets?") == DialogResult.No) return;
            foreach (var item in objectListView2.SelectedObjects)
            {
                sheetsInfos.Remove(item as SheetLoadInfo);
            }
            updateSheetInfos();
        }
        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            deleteSheet();
        }

        private void rectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddSheetDialog a = new AddSheetDialog();
            if (a.ShowDialog() != DialogResult.OK) return;
            sheetsInfos.Add(new SheetLoadInfo()
            {
                Height = a.SheetHeight,
                Width = a.SheetWidth,
                Nfp = NewSheet(a.SheetWidth, a.SheetHeight),
                Quantity = 1
            });
            updateSheetInfos();
        }

        private void dxfToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            ofd.FilterIndex = lastOpenFilterIndex;
            ofd.Multiselect = true;
            if (ofd.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                lastOpenFilterIndex = ofd.FilterIndex;
                try
                {
                    RawDetail[] det = null;
                    if (ofd.FileNames[i].ToLower().EndsWith("dxf"))
                        det = DxfParser.LoadDxf(ofd.FileNames[i]);

                    if (ofd.FileNames[i].ToLower().EndsWith("svg"))
                        det = SvgParser.LoadSvg(ofd.FileNames[i]);

                    var fr = sheetsInfos.FirstOrDefault(z => z.Path == ofd.FileNames[i]);
                    if (fr != null)
                        fr.Quantity++;
                    else
                    {
                        foreach (var r in det)
                        {
                            var nfp = r.ToNfp();
                            var bbox = r.BoundingBox();
                            sheetsInfos.Add(new SheetLoadInfo()
                            {
                                Quantity = 1,
                                Nfp = nfp,
                                Width = bbox.Width,
                                Height = bbox.Height,
                                Info = new FileInfo(ofd.FileNames[i]).Name,
                                Path = ofd.FileNames[i]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ofd.FileNames[i]}: {ex.Message}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            updateSheetInfos();
        }

        internal void ZoomOut()
        {
            ctx.ZoomOut();
        }

        internal void ZoomIn()
        {
            ctx.ZoomIn();
        }

        internal void FitAll(NFP[] nfps = null)
        {
            List<PointF> points = new List<PointF>();
            if (nfps == null)
            {
                nfps = sheets.ToArray();
            }
            foreach (var item in nfps)
            {
                points.AddRange(item.Points.Select(z => new PointF((float)(z.x + item.x), (float)(z.y + item.y))));
            }

            if (points.Any())
                ctx.FitToPoints(points.ToArray(), 20);
        }

        int currentSheet = 0;
        internal void FitNextSheet()
        {
            if (sheets.Count == 0)
                return;

            currentSheet++;
            if (currentSheet >= sheets.Count)
            {
                currentSheet = 0;
            }
            FitAll(new[] { sheets[currentSheet] });
        }

        internal void ExportAll()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Dxf files (*.dxf)|*.dxf|Svg files (*.svg)|*.svg";
            //sfd.FilterIndex = lastSaveFilterIndex;
            if (sfd.ShowDialog() != DialogResult.OK)
                return;

            foreach (var item in polygons)
            {
                item.Tag = dxfCache[item.Name];
            }
            // lastSaveFilterIndex = sfd.FilterIndex;
            if (sfd.FilterIndex == 1)
                DxfParser.Export(sfd.FileName, polygons.ToArray(), sheets.ToArray());

            if (sfd.FilterIndex == 2)
                SvgParser.Export(sfd.FileName, polygons.ToArray(), sheets.ToArray());

        }

        internal void SwitchSettingsPanel()
        {
            tableLayoutPanel2.Visible = !tableLayoutPanel2.Visible;
            stgControl.Visible = !stgControl.Visible;
        }

        internal void ColorsView(bool v)
        {
            UseColors = v;
        }
        public bool BorderScrollEnabled = false;
        internal void BorderScroll(bool v)
        {
            BorderScrollEnabled = v;
        }

        internal void ShowNestsList()
        {
            Form f = new Form();
            f.Text = "nest selector";
            f.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ListView l = new ListView();
            l.View = View.Details;
            l.Columns.Add(new ColumnHeader() { Text = "Quality", Width = 150 });
            foreach (var item in nest.nests)
            {
                var lvi = new ListViewItem(new string[] { item.fitness.ToString() }) { Tag = item };
                l.Items.Add(lvi);

            }
            l.GridLines = true;
            l.FullRowSelect = true;
            l.DoubleClick += L_DoubleClick;
            f.Controls.Add(l);
            l.Dock = DockStyle.Fill;
            f.TopMost = true;
            f.Show();
        }

        private void L_DoubleClick(object? sender, EventArgs e)
        {
            var l = sender as ListView;
            if (l.SelectedItems.Count == 0)
                return;

            var sp = (l.SelectedItems[0].Tag as SheetPlacement);
            context.AssignPlacement(sp);
        }
    }
}