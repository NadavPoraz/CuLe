using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;
using System.Xml;
using System.Text.RegularExpressions;

namespace CuLe
{
    public class CuLeDataSet : DataSet
    {

        public List<ErrorMessage> m_ErrorMessages;

        public List<ForeignKeyConstraint> m_ForeignKeyConstraints;

        protected List<ConstantValueCuLeSet> m_ConstantValueCuLeSets;
        protected List<CuLeRuleGroup> m_CuLeRuleGroups;

        protected List<CuLeTransaction> m_CuLeTransactions;

        private const int m_maxAlloyGroupSize = 7;

        public CuLeDataSet(StatementList i_StatementList)
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

            List<ConstraintDef> l_AllConstraints = new List<ConstraintDef>();
            List<String> l_AllConstraintNames = new List<String>();



            

            m_ErrorMessages = new List<ErrorMessage>();
            m_ConstantValueCuLeSets = new List<ConstantValueCuLeSet>();
            m_CuLeRuleGroups = new List<CuLeRuleGroup>();

            l_AllConstraints.Clear();

            //  first iteration

            foreach (CreateTableStatement l_createTableStatement in i_StatementList.m_CreateTableStatementList)
            {
                if (this.Tables.Contains(l_createTableStatement.m_id_simple.Token.Text))
                {

                    m_ErrorMessages.Add(new ErrorMessage("Table ID " + l_createTableStatement.m_id_simple.Token.Text + " already Exists", l_createTableStatement.m_id_simple.Token.Location));
                    continue;
                }

                DataTable dataTable = new DataTable(l_createTableStatement.m_id_simple.Token.Text);

                foreach (TableFieldDef tableFieldDef in l_createTableStatement.m_FieldDefList)
                {

                    // unique Table Column Names for Table
                    if (dataTable.Columns.Contains(tableFieldDef.m_id_simple.Token.Text))
                    {
                        m_ErrorMessages.Add(new ErrorMessage("Column ID " + tableFieldDef.m_id_simple.Token.Text + " already Exists", tableFieldDef.m_id_simple.Token.Location));
                    }
                    else
                    {
                        DataColumn dataColumn = new DataColumn(tableFieldDef.m_id_simple.Token.Text);

                        dataColumn.DataType = Statement.GetTypeFromStatement(tableFieldDef.m_typeName.Token.Text);

                        dataColumn.Caption = tableFieldDef.m_typeName.Token.Text.ToLower();

                        if (dataColumn.Caption == "char")
                        {
                            dataColumn.Caption = "string";
                        }

                        if (dataColumn.DataType == null)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Unknown Data Type " + tableFieldDef.m_typeName.Token.Text, tableFieldDef.m_typeName.Token.Location));
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
                        m_ErrorMessages.Add(new ErrorMessage("Constraint Name  " + l_ConstraintDef.m_id_simple.Token.Text + " already Defined", l_ConstraintDef.m_id_simple.Token.Location));
                        continue;
                    }

                    l_AllConstraints.Add(l_ConstraintDef);

                    if (l_ConstraintDef is PrimaryKeyConstraintDef)
                    {
                        PrimaryKeyConstraintDef l_PrimaryKeyConstraintDef = (PrimaryKeyConstraintDef)l_ConstraintDef;

                        if (dataTable.PrimaryKey != null && dataTable.PrimaryKey.Count() != 0)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Primary Key for Table " + dataTable.TableName + " already Defined", l_PrimaryKeyConstraintDef.m_id_simple.Token.Location));
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
                                    m_ErrorMessages.Add(new ErrorMessage("Primary Key error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                    continue;
                                }

                                //dataTable.PrimaryKey[primaryKeyIndex] = l_dataColumn;
                                ColumnArray[primaryKeyIndex] = l_dataColumn;
                                primaryKeyIndex++;

                            }

                            //dataTable.PrimaryKey = ColumnArray;

                            if (m_ErrorMessages != null && m_ErrorMessages.Count != 0)
                            {
                                continue;
                            }
                            else
                            {
                                UniqueConstraint l_UniqueConstraint = new UniqueConstraint(l_PrimaryKeyConstraintDef.ConstraintName(), ColumnArray, true);
                                dataTable.Constraints.Add(l_UniqueConstraint);
                            }

                        }
                    }

                    else if (l_ConstraintDef is UniqueConstraintDef)
                    {
                        UniqueConstraintDef l_UniqueConstraintDef = (UniqueConstraintDef)l_ConstraintDef;

                        DataColumn[] l_columns;

                        int l_coloumns_array_index = 0;

                        if (l_UniqueConstraintDef.m_coloumns == null )
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Unique Key error, no Columns Specified", l_ConstraintDef.m_id_simple.Token.Location));
                            continue;
                        }

                        if (l_UniqueConstraintDef.m_coloumns.Count == 0)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Unique Key error, no Columns Specified", l_ConstraintDef.m_id_simple.Token.Location));
                            continue;
                        }


                            
                         l_columns = new DataColumn[ l_UniqueConstraintDef.m_coloumns.Count ];

                        
                        foreach (ParseTreeNode parseTreeNodeColumn in l_UniqueConstraintDef.m_coloumns)
                        {

                            int l_dataColumnIndex;

                            if (dataTable.Columns.Contains(parseTreeNodeColumn.Token.Text))
                            {
                                l_dataColumnIndex = dataTable.Columns.IndexOf(parseTreeNodeColumn.Token.Text);
                            }
                            else
                            {
                                m_ErrorMessages.Add(new ErrorMessage("Unique Constraint error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                continue;
                            }

                            l_columns[l_coloumns_array_index] = dataTable.Columns[l_dataColumnIndex];
                            l_coloumns_array_index++;

                           // dataTable.Columns[l_dataColumnIndex].Unique = true;

                        }

                        if (l_columns != null)
                        {

                            UniqueConstraint l_UniqueConstraint = new UniqueConstraint(l_UniqueConstraintDef.ConstraintName(), l_columns, false);

                            dataTable.Constraints.Add(l_UniqueConstraint);
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
                                m_ErrorMessages.Add(new ErrorMessage("Not Null Constraint error, unknown Coloumn " + parseTreeNodeColumn.Token.Text, parseTreeNodeColumn.Token.Location));
                                continue;
                            }

                            dataTable.Columns[l_dataColumnIndex].AllowDBNull = false;

                        }

                    }
                }



