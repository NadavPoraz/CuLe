using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CuLe
{
    public class TableFieldDef
    {
        public ParseTreeNode m_id_simple;
        public ParseTreeNode m_typeName;
        public ParseTreeNode m_typeParams;

        public TableFieldDef(ParseTreeNode i_parseNode)
        {

            if (i_parseNode.Term.Name != "fieldDef")
            {
                throw new System.ArgumentException("ParseNode must be of type 'fieldDef' ");
            }

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

                    case "typeName":

                        foreach (ParseTreeNode parseNode_typeName in parseNode.ChildNodes)
                        {

                            if (parseNode_typeName.Token != null)
                            {
                                m_typeName = parseNode_typeName;
                                break;

                            }

                        }

                        break;

                    case "typeParams":

                        foreach (ParseTreeNode parseNode_typeParams in parseNode.ChildNodes)
                        {

                            if (parseNode_typeParams.Term.Name == "number")
                            {
                                m_typeParams = parseNode_typeParams;
                                break;

                            }

                        }


                        break;

                    default:

                        break;

                }

            }

        }

        public string GetId()
        {

            if (m_id_simple != null)
            {
                return m_id_simple.Term.Name;

            }
            else return null;
        }



    }
}
