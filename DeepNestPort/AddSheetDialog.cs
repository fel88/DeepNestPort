using System;
using System.Windows.Forms;

namespace DeepNestPort
{
    public partial class AddSheetDialog : Form
    {
        public AddSheetDialog()
        {
            InitializeComponent();
        }

        public int SheetWidth { get; set; }
        public int SheetHeight { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            SheetWidth = (int)numericUpDown1.Value;
            SheetHeight = (int)numericUpDown2.Value;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
