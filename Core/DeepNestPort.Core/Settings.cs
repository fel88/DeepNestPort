using DeepNestLib;

namespace DeepNestPort.Core
{
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
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
    }
}
