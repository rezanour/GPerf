namespace GPerf
{
    partial class ContextPane
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
            this.contextLabel = new System.Windows.Forms.Label();
            this.colorKeyPanel = new System.Windows.Forms.Panel();
            this.packetPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // contextLabel
            // 
            this.contextLabel.AutoSize = true;
            this.contextLabel.Location = new System.Drawing.Point(3, 5);
            this.contextLabel.Name = "contextLabel";
            this.contextLabel.Size = new System.Drawing.Size(80, 13);
            this.contextLabel.TabIndex = 0;
            this.contextLabel.Text = "Device Context";
            // 
            // colorKeyPanel
            // 
            this.colorKeyPanel.BackColor = System.Drawing.Color.Blue;
            this.colorKeyPanel.Location = new System.Drawing.Point(3, 21);
            this.colorKeyPanel.Name = "colorKeyPanel";
            this.colorKeyPanel.Size = new System.Drawing.Size(77, 43);
            this.colorKeyPanel.TabIndex = 1;
            // 
            // packetPanel
            // 
            this.packetPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.packetPanel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.packetPanel.Location = new System.Drawing.Point(86, 21);
            this.packetPanel.Name = "packetPanel";
            this.packetPanel.Size = new System.Drawing.Size(200, 43);
            this.packetPanel.TabIndex = 2;
            // 
            // ContextPane
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.packetPanel);
            this.Controls.Add(this.colorKeyPanel);
            this.Controls.Add(this.contextLabel);
            this.Name = "ContextPane";
            this.Size = new System.Drawing.Size(289, 70);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label contextLabel;
        private System.Windows.Forms.Panel colorKeyPanel;
        private System.Windows.Forms.Panel packetPanel;
    }
}
