using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CuLe;
using Irony.Parsing;
using FormsApplicationForCuLe.Properties;
using Irony.GrammarExplorer.Highlighter;
using System.Data.OleDb;
using ADOX;

namespace FormsApplicationForCuLe
{

    public partial class Form1 : Form
    {

        //fields
        public CuLe.CuLe m_CuLe;

        bool _loaded;
        bool _treeClickDisabled; //to temporarily disable tree click when we locate the node programmatically

        FastColoredTextBoxHighlighter _highlighter;

        public Form1()
        {
            InitializeComponent();
            m_CuLe = new CuLe.CuLe();

            InputTextBox.Text = Settings.Default.InputText;

            this.StartHighlighter();
        }

        private void button2_Click(object sender, EventArgs e)
        {

            Settings.Default.InputText = InputTextBox.Text;

            ParseSample();
        }

        private void ParseSample()
        {
            ClearParserOutput();
            if (!m_CuLe.CanParse()) return;
            GC.Collect(); //to avoid disruption of perf times with occasional collections

            try
            {
                m_CuLe.Parse(InputTextBox.Text);
            }
            catch (Exception ex)
            {
                gridCompileErrors.Rows.Add(null, ex.Message, null);
                throw;
            }
            finally
            {

                ShowCompilerErrors();
                //  if (chkParserTrace.Checked)
                //  {
                //      ShowParseTrace();
                //   }
                //  ShowCompileStats();
                  ShowParseTree();
                //  ShowAstTree();
            }
        }

        private void ClearParserOutput()
        {
            //lblSrcLineCount.Text = string.Empty;
            //lblSrcTokenCount.Text = "";
            //lblParseTime.Text = "";
            //lblParseErrorCount.Text = "";

            //lstTokens.Items.Clear();
            //gridCompileErrors.Rows.Clear();
            //gridParserTrace.Rows.Clear();
            //lstTokens.Items.Clear();
            //tvParseTree.Nodes.Clear();
            //tvAst.Nodes.Clear();
            //Application.DoEvents();
        }

        private void InputTextBox_TextChanged(object sender, EventArgs e)
        {

            Settings.Default.InputText = InputTextBox.Text;

        }

        private void InputTextBox_SelectionChanged(object sender, EventArgs e)
        {

            int l_line = InputTextBox.Selection.Start.iLine + 1;

            int l_col = InputTextBox.Selection.Start.iChar + 1;
           
            label1.Text = l_line.ToString() + ", " + l_col.ToString();
        }

        private void ShowCompilerErrors()
        {
            gridCompileErrors.Rows.Clear();
            if (m_CuLe.m_ParseTree == null || m_CuLe.m_ParseTree.ParserMessages.Count == 0) return;
            foreach (var err in m_CuLe.m_ParseTree.ParserMessages)
                gridCompileErrors.Rows.Add(err.Location, err, err.ParserState);
        }


        private void ShowParseTree()
        {
            tvParseTree.Nodes.Clear();
            if (m_CuLe.m_ParseTree == null) return;
            AddParseNodeRec(null, m_CuLe.m_ParseTree.Root);
        }

      private void AddParseNodeRec(TreeNode parent, ParseTreeNode node)
      {
          if (node == null) return;
          string txt = node.ToString();
          TreeNode tvNode = (parent == null ? tvParseTree.Nodes.Add(txt) : parent.Nodes.Add(txt));
          tvNode.Tag = node;
          foreach (var child in node.ChildNodes)
              AddParseNodeRec(tvNode, child);
      }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_Close(object sender, FormClosingEventArgs e)
        {
            Settings.Default.Save();
        }

