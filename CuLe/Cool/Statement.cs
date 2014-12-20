using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{

    public enum SetOperator { IsSubsetOf, EQ, NE, GT, GE, ST, SE };
    public enum RuleOperator { AND, OR };
    public enum QuantifierMultiplicity { Single, Multiple };

    public class Statement
    {
        public ParseTreeNode m_id_simple;

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

    public class StatementList 
    {
        public List<CreateTableStatement> m_CreateTableStatementList ;
        public List<CuLeRuleStatement> m_CuLeRuleStatementList;
        public List<CuLeTransactionStatement> m_CuLeTransactionStatementList;

        public StatementList()
        {
            m_CreateTableStatementList = new List<CreateTableStatement>();
            m_CuLeRuleStatementList = new List<CuLeRuleStatement>();
            m_CuLeTransactionStatementList = new List<CuLeTransactionStatement>();
        }
    }

    public class CreateTableStatement : Statement
    {

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

    public class CuLeRuleStatement : Statement
    {

        public CuLeQuantifierList m_CuLeQuantifiers;
        public CuLeRuleDefList m_CuLeRuleDefList;

        private string m_name;

        public string Name
        {
            get
            {
                return m_name;
            }
        }

        public CuLeRuleStatement(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError) return;

            if (i_parseTreeNode.Term.Name != "CuLeRuleStmt" && i_parseTreeNode.Term.Name != "CuLeAssertion") return;

            m_id_simple = i_parseTreeNode;

            foreach (ParseTreeNode childNode in i_parseTreeNode.ChildNodes)
            {
                if (childNode.Term.Name == "CuLeRuleList")
                {
                    m_CuLeRuleDefList = new CuLeRuleDefList(childNode);
                }

                else if (childNode.Term.Name == "QuantifierStmt")
                {

                    m_CuLeQuantifiers = new CuLeQuantifierList(childNode);

                    m_CuLeQuantifiers.Add(new CuLeSystemField("Date", "System_Date"));
                    m_CuLeQuantifiers.Add(new CuLeSystemField("Time", "System_Time"));
                }
                else if (childNode.Term.Name == "CuLeRuleName")
                {

                    m_name = childNode.ChildNodes[1].ChildNodes[0].Token.Value.ToString();

                }
            }

        }



    }

    public class CuLeTransactionStatement : Statement
    {

        private CuLeQuantifierList m_ParametersDef;
        //private List<String> m_ChangingTableNames;
        private List<CuLeSetDef> m_ChangingSetDefs;

        private List<CuLeRuleStatement> m_PreconditionDef;
        private List<CuLeRuleStatement> m_ResultDef;
       
        //private CuLeRuleDefList m_PreconditionDef;
        //private CuLeRuleDefList m_ResultDef;
        private CuLeRuleDefList m_InputRestrictionDef;

        private string m_transactionName;

        public string TransactionName
        {
            get
            {
                return m_transactionName;
            }
        }



        public CuLeQuantifierList ParametersDef
        {
            get
            {
                return m_ParametersDef;
            }
        }

        //public List<String> ChangingTableNames
        //{
        //    get
        //    {
        //        return m_ChangingTableNames;
        //    }
        //}

        public List<CuLeSetDef> ChangingSetDefs
        {
            get
            {
                return m_ChangingSetDefs;
            }
        }


        public List<CuLeRuleStatement> PreconditionDef
        {
            get
            {
                return m_PreconditionDef;
            }
        }

        public List<CuLeRuleStatement> ResultDef
        {
            get
            {
                return m_ResultDef;
            }
        }

        public CuLeRuleDefList InputRestrictionDef
        {
            get
            {
                return m_InputRestrictionDef;
            }
        }

        public CuLeTransactionStatement(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError) return;

            if (i_parseTreeNode.Term.Name != "TransactionStmt") return;

            m_id_simple = i_parseTreeNode;

            if (i_parseTreeNode.ChildNodes == null || i_parseTreeNode.ChildNodes.Count != 11)
            {
                return;
            }

            // Transaction Name

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "Transaction")
            {
                return;
            }

            if (i_parseTreeNode.ChildNodes[1].Term.Name != "Id")
            {
                return;
            }
            else
            {
                if (i_parseTreeNode.ChildNodes[1].ChildNodes == null || i_parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name != "id_simple")
                {
                    return;
                }

                else
                {
                    m_transactionName = i_parseTreeNode.ChildNodes[1].ChildNodes[0].Token.ValueString;
                }
            }

            // Transaction Parameters

            if ( i_parseTreeNode.ChildNodes[3].Term.Name == "TransactionParameterList")
            {

                m_ParametersDef = new CuLeQuantifierList(i_parseTreeNode.ChildNodes[3]);

           }

            // Transaction Changing

            this.m_ChangingSetDefs = new List<CuLeSetDef>();

            if (i_parseTreeNode.ChildNodes[6].Term.Name == "Changing")
            {
                if (i_parseTreeNode.ChildNodes[6].ChildNodes[1].Term.Name == "ChangeObjectList")
                {
                    foreach (ParseTreeNode l_ParseTreeNodeChangeObject in i_parseTreeNode.ChildNodes[6].ChildNodes[1].ChildNodes)
                    {
                        if (l_ParseTreeNodeChangeObject.Term.Name == "ChangeObject")
                        {
                            if (l_ParseTreeNodeChangeObject.ChildNodes[0].Term.Name == "Set")
                            {
                                CuLeSetDef l_CuLeSetDef = new CuLeSetDef(l_ParseTreeNodeChangeObject.ChildNodes[0]);
                                this.m_ChangingSetDefs.Add(l_CuLeSetDef);
                            }
                        }

                    }
                   
                }
            }

            // Transaction PreCondition

            if (i_parseTreeNode.ChildNodes[7].Term.Name == "Precondition")
            {

                m_PreconditionDef = new List<CuLeRuleStatement>();

                if (i_parseTreeNode.ChildNodes[7].ChildNodes != null && i_parseTreeNode.ChildNodes[7].ChildNodes[1].Term.Name == "CuLeAssertionList")
                {
                    if (i_parseTreeNode.ChildNodes[7].ChildNodes[1].ChildNodes != null)
                    {
                        foreach (ParseTreeNode l_CuLeAssertionTreeNode in i_parseTreeNode.ChildNodes[7].ChildNodes[1].ChildNodes)
                        {
                            if (l_CuLeAssertionTreeNode.Term.Name == "CuLeAssertion")
                            {
                                CuLeRuleStatement l_CuLeAssertionStatement = new CuLeRuleStatement(l_CuLeAssertionTreeNode);
                                m_PreconditionDef.Add(l_CuLeAssertionStatement);
                            }
                        }
                    }


                }
            }

            // Transaction Input Restrictions

            if (i_parseTreeNode.ChildNodes[8].Term.Name == "Input_Restriction")
            {

                if (i_parseTreeNode.ChildNodes[8].ChildNodes != null && i_parseTreeNode.ChildNodes[8].ChildNodes[1].Term.Name == "CuLeRuleList")
                {
                    m_InputRestrictionDef = new CuLeRuleDefList(i_parseTreeNode.ChildNodes[8].ChildNodes[1]);
                }
            }


            // Transaction Result

            if (i_parseTreeNode.ChildNodes[9].Term.Name == "Result")
            {

                m_ResultDef = new List<CuLeRuleStatement>();

                if (i_parseTreeNode.ChildNodes[9].ChildNodes != null && i_parseTreeNode.ChildNodes[9].ChildNodes[1].Term.Name == "CuLeAssertionList")
                {
                    if (i_parseTreeNode.ChildNodes[9].ChildNodes[1].ChildNodes != null)
                    {
                        foreach (ParseTreeNode l_CuLeAssertionTreeNode in i_parseTreeNode.ChildNodes[9].ChildNodes[1].ChildNodes)
                        {
                            if (l_CuLeAssertionTreeNode.Term.Name == "CuLeAssertion")
                            {
                                CuLeRuleStatement l_CuLeAssertionStatement = new CuLeRuleStatement(l_CuLeAssertionTreeNode);
                                m_ResultDef.Add(l_CuLeAssertionStatement);
                            }
                        }
                    }

                                      
                }
            }


        }
    }

    public abstract class CuLeQuantifier
    {
        public ParseTreeNode m_id_simple;

        public abstract string Name { get; }
        public abstract string Signature { get; }

        public abstract string ToAlloy(string i_State);
        
        public abstract QuantifierMultiplicity QuantifierMultiplicity  { get; }

        public Token Token{
            get
            {
                return m_id_simple.FindToken();
            }

        }

        

        public static CuLeQuantifier CreateCuLeQuantifier(ParseTreeNode i_parseTreeNode)
        {
            string l_VariableName = null;
            string l_VariableType = null;

            bool l_FlagMultiValue = false;

            if (i_parseTreeNode.IsError == true) return null;
            if (i_parseTreeNode.Term.Name == "Quantifier")
            {

                if (i_parseTreeNode.ChildNodes == null) return null;

                if (i_parseTreeNode.ChildNodes.Count != 2) return null;

                if (i_parseTreeNode.ChildNodes[0].Term.Name != "TableName") return null;
                if (i_parseTreeNode.ChildNodes[1].Term.Name != "Tuple") return null;

                l_VariableType = i_parseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Text;
                l_VariableName = i_parseTreeNode.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Text;

            }

            else if (i_parseTreeNode.Term.Name == "TransactionParameter")
            {
                if( i_parseTreeNode.ChildNodes[0].Term.Name == "TransactionSingleValueParameter")
                {

                    l_FlagMultiValue = false;

                    if (i_parseTreeNode.ChildNodes[0].ChildNodes[0].Term.Name != "TransactionParameterType") return null;
                    if (i_parseTreeNode.ChildNodes[0].ChildNodes[1].Term.Name != "TransactionParameterName") return null;

                    l_VariableType = i_parseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Text;
                    l_VariableName = i_parseTreeNode.ChildNodes[0].ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Text;
                }

                else if (i_parseTreeNode.ChildNodes[0].Term.Name == "TransactionMultiValueParameter")
                {
                    l_FlagMultiValue = true;

                    if (i_parseTreeNode.ChildNodes[0].ChildNodes[0].Term.Name != "TransactionParameterType") return null;
                    if (i_parseTreeNode.ChildNodes[0].ChildNodes[2].Term.Name != "TransactionParameterName") return null;

                    l_VariableType = i_parseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Text;
                    l_VariableName = i_parseTreeNode.ChildNodes[0].ChildNodes[2].ChildNodes[0].ChildNodes[0].Token.Text;
                }
            }


            if (l_FlagMultiValue == false)
            {

                if (CuLe.CuLeBaseValues.Contains(l_VariableType))
                {
                    return new CuLeSingleBaseValueQuantifier(l_VariableType, l_VariableName, i_parseTreeNode);
                }
                else
                {

                    return new CuLeSingleTupleQuantifier(l_VariableType, l_VariableName, i_parseTreeNode);
                }
            }
            else
            {
                if (CuLe.CuLeBaseValues.Contains(l_VariableType))
                {
                    return new CuLeMultiBaseValueQuantifier(l_VariableType, l_VariableName, i_parseTreeNode);
                }
                else
                {

                    return new CuLeMultiTupleQuantifier(l_VariableType, l_VariableName, i_parseTreeNode);
                }

            }

          }

    }

    public abstract class CuLeBaseValueQuantifier : CuLeQuantifier
    {

        protected string m_ValueType;
        protected string m_VariableName;

        public override string Name
        {
            get
            {
                return m_VariableName;
            }
        }

        public override string Signature
        {
            get
            {
                return m_ValueType;
            }
        }
    }

    public class CuLeSingleBaseValueQuantifier : CuLeBaseValueQuantifier
    {   


        public override string ToAlloy(string i_State)
        {
            return (this.Name + ": " + this.Signature);
        }


        public override QuantifierMultiplicity QuantifierMultiplicity
        {
            get { return QuantifierMultiplicity.Single; }
        }

        public CuLeSingleBaseValueQuantifier(string i_ValueType, string i_VariableName, ParseTreeNode i_ParseTreeNode)
        {
            m_ValueType = i_ValueType;
            m_VariableName = i_VariableName;
            m_id_simple = i_ParseTreeNode;

        }
    }

    public class CuLeSystemField : CuLeSingleBaseValueQuantifier
    {
        public CuLeSystemField(string i_ValueType, string i_VariableName) : base(i_ValueType, i_VariableName, null)
        {            
            
        }

    }

    public class CuLeMultiBaseValueQuantifier : CuLeBaseValueQuantifier
    {


        public override string ToAlloy(string i_State)
        {
            return (this.Name + ": set " + this.Signature);
        }


        public override QuantifierMultiplicity QuantifierMultiplicity
        {
            get { return QuantifierMultiplicity.Multiple; }
        }

        public CuLeMultiBaseValueQuantifier(string i_ValueType, string i_VariableName, ParseTreeNode i_ParseTreeNode)
        {
            m_ValueType = i_ValueType;
            m_VariableName = i_VariableName;
            m_id_simple = i_ParseTreeNode;

        }
    }

    public abstract class CuLeTupleQuantifier : CuLeQuantifier
    {
        protected String m_TableName;
        protected String m_Tuple;

        public override string Name
        {
            get
            {
                return m_Tuple;
            }
        }

        public override string Signature
        {
            get
            {
                return m_TableName;
            }
        }
    }


    public class CuLeSingleTupleQuantifier : CuLeTupleQuantifier
    {
        public override string ToAlloy(string i_State)
        {
            if (i_State != null)
            {
                return (this.Name + ": " + i_State + "." + this.Signature);
            }
            else
            {
                return (this.Name + ": " + "this/" + this.Signature);
            }
        }


        public override QuantifierMultiplicity QuantifierMultiplicity
        {
            get { return QuantifierMultiplicity.Single; }
        }


        public CuLeSingleTupleQuantifier(string i_TableName, string i_TupleName, ParseTreeNode i_ParseTreeNode)
        {
            m_TableName = i_TableName;
            m_Tuple = i_TupleName;
            m_id_simple = i_ParseTreeNode;

        }
        
    }

    public class CuLeMultiTupleQuantifier : CuLeTupleQuantifier
    {
        public override string ToAlloy(string i_State)
        {
            if (i_State != null)
            {
                return (this.Name + ": set " + i_State + "." + this.Signature);
            }
            else
            {
                return (this.Name + ": set " + "this/" + this.Signature);
            }
        }


        public override QuantifierMultiplicity QuantifierMultiplicity
        {
            get { return QuantifierMultiplicity.Multiple; }
        }

        public CuLeMultiTupleQuantifier(string i_TableName, string i_TupleName, ParseTreeNode i_ParseTreeNode)
        {
            m_TableName = i_TableName;
            m_Tuple = i_TupleName;
            m_id_simple = i_ParseTreeNode;

        }

    }

    public class CuLeQuantifierList : List<CuLeQuantifier>
    {

        public CuLeQuantifierList(List<CuLeQuantifier> i_CuLeQuantifierList)
        {
            if (i_CuLeQuantifierList != null)
            {
                foreach (CuLeQuantifier l_CuLeQuantifier in i_CuLeQuantifierList)
                {
                    this.Add(l_CuLeQuantifier);
                }
            }
        }

        public CuLeQuantifierList(ParseTreeNode i_parseTreeNode)
        {

            CuLeQuantifier l_CuLeQuantifier;

            if (i_parseTreeNode.IsError == true) return;

            if (i_parseTreeNode.Term.Name == "QuantifierStmt")
            {
                if (i_parseTreeNode.ChildNodes == null) return;
                if (i_parseTreeNode.ChildNodes.Count != 4) return;
                if (i_parseTreeNode.ChildNodes[2].Term.Name != "QuantifierList") return;
                if (i_parseTreeNode.ChildNodes[2].ChildNodes == null) return;

                foreach (ParseTreeNode childNode in i_parseTreeNode.ChildNodes[2].ChildNodes)
                {
                    l_CuLeQuantifier = CuLeQuantifier.CreateCuLeQuantifier(childNode);

                    if (l_CuLeQuantifier != null)
                    {
                        if (l_CuLeQuantifier.m_id_simple != null)
                        {
                            this.Add(l_CuLeQuantifier);
                        }
                    }

                }
            }
            else if (i_parseTreeNode.Term.Name == "TransactionParameterList")
            {
                if (i_parseTreeNode.ChildNodes == null) return;

                foreach (ParseTreeNode l_ParameterParseTreeNode in i_parseTreeNode.ChildNodes)
                {
                    if (l_ParameterParseTreeNode.Term.Name == "TransactionParameter")
                    {
                        l_CuLeQuantifier = CuLeQuantifier.CreateCuLeQuantifier(l_ParameterParseTreeNode);

                        if (l_CuLeQuantifier != null)
                        {
                            if (l_CuLeQuantifier.m_id_simple != null)
                            {
                                this.Add(l_CuLeQuantifier);
                            }
                        }

                    }
                }
            }

            else if (i_parseTreeNode.Term.Name == "ChangeObjectList")
            {
                if (i_parseTreeNode.ChildNodes == null) return;

                foreach (ParseTreeNode l_ChangeObjectParseTreeNode in i_parseTreeNode.ChildNodes)
                {
                    if (l_ChangeObjectParseTreeNode.Term.Name == "ChangeObject")
                    {
                        l_CuLeQuantifier = CuLeQuantifier.CreateCuLeQuantifier(l_ChangeObjectParseTreeNode);

                        if (l_CuLeQuantifier != null)
                        {
                            if (l_CuLeQuantifier.m_id_simple != null)
                            {
                                this.Add(l_CuLeQuantifier);
                            }
                        }

                    }
                }
            }
        }

        public bool Contains(string name)
        {

           for (int i = 0; i < this.Count; i++)
            {
                
               if (this[i].Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public CuLeQuantifier GetQuantifier(string name)
        {
            for (int i = 0; i < this.Count; i++)
            {

                if (this[i].Name == name)
                {
                    return this[i];
                }
            }

            return null;
        }       

    }

    public abstract class CuLeDef
    {
        protected ParseTreeNode m_parseTreeNode;

       // public enum SetOperator{ IsSubsetOf , EQ , NE, GT, GE, ST, SE};
       // public enum RuleOperator{ AND, OR };


        public SourceLocation SourceLocation
        {
            get
            {
                return this.GetLocation(m_parseTreeNode);
            }
        }

        public ParseTreeNode ParseTreeNode
        {
            get
            {
                return m_parseTreeNode;
            }
        }

        private SourceLocation GetLocation(ParseTreeNode i_parseTreeNode)
        {

            if (i_parseTreeNode.Token != null)
            {
                return i_parseTreeNode.Token.Location;
            }
            else if (i_parseTreeNode.ChildNodes != null)
            {
                return GetLocation(i_parseTreeNode.ChildNodes[0]);
            }
            else
            {
                return new SourceLocation();

            }

        }

        public static SetOperator GetSetOperator(string i_string)
        {

            switch (i_string)
            {
                case "SetOperator_IN":

                    return SetOperator.IsSubsetOf;

                case "SetOperator_NE":

                    return SetOperator.NE;

                case "SetOperator_EQ":

                    return SetOperator.EQ;

                case "SetOperator_ST":

                    return SetOperator.ST;

                case "SetOperator_SE":

                    return SetOperator.SE;

                case "SetOperator_GT":

                    return SetOperator.GT;

                case "SetOperator_GE":

                    return SetOperator.GE;

                default:

                    throw new ArgumentException("Set Operator Definition Error");

            }
        }

    }

    public abstract class CuLeRuleDef : CuLeDef
    {
        //protected ParseTreeNode m_id_simple;    
        
    }

    public class SetOperatorCuLeRuleDef : CuLeRuleDef
    {
        private SetOperator m_SetOperator;
        private CuLeSetDef m_SetDefA;
        private CuLeSetDef m_SetDefB;

        public SetOperator SetOperator
        {
            get
            {
                return m_SetOperator;
            }
        }

        public CuLeSetDef SetDefA()
        {
            return m_SetDefA;
        }

        public CuLeSetDef SetDefB()
        {
            return m_SetDefB;
        }


        public SetOperatorCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 3) throw new ArgumentException("Rule Definition Error");

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "Set") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[1].Term.Name != "SetOperator") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[2].Term.Name != "Set") throw new ArgumentException("Rule Definition Error");


            m_SetOperator = CuLeDef.GetSetOperator(i_parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name);
            m_parseTreeNode = i_parseTreeNode;

            m_SetDefA = new CuLeSetDef(i_parseTreeNode.ChildNodes[0]);
            m_SetDefB = new CuLeSetDef(i_parseTreeNode.ChildNodes[2]);

        }
    }

    public class RuleOperatorCuLeRuleDef : CuLeRuleDef
    {
        private RuleOperator m_RuleOperator;

        private CuLeRuleDef m_CuLeRuleDef_A;
        private CuLeRuleDef m_CuLeRuleDef_B;

        public RuleOperator RuleOperator
        {
            get
            {
                return m_RuleOperator;
            }
        }
        public CuLeRuleDef CuLeRuleDef_A()
        {
            return m_CuLeRuleDef_A;
        }

        public CuLeRuleDef CuLeRuleDef_B()
        {
            return m_CuLeRuleDef_B;
        }

        public RuleOperatorCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 3) throw new ArgumentException("Rule Definition Error");

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "CuLeRule" || i_parseTreeNode.ChildNodes[2].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");

            switch (i_parseTreeNode.ChildNodes[1].Term.Name)
            {
                case "AND":

                    m_RuleOperator = RuleOperator.AND;
                    break;

                case "OR":

                    m_RuleOperator = RuleOperator.OR;
                    break;
                                  
                default:

                    throw new ArgumentException("Rule Definition Error");

            }

            m_parseTreeNode = i_parseTreeNode;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            if (l_CuLeRuleDefFactory == null)
            {
                throw new ArgumentException("Rule Definition Error");
            }

            m_CuLeRuleDef_A = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[0]);
            m_CuLeRuleDef_B = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[2]);
    
        }        

    }

    public class NegationCuLeRuleDef : CuLeRuleDef
    {
        private CuLeRuleDef m_CuLeRuleDef_A;

        public CuLeRuleDef CuLeRuleDef_A()
        {
            return m_CuLeRuleDef_A;
        }

        public NegationCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 2) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[0].Term.Name != "NOT") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[1].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
                        
           
            m_parseTreeNode = i_parseTreeNode;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            if (l_CuLeRuleDefFactory == null)
            {
                throw new ArgumentException("Rule Definition Error");
            }

            m_CuLeRuleDef_A = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[1]);
        }

    }

    public abstract class ConditionalCuLeRuleDef : CuLeRuleDef
    {
    }

    public abstract class ImplicationCuLeRuleDef : ConditionalCuLeRuleDef
    {

        protected CuLeRuleDef m_CuLeRuleDef_IF;
        protected CuLeRuleDef m_CuLeRuleDef_THEN;
  

        public CuLeRuleDef CuLeRuleDef_IF()
        {
            return m_CuLeRuleDef_IF;
        }

        public CuLeRuleDef CuLeRuleDef_THEN()
        {
            return m_CuLeRuleDef_THEN;
        }

    }
    
    public class IF_ImplicationCuLeRuleDef:  ImplicationCuLeRuleDef
    {


        public IF_ImplicationCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 4 ) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[0].Term.Name != "IF") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[1].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[2].Term.Name != "THEN") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[3].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
                      
           
            m_parseTreeNode = i_parseTreeNode;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            if (l_CuLeRuleDefFactory == null)
            {
                throw new ArgumentException("Rule Definition Error");
            }

            m_CuLeRuleDef_IF = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[1]);
            m_CuLeRuleDef_THEN = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[3]);

        }

    }

    public class IF_ELSE_ImplicationCuLeRuleDef : ImplicationCuLeRuleDef
    {
        protected CuLeRuleDef m_CuLeRuleDef_ELSE;

        public CuLeRuleDef CuLeRuleDef_ELSE()
        {
            return m_CuLeRuleDef_ELSE;
        } 

        public IF_ELSE_ImplicationCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 6) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[0].Term.Name != "IF") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[1].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[2].Term.Name != "THEN") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[3].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[4].Term.Name != "ELSE") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes[5].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");


            m_parseTreeNode = i_parseTreeNode;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            if (l_CuLeRuleDefFactory == null)
            {
                throw new ArgumentException("Rule Definition Error");
            }

            m_CuLeRuleDef_IF = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[1]);
            m_CuLeRuleDef_THEN = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[3]);
            m_CuLeRuleDef_ELSE = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[5]);

        }

    }

    public class EquivalenceCuLeRuleDef : ConditionalCuLeRuleDef
    {
        protected CuLeRuleDef m_CuLeRuleDef_A;
        protected CuLeRuleDef m_CuLeRuleDef_B;

        public CuLeRuleDef CuLeRuleDef_A()
        {
            return m_CuLeRuleDef_A;
        }

        public CuLeRuleDef CuLeRuleDef_B()
        {
            return m_CuLeRuleDef_B;
        }

        public EquivalenceCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {
            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes.Count != 3) throw new ArgumentException("Rule Definition Error");

             if (i_parseTreeNode.ChildNodes[0].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
             if (i_parseTreeNode.ChildNodes[1].Term.Name != "IFF") throw new ArgumentException("Rule Definition Error");
             if (i_parseTreeNode.ChildNodes[2].Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");


            m_parseTreeNode = i_parseTreeNode;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            if (l_CuLeRuleDefFactory == null)
            {
                throw new ArgumentException("Rule Definition Error");
            }

            m_CuLeRuleDef_A = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[0]);
            m_CuLeRuleDef_B = l_CuLeRuleDefFactory.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[2]);
    
        }        


    }
   

    public class CuLeRuleDefFactory
    {

        public CuLeRuleDef CreateCuLeRuleDef(ParseTreeNode i_parseTreeNode)
        {

            if (i_parseTreeNode.IsError == true) throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.Term.Name != "CuLeRule") throw new ArgumentException("Rule Definition Error");
            if (i_parseTreeNode.ChildNodes == null) throw new ArgumentException("Rule Definition Error");

            if( i_parseTreeNode.ChildNodes.Count == 2)
            {
                if (   i_parseTreeNode.ChildNodes[0].Term.Name == "NOT" 
                    && i_parseTreeNode.ChildNodes[1].Term.Name == "CuLeRule")
                {
                    return new NegationCuLeRuleDef(i_parseTreeNode);
                }
            }

            if ( i_parseTreeNode.ChildNodes.Count == 3)
            {

                if (i_parseTreeNode.ChildNodes[0].Term.Name == "("
                    && i_parseTreeNode.ChildNodes[1].Term.Name == "CuLeRule"
                    && i_parseTreeNode.ChildNodes[2].Term.Name == ")"
                   )
                {
                    return this.CreateCuLeRuleDef(i_parseTreeNode.ChildNodes[1]);

                }

                if(      i_parseTreeNode.ChildNodes[0].Term.Name == "Set"
                      && i_parseTreeNode.ChildNodes[1].Term.Name == "SetOperator"
                      && i_parseTreeNode.ChildNodes[2].Term.Name == "Set"
                    )
                {
                    return new SetOperatorCuLeRuleDef(i_parseTreeNode);
                }

                if (     i_parseTreeNode.ChildNodes[0].Term.Name == "CuLeRule"
                      && i_parseTreeNode.ChildNodes[2].Term.Name == "CuLeRule"
                    )
                {
                    if(i_parseTreeNode.ChildNodes[1].Term.Name == "AND" || i_parseTreeNode.ChildNodes[1].Term.Name == "OR" )
                    {
                        return new RuleOperatorCuLeRuleDef(i_parseTreeNode);
                    }
                    if( i_parseTreeNode.ChildNodes[1].Term.Name == "IFF")
                    {
                        return new EquivalenceCuLeRuleDef(i_parseTreeNode);
                    }
                }
            }

            if (i_parseTreeNode.ChildNodes.Count == 4)
            {
                if (   i_parseTreeNode.ChildNodes[0].Term.Name == "IF" 
                    && i_parseTreeNode.ChildNodes[1].Term.Name == "CuLeRule" 
                    && i_parseTreeNode.ChildNodes[2].Term.Name == "THEN"
                    && i_parseTreeNode.ChildNodes[3].Term.Name == "CuLeRule"
                    )
                {
                    return new IF_ImplicationCuLeRuleDef(i_parseTreeNode);
                }
            }

            if (i_parseTreeNode.ChildNodes.Count == 6)
            {
                if (   i_parseTreeNode.ChildNodes[0].Term.Name == "IF"
                    && i_parseTreeNode.ChildNodes[1].Term.Name == "CuLeRule"
                    && i_parseTreeNode.ChildNodes[2].Term.Name == "THEN"
                    && i_parseTreeNode.ChildNodes[3].Term.Name == "CuLeRule"
                    && i_parseTreeNode.ChildNodes[4].Term.Name == "ELSE"
                    && i_parseTreeNode.ChildNodes[5].Term.Name == "CuLeRule"
                    )
                {
                    return new IF_ELSE_ImplicationCuLeRuleDef(i_parseTreeNode);
                }
            }

            throw new ArgumentException("Rule Definition Error");

        }

    }




    public class CuLeRuleDefList  : List<CuLeRuleDef>
    {

        public CuLeRuleDefList(ParseTreeNode i_parseTreeNode)
        {

            CuLeRuleDef l_CuLeRuleDef;
            if (i_parseTreeNode.IsError == true) return;
            if (i_parseTreeNode.Term.Name != "CuLeRuleList") return;

            CuLeRuleDefFactory l_CuLeRuleDefFactory = new CuLeRuleDefFactory();

            foreach( ParseTreeNode childNode in i_parseTreeNode.ChildNodes )
            {
                if (childNode.Term.Name == "CuLeRule")
                {

                    l_CuLeRuleDef = l_CuLeRuleDefFactory.CreateCuLeRuleDef(childNode); 

                    if (l_CuLeRuleDef != null)
                    {
                            this.Add(l_CuLeRuleDef);
                    }
                }
            }
        }
    }

    public class CuLeSetDef : CuLeDef
    {
        public enum SetOperation { Union, Intersect, Minus };

        public CuLeSetDef(ParseTreeNode i_parseTreeNode)
        {
            m_parseTreeNode = i_parseTreeNode;
        }


    }

    public class WhereClauseDef : CuLeDef
    {

        private WhereExpressionDef m_WhereExpressionDef;

        public WhereExpressionDef WhereExpressionDef
        {
            get
            {
                return m_WhereExpressionDef;
            }
        }

        public WhereClauseDef(ParseTreeNode i_parseTreeNode)
        {
            m_parseTreeNode = i_parseTreeNode;

            if (i_parseTreeNode.Term.Name != "WhereClause")
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }

            if (i_parseTreeNode.ChildNodes.Count != 3)
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "( WHERE")
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[2].Term.Name != ")")
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[1].Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[1].ChildNodes.Count != 3)
            {
                throw new System.ArgumentException("Where Clause Definition Error");
            }


            switch (i_parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name)
            {
                case "WhereExpression":

                    m_WhereExpressionDef = new OperatorWhereExpressionDef(i_parseTreeNode.ChildNodes[1]);

                    break;

                case "ArgumentName":

                    m_WhereExpressionDef = new BinaricWhereExpressionDef(i_parseTreeNode.ChildNodes[1]);
                    break;

                default:

                    throw new System.ArgumentException("Where Clause Definition Error");
            }

        }
    }

    

    public abstract class WhereExpressionDef : CuLeDef
    {

        public static WhereExpressionDef CreateWhereExpressionDef( ParseTreeNode i_parseTreeNode)
        {

             if (i_parseTreeNode.Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes.Count != 3)
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].Term.Name == "ArgumentName")
            {

                return new BinaricWhereExpressionDef(i_parseTreeNode);
            }

            if( i_parseTreeNode.ChildNodes[0].Term.Name == "WhereExpression")
            {
                return new OperatorWhereExpressionDef(i_parseTreeNode);
            }

            throw new System.ArgumentException("Where Expression Definition Error");

        }

    }

    public class BinaricWhereExpressionDef : WhereExpressionDef
    {

        private string m_AttrbuteName;
        private CuLeSetDef m_CuLeSetDef;
        private SetOperator m_SetOperator;

        public string AttributeName
        {
            get
            {
                return m_AttrbuteName;
            }
        }

        public CuLeSetDef CuLeSetDef
        {
            get
            {
                return m_CuLeSetDef;
            }
        }

        public SetOperator SetOperator
        {
            get
            {
                return m_SetOperator;
            }
        }

        public BinaricWhereExpressionDef(ParseTreeNode i_parseTreeNode)
        {

            if (i_parseTreeNode.Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes.Count != 3)
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "ArgumentName")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[1].Term.Name != "SetOperator")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[2].Term.Name != "Set")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].ChildNodes[0].Term.Name != "Id")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Term.Name != "id_simple" )
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            m_AttrbuteName = i_parseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Value.ToString();
            m_SetOperator = CuLeDef.GetSetOperator(i_parseTreeNode.ChildNodes[1].ChildNodes[0].Term.Name);
            m_CuLeSetDef = new CuLeSetDef(i_parseTreeNode.ChildNodes[2]);
        }

    }


    public class OperatorWhereExpressionDef : WhereExpressionDef
    {

        private RuleOperator m_RuleOperator;

        private WhereExpressionDef m_WhereExpressionDefA;
        private WhereExpressionDef m_WhereExpressionDefB;

        public RuleOperator RuleOperator
        {
            get
            {
                return m_RuleOperator;
            }
        }

        public WhereExpressionDef WhereExpressionDefA
        {
            get
            {
                return m_WhereExpressionDefA;
            }
        }

        public WhereExpressionDef WhereExpressionDefB
        {
            get
            {
                return m_WhereExpressionDefB;
            }
        }


        public OperatorWhereExpressionDef(ParseTreeNode i_parseTreeNode)
         {
            if (i_parseTreeNode.Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes.Count != 3)
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[0].Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[1].Term.Name != "AND" && i_parseTreeNode.ChildNodes[1].Term.Name != "OR")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            if (i_parseTreeNode.ChildNodes[2].Term.Name != "WhereExpression")
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }

            m_WhereExpressionDefA = CreateWhereExpressionDef(i_parseTreeNode.ChildNodes[0]);
            m_WhereExpressionDefB = CreateWhereExpressionDef(i_parseTreeNode.ChildNodes[2]);


            if (i_parseTreeNode.ChildNodes[1].Term.Name == "AND")
            {
                m_RuleOperator = RuleOperator.AND;
            }

            else if (i_parseTreeNode.ChildNodes[1].Term.Name == "OR")
            {
                m_RuleOperator = RuleOperator.OR;
            }

            else
            {
                throw new System.ArgumentException("Where Expression Definition Error");
            }        

         }

    }


}









