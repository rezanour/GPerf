using System;
using System.Windows.Forms;

namespace GPerf
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tablePanel.RowCount = 2;

            ProcessPane pane = new ProcessPane(2345, "Outlook.exe");
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 0);

            pane = new ProcessPane(1173, "OculusWorldDemo.exe");
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 1);
        }
    }
}
