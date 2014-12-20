using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CuLe
{
    public class CuLeGrammar : Grammar
    {

        public CuLeGrammar()
            : base(false) // Case Insensitive
        {

            //Terminals
            var comment = new CommentTerminal("comment", "/*", "*/");
            var lineComment = new CommentTerminal("line_comment", "--", "\n", "\r\n");

            var Id_simple = TerminalFactory.CreateSqlExtIdentifier(this, "id_simple"); //covers normal identifiers (abc) and quoted id's ([abc d], "abc d")

            NonGrammarTerminals.Add(comment);
            NonGrammarTerminals.Add(lineComment);

            var CREATE = ToTerm("CREATE");
            var NULL = ToTerm("NULL");
            var NOT = ToTerm("NOT");
            var UNIQUE = ToTerm("UNIQUE");
            var WITH = ToTerm("WITH");
            var TABLE = ToTerm("TABLE");

            var FOREACH = ToTerm("FOREACH");

            var COLUMN = ToTerm("COLUMN");

            var CONSTRAINT = ToTerm("CONSTRAINT");

            var KEY = ToTerm("KEY");
            var PRIMARY = ToTerm("PRIMARY");

            var dot = ToTerm(".");
            var comma = ToTerm(",");
            var number = new NumberLiteral("number");

            //Non-terminals

            var Id = new NonTerminal("Id");
            var stmt = new NonTerminal("stmt");
            var createTableStmt = new NonTerminal("createTableStmt");
            
            var fieldDef = new NonTerminal("fieldDef");
            var fieldDefList = new NonTerminal("fieldDefList");
            var typeName = new NonTerminal("typeName");
            var typeSpec = new NonTerminal("typeSpec");
            var typeParamsOpt = new NonTerminal("typeParams");
            var constraintDef = new NonTerminal("constraintDef");
            var constraintListOpt = new NonTerminal("constraintListOpt");
            var constraintTypeOpt = new NonTerminal("constraintTypeOpt");
            var idlist = new NonTerminal("idlist");
            var idlistPar = new NonTerminal("idlistPar");
            var stmtList = new NonTerminal("stmtList");
            var stmtLine = new NonTerminal("stmtLine");
            var semiOpt = new NonTerminal("semiOpt");
            var nullSpecOpt = new NonTerminal("nullSpecOpt");

            var CuLeRule = new NonTerminal("CuLeRule");
            var CuLeRuleList = new NonTerminal("CuLeRuleList");
            var CuLeRuleStmt = new NonTerminal("CuLeRuleStmt");
            var CuLeRuleName = new NonTerminal("CuLeRuleName");

            var Set = new NonTerminal("Set");
            var Attribute = new NonTerminal("Attribute");
            var Tuple = new NonTerminal("Tuple");
            var TupleList = new NonTerminal("TupleList");
            var Constant = new NonTerminal("Constant");
            
            var Quantifier = new NonTerminal("Quantifier");
            var QuantifierList = new NonTerminal("QuantifierList");
            var QuantifierStmt = new NonTerminal("QuantifierStmt");


            var TransactionParameter = new NonTerminal("TransactionParameter");
            var TransactionSingleValueParameter = new NonTerminal("TransactionSingleValueParameter");
           // var TransactionMultiValueParameter = new NonTerminal("TransactionMultiValueParameter");
            var TransactionParameterName = new NonTerminal("TransactionParameterName");
            var TransactionParameterType = new NonTerminal("TransactionParameterType");
            var TransactionParameterList = new NonTerminal("TransactionParameterList");
    
            var TableName = new NonTerminal("TableName");

            var ArgumentName = new NonTerminal("ArgumentName");


            var Multiplicity = new NonTerminal("Multiplicity");
            var Cardinality_1to1 = new NonTerminal("Cardinality_1to1");
            var Cardinality_1ton = new NonTerminal("Cardinality_1ton");
            var Cardinality_nton = new NonTerminal("Cardinality_nton");

            var SetOperator = new NonTerminal("SetOperator");

            var SetOperator_IN = new NonTerminal("SetOperator_IN");
            var SetOperator_EQ = new NonTerminal("SetOperator_EQ");
            var SetOperator_NE = new NonTerminal("SetOperator_NE");
            var SetOperator_ST = new NonTerminal("SetOperator_ST");
            var SetOperator_GT = new NonTerminal("SetOperator_GT");
            var SetOperator_GE = new NonTerminal("SetOperator_GE");
            var SetOperator_SE = new NonTerminal("SetOperator_SE");

            var WhereClause     = new NonTerminal("WhereClause");
            var WhereExpression = new NonTerminal("WhereExpression");

            var TransactionStmt = new NonTerminal("TransactionStmt");
            var Changing = new NonTerminal("Changing");
            var Precondition = new NonTerminal("Precondition");
            var Input_Restriction = new NonTerminal("Input_Restriction");
            var Result = new NonTerminal("Result");
            var ChangeObject = new NonTerminal("ChangeObject");
            var ChangeObjectList = new NonTerminal("ChangeObjectList");

            var CuLeAssertion = new NonTerminal("CuLeAssertion");
            var CuLeAssertionList = new NonTerminal("CuLeAssertionList");

            var State = new NonTerminal("State"); 




            //BNF Rules
            this.Root = stmtList;
            stmtLine.Rule = stmt + semiOpt;
            semiOpt.Rule = Empty | ";";
            stmtList.Rule = MakePlusRule(stmtList, stmtLine);

            //ID
            //Id.Rule = MakePlusRule(Id, dot, Id_simple);
            Id.Rule = Id_simple;


            stmt.Rule = createTableStmt | CuLeRuleStmt | TransactionStmt;
            //Create table
            createTableStmt.Rule = CREATE + TABLE + Id + "(" + fieldDefList + ")" + constraintListOpt;
            fieldDefList.Rule = MakePlusRule(fieldDefList, comma, fieldDef);
            fieldDef.Rule = Id + typeName + typeParamsOpt + nullSpecOpt;
            nullSpecOpt.Rule = NULL | NOT + NULL | Empty;
            //typeName.Rule = ToTerm("BIT") | "DATE" | "TIME" | "TIMESTAMP" | "DECIMAL" | "REAL" | "FLOAT" | "SMALLINT" | "INTEGER" | "CHARACTER"
                                         
            //    // MS SQL types:  
            //                             | "DATETIME" | "INT" | "DOUBLE" | "CHAR" | "NCHAR" | "VARCHAR" | "NVARCHAR"
            //                             | "IMAGE" | "TEXT" | "NTEXT";

            typeName.Rule = ToTerm("DATE") | "TIME" | "DECIMAL" | "FLOAT" | "INTEGER" | "CHARACTER" | "INT" | "DOUBLE" | "CHAR";

            //typeParamsOpt.Rule = "(" + number + ")" | "(" + number + comma + number + ")" | Empty;
            typeParamsOpt.Rule = "(" + number + ")" | Empty;
            constraintDef.Rule = CONSTRAINT + Id + constraintTypeOpt;
            constraintListOpt.Rule = MakeStarRule(constraintListOpt, constraintDef);
            constraintTypeOpt.Rule = PRIMARY + KEY + idlistPar | UNIQUE + idlistPar | NOT + NULL + idlistPar
                                   | "FOREIGN" + KEY + idlistPar + "REFERENCES" + Id + idlistPar
                                   | "FOREIGN" + KEY + idlistPar + "REFERENCES" + Id + idlistPar + "Multiplicity" + Multiplicity;
            idlistPar.Rule = "(" + idlist + ")";
            idlist.Rule = MakePlusRule(idlist, comma, Id);


            Multiplicity.Rule = Cardinality_1to1 | Cardinality_1ton | Cardinality_nton;

            Cardinality_1to1.Rule = "1 to 1";
            Cardinality_1ton.Rule = "1 to many";
            Cardinality_nton.Rule = "many to many";

            WhereClause.Rule = "( WHERE" + WhereExpression + ")";

            WhereExpression.Rule = ArgumentName + SetOperator + Set | "(" + WhereExpression + ")" | WhereExpression + "AND" + WhereExpression | WhereExpression + "OR" + WhereExpression;



            Set.Rule = ArgumentName | "{" + TupleList + "}" | "EMPTYSET" | Set + "." + Attribute | Set + "Union" + Set | Set + "Intersect" + Set | Constant | Set + WhereClause | Set + "Minus" + Set | "<" + State + ">"  +  Set | "(" + Set + ")"   ;

            Tuple.Rule = Id;
            TableName.Rule = Id;
            Attribute.Rule = Id | "~" + Id;
            ArgumentName.Rule = Id;

            Constant.Rule = "CONSTANT" + typeName + "(" + Id_simple +")";

            TupleList.Rule = MakePlusRule(TupleList, comma, Tuple);

            Quantifier.Rule = TableName + Tuple;

            QuantifierList.Rule = MakePlusRule(QuantifierList, comma, Quantifier);

            QuantifierStmt.Rule = FOREACH + "(" + QuantifierList + ")";

            SetOperator_IN.Rule = "IN";
            SetOperator_EQ.Rule = "="; 
            SetOperator_NE.Rule = "!="; 
            SetOperator_ST.Rule = "<"; 
            SetOperator_GT.Rule = ">"; 
            SetOperator_GE.Rule = ">=";
            SetOperator_SE.Rule = "<="; 


            SetOperator.Rule = SetOperator_IN | SetOperator_EQ | SetOperator_NE | SetOperator_ST | SetOperator_GT | SetOperator_GE | SetOperator_SE;

            CuLeRule.Rule = Set + SetOperator + Set
                            | "(" + CuLeRule + ")"
                            | CuLeRule + "OR" + CuLeRule
                            | CuLeRule + "AND" + CuLeRule
                            | "NOT" + CuLeRule
                            | "IF" + CuLeRule + "THEN" + CuLeRule
                            | "IF" + CuLeRule + "THEN" + CuLeRule + "ELSE" + CuLeRule
                            | CuLeRule + "IFF" + CuLeRule
                            ;


     
            CuLeRuleList.Rule = MakePlusRule(CuLeRuleList, comma, CuLeRule);

            CuLeRuleName.Rule = "Rule" + Id + ":" ;

            CuLeRuleStmt.Rule = CuLeRuleName + CuLeRuleList | CuLeRuleName + QuantifierStmt + ": (" + CuLeRuleList + ")";

            TransactionStmt.Rule = "Transaction" + Id + "(" + TransactionParameterList + ")" + "{" + Changing + Precondition + Input_Restriction + Result + "}";

            Changing.Rule = "Changing" + "(" + ChangeObjectList + ")" | Empty + Changing | Changing + Empty;

            Precondition.Rule = "Precondition" + CuLeAssertionList |Precondition + Empty  | Empty + Precondition;

            Input_Restriction.Rule = "Input_Restriction" + "(" + CuLeRuleList + ")" | "Input_Restriction" + "(" + Empty + ")";

            Result.Rule = "Result" + CuLeAssertionList | Empty + Result | Result + Empty;

            ChangeObjectList.Rule = MakePlusRule(ChangeObjectList, comma, ChangeObject);


            ChangeObject.Rule = Set;

            TransactionParameterName.Rule = Id;

            TransactionParameterType.Rule = Id;

            TransactionSingleValueParameter.Rule = TransactionParameterType + TransactionParameterName;

            TransactionParameter.Rule = TransactionSingleValueParameter ; 

            TransactionParameterList.Rule = MakePlusRule(TransactionParameterList, comma, TransactionParameter);

            CuLeAssertion.Rule = "(" + CuLeRuleList + ")" | "(" +  QuantifierStmt + ": (" + CuLeRuleList + ")" + ")";
            CuLeAssertionList.Rule = MakePlusRule(CuLeAssertionList, comma, CuLeAssertion);

            State.Rule = Id ; 


        }


    }
}

