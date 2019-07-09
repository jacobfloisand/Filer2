namespace FilerClient2
{
    partial class FilerView2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FilerView2));
            this.ClassesPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ResourcePanel = new System.Windows.Forms.FlowLayoutPanel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.ResourcePanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // ClassesPanel
            // 
            this.ClassesPanel.BackColor = System.Drawing.SystemColors.Window;
            this.ClassesPanel.Font = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClassesPanel.Location = new System.Drawing.Point(136, 150);
            this.ClassesPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ClassesPanel.Name = "ClassesPanel";
            this.ClassesPanel.Size = new System.Drawing.Size(1000, 400);
            this.ClassesPanel.TabIndex = 0;
            // 
            // ResourcePanel
            // 
            this.ResourcePanel.BackColor = System.Drawing.SystemColors.Window;
            this.ResourcePanel.Controls.Add(this.tableLayoutPanel1);
            this.ResourcePanel.Location = new System.Drawing.Point(15, 16);
            this.ResourcePanel.Margin = new System.Windows.Forms.Padding(4);
            this.ResourcePanel.Name = "ResourcePanel";
            this.ResourcePanel.Size = new System.Drawing.Size(1325, 644);
            this.ResourcePanel.TabIndex = 1;
            this.ResourcePanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragDrop);
            this.ResourcePanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragEnter);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 74.07407F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.92593F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.32836F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65.67164F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(128, 128);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // FilerView2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1357, 675);
            this.Controls.Add(this.ResourcePanel);
            this.Controls.Add(this.ClassesPanel);
            this.Font = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "FilerView2";
            this.Text = "Textbooks 2.0";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilerView2_DragDrop);
            this.ResourcePanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ClassesPanel;
        private System.Windows.Forms.FlowLayoutPanel ResourcePanel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
    }
}

