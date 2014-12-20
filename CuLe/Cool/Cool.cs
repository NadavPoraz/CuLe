using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{

    public class CuLe
    {

        private Parser m_parser;

        public ParseTree m_ParseTree;

        public CuLeDataSet m_DataSet;

        private LanguageData m_language;

        public List<ErrorMessage> m_ErrorMessages;

        public static List<String> CuLeBaseValues = new List<String>{"int", "integer", "time", "date", "string", "double"};


        public CuLe()
        {
            Grammar myCuLeGrammar = new CuLeGrammar();
            m_language = new LanguageData(myCuLeGrammar);
            m_parser = new Parser(m_language);
            m_parser.Context.TracingEnabled = true;

        }

        public void Parse(string i_sourceCode)
        {

            m_ParseTree = m_parser.Parse(i_sourceCode);

        }

        public bool CreateDataSet()
        {
            if (m_ParseTree == null || m_ParseTree.Root == null) return false;

            StatementList l_StatementList = GetStatementList();

            if (m_ErrorMessages != null)
            {

                m_ErrorMessages.Clear();

            }

            m_DataSet = new CuLeDataSet(l_StatementList);

            m_ErrorMessages = m_DataSet.m_ErrorMessages;

            if (m_ErrorMessages != null)
            {

                if (m_ErrorMessages.Count != 0)
                {
                    if (m_DataSet != null)
                    {
                        m_DataSet.Clear();
                    }
                    return false;
                }
            }



            return true;

        }

        private StatementList GetStatementList()
        {


            if (m_ParseTree == null || m_ParseTree.Root == null) return null;

            StatementList o_StatementList = new StatementList();

            foreach (ParseTreeNode parseNode_stmtLine in m_ParseTree.Root.ChildNodes)
            {

                if (parseNode_stmtLine.IsError == false && parseNode_stmtLine.Term.Name == "stmtLine")
                {

                    foreach (ParseTreeNode parseNode_stmt in parseNode_stmtLine.ChildNodes)
                    {
                        if (parseNode_stmt.IsError == false && parseNode_stmt.Term.Name == "stmt")
                        {

                            foreach (ParseTreeNode parseNode_Statement in parseNode_stmt.ChildNodes)
                            {
                                if (parseNode_Statement.IsError == false && parseNode_Statement.Term.Name == "createTableStmt")
                                {

                                    CreateTableStatement l_createTableStatement = new CreateTableStatement(parseNode_Statement);

                                    if (l_createTableStatement.m_id_simple != null)
                                    {
                                        o_StatementList.m_CreateTableStatementList.Add(l_createTableStatement);
                                    }

                                }

                                else if (parseNode_Statement.IsError == false && parseNode_Statement.Term.Name == "CuLeRuleStmt")
                                {

                                    CuLeRuleStatement l_CuLeRuleStatement = new CuLeRuleStatement(parseNode_Statement);

                                    if (l_CuLeRuleStatement.m_id_simple != null)
                                    {
                                        o_StatementList.m_CuLeRuleStatementList.Add(l_CuLeRuleStatement);
                                    }

                                }

                                else if (parseNode_Statement.IsError == false && parseNode_Statement.Term.Name == "TransactionStmt")
                                {
                                    CuLeTransactionStatement l_CuLeTransactionStatement = new CuLeTransactionStatement(parseNode_Statement);

                                    if (l_CuLeTransactionStatement.m_id_simple != null)
                                    {
                                        o_StatementList.m_CuLeTransactionStatementList.Add(l_CuLeTransactionStatement);
                                    }

                                }

                                    

                            }   // End Foreach createTableStmt

                        }
                    } // End Foreach stmt
                }
            }  // End Foreach stmtLine



            return o_StatementList;


        }




        public bool isValid(string i_sourceCode)
        {

            ParseTree parseTree;
            ParseTreeNode parseTreeRoot;

            parseTree = m_parser.Parse(i_sourceCode);
            parseTreeRoot = parseTree.Root;


            //parseTree.ParserMessages


            return parseTreeRoot != null;

        }

        public bool CanParse()
        {
            return (m_parser != null && m_parser.Language.CanParse());
        }

        public LanguageData GetLanguage()
        {
            return m_language;
        }

    }






       

        public class ErrorMessage
        {
            private String m_MessageText;
            private SourceLocation m_Location;

            public ErrorMessage(string messageText, SourceLocation sourceLocation)
            {
                m_MessageText = messageText;
                m_Location = sourceLocation;
            }

            public string MessageText()
            {
                return m_MessageText;
            }

            public SourceLocation SourceLocation()
            {
                return m_Location;
            }
        }

    }




    

