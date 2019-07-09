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
    class ResourceImage
    {
        /*
        Panel ImagePanel;
        TableLayoutPanel RibbonPanel;
        Panel LinkPanel;
        Panel ExitPanel;
        string ResourcesPath = "";
        public event Action<string> Clicked;
        Label NameLabel;
        Label DataLabel;
        Label TypeLabel;
        string[] ColorList = {"Dark_Purple_Square.png", "Pink_Square.png","Blue_Green_Square.png","Yellow_Square.png", "Red_Square.png","Light_Blue_Square.png","Orange_Square.png",
                                "Light_Purple_Square.png","Green_Square.png","Dark_Blue_Square.png" };
        public ResourceImage(string Name, string Date, string Type, bool IsLink, ResourceColor ClassColor)
        {
            //Path
            ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            //RibbonPanel
            RibbonPanel = new TableLayoutPanel();
            RibbonPanel.RowCount = 1;
            RibbonPanel.ColumnCount = 4;
            string LinkPath = Path.Combine(ResourcesPath, "Link_Icon.png");
            LinkPanel = new Panel();
            LinkPanel.BackgroundImage = System.Drawing.Image.FromFile(LinkPath);
            RibbonPanel.Controls.Add(LinkPanel);
            RibbonPanel.Controls.Add(new Panel());
            RibbonPanel.Controls.Add(new Panel());
            RibbonPanel.Controls.Add(LinkPanel);
            ExitPanel = new Panel();
            ExitPanel.BackColor = Color.Red;
            RibbonPanel.Controls.Add(ExitPanel);

            //NameLabel
            NameLabel = new Label();
            NameLabel.BackColor = Color.Transparent;
            TextBox.Location = new Point(6, 7);
            TextBox.Size = new System.Drawing.Size(107, 90);
            TextBox.Text = Name;
            TextBox.TextAlign = ContentAlignment.MiddleCenter;
            ImagePanel.Controls.Add(TextBox);
            TextBox.Click += ImageClicked;
            //Path
            ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            ResourcesPath = Path.Combine(ResourcesPath, "Square_Images");
            string BookColor = ColorList[(int)ClassColor];
            string PathToPicture = Path.Combine(ResourcesPath, BookColor);
            ImagePanel.BackgroundImage = System.Drawing.Image.FromFile(PathToPicture);

            //ImagePanel
            ImagePanel = this.PagePanel;
            ImagePanel.
            ImagePanel.Size = new System.Drawing.Size(128, 128);
            ImagePanel.Click += ImageClicked;
            ImagePanel.Controls.Add(RibbonPanel);
        }

        public void AddTo(Panel parent)
        {
            parent.Controls.Add(ImagePanel);
        }

        private void ImageClicked(Object o, EventArgs e)
        {
            Clicked?.Invoke(TextBox.Text);
        }
        */
    }
}
