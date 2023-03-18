using DeepNestLib;

namespace DeepNestPort.Core
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        public void InitValues()
        {
            numericUpDown1.Value = (decimal)SvgNest.Config.spacing;
            numericUpDown2.Value = (decimal)SvgNest.Config.sheetSpacing;
            numericUpDown3.Value = Form1.Form.MaxNestSeconds;
            numericUpDown4.Value = SvgNest.Config.rotations;
            checkBox1.Checked = Form1.Form.UseNestTimeLimit;
            comboBox1.SelectedIndex = 0;
            numericUpDown3.Enabled = Form1.Form.UseNestTimeLimit;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.spacing = (float)numericUpDown1.Value;
        }

        public event Action Close;
        private void button1_Click(object sender, EventArgs e)
        {
            Close?.Invoke();
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.sheetSpacing = (double)numericUpDown2.Value;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
                Form1.Form.SplitMode = LoadDetailSplitMode.Ask;
            if (comboBox1.SelectedIndex == 1)
                Form1.Form.SplitMode = LoadDetailSplitMode.Always;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            Form1.Form.MaxNestSeconds = (int)numericUpDown3.Value;
        }

        private void Settings_Load(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown3.Enabled = checkBox1.Checked;
            Form1.Form.UseNestTimeLimit = checkBox1.Checked;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            SvgNest.Config.rotations = (int)numericUpDown4.Value;
        }
    }
}
