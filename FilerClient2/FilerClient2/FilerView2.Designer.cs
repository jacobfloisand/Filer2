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
            this.ResourcePanel = new System.Windows.Forms.Panel();
            this.ResourcesRightPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.ResourcesLeftPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.RibbonPanel = new System.Windows.Forms.Panel();
            this.UserLabel = new System.Windows.Forms.Label();
            this.UnitTypeDropDown = new System.Windows.Forms.ComboBox();
            this.CurrentClassLabel = new System.Windows.Forms.Label();
            this.LoadingPanel = new System.Windows.Forms.PictureBox();
            this.ArrowPanel = new System.Windows.Forms.Panel();
            this.ResourcePanel.SuspendLayout();
            this.RibbonPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // ClassesPanel
            // 
            this.ClassesPanel.BackColor = System.Drawing.Color.Transparent;
            this.ClassesPanel.Font = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ClassesPanel.Location = new System.Drawing.Point(136, 150);
            this.ClassesPanel.Margin = new System.Windows.Forms.Padding(4);
            this.ClassesPanel.Name = "ClassesPanel";
            this.ClassesPanel.Size = new System.Drawing.Size(1090, 400);
            this.ClassesPanel.TabIndex = 0;
            // 
            // ResourcePanel
            // 
            this.ResourcePanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ResourcePanel.BackgroundImage")));
            this.ResourcePanel.Controls.Add(this.ResourcesRightPanel);
            this.ResourcePanel.Controls.Add(this.ResourcesLeftPanel);
            this.ResourcePanel.Location = new System.Drawing.Point(15, 30);
            this.ResourcePanel.Margin = new System.Windows.Forms.Padding(4);
            this.ResourcePanel.Name = "ResourcePanel";
            this.ResourcePanel.Size = new System.Drawing.Size(1325, 644);
            this.ResourcePanel.TabIndex = 1;
            this.ResourcePanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragDrop);
            this.ResourcePanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragEnter);
            // 
            // ResourcesRightPanel
            // 
            this.ResourcesRightPanel.BackColor = System.Drawing.Color.Transparent;
            this.ResourcesRightPanel.Location = new System.Drawing.Point(674, 24);
            this.ResourcesRightPanel.Name = "ResourcesRightPanel";
            this.ResourcesRightPanel.Size = new System.Drawing.Size(592, 599);
            this.ResourcesRightPanel.TabIndex = 1;
            this.ResourcesRightPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragDrop);
            this.ResourcesRightPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragEnter);
            // 
            // ResourcesLeftPanel
            // 
            this.ResourcesLeftPanel.BackColor = System.Drawing.Color.Transparent;
            this.ResourcesLeftPanel.Location = new System.Drawing.Point(58, 24);
            this.ResourcesLeftPanel.Name = "ResourcesLeftPanel";
            this.ResourcesLeftPanel.Size = new System.Drawing.Size(593, 599);
            this.ResourcesLeftPanel.TabIndex = 0;
            this.ResourcesLeftPanel.DragDrop += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragDrop);
            this.ResourcesLeftPanel.DragEnter += new System.Windows.Forms.DragEventHandler(this.ResourcePanel_DragEnter);
            // 
            // RibbonPanel
            // 
            this.RibbonPanel.Controls.Add(this.UserLabel);
            this.RibbonPanel.Controls.Add(this.UnitTypeDropDown);
            this.RibbonPanel.Controls.Add(this.CurrentClassLabel);
            this.RibbonPanel.Controls.Add(this.LoadingPanel);
            this.RibbonPanel.Controls.Add(this.ArrowPanel);
            this.RibbonPanel.Location = new System.Drawing.Point(18, 1);
            this.RibbonPanel.Name = "RibbonPanel";
            this.RibbonPanel.Size = new System.Drawing.Size(1319, 30);
            this.RibbonPanel.TabIndex = 2;
            // 
            // UserLabel
            // 
            this.UserLabel.Location = new System.Drawing.Point(1116, 3);
            this.UserLabel.Name = "UserLabel";
            this.UserLabel.Size = new System.Drawing.Size(200, 22);
            this.UserLabel.TabIndex = 4;
            this.UserLabel.Text = "Mary Christensen";
            this.UserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // UnitTypeDropDown
            // 
            this.UnitTypeDropDown.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.UnitTypeDropDown.FormattingEnabled = true;
            this.UnitTypeDropDown.Location = new System.Drawing.Point(767, 3);
            this.UnitTypeDropDown.Name = "UnitTypeDropDown";
            this.UnitTypeDropDown.Size = new System.Drawing.Size(121, 30);
            this.UnitTypeDropDown.TabIndex = 3;
            // 
            // CurrentClassLabel
            // 
            this.CurrentClassLabel.Location = new System.Drawing.Point(509, 3);
            this.CurrentClassLabel.Name = "CurrentClassLabel";
            this.CurrentClassLabel.Size = new System.Drawing.Size(300, 22);
            this.CurrentClassLabel.TabIndex = 2;
            this.CurrentClassLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // LoadingPanel
            // 
            this.LoadingPanel.Image = ((System.Drawing.Image)(resources.GetObject("LoadingPanel.Image")));
            this.LoadingPanel.Location = new System.Drawing.Point(84, 0);
            this.LoadingPanel.Name = "LoadingPanel";
            this.LoadingPanel.Size = new System.Drawing.Size(177, 30);
            this.LoadingPanel.TabIndex = 1;
            this.LoadingPanel.TabStop = false;
            // 
            // ArrowPanel
            // 
            this.ArrowPanel.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ArrowPanel.BackgroundImage")));
            this.ArrowPanel.Location = new System.Drawing.Point(0, 0);
            this.ArrowPanel.Name = "ArrowPanel";
            this.ArrowPanel.Size = new System.Drawing.Size(41, 30);
            this.ArrowPanel.TabIndex = 0;
            this.ArrowPanel.Click += new System.EventHandler(this.ArrowPanel_Click);
            // 
            // FilerView2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1357, 675);
            this.Controls.Add(this.RibbonPanel);
            this.Controls.Add(this.ResourcePanel);
            this.Controls.Add(this.ClassesPanel);
            this.Font = new System.Drawing.Font("Palatino Linotype", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 6, 5, 6);
            this.Name = "FilerView2";
            this.Text = "Textbooks 2.0";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.FilerView2_DragDrop);
            this.ResourcePanel.ResumeLayout(false);
            this.RibbonPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.LoadingPanel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel ClassesPanel;
        private System.Windows.Forms.Panel ResourcePanel;
        private System.Windows.Forms.FlowLayoutPanel ResourcesRightPanel;
        private System.Windows.Forms.FlowLayoutPanel ResourcesLeftPanel;
        private System.Windows.Forms.Panel RibbonPanel;
        private System.Windows.Forms.Panel ArrowPanel;
        private System.Windows.Forms.PictureBox LoadingPanel;
        private System.Windows.Forms.ComboBox UnitTypeDropDown;
        private System.Windows.Forms.Label CurrentClassLabel;
        private System.Windows.Forms.Label UserLabel;
    }
}

