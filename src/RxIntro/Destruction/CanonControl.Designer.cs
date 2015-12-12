namespace Bbv.Rx.Destruction
{
    partial class CanonControl
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
            this.components = new System.ComponentModel.Container();
            this.power = new System.Windows.Forms.TrackBar();
            this.angle = new System.Windows.Forms.VScrollBar();
            this.toolTipAngle = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.power)).BeginInit();
            this.SuspendLayout();
            // 
            // power
            // 
            this.power.Dock = System.Windows.Forms.DockStyle.Top;
            this.power.LargeChange = 2;
            this.power.Location = new System.Drawing.Point(0, 0);
            this.power.Name = "power";
            this.power.Size = new System.Drawing.Size(584, 45);
            this.power.TabIndex = 3;
            this.power.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.power.Value = 3;
            // 
            // angle
            // 
            this.angle.Dock = System.Windows.Forms.DockStyle.Right;
            this.angle.Location = new System.Drawing.Point(567, 45);
            this.angle.Maximum = 89;
            this.angle.Minimum = 1;
            this.angle.Name = "angle";
            this.angle.Size = new System.Drawing.Size(17, 516);
            this.angle.TabIndex = 4;
            this.angle.Value = 45;
            this.angle.Scroll += new System.Windows.Forms.ScrollEventHandler(this.AngleScroll);
            // 
            // CanonControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(584, 561);
            this.Controls.Add(this.angle);
            this.Controls.Add(this.power);
            this.Name = "CanonControl";
            this.Text = "Destruction";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            ((System.ComponentModel.ISupportInitialize)(this.power)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TrackBar power;
        private System.Windows.Forms.VScrollBar angle;
        private System.Windows.Forms.ToolTip toolTipAngle;
    }
}