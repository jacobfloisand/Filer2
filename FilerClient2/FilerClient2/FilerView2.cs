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
using System.Runtime.InteropServices;

namespace FilerClient2
{
    public partial class FilerView2 : Form
    {
        public string CurrentClass = "";

        private delegate void NoArgDelegate();
        private delegate void DrawingDelegate(Control Target);
        private delegate void AddResourcesDelegate(List<ResourceData> Resources, int PageNum);

        public event Action<string> ClassClick; //Param: the class name that was clicked.
        public event Action<string, string, string, string, string, string, string> UploadEvent; //Params: Contents, Name, Unit, Type, IsLink, Override, Comments
        public event Action<string, string> DeleteEvent; //Name and IsLink.
        public event Action<string> GetContentsEvent; //Params: Name
        public event Action<string, string, string, string, string, string, string, string> UpdateResourceEvent; //CurrentName, IsLink, UpdatedClass, UpdatedName, UpdatedContents, UpdatedUnit, UpdatedType, UpdatedComments

        private Thread MoveUpThread = null;

        private Bitmap ClassesBackImage;
        private Bitmap ResourcesBackImage;

        public bool ClassView = true;

        private int CurrentPage = 0;

        public List<ResourceData> CurrentResources = new List<ResourceData>();
        public List<string> AllClasses = new List<string>();

        FlowLayoutPanel ClassesMediator;

        string ResourcesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

        private const int FRAMESPERSEC = 2;
        private const int CLASSESPANELJUMPPIXELS = 200;

        public ResourceData TempOldResource = null;
        public ResourceData TempUpdatedResource = null;


        public Dictionary<string, string> Comments = new Dictionary<string, string>();
        public FilerView2()
        {
            InitializeComponent();
            LoadingPanel.Show(); //For debugging: Makes loading symbol go.

            //Set background image for ClassesPanel(brown one).
            string ClassesBackPath = Path.Combine(ResourcesPath, "Images");
            ClassesBackPath = Path.Combine(ClassesBackPath, "Classes_Background.png");
            ClassesPanel.BackgroundImage = System.Drawing.Image.FromFile(ClassesBackPath);

            //Set background image for ResourcesPanel(The big book).
            string ResBackPath = Path.Combine(ResourcesPath, "Images");
            ResBackPath = Path.Combine(ResBackPath, "Open_Books");
            ResBackPath = Path.Combine(ResBackPath, "Blue_Green_Open_Book.png");
            ResourcePanel.AllowDrop = true;

            //Build ClassesPanelHelper. The ClassesPanelHelper is used for the background when showing the ClassesPanel.
            Panel ClassesPanelHelper = new Panel();
            ClassesPanelHelper.Size = ResourcePanel.Size;
            ClassesPanelHelper.Location = ResourcePanel.Location;
            ClassesPanelHelper.BackgroundImage = ResourcePanel.BackgroundImage;
            //Add ClassesPanelHelper to the View.
            ClassesPanelHelper.Parent = this;
            ClassesPanel.Parent = ClassesPanelHelper;
            ClassesPanelHelper.BringToFront();
            //ControlHelper.SuspendDrawing(ResourcePanel); //Keeping this here in case we need is somewhere else in the code.

            //Build ClassesMediator. This is used to make the ClassesPanel act like a flow layout panel.
            ClassesMediator = new FlowLayoutPanel();
            ClassesMediator.BackColor = Color.Transparent;
            ClassesPanel.Controls.Add(ClassesMediator);
            ClassesMediator.Location = new Point(0, 0);
            ClassesMediator.Size = new Size(ClassesPanel.Size.Width - 20, ClassesPanel.Size.Height - 20);
        }

