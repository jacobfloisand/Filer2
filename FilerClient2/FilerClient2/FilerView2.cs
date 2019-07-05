using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FilerClient2
{
    public partial class FilerView2 : Form
    {
        public FilerView2()
        {
            InitializeComponent();
            ClassImage testImage = new ClassImage("World European Civilizations of the West", ResourceColor.Orange);
            ClassImage testImage2 = new ClassImage("World European Civilizations of the West", ResourceColor.Blue_Green);
            testImage.AddTo(ClassesPanel);
            testImage2.AddTo(ClassesPanel);
        }
    }
}
