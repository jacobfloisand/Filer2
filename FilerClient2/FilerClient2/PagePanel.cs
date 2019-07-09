using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilerClient2
{
    class PagePanel
    {
        TableLayoutPanel BasePanel;
        string[] ColorList = {"Dark_Purple_Square.png", "Pink_Square.png","Blue_Green_Square.png","Yellow_Square.png", "Red_Square.png","Light_Blue_Square.png","Orange_Square.png",
                                "Light_Purple_Square.png","Green_Square.png","Dark_Blue_Square.png" };
        public PagePanel(string Name, string Date, string Type, bool IsLink, ResourceColor ResourceColor)
        {
            BasePanel = new TableLayoutPanel();
            this.BasePanel.ColumnCount = 4;
            this.BasePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 74.07407F));
            this.BasePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.92593F));
            this.BasePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 37F));
            this.BasePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.BasePanel.RowCount = 5;
            this.BasePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 34.32836F));
            this.BasePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 65.67164F));
            this.BasePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 21F));
            this.BasePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 18F));
            this.BasePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 10F));
            this.BasePanel.Size = new System.Drawing.Size(128, 128);


            
            string ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            ResourcesPath = Path.Combine(ResourcesPath, "Square_Images");
            string SquarePath = Path.Combine(ResourcesPath, ColorList[(int)ResourceColor]);
            BasePanel.BackgroundImage = System.Drawing.Image.FromFile(SquarePath);

            if (IsLink)
            {
                AddLinkSymbol();
            }


            //Add Delete Symbol
            Panel DeletePanel = new Panel();
            DeletePanel.BackColor = Color.Red;
            BasePanel.Controls.Add(DeletePanel, 3, 0);

            //Add Name Label
            Label NameLabel = new Label();
            NameLabel.Text = Name;
            NameLabel.BackColor = Color.Transparent;
            NameLabel.Size = new Size(128, 50);
            BasePanel.Controls.Add(NameLabel, 0, 1);
            BasePanel.SetColumnSpan(NameLabel, 5);
            NameLabel.TextAlign = ContentAlignment.MiddleCenter;

            //Add Date label
            Label DateLabel = new Label();
            DateLabel.Text = Date;
            DateLabel.BackColor = Color.Transparent;
            DateLabel.Size = new Size(128, 20);
            DateLabel.Font = new Font("Palatino Linotype", 8, FontStyle.Bold);
            BasePanel.Controls.Add(DateLabel, 0, 2);
            BasePanel.SetColumnSpan(DateLabel, 4);
            DateLabel.TextAlign = ContentAlignment.TopCenter;

            //Add Type label
            Label TypeLabel = new Label();
            TypeLabel.Text = Type;
            TypeLabel.BackColor = Color.Transparent;
            TypeLabel.Size = new Size(128, 20);
            TypeLabel.Font = new Font("Palatino Linotype", 8, FontStyle.Bold);
            BasePanel.Controls.Add(TypeLabel, 0, 3);
            BasePanel.SetColumnSpan(TypeLabel, 4);
            TypeLabel.TextAlign = ContentAlignment.TopCenter;
        }

        public void AddLinkSymbol()
        {
            //Path
            string ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            string LinkPath = Path.Combine(ResourcesPath, "Link_Icon.png");
            Panel LinkPanel = new Panel();
            LinkPanel.BackColor = Color.Transparent;
            LinkPanel.BackgroundImage = System.Drawing.Image.FromFile(LinkPath);
            BasePanel.Controls.Add(LinkPanel, 0, 0);
        }

        public void AddTo(Panel Parent)
        {
            Parent.Controls.Add(BasePanel);
        }
    }
}