                this.Tables.Add(dataTable);

            }

            //end of first iteration

            /*
            // second iteration
            
             *  foreign key - source columns exist, source columns unique, target table exists, 
             *  target columns exist, target columns unique, target columns of same Type as source columns,
             *  constraint name unique
             *  mulitplicity is valid
             *  */


            foreach (CreateTableStatement l_createTableStatement in i_StatementList.m_CreateTableStatementList)
            {

                DataTable l_childTable;

                if (this.Tables.Contains(l_createTableStatement.m_id_simple.Token.Text))
                {
                    l_childTable = this.Tables[this.Tables.IndexOf(l_createTableStatement.m_id_simple.Token.Text)];
                }
                else
                {
                    m_ErrorMessages.Add(new ErrorMessage("Child Table not recognized " + l_createTableStatement.m_id_simple.Token.Text, l_createTableStatement.m_id_simple.Token.Location));
                    continue;
                }


                foreach (ConstraintDef l_ConstraintDef in l_createTableStatement.m_constraintDefList)
                {

                    if (!(l_ConstraintDef is ForeignKeyConstraintDef))
                    {
                        continue;
                    }

                    ForeignKeyConstraintDef l_ForeignKeyConstraintDef = (ForeignKeyConstraintDef)l_ConstraintDef;

                    int l_numOfSourceColumns = l_ForeignKeyConstraintDef.m_childColoumns.Count;
                    int l_numOfTargetColumns = l_ForeignKeyConstraintDef.m_referencedColumns.Count;

                    bool l_flag_CurrForeignKeyIsValid = true;

                    DataTable l_ReferencedTable;



                    if (this.Tables.Contains(l_ForeignKeyConstraintDef.m_referencedTable.Token.Text))
                    {
                        l_ReferencedTable = this.Tables[this.Tables.IndexOf(l_ForeignKeyConstraintDef.m_referencedTable.Token.Text)];
                    }
                    else
                    {
                        m_ErrorMessages.Add(new ErrorMessage("Foreign Key Constraint " + l_ForeignKeyConstraintDef.m_id_simple.Token.Text + " Referenced table " + l_ForeignKeyConstraintDef.m_referencedTable.Token.Text + " not recognized", l_ForeignKeyConstraintDef.m_referencedTable.Token.Location));
                        l_flag_CurrForeignKeyIsValid = false;
                        continue;

                    }

                    if (l_numOfSourceColumns != l_numOfTargetColumns)
                    {
                        m_ErrorMessages.Add(new ErrorMessage("Foreign Key Constraint " + l_ForeignKeyConstraintDef.m_id_simple.Token.Text + " number of Child/Referenced columns mismatch ", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                        l_flag_CurrForeignKeyIsValid = false;
                        continue;
                    }

                    DataColumn[] l_childColumnsArray = new DataColumn[l_numOfSourceColumns];
                    DataColumn[] l_referencedColumnsArray = new DataColumn[l_numOfSourceColumns];


                    for (int i = 0; i < l_numOfSourceColumns; i++)
                    {
                        ParseTreeNode l_ParseNodeSourceColumn = l_ForeignKeyConstraintDef.m_childColoumns[i];
                        ParseTreeNode l_ParseNodeTargetColumn = l_ForeignKeyConstraintDef.m_referencedColumns[i];

                        DataColumn l_ChildColumn = null;
                        DataColumn l_ReferencedColoumn = null;
                        


                        if (l_childTable.Columns.Contains(l_ParseNodeSourceColumn.Token.Text))
                        {
                            l_ChildColumn = l_childTable.Columns[l_childTable.Columns.IndexOf(l_ParseNodeSourceColumn.Token.Text)];
                        }
                        else
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Child Column " + l_ParseNodeSourceColumn.Token.Text + " of table " + l_childTable.TableName + " not recognized ", l_ParseNodeSourceColumn.Token.Location));
                            l_flag_CurrForeignKeyIsValid = false;
                        }
                        if (l_ReferencedTable.Columns.Contains(l_ParseNodeTargetColumn.Token.Text))
                        {
                            l_ReferencedColoumn = l_ReferencedTable.Columns[l_ReferencedTable.Columns.IndexOf(l_ParseNodeTargetColumn.Token.Text)];
                        }
                        else
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Refrenced Column " + l_ParseNodeTargetColumn.Token.Text + " of table " + l_ReferencedTable.TableName + " not recognized ", l_ParseNodeTargetColumn.Token.Location));
                            l_flag_CurrForeignKeyIsValid = false;
                        }

                        if (l_ChildColumn != null && l_ReferencedColoumn != null)
                        {
                            if (l_ChildColumn.DataType != l_ReferencedColoumn.DataType)
                            {
                                m_ErrorMessages.Add(new ErrorMessage("DataType Mismatch, Child Column " + l_childTable.TableName + "." + l_ParseNodeSourceColumn.Token.Text + " and Referenced Column " + l_ReferencedTable.TableName + "." + l_ParseNodeTargetColumn.Token.Text, l_ParseNodeTargetColumn.Token.Location));
                                l_flag_CurrForeignKeyIsValid = false;
                            }

                            else if (l_ChildColumn.MaxLength != l_ReferencedColoumn.MaxLength)
                            {
                                m_ErrorMessages.Add(new ErrorMessage("Data Length Mismatch, Child Column " + l_childTable.TableName + "." + l_ParseNodeSourceColumn.Token.Text + " and Referenced Column " + l_ReferencedTable.TableName + "." + l_ParseNodeTargetColumn.Token.Text, l_ParseNodeTargetColumn.Token.Location));
                                l_flag_CurrForeignKeyIsValid = false;
                            }

                        }

   
                        if (l_flag_CurrForeignKeyIsValid == true)
                        {
                            l_childColumnsArray[i] = l_ChildColumn;
                            l_referencedColumnsArray[i] = l_ReferencedColoumn;
                        }


                    }

                    if (l_flag_CurrForeignKeyIsValid == false)
                    {
                        continue;
                    }

                    // find unique constraint for referenced Table

                    // if the referenced columns list includes a full key, then it is unique. else, it is multiple.

                    // the referenced columns list includes a full key if exists a unique constraint for referenced Table which ALL it's columns  included in the referenced column list

                    bool lv_parent_is_unique = false;

                    foreach (Constraint l_constraint in l_ReferencedTable.Constraints)
                    {
                        if (l_constraint is UniqueConstraint)
                        {

                            bool lv_flag_all_coloums_referenced = true;

                            UniqueConstraint l_uniqueConstraint = (UniqueConstraint)l_constraint;

                            foreach (DataColumn l_dataColumn in l_uniqueConstraint.Columns)
                            {

                                if( l_referencedColumnsArray.Contains(l_dataColumn))
                                {
                                }
                                else
                                {
                                    lv_flag_all_coloums_referenced = false;
                                }
                            }
                            if (lv_flag_all_coloums_referenced == true)  // parent is unique
                            {
                                lv_parent_is_unique = true;
                            }

                        }
                    }

                    if (lv_parent_is_unique == true)  // parent is unique
                    {
                        if (l_ForeignKeyConstraintDef.m_parentMultiplicity == ForeignKeyConstraintDef.Multiplicity.many)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Mulitplicity Error, Refrenced Entry is unique", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                            l_flag_CurrForeignKeyIsValid = false;
                        }
                    }
                    else  // parent is not unique
                    {
                        if (l_ForeignKeyConstraintDef.m_parentMultiplicity == ForeignKeyConstraintDef.Multiplicity.single)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Mulitplicity Error, Refrenced Entry is not unique", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                            l_flag_CurrForeignKeyIsValid = false;
                        }
                    }

                    // end of uniqueness check for referenced Table


                    // find unique constraint for child Table

                    // if child coloumns list includes a full key, then it is unique. else, it is multiple.
                    // the child coloumns list includes a key if exists a unique constraint which ALL it's coloumns are in the child coloumns list


                    bool lv_child_is_unique = false;

                    foreach (Constraint l_constraint in l_childTable.Constraints)
                    {
                        if (l_constraint is UniqueConstraint)
                        {

                            bool lv_flag_all_coloums_referenced = true;

                            UniqueConstraint l_uniqueConstraint = (UniqueConstraint)l_constraint;

                            foreach (DataColumn l_dataColumn in l_uniqueConstraint.Columns)
                            {

                                if( l_childColumnsArray.Contains(l_dataColumn))
                                {
                                }
                                else
                                {
                                    lv_flag_all_coloums_referenced = false;
                                }
                            }
                            if (lv_flag_all_coloums_referenced == true)  // child is unique
                            {
                                lv_child_is_unique = true;
                            }
                        }
                    }


                    if (lv_child_is_unique == true)  // child is unique
                    {

                        if (l_ForeignKeyConstraintDef.m_ChildMultiplicity == ForeignKeyConstraintDef.Multiplicity.many)
                        {
                            m_ErrorMessages.Add(new ErrorMessage("Mulitplicity Error, Child Entry is unique", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                            l_flag_CurrForeignKeyIsValid = false;
                        }
                    }
                    else  // child is not unique
                    {
                         if (l_ForeignKeyConstraintDef.m_ChildMultiplicity == ForeignKeyConstraintDef.Multiplicity.single)
                         {
                             m_ErrorMessages.Add(new ErrorMessage("Mulitplicity Error, Child Entry is not unique", l_ForeignKeyConstraintDef.m_id_simple.Token.Location));
                             l_flag_CurrForeignKeyIsValid = false;
                         }
                    }

                        
                     // end of uniqueness check for Child Table

                    if (l_flag_CurrForeignKeyIsValid == false)
                    {
                        continue;
                    }
                

                      ForeignKeyConstraint l_ForeignKeyConstraint = new ForeignKeyConstraint(l_ForeignKeyConstraintDef.ConstraintName(), l_referencedColumnsArray, l_childColumnsArray);
        
                         l_childTable.Constraints.Add(l_ForeignKeyConstraint);

                        //DataRelation l_dataRelation = new DataRelation(l_ForeignKeyConstraint.ConstraintName, l_referencedColumnsArray, l_childColumnsArray);

                }

            }// end of second iteration

            List<ForeignKeyConstraint> l_allForeignKeyConstraints = new List<ForeignKeyConstraint>();

            foreach (DataTable l_DataTable in this.Tables)
            {
                foreach (Constraint l_Constraint in l_DataTable.Constraints)
                {
                    if (l_Constraint is ForeignKeyConstraint)
                    {
                        ForeignKeyConstraint l_ForeignKeyConstraint = (ForeignKeyConstraint)l_Constraint;

                        l_allForeignKeyConstraints.Add(l_ForeignKeyConstraint);

                        DataRelation l_DataRelation = new DataRelation(l_ForeignKeyConstraint.ConstraintName, l_ForeignKeyConstraint.RelatedColumns, l_ForeignKeyConstraint.Columns);

                        this.Relations.Add(l_DataRelation);
                       }

                }

            }

           

            if (i_StatementList.m_CuLeRuleStatementList != null)
            {
                CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory(this.Tables, this.Relations, null);

                foreach (CuLeRuleStatement l_CuLeRuleStatement in i_StatementList.m_CuLeRuleStatementList)
                {

                    // validate all quantifier Table Names refer to actual tables
                    // validate unique tuple names in Quantifier Statement


                    // m_ConstantValueCuLeSets

                    List<string> l_lisOfTuples = new List<string>();

                    CuLeRuleGroup l_CuLeRuleGroup = new CuLeRuleGroup(l_CuLeRuleStatement.Name);

                    if (l_CuLeRuleStatement.m_CuLeQuantifiers != null)
                    {

                        foreach (CuLeQuantifier l_CuLeQuantifier in l_CuLeRuleStatement.m_CuLeQuantifiers)
                        {
                            if (l_CuLeQuantifier is CuLeTupleQuantifier && (!(this.Tables.Contains(l_CuLeQuantifier.Signature))))
                            {
                                m_ErrorMessages.Add(new ErrorMessage("Unknown Table " + l_CuLeQuantifier.Signature, l_CuLeQuantifier.m_id_simple.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Location));
                            }

                            else
                            {
                                if (l_lisOfTuples.Contains(l_CuLeQuantifier.Name))
                                {
                                    m_ErrorMessages.Add(new ErrorMessage("Quantifier " + l_CuLeQuantifier.Name + " already Declared", l_CuLeQuantifier.m_id_simple.Token.Location));
                                }
                                else
                                {
                                    l_lisOfTuples.Add(l_CuLeQuantifier.Name);
                                }
                            }
                        }
                    }



                    foreach (CuLeRuleDef l_CuLeRuleDef in l_CuLeRuleStatement.m_CuLeRuleDefList)
                    {

                        CuLeRule l_CuLeRule = l_CuLeRuleFactory.CreateCuLeRule(l_CuLeRuleDef, l_CuLeRuleStatement.m_CuLeQuantifiers, null);

                        if (l_CuLeRuleFactory != null)
                        {
                            if (l_CuLeRuleFactory.m_errorMessages != null)
                            {
                                if (l_CuLeRuleFactory.m_errorMessages.Count != 0)
                                {
                                    this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeRuleFactory.m_errorMessages).ToList();
                                    l_CuLeRuleFactory.m_errorMessages.Clear();
                                    continue;

                                }
                            }

                            l_CuLeRuleGroup.m_CuLeRules.Add(l_CuLeRule);


                        }
                    }

                    l_CuLeRuleGroup.m_CuLeQuantifiers = l_CuLeRuleStatement.m_CuLeQuantifiers;

                    m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Concat(l_CuLeRuleGroup.GetConstantValueCuLeSets()).ToList();

                    m_CuLeRuleGroups.Add(l_CuLeRuleGroup);

                }
            }

           

           // m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Distinct().ToList();


            if (i_StatementList.m_CuLeTransactionStatementList != null)
            {
                m_CuLeTransactions = new List<CuLeTransaction>();

                foreach (CuLeTransactionStatement l_CuLeTransactionStatement in i_StatementList.m_CuLeTransactionStatementList)
                {

                    // Transaction 

                    List<CuLeRuleGroup> l_ResultCuLeRuleGroupList = new List<CuLeRuleGroup>();
                    List<CuLeRuleGroup> l_PreconditionCuLeRuleGroupList = new List<CuLeRuleGroup>();
                    List<CuLeRule> l_InputRestrictionCuLeRulesList = new List<CuLeRule>();
                    List<CuLeSet> l_ChangingCuLeSets = new List<CuLeSet>();

                    

                    // Transaction PreCondition

                    if (l_CuLeTransactionStatement.PreconditionDef != null)
                    {
                        foreach (CuLeRuleStatement l_CuLeRuleStatement in l_CuLeTransactionStatement.PreconditionDef)
                        {
                            CuLeRuleGroup l_CuLeRuleGroup = new CuLeRuleGroup(null);

                            List<String> l_States = new List<string>();

                            l_States.Add("OLD_STATE");

                            CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory(this.Tables, this.Relations, l_States);

                            List<string> l_lisOfTuples = new List<string>();

                            List<CuLeQuantifier> l_ListOfCuLeQuantifier = new List<CuLeQuantifier>();

                            if (l_CuLeTransactionStatement.ParametersDef != null)
                            {
                                l_ListOfCuLeQuantifier.AddRange(l_CuLeTransactionStatement.ParametersDef);
                            }
                            if (l_CuLeRuleStatement.m_CuLeQuantifiers != null)
                            {
                                l_ListOfCuLeQuantifier.AddRange(l_CuLeRuleStatement.m_CuLeQuantifiers);
                            }

                            CuLeQuantifierList l_CuLeQuantifierList = new CuLeQuantifierList(l_ListOfCuLeQuantifier);

                            l_CuLeQuantifierList.Add(new CuLeSystemField("Date","System_Date"));
                            l_CuLeQuantifierList.Add(new CuLeSystemField("Time", "System_Time"));

                            if (l_CuLeQuantifierList != null)
                            {
                                foreach (CuLeQuantifier l_CuLeQuantifier in l_CuLeQuantifierList)
                                {
                                    if (l_CuLeQuantifier is CuLeTupleQuantifier && (!(this.Tables.Contains(l_CuLeQuantifier.Signature))))
                                    {
                                        m_ErrorMessages.Add(new ErrorMessage("Unknown Table " + l_CuLeQuantifier.Signature, l_CuLeQuantifier.Token.Location));
                                    }

                                    else
                                    {
                                        if (l_lisOfTuples.Contains(l_CuLeQuantifier.Name))
                                        {
                                            m_ErrorMessages.Add(new ErrorMessage("Quantifier " + l_CuLeQuantifier.Name + " already Declared", l_CuLeQuantifier.m_id_simple.Token.Location));
                                        }
                                        else
                                        {
                                            l_lisOfTuples.Add(l_CuLeQuantifier.Name);
                                        }
                                    }
                                }
                            }

                            foreach (CuLeRuleDef l_CuLeRuleDef in l_CuLeRuleStatement.m_CuLeRuleDefList)
                            {
                                CuLeRule l_CuLeRule = l_CuLeRuleFactory.CreateCuLeRule(l_CuLeRuleDef, l_CuLeQuantifierList, "OLD_STATE");

                                if (l_CuLeRuleFactory.m_errorMessages != null)
                                {
                                    if (l_CuLeRuleFactory.m_errorMessages.Count != 0)
                                    {
                                        this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeRuleFactory.m_errorMessages).ToList();
                                        l_CuLeRuleFactory.m_errorMessages.Clear();
                                        continue;
                                    }
                                }

                                l_CuLeRuleGroup.m_CuLeRules.Add(l_CuLeRule);
                                
                            }

                            l_CuLeRuleGroup.m_CuLeQuantifiers = l_CuLeRuleStatement.m_CuLeQuantifiers;

                            m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Concat(l_CuLeRuleGroup.GetConstantValueCuLeSets()).ToList();

                            l_PreconditionCuLeRuleGroupList.Add(l_CuLeRuleGroup);

                        }
                    }           
            

                    // Transaction Input_Restrictions

                    if (l_CuLeTransactionStatement.InputRestrictionDef != null)
                    {
                        List<String> l_States = new List<string>();

                        l_States.Add("NEW_STATE");

                        CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory(this.Tables, this.Relations, l_States);

                        foreach (CuLeRuleDef l_InputRestrictionCuLeRuleDef in l_CuLeTransactionStatement.InputRestrictionDef)
                        {
                            CuLeRule l_InputRestrictionCuLeRule = l_CuLeRuleFactory.CreateCuLeRule(l_InputRestrictionCuLeRuleDef, l_CuLeTransactionStatement.ParametersDef, "NEW_STATE");

                            if (l_CuLeRuleFactory.m_errorMessages.Count != 0)
                            {
                                this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeRuleFactory.m_errorMessages).ToList();
                                l_CuLeRuleFactory.m_errorMessages.Clear();
                                continue;

                            }

                            l_InputRestrictionCuLeRulesList.Add(l_InputRestrictionCuLeRule);

                            m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Concat(l_InputRestrictionCuLeRule.GetConstantValueCuLeSets()).ToList();
                        }
                    }

                    

                    // Transaction Changing

                    if (l_CuLeTransactionStatement.ChangingSetDefs != null)
                    {

                        CuLeSetFactory l_CuLeSetFactory = new CuLeSetFactory(this.Tables, this.Relations, null);

                        foreach (CuLeSetDef l_CuLeSetDef in l_CuLeTransactionStatement.ChangingSetDefs)
                        {
                            CuLeSet l_CuLeSet = l_CuLeSetFactory.CreateCuLeSet(l_CuLeSetDef, l_CuLeTransactionStatement.ParametersDef, null);

                            if (l_CuLeSetFactory.m_errorMessages.Count != 0)
                            {
                                this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeSetFactory.m_errorMessages).ToList();
                                l_CuLeSetFactory.m_errorMessages.Clear();
                                continue;
                            }

                            if (!(l_CuLeSet.CuLeSetType is TupleCuLeSetType))
                            {
                                m_ErrorMessages.Add(new ErrorMessage("Changing Declaration Can only be applied to Tuples and Tables", l_CuLeSetDef.SourceLocation));
                            }
                            else
                            {
                                l_ChangingCuLeSets.Add(l_CuLeSet);
                            }


                        }
                    }

                   
                    // Transaction Result

                    if (l_CuLeTransactionStatement.ResultDef != null)
                    {
                        foreach (CuLeRuleStatement l_CuLeRuleStatement in l_CuLeTransactionStatement.ResultDef)
                        {
                                List<String> l_States = new List<string>();

                                l_States.Add("OLD_STATE");
                                l_States.Add("NEW_STATE");

                            CuLeRuleGroup l_CuLeRuleGroup = new CuLeRuleGroup(null);

                            CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory(this.Tables, this.Relations, l_States);

                            List<string> l_lisOfTuples = new List<string>();

                            List<CuLeQuantifier> l_ListOfCuLeQuantifier = new List<CuLeQuantifier>();

                            if (l_CuLeTransactionStatement.ParametersDef != null)
                            {
                                l_ListOfCuLeQuantifier.AddRange(l_CuLeTransactionStatement.ParametersDef);
                            }
                            if (l_CuLeRuleStatement.m_CuLeQuantifiers != null)
                            {
                                l_ListOfCuLeQuantifier.AddRange(l_CuLeRuleStatement.m_CuLeQuantifiers);
                            }

                            CuLeQuantifierList l_CuLeQuantifierList = new CuLeQuantifierList(l_ListOfCuLeQuantifier);

                           // l_CuLeQuantifierList.Add(new CuLeSystemField("Date", "System_Date"));
                            //l_CuLeQuantifierList.Add(new CuLeSystemField("Time", "System_Time"));


                            if (l_CuLeQuantifierList != null)
                            {
                                foreach (CuLeQuantifier l_CuLeQuantifier in l_CuLeQuantifierList)
                                {
                                    if (l_CuLeQuantifier is CuLeTupleQuantifier && (!(this.Tables.Contains(l_CuLeQuantifier.Signature))))
                                    {
                                        m_ErrorMessages.Add(new ErrorMessage("Unknown Table " + l_CuLeQuantifier.Signature, l_CuLeQuantifier.Token.Location));
                                    }

                                    else
                                    {
                                        if (l_lisOfTuples.Contains(l_CuLeQuantifier.Name))
                                        {
                                            m_ErrorMessages.Add(new ErrorMessage("Quantifier " + l_CuLeQuantifier.Name + " already Declared", l_CuLeQuantifier.m_id_simple.Token.Location));
                                        }
                                        else
                                        {
                                            l_lisOfTuples.Add(l_CuLeQuantifier.Name);
                                        }
                                    }
                                }
                            }

                            foreach (CuLeRuleDef l_CuLeRuleDef in l_CuLeRuleStatement.m_CuLeRuleDefList)
                            {
                                CuLeRule l_CuLeRule = l_CuLeRuleFactory.CreateCuLeRule(l_CuLeRuleDef, l_CuLeQuantifierList, null);

                                if (l_CuLeRuleFactory.m_errorMessages != null)
                                {
                                    if (l_CuLeRuleFactory.m_errorMessages.Count != 0)
                                    {
                                        this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeRuleFactory.m_errorMessages).ToList();
                                        l_CuLeRuleFactory.m_errorMessages.Clear();
                                        continue;
                                    }
                                }

                                l_CuLeRuleGroup.m_CuLeRules.Add(l_CuLeRule);

                            }

                            l_CuLeRuleGroup.m_CuLeQuantifiers = l_CuLeRuleStatement.m_CuLeQuantifiers;

                            m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Concat(l_CuLeRuleGroup.GetConstantValueCuLeSets()).ToList();

                            l_ResultCuLeRuleGroupList.Add(l_CuLeRuleGroup);

                        }
                    } 

                    //if (l_CuLeTransactionStatement.ResultDef != null)
                    //{

                    //    List<String> l_States = new List<string>();

                    //    l_States.Add("OLD_STATE");
                    //    l_States.Add("NEW_STATE");

                    //    CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory(this.Tables, this.Relations, l_States);

                    //    foreach (CuLeRuleDef l_ResultCuLeRuleDef in l_CuLeTransactionStatement.ResultDef)
                    //    {
                    //        CuLeRule l_ResultCuLeRule = l_CuLeRuleFactory.CreateCuLeRule(l_ResultCuLeRuleDef, l_CuLeTransactionStatement.ParametersDef, null);

                    //        if (l_CuLeRuleFactory.m_errorMessages.Count != 0)
                    //        {
                    //            this.m_ErrorMessages = m_ErrorMessages.Concat(l_CuLeRuleFactory.m_errorMessages).ToList();
                    //            l_CuLeRuleFactory.m_errorMessages.Clear();
                    //            continue;

                    //        }

                    //        l_ResultCuLeRulesList.Add(l_ResultCuLeRule);
                    //    }
                    //}

                   

                    // Transaction 


                    CuLeTransaction l_CuLeTransaction = new CuLeTransaction(l_CuLeTransactionStatement.TransactionName, l_CuLeTransactionStatement.ParametersDef, l_ChangingCuLeSets, l_PreconditionCuLeRuleGroupList, l_InputRestrictionCuLeRulesList, l_ResultCuLeRuleGroupList);
                    this.m_CuLeTransactions.Add(l_CuLeTransaction);
                }
            }

            List<ConstantValueCuLeSet> l_ConstantValueCuLeSets = new List<ConstantValueCuLeSet>();

            l_ConstantValueCuLeSets.AddRange(m_ConstantValueCuLeSets);

            m_ConstantValueCuLeSets.Clear();

            foreach (ConstantValueCuLeSet l_ConstantValueCuLeSetA in l_ConstantValueCuLeSets)
            {
                if (!(m_ConstantValueCuLeSets.Contains(l_ConstantValueCuLeSetA)))
                {
                    m_ConstantValueCuLeSets.Add(l_ConstantValueCuLeSetA);
                }
            }

            m_ConstantValueCuLeSets = m_ConstantValueCuLeSets.Distinct<ConstantValueCuLeSet>().ToList<ConstantValueCuLeSet>();

            if (m_ErrorMessages.Count != 0)
            {

            }
        }

        private List<ErrorMessage> Validate(ParseTreeNode i_Set, List<ForeignKeyConstraint> i_ListOfForeignKeyConstraints, List<string> i_lisOfTuples)
        {

            return null;           

        }

        public List<String> WriteSchemaToAlloy()
        {

            List<String> l_SchemaListOfStrings = new List<string>();

            l_SchemaListOfStrings.Add("open util/ordering[State] as ord");
            l_SchemaListOfStrings.Add("open util/ordering[double] as DoubleOrder");
            l_SchemaListOfStrings.Add("open util/ordering[time] as TimeOrder");
            l_SchemaListOfStrings.Add("open util/ordering[date] as DateOrder");
            l_SchemaListOfStrings.Add("open util/ordering[string] as StringOrder");
            l_SchemaListOfStrings.Add("open util/ordering[integer] as IntOrder");
            
            l_SchemaListOfStrings.Add("sig double {}");
            l_SchemaListOfStrings.Add("sig date {}");
            l_SchemaListOfStrings.Add("sig time {}");
            l_SchemaListOfStrings.Add("sig string {}");
            l_SchemaListOfStrings.Add("sig integer {}");


            l_SchemaListOfStrings.Add("sig State {");

            foreach (DataTable l_dataTable in this.Tables)
            {
                l_SchemaListOfStrings.Add(l_dataTable.TableName + ": set this/" + l_dataTable.TableName + ",");
            }

            l_SchemaListOfStrings.Add("System_Date : one date,");
            l_SchemaListOfStrings.Add("System_Time : one time");
            l_SchemaListOfStrings.Add(" }");

            foreach (DataTable l_dataTable in this.Tables)
            {

                List<String> l_TableListOfStrings = new List<string>();

                List<String> l_FieldsTableListOfStrings = new List<string>();
                List<String> l_RelationsListOfStrings = new List<string>();
                List<String> l_LimitRelationsForStateListOfStrings = new List<string>();
                List<String> l_MultiplicityListOfStrings = new List<string>();

                string l_signatureString = "";
                string l_fieldString = "";
                string l_uniqueConstraintString = "";
                string l_multiplicityString = "";

                l_signatureString = "sig " + l_dataTable.TableName + "{ ";

                l_TableListOfStrings.Add(l_signatureString);

                if (l_dataTable.Columns != null)
                {

                    foreach (DataColumn l_dataColumn in l_dataTable.Columns)
                    {
                        if (l_fieldString != "")
                        {
                            l_fieldString = l_fieldString + ", ";
                            l_FieldsTableListOfStrings.Add(l_fieldString);
                        }

                        if (l_dataColumn.AllowDBNull == true)
                        {                           
                         //   l_fieldString = l_dataColumn.ColumnName + ": lone " + l_dataColumn.Caption + " ";
                           // l_fieldString = l_dataColumn.ColumnName + ": " + l_dataColumn.Caption + " lone -> State ";

                            l_fieldString = l_dataColumn.ColumnName + ": " + l_dataColumn.Caption + " -> State ";

                            l_multiplicityString = "fact{ all State_X: State, " + l_dataTable.TableName + "1 : State_X." + l_dataTable.TableName + " | # " + l_dataTable.TableName + "1." + l_dataColumn.ColumnName + ".State_X <= 1 }";
                        }
                        else
                        {                           
                        //    l_fieldString = l_dataColumn.ColumnName + ": one " + l_dataColumn.Caption + " ";
                         //   l_fieldString = l_dataColumn.ColumnName + ": " + l_dataColumn.Caption + " one -> State ";

                            l_fieldString = l_dataColumn.ColumnName + ": " + l_dataColumn.Caption + " -> State ";

                            l_multiplicityString = "fact{ all State_X: State, " + l_dataTable.TableName + "1 : State_X." + l_dataTable.TableName + " | # " + l_dataTable.TableName + "1." + l_dataColumn.ColumnName + ".State_X = 1 }";
                           
                        }

                        l_MultiplicityListOfStrings.Add(l_multiplicityString);

                        l_multiplicityString = "fact{ all State_X: State, " + l_dataTable.TableName + "1 : this/" + l_dataTable.TableName + " - State_X." + l_dataTable.TableName + " | # " + l_dataTable.TableName + "1." + l_dataColumn.ColumnName + ".State_X = 0 }";

                        l_MultiplicityListOfStrings.Add(l_multiplicityString);
                    }

                    l_FieldsTableListOfStrings.Add(l_fieldString);

                    string l_relationString = "";
                    string l_LimitRelationForStateString = "";

                    if (l_dataTable.Constraints != null)
                    {

                        foreach (Constraint l_Constraint in l_dataTable.Constraints)
                        {

                            if ( l_Constraint is ForeignKeyConstraint)
                            {

                                ForeignKeyConstraint l_ForeignKeyConstraint = (ForeignKeyConstraint)l_Constraint;

                                if (this.Relations.Contains(l_ForeignKeyConstraint.ConstraintName))
                                {
                                    DataRelation l_DataRelation = this.Relations[this.Relations.IndexOf(l_ForeignKeyConstraint.ConstraintName)];

                                    if (l_DataRelation == null)
                                    {
                                        throw new System.ArgumentException("Relation Argument Error");
                                    }

                                    if (l_DataRelation.ChildTable != l_dataTable)
                                    {
                                        throw new System.ArgumentException("Relation Argument Error");
                                    }

                                    if (l_relationString != "")
                                    {
                                        l_relationString = l_relationString + ", ";
                                        l_RelationsListOfStrings.Add(l_relationString);
                                    }
                                    

                                    RelationMultiplicity l_RelationMultiplicity = GetRelationMultiplicity(l_DataRelation.RelationName, this.Tables, this.Relations);

                                    if (l_RelationMultiplicity == RelationMultiplicity.OneToOne)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " one -> one " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": one " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " one -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";
                                    }

                                    else if (l_RelationMultiplicity == RelationMultiplicity.OneToMany)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " one -> some " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": some " + l_DataRelation.ParentTable.TableName;
                                       // l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " some -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";
                                    }

                                    else if (l_RelationMultiplicity == RelationMultiplicity.LoneToOne)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " one -> one " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": one " + l_DataRelation.ParentTable.TableName;
                                       // l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " one -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";

                                        //  l_LimitRelationForStateString = "fact{ all State_X : State, " + l_dataTable.TableName + "1: this/" + l_dataTable.TableName + " - State_X." + l_dataTable.TableName + " | no " + l_dataTable.TableName + "1." + l_DataRelation.RelationName + ".State_X }";

                                        //  l_LimitRelationsForStateListOfStrings.Add(l_LimitRelationForStateString);
                                    }

                                    else if (l_RelationMultiplicity == RelationMultiplicity.LoneToMany)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " one -> some " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": some " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " some -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";

                                       // l_LimitRelationForStateString = "fact{ all State_X : State, " + l_dataTable.TableName + "1: this/" + l_dataTable.TableName + " - State_X." + l_dataTable.TableName + " | no " + l_dataTable.TableName + "1." + l_DataRelation.RelationName + ".State_X }";

                                        //  l_LimitRelationsForStateListOfStrings.Add(l_LimitRelationForStateString);
                                    }


                                    else if (l_RelationMultiplicity == RelationMultiplicity.ManyToOne)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " some -> one " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": one " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " one -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";

                                    }

                                    else if (l_RelationMultiplicity == RelationMultiplicity.ManyToMany)
                                    {
                                        //l_relationString = l_DataRelation.RelationName + ": " + l_DataRelation.ChildTable.TableName + " some -> some " + l_DataRelation.ParentTable.TableName;
                                        //l_relationString = l_DataRelation.RelationName + ": some " + l_DataRelation.ParentTable.TableName;
                                       // l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " some -> State";

                                        l_relationString = l_DataRelation.RelationName + " : " + l_DataRelation.ParentTable.TableName + " -> State";

                                    }

                                    else
                                    {
                                        throw new System.ArgumentException("Relation Argument Error");
                                    }




                                    l_LimitRelationForStateString = "fact{ all State_X : State, " + l_dataTable.TableName + "1 : State_X." + l_dataTable.TableName + " | " + l_dataTable.TableName + "1." + l_DataRelation.RelationName + ".State_X in State_X." + l_DataRelation.ParentTable.TableName + " }";

                                    l_LimitRelationsForStateListOfStrings.Add(l_LimitRelationForStateString);

                                    l_LimitRelationForStateString = "fact{ all State_X : State, " + l_dataTable.TableName + "1: this/" + l_dataTable.TableName + " - State_X." + l_dataTable.TableName + " | no " + l_dataTable.TableName + "1." + l_DataRelation.RelationName + ".State_X }";

                                    l_LimitRelationsForStateListOfStrings.Add(l_LimitRelationForStateString);


                                }
                            }

                        }

                        if (l_relationString != "")
                        {
                            l_RelationsListOfStrings.Add(l_relationString);
                        }

                    }


                    

                    if (l_FieldsTableListOfStrings.Count != 0 && l_RelationsListOfStrings.Count != 0)
                    {

                        string lv_lastFieldString = l_FieldsTableListOfStrings[l_FieldsTableListOfStrings.Count - 1];

                        lv_lastFieldString = lv_lastFieldString + ", ";

                        l_FieldsTableListOfStrings[l_FieldsTableListOfStrings.Count - 1] = lv_lastFieldString;

                        //l_TableListOfStrings.Add(",");
                    }

                    l_TableListOfStrings.AddRange(l_FieldsTableListOfStrings);
                    l_TableListOfStrings.AddRange(l_RelationsListOfStrings);
                    
                    l_TableListOfStrings.Add("}");

                }

                l_TableListOfStrings.AddRange(l_MultiplicityListOfStrings);
                l_TableListOfStrings.AddRange(l_LimitRelationsForStateListOfStrings);  

                if (l_dataTable.Constraints != null)
                {

                    foreach (Constraint l_Constraint in l_dataTable.Constraints)
                    {

                        if (l_Constraint is UniqueConstraint)
                        {
                            UniqueConstraint l_UniqueConstraint = (UniqueConstraint)l_Constraint;

                            string l_tuple1 = l_dataTable.TableName + "1";
                            string l_tuple2 = l_dataTable.TableName + "2";
                            string l_ColumnString = "";

                                l_uniqueConstraintString = "fact { all State_X : State, " + l_tuple1 + ", " + l_tuple2 + " : " + "State_X." + l_dataTable.TableName + " | ( ( ";

                                // l_uniqueConstraintString = l_uniqueConstraintString + " ( " + l_tuple1 + " in State_X." + l_dataTable.TableName + " and " + l_tuple2 + " in State_X." + l_dataTable.TableName + "  ) implies "; 

                                l_ColumnString = "";

                                foreach (DataColumn l_DataColumn in l_UniqueConstraint.Columns)
                                {
                                    if (l_ColumnString != "")
                                    {
                                        l_ColumnString = l_ColumnString + " and ";
                                       // l_uniqueConstraintString = l_uniqueConstraintString + l_ColumnString;
                                    }

                                    l_ColumnString = l_ColumnString + "( " + l_tuple1 + "." + l_DataColumn.ColumnName + ".State_X" + " = " + l_tuple2 + "." + l_DataColumn.ColumnName + ".State_X  )";

                                }

                                
                                l_uniqueConstraintString = l_uniqueConstraintString + l_ColumnString + " ) implies ( " + l_tuple1 + " = " + l_tuple2 + " ) ) } ";

                                l_TableListOfStrings.Add("//Enforce Unique Constraint");
                                l_TableListOfStrings.Add(l_uniqueConstraintString);

                                if (l_UniqueConstraint.IsPrimaryKey)
                                {

                                    string l_PrimaryKeyConstraintString = "fact { all " + l_tuple1 + " : " + "this/" + l_dataTable.TableName + ", State_X, State_Y : State | ( " + l_tuple1 + " in State_X." + l_dataTable.TableName + " and " + l_tuple1 + " in State_Y." + l_dataTable.TableName + "  ) implies (";

                                    l_ColumnString = "";

                                    foreach (DataColumn l_DataColumn in l_UniqueConstraint.Columns)
                                    {

                                        if (l_ColumnString != "")
                                        {
                                            l_ColumnString = l_ColumnString + " and ";
                                       }

                                        l_ColumnString =  l_ColumnString + "( " + l_tuple1 + "." + l_DataColumn.ColumnName + ".State_X" + " = " + l_tuple1 + "." + l_DataColumn.ColumnName + ".State_Y )  ";

                                    }

                                    l_PrimaryKeyConstraintString = l_PrimaryKeyConstraintString +  l_ColumnString + " ) }";

                                    l_TableListOfStrings.Add("//Enforce Primary Key Constraint");

                                    l_TableListOfStrings.Add(l_PrimaryKeyConstraintString);
                                }



                        }

                            

                            

                        else if (l_Constraint is ForeignKeyConstraint)
                        {

                        }
                    
                    }

                }



                foreach (DataColumn l_dataColumn1 in l_dataTable.Columns)
                {

                    string l_NotNullConstraintString = null;
                    if (l_dataColumn1.AllowDBNull == false)
                    {

                        //foreach (DataColumn l_dataColumn2 in l_dataTable.Columns)
                        //{
                        //    if (l_NotNullConstraintString == null)
                        //    {
                        //        l_NotNullConstraintString = l_NotNullConstraintString + " and ";
                        //    }
                        //    else
                        //    {
                        //        l_NotNullConstraintString = l_NotNullConstraintString + "( " + l_dataTable.TableName + "1." + l_dataColumn2.ColumnName + ".State_X = none ) ";
                        //    }

                        //    l_NotNullConstraintString = l_NotNullConstraintString + ") } ";
                        //}

                        //l_NotNullConstraintString = "fact { all State_X : State, " + l_dataTable.TableName + "1 : State_X." + l_dataTable.TableName + " | ( " + l_dataTable.TableName + "1." + l_dataColumn1.ColumnName + ".State_X = none) implies (" + l_NotNullConstraintString + " ) } ";

                        l_NotNullConstraintString = "fact { all State_X : State, " + l_dataTable.TableName + "1 : State_X." + l_dataTable.TableName + " | ( not ( " + l_dataTable.TableName + "1." + l_dataColumn1.ColumnName + ".State_X = none) ) }";
                    }

                    if (l_NotNullConstraintString != null)
                    {
                        l_TableListOfStrings.Add("//Enforce Not Null Constraint");

                        l_TableListOfStrings.Add(l_NotNullConstraintString);
                    }

                }

                

                //string lv_fieldsInitialString = null;

                //foreach (DataColumn l_dataColumn in l_dataTable.Columns)
                //{
                //    if (lv_fieldsInitialString == null)
                //    {
                //        lv_fieldsInitialString = "no " + l_dataTable + "1." + l_dataColumn + ".State_X";
                //    }
                //    else
                //    {
                //        lv_fieldsInitialString = lv_fieldsInitialString + " and no " + l_dataTable + "1." + l_dataColumn + ".State_X";
                //    }

                //}

                //l_TableListOfStrings.Add( "fact { all State_X: State, " + l_dataTable + "1 : this/" + l_dataTable.TableName + " | (not ( " + l_dataTable + "1 in State_X." + l_dataTable.TableName + " ) ) implies ( " + lv_fieldsInitialString + " ) }" );

                l_SchemaListOfStrings.Add("");
                l_SchemaListOfStrings.Add("//Define Table " + l_dataTable.TableName);
                l_SchemaListOfStrings = l_SchemaListOfStrings.Concat(l_TableListOfStrings).ToList();
            }


            l_SchemaListOfStrings.Add("//Declare Constant Values");
  
            foreach ( ConstantValueCuLeSet l_ConstantValueCuLeSet in m_ConstantValueCuLeSets )
            {

                l_SchemaListOfStrings.Add("one sig " + l_ConstantValueCuLeSet.ToAlloy(null) + " extends " + l_ConstantValueCuLeSet.GetTypeString() + "{}" ); 

                //l_SchemaListOfStrings.Add(l_ConstantValueCuLeSet.ToAlloy() + ": one " + l_ConstantValueCuLeSet.GetTypeString() + ",");

            }

            l_SchemaListOfStrings.Add("//Enforce Constant Values must maintain Ordering");

            foreach (ConstantValueCuLeSet l_ConstantValueCuLeSet1 in m_ConstantValueCuLeSets)
            {
                foreach (ConstantValueCuLeSet l_ConstantValueCuLeSet2 in m_ConstantValueCuLeSets)
                {

                    if (l_ConstantValueCuLeSet1 != l_ConstantValueCuLeSet2)
                    {
                        if (l_ConstantValueCuLeSet1.GetTypeString() == l_ConstantValueCuLeSet2.GetTypeString())
                        {

                            switch( l_ConstantValueCuLeSet1.CompareTo(l_ConstantValueCuLeSet2))
                            {

                                case 1:

                                    l_SchemaListOfStrings.Add("fact { gt [ " + l_ConstantValueCuLeSet1.ToAlloy(null) + " , " + l_ConstantValueCuLeSet2.ToAlloy(null) + " ] }");

                                    if (l_ConstantValueCuLeSet1.GetTypeString() != "string")
                                    {
                                        int lv_difference = ConstantValueCuLeSet.GetDifference(l_ConstantValueCuLeSet2, l_ConstantValueCuLeSet1);

                                        if (lv_difference > 0 && lv_difference <= m_maxAlloyGroupSize)
                                        {
                                            l_SchemaListOfStrings.Add("fact { #{ x:" + l_ConstantValueCuLeSet1.GetTypeString() + " | lt[x," + l_ConstantValueCuLeSet1.ToAlloy(null) + "] && gt[x," + l_ConstantValueCuLeSet2.ToAlloy(null) + "] } <= " + (lv_difference - 1).ToString() + " }"  ); 
                                        }

                                    }

                                    break;

                                case 0:

                                 l_SchemaListOfStrings.Add("fact { " + l_ConstantValueCuLeSet1.ToAlloy(null) + " = " + l_ConstantValueCuLeSet2.ToAlloy(null) + " } ");
                                 break;

                                case -1:

                                   // l_SchemaListOfStrings.Add("fact { lt [ " + l_ConstantValueCuLeSet1.ToAlloy() + " , " + l_ConstantValueCuLeSet2.ToAlloy() + " ] }");
                                    break;



                            }                                                       
                        }

                    }
                }

                // l_SchemaListOfStrings.Add("one sig " + l_ConstantValueCuLeSet.ToAlloy() + " extends " + l_ConstantValueCuLeSet.GetTypeString() + "{}");

                //l_SchemaListOfStrings.Add(l_ConstantValueCuLeSet.ToAlloy() + ": one " + l_ConstantValueCuLeSet.GetTypeString() + ",");

            }


            l_SchemaListOfStrings.Add("//Enforce Foreign Keys relations");

            foreach (DataRelation l_DataRelation in this.Relations)
            {

                 int l_numOfColoumns = l_DataRelation.ParentColumns.Count();

                string l_ChildTuple = l_DataRelation.ChildTable.TableName.ToString() + "1";
                string l_ParentTuple = l_DataRelation.ParentTable.TableName.ToString() + "1";

                string l_ColoumsString = "fact { all State_X : State, " + l_ChildTuple + ": " + "this/" + l_DataRelation.ChildTable.TableName + " , " + l_ParentTuple + ": " + "this/" + l_DataRelation.ParentTable.TableName + " | ";

                l_ColoumsString = l_ColoumsString + " ( ( " + l_ParentTuple + " in " + l_ChildTuple + "." + l_DataRelation.RelationName + ".State_X ) iff ( ";

                for (int i = 0; i < l_numOfColoumns; i++)
                {
                    if (i == 0)
                    {

                    }
                    else
                    {
                        l_ColoumsString = l_ColoumsString + " and ";
                    }

                    l_ColoumsString = l_ColoumsString + l_ChildTuple + "." + l_DataRelation.ChildColumns[i].ColumnName + ".State_X" + " = " + l_ParentTuple + "." + l_DataRelation.ParentColumns[i].ColumnName + ".State_X";

                }

                l_ColoumsString = l_ColoumsString + " ) ) }";

                l_SchemaListOfStrings.Add(l_ColoumsString);

                l_ColoumsString = "";

                l_ColoumsString = "fact { all State_X : State, " + l_ChildTuple + ": " + "State_X." + l_DataRelation.ChildTable.TableName + " | ";

                l_ColoumsString = l_ColoumsString + " ( " + l_ChildTuple + "." + l_DataRelation.RelationName + ".State_X  in  State_X." + l_DataRelation.ParentTable.TableName + " ) }";

                l_SchemaListOfStrings.Add(l_ColoumsString);

                l_ColoumsString = "fact { all State_X : State, " + l_ChildTuple + ": " + "State_X." + l_DataRelation.ChildTable.TableName + " | ";

                for (int i = 0; i < l_numOfColoumns; i++)
                {
                    if (i == 0)
                    {

                    }
                    else
                    {
                        l_ColoumsString = l_ColoumsString + " and ";
                    }


                   
                    l_ColoumsString = l_ColoumsString + " ( " + l_ChildTuple + "." + l_DataRelation.ChildColumns[i].ColumnName + ".State_X != none )";
                }

                l_ColoumsString = l_ColoumsString + " implies ( { " + l_ParentTuple + " : State_X." + l_DataRelation.ParentTable + " |";
 
                for( int i = 0; i < l_numOfColoumns; i++)
                {
                    if (i == 0)
                    {

                    }
                    else
                    {
                        l_ColoumsString = l_ColoumsString + " and ";
                    }


                    l_ColoumsString = l_ColoumsString + "( " + l_ParentTuple + "." + l_DataRelation.ParentColumns[i].ColumnName + ".State_X = " + l_ChildTuple + "." + l_DataRelation.ChildColumns[i].ColumnName + ".State_X )";

                }

                l_ColoumsString = l_ColoumsString + "} != none ) } ";

                    
               l_SchemaListOfStrings.Add(l_ColoumsString);

            }  //End Foreach( DataRelation l_DataRelation in this.Relations )

            l_SchemaListOfStrings.Add("");

            l_SchemaListOfStrings.Add("//Applicaion Business Logic");

            foreach (CuLeRuleGroup l_CuLeRuleGroup in m_CuLeRuleGroups)
            {
                l_SchemaListOfStrings.Add(l_CuLeRuleGroup.ToAlloyBusinessRule());
            }

            l_SchemaListOfStrings.Add("//Application Business Transactions");

            foreach (DataTable l_dataTable in this.Tables)
            {

                string lv_SetName = "InputSetOf" + l_dataTable.TableName;
                string lv_IndexName = l_dataTable.TableName + "1";

                l_SchemaListOfStrings.Add("pred No" + l_dataTable.TableName + "ChangeExcept[s,s' : State, " + lv_SetName + " : set " + "this/" + l_dataTable.TableName + " ] ");
                l_SchemaListOfStrings.Add("{");
                
                l_SchemaListOfStrings.Add("all " + lv_IndexName + " : " + l_dataTable.TableName + " - " + lv_SetName + " |(");

                string lv_AllFieldRestriction = null;

                foreach (DataColumn l_dataColumn in l_dataTable.Columns)
                {

                    string lv_FieldRestriction = lv_IndexName + "." + l_dataColumn.ColumnName + ".s = " + lv_IndexName + "." + l_dataColumn.ColumnName + ".s'";

                    if (lv_AllFieldRestriction == null)
                    {
                        lv_AllFieldRestriction = lv_FieldRestriction;
                    }
                    else
                    {
                        lv_AllFieldRestriction = lv_AllFieldRestriction + " and " + lv_FieldRestriction;
                    }

                }

                l_SchemaListOfStrings.Add(lv_AllFieldRestriction);

                l_SchemaListOfStrings.Add(")");
                l_SchemaListOfStrings.Add("}");

            }

            if (this.m_CuLeTransactions != null)
            {

                foreach (CuLeTransaction l_CuLeTransaction in this.m_CuLeTransactions)
                {

                    string l_TransactionString = l_CuLeTransaction.ToAlloyPred(this.Tables);
                    l_TransactionString = l_TransactionString.Replace("NEW_STATE", "s'");
                    l_TransactionString = l_TransactionString.Replace("OLD_STATE", "s");
                    l_SchemaListOfStrings.Add(l_TransactionString);
                }
            }

            l_SchemaListOfStrings.Add("pred MoveSystemTimeStamp[s,s' : State]");
            l_SchemaListOfStrings.Add("{");


            foreach (DataTable l_dataTable in this.Tables)
            {
                l_SchemaListOfStrings.Add("No" + l_dataTable.TableName + "ChangeExcept[s,s',none]");
            }

            l_SchemaListOfStrings.Add(" not( s.System_Date = last and s.System_Time = last) and ( ( s.System_Time != last and ( s'.System_Time = s.System_Time.next and s'.System_Date = s.System_Date)) or ( s.System_Time = last and ( s'.System_Time = first and s'.System_Date = s.System_Date.next)) ) ");

            
            l_SchemaListOfStrings.Add("}");



            l_SchemaListOfStrings.Add("fact Traces{");

            l_SchemaListOfStrings.Add("all s: State - last | let s' = s.next |");

            l_SchemaListOfStrings.Add("MoveSystemTimeStamp[s,s']");

            foreach( CuLeTransaction l_CuLeTransaction in this.m_CuLeTransactions)
            {
                l_SchemaListOfStrings.Add("or ( " + l_CuLeTransaction.ToAlloyTraces() + ")");
            }

            l_SchemaListOfStrings.Add("}");

            if (this.m_CuLeTransactions == null || this.m_CuLeTransactions.Count == 0)
            {
                l_SchemaListOfStrings.Add( "fact{ #this/States = 1 } ");
            }



            for (int i = 0; i < l_SchemaListOfStrings.Count; i++)
            {
                l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace(" int, ", " integer, ");
                l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace(" int ", " integer ");
                l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace("int{", "integer{");
                l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace(":int", ":integer");
                //l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace(" string, ", " String, ");
                //l_SchemaListOfStrings[i] = l_SchemaListOfStrings[i].Replace(" string ", " String ");
            }

            l_SchemaListOfStrings.Add("run { } for " + m_maxAlloyGroupSize.ToString());

            return l_SchemaListOfStrings;

        } // End Method ToAlloySchema()

        public enum RelationMultiplicity { OneToOne, OneToMany, ManyToOne, ManyToMany, LoneToOne, LoneToMany };

        public static RelationMultiplicity GetRelationMultiplicity(string i_RelationName, DataTableCollection i_Tables, DataRelationCollection i_Relations)
        {

            DataRelation l_DataRelation = i_Relations[i_Relations.IndexOf(i_RelationName)];

            

            if (l_DataRelation == null)
            {
                throw new System.ArgumentException("Relation Argument Error");
            }

            CuLeSetType.Multiplicity l_ParentMultiplicity;
            CuLeSetType.Multiplicity l_ChildMultiplicity;


            if (l_DataRelation.ParentKeyConstraint != null)
            {
                l_ParentMultiplicity = CuLeSetType.Multiplicity.single;
            }
            else
            {
                l_ParentMultiplicity = CuLeSetType.Multiplicity.many;
            }

            if (l_DataRelation.ChildKeyConstraint == null)
            {
                throw new System.ArgumentException("Relation Argument Error");
            }

            l_ChildMultiplicity = CuLeSetType.Multiplicity.many;

            if (!(i_Tables.Contains(l_DataRelation.ChildTable.TableName)))
            {
                throw new System.ArgumentException("Relation Argument Error, Child Table not found");
            }
            
            DataTable l_ChildTable = i_Tables[i_Tables.IndexOf(l_DataRelation.ChildTable)];

            foreach (Constraint l_constraint in l_ChildTable.Constraints)
            {
                if (l_constraint is UniqueConstraint)
                {
                    UniqueConstraint l_UniqueConstraint = (UniqueConstraint)l_constraint;

                    // if the related Child Coloumns include an Entire Unique Constraint, then the child is unique

                    bool lv_flag_all_columns_included_in_constraint = true;

                    foreach (DataColumn l_dataColumn in l_UniqueConstraint.Columns)
                    {
                        if (l_DataRelation.ChildColumns.Contains(l_dataColumn))
                        {
                        }
                        else
                        {
                            lv_flag_all_columns_included_in_constraint = false;
                        }

                    }

                    if (lv_flag_all_columns_included_in_constraint == true)
                    {
                        l_ChildMultiplicity = CuLeSetType.Multiplicity.single;
                    }

                }
            }

            bool lv_ChildIsOptional = false;

            foreach (DataColumn l_dataColumn in l_DataRelation.ChildColumns)
            {
                if (l_dataColumn.AllowDBNull == true)
                {
                    lv_ChildIsOptional = true;
                }
            }

            if (l_ChildMultiplicity == CuLeSetType.Multiplicity.single)
            {
                if (l_ParentMultiplicity == CuLeSetType.Multiplicity.single)
                {

                    if (lv_ChildIsOptional == true)
                    {
                        return RelationMultiplicity.LoneToOne;
                    }
                    else
                    {
                        return RelationMultiplicity.OneToOne;
                    }

                }
                else //(l_ParentMultiplicity == CuLeSetType.Multiplicity.many)
                {
                    return RelationMultiplicity.ManyToOne;
                }

            }
            else // (l_ChildMultiplicity == CuLeSetType.Multiplicity.many)
            {
                if (l_ParentMultiplicity == CuLeSetType.Multiplicity.single)
                {
                    if (lv_ChildIsOptional == true)
                    {
                        return RelationMultiplicity.LoneToMany;
                    }
                    else
                    {
                        return RelationMultiplicity.OneToMany;
                    }

                }
                else //(l_ParentMultiplicity == CuLeSetType.Multiplicity.many)
                {
                    return RelationMultiplicity.ManyToMany;
                }
            }


        } // End Method GetRelationMultiplicity()

        public List<DataSet> GenerateInstanceFromAlloyXML(string i_XmlString)
        {

            CuLeDataAlloyInstance l_CuLeDataAlloyInstance = new CuLeDataAlloyInstance((DataSet)this, i_XmlString);

            

            return l_CuLeDataAlloyInstance.DataSets;
        } // End Method GenerateInstanceFromAlloyXML

        private abstract class ConstantVariable
        {

            protected string m_varaibleName;

            public string VariableName
            {
                get
                {
                    return m_varaibleName;
                }
            }

        } // End abstract class ConstantVariable

        private class ConstantVariable_Date : ConstantVariable
        {

            private DateTime m_DateTime;

            public DateTime DateTimeValue
            {
                get
                {
                    return m_DateTime;
                }
            }

            private ConstantVariable_Date(string i_variableName)
            {

                m_varaibleName = i_variableName;

                char[] lv_yearChar = { i_variableName[22] , i_variableName[23], i_variableName[24], i_variableName[25] } ;
                char[] lv_monthChar = { i_variableName[19], i_variableName[20] } ;
                char[] lv_dayChar   = { i_variableName[16], i_variableName[17] } ;

                int lv_yearInt    = int.Parse(new string(lv_yearChar));
                int lv_monthInt   = int.Parse(new string(lv_monthChar));
                int lv_dayInt     = int.Parse(new string(lv_dayChar));

                m_DateTime = new DateTime(lv_yearInt, lv_monthInt, lv_dayInt); 

            }

            public static ConstantVariable_Date Create(string i_variableName)
            {
                if (Regex.IsMatch(i_variableName, "this/Const_date_??_??_????"))
                {

                    if (char.IsDigit(i_variableName[16])
                    && char.IsDigit(i_variableName[17])
                    && char.IsDigit(i_variableName[19])
                    && char.IsDigit(i_variableName[20])
                    && char.IsDigit(i_variableName[22])
                    && char.IsDigit(i_variableName[23])
                    && char.IsDigit(i_variableName[24])
                    && char.IsDigit(i_variableName[25]))
                    {

                        return new ConstantVariable_Date(i_variableName);
                    }
                }

                return null;
            }


        } // End Class ConstantVariable_Date

        private class ConstantVariable_Int
        {

        }
    }  // End Class CuLeDataSet


   
}  // End NameSpace CuLe


