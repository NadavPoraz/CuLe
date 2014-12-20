using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace CuLe
{
    public class CuLeTransaction
    {

        private CuLeQuantifierList m_Parameters;
        private List<CuLeSet> m_Changing;

        //private List<CuLeRule> m_Precondition;
        private List<CuLeRuleGroup> m_Precondition;
        private List<CuLeRule> m_InputRestriction;
       // private List<CuLeRule> m_Result;
        private List<CuLeRuleGroup> m_Result;

        private string m_transactionName;

        public string TransactionName
        {
            get
            {
                return m_transactionName;
            }
        }

        public string ToAlloyPred( DataTableCollection i_Tables )
        {

            List<string> l_ListOfStrings = new List<string>();

            l_ListOfStrings.Add( "// Transaction " + this.m_transactionName);

            string l_pred = ("pred " + this.m_transactionName + "[ OLD_STATE, NEW_STATE: State ");

            if (this.m_Parameters != null)
            {
                foreach (CuLeQuantifier l_parameter in this.m_Parameters)
                {
                    l_pred = l_pred + ", " + l_parameter.ToAlloy(null);
                }
            }


            l_pred = l_pred + " ]";
            l_ListOfStrings.Add(l_pred);
            
            l_ListOfStrings.Add("{");

            l_ListOfStrings.Add("// Preconditions");

            if (this.m_Precondition != null)
            {

                foreach (CuLeRuleGroup l_CuLeRuleGroup in this.m_Precondition)
                {
                    l_ListOfStrings.Add(l_CuLeRuleGroup.ToAlloyAssertion("OLD_STATE"));
                }

            }

            l_ListOfStrings.Add("// Input Restrictions");

            if (this.m_InputRestriction != null)
            {
                foreach (CuLeRule l_CuLeRule in this.m_InputRestriction)
                {
                    l_ListOfStrings.Add(l_CuLeRule.ToAlloy(null));
                }
            }
            
            l_ListOfStrings.Add("// Results");

            if (this.m_Result != null)
            {

                foreach (CuLeRuleGroup l_CuLeRuleGroup in this.m_Result)
                {
                    l_ListOfStrings.Add(l_CuLeRuleGroup.ToAlloyAssertion(null));
                }

            }

            l_ListOfStrings.Add("// Prevent all other changes");


            List<String> l_ChangingTableNames = new List<string>();


            if (this.m_Changing != null)
            {

                foreach( CuLeSet l_CuLeSet in this.m_Changing)
                {

                    if (l_CuLeSet is DBTableCuLeSet)
                    {
                        DBTableCuLeSet l_DBTableCuLeSet = (DBTableCuLeSet)l_CuLeSet;

                        l_ChangingTableNames.Add(l_DBTableCuLeSet.TableName());
                    }
                }
            }

                foreach( DataTable l_DataTable in i_Tables)
                {

                    if( l_ChangingTableNames.Contains(l_DataTable.TableName))
                    {

                    }
                    else
                    {
                        l_ListOfStrings.Add("NEW_STATE." + l_DataTable.TableName + " = " + "OLD_STATE." + l_DataTable.TableName );
                    }

                }

                foreach (DataTable l_DataTable in i_Tables)
                {

                    string l_changingTuplesString = null;

                    if (this.m_Changing != null)
                    {

                        foreach (CuLeSet l_CuLeSet in this.m_Changing)
                        {
                            if (l_CuLeSet is DBTableCuLeSet)
                            {
                            }
                            else
                            {
                                if ( (l_CuLeSet.CuLeSetType.GetSignatureString == l_DataTable.TableName) || (l_CuLeSet.CuLeSetType.GetSignatureString == ( l_DataTable.TableName + "{}") ) )
                                {
                                    if (l_changingTuplesString == null)
                                    {
                                        l_changingTuplesString = l_CuLeSet.ToAlloy("NEW_STATE");
                                    }
                                    else
                                    {
                                         l_changingTuplesString = l_changingTuplesString + " + " + l_CuLeSet.ToAlloy("NEW_STATE");
                                    }
                                }

                            }
                        }
                    }

                    if (l_changingTuplesString == null)
                    {
                        l_changingTuplesString = "none";
                    }

                    l_ListOfStrings.Add("No" + l_DataTable.TableName + "ChangeExcept[ OLD_STATE,NEW_STATE, " + l_changingTuplesString + " ]");

                   

                }



            
                    //else
                    //{
                    //    if (i_Tables.Contains(l_CuLeSet.CuLeSetType.GetSignatureString))
                    //    {

                    //    }
                    //   // if( i_tables. l_CuLeSet.CuLeSetType.GetSignatureString

                    //}

                
            

            l_ListOfStrings.Add("}");
            string o_string = null;

            foreach (string l_string in l_ListOfStrings)
            {
                o_string = o_string + "\n" + l_string;
            }

            return o_string;
        }

        public string ToAlloyTraces()
        {
            string l_TransactionParametersString = null;
            string l_TransactionQuantifiersString = null;
            if (this.m_Parameters != null)
            {
                foreach (CuLeQuantifier l_CuLeQuantifier in this.m_Parameters)
                {
                    if (l_TransactionQuantifiersString != null)
                    {
                        l_TransactionQuantifiersString = l_TransactionQuantifiersString + " , ";
                    }

                    l_TransactionQuantifiersString = l_TransactionQuantifiersString + l_CuLeQuantifier.ToAlloy(null);

                    l_TransactionParametersString = l_TransactionParametersString + " , " + l_CuLeQuantifier.Name;
                }
            }

            return( "some " + l_TransactionQuantifiersString + " | " + this.m_transactionName + "[ s,s'" + l_TransactionParametersString + " ] ");


            //if (l_CuLeQuantifier.QuantifierMultiplicity == QuantifierMultiplicity.Single)
            //{
            //    l_TransactionParametersString = l_TransactionParametersString + "some " + l_CuLeQuantifier.Name + " : " 
            //}
            //else if (l_CuLeQuantifier.QuantifierMultiplicity == QuantifierMultiplicity.Multiple)
            //{

            //}

            // l_TransactionParametersString = l_TransactionParametersString + "some " + l_CuLeQuantifier.Name

        }
            
        

        public CuLeTransaction(string i_transactionName, CuLeQuantifierList i_Parameters, List<CuLeSet> i_Changing, List<CuLeRuleGroup> i_Precondition, List<CuLeRule> i_InputResriction, List<CuLeRuleGroup> i_Result)
        {
            m_Parameters = i_Parameters;
            m_Changing = i_Changing;
            m_Precondition = i_Precondition;
            m_Result = i_Result;
            m_transactionName = i_transactionName;
            m_InputRestriction = i_InputResriction;
        }
    }






}


