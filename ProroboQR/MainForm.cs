using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProroboQR
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void programTextBox_TextChanged(object sender, EventArgs e)
        {
            //もし、文字末尾が\nだったら、反映する。
            if (programTextBox.Text.Length > 0)
            {
                if (programTextBox.Text.Substring(programTextBox.Text.Length - 1,1) == "\n")
                {
                    var commmandsList = new List<string>();
                    commmandsList.AddRange(programTextBox.Text.Split('\n'));

                }
            }
            else
            {
                //初期化処理
            }
        }

        private void Initial()
        {
            programTextBox.Text = "M = " + moveTimeNumericUpDown.Value + "\nT = " + twistTimeNumericUpDown.Value + "\n";
            programTextBox.Text += "START_UNSAFE\n";
        }

        private void ChangeCards(List<string> commandsList)
        {
            var i = 0;
            foreach(string s in commandsList)
            {
                if (s == "START_UNSAFE")
                {
                   
                }
            }
        }
    }
}