        private void InputTextBox_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void InputTextBox_SelectionChanged_1(object sender, EventArgs e)
        {

            int i = 7;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (m_CuLe.m_ParseTree == null || m_CuLe.m_ParseTree.Root == null)
            {
                return;
            }

            m_CuLe.CreateDataSet();

            if (m_CuLe.m_ErrorMessages != null)
            {

                if (m_CuLe.m_ErrorMessages.Count != 0)
                {

                    foreach (ErrorMessage l_ErrorMessage in m_CuLe.m_ErrorMessages)
                    {

                        try
                        {
                            throw new System.ArgumentException(l_ErrorMessage.SourceLocation().ToString() + " | " + l_ErrorMessage.MessageText());
                        }

                        catch (Exception ex)
                        {
                            gridCompileErrors.Rows.Add(null, ex.Message, null);

                        }
                    }


                }
            }



            //                try
            //    {
            //        m_CuLe.Parse(InputTextBox.Text);
            //    }
            //    catch (Exception ex)
            //    {
            //        gridCompileErrors.Rows.Add(null, ex.Message, null);
            //        throw;
            //    }
            //    finally
            //    {

            //        ShowCompilerErrors();
            //}


        }


        private void InputTextBox_Load(object sender, EventArgs e)
        {

           InputTextBox.Text = Settings.Default.InputText ;

        }


        private void StartHighlighter()
        {

            _highlighter = new FastColoredTextBoxHighlighter(InputTextBox, m_CuLe.GetLanguage());
            _highlighter.Adapter.Activate();
        }
        private void StopHighlighter()
        {
            if (_highlighter == null) return;
            _highlighter.Dispose();
            _highlighter = null;
            ClearHighlighting();
        }
        private void ClearHighlighting()
        {
            var selectedRange = InputTextBox.Selection;
            var visibleRange = InputTextBox.VisibleRange;
            var firstVisibleLine = Math.Min(visibleRange.Start.iLine, visibleRange.End.iLine);

            var txt = InputTextBox.Text;
            InputTextBox.Clear();
            InputTextBox.Text = txt; //remove all old highlighting

            InputTextBox.SetVisibleState(firstVisibleLine, FastColoredTextBoxNS.VisibleState.Visible);
            InputTextBox.Selection = selectedRange;
        }
        private void EnableHighlighter(bool enable)
        {
            if (_highlighter != null)
                StopHighlighter();
            if (enable)
                StartHighlighter();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "TXT File|*.CuLe";
            saveFileDialog1.Title = "Save CuLe File";
            saveFileDialog1.ShowDialog();

            if (saveFileDialog1.FileName != "")
            {
                System.IO.File.WriteAllText(saveFileDialog1.FileName, InputTextBox.Text);

            }


        }

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Settings.Default.InputText = InputTextBox.Text;

            ParseSample();
        }

