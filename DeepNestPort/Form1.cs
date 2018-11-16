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
            listView4.DoubleBuffered(true);
            progressBar1 = new PictureBoxProgressBar();
            progressBar1.Dock = DockStyle.Fill;
            panel1.Controls.Add(progressBar1);
            sx = pictureBox1.Width / 2;
            sy = -pictureBox1.Height / 2;
            checkBox2.Checked = SvgNest.Config.simplify;
            checkBox4.Checked = Background.UseParallel;

            UpdateFilesList(@"svgs");
            ctx.Create(pictureBox1);

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox1.MouseUp += PictureBox1_MouseUp;
            MouseWheel += Form1_MouseWheel;
        }
        PictureBoxProgressBar progressBar1;
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            float zold = zoom;
            if (e.Delta > 0) { zoom *= 1.5f; ; }
            else { zoom *= 0.5f; }
            if (zoom < 0.08) { zoom = 0.08f; }
            if (zoom > 1000) { zoom = 1000f; }

            var pos = pictureBox1.PointToClient(Cursor.Position);

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;

            var p = pictureBox1.PointToClient(Cursor.Position);
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var posx = (pos.X / zoom - sx);
            var posy = (-pos.Y / zoom - sy);
        }

        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var p = ctx.Transform(pos);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;
                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }

        public void UpdateList()
        {
            listView1.Items.Clear();
            foreach (var item in polygons)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.id+"",item.Name,
                    item.Points.Count() + "" })
                { Tag = item });
            }
            listView2.Items.Clear();
            foreach (var item in sheets)
            {
                listView2.Items.Add(new ListViewItem(new string[] { ""+item.id,item.Name,
                    item.Points.Count() + "" })
                { Tag = item });
            }

            label1.Text = "parts: " + polygons.Count() + "; sheets: " + sheets.Count;
        }


        public NestingContext Context = new NestingContext();


        public SvgNest nest { get { return context.Nest; } }

        public object selected = null;

        Thread dth;
        public void Redraw()
        {
            var pos = pictureBox1.PointToClient(Cursor.Position);
            var posx = (pos.X / zoom - sx);
            var posy = (-pos.Y / zoom - sy);
            if (isDrag)
            {
                var p = pictureBox1.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
            ctx.gr.SmoothingMode = SmoothingMode.AntiAlias;
            ctx.gr.Clear(Color.White);

            ctx.gr.ResetTransform();
            ctx.gr.DrawLine(Pens.Red, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(1000, 0)));
            ctx.gr.DrawLine(Pens.Blue, ctx.Transform(new PointF(0, 0)), ctx.Transform(new PointF(0, 1000)));
            ctx.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), new Font("Arial", 12), Brushes.Blue, 0, 0);

            ctx.gr.DrawString($"Material Utilization: {Math.Round(context.MaterialUtilization * 100.0f, 2)}%   Iterations: {context.Iterations}    parts placed: {context.PlacedPartsCount}/{polygons.Count}",
                new Font("Arial", 20), Brushes.DarkBlue, 0, 20);

            ctx.gr.DrawString($"Sheets: {sheets.Count}   Parts:{polygons.Count}    parts types: {polygons.GroupBy(z => z.source).Count()}",
                new Font("Arial", 15), Brushes.DarkBlue, 0, 50);

            if (nest != null && nest.nests.Any())
            {
                ctx.gr.DrawString($"Nests: {nest.nests.Count} Fitness: {nest.nests.First().fitness}   Area:{nest.nests.First().area}  ",
                    new Font("Arial", 15), Brushes.DarkBlue, 0, 80);
            }


            ctx.gr.DrawString($"Call counter: {Background.callCounter};  last placeParts time: {Background.LastPlacePartTime}ms",
                    new Font("Arial", 15), Brushes.DarkBlue, 0, 110);


            foreach (var item in polygons.Union(sheets))
            {

                if (!(item is RectanglePolygonSheet))
                {
                    //if (!item.fitted) continue;                    
                }

                GraphicsPath path = new GraphicsPath();
                if (item.Points != null && item.Points.Any())
                {
                    //rotate first;
                    var m = new Matrix();
                    m.Translate((float)item.x, (float)item.y);
                    m.Rotate(item.rotation);

                    List<SvgPoint> points = new List<SvgPoint>();
                    foreach (var pitem in item.Points)
                    {
                        PointF[] pp = new[] { new PointF((float)pitem.x, (float)pitem.y) };
                        m.TransformPoints(pp);
                        points.Add(new SvgPoint(pp[0].X, pp[0].Y));
                    }

                    path.AddPolygon(points.Select(z => new PointF((float)z.x, (float)z.y)).Select(z => ctx.Transform(z)).ToArray());                    

                    ctx.gr.ResetTransform();                    

                    if (selected == item)
                    {
                        ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.Orange)), path);
                        ctx.gr.DrawPath(Pens.DarkBlue, path);

                    }
                    else
                    {
                        if (!sheets.Contains(item))
                        {
                            ctx.gr.FillPath(new SolidBrush(Color.FromArgb(128, Color.LightBlue)), path);
                        }
                        ctx.gr.DrawPath(Pens.Black, path);
                    }

                    if (item is RectanglePolygonSheet)
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
                            var trans1 = ctx.Transform(new PointF((float)points[0].x, (float)points[0].y - 30));
                            if (was)
                            {
                                ctx.gr.DrawString("util: " + res + "%", new Font("Arial", 18), Brushes.Black, trans1);
                            }
                        }
                    }
                }
            }
            pictureBox1.Image = ctx.bmp;
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

        public static DrawingContext ctx = new DrawingContext();
        float sx { get { return ctx.sx; } set { ctx.sx = value; } }
        float sy { get { return ctx.sy; } set { ctx.sy = value; } }
        float zoom { get { return ctx.zoom; } set { ctx.zoom = value; } }

        float startx, starty;
        float origsx, origsy;
        bool isDrag = false;



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
            }
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

     
        public void UpdateFilesList(string path)
        {
            var di = new DirectoryInfo(path);
            listView3.Items.Clear();
            listView3.Items.Add(new ListViewItem(new string[] { ".." }) { Tag = di.Parent, BackColor = Color.LightBlue });
            foreach (var item in di.GetDirectories())
            {
                listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item, BackColor = Color.LightBlue });
            }
            foreach (var item in di.GetFiles())
            {
                if (!item.Extension.Contains("svg")) continue;
                listView3.Items.Add(new ListViewItem(new string[] { item.Name }) { Tag = item });
            }

        }

        public void AddSheet(int w = 3000, int h = 1500)
        {
            var tt = new RectanglePolygonSheet();
            tt.Name = "sheet" + (sheets.Count + 1);
            sheets.Add(tt);
            var p = sheets.Last();
            tt.Height = h;
            tt.Width = w;
            tt.Rebuild();
            UpdateList();
            context.ReorderSheets();
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
                sheets.Add(pol);
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
                        for (int i = 0; i < q.Qnt; i++)
                        {

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

                        var svg = SvgParser.LoadSvg(t.FullName);
                        int src = 0;
                        if (polygons.Any())
                        {
                            src = polygons.Max(z => z.source.Value) + 1;
                        }
                        for (int i = 0; i < q.Qnt; i++)
                        {
                            context.ImportFromRawDetail(svg, src);
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
                        toolStripStatusLabel1.Text = "Nesting complete within: " + sw.ElapsedMilliseconds + "ms";
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

            for (int i = 0; i < cnt; i++)
            {
                AddSheet(ww.Value, hh.Value);
            }
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
                pl.Points = new SvgPoint[] { };
                pl.AddPoint(new SvgPoint(xx, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy + hh));
                pl.AddPoint(new SvgPoint(xx, yy + hh));
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
                pl.Points = new SvgPoint[] { };
                for (int ang = 0; ang < 360; ang += 15)
                {
                    var xx1 = (float)(xx + rad * Math.Cos(ang * Math.PI / 180.0f));
                    var yy1 = (float)(yy + rad * Math.Sin(ang * Math.PI / 180.0f));
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
                pl.AddPoint(new SvgPoint(xx - ww, yy));
                pl.AddPoint(new SvgPoint(xx + ww, yy));
                pl.AddPoint(new SvgPoint(xx, yy + hh));

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
    }
}
