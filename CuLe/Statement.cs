using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{

    class Statement
    {
        static public System.Type GetTypeFromStatement(string i_type)
        {

            if (i_type == null)
            {
                return null;
            }

            string l_typeCap = i_type.ToUpper();

            switch (l_typeCap)
            {

                case "BIT":

                    return System.Type.GetType("System.Boolean");

                case "DATE":

                    return System.Type.GetType("System.DateTime");

                case "TIME":

                    return System.Type.GetType("System.DateTime");

                case "TIMESTAMP":

                    return System.Type.GetType("System.Byte");

                case "DECIMAL":

                    return System.Type.GetType("System.Decimal");

                case "REAL":

                    return System.Type.GetType("System.Single");

                case "FLOAT":

                    return System.Type.GetType("System.Double");

                case "SMALLINT":

                    return System.Type.GetType("System.Int16");


                case "INTEGER":

                    return System.Type.GetType("System.Int32");


                case "CHARACTER":

                    return System.Type.GetType("System.String");

                case "DATETIME":

                    return System.Type.GetType("System.DateTime");

                case "INT":

                    return System.Type.GetType("System.Int32");

                case "DOUBLE":

                    return System.Type.GetType("System.Double");

                case "CHAR":

                    return System.Type.GetType("System.String");

                case "NCHAR":

                    return System.Type.GetType("System.String");

                case "VARCHAR":

                    return System.Type.GetType("System.String");

                case "NVARCHAR":

                    return System.Type.GetType("System.String");

                case "IMAGE":

                    return System.Type.GetType("System.Byte");

                case "TEXT":

                    return System.Type.GetType("System.String");

                case "NTEXT":

                    return System.Type.GetType("System.String");

            }

            return null;

        }       
    }

    class StatementList : List<Statement>
    {

        public List<ErrorMessage> CreateCuLeDataSet(out CuLeDataSet o_DataSet)
        {

            /* 
             * list of validations
             * first iteration - tables & columns
             *  unique Table Names for schema
             *  unique Table Column Names for Table
             *  unique Constraint Names for all constraints
             *  primary key - Columns exist, columns unique
             *  unique key - columns exist, columns unique
             *  not null key - columns exist, column unique
             *  
             * second iteration - table relationships
             *  foreign key - source columns exist, source columns unique, target table exists, 
             *  target columns exist, target columns unique, target columns of same Type as source columns,
 
            */

            CuLeDataSet l_DataSet = new CuLeDataSet();

            List<ConstraintDef> l_AllConstraints = new List<ConstraintDef>();
            List<String> l_AllConstraintNames = new List<String>();

            List<ErrorMessage> ErrorMessages = new List<ErrorMessage>();

            l_AllConstraints.Clear();

            //  first iteration

            foreach (CreateTableStatement l_createTableStatement in this)
            {
                if (l_DataSet.Tables.Contains(l_createTableStatement.m_id_simple.Token.Text))
                {

                    ErrorMessages.Add(new ErrorMessage("Table ID " + l_createTableStatement.m_id_simple.Token.Text + " already Exists", l_createTableStatement.m_id_simple.Token.Location));
                    continue;
                }

                DataTable dataTable = new DataTable(l_createTableStatement.m_id_simple.Token.Text);



                foreach (TableFieldDef tableFieldDef in l_createTableStatement.m_FieldDefList)
                {

                    // unique Table Column Names for Table
                    if (dataTable.Columns.Contains(tableFieldDef.m_id_simple.Token.Text))
                    {
                        ErrorMessages.Add(new ErrorMessage("Column ID " + tableFieldDef.m_id_simple.Token.Text + " already Exists", tableFieldDef.m_id_simple.Token.Location));
                    }
                    else
                    {
                        DataColumn dataColumn = new DataColumn(tableFieldDef.m_id_simple.Token.Text);

                        dataColumn.DataType = Statement.GetTypeFromStatement(tableFieldDef.m_typeName.Token.Text);

                        //dataColumn.

                        
                        if (dataColumn.DataType == null)
                        {
                            ErrorMessages.Add(new ErrorMessage("Unknown Data Type " + tableFieldDef.m_typeName.Token.Text, tableFieldDef.m_typeName.Token.Location));
                        }
                        else
                        {
                            if (tableFieldDef.m_typeParams != null)
                            {
                                if (tableFieldDef.m_typeParams.Token.Value != null)
                                {


                                    int maxlength;

                                    if (System.Int32.TryParse(tableFieldDef.m_typeParams.Token.Value.ToString(), out maxlength))
                                    {
                                        dataColumn.MaxLength = maxlength;
                                    }
                                }
                            }

                            dataTable.Columns.Add(dataColumn);

                        }


                    }
                }


                foreach (ConstraintDef l_ConstraintDef in l_createTableStatement.m_constraintDefList)
                {


                    if (l_AllConstraints.Contains(l_ConstraintDef))
                    {
                        ErrorMessages.Add(new ErrorMessage("Constraint Name  " + l_ConstraintDef.m_id_simple.Token.Text + " already Defined", l_ConstraintDef.m_id_simple.Token.Location));
                        continue;
                    }

                    l_AllConstraints.Add(l_ConstraintDef);

                    if (l_ConstraintDef is PrimaryKeyConstraintDef)
                    {
                        PrimaryKeyConstraintDef l_PrimaryKeyConstraintDef = (PrimaryKeyConstraintDef)l_ConstraintDef;

                        if (dataTable.PrimaryKey != null && dataTable.PrimaryKey.Count() != 0)
                        {
                            ErrorMessages.Add(new ErrorMessage("Primary Key for Table " + dataTable.TableName + " already Defined", l_PrimaryKeyConstraintDef.m_id_simple.Token.Location));
                        }
                        else
                        {

                            int primaryKeyCount = l_PrimaryKeyConstraintDef.m_coloumns.Count();

                            DataColumn[] ColumnArray = new DataColumn[primaryKeyCount];


                            int primaryKeyIndex = 0;

                            foreach (ParseTreeNode parseTreeNodeColumn in l_PrimaryKeyConstraintDef.m_coloumns)
                            {

                                DataColumn l_dataColumn;

                                if (dataTable.Columns.Contains(parseTreeNodeColumn.Token.Text))
                                {
                                    l_dataColumn = dataTable.Columns[dataTable.Columns.IndexOf(parseTreeNodeColumn.Token.Text)];
                                }
                                else
                                {
                                    ErrorMessages.Add(new ErrorMessage("Primary Key error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                    continue;
                                }

                                //dataTable.PrimaryKey[primaryKeyIndex] = l_dataColumn;
                                ColumnArray[primaryKeyIndex] = l_dataColumn;
                                primaryKeyIndex++;

                            }

                            dataTable.PrimaryKey = ColumnArray;


                        }
                    }

                    else if (l_ConstraintDef is UniqueConstraintDef)
                    {
                        UniqueConstraintDef l_UniqueConstraintDef = (UniqueConstraintDef)l_ConstraintDef;

                        foreach (ParseTreeNode parseTreeNodeColumn in l_UniqueConstraintDef.m_coloumns)
                        {

                            int l_dataColumnIndex;

                            if (dataTable.Columns.Contains(parseTreeNodeColumn.Token.Text))
                            {
                                l_dataColumnIndex = dataTable.Columns.IndexOf(parseTreeNodeColumn.Token.Text);
                            }
                            else
                            {
                                ErrorMessages.Add(new ErrorMessage("Unique Constraint error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                continue;
                            }

                           // dataTable.Columns[l_dataColumnIndex].Unique = true;


                        }

                    }

                    if (l_ConstraintDef is NotNullConstraintDef)
                    {
                        NotNullConstraintDef l_NotNullConstraintDef = (NotNullConstraintDef)l_ConstraintDef;

                        foreach (ParseTreeNode parseTreeNodeColumn in l_NotNullConstraintDef.m_coloumns)
                        {

                            int l_dataColumnIndex;

                            if (dataTable.Columns.Contains(parseTreeNodeColumn.Token.Text))
                            {
                                l_dataColumnIndex = dataTable.Columns.IndexOf(parseTreeNodeColumn.Token.Text);
                            }
                            else
                            {
                                ErrorMessages.Add(new ErrorMessage("Not Null Constraint error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                continue;
                            }

                            dataTable.Columns[l_dataColumnIndex].AllowDBNull = false;

                        }

                    }
                }



                l_DataSet.Tables.Add(dataTable);

            }

            //end of first iteration

            /*
            // second iteration
            
             *  foreign key - source columns exist, source columns unique, target table exists, 
             *  target columns exist, target columns unique, target columns of same Type as source columns,
             *  constraint name unique
             *  */


            foreach (CreateTableStatement l_createTableStatement in this)
            {

                DataTable l_sourceTable;

                if (l_DataSet.Tables.Contains(l_createTableStatement.m_id_simple.Token.Text))
                {
                    l_sourceTable = l_DataSet.Tables[l_DataSet.Tables.IndexOf(l_createTableStatement.m_id_simple.Token.Text)];
                }
                else
                {
                    ErrorMessages.Add(new ErrorMessage("Source Table not recognized " + l_createTableStatement.m_id_simple.Token.Text, l_createTableStatement.m_id_simple.Token.Location));
                    continue;
                }


                foreach (ConstraintDef l_ConstraintDef in l_createTableStatement.m_constraintDefList)
                {

                    if (!(l_ConstraintDef is ForeignKeyConstraintDef))
                    {
                        continue;
                    }

                    ForeignKeyConstraintDef l_ForeignKeyConstraintDef = (ForeignKeyConstraintDef)l_ConstraintDef;

                    int l_numOfSourceColumns = l_ForeignKeyConstraintDef.m_sourceColoumns.Count;
                    int l_numOfTargetColumns = l_ForeignKeyConstraintDef.m_referencedColumns.Count;

                    DataTable l_TargetTable;



                    if (l_DataSet.Tables.Contains(l_ForeignKeyConstraintDef.m_referencedTable.Token.Text))
                    {
                        l_TargetTable = l_DataSet.Tables[l_DataSet.Tables.IndexOf(l_ForeignKeyConstraintDef.m_referencedTable.Token.Text)];
                    }
                    else
                    {
                        ErrorMessages.Add(new ErrorMessage("Foreign Key Constraint " + l_ForeignKeyConstraintDef.m_id_simple.Token.Text + " Referenced table " + l_ForeignKeyConstraintDef.m_referencedTable.Token.Text + " not recognized", l_ForeignKeyConstraintDef.m_referencedTable.Token.Location));
                        continue;

                    }

                    if (l_numOfSourceColumns != l_numOfTargetColumns)
                    {
                        ErrorMessages.Add(new ErrorMessage("Foreign Key Constraint " + l_ForeignKeyConstraintDef.m_id_simple.Token.Text + " number of source/Referenced columns mismatch ", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                        continue;
                    }

                    for (int i = 0; i < l_numOfSourceColumns; i++)
                    {
                        ParseTreeNode l_ParseNodeSourceColumn = l_ForeignKeyConstraintDef.m_sourceColoumns[i];
                        ParseTreeNode l_ParseNodeTargetColumn = l_ForeignKeyConstraintDef.m_referencedColumns[i];

                        DataColumn l_sourceColumn = null;
                        DataColumn l_TargetColoumn = null;


                        if (l_sourceTable.Columns.Contains(l_ParseNodeSourceColumn.Token.Text))
                        {
                            l_sourceColumn = l_sourceTable.Columns[l_sourceTable.Columns.IndexOf(l_ParseNodeSourceColumn.Token.Text)];
                        }
                        else
                        {
                            ErrorMessages.Add(new ErrorMessage("Source Column " + l_ParseNodeSourceColumn.Token.Text + " of table " + l_sourceTable.TableName + " not recognized ", l_ParseNodeSourceColumn.Token.Location));
                        }
                        if (l_TargetTable.Columns.Contains(l_ParseNodeTargetColumn.Token.Text))
                        {
                            l_TargetColoumn = l_TargetTable.Columns[l_TargetTable.Columns.IndexOf(l_ParseNodeTargetColumn.Token.Text)];
                        }
                        else
                        {
                            ErrorMessages.Add(new ErrorMessage("Refrenced Column " + l_ParseNodeTargetColumn.Token.Text + " of table " + l_TargetTable.TableName + " not recognized ", l_ParseNodeTargetColumn.Token.Location));
                        }

                        if (l_sourceColumn != null && l_TargetColoumn != null)
                        {
                            if (l_sourceColumn.DataType != l_TargetColoumn.DataType)
                            {
                                ErrorMessages.Add(new ErrorMessage("DataType Mismatch, Source Column " + l_sourceTable.TableName + "." + l_ParseNodeSourceColumn.Token.Text + " and Referenced Column " + l_TargetTable.TableName + "." + l_ParseNodeTargetColumn.Token.Text, l_ParseNodeTargetColumn.Token.Location));
                            }

                            else if (l_sourceColumn.MaxLength != l_TargetColoumn.MaxLength)
                            {
                                ErrorMessages.Add(new ErrorMessage("Data Length Mismatch, Source Column " + l_sourceTable.TableName + "." + l_ParseNodeSourceColumn.Token.Text + " and Referenced Column " + l_TargetTable.TableName + "." + l_ParseNodeTargetColumn.Token.Text, l_ParseNodeTargetColumn.Token.Location));
                            }

                        }


                    }



                }

            }// end of second iteration

            o_DataSet = l_DataSet;

            if (ErrorMessages.Count != 0)
            {
                o_DataSet.Clear();
            }

            return ErrorMessages;




        }
    }

    class CreateTableStatement : Statement
    {


        public ParseTreeNode m_id_simple;
        public List<TableFieldDef> m_FieldDefList;
        public List<ConstraintDef> m_constraintDefList;

        public CreateTableStatement(ParseTreeNode i_parseNode)
        {

            if (i_parseNode.IsError) return;

            if (i_parseNode.Term.Name != "createTableStmt") return;

            m_constraintDefList = new List<ConstraintDef>();

            foreach (ParseTreeNode parseNode in i_parseNode.ChildNodes)
            {

                if (i_parseNode.IsError) continue;

                switch (parseNode.Term.Name)
                {

                    case "Id":

                        foreach (ParseTreeNode parseNode_ID in parseNode.ChildNodes)
                        {

                            if (parseNode_ID.Term.Name == "id_simple")
                            {
                                m_id_simple = parseNode_ID;
                                break;

                            }

                        }

                        break;

                    case "fieldDefList":

                        foreach (ParseTreeNode parseTreeNode_FieldDef in parseNode.ChildNodes)
                        {

                            if (parseTreeNode_FieldDef.Term.Name == "fieldDef")
                            {
                                if (m_FieldDefList == null)
                                {
                                    m_FieldDefList = new List<TableFieldDef>();
                                }

                                m_FieldDefList.Add(new TableFieldDef(parseTreeNode_FieldDef));

                            }

                        }

                        break;


                    case "constraintListOpt":

                        foreach (ParseTreeNode ParseTreeNodeConstraintDef in parseNode.ChildNodes)
                        {
                            if (ParseTreeNodeConstraintDef.Term.Name == "constraintDef")
                            {

                                if (m_constraintDefList == null)
                                {

                                    m_constraintDefList = new List<ConstraintDef>();
                                }

                                switch (ParseTreeNodeConstraintDef.ChildNodes[2].ChildNodes[0].Term.Name)
                                {

                                    case "PRIMARY":

                                        m_constraintDefList.Add(new PrimaryKeyConstraintDef(ParseTreeNodeConstraintDef));
                                        break;

                                    case "NOT":
                                        m_constraintDefList.Add(new NotNullConstraintDef(ParseTreeNodeConstraintDef));
                                        break;

                                    case "FOREIGN":

                                        m_constraintDefList.Add(new ForeignKeyConstraintDef(ParseTreeNodeConstraintDef));
                                        break;


                                    case "UNIQUE":

                                        m_constraintDefList.Add(new UniqueConstraintDef(ParseTreeNodeConstraintDef));
                                        break;


                                }
                            }

                        }



                        break;

                    default:

                        break;

                }


            }

        }

    }

}
