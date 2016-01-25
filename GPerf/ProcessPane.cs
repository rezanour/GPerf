using System;
using System.Drawing;
using System.Windows.Forms;

namespace GPerf
{
    public partial class ProcessPane : UserControl
    {
        public int ProcessId { get; private set; }
        public string ProcessName { get; private set; }

        public ProcessPane(int processId, string processName)
        {
            ProcessId = processId;
            ProcessName = processName;

            InitializeComponent();
        }

        private void ProcessPane_Load(object sender, EventArgs e)
        {
            processNameLabel.Text = string.Format("{0} ({1})", ProcessName, ProcessId);

            tablePanel.RowCount = 2;

            ContextPane pane = new ContextPane(Color.Green);
            pane.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 0);

            pane = new ContextPane(Color.Blue);
            pane.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            tablePanel.Controls.Add(pane);
            tablePanel.SetRow(pane, 1);

            int height = 0;
            foreach (Control control in tablePanel.Controls)
            {
                height += control.Height;
            }

            int oldHeight = tablePanel.Height;
            tablePanel.Height = height;

            Height += height - oldHeight;
        }
    }
}
