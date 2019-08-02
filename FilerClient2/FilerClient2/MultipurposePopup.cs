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
    public partial class MultipurposePopup : Form
    {

        bool IsInput = true;

        public MultipurposePopup()
        {
            InitializeComponent();
        }


        public string MyText = null;

        public MultipurposePopup(string MainText, string DesiredInput, bool Input)
        {
            InitializeComponent();
            IsInput = Input;
            if (Input)
            {
                MainLabel.Text = MainText;
                TextBoxDescriptor.Text = DesiredInput;
            }
            else
            {
                TextBox1.Hide();
                TextBoxDescriptor.Hide();
                MainLabel.Text = MainText;
            }
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Equals("") && IsInput)
            {
                this.DialogResult = DialogResult.Cancel;
            }
            else
            {
                MyText = TextBox1.Text;
                this.DialogResult = DialogResult.OK;
            }

        }
    }
}
