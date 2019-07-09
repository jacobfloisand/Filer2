using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Dynamic;

namespace FilerClient2
{
    public partial class FilerView2 : Form
    {
        public string CurrentClass = "";
        private delegate void MoveUpDelegate();
        public event Action<string> ClassClick; //Param is the class name that was clicked.
        public event Action<string, string, string, string, string, string, string> UploadEvent; //Contents, Name, Unit, Type, IsLink, Override, Comments
        private Dictionary<string, string> Comments = new Dictionary<string, string>();
        public FilerView2()
        {
            InitializeComponent();
            ClassesPanel.BringToFront();
            ResourcePanel.BackColor = Color.MediumPurple;
            ResourcePanel.AllowDrop = true;
            AllowDrop = true;
        }

        public void UpdateClasses(List<string> Classes)
        {
            int count = 0; //This is used to keep the code simple. In the future save the color with the name of the item.
            foreach(string c in Classes)
            {
                ClassImage current = new ClassImage(c, (ResourceColor)((count++) % 10));
                current.Clicked += ClassClicked;
                current.AddTo(ClassesPanel);
            }
        }

        private void ClassClicked(string ClassName)
        {
            CurrentClass = ClassName;
            Thread t = new Thread(new ThreadStart(SwitchToResourcesPanel));
            t.Start();
            ClassClick?.Invoke(ClassName);
        }

        private void SwitchToResourcesPanel()
        {
            while(ClassesPanel.Location.Y + ClassesPanel.Size.Height > 0)
            {
                ClassesPanel.Invoke((MoveUpDelegate)MoveUp);
                Thread.Sleep(5);
            }
        }

        private void MoveUp()
        {
            ClassesPanel.Location = new Point(
                    ClassesPanel.Location.X,
                    ClassesPanel.Location.Y - 5
                );
        }

        internal void UpdateResources(List<ResourceData> Resources)
        {
            int count = 0;
            foreach(ResourceData r in Resources)
            {
                PagePanel current = new PagePanel(r.Name, r.Date, r.Type, r.Link != null, (ResourceColor)((count++)%10));
                Comments.Add(r.Name, r.Comments);
                current.AddTo(ResourcePanel);
            }
        }

        private void ResourcePanel_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            e.Effect = DragDropEffects.Copy;
            if(fileList.Count() > 1)
            {
                throw new Exception("Please drag and drop one file at a time!");
            }
            foreach(string s in fileList)
            {
                string Contents = File.ReadAllText(s);
                UploadEvent?.Invoke(Contents, Path.GetFileName(s), null, null, "false", "false", null); 
            }
        }

        private void ResourcePanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        private void FilerView2_DragDrop(object sender, DragEventArgs e)
        {
            ResourcePanel_DragDrop(sender, e);
        }
    }
}
