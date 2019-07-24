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
        public event Action<string> Delete_Clicked;
        public event Action<string> Double_Clicked;
        public event Action<string, Point> Right_Clicked;
        string Name = null;
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

            this.Name = Name;
            
            string ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            ResourcesPath = Path.Combine(ResourcesPath, "Square_Images");
            string SquarePath = Path.Combine(ResourcesPath, ColorList[(int)ResourceColor]);
            BasePanel.BackgroundImage = System.Drawing.Image.FromFile(SquarePath);
            BasePanel.BackColor = Color.Transparent;
            BasePanel.DoubleClick += Double_Click;
            BasePanel.Click += Right_Click;

            if (IsLink)
            {
                AddLinkSymbol();
            }


            //Add Delete Symbol
            PictureBox DeletePanel = new PictureBox();
            string ExitPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ExitPath = Path.Combine(ExitPath, "Images");
            ExitPath = Path.Combine(ExitPath, "Delete_Symbol.png");
            DeletePanel.ImageLocation = ExitPath;
            DeletePanel.InitialImage = System.Drawing.Image.FromFile(ExitPath);
            BasePanel.Controls.Add(DeletePanel, 3, 0);
            DeletePanel.Click += DeletePanel_Click;

            //Add Name Label
            Label NameLabel = new Label();
            NameLabel.Text = Name;
            NameLabel.BackColor = Color.Transparent;
            NameLabel.Size = new Size(128, 50);
            BasePanel.Controls.Add(NameLabel, 0, 1);
            BasePanel.SetColumnSpan(NameLabel, 5);
            NameLabel.TextAlign = ContentAlignment.MiddleCenter;
            NameLabel.DoubleClick += Double_Click;
            NameLabel.Click += Right_Click;

            //Add Date label
            Label DateLabel = new Label();
            DateLabel.Text = Date;
            DateLabel.BackColor = Color.Transparent;
            DateLabel.Size = new Size(128, 20);
            DateLabel.Font = new Font("Palatino Linotype", 8, FontStyle.Bold);
            BasePanel.Controls.Add(DateLabel, 0, 2);
            BasePanel.SetColumnSpan(DateLabel, 4);
            DateLabel.TextAlign = ContentAlignment.TopCenter;
            DateLabel.DoubleClick += Double_Click;
            DateLabel.Click += Right_Click;

            //Add Type label
            Label TypeLabel = new Label();
            TypeLabel.Text = Type;
            TypeLabel.BackColor = Color.Transparent;
            TypeLabel.Size = new Size(128, 20);
            TypeLabel.Font = new Font("Palatino Linotype", 8, FontStyle.Bold);
            BasePanel.Controls.Add(TypeLabel, 0, 3);
            BasePanel.SetColumnSpan(TypeLabel, 4);
            TypeLabel.TextAlign = ContentAlignment.TopCenter;
            TypeLabel.DoubleClick += Double_Click;
            TypeLabel.Click += Right_Click;
        }

        private void DeletePanel_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Delete button clicked.");
            Delete_Clicked?.Invoke(Name);
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

        private void Delete_Click(object sender, EventArgs e)
        {
            //Delete_Clicked?.Invoke(Name);
        }

        public void AddTo(Panel Parent)
        {
            Parent.Controls.Add(BasePanel);
        }

        private void Double_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Double Click happened in the Page Panel");
            Double_Clicked?.Invoke(Name);
        }

        private void Right_Click(object sender, EventArgs e)
        {
            MouseEventArgs m = (MouseEventArgs)e;
            if(m.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Console.WriteLine("Right click happened in the page panel.");
                Right_Clicked?.Invoke(Name, BasePanel.Parent.Parent.PointToClient(Cursor.Position));
            }
        }
    }
}
