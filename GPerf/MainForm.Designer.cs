namespace GPerf
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.timelinePanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // tablePanel
            // 
            this.tablePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tablePanel.AutoScroll = true;
            this.tablePanel.BackColor = System.Drawing.SystemColors.ControlDark;
            this.tablePanel.ColumnCount = 1;
            this.tablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tablePanel.Location = new System.Drawing.Point(12, 51);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.RowCount = 1;
            this.tablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tablePanel.Size = new System.Drawing.Size(981, 578);
            this.tablePanel.TabIndex = 0;
            // 
            // timelinePanel
            // 
            this.timelinePanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.timelinePanel.Location = new System.Drawing.Point(12, 12);
            this.timelinePanel.Name = "timelinePanel";
            this.timelinePanel.Size = new System.Drawing.Size(981, 33);
            this.timelinePanel.TabIndex = 1;
            this.timelinePanel.Paint += new System.Windows.Forms.PaintEventHandler(this.timelinePanel_Paint);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1005, 641);
            this.Controls.Add(this.timelinePanel);
            this.Controls.Add(this.tablePanel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MainForm";
            this.Text = "GPerf";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tablePanel;
        private System.Windows.Forms.Panel timelinePanel;
    }
}