        /// <summary>
        /// This function displays the list of classes in the classes viewer. It does not show the ClassesPanel.
        /// </summary>
        /// <param name="Classes"></param>
        public void UpdateClasses(List<string> Classes)
        {
            AllClasses = Classes;
            int count = 0; //This is used to keep the code simple. In the future save the color with the name of the item.
            foreach(string c in Classes)
            {
                ClassImage current = new ClassImage(c, (ResourceColor)((count++) % 10));
                current.Clicked += ClassClicked;
                current.AddTo(ClassesMediator);
            }
            //The AddClassPanel is used to hold the AddClassButton.
            TableLayoutPanel AddClassPanel = new TableLayoutPanel();
            AddClassPanel.RowCount = 3;
            AddClassPanel.ColumnCount = 3;
            AddClassPanel.Size = new Size(128,128);
            AddClassPanel.BackColor = Color.Transparent;

            PictureBox PlusButton = new PictureBox();
            PlusButton.Size = new Size(32, 32);
            string AddClassPath = Path.Combine(ResourcesPath, "Images");
            AddClassPath = Path.Combine(AddClassPath, "Plus_Sign.png");
            PlusButton.ImageLocation = AddClassPath;
            //This for loop makes sure that the panel is pin the right place.
            for(int i = 0; i < 1; i++)
            {
                Panel Current = new Panel();
                Current.Size = new Size(32, 32);
                Current.Location = new Point(1, 1);
                Current.BackColor = Color.Transparent;
                AddClassPanel.Controls.Add(Current);
            }
            AddClassPanel.Controls.Add(PlusButton, 8, 8);
            ClassesMediator.Controls.Add(AddClassPanel);
        }

        /// <summary>
        /// This function is called when a Class Item is clicked. It sets the name of the current class and takes
        /// care of the transition from the ClassesPanel to the ResourcesPanel. It launches the ClassClick event
        /// so that the controller can get all of the Resources from the server for that class.
        /// </summary>
        /// <param name="ClassName"></param>
        private void ClassClicked(string ClassName)
        {
            CurrentClass = ClassName;
            CurrentClassLabel.Text = CurrentClass;
            ClassView = false;
            MoveUpThread = new Thread(new ThreadStart(SwitchToResourcesPanel));
            MoveUpThread.Start();
            ClassClick?.Invoke(ClassName);
        }

        /// <summary>
        /// This function is in charge of moving the ClassesPanel off of the screen when a class is clicked.
        /// </summary>
        private void SwitchToResourcesPanel()
        {
            while (ClassesPanel.Location.X + ClassesPanel.Size.Width > 50)
            {
                
                ClassesPanel.Invoke((NoArgDelegate)MoveLeft);
                Thread.Sleep(1000/FRAMESPERSEC);

            }
            ClassesPanel.Invoke((NoArgDelegate)ClassesPanel.Hide);
        }

        /// <summary>
        /// This function was designed to be called by SwitchToResourcesPanel only. It moves the ClassesPanel to the left
        /// by CLASSESPANELJUMPPIXELS.
        /// </summary>
        private void MoveLeft()
        {
            ClassesPanel.Location = new Point(
                    ClassesPanel.Location.X - CLASSESPANELJUMPPIXELS,
                    ClassesPanel.Location.Y
                );
        }

        /// <summary>
        /// This function adds anything it finds in Resources to the CurrentResources already shown
        /// in the ResourcesPanel. It takes care of updating the GUI, Comments, and Current Resources.
        /// </summary>
        /// <param name="Resources"></param>
        internal void UpdateResources(List<ResourceData> Resources)
        {
            while(Resources.Remove(null)){ }
            CurrentResources = CurrentResources.Concat(Resources).ToList();
            Comments.Clear();
            ResourcesLeftPanel.Controls.Clear();
            ResourcesRightPanel.Controls.Clear();
            CurrentResources.Sort(new DateComparator());
            Thread t = new Thread(() => UpdateResourcesHelper(CurrentResources));
            t.Start();
        }

        /// <summary>
        /// This function waits until the MoveUpThread is done(meaning the ClassesPanel has moved off screen),
        /// and then it calls AddResourcesAfterWait on the GUI thread. If MoveUpThread is null it simply calls AddResourcesAfterWait
        /// on the GUI thread. This function should only be called by UpdateResources.
        /// </summary>
        /// <param name="Resources"></param>
        private void UpdateResourcesHelper(List<ResourceData> Resources)
        {
            if(MoveUpThread != null)
            {
                MoveUpThread.Join();
                MoveUpThread = null;
            }
            ResourcePanel.Invoke((AddResourcesDelegate)AddResourcesAfterWait, Resources, CurrentPage);
        }

