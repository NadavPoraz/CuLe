using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FormsApplicationForCuLe
{
    public partial class AlloyForm : Form
    {
        public AlloyForm(List<String> i_ListOfStrings)
        {
            InitializeComponent();

            string g_string = "";

            if( i_ListOfStrings != null)
            {
                foreach( string l_string in i_ListOfStrings)
                {
                    g_string = g_string + l_string + System.Environment.NewLine;
                }
            }

            this.AlloyTextBox.Text = g_string;
            
        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void AlloyTextBox_Load(object sender, EventArgs e)
        {

        }


    }
}
