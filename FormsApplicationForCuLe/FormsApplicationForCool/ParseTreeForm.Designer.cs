namespace FormsApplicationForCuLe
{
    partial class ParseTreeForm
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
            this.ParseTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // ParseTreeView
            // 
            this.ParseTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ParseTreeView.Location = new System.Drawing.Point(0, 0);
            this.ParseTreeView.Name = "ParseTreeView";
            this.ParseTreeView.Size = new System.Drawing.Size(1264, 480);
            this.ParseTreeView.TabIndex = 0;
            this.ParseTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect_1);
            // 
            // ParseTree
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 480);
            this.Controls.Add(this.ParseTreeView);
            this.Name = "ParseTree";
            this.Text = "ParseTree";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView ParseTreeView;

    }
}