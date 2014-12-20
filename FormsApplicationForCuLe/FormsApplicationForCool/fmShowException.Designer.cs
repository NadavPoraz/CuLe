namespace Irony.GrammarExplorer
{
    partial class fmShowException
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        //protected override void dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.dispose();
        //    }
        //    base.dispose(disposing);
        //}

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtException = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // txtException
            // 
            this.txtException.AcceptsReturn = true;
            this.txtException.AcceptsTab = true;
            this.txtException.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.txtException.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtException.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtException.HideSelection = false;
            this.txtException.Location = new System.Drawing.Point(0, 0);
            this.txtException.Multiline = true;
            this.txtException.Name = "txtException";
            this.txtException.ReadOnly = true;
            this.txtException.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtException.Size = new System.Drawing.Size(806, 460);
            this.txtException.TabIndex = 2;
            this.txtException.TextChanged += new System.EventHandler(this.txtException_TextChanged);
            // 
            // fmShowException
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 460);
            this.Controls.Add(this.txtException);
            this.Name = "fmShowException";
            this.Text = "Exception";
            this.Load += new System.EventHandler(this.fmShowException_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtException;
    }
}