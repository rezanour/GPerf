namespace GPerf
{
    partial class ProcessPane
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.processNameLabel = new System.Windows.Forms.Label();
            this.tablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // processNameLabel
            // 
            this.processNameLabel.AutoSize = true;
            this.processNameLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.processNameLabel.Location = new System.Drawing.Point(4, 4);
            this.processNameLabel.Name = "processNameLabel";
            this.processNameLabel.Size = new System.Drawing.Size(126, 13);
            this.processNameLabel.TabIndex = 0;
            this.processNameLabel.Text = "<processName (pid)>";
            // 
            // tablePanel
            // 
            this.tablePanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tablePanel.ColumnCount = 1;
            this.tablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tablePanel.Location = new System.Drawing.Point(7, 21);
            this.tablePanel.Name = "tablePanel";
            this.tablePanel.RowCount = 1;
            this.tablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tablePanel.Size = new System.Drawing.Size(402, 68);
            this.tablePanel.TabIndex = 1;
            // 
            // ProcessPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.tablePanel);
            this.Controls.Add(this.processNameLabel);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ProcessPane";
            this.Size = new System.Drawing.Size(416, 97);
            this.Load += new System.EventHandler(this.ProcessPane_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label processNameLabel;
        private System.Windows.Forms.TableLayoutPanel tablePanel;
    }
}
