using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Ribbon;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace DeepNestPort.Core
{
    public partial class RibbonMenu : UserControl
    {
        //public RibbonTab DraftTab => Ribbon.draftTab;
        public RibbonTab ProjectTab => Ribbon.projectTab;
        public RibbonMenu()
        {
            InitializeComponent();
            ElementHost elementHost1 = new ElementHost();
            Ribbon = new RibbonMenuWpf();
            //Ribbon.DataContext = Form1.Form;
            elementHost1.Child = Ribbon;
            Controls.Add(elementHost1);
            elementHost1.Dock = DockStyle.Fill;
            elementHost1.AutoSize = true;
        }

        public RibbonMenuWpf Ribbon;

        internal void SetTab(RibbonTab tab)
        {
            tab.IsSelected = true;
        }
    }
}
