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
        string FullName;
        string ResourcesPath = "";
        public event Action<string> Clicked;
        public event Action<string, Point> RightClicked;
        Label TextBox;
        string[] ColorList = { "Blue_Green_Book.png", "Dark_Blue_Book.png", "Dark_Purple_Book.png", "Green_Book.png", "Light_Blue_Book.png",
                                "Light_Purple_Book.png", "Orange_Book.png", "Pink_Book.png", "Red_Book.png", "Yellow_Book.png"};
        public ClassImage(string Name, ResourceColor ClassColor)
        {
            //ImagePanel
            ImagePanel = new Panel();
            ImagePanel.Size = new System.Drawing.Size(128, 128);
            FullName = Name;
            ImagePanel.Click += ImageClicked;
            //TextBox
            TextBox = new Label();
            TextBox.BackColor = Color.Transparent;
            //TextBox.BackColor = Color.Purple;
            TextBox.Location = new Point(6, 7);
            TextBox.Size = new System.Drawing.Size(107, 90);
            TextBox.Text = Name.Substring(0, Name.Length - 3);
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
            TextBox.BackColor = Color.Transparent;
            ImagePanel.BackColor = Color.Transparent;

        }

        public void AddTo(Panel parent)
        {
            parent.Controls.Add(ImagePanel);
        }

        private void ImageClicked(Object o, EventArgs e)
        {
            MouseEventArgs m = (MouseEventArgs)e;
            if (m.Button == System.Windows.Forms.MouseButtons.Left)
            {
                Clicked?.Invoke(FullName);
            }
            else if(m.Button == System.Windows.Forms.MouseButtons.Right)
            {
                RightClicked?.Invoke(FullName, ImagePanel.Parent.PointToClient(Cursor.Position));
            }
        }
    }
}
