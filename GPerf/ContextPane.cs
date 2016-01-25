using System.Drawing;
using System.Windows.Forms;

namespace GPerf
{
    public partial class ContextPane : UserControl
    {
        public ContextPane(Color color)
        {
            InitializeComponent();
            colorKeyPanel.BackColor = color;
        }
    }
}
