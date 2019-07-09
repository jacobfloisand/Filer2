using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilerClient2
{
    class ClassImage
    {
        Panel ImagePanel;
        string Text;
        string ResourcesPath = "";
        public event Action<string> Clicked;
        Label TextBox;
        string[] ColorList = {"Dark_Purple_Book.png", "Pink_Book.png","Blue_Green_Book.png","Yellow_Book.png", "Red_Book.png","Light_Blue_Book.png","Orange_Book.png",
                                "Light_Purple_Book.png","Green_Book.png","Dark_Blue_Book.png" };
        public ClassImage(string Name, ResourceColor ClassColor)
        {
            //ImagePanel
            ImagePanel = new Panel();
            ImagePanel.Size = new System.Drawing.Size(128, 128);
            Text = Name;
            ImagePanel.Click += ImageClicked;
            //TextBox
            TextBox = new Label();
            TextBox.BackColor = Color.Transparent;
            TextBox.Location = new Point(6, 7);
            TextBox.Size = new System.Drawing.Size(107, 90);
            TextBox.Text = Name;
            TextBox.TextAlign = ContentAlignment.MiddleCenter;
            ImagePanel.Controls.Add(TextBox);
            TextBox.Click += ImageClicked;
            //Path
            ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            ResourcesPath = Path.Combine(ResourcesPath, "Images");
            ResourcesPath = Path.Combine(ResourcesPath, "Book_Images");
            string BookColor = ColorList[(int)ClassColor];
            string PathToPicture = Path.Combine(ResourcesPath, BookColor);
            ImagePanel.BackgroundImage = System.Drawing.Image.FromFile(PathToPicture);

            
        }

        public void AddTo(Panel parent)
        {
            parent.Controls.Add(ImagePanel);
        }

        private void ImageClicked(Object o, EventArgs e)
        {
            Clicked?.Invoke(TextBox.Text);
        }
    }
}
