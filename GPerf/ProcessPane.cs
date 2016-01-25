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

            ContextPane pane = new ContextPane(Color.Green);
            flowPanel.Controls.Add(pane);

            pane = new ContextPane(Color.Blue);
            flowPanel.Controls.Add(pane);
        }
    }
}
