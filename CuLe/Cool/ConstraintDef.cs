using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{

    abstract public class ConstraintDef
    {

        public ParseTreeNode m_id_simple;



        public override bool Equals(object obj)
        {

            if (!(obj is ConstraintDef)) return false;

            return (this.m_id_simple.Token.Text == ((ConstraintDef)obj).m_id_simple.Token.Text);

        }

        public string ConstraintName()
        {
            return m_id_simple.Token.Text;
        }


    }


    class PrimaryKeyConstraintDef : ConstraintDef
    {

        public ParseTreeNodeList m_coloumns;

        public PrimaryKeyConstraintDef(ParseTreeNode i_parseNode)
        {
            if (i_parseNode.Term.Name != "constraintDef") throw new SyntaxErrorException("ParseNode must be of type 'constraintDef'");

            foreach (ParseTreeNode parseNode in i_parseNode.ChildNodes)
            {
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

                    case "constraintTypeOpt":

                        if (!(parseNode.ChildNodes[0].Term.Name == "PRIMARY" && parseNode.ChildNodes[1].Term.Name == "KEY"))
                            throw new SyntaxErrorException("ParseNode must be of type PRIMARY KEY constraintDef");

                        if (!(parseNode.ChildNodes[2].Term.Name == "idlistPar"))
                            throw new SyntaxErrorException("ParseNode PRIMARY KEY constraintDef must iclude idlistPar");

                        foreach (ParseTreeNode parseTreeNodeID in parseNode.ChildNodes[2].ChildNodes[1].ChildNodes)
                        {

                            if (parseTreeNodeID.Term.Name == "Id")
                            {

                                if (m_coloumns == null)
                                {
                                    m_coloumns = new ParseTreeNodeList();
                                }

                                m_coloumns.Add(parseTreeNodeID.ChildNodes[0]);

                            }
                        }

                        break;



                }
            }

        }

    }

    class ForeignKeyConstraintDef : ConstraintDef
    {

        public ParseTreeNodeList m_childColoumns;
        public ParseTreeNode m_referencedTable;
        public ParseTreeNodeList m_referencedColumns;

        public enum Multiplicity { single, many };

        public Multiplicity m_ChildMultiplicity;
        public Multiplicity m_parentMultiplicity;


        public ForeignKeyConstraintDef(ParseTreeNode i_parseNode)
        {
            if (i_parseNode.Term.Name != "constraintDef") throw new SyntaxErrorException("ParseNode must be of type 'constraintDef'");

            foreach (ParseTreeNode parseNode in i_parseNode.ChildNodes)
            {
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

                    case "constraintTypeOpt":

                        if (!(parseNode.ChildNodes[0].Term.Name == "FOREIGN" && parseNode.ChildNodes[1].Term.Name == "KEY" && parseNode.ChildNodes[3].Term.Name == "REFERENCES"))
                            throw new SyntaxErrorException("ParseNode must be of type FOREIGN KEY constraintDef");

                        if (!(parseNode.ChildNodes[2].Term.Name == "idlistPar" && parseNode.ChildNodes[4].Term.Name == "Id" && parseNode.ChildNodes[5].Term.Name == "idlistPar"))
                            throw new SyntaxErrorException("ParseNode of type FOREIGN KEY constraintDef argument Error");

                        foreach (ParseTreeNode parseTreeNodeID in parseNode.ChildNodes[2].ChildNodes[1].ChildNodes)
                        {

                            if (m_childColoumns == null)
                            {
                                m_childColoumns = new ParseTreeNodeList();
                            }

                            if (parseTreeNodeID.Term.Name == "Id")
                            {

                                m_childColoumns.Add(parseTreeNodeID.ChildNodes[0]);
                            }

                        }

                        m_referencedTable = parseNode.ChildNodes[4].ChildNodes[0];

                        foreach (ParseTreeNode parseTreeNodeID in parseNode.ChildNodes[5].ChildNodes[1].ChildNodes)
                        {

                            if (m_referencedColumns == null)
                            {
                                m_referencedColumns = new ParseTreeNodeList();
                            }

                            if (parseTreeNodeID.Term.Name == "Id")
                            {
                                m_referencedColumns.Add(parseTreeNodeID.ChildNodes[0]);
                            }

                        }

                        if (parseNode.ChildNodes.Count >= 8)
                        {

                            if( parseNode.ChildNodes[7].Term.Name == "Multiplicity")
                            {
                                switch (parseNode.ChildNodes[7].ChildNodes[0].Term.Name)
                                {

                                    case "Cardinality_1ton":

                                        this.m_ChildMultiplicity = Multiplicity.many;
                                        this.m_parentMultiplicity = Multiplicity.single;

                                        break;

                                    case "Cardinality_1to1":

                                        this.m_ChildMultiplicity = Multiplicity.single;
                                        this.m_parentMultiplicity = Multiplicity.single;
                                        break;

                                    case "Cardinality_nton":

                                        this.m_ChildMultiplicity = Multiplicity.many;
                                        this.m_parentMultiplicity = Multiplicity.many;
                                        break;

                                }

                            }

                        }



                        break;

                }
            }

        }


    }

    class UniqueConstraintDef : ConstraintDef
    {
        public ParseTreeNodeList m_coloumns;

        public UniqueConstraintDef(ParseTreeNode i_parseNode)
        {
            if (i_parseNode.Term.Name != "constraintDef") throw new SyntaxErrorException("ParseNode must be of type 'constraintDef'");

            foreach (ParseTreeNode parseNode in i_parseNode.ChildNodes)
            {
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

                    case "constraintTypeOpt":

                        

                        if (!(parseNode.ChildNodes[0].Term.Name == "UNIQUE"))
                            throw new SyntaxErrorException("ParseNode must be of type NOT NULL constraintDef");

                        if (!(parseNode.ChildNodes[1].Term.Name == "idlistPar"))
                            throw new SyntaxErrorException("ParseNode UNIQUE constraintDef must iclude idlistPar");

                        foreach (ParseTreeNode parseTreeNodeID in parseNode.ChildNodes[1].ChildNodes[1].ChildNodes)
                        {

                            if (parseTreeNodeID.Term.Name == "Id")
                            {

                                if (m_coloumns == null)
                                {
                                    m_coloumns = new ParseTreeNodeList();
                                }

                                m_coloumns.Add(parseTreeNodeID.ChildNodes[0]);

                            }
                        }

                        break;



                }
            }

        }

    }

    class NotNullConstraintDef : ConstraintDef
    {

        public ParseTreeNodeList m_coloumns;

        public NotNullConstraintDef(ParseTreeNode i_parseNode)
        {
            if (i_parseNode.Term.Name != "constraintDef") throw new SyntaxErrorException("ParseNode must be of type 'constraintDef'");

            foreach (ParseTreeNode parseNode in i_parseNode.ChildNodes)
            {
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

                    case "constraintTypeOpt":

                        if (!(parseNode.ChildNodes[0].Term.Name == "NOT" && parseNode.ChildNodes[1].Term.Name == "NULL"))
                            throw new SyntaxErrorException("ParseNode must be of type NOT NULL constraintDef");

                        if (!(parseNode.ChildNodes[2].Term.Name == "idlistPar"))
                            throw new SyntaxErrorException("ParseNode NOT NULL constraintDef must iclude idlistPar");

                        foreach (ParseTreeNode parseTreeNodeID in parseNode.ChildNodes[2].ChildNodes[1].ChildNodes)
                        {

                            if (parseTreeNodeID.Term.Name == "Id")
                            {

                                if (m_coloumns == null)
                                {
                                    m_coloumns = new ParseTreeNodeList();
                                }

                                m_coloumns.Add(parseTreeNodeID.ChildNodes[0]);

                            }
                        }

                        break;



                }
            }

        }

    }
}
