using DeepNestLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DeepNestPort
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ctx = new DrawingContext(pictureBox1);
            ctx2 = new DrawingContext(pictureBox2);

            listView1.DoubleBuffered(true);
            listView2.DoubleBuffered(true);
            listView3.DoubleBuffered(true);
            listView4.DoubleBuffered(true);
            progressBar1 = new PictureBoxProgressBar();
            progressBar1.Dock = DockStyle.Fill;
            panel1.Controls.Add(progressBar1);

            checkBox2.Checked = SvgNest.Config.simplify;
            checkBox4.Checked = Background.UseParallel;

            UpdateFilesList(@"dxfs");


        }
        PictureBoxProgressBar progressBar1;


        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in polygons)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.id.ToString(), item.source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
            }
            listView2.Items.Clear();
            foreach (var item in sheets)
            {
                listView2.Items.Add(new ListViewItem(new string[] { item.id.ToString(), item.source.ToString(), item.Name, item.Points.Count().ToString() }) { Tag = item });
            }

            groupBox5.Text = "Parts: " + polygons.Count();
            groupBox6.Text = "Sheets: " + sheets.Count;
        }


        public NestingContext Context = new NestingContext();


        public SvgNest nest { get { return context.Nest; } }

        public object selected = null;

        Thread dth;

        object Preview;
        public void Redraw()
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var pos1 = ctx.GetPos();
            var posx = pos1.X;
            var posy = pos1.Y;
            ctx.Update();
            ctx2.Update();


            #region preview draw
            ctx2.gr.Clear(Color.White);

            if (Preview != null)
            {
                ctx2.gr.ResetTransform();
                GraphicsPath gp = new GraphicsPath();
                if (Preview is RawDetail)
                {
                    foreach (var item in (Preview as RawDetail).Outers)
                    {
                        gp.AddPolygon(item.Points.Select(z => ctx2.Transform(z)).ToArray());
                    }
                }
                if (Preview is NFP)
                {
                    var nfp = (Preview as NFP);

                    gp.AddPolygon(nfp.Points.Select(z => ctx2.Transform(z.x, z.y)).ToArray());
                    if (nfp.children != null)
                    {
                        foreach (var item in nfp.children)
                        {
                            gp.AddPolygon(item.Points.Select(z => ctx2.Transform(z.x, z.y)).ToArray());
                        }
                    }

                }

                ctx2.gr.FillPath(Brushes.LightBlue, gp);
                ctx2.gr.DrawPath(Pens.Black, gp);
            }
            ctx2.Setup();

            #endregion

            ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
            ctx.gr.Clear(Color.White);

            ctx.gr.ResetTransform();

            ctx.gr.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
            ctx.gr.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));
            int yy = 0;
            int gap = (int)Font.Size;
            if (isInfoShow)
            {
                ctx.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), Font, Brushes.Blue, 0, yy);
                yy += (int)Font.Size + gap;
                ctx.gr.DrawString($"Material Utilization: {Math.Round(context.MaterialUtilization * 100.0f, 2)}%   Iterations: {context.Iterations}    Parts placed: {context.PlacedPartsCount}/{polygons.Count}", Font, Brushes.DarkBlue, 0, yy);
                yy += (int)Font.Size + gap;
                ctx.gr.DrawString($"Sheets: {sheets.Count}   Parts:{polygons.Count}    parts types: {polygons.GroupBy(z => z.source).Count()}", Font, Brushes.DarkBlue, 0, yy);
                yy += (int)Font.Size + gap;

                if (nest != null && nest.nests.Any())
                {
                    ctx.gr.DrawString($"Nests: {nest.nests.Count} Fitness: {nest.nests.First().fitness}   Area:{nest.nests.First().area}  ", Font, Brushes.DarkBlue, 0, yy);
                    yy += (int)Font.Size + gap;
                }


                ctx.gr.DrawString($"Call counter: {Background.callCounter};  Last placing time: {Background.LastPlacePartTime}ms", Font, Brushes.DarkBlue, 0, yy);
                yy += (int)Font.Size + gap;
            }
            else
            {
                ctx.gr.DrawString($"Iterations: {context.Iterations}    Parts placed: {context.PlacedPartsCount}/{polygons.Count}", Font, Brushes.DarkBlue, 0, yy);
                yy += (int)Font.Size + gap;
                ctx.gr.DrawString($"Sheets: {sheets.Count}   Parts:{polygons.Count}    Parts types: {polygons.GroupBy(z => z.source).Count()}", Font, Brushes.DarkBlue, 0, yy);
                yy += (int)Font.Size + gap;
            }

            if (!checkBox1.Checked)
            {
                if (bb != null)
                {
                    //ctx.gr.TranslateTransform((float)sheets[0].x, (float)sheets[0].y);
                    var pp = ctx.Transform((float)sheets[0].x, (float)sheets[0].y);
                    ctx.gr.DrawImage(bb, new RectangleF(pp.X, pp.Y, bb.Width * ctx.zoom, bb.Height * ctx.zoom), new Rectangle(0, 0, bb.Width, bb.Height), GraphicsUnit.Pixel);
                }
            }
            foreach (var item in polygons.Union(sheets))
            {
                if (!checkBox1.Checked)
                {
                    continue;
                }
                if (!(item is Sheet))
                {
                    if (!item.fitted) continue;
                }

                GraphicsPath path = new GraphicsPath();
                if (item.Points != null && item.Points.Any())
                {
                    //rotate first;
                    var m = new Matrix();
                    m.Translate((float)item.x, (float)item.y);
                    m.Rotate(item.rotation);



                    var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    m.TransformPoints(pnts);

                    path.AddPolygon(pnts.Select(z => ctx.Transform(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            path.AddPolygon(pnts2.Select(z => ctx.Transform(z)).ToArray());

                        }
                    }
                    ctx.gr.ResetTransform();

                    /*if (selected == item)
                    {
                        ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.Orange)), path);
                        ctx.gr.DrawPath(Pens.DarkBlue, path);

                    }
                    else*/
                    {
                        if (!sheets.Contains(item))
                        {
                            ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                        }
                        ctx.gr.DrawPath(Pens.Black, path);
                    }

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
                            if (was && isInfoShow)
                            {
                                ctx.gr.DrawString("util: " + res + "%", Font, Brushes.Black, trans1);
                            }
                        }
                    }
                }
            }
            ctx.Setup();
        }


        public void RenderSheet()
        {
            ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
            ctx.gr.Clear(Color.White);

            ctx.gr.ResetTransform();


            foreach (var item in polygons.Union(sheets))
            {

                if (!(item is Sheet))
                {
                    if (!item.fitted) continue;
                }

                GraphicsPath path = new GraphicsPath();
                if (item.Points != null && item.Points.Any())
                {
                    //rotate first;
                    var m = new Matrix();
                    m.Translate((float)item.x, (float)item.y);
                    m.Rotate(item.rotation);



                    var pnts = item.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                    m.TransformPoints(pnts);

                    path.AddPolygon(pnts.Select(z => ctx.Transform(z)).ToArray());
                    if (item.children != null)
                    {
                        foreach (var citem in item.children)
                        {
                            var pnts2 = citem.Points.Select(z => new PointF((float)z.x, (float)z.y)).ToArray();
                            m.TransformPoints(pnts2);
                            path.AddPolygon(pnts2.Select(z => ctx.Transform(z)).ToArray());

                        }
                    }
                    ctx.gr.ResetTransform();


                    if (!sheets.Contains(item))
                    {
                        ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                    }
                    ctx.gr.DrawPath(Pens.Black, path);


                }
            }
        }
        public void RedrawAsync()
        {
            if (dth != null) return;
            dth = new Thread(() =>
             {
                 Redraw();
                 dth = null;
             });
            dth.IsBackground = true;
            dth.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            progressBar1.UpdateImg();
            progressBar1.Value = (int)Math.Round(progressVal * 100f);
            Redraw();
        }

        public DrawingContext ctx;
        public DrawingContext ctx2;

        public void UpdateNestsList()
        {
            if (nest != null)
            {
                listView4.Invoke((Action)(() =>
                {
                    listView4.BeginUpdate();
                    listView4.Items.Clear();
                    foreach (var item in nest.nests)
                    {
                        listView4.Items.Add(new ListViewItem(new string[] { item.fitness + "" }) { Tag = item });
                    }
                    listView4.EndUpdate();
                }));
            }
        }


        Thread th;

        internal void displayProgress(float progress)
        {
            progressVal = progress;

        }
        public float progressVal = 0;




        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                selected = listView1.SelectedItems[0].Tag;
                Preview = selected;
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }


        public void UpdateFilesList(string path)
        {
            var di = new DirectoryInfo(path);
            groupBox3.Text = "Files: " + di.FullName;
            listView3.Items.Clear();
            listView3.Items.Add(new ListViewItem(new string[] { ".." }) { Tag = di.Parent, BackColor = Color.LightBlue });
            foreach (var item in di.GetDirectories())
            {
                listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item, BackColor = Color.LightBlue });
            }
            foreach (var item in di.GetFiles())
            {
                if (!(item.Extension.Contains("svg") || item.Extension.Contains("dxf"))) continue;
                listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
            }

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
        public Sheet NewRhombusSheet(int w = 3000, int h = 1500)
        {
            var tt = new Sheet();
            tt.Name = "rhombSheet" + (sheets.Count + 1);        
            
            tt.Height = h;
            tt.Width = w;
            tt.Points = new SvgPoint[] { };
            int x = 0;
            int y = 0;
            int _width = w;
            int _height = h;

            tt.AddPoint(new SvgPoint(x + _width / 2, y));
            tt.AddPoint(new SvgPoint(x, y + _height / 2));
            tt.AddPoint(new SvgPoint(x + _width / 2, y + _height));
            tt.AddPoint(new SvgPoint(x + _width, y + _height / 2));

            return tt;
        }

        public Sheet NewCircleSheet(int w = 3000)
        {
            var tt = new Sheet();
            tt.Name = "circleSheet" + (sheets.Count + 1);            
            
            tt.Height = w;
            tt.Width = w;
            tt.Points = new SvgPoint[] { };
            int x = 0;
            int y = 0;



            for (int i = 0; i < 360; i += 5)
            {
                var xx = w / 2 * Math.Cos(i * Math.PI / 180.0f);
                var yy = w / 2 * Math.Sin(i * Math.PI / 180.0f);
                tt.AddPoint(new SvgPoint(xx + w / 2, yy + w / 2));
            }



            return tt;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polygons.Clear();
            UpdateList();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                for (int i = 0; i < listView1.SelectedItems.Count; i++)
                {
                    polygons.Remove(listView1.SelectedItems[i].Tag as NFP);
                }
                UpdateList();
            }
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.spacing = float.Parse(textBox1.Text, CultureInfo.InvariantCulture);
                textBox1.BackColor = Color.White;
                textBox1.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox1.BackColor = Color.Red;
                textBox1.ForeColor = Color.White;
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.sheetSpacing = float.Parse(textBox2.Text, CultureInfo.InvariantCulture);
                textBox2.BackColor = Color.White;
                textBox2.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox2.BackColor = Color.Red;
                textBox2.ForeColor = Color.White;
            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                SvgNest.Config.rotations = int.Parse(textBox3.Text, CultureInfo.InvariantCulture);
                textBox3.BackColor = Color.White;
                textBox3.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox3.BackColor = Color.Red;
                textBox3.ForeColor = Color.White;
            }
        }

        private void moveToSheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (listView1.SelectedItems.Count > 0)
            {
                var pol = listView1.SelectedItems[0].Tag as NFP;
                polygons.Remove(pol);
                var b = GeometryUtil.getPolygonBounds(pol);
                Sheet sheet = new Sheet();
                sheet.Points = new SvgPoint[] { };
                foreach (var item in pol.Points)
                {
                    sheet.AddPoint(new SvgPoint(item.x - b.x, item.y - b.y));
                }
                /*if (pol.children != null)
                {
                    sheet.children = new List<NFP>();
                    for (int i = 0; i < pol.children.Count; i++)
                    {
                        var child = pol.children[i];
                        NFP newchild = new NFP();
                        for (var j = 0; j < child.length; j++)
                        {
                            newchild.AddPoint(new SvgPoint(child[j].x - b.x, child[j].y - b.y));
                        }
                        sheet.children.Add(newchild);
                    }
                }*/

                sheet.Width = (float)b.width;
                sheet.Height = (float)b.height;

                sheet.source = context.GetNextSheetSource();
                sheets.Add(sheet);
                context.ReorderSheets();
                UpdateList();
            }
        }


        private void clearAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            sheets.Clear();
            UpdateList();
        }


        private void moveToPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var pol = listView2.SelectedItems[0].Tag as NFP;
                sheets.Remove(pol);
                polygons.Add(pol);
                UpdateList();
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                selected = listView2.SelectedItems[0].Tag;
                Preview = selected;
            }
        }

        private void listView3_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                var si = listView3.SelectedItems[0].Tag;
                if (si is DirectoryInfo)
                {
                    UpdateFilesList((si as DirectoryInfo).FullName);

                }
                if (si is FileInfo)
                {
                    var f = (si as FileInfo);
                    QntDialog q = new QntDialog();
                    if (q.ShowDialog() == DialogResult.OK)
                    {
                        RawDetail det = null;
                        if (f.Extension == ".svg")
                        {
                            det = SvgParser.LoadSvg(f.FullName);
                        }
                        if (f.Extension == ".dxf")
                        {
                            det = DxfParser.loadDxf(f.FullName);
                        }

                        int src = 0;
                        if (polygons.Any())
                        {
                            src = polygons.Max(z => z.source.Value) + 1;
                        }
                        for (int i = 0; i < q.Qnt; i++)
                        {
                            context.ImportFromRawDetail(det, src);
                        }

                        UpdateList();

                    }
                }
            }
        }


        private void importSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0)
            {
                QntDialog q = new QntDialog();
                if (q.ShowDialog() == DialogResult.OK)
                {
                    foreach (var item in listView3.SelectedItems)
                    {
                        var t = (item as ListViewItem).Tag as FileInfo;
                        RawDetail det = null;
                        if (t.Extension == ".svg")
                        {
                            det = SvgParser.LoadSvg(t.FullName);
                        }
                        if (t.Extension == ".dxf")
                        {
                            det = DxfParser.loadDxf(t.FullName);
                        }
                        int src = 0;
                        if (polygons.Any())
                        {
                            src = polygons.Max(z => z.source.Value) + 1;
                        }
                        for (int i = 0; i < q.Qnt; i++)
                        {
                            context.ImportFromRawDetail(det, src);
                        }
                    }
                    UpdateList();
                }

            }
        }


        bool stop = false;
        public void RunDeepnest()
        {

            if (th == null)
            {
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
        }


        Random r = new Random();
        private void cloneQntToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                QntDialog qd = new QntDialog();
                if (qd.ShowDialog() == DialogResult.OK)
                {
                    var nfp = (listView1.SelectedItems[0].Tag as NFP);
                    for (int i = 0; i < qd.Qnt; i++)
                    {
                        var r = Background.clone(nfp);
                        polygons.Add(r);
                    }
                    UpdateList();
                }
            }
        }


        private void button10_Click(object sender, EventArgs e)
        {
            if (sheets.Count == 0 || polygons.Count == 0)
            {
                MessageBox.Show("There are no sheets or parts", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            stop = false;
            progressBar1.Value = 0;
            tabControl1.SelectedTab = tabPage4;
            context.ReorderSheets();
            RunDeepnest();
        }


        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            SvgNest.Config.simplify = checkBox2.Checked;
        }


        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Background.UseParallel = checkBox4.Checked;
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var t = comboBox1.SelectedItem as string;
            if (t.ToLower().Contains("gravi"))
            {
                SvgNest.Config.placementType = PlacementTypeEnum.gravity;
            }
            if (t.ToLower().Contains("box"))
            {
                SvgNest.Config.placementType = PlacementTypeEnum.box;
            }
            if (t.ToLower().Contains("squ"))
            {
                SvgNest.Config.placementType = PlacementTypeEnum.squeeze;
            }

        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.populationSize = (int)numericUpDown1.Value;
        }

        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView4.SelectedItems.Count > 0)
            {
                var shp = listView4.SelectedItems[0].Tag as SheetPlacement;
                context.AssignPlacement(shp);
            }
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.mutationRate = (int)numericUpDown2.Value;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            var cnt = (int)numericUpDown3.Value;
            int? ww = null;
            int? hh = null;

            try
            {
                ww = int.Parse(textBox4.Text);
                textBox4.BackColor = Color.White;
                textBox4.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox4.BackColor = Color.Red;
                textBox4.ForeColor = Color.White;
            }

            try
            {
                hh = int.Parse(textBox5.Text);
                textBox5.BackColor = Color.White;
                textBox5.ForeColor = Color.Black;
            }
            catch (Exception ex)
            {
                textBox5.BackColor = Color.Red;
                textBox5.ForeColor = Color.White;
            }

            if (ww == null || hh == null)
            {
                MessageBox.Show("Wrong sizes", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (comboBox2.SelectedItem == null)
            {
                label11.BackColor = Color.Red;
                label11.ForeColor = Color.White;
                return;
            }
            label11.BackColor = label11.Parent.BackColor;
            label11.ForeColor = label11.Parent.ForeColor;
            List<Sheet> sh = new List<Sheet>();
            var src = context.GetNextSheetSource();
            for (int i = 0; i < cnt; i++)
            {
                switch (comboBox2.SelectedItem.ToString())
                {
                    case "Rectangle":
                        sh.Add(NewSheet(ww.Value, hh.Value));
                        break;
                    case "Rhombus":
                        sh.Add(NewRhombusSheet(ww.Value, hh.Value));
                        break;
                    case "Circle":
                        sh.Add(NewCircleSheet(ww.Value));
                        break;
                }
            }
            foreach (var item in sh)
            {
                item.source = src;
                context.Sheets.Add(item);
            }
            UpdateList();
            context.ReorderSheets();
        }

        public int GetCountFromDialog()
        {
            QntDialog q = new DeepNestPort.QntDialog();
            if (q.ShowDialog() == DialogResult.OK)
            {
                return q.Qnt;
            }
            return 0;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var cnt = GetCountFromDialog();
            Random r = new Random();
            for (int i = 0; i < cnt; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(60) + 10;
                var hh = r.Next(60) + 5;
                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                polygons.Add(pl);
                pl.source = src;
                pl.x = xx;
                pl.y = yy;
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(0, 0));
                pl.AddPoint(new SvgPoint(ww, 0));
                pl.AddPoint(new SvgPoint(ww, hh));
                pl.AddPoint(new SvgPoint(0, hh));
            }
            UpdateList();
        }

        private void button14_Click(object sender, EventArgs e)
        {

            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var rad = r.Next(60) + 10;

                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                pl.source = src;
                polygons.Add(pl);
                pl.x = xx;
                pl.y = yy;
                pl.Points = new SvgPoint[] { };
                for (int ang = 0; ang < 360; ang += 15)
                {
                    var xx1 = (float)(rad * Math.Cos(ang * Math.PI / 180.0f));
                    var yy1 = (float)(rad * Math.Sin(ang * Math.PI / 180.0f));
                    pl.AddPoint(new SvgPoint(xx1, yy1));
                }


            }
            UpdateList();
        }

        private void button15_Click(object sender, EventArgs e)
        {

            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(60) + 10;
                var hh = r.Next(60) + 5;
                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                pl.source = src;
                polygons.Add(pl);
                pl.Points = new SvgPoint[] { };
                pl.x = xx;
                pl.y = yy;
                pl.AddPoint(new SvgPoint(-ww, 0));
                pl.AddPoint(new SvgPoint(+ww, 0));
                pl.AddPoint(new SvgPoint(0, +hh));

            }
            UpdateList();
        }

        private void button16_Click(object sender, EventArgs e)
        {

            Random r = new Random();
            for (int i = 0; i < 10; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(400) + 10;
                var hh = r.Next(400) + 5;
                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                pl.source = src;
                polygons.Add(pl);
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(xx, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
                pl.AddPoint(new SvgPoint(xx, yy + hh));
            }
            UpdateList();
        }

        private void button17_Click(object sender, EventArgs e)
        {
            var ww = r.Next(400) + 10;
            var hh = r.Next(400) + 5;
            QntDialog q = new QntDialog();
            int src = 0;
            if (polygons.Any())
            {
                src = polygons.Max(z => z.source.Value) + 1;
            }
            if (q.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < q.Qnt; i++)
                {
                    var xx = r.Next(2000) + 100;
                    var yy = r.Next(2000);

                    NFP pl = new NFP();

                    pl.source = src;
                    polygons.Add(pl);
                    pl.Points = new SvgPoint[] { };
                    pl.x = xx;
                    pl.y = yy;
                    pl.AddPoint(new SvgPoint(0, 0));
                    pl.AddPoint(new SvgPoint(0 + ww, 0));
                    pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
                    pl.AddPoint(new SvgPoint(0, 0 + hh));
                }

                UpdateList();
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            stop = true;
        }

        NestingContext context = new NestingContext();

        List<NFP> polygons { get { return context.Polygons; } }

        private void button6_Click(object sender, EventArgs e)
        {
            var cnt = GetCountFromDialog();
            Random r = new Random();
            for (int i = 0; i < cnt; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var ww = r.Next(250) + 150;
                var hh = r.Next(250) + 120;
                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                polygons.Add(pl);
                pl.source = src;
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(0, 0));
                pl.AddPoint(new SvgPoint(0 + ww, 0));
                pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
                pl.AddPoint(new SvgPoint(0, 0 + hh));
                pl.x = xx;
                pl.y = yy;
                var hole = new NFP();

                pl.children = new List<NFP>();
                pl.children.Add(hole);
                hole.Points = new SvgPoint[] { };
                int gap = 10;
                hole.AddPoint(new SvgPoint(0 + gap, 0 + gap));
                hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + gap));
                hole.AddPoint(new SvgPoint(0 + ww - gap, 0 + hh - gap));
                hole.AddPoint(new SvgPoint(0 + gap, 0 + hh - gap));
                hole.x = xx;
                hole.y = yy;

            }
            UpdateList();
        }


        private void button5_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                var xx = r.Next(2000) + 100;
                var yy = r.Next(2000);
                var rad = r.Next(60) + 10;
                int rad2 = rad - 8;

                NFP pl = new NFP();
                int src = 0;
                if (polygons.Any())
                {
                    src = polygons.Max(z => z.source.Value) + 1;
                }
                pl.source = src;
                polygons.Add(pl);
                pl.Points = new SvgPoint[] { };

                NFP hole = new NFP();
                for (int ang = 0; ang < 360; ang += 15)
                {
                    var xx1 = (float)(rad * Math.Cos(ang * Math.PI / 180.0f));
                    var yy1 = (float)(rad * Math.Sin(ang * Math.PI / 180.0f));
                    pl.AddPoint(new SvgPoint(xx1, yy1));
                    var xx2 = (float)(rad2 * Math.Cos(ang * Math.PI / 180.0f));
                    var yy2 = (float)(rad2 * Math.Sin(ang * Math.PI / 180.0f));
                    hole.AddPoint(new SvgPoint(xx2, yy2));
                }
                pl.children = new List<NFP>();
                pl.children.Add(hole);
                pl.x = xx;
                pl.y = yy;


            }
            UpdateList();
        }

        private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (listView2.SelectedItems.Count > 0)
            {
                var f = listView2.SelectedItems[0].Tag as NFP;
                sheets.Remove(f);
                UpdateList();
                context.ReorderSheets();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Random r = new Random();


            var xx = r.Next(2000) + 100;
            var yy = r.Next(2000);
            var ww = 20;
            var hh = 20;
            NFP pl = new NFP();
            int src = 0;
            if (polygons.Any())
            {
                src = polygons.Max(z => z.source.Value) + 1;
            }
            polygons.Add(pl);
            pl.source = src;
            pl.x = xx;
            pl.y = yy;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(0, 0));
            pl.AddPoint(new SvgPoint(ww, 0));
            pl.AddPoint(new SvgPoint(ww, hh));
            pl.AddPoint(new SvgPoint(0, hh));

            UpdateList();
        }
        bool isInfoShow = false;
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            isInfoShow = !isInfoShow;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Font = new Font(Font.FontFamily.Name, Font.Size + 1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Font = new Font(Font.FontFamily.Name, Font.Size - 1);
        }

        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedItems.Count > 0 && listView3.SelectedItems[0].Tag is FileInfo)
            {
                try
                {
                    var path = (FileInfo)listView3.SelectedItems[0].Tag;
                    RawDetail det = null;
                    if (path.Extension == ".svg")
                    {
                        det = SvgParser.LoadSvg(path.FullName);
                    }
                    if (path.Extension == ".dxf")
                    {
                        det = DxfParser.loadDxf(path.FullName);
                    }

                    Preview = det;
                }
                catch (Exception ex)
                {
                    Preview = null;
                }

            }
        }

        List<NFP> sheets { get { return context.Sheets; } }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Svgs files (*.svg)|*.svg";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                SvgParser.Export(sfd.FileName, polygons.ToArray(), sheets.ToArray());
            }
        }

        private void listView3_MouseMove(object sender, MouseEventArgs e)
        {
            var ch = listView3.GetChildAtPoint(listView3.PointToClient(Cursor.Position));

            if (ch != null)
            {

            }


        }

        Bitmap bb;
        private void button7_Click(object sender, EventArgs e)
        {
            var sh = sheets[0] as Sheet;
            ctx.sx = (float)sh.x;
            ctx.sy = (float)sh.y;
            ctx.InvertY = false;
            bb = new Bitmap(1 + (int)sh.Width, 1 + (int)sh.Height);
            var gr = Graphics.FromImage(bb);
            var tempgr = ctx.gr;
            var tmpbmp = ctx.bmp;
            ctx.gr = gr;
            ctx.bmp = bb;

            RenderSheet();
            ctx.gr = gr;
            ctx.bmp = tmpbmp;
            ctx.InvertY = true;
            //bb = ctx.bmp.Clone(new Rectangle(0, 0, ctx.bmp.Width, ctx.bmp.Height), ctx.bmp.PixelFormat);
            Clipboard.SetImage(bb);

        }

        private void button8_Click(object sender, EventArgs e)
        {

            var xx = r.Next(2000) + 100;
            var yy = r.Next(2000);
            var ww = r.Next(250) + 150;
            var hh = r.Next(250) + 120;
            NFP pl = new NFP();
            int src = 0;
            if (polygons.Any())
            {
                src = polygons.Max(z => z.source.Value) + 1;
            }
            polygons.Add(pl);
            pl.source = src;
            pl.Points = new SvgPoint[] { };
            pl.AddPoint(new SvgPoint(0, 0));
            pl.AddPoint(new SvgPoint(0 + ww, 0));
            pl.AddPoint(new SvgPoint(0 + ww, 0 + hh));
            pl.AddPoint(new SvgPoint(0, 0 + hh));
            pl.x = xx;
            pl.y = yy;
            pl.children = new List<NFP>();
            int gap = 10;
            int szx = ww / 4;
            int szy = hh / 3;
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 2; j++)
                {

                    var hole = new NFP();


                    pl.children.Add(hole);
                    hole.Points = new SvgPoint[] { };


                    int hx = (i * ww / 4) + gap * (i + 1);
                    int hy = (j * hh / 3) + gap * (j + 1);

                    hole.AddPoint(new SvgPoint(hx + szx, hy + szy));
                    hole.AddPoint(new SvgPoint(hx, hy + szy));
                    hole.AddPoint(new SvgPoint(hx, hy));
                    hole.AddPoint(new SvgPoint(hx + szx, hy));
                    hole.x = xx;
                    hole.y = yy;
                }
            }



            UpdateList();
        }
    }
}