        internal void ShowFile(string Contents, string Name)
        {
            string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"_Viewed_Files");
            FilePath = Path.Combine(FilePath, Name);
            byte[] ContentsAsBytes = Convert.FromBase64String(Contents);
            File.WriteAllBytes(FilePath, ContentsAsBytes);
            System.Diagnostics.Process.Start(FilePath);
        }

        /// <summary>
        /// This function was made to be called by UpdateResourcesHelper only. It creates an object for every item in CurrentResources
        /// and paints it onto the ResourcePanel. It's in charge of knowing which resources to put on the pages and where. It updates
        /// Comments.
        /// </summary>
        /// <param name="Resources"></param>
        /// <param name="PageNum"></param>
        private void AddResourcesAfterWait(List<ResourceData> Resources, int PageNum)
        {
            ControlHelper.ResumeDrawing(ResourcePanel);
            ResourcePanel.Show();
            ResourcePanel.BringToFront();
            int count = 0; //Used for colors.
            //Add to left page
            for(int i = PageNum * 32; i < PageNum * 32 + 16; i++)
            {
                if(i >= Resources.Count)
                {
                    break;
                }
                PagePanel current = new PagePanel(Resources[i].Name, Resources[i].Date, Resources[i].Type, Resources[i].Link != null, (ResourceColor)((count++) % 10));
                Comments.Add(Resources[i].Name, Resources[i].Comments);
                current.AddTo(ResourcesLeftPanel);
                current.Delete_Clicked += Received_Delete;
                current.Double_Clicked += Received_Double_Click;
                current.Right_Clicked += Received_Right_Click;
            }

            //Add to right page
            for (int i = PageNum * 32 + 16; i < PageNum * 32 + 32; i++)
            {
                if (i >= Resources.Count)
                {
                    break;
                }
                PagePanel current = new PagePanel(Resources[i].Name, Resources[i].Date, Resources[i].Type, Resources[i].Link != null, (ResourceColor)((count++) % 10));
                Comments.Add(Resources[i].Name, Resources[i].Comments);
                current.AddTo(ResourcesRightPanel);
                current.Delete_Clicked += Received_Delete;
                current.Double_Clicked += Received_Double_Click;
                current.Right_Clicked += Received_Right_Click;
            }
        }

