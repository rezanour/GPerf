using System;
using System.Windows.Forms;

namespace GPerf
{
    public partial class MainForm : Form
    {
        DateTime StartTime = DateTime.Now;
        DateTime EndTime = DateTime.Now + new TimeSpan(0, 0, 120);

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            tablePanel.RowCount = 2;

            ProcessPane pane = new ProcessPane(2345, "Outlook.exe");
            pane.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 0);

            pane = new ProcessPane(1173, "OculusWorldDemo.exe");
            pane.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 1);
        }

        private void timelinePanel_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
