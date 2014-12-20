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
    public partial class ParseTreeForm : Form
    {
        private Irony.Parsing.ParseTree m_ParseTree;

        public ParseTreeForm(Irony.Parsing.ParseTree i_ParseTree)
        {
            InitializeComponent();

            m_ParseTree = i_ParseTree;

            if( m_ParseTree != null)
            {
                ShowParseTree();
            }

            //ParseTreeView.Nodes.Clear();
            //if (i_CuLe == null) return;
            //if (i_CuLe.m_ParseTree == null) return;

            //AddParseNodeRec(null, i_CuLe.m_ParseTree.Root);
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void treeView1_AfterSelect_1(object sender, TreeViewEventArgs e)
        {

        }

        private void ShowParseTree()
        {
            ParseTreeView.Nodes.Clear();
            if (m_ParseTree == null) return;
            AddParseNodeRec(null, m_ParseTree.Root);
        }

        private void AddParseNodeRec(TreeNode parent, Irony.Parsing.ParseTreeNode node)
        {
            if (node == null) return;
            string txt = node.ToString();
            TreeNode tvNode = (parent == null ? ParseTreeView.Nodes.Add(txt) : parent.Nodes.Add(txt));
            tvNode.Tag = node;
            foreach (var child in node.ChildNodes)
                AddParseNodeRec(tvNode, child);
        }

    }
}