        private void button6_Click(object sender, EventArgs e)
        {

            if (InputTextBox.Text != "")
            {
                
            }
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.StreamReader sr = new
                   System.IO.StreamReader(openFileDialog1.FileName);

                InputTextBox.Text = (sr.ReadToEnd());
                //MessageBox.Show(sr.ReadToEnd());
                sr.Close();
            }
        }

        private void saveFile(bool i_flag_no_popup)
        {

            bool lv_break_flag = false;

            if( i_flag_no_popup == false)
            {

   

            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (m_CuLe.m_DataSet != null)
            {

                List<String> l_ListOfStrings = m_CuLe.m_DataSet.WriteSchemaToAlloy();

                AlloyForm AlloyForm1 = new AlloyForm(l_ListOfStrings);

                AlloyForm1.Show();


            }
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            XMLinputForm XMLinputForm1 = new XMLinputForm();

            XMLinputForm1.ShowDialog();

            if( XMLinputForm1.DialogResult == DialogResult.OK )
            {

                List<DataSet> l_DataSets = m_CuLe.m_DataSet.GenerateInstanceFromAlloyXML(XMLinputForm1.XMLstring);

                folderBrowserDialog1.ShowDialog();

                //saveFileDialog1.Filter = "MDB File|*.mdb";
                //saveFileDialog1.Title = "Save MDB File";

                
                //saveFileDialog1.ShowDialog();

                if (folderBrowserDialog1.SelectedPath != "")
                {
                    foreach (DataSet l_dataSet in l_DataSets)
                    {
                        string lv_FileName = System.IO.Path.Combine(folderBrowserDialog1.SelectedPath, l_dataSet.DataSetName + ".mdb");

                        Exportmdb(lv_FileName, l_dataSet);
                    }
                }              
                

            }
        }

        private void Exportmdb(string strDirectory, System.Data.DataSet dtt)
        {
            try
            {

                Catalog cat = new Catalog();
                //UpdateProgress("");
                string str = "provider=Microsoft.Jet.OleDb.4.0;Data Source=" + strDirectory;
                cat.Create(str);
                cat = null;
                OleDbConnection sceConnection = new OleDbConnection(@"Provider=Microsoft.Jet.OLEDB.4.0; Data Source=" + strDirectory);
                sceConnection.Open();
                foreach (System.Data.DataTable dttemp in dtt.Tables)
                {
                    string tableName = dttemp.TableName;

                    StringBuilder stbSqlGetHeaders = new StringBuilder();
                    stbSqlGetHeaders.Append("create table " + tableName + " (");
                    int z = 0;
                    StringBuilder stbSqlQuery = new StringBuilder();
                    StringBuilder stbFields = new StringBuilder();
                    StringBuilder stbParameters = new StringBuilder();


                    foreach (DataColumn col in dttemp.Columns)
                    {
                        string datatyp = col.DataType.Name.ToString().Trim().ToLower();
                        if (z != 0) stbSqlGetHeaders.Append(", "); ;
                        String strName = col.ColumnName;
                        String strType = col.DataType.Name.ToString().Trim().ToLower();
                        if (strType.Equals("")) throw new ArgumentException("DataType Empty");
                        if (strType.Equals("int32")) strType = "Number";
                        if (strType.Equals("int64")) strType = "Number";
                        if (strType.Equals("int16")) strType = "Number";
                        if (strType.Equals("float")) strType = "float";
                        if (strType.Equals("double")) strType = "Double";
                        if (strType.Equals("decimal")) strType = "Double";
                        if (strType.Equals("string")) strType = "memo";
                        if (strType.Equals("boolean")) strType = "Bit";
                        if (strType.Equals("datetime")) strType = "datetime";
                        if (strType.Equals("byte[]")) strType = "Image";

                        stbSqlGetHeaders.Append("[" + strName + "] " + strType);
                        z++;

                        stbFields.Append("[" + col.ColumnName + "]");

                        stbParameters.Append("@" + col.ColumnName.ToLower());

                        if (col.ColumnName != dttemp.Columns[dttemp.Columns.Count - 1].ColumnName)
                        {
                            stbFields.Append(", ");
                            stbParameters.Append(", ");
                        }

                    }



                    foreach (Constraint l_Constraint in dttemp.Constraints)
                    {
                        if (l_Constraint is UniqueConstraint)
                        {
                            UniqueConstraint l_UniqueConstraint = (UniqueConstraint)l_Constraint;

                            string lv_constraint = null;

                            if (l_UniqueConstraint.IsPrimaryKey)
                            {
                                lv_constraint = " PRIMARY KEY ";
                            }
                            else
                            {
                                lv_constraint = " UNIQUE ";
                            }

                            string lv_columns_string = null;

                            foreach (DataColumn l_Column in l_UniqueConstraint.Columns)
                            {
                                if (lv_columns_string == null)
                                {
                                    lv_columns_string = l_Column.ColumnName;
                                }
                                else
                                {
                                    lv_columns_string = lv_columns_string + " , " + l_Column.ColumnName;
                                }
                            }

                            if (lv_columns_string != null)
                            {
                                stbSqlGetHeaders.Append(" , CONSTRAINT " + l_UniqueConstraint.ConstraintName + lv_constraint + " ( " + lv_columns_string + " )");
                            }
                        }


                    }

                    stbSqlGetHeaders.Append(")");
                    
                    OleDbCommand sceCreateTableCommand;
                    string strCreateTableQuery = stbSqlGetHeaders.ToString();


                    sceCreateTableCommand = new OleDbCommand(strCreateTableQuery, sceConnection);

                    sceCreateTableCommand.ExecuteNonQuery();

                   

                    stbSqlQuery.Append("insert into " + tableName + " (");
                    OleDbCommand comm = new OleDbCommand();

                    stbSqlQuery.Append(stbFields.ToString() + ") ");
                    stbSqlQuery.Append("values (");
                    stbSqlQuery.Append(stbParameters.ToString() + ") ");

                    string strTotalRows = dttemp.Rows.Count.ToString();

                    foreach (DataRow row in dttemp.Rows)
                    {
                        OleDbCommand sceInsertCommand = new OleDbCommand(stbSqlQuery.ToString(), sceConnection);
                        foreach (DataColumn col in dttemp.Columns)
                        {
                            string colnameparam = col.ColumnName;
                            string colparam = col.ColumnName.ToLower();
                            string datatyp1 = col.DataType.Name.ToString().Trim().ToLower();
                            if (datatyp1.Substring(0, 3) == "str")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.LongVarWChar).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.LongVarWChar).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "dat")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Date).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Date).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "byt")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.LongVarBinary).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.LongVarBinary).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "int")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.BigInt).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.BigInt).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "boo")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Boolean).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Boolean).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "flo")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Double).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Double).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "dou")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Double).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Double).Value = DBNull.Value;
                                }
                            }
                            else if (datatyp1.Substring(0, 3) == "dec")
                            {
                                if (row[colnameparam].ToString() != "")
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Decimal).Value = row[colnameparam];
                                }
                                else
                                {
                                    sceInsertCommand.Parameters.Add("@" + colparam.Trim(), OleDbType.Decimal).Value = DBNull.Value;
                                }
                            }
                        }
                        sceInsertCommand.ExecuteNonQuery();
                    }
                }

                foreach (DataRelation l_DataRelation in dtt.Relations)
                {

                    string lv_ChildColumns_string = null;

                    foreach (DataColumn l_Column in l_DataRelation.ChildColumns)
                    {
                        if (lv_ChildColumns_string == null)
                        {
                            lv_ChildColumns_string = l_Column.ColumnName;
                        }
                        else
                        {
                            lv_ChildColumns_string = lv_ChildColumns_string + " , " + l_Column.ColumnName;
                        }
                    }

                    string lv_ParentColumns_string = null;

                    foreach (DataColumn l_Column in l_DataRelation.ParentColumns)
                    {
                        if (lv_ParentColumns_string == null)
                        {
                            lv_ParentColumns_string = l_Column.ColumnName;
                        }
                        else
                        {
                            lv_ParentColumns_string = lv_ParentColumns_string + " , " + l_Column.ColumnName;
                        }
                    }



                    if (lv_ChildColumns_string != null && lv_ParentColumns_string != null)
                    {
                        string strAlterTableQuery = " ALTER TABLE " + l_DataRelation.ChildTable.TableName + " ADD CONSTRAINT " + l_DataRelation.RelationName + " FOREIGN KEY ( " + lv_ChildColumns_string + " ) REFERENCES " + l_DataRelation.ParentTable.TableName + " ( " + lv_ParentColumns_string + " ) ";

                        OleDbCommand sceAlterTableCommand = new OleDbCommand(strAlterTableQuery, sceConnection);

                        sceAlterTableCommand.ExecuteNonQuery();
                    }



                }

                sceConnection.Close();
            }

              

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Export Data", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1);
            }

            
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {

            if (m_CuLe != null)
            {
                if (m_CuLe.m_ParseTree != null)
                {
                    ParseTreeForm ParseTreeForm1 = new ParseTreeForm(m_CuLe.m_ParseTree);
                    ParseTreeForm1.ShowDialog();
                }
            }
        }

        private void tvParseTree_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void InputTextBox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {

        }

      

        }
    }




/*

create table Nadav1
(

column1 int,
column2 int
)
constraint pk1 PRIMARY KEY (column1) 
constraint nn1 NOT NULL (column1)
constraint fk1 FOREIGN KEY (column2) REFERENCES nadav2(column2)



create table "nadav2"
(
   column1 int,
   column2 int
   )
  
 */ 
