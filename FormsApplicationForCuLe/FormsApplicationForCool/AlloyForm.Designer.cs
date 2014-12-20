namespace FormsApplicationForCuLe
{
    partial class AlloyForm
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
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.AlloyTextBox = new FastColoredTextBoxNS.FastColoredTextBox();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.AlloyTextBox);
            this.groupBox3.Location = new System.Drawing.Point(-2, 0);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(1227, 571);
            this.groupBox3.TabIndex = 7;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Alloy Model";
            this.groupBox3.Enter += new System.EventHandler(this.groupBox3_Enter);
            // 
            // AlloyTextBox
            // 
            this.AlloyTextBox.AutoScrollMinSize = new System.Drawing.Size(25, 15);
            this.AlloyTextBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.AlloyTextBox.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.AlloyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.AlloyTextBox.Location = new System.Drawing.Point(3, 16);
            this.AlloyTextBox.Name = "AlloyTextBox";
            this.AlloyTextBox.ReadOnly = true;
            this.AlloyTextBox.Size = new System.Drawing.Size(1221, 552);
            this.AlloyTextBox.TabIndex = 24;
            this.AlloyTextBox.Load += new System.EventHandler(this.AlloyTextBox_Load);
            // 
            // AlloyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1227, 571);
            this.Controls.Add(this.groupBox3);
            this.Name = "AlloyForm";
            this.Text = "AlloyForm";
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox3;
        private FastColoredTextBoxNS.FastColoredTextBox AlloyTextBox;
    }
}