        /// <summary>
        /// This function reacts to a DragDrop event. It takes the file information from the drag drop event and
        /// launches an UploadEvent for the Controller. It automatically makes Unit, Type, and Comments null. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                byte[] ContentsAsBytes = File.ReadAllBytes(s);
                //Contents.PadRight(Contents.Length + (64 - Contents.Length % 64));
                PadBytes(ContentsAsBytes);
                string Contents = Convert.ToBase64String(ContentsAsBytes); //64 bit encoding works for all file types.
                UploadEvent?.Invoke(Contents, Path.GetFileName(s), null, null, "false", "false", null); 
            }
        }

        /// <summary>
        /// Used to pad a byte array with zeros so that it can be 64bit encoded.
        /// </summary>
        /// <param name="Bytes"></param>
        private void PadBytes(byte[] Bytes)
        {
            byte[] ByteCopy;
            byte[] temp;
            while(Bytes.Length % 64 != 0)
            {
                ByteCopy = new byte[Bytes.Length + 1];
                Bytes.CopyTo(ByteCopy, 0);
                ByteCopy[Bytes.Length] = 0x0;
                temp = ByteCopy;
                ByteCopy = Bytes;
                Bytes = temp;
            }
        }
        /// <summary>
        /// System required method for drag and drop functionality.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResourcePanel_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Copy;
        }

        /// <summary>
        /// Calls ResourcePanel_DragDrop when a drag drop event has occured on the resourcePanel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilerView2_DragDrop(object sender, DragEventArgs e)
        {
            ResourcePanel_DragDrop(sender, e);
        }
        
        protected override CreateParams CreateParams //https://stackoverflow.com/questions/14281085/moving-overlapping-pictureboxes-at-runtime-causes-lag-in-repaint
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        /// <summary>
        /// This function takes care of transitioning from The Resources View to the Classes View when the top left Arrow is clicked.
        /// It also clears out the CurrentResources.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ArrowPanel_Click(object sender, EventArgs e)
        {
            if (ClassView)
            {
                return;
            }
            else
            {
                ClassView = true;
                CurrentResources.Clear(); //Throw away the resources because the class is changing.
                ClassesPanel.Location = new Point(136, 150);
                ClassesPanel.BringToFront();
                ClassesPanel.Show();
                ResourcePanel.Hide();
            }
        }

        /// <summary>
        /// This function is called whenever the delete button is clicked on a Resource Box. The parameter is the argument
        /// is the name of the resource the box represents. Sends DeleteEvent out.
        /// </summary>
        /// <param name="Name"></param>

        private void Received_Delete(string Name)
        {
            //TODO make a textbox that asks if the user is sure?
            //Find the ResourceData with that name.
            ResourceData ToDelete = null;
            foreach(ResourceData d in CurrentResources)
            {
                if (d.Name.Equals(Name))
                {
                    ToDelete = d;
                    break;
                }
            }
            //Emit delete signal for controller to catch.
            string IsLink;
            if(ToDelete.Link == null || ToDelete.Link.Equals(""))
            {
                IsLink = "false";
            }
            else
            {
                IsLink = "true";
            }
            DeleteEvent?.Invoke(Name, IsLink);
            //Here we will assume that the delete worked and remove it from the interface.
            CurrentResources.Remove(ToDelete);
            UpdateResources(new List<ResourceData>());
        }

        private void Received_Double_Click(string Name)
        {
            Console.WriteLine("Received double click in the view: " + Name);
            GetContentsEvent?.Invoke(Name);
        }

        private void Received_Right_Click(string Name, Point MouseLocation) //MouseLocation is with respect to the ResourcePanel
        {
            ContextMenu ResourceContextMenu = GetContextMenu(Name);
            ResourceContextMenu.Show(ResourcePanel, MouseLocation);
        }

        private ContextMenu GetContextMenu(string Name)
        {
            ContextMenu Menu = new ContextMenu();
            Menu.Name = Name;
            MenuItem UnitSubMenu = GetUnitSubMenu();
            MenuItem TypeSubMenu = GetTypeSubMenu();
            MenuItem ClassSubMenu = GetClassSubMenu();
            UnitSubMenu.Name = Name;
            TypeSubMenu.Name = Name;
            ClassSubMenu.Name = Name;
            MenuItem RenameItem = new MenuItem("Rename...");
            RenameItem.Click += MainMenuItemClick;
            Menu.MenuItems.Add(UnitSubMenu);
            Menu.MenuItems.Add(TypeSubMenu);
            Menu.MenuItems.Add(ClassSubMenu);
            Menu.MenuItems.Add(RenameItem);
            return Menu;
        }

        private MenuItem GetClassSubMenu()
        {
            MenuItem ClassMenu = new MenuItem("Move to...");
            foreach(string S in AllClasses)
            {
                MenuItem Current = new MenuItem(S);
                Current.Click += ClassMenuItemClick;
                ClassMenu.MenuItems.Add(Current);
            }
            return ClassMenu;
        }

        private MenuItem GetUnitSubMenu()
        {
            MenuItem UnitsMenu = new MenuItem("Unit");
            ISet<string> UnitsSet = new HashSet<string>();
            foreach(ResourceData R in CurrentResources)
            {
                UnitsSet.Add(R.Unit);
            }
            UnitsSet.Remove(null);
            foreach(string S in UnitsSet)
            {
                MenuItem Current = new MenuItem(S);
                Current.Click += UnitMenuItemClick;
                UnitsMenu.MenuItems.Add(Current);
            }
            MenuItem AddUnit = new MenuItem("New Unit...");
            AddUnit.Click += UnitMenuItemClick;
            UnitsMenu.MenuItems.Add(AddUnit);

            return UnitsMenu;
        }

        private MenuItem GetTypeSubMenu()
        {
            MenuItem TypesMenu = new MenuItem("Type");
            ISet<string> TypesSet = new HashSet<string>();
            foreach (ResourceData R in CurrentResources)
            {
                TypesSet.Add(R.Type);
            }
            TypesSet.Remove(null);
            foreach (string S in TypesSet)
            {
                MenuItem Current = new MenuItem(S);
                Current.Click += TypeMenuItemClick;
                TypesMenu.MenuItems.Add(Current);
            }
            MenuItem AddType = new MenuItem("New Type...");
            AddType.Click += TypeMenuItemClick;
            TypesMenu.MenuItems.Add(AddType);
            return TypesMenu;
        }

        private void UnitMenuItemClick(object sender, EventArgs e) //refering to the right click context menu for resources.
        {
            MenuItem CurrentMenu = (MenuItem)sender;
            string Name = CurrentMenu.Parent.Name;
            
            ResourceData CurrentResource = null;
            foreach(ResourceData R in CurrentResources)
            {
                if (R.Name.Equals(Name))
                {
                    CurrentResource = R;
                    break;
                }
            }
            if (CurrentMenu.Text.Equals("New Unit..."))
            {
                //Perform add unit.
                MultipurposePopup UnitSelector = new MultipurposePopup("", "Unit:", true);
                var Result = UnitSelector.ShowDialog();
                if (Result == DialogResult.OK)
                {
                    TempOldResource = CurrentResource;
                    TempUpdatedResource = CurrentResource.Clone();
                    TempUpdatedResource.Unit = UnitSelector.MyText;
                    CurrentResources.Remove(CurrentResource);
                    UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), null, null, null, UnitSelector.MyText, CurrentResource.Type, CurrentResource.Comments);
                    return;
                }
                else
                {
                    return;
                }
            }
            TempOldResource = CurrentResource;
            TempUpdatedResource = CurrentResource.Clone();
            TempUpdatedResource.Unit = CurrentMenu.Text;
            CurrentResources.Remove(CurrentResource);
            UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), null, null, null, CurrentMenu.Text, CurrentResource.Type, CurrentResource.Comments);
        }

        private void TypeMenuItemClick(object sender, EventArgs e) //refering to the right click context menu for resources.
        {
            MenuItem CurrentMenu = (MenuItem)sender;
            string Name = CurrentMenu.Parent.Name;
            ResourceData CurrentResource = null;
            foreach (ResourceData R in CurrentResources)
            {
                if (R.Name.Equals(Name))
                {
                    CurrentResource = R;
                    break;
                }
            }
            if (CurrentMenu.Text.Equals("New Type..."))
            {
                //Perform add type.
                MultipurposePopup TypeSelector = new MultipurposePopup("", "Type:", true);
                var Result = TypeSelector.ShowDialog();
                if (Result == DialogResult.OK)
                {
                    TempOldResource = CurrentResource;
                    TempUpdatedResource = CurrentResource.Clone();
                    TempUpdatedResource.Type = TypeSelector.MyText;
                    CurrentResources.Remove(CurrentResource);
                    UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), null, null, null, CurrentResource.Unit, TypeSelector.MyText, CurrentResource.Comments);
                    return;
                }
                else
                {
                    return;
                }
            }
            TempOldResource = CurrentResource;
            TempUpdatedResource = CurrentResource.Clone();
            TempUpdatedResource.Type = CurrentMenu.Text;
            CurrentResources.Remove(CurrentResource);
            UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), null, null, null, CurrentResource.Unit, CurrentMenu.Text, CurrentResource.Comments);
        }

        private void ClassMenuItemClick(object sender, EventArgs e)
        {
            MenuItem CurrentMenu = (MenuItem)sender;
            string Name = CurrentMenu.Parent.Name;
            ResourceData CurrentResource = null;
            foreach (ResourceData R in CurrentResources)
            {
                if (R.Name.Equals(Name))
                {
                    CurrentResource = R;
                    break;
                }
            }
            TempOldResource = CurrentResource;
            TempUpdatedResource = null;
            CurrentResources.Remove(CurrentResource);
            UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), CurrentMenu.Text, null, null, CurrentResource.Unit, CurrentResource.Type, CurrentResource.Comments);
        }

        private void MainMenuItemClick(object sender, EventArgs e) //refering to the right click context menu for resources.
        {
            MenuItem CurrentMenu = (MenuItem)sender;
            string Name = CurrentMenu.Parent.Name;
            ResourceData CurrentResource = null;
            foreach (ResourceData R in CurrentResources)
            {
                if (R.Name.Equals(Name))
                {
                    CurrentResource = R;
                    break;
                }
            }
            if (CurrentMenu.Text.Equals("Rename..."))
            {
                TextBox NameTextBox = new TextBox();
                NameTextBox.Text = Name;
                //NameTextBox.Size = new Size(NameTextBox.Height, NameTextBox.Size.Width * 10);
                TableLayoutPanel PanelClickedOn = null;
                Label NameLabel = null;

                foreach(Control C in ResourcesLeftPanel.Controls)
                {
                    TableLayoutPanel Current = (TableLayoutPanel)C;
                    NameLabel = (Label)Current.GetChildAtPoint(new Point(50, 50), GetChildAtPointSkip.None);
                    if (NameLabel.Text.Equals(Name))
                    {
                        PanelClickedOn = Current;
                        break;

                    }
                }
                PanelClickedOn.Controls.Remove(NameLabel);
                PanelClickedOn.Controls.Add(NameTextBox, 0, 1);
                PanelClickedOn.SetColumnSpan(NameTextBox, 5);
                NameTextBox.TextAlign = HorizontalAlignment.Center;
                NameTextBox.SelectAll();
                NameTextBox.Name = Name;
                NameTextBox.KeyDown += NameTextBoxKeyDown;

                //UpdateResourceEvent?.Invoke(CurrentResource.Name, GetIsLinkFromR(CurrentResource), null, UpdatedName, null, CurrentResource.Unit, CurrentResource.Type, CurrentResource.Comments);
            }
        }

        private void NameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                TextBox CurrentTextBox = (TextBox)sender;
                string UpdatedName = CurrentTextBox.Text;
                string OldName = CurrentTextBox.Name;
                ResourceData CurrentResource = GetResourceFromName(OldName);

                TempOldResource = CurrentResource;
                TempUpdatedResource = CurrentResource.Clone();
                TempUpdatedResource.Name = UpdatedName;
                CurrentResources.Remove(CurrentResource);

                UpdateResourceEvent?.Invoke(OldName, GetIsLinkFromR(CurrentResource), null, UpdatedName, null, CurrentResource.Unit, CurrentResource.Type, CurrentResource.Comments);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private ResourceData GetResourceFromName(string Name)
        {
            foreach (ResourceData R in CurrentResources)
            {
                if (R.Name.Equals(Name))
                {
                    return R;
                }
            }
            return null;
        }

        private string GetIsLinkFromR(ResourceData Current)
        {
            if(Current.Link == null || Current.Link.Equals(""))
            {
                return "false";
            }
            else
            {
                return "true";
            }
        }
    }



    /// <summary>
    /// Used to compare items the ResourceData List. It is designed to put later dates first.
    /// </summary>
    internal class DateComparator : IComparer<ResourceData>
    {
        public int Compare(ResourceData x, ResourceData y)
        {
            return DateTime.Compare(DateTime.Parse(x.Date), DateTime.Parse(y.Date)) * -1; //Flip sign so last uploaded is shown first.
        }
    }

    public static class ControlHelper
    {
        #region Redraw Suspend/Resume
        [DllImport("user32.dll", EntryPoint = "SendMessageA", ExactSpelling = true, CharSet = CharSet.Ansi, SetLastError = true)]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
        private const int WM_SETREDRAW = 0xB;

        public static void SuspendDrawing(this Control target)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 0, 0);
        }

        public static void ResumeDrawing(this Control target) { ResumeDrawing(target, true); }
        public static void ResumeDrawing(this Control target, bool redraw)
        {
            SendMessage(target.Handle, WM_SETREDRAW, 1, 0);

            if (redraw)
            {
                target.Refresh();
            }
        }
        #endregion
    }
}
