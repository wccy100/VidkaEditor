using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vidka.Components
{
    public partial class DialogInput : Form
    {
        public DialogInput()
        {
            InitializeComponent();
        }

        private void DialogInput_Load(object sender, EventArgs e)
        {

        }

        public string Answer {
            get { return txtInput.Text; }
            set { txtInput.Text = value; }
        }

        private static DialogInput singleton { get; set; }
        public static DialogResult ShowInputDialog(string title, string question, string value, out string newValue)
        {
            if (singleton == null)
                singleton = new DialogInput();
            singleton.Text = title;
            singleton.lblQuestion.Text = question;
            singleton.txtInput.Text = value;
            newValue = value;
            var result = singleton.ShowDialog();
            if (result == DialogResult.OK)
                newValue = singleton.txtInput.Text;
            return result;
        }

    }
}
