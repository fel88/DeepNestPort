using System.Windows.Controls.Ribbon;
using System.Windows.Forms.Integration;

namespace DeepNestPort.Core
{
    public partial class RibbonMenu : UserControl
    {
        public RibbonTab NestTab => Ribbon.nestTab;
        public RibbonTab GeneralTab => Ribbon.projectTab;
        public RibbonMenu()
        {
            InitializeComponent();
            
            ElementHost elementHost1 = new ElementHost();
            Ribbon = new RibbonMenuWpf();
            Ribbon.TabChanged += Ribbon_TabChanged;

            //Ribbon.DataContext = Form1.Form;
            elementHost1.Child = Ribbon;
            Controls.Add(elementHost1);
            elementHost1.Dock = DockStyle.Fill;
            elementHost1.AutoSize = true;
        }

        private void Ribbon_TabChanged()
        {
            TabChanged?.Invoke();
        }

        public event Action TabChanged;

        public RibbonMenuWpf Ribbon;

        internal void SetTab(RibbonTab tab)
        {
            tab.IsSelected = true;
        }

        internal void ApplySettings()
        {
            Ribbon.borderScroll.IsChecked = Form1.Form.BorderScrollEnabled;
        }
    }
}
