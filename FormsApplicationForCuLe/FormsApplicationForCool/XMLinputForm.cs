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
    public partial class XMLinputForm : Form
    {

        private String m_XMLString;

        public string XMLstring
        {
            get
            {
                return m_XMLString;
            }
        }

        
        public XMLinputForm()
        {
            InitializeComponent();
        }

        private void XMLinputForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            m_XMLString = string.Empty;

            foreach (string l_string in this.richTextBox1.Lines)
            {

                m_XMLString = m_XMLString + l_string;

            }

            if (!(m_XMLString == string.Empty))
            {

                this.DialogResult = DialogResult.OK;

                this.Close();
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
