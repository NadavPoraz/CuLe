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

    class CuLeDataAlloyInstance
    {

        private List<AlloyDateValue> m_DateValues;
        private List<AlloyDoubleValue> m_DoubleValues;
        private List<AlloyIntegerValue> m_IntegerValues;
        private List<AlloyStringValue> m_StringValues;
        private List<AlloyTimeValue> m_TimeValues;

        private XmlDocument m_XmlDoc;

        private List<AlloyTable> m_AlloyTables;

        private List<AlloyState> m_AlloyStates;

      //  private DataSet m_DataSet; 

        private List<DataSet> m_DataSets;


        public CuLeDataAlloyInstance(DataSet i_DataSet, string i_AloyXMLInstanceString)
        {
            m_XmlDoc = new XmlDocument();

            m_XmlDoc.LoadXml(i_AloyXMLInstanceString);

            m_AlloyStates = new List<AlloyState>();

            this.PopulateAlloyBaseValues();

            this.PopulateAlloyTables(i_DataSet);

            m_DataSets = new List<DataSet>();

            foreach (AlloyState l_AlloyState in m_AlloyStates)
            {

                DataSet l_DataSet = new DataSet(l_AlloyState.Name);

                foreach (DataTable l_DataTable in i_DataSet.Tables)
                {
                    DataTable l_newDataTable = l_DataTable.Clone();
                    l_DataSet.Tables.Add(l_newDataTable);
                }

                foreach (DataRelation l_DataRelation in i_DataSet.Relations)
                {

                    DataColumn[] l_ParentColumns = new DataColumn[l_DataRelation.ParentColumns.Length];
                    DataColumn[] l_ChildColumns = new DataColumn[l_DataRelation.ChildColumns.Length];

                    for (int i = 0; i < l_DataRelation.ParentColumns.Length; i++)
                    {
                        l_ParentColumns[i] = l_DataSet.Tables[l_DataRelation.ParentTable.TableName].Columns[l_DataRelation.ParentColumns[i].ColumnName];
                    }

                    for (int i = 0; i < l_DataRelation.ChildColumns.Length; i++)
                    {
                        l_ChildColumns[i] = l_DataSet.Tables[l_DataRelation.ChildTable.TableName].Columns[l_DataRelation.ChildColumns[i].ColumnName];
                    }



                    l_DataSet.Relations.Add(l_DataRelation.RelationName, l_ParentColumns, l_ChildColumns);

                }

                m_DataSets.Add(l_DataSet);

            }
            

            this.PopulateDataTables();


        }

        public List<DataSet> DataSets
        {
            get
            {
                
                return this.m_DataSets;
            }

        }


        private void PopulateAlloyBaseValues()
        {
            if (this.m_XmlDoc == null)
            {
                return;
            }

            AlloyBaseValueList l_AlloyBaseValueListDates = new AlloyBaseValueList(AlloySignature.Date);
            AlloyBaseValueList l_AlloyBaseValueListDoubles = new AlloyBaseValueList(AlloySignature.Double);
            AlloyBaseValueList l_AlloyBaseValueListIntegers = new AlloyBaseValueList(AlloySignature.Integer);
            AlloyBaseValueList l_AlloyBaseValueListStrings = new AlloyBaseValueList(AlloySignature.String);
            AlloyBaseValueList l_AlloyBaseValueListTimes = new AlloyBaseValueList(AlloySignature.Time);
            //AlloyBaseValueList l_AlloyStates = new AlloyBaseValueList(AlloySignature.State);

            m_DateValues = new List<AlloyDateValue>();
            m_DoubleValues = new List<AlloyDoubleValue>();
            m_IntegerValues = new List<AlloyIntegerValue>();
            m_StringValues = new List<AlloyStringValue>();
            m_TimeValues = new List<AlloyTimeValue>();

            AlloyBaseValueSorter l_DoubleValuesSorter = new AlloyBaseValueSorter();
            AlloyBaseValueSorter l_DateValuesSorter = new AlloyBaseValueSorter();
            AlloyBaseValueSorter l_IntegerValuesSorter = new AlloyBaseValueSorter();
            AlloyBaseValueSorter l_StringValuesSorter = new AlloyBaseValueSorter();
            AlloyBaseValueSorter l_TimeValuesSorter = new AlloyBaseValueSorter();

            AlloyBaseValueSorter l_StateSorter = new AlloyBaseValueSorter();

            if (m_XmlDoc.LastChild.Name == "alloy")
            {

                if (m_XmlDoc.LastChild.FirstChild.Name == "instance")
                {

                    // get all ORDERed elements
                    foreach (XmlNode l_XmlNode in m_XmlDoc.LastChild.FirstChild.ChildNodes)
                    {

                        if (l_XmlNode.Name == "field")
                        {
                            if (l_XmlNode.Attributes != null)
                            {
                                if (l_XmlNode.Attributes["label"].Value == "Next")
                                {
                                    if (l_XmlNode.ChildNodes != null)
                                    {
                                        foreach (XmlNode l_xmlTupleNode in l_XmlNode.ChildNodes)
                                        {

                                            if (l_xmlTupleNode.Name == "tuple" && l_xmlTupleNode.ChildNodes != null && l_xmlTupleNode.ChildNodes.Count == 3)
                                            {

                                                XmlNode l_OrderNode = l_xmlTupleNode.ChildNodes[0];
                                                XmlNode l_LowValueNode = l_xmlTupleNode.ChildNodes[1];
                                                XmlNode l_HighValueNode = l_xmlTupleNode.ChildNodes[2];

                                                switch (l_OrderNode.Attributes["label"].Value)
                                                {

                                                    case "DoubleOrder/Ord$0":

                                                        l_DoubleValuesSorter.InsertAlloyBaseValue(new AlloyDoubleValue(l_LowValueNode.Attributes["label"].Value), new AlloyDoubleValue(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                    case "IntOrder/Ord$0":

                                                        l_IntegerValuesSorter.InsertAlloyBaseValue(new AlloyIntegerValue(l_LowValueNode.Attributes["label"].Value), new AlloyIntegerValue(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                    case "StringOrder/Ord$0":

                                                        l_StringValuesSorter.InsertAlloyBaseValue(new AlloyStringValue(l_LowValueNode.Attributes["label"].Value), new AlloyStringValue(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                    case "DateOrder/Ord$0":

                                                        l_DateValuesSorter.InsertAlloyBaseValue(new AlloyDateValue(l_LowValueNode.Attributes["label"].Value), new AlloyDateValue(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                    case "TimeOrder/Ord$0":

                                                        l_TimeValuesSorter.InsertAlloyBaseValue(new AlloyTimeValue(l_LowValueNode.Attributes["label"].Value), new AlloyTimeValue(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                    case "ord/Ord$0":

                                                        l_StateSorter.InsertAlloyBaseValue(new AlloyState(l_LowValueNode.Attributes["label"].Value), new AlloyState(l_HighValueNode.Attributes["label"].Value));

                                                        break;

                                                }
                                            }
                                        }


                                    }
                                }
                            }

                        }
                    }

                    if (l_DateValuesSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("Date Value Next Order invalid");
                    }
                    if (l_DoubleValuesSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("Double Value Next Order invalid");
                    }
                    if (l_IntegerValuesSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("Integer Value Next Order invalid");
                    }
                    if (l_StringValuesSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("String Value Next Order invalid");
                    }
                    if (l_TimeValuesSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("Time Value Next Order invalid");
                    }

                    if (l_StateSorter.Count > 1)
                    {
                        throw new SyntaxErrorException("Time Value Next Order invalid");
                    }


                    if (l_DateValuesSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_DateValuesSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyDateValue)
                            {
                                //this.m_DateValues.Add((AlloyDateValue)l_AlloyBaseValue);
                                l_AlloyBaseValueListDates.Add(l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("Date Value Next Order invalid");
                            }

                        }
                    }

                    if (l_DoubleValuesSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_DoubleValuesSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyDoubleValue)
                            {
                                //this.m_DoubleValues.Add((AlloyDoubleValue)l_AlloyBaseValue);
                                l_AlloyBaseValueListDoubles.Add(l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("Double Value Next Order invalid");
                            }
                        }
                    }

                    if (l_IntegerValuesSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_IntegerValuesSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyIntegerValue)
                            {
                                //this.m_IntegerValues.Add((AlloyIntegerValue)l_AlloyBaseValue);
                                l_AlloyBaseValueListIntegers.Add(l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("Integer Value Next Order invalid");
                            }
                        }
                    }

                    if (l_StringValuesSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_StringValuesSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyStringValue)
                            {
                                //this.m_StringValues.Add((AlloyStringValue)l_AlloyBaseValue);
                                l_AlloyBaseValueListStrings.Add(l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("String Value Next Order invalid");
                            }
                        }
                    }

                    if (l_TimeValuesSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_TimeValuesSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyTimeValue)
                            {
                                // this.m_TimeValues.Add((AlloyTimeValue)l_AlloyBaseValue);
                                l_AlloyBaseValueListTimes.Add(l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("Time Value Next Order invalid");
                            }
                        }
                    }

                    if (l_StateSorter.Count == 1)
                    {
                        foreach (AlloyBaseValue l_AlloyBaseValue in l_StateSorter[0])
                        {
                            if (l_AlloyBaseValue is AlloyState)
                            {
                                //this.m_DateValues.Add((AlloyDateValue)l_AlloyBaseValue);
                                m_AlloyStates.Add((AlloyState)l_AlloyBaseValue);
                            }
                            else
                            {
                                throw new SyntaxErrorException("State Next Order invalid");
                            }

                        }
                    }


                    l_AlloyBaseValueListDoubles = l_AlloyBaseValueListDoubles.AssignRandomValues();
                    l_AlloyBaseValueListDates = l_AlloyBaseValueListDates.AssignRandomValues();
                    l_AlloyBaseValueListIntegers = l_AlloyBaseValueListIntegers.AssignRandomValues();
                    l_AlloyBaseValueListStrings = l_AlloyBaseValueListStrings.AssignRandomValues();
                    l_AlloyBaseValueListTimes = l_AlloyBaseValueListTimes.AssignRandomValues();

                    foreach (AlloyBaseValue l_AlloyBaseValue in l_AlloyBaseValueListDates)
                    {
                        if (l_AlloyBaseValue is AlloyDateValue)
                        {
                            this.m_DateValues.Add((AlloyDateValue)l_AlloyBaseValue);
                        }
                        else
                        {
                            throw new SyntaxErrorException("Date Value Next Order invalid");
                        }
                    }

                    foreach (AlloyBaseValue l_AlloyBaseValue in l_AlloyBaseValueListDoubles)
                    {
                        if (l_AlloyBaseValue is AlloyDoubleValue)
                        {
                            this.m_DoubleValues.Add((AlloyDoubleValue)l_AlloyBaseValue);
                        }
                        else
                        {
                            throw new SyntaxErrorException("Double Value Next Order invalid");
                        }
                    }


                    foreach (AlloyBaseValue l_AlloyBaseValue in l_AlloyBaseValueListIntegers)
                    {
                        if (l_AlloyBaseValue is AlloyIntegerValue)
                        {
                            this.m_IntegerValues.Add((AlloyIntegerValue)l_AlloyBaseValue);
                        }
                        else
                        {
                            throw new SyntaxErrorException("Integer Value Next Order invalid");
                        }
                    }


                    foreach (AlloyBaseValue l_AlloyBaseValue in l_AlloyBaseValueListStrings)
                    {
                        if (l_AlloyBaseValue is AlloyStringValue)
                        {
                            this.m_StringValues.Add((AlloyStringValue)l_AlloyBaseValue);
                        }
                        else
                        {
                            throw new SyntaxErrorException("String Value Next Order invalid");
                        }
                    }



                    foreach (AlloyBaseValue l_AlloyBaseValue in l_AlloyBaseValueListTimes)
                    {
                        if (l_AlloyBaseValue is AlloyTimeValue)
                        {
                            this.m_TimeValues.Add((AlloyTimeValue)l_AlloyBaseValue);
                        }
                        else
                        {
                            throw new SyntaxErrorException("Time Value Next Order invalid");
                        }
                    }


                }
            }


        }

        private void PopulateAlloyTables(DataSet i_dataSet)
        {

           
            List<AlloyBaseValue> l_AllAlloyBaseValues = new List<AlloyBaseValue>();

            l_AllAlloyBaseValues.AddRange(m_DateValues);
            l_AllAlloyBaseValues.AddRange(m_DoubleValues);
            l_AllAlloyBaseValues.AddRange(m_IntegerValues);
            l_AllAlloyBaseValues.AddRange(m_StringValues);
            l_AllAlloyBaseValues.AddRange(m_TimeValues);

            this.m_AlloyTables = new List<AlloyTable>();



            if (this.m_XmlDoc == null)
            {
                return;
            }

            if (m_XmlDoc.LastChild.Name == "alloy")
            {

                if (m_XmlDoc.LastChild.FirstChild.Name == "instance")
                {

                    foreach (XmlNode l_TableNode in m_XmlDoc.LastChild.FirstChild.ChildNodes)
                    {

                        if (l_TableNode.Name == "sig" && l_TableNode.Attributes != null && l_TableNode.Attributes["label"].Value.Contains("this/"))
                        {
                            char[] delimiterChars = { '/' };

                            string[] l_tokens = l_TableNode.Attributes["label"].Value.Split(delimiterChars);

                            if (l_tokens.Length == 2)
                            {
                                string lv_SignatureName = l_tokens[1];

                                if (i_dataSet.Tables.Contains(lv_SignatureName))
                                {

                                    //this.m_AlloyTables.Add(new AlloyTable(lv_SignatureName));

                                    AlloyTable l_AlloyTable = new AlloyTable(lv_SignatureName);

                                    if (l_TableNode.ChildNodes != null)
                                    {
                                        foreach (XmlNode l_xmlTupleNode in l_TableNode.ChildNodes)
                                        {

                                            string lv_TupleName = l_xmlTupleNode.Attributes["label"].Value;

                                            l_AlloyTable.AlloyTuples.Add(new AlloyTuple(lv_SignatureName, lv_TupleName));
                                        }

                                        foreach (XmlNode l_FieldNode in m_XmlDoc.LastChild.FirstChild.ChildNodes)
                                        {
                                            if (l_FieldNode.Name == "field" && l_FieldNode.Attributes != null && l_FieldNode.Attributes["parentID"].Value == l_TableNode.Attributes["ID"].Value && l_FieldNode.ChildNodes != null)
                                            {

                                                string lv_fieldName = l_FieldNode.Attributes["label"].Value;

                                                if (i_dataSet.Tables[lv_SignatureName].Columns.Contains(lv_fieldName))
                                                {
                                                    foreach (XmlNode l_AttributeNode in l_FieldNode.ChildNodes)
                                                    {
                                                        if (l_AttributeNode.Name == "tuple" && l_AttributeNode.ChildNodes != null && l_AttributeNode.ChildNodes.Count == 3)
                                                        {

                                                            string lv_TupleName = l_AttributeNode.ChildNodes[0].Attributes["label"].Value;
                                                            string lv_BaseValueName = l_AttributeNode.ChildNodes[1].Attributes["label"].Value;

                                                            string lv_StateName = l_AttributeNode.ChildNodes[2].Attributes["label"].Value;

                                                            AlloyTuple l_AlloyTuple = l_AlloyTable.AlloyTuples[l_AlloyTable.AlloyTuples.IndexOf( new AlloyTuple( lv_SignatureName, lv_TupleName))];

                                                            

                                                            if (l_AlloyTuple != null)
                                                            {
                                                                foreach (AlloyState l_AlloyState in m_AlloyStates)
                                                                {
                                                                    if (l_AlloyState.Name == lv_StateName)
                                                                    {
                                                                        foreach (AlloyBaseValue l_AlloyBaseValue in l_AllAlloyBaseValues)
                                                                        {
                                                                            if (l_AlloyBaseValue.Name == lv_BaseValueName)
                                                                            {
                                                                                l_AlloyTuple.AlloyValueFields.Add(new AlloyValueField(lv_fieldName, l_AlloyBaseValue, l_AlloyState));
                                                                                break;
                                                                            }

                                                                        }
                                                                    }
                                                                }
                                                            }  

                                                        }

                                                    } //end foreach Attribute Node
                                                }
                                                
                                            }
                                        } // end foreach Field Node


                                        this.m_AlloyTables.Add(l_AlloyTable);
                                    }
                                }
                            }





                        }
                    }
                }
            }
        }

        private void PopulateDataTables()
        {

            if (this.m_DataSets == null)
            {
                return;

            }

            if (this.m_DataSets.Count == 0)
            {
                return;
            }

            if (this.m_AlloyTables == null)
            {
                return;
            }

            foreach (DataSet l_dataSet in m_DataSets)
            {

                l_dataSet.EnforceConstraints = false;


                foreach (AlloyTable l_AlloyTable in m_AlloyTables)
                {
                    if (!(l_dataSet.Tables.Contains(l_AlloyTable.TableName)))
                    {
                        throw new SyntaxErrorException("Unknown Table Name Error");
                    }

                    DataTable l_DataTable = l_dataSet.Tables[l_AlloyTable.TableName];

                    foreach (AlloyTuple l_AlloyTuple in l_AlloyTable.AlloyTuples )
                    {
                        DataRow l_DataRow = l_DataTable.NewRow();

                        bool lv_flag_add_Row = false;

                        foreach (AlloyValueField l_AlloyValueField in l_AlloyTuple.AlloyValueFields)
                        {

                            if (l_AlloyValueField.AlloyState.Name == l_dataSet.DataSetName)
                            {

                                lv_flag_add_Row = true;

                                if (l_AlloyValueField.AlloySignature == AlloySignature.String)
                                {
                                    if (l_DataTable.Columns[l_AlloyValueField.Name].MaxLength < l_AlloyValueField.ObjectValue.ToString().Length)
                                    {
                                        l_DataRow[l_AlloyValueField.Name] = l_AlloyValueField.ObjectValue.ToString().Substring(0, l_DataTable.Columns[l_AlloyValueField.Name].MaxLength);
                                    }
                                    else
                                    {
                                        l_DataRow[l_AlloyValueField.Name] = l_AlloyValueField.ObjectValue;
                                    }
                                }
                                else
                                {
                                    l_DataRow[l_AlloyValueField.Name] = l_AlloyValueField.ObjectValue;
                                }

                            }
                        }

                        if (lv_flag_add_Row == true)
                        {
                            l_DataTable.Rows.Add(l_DataRow);
                        }
                        
                    }
                }

                l_dataSet.EnforceConstraints = true;

                l_dataSet.AcceptChanges();
            }
        }

        private abstract class AlloyBaseValue : IEquatable<AlloyBaseValue>, IEquatable<String>
        {

            protected string m_name;

            protected object m_value;

            public abstract AlloySignature AlloySignature { get; }

            public bool IsAssigned
            {
                get
                {
                    return (m_value != null);
                }
            }

            public Object ObjectValue
            {
                get
                {
                    return m_value;
                }
            }


            //  protectedstring m_sig;
            // protected System.Type m_type;

            public string Name
            {
                get
                {
                    return m_name;
                }
            }

            //public string Sig
            //{

            //}

            //public System.Type Type
            //{

            //}

            protected AlloyBaseValue(string i_name)
            {

                m_name = i_name;
            }

            public bool Equals(AlloyBaseValue other)
            {

                if (other.Name == this.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            public bool Equals(string otherName)
            {

                if (otherName == this.Name)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }



        }

        private class AlloyState : AlloyBaseValue
        {

            private AlloyDateValue m_AlloyDateValue;

            private AlloyTimeValue m_AlloyTimeValue;

            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.State;
                }
            }

            public AlloyState(string i_name)
                : base(i_name)
            {

            }

        }

        private class AlloyIntegerValue : AlloyBaseValue
        {

            //int m_value;

            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.Integer;
                }
            }


            public int Value
            {
                get
                {
                    return (int)m_value;
                }
                set
                {
                    m_value = value;
                }
            }



            public AlloyIntegerValue(string i_name)
                : base(i_name)
            {
                if (i_name.Contains("Const_int"))
                {

                    char[] delimiterChars = { '_', '$' };
                    string[] l_tokens = i_name.Split(delimiterChars);

                    m_value = Convert.ToInt32(l_tokens[2]);
                }




            }

        }

        private class AlloyStringValue : AlloyBaseValue
        {

            //string m_value;

            public string Value
            {
                get
                {
                    return m_value.ToString();
                }
                set
                {
                    m_value = value;
                }
            }

            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.String;
                }
            }

            public AlloyStringValue(string i_name)
                : base(i_name)
            {

                if (i_name.Contains("Const_string"))
                {

                    char[] delimiterChars = { '_', '$' };
                    string[] l_tokens = i_name.Split(delimiterChars);

                    m_value = (l_tokens[2]).ToString();
                }

            }


        }

        private class AlloyDateValue : AlloyBaseValue
        {

            // DateTime m_value;

            public DateTime Value
            {
                get
                {
                    return (DateTime)m_value;
                }
                set
                {
                    m_value = value;
                }
            }

            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.Date;
                }
            }




            public AlloyDateValue(string i_name)
                : base(i_name)
            {
                if (i_name.Contains("Const_date"))
                {

                    int l_dayInt = Convert.ToInt32(i_name[11].ToString() + i_name[12].ToString());

                    int l_monthInt = Convert.ToInt32(i_name[14].ToString() + i_name[15].ToString());

                    int l_YearInt = Convert.ToInt32(i_name[17].ToString() + i_name[18].ToString() + i_name[19].ToString() + i_name[20].ToString());

                    m_value = new DateTime(l_YearInt, l_monthInt, l_dayInt);

                }
                // Const_date_12_12_2013$0
            }

        }

        private class AlloyTimeValue : AlloyBaseValue
        {

            //DateTime m_value;

            public DateTime Value
            {
                get
                {
                    return (DateTime)m_value;
                }
                set
                {
                    m_value = value;
                }
            }



            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.Time;
                }
            }

            public AlloyTimeValue(string i_name)
                : base(i_name)
            {
                if (i_name.Contains("Const_time"))
                {
                    char[] delimiterChars = { '_', '$' };
                    string[] l_tokens = i_name.Split(delimiterChars);

                    DateTime l_Value = new DateTime();

                    l_Value = l_Value.AddHours(Convert.ToDouble(l_tokens[2]));
                    l_Value = l_Value.AddMinutes(Convert.ToDouble(l_tokens[3]));
                    l_Value = l_Value.AddSeconds(Convert.ToDouble(l_tokens[4]));

                    m_value = l_Value;

                }

            }

        }

        private class AlloyDoubleValue : AlloyBaseValue
        {

            //double m_value;

            public double Value
            {
                get
                {
                    return (double)m_value;
                }
                set
                {
                    m_value = value;
                }
            }


            override public AlloySignature AlloySignature
            {
                get
                {
                    return AlloySignature.Double;
                }
            }

            public AlloyDoubleValue(string i_name)
                : base(i_name)
            {

                if (i_name.Contains("Const_double_"))
                {
                    char[] delimiterChars = { '_', '"', '$', '/' };
                    string[] l_tokens = i_name.Split(delimiterChars);

                    if (l_tokens.Length != 5)
                    {
                        throw new SyntaxErrorException("Double Definition Error");
                    }

                    string l_integerString = l_tokens[2];
                    string l_fractionString = l_tokens[3];

                    string l_valueString = l_integerString + "." + l_fractionString;

                    m_value = Convert.ToDouble(l_valueString);

                }

            }


        }


        private class AlloyBaseValueList : List<AlloyBaseValue>
        {

            private AlloySignature m_AlloySignature;

            public List<String> m_ErrorMessages;

            public AlloySignature AlloySignature
            {
                get
                {
                    return m_AlloySignature;
                }
            }


            public AlloyBaseValueList(AlloySignature i_AlloySignature)
            {
                m_AlloySignature = i_AlloySignature;

                m_ErrorMessages = new List<String>();
            }

            public void InsertAlloyBaseValue(AlloyBaseValue i_LowValue, AlloyBaseValue i_HighValue)
            {

                if (this.Count == 0)
                {
                    this.Add(i_LowValue);
                    this.Add(i_HighValue);
                }
                else
                {
                    if (this.Contains(i_LowValue) && this.Contains(i_HighValue))
                    {
                        throw new SyntaxErrorException("AlloyBaseValue List - Both Values are already contained in List");
                    }
                    if ((!this.Contains(i_LowValue)) && (!this.Contains(i_HighValue)))
                    {
                        throw new SyntaxErrorException("AlloyBaseValue List - neither value is already contained in List");
                    }
                    if (this.Contains(i_LowValue) && (!(this.Contains(i_HighValue))))
                    {

                        this.Insert(this.IndexOf(i_LowValue) + 1, i_HighValue);

                    }
                    if (!(this.Contains(i_LowValue)) && this.Contains(i_HighValue))
                    {
                        this.Insert(this.IndexOf(i_HighValue), i_LowValue);

                    }

                }


            }


            new public void Add(AlloyBaseValue item)
            {

                if (this.m_AlloySignature == item.AlloySignature)
                {
                    base.Add(item);
                }
                else
                {
                    throw new SyntaxErrorException("Unable to Insert item of signature " + item.AlloySignature.ToString() + " to List with signature " + this.m_AlloySignature.ToString());
                }

            }

            new public AlloyBaseValueList GetRange(int index, int count)
            {

                List<AlloyBaseValue> l_List = base.GetRange(index, count);

                AlloyBaseValueList o_AlloyBaseValueList = new AlloyBaseValueList(this.AlloySignature);

                o_AlloyBaseValueList.AddRange(l_List);

                return o_AlloyBaseValueList;

            }

            public AlloyBaseValueList Concat(AlloyBaseValueList other)
            {

                if (this.AlloySignature != other.AlloySignature)
                {
                    throw new SyntaxErrorException("Unable to Concatenate List signature " + this.AlloySignature.ToString() + " to List with signature " + other.AlloySignature.ToString());
                }

                AlloyBaseValueList o_AlloyBaseValueList = new AlloyBaseValueList(this.AlloySignature);

                o_AlloyBaseValueList.AddRange(this);
                o_AlloyBaseValueList.AddRange(other);

                return o_AlloyBaseValueList;

            }



            public AlloyBaseValueList AssignRandomValues()
            {

                if (this == null)
                {
                    return null;
                }

                if (this.Count == 0)
                {
                    return this;
                }

                if (this.AlloySignature == AlloySignature.String)
                {

                    for (int i = 0; i < this.Count; i++)
                    {
                        if (this[i].IsAssigned)
                        {
                        }
                        else
                        {
                            string lv_string = null;

                            for (int k = 0; k < 20; k++)
                            {
                                lv_string = lv_string + (char)(65 + i);
                            }

                            ((AlloyStringValue)this[i]).Value = lv_string;

                        }

                    }

                    return this;

                }

                bool lv_flag_ListHasUnassignedValues = false;
                bool lv_flag_InnerNodesAllUnassigned = true;

                int lv_IndexOfFirstAssignedInnerNode = -1;

                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].IsAssigned)
                    {
                        if (i > 0 && i < this.Count - 1)
                        {
                            lv_flag_InnerNodesAllUnassigned = false;

                            if (lv_IndexOfFirstAssignedInnerNode == -1)
                            {
                                lv_IndexOfFirstAssignedInnerNode = i;
                            }

                        }
                    }
                    else
                    {
                        lv_flag_ListHasUnassignedValues = true;
                    }

                }

                if (lv_flag_ListHasUnassignedValues == false)
                {
                    return this;
                }

                if (lv_flag_InnerNodesAllUnassigned == false)
                {

                    if (lv_IndexOfFirstAssignedInnerNode != -1)
                    {

                        AlloyBaseValueList l_LeftList = this.GetRange(0, lv_IndexOfFirstAssignedInnerNode + 1);

                        AlloyBaseValueList l_RightList = this.GetRange(lv_IndexOfFirstAssignedInnerNode, this.Count - lv_IndexOfFirstAssignedInnerNode);


                        AlloyBaseValueList l_LeftListWithValues = null;
                        AlloyBaseValueList l_RightListWithValues = null;

                        if (l_LeftList != null)
                        {
                            l_LeftListWithValues = l_LeftList.AssignRandomValues();
                        }

                        if (l_RightList != null)
                        {
                            l_RightListWithValues = l_RightList.AssignRandomValues();
                        }


                        if (l_LeftListWithValues == null)
                        {
                            return l_RightListWithValues;
                        }

                        if (l_RightListWithValues == null)
                        {
                            return l_LeftListWithValues;
                        }

                        if (l_LeftListWithValues.Count == 0)
                        {
                            return l_RightListWithValues;
                        }

                        if (l_RightListWithValues.Count == 0)
                        {
                            return l_LeftListWithValues;
                        }

                        l_RightListWithValues = l_RightListWithValues.GetRange(1, l_RightListWithValues.Count - 1);

                        //l_RightListWithValues = (AlloyBaseValueList)(l_RightListWithValues.GetRange(1, l_RightListWithValues.Count - 1).ToList());

                        return (l_LeftListWithValues.Concat(l_RightListWithValues));

                    }
                    else
                    {

                        throw new SyntaxErrorException("Unknown error");

                    }
                }
                else
                {

                    object[] lv_ObjectsArray;
                    ObjectRange lv_ObjectRange = new ObjectRange(this.AlloySignature);

                    if (this.First().IsAssigned)
                    {
                        if (this.Last().IsAssigned) // X-X
                        {
                            lv_ObjectRange.LowerBound = this.First().ObjectValue;
                            lv_ObjectRange.UpperBound = this.Last().ObjectValue;
                        }
                        else   // X-0
                        {
                            lv_ObjectRange.LowerBound = this.First().ObjectValue;
                            lv_ObjectRange.UpperBound = null;
                        }
                    }
                    else
                    {
                        if (this.Last().IsAssigned) // 0-X
                        {
                            lv_ObjectRange.LowerBound = null;
                            lv_ObjectRange.UpperBound = this.Last().ObjectValue;
                        }
                        else   // 0-0
                        {
                            lv_ObjectRange.LowerBound = null;
                            lv_ObjectRange.UpperBound = null;

                        }
                    }

                    lv_ObjectsArray = GenerateRandomValuesWithinRange(this.Count, lv_ObjectRange);

                    for (int i = 0; i < this.Count; i++)
                    {

                        if (this[i].IsAssigned)
                        {

                            if (lv_ObjectsArray[i].ToString() != this[i].ObjectValue.ToString())
                            {
                                throw new SyntaxErrorException("Unknown error");
                            }
                        }
                        else
                        {
                            switch (this.AlloySignature)
                            {

                                case global::CuLe.AlloySignature.Date:

                                    ((AlloyDateValue)this[i]).Value = (DateTime)lv_ObjectsArray[i];
                                    break;

                                case global::CuLe.AlloySignature.Double:

                                    ((AlloyDoubleValue)this[i]).Value = (Double)lv_ObjectsArray[i];
                                    break;

                                case global::CuLe.AlloySignature.Integer:

                                    ((AlloyIntegerValue)this[i]).Value = (Int32)lv_ObjectsArray[i];
                                    break;

                                case global::CuLe.AlloySignature.String:

                                    ((AlloyStringValue)this[i]).Value = lv_ObjectsArray[i].ToString();
                                    break;

                                case global::CuLe.AlloySignature.Time:

                                    ((AlloyTimeValue)this[i]).Value = (DateTime)lv_ObjectsArray[i];
                                    break;
                            }

                        }
                    }

                    return this;
                }
            }

            private class ObjectRange
            {
                private Object m_UpperBound;
                private Object m_LowerBound;

                private AlloySignature m_AlloySignature;


                public ObjectRange(AlloySignature i_AlloySignature)
                {
                    m_AlloySignature = i_AlloySignature;
                }

                public AlloySignature AlloySignature
                {
                    get
                    {
                        return m_AlloySignature;
                    }
                }


                public object UpperBound
                {
                    get
                    {
                        return m_UpperBound;
                    }
                    set
                    {
                        if (value != null)
                        {
                            switch (m_AlloySignature)
                            {
                                case AlloySignature.Date:

                                    if (value is System.DateTime)
                                    {

                                        m_UpperBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;

                                case AlloySignature.Double:

                                    if (value is System.Double)
                                    {

                                        m_UpperBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;

                                case AlloySignature.Integer:

                                    if (value is Int32)
                                    {

                                        m_UpperBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;


                                case AlloySignature.String:

                                    m_UpperBound = value.ToString();
                                    break;

                                case AlloySignature.Time:

                                    if (value is System.DateTime)
                                    {

                                        m_UpperBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;
                            }
                        }

                    }
                }

                public object LowerBound
                {
                    get
                    {
                        return m_LowerBound;
                    }
                    set
                    {

                        if (value != null)
                        {
                            switch (m_AlloySignature)
                            {
                                case AlloySignature.Date:

                                    if (value is System.DateTime)
                                    {

                                        m_LowerBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;

                                case AlloySignature.Double:

                                    if (value is System.Double)
                                    {

                                        m_LowerBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;

                                case AlloySignature.Integer:

                                    if (value is Int32)
                                    {

                                        m_LowerBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;


                                case AlloySignature.String:

                                    m_LowerBound = value.ToString();
                                    break;

                                case AlloySignature.Time:

                                    if (value is System.DateTime)
                                    {

                                        m_LowerBound = value;
                                    }
                                    else
                                    {
                                        throw new SyntaxErrorException("Data Type Mismatch");
                                    }

                                    break;
                            }

                        }
                    }
                }

            }



            private object[] GenerateRandomValuesWithinRange(int i_numberOfValues, ObjectRange i_ObjectRange)
            {


                if (i_ObjectRange == null)
                {
                    return null;
                }

                if (i_numberOfValues <= 0)
                {
                    return null;
                }

                switch (i_ObjectRange.AlloySignature)
                {

                    case AlloySignature.Date:

                        return (GenerateDatesInRange(i_numberOfValues, i_ObjectRange));


                    case AlloySignature.Double:

                        return (GenerateDoublesInRange(i_numberOfValues, i_ObjectRange));

                    case AlloySignature.Integer:

                        return (GenerateIntegersInRange(i_numberOfValues, i_ObjectRange));

                    case AlloySignature.String:

                        break;

                    case AlloySignature.Time:

                        return (GenerateTimesInRange(i_numberOfValues, i_ObjectRange));

                }
                //if (i_ObjectRange.UpperBound < i_ObjectRange.LowerBound)
                //{
                //    throw new SyntaxErrorException("Upper bound is lower then lower bound");
                //}

                //if ((i_ObjectRange.UpperBound - i_ObjectRange.LowerBound) < i_numberOfValues)
                //{
                //    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                //    return null;
                //}

                // if( 




                return null;

            }

            private object[] GenerateDatesInRange(int i_numberOfValues, ObjectRange i_ObjectRange)
            {

                if (i_ObjectRange == null)
                {
                    return null;
                }

                if (i_ObjectRange.AlloySignature != AlloySignature.Date)
                {
                    throw new SyntaxErrorException("Data Type Mismatch");
                }


                DateTime l_LowerBoundDate;
                DateTime l_UpperBoundDate;

                if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound != null)
                {
                    l_LowerBoundDate = ((DateTime)i_ObjectRange.LowerBound).Date;
                    l_UpperBoundDate = ((DateTime)i_ObjectRange.UpperBound).Date;
                }

                else if (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound != null)
                {
                    l_UpperBoundDate = ((DateTime)i_ObjectRange.UpperBound).Date;
                    l_LowerBoundDate = l_UpperBoundDate.AddDays(-7 * (i_numberOfValues - 1));
                }

                else if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundDate = ((DateTime)i_ObjectRange.LowerBound).Date;
                    l_UpperBoundDate = l_LowerBoundDate.AddDays(7 * (i_numberOfValues - 1));
                }

                else // (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundDate = DateTime.Now.Date;
                    l_UpperBoundDate = l_LowerBoundDate.AddDays(7 * (i_numberOfValues - 1));
                }

                if (l_LowerBoundDate.Date > l_UpperBoundDate.Date)
                {
                    throw new SyntaxErrorException("Upper bound is lower then lower bound");
                }

                TimeSpan lv_dateSpan = l_UpperBoundDate - l_LowerBoundDate;

                if ((lv_dateSpan.Days - 1) < i_numberOfValues)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                int lv_daysApart = (lv_dateSpan.Days) / (i_numberOfValues - 1);

                if (lv_daysApart == 0)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                object[] o_DatesArray = new object[i_numberOfValues];

                for (int i = 0; i < i_numberOfValues; i++)
                {
                    o_DatesArray[i] = l_LowerBoundDate.AddDays(i * lv_daysApart);
                }

                o_DatesArray[i_numberOfValues - 1] = l_UpperBoundDate;

                return o_DatesArray;
            }

            private object[] GenerateDoublesInRange(int i_numberOfValues, ObjectRange i_ObjectRange)
            {

                if (i_ObjectRange == null)
                {
                    return null;
                }

                if (i_ObjectRange.AlloySignature != AlloySignature.Double)
                {
                    throw new SyntaxErrorException("Data Type Mismatch");
                }


                double l_LowerBoundDouble;
                double l_UpperBoundDouble;

                if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound != null)
                {
                    l_LowerBoundDouble = (Double)i_ObjectRange.LowerBound;
                    l_UpperBoundDouble = (Double)i_ObjectRange.UpperBound;
                }

                else if (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound != null)
                {
                    l_UpperBoundDouble = (Double)i_ObjectRange.UpperBound;
                    l_LowerBoundDouble = l_UpperBoundDouble + (-(1.11) * (i_numberOfValues - 1));
                }

                else if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundDouble = (Double)i_ObjectRange.LowerBound;
                    l_UpperBoundDouble = l_LowerBoundDouble + ((1.11) * (i_numberOfValues - 1));
                }

                else // (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundDouble = 1.11;
                    l_UpperBoundDouble = l_LowerBoundDouble + ((1.11) * (i_numberOfValues - 1));
                }

                if (l_LowerBoundDouble > l_UpperBoundDouble)
                {
                    throw new SyntaxErrorException("Upper bound is lower then lower bound");
                }

                int lv_hundredthsDifference = (int)((System.Math.Round((l_UpperBoundDouble - l_LowerBoundDouble), 2) * 100));


                if ((lv_hundredthsDifference - 1) < i_numberOfValues)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                int lv_hundredthsApart = (lv_hundredthsDifference) / (i_numberOfValues - 1);

                if (lv_hundredthsApart == 0)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                object[] o_DoublesArray = new object[i_numberOfValues];

                for (int i = 0; i < i_numberOfValues; i++)
                {
                    o_DoublesArray[i] = l_LowerBoundDouble + (i * lv_hundredthsApart * 0.01);
                }

                o_DoublesArray[i_numberOfValues - 1] = l_UpperBoundDouble;

                return o_DoublesArray;
            }

            private object[] GenerateIntegersInRange(int i_numberOfValues, ObjectRange i_ObjectRange)
            {

                if (i_ObjectRange == null)
                {
                    return null;
                }

                if (i_ObjectRange.AlloySignature != AlloySignature.Integer)
                {
                    throw new SyntaxErrorException("Data Type Mismatch");
                }


                int l_LowerBoundInt;
                int l_UpperBoundInt;

                if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound != null)
                {
                    l_LowerBoundInt = (int)i_ObjectRange.LowerBound;
                    l_UpperBoundInt = (int)i_ObjectRange.UpperBound;
                }

                else if (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound != null)
                {
                    l_UpperBoundInt = (int)i_ObjectRange.UpperBound;
                    l_LowerBoundInt = l_UpperBoundInt - (i_numberOfValues - 1);
                }

                else if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundInt = (int)i_ObjectRange.LowerBound;
                    l_UpperBoundInt = l_LowerBoundInt + (i_numberOfValues - 1);
                }

                else // (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundInt = 1;
                    l_UpperBoundInt = l_LowerBoundInt + (i_numberOfValues - 1);
                }

                if (l_LowerBoundInt > l_UpperBoundInt)
                {
                    throw new SyntaxErrorException("Upper bound is lower then lower bound");
                }

                int lv_Difference = l_UpperBoundInt - l_LowerBoundInt;

                int lv_apart = (l_UpperBoundInt - l_LowerBoundInt) / (i_numberOfValues - 1);


                if (lv_apart == 0)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                object[] o_IntegersArray = new object[i_numberOfValues];

                for (int i = 0; i < i_numberOfValues; i++)
                {
                    o_IntegersArray[i] = l_LowerBoundInt + (i * lv_apart);
                }

                o_IntegersArray[i_numberOfValues - 1] = l_UpperBoundInt;

                return o_IntegersArray;
            }

            private object[] GenerateTimesInRange(int i_numberOfValues, ObjectRange i_ObjectRange)
            {

                if (i_ObjectRange == null)
                {
                    return null;
                }

                if (i_ObjectRange.AlloySignature != AlloySignature.Time)
                {
                    throw new SyntaxErrorException("Data Type Mismatch");
                }


                DateTime l_LowerBoundTime;
                DateTime l_UpperBoundTime;

                if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound != null)
                {
                    l_LowerBoundTime = (DateTime)i_ObjectRange.LowerBound;
                    l_UpperBoundTime = (DateTime)i_ObjectRange.UpperBound;
                }

                else if (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound != null)
                {
                    l_UpperBoundTime = (DateTime)i_ObjectRange.UpperBound;
                    l_LowerBoundTime = l_UpperBoundTime.AddSeconds(-66 * (i_numberOfValues - 1));
                }

                else if (i_ObjectRange.LowerBound != null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundTime = (DateTime)i_ObjectRange.LowerBound;
                    l_UpperBoundTime = l_LowerBoundTime.AddSeconds(66 * (i_numberOfValues - 1));
                }

                else // (i_ObjectRange.LowerBound == null && i_ObjectRange.UpperBound == null)
                {
                    l_LowerBoundTime = DateTime.Now;
                    l_UpperBoundTime = l_LowerBoundTime.AddSeconds(66 * (i_numberOfValues - 1));
                }

                if (l_LowerBoundTime.TimeOfDay > l_UpperBoundTime.TimeOfDay)
                {
                    throw new SyntaxErrorException("Upper bound is lower then lower bound");
                }

                TimeSpan lv_TimeSpan = l_UpperBoundTime - l_LowerBoundTime;

                if ((lv_TimeSpan.TotalSeconds - 1) < i_numberOfValues)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                double lv_SecondsApart = lv_TimeSpan.TotalSeconds / (i_numberOfValues - 1);

                if (lv_SecondsApart < 1)
                {
                    this.m_ErrorMessages.Add("Error, Unable to generate sufficient Values");
                }

                object[] o_TimesArray = new object[i_numberOfValues];

                for (int i = 0; i < i_numberOfValues; i++)
                {
                    o_TimesArray[i] = l_LowerBoundTime.AddSeconds(i * lv_SecondsApart);
                }

                o_TimesArray[i_numberOfValues - 1] = l_UpperBoundTime
                    ;

                return o_TimesArray;
            }

        }



        private class AlloyBaseValueSorter : List<AlloyBaseValueList>
        {

            public void InsertAlloyBaseValue(AlloyBaseValue i_LowValue, AlloyBaseValue i_HighValue)
            {


                if (this.Count == 0)
                {

                    AlloyBaseValueList l_AlloyBaseValueList = new AlloyBaseValueList(i_LowValue.AlloySignature);

                    l_AlloyBaseValueList.InsertAlloyBaseValue(i_LowValue, i_HighValue);

                    this.Add(l_AlloyBaseValueList);

                    return;
                }
                else
                {
                    AlloyBaseValueList l_ListWithLowValue = null;
                    AlloyBaseValueList l_ListWithHighValue = null;

                    foreach (AlloyBaseValueList l_AlloyBaseValueList in this)
                    {
                        if (l_AlloyBaseValueList.Contains(i_LowValue))
                        {
                            l_ListWithLowValue = l_AlloyBaseValueList;
                        }

                        if (l_AlloyBaseValueList.Contains(i_HighValue))
                        {
                            l_ListWithHighValue = l_AlloyBaseValueList;
                        }
                    }

                    if (l_ListWithLowValue == null && l_ListWithHighValue == null)
                    {

                        AlloyBaseValueList l_AlloyBaseValueList = new AlloyBaseValueList(i_LowValue.AlloySignature);

                        l_AlloyBaseValueList.InsertAlloyBaseValue(i_LowValue, i_HighValue);

                        this.Add(l_AlloyBaseValueList);

                        return;
                    }

                    else if (l_ListWithLowValue != null && l_ListWithHighValue == null)
                    {

                        this.Remove(l_ListWithLowValue);

                        l_ListWithLowValue.InsertAlloyBaseValue(i_LowValue, i_HighValue);

                        this.Add(l_ListWithLowValue);

                        return;

                    }

                    else if (l_ListWithLowValue == null && l_ListWithHighValue != null)
                    {

                        this.Remove(l_ListWithHighValue);

                        l_ListWithHighValue.InsertAlloyBaseValue(i_LowValue, i_HighValue);

                        this.Add(l_ListWithHighValue);

                        return;

                    }

                    else if (l_ListWithLowValue != null && l_ListWithHighValue != null)
                    {
                        if (l_ListWithLowValue.Last<AlloyBaseValue>().Name != i_LowValue.Name)
                        {

                            throw new SyntaxErrorException("Next Ordering out of order");
                        }

                        if (l_ListWithHighValue.First<AlloyBaseValue>().Name != i_HighValue.Name)
                        {

                            throw new SyntaxErrorException("Next Ordering out of order");
                        }

                        this.Remove(l_ListWithHighValue);
                        this.Remove(l_ListWithLowValue);

                        l_ListWithLowValue.AddRange(l_ListWithHighValue);

                        this.Add(l_ListWithLowValue);

                        return;

                    }
                }

            }

        }

        private class AlloyValueField
        {
            private string m_name;
            // private AlloySignature m_AlloySignature;
            private AlloyBaseValue m_AlloyBaseValue;
            private AlloyState m_AlloyState;

            public string Name
            {
                get
                {
                    return m_name;
                }

            }

            public AlloySignature AlloySignature
            {
                get
                {
                    return m_AlloyBaseValue.AlloySignature;
                }
            }

            public AlloyBaseValue AlloyBaseValue
            {
                get
                {
                    return m_AlloyBaseValue;
                }
            }

            public object ObjectValue
            {
                get
                {

                    if (m_AlloyBaseValue == null)
                    {
                        return null;
                    }
                    else
                    {
                        return m_AlloyBaseValue.ObjectValue;
                    }
                }
            }

            public AlloyState AlloyState
            {
                get
                {
                    return m_AlloyState;
                }
            }

            public AlloyValueField(string i_fieldName, AlloyBaseValue i_AlloyBaseValue, AlloyState i_AlloyState)
            {

                m_name = i_fieldName;
                m_AlloyBaseValue = i_AlloyBaseValue;
                m_AlloyState = i_AlloyState;

            }

        }

        private class AlloyTuple : IEquatable<AlloyTuple>
        {

            private string m_tupleName;
            private string m_tableName;
            public List<AlloyValueField> AlloyValueFields;

            private AlloyState m_AlloyState;

            public string TupleName
            {
                get
                {
                    return m_tupleName;
                }

            }

            public AlloyState AlloyState
            {
                get
                {
                    return m_AlloyState;
                }
            }

            public string TableName
            {
                get
                {
                    return m_tableName;
                }

            }

            public AlloyTuple(string i_tableName, string i_tupleName)
            {

                m_tableName = i_tableName;
                m_tupleName = i_tupleName;

               // m_AlloyState = i_AlloyState;

                AlloyValueFields = new List<AlloyValueField>();
            }

            public bool Equals(AlloyTuple other)
            {

                if (other == null)
                {
                    return false;
                }

                return (other.TableName == this.TableName && other.TupleName == this.TupleName);
            

            }

        }

        private class AlloyTable : IEquatable<AlloyTable>
        {

            private string m_tableName;
            public List<AlloyTuple> AlloyTuples;

            public string TableName
            {
                get
                {
                    return m_tableName;
                }

            }

            public AlloyTable(string i_tableName)
            {
                m_tableName = i_tableName;

                AlloyTuples = new List<AlloyTuple>();
            }


            public bool Equals(AlloyTable other)
            {
                if (other == null)
                {
                    return false;
                }

                if (other.TableName == this.TableName)
                {
                    return true;
                }

                return false;
            }    

        }
    }

    public enum AlloySignature{ Integer, Double, Date, Time, String, State};
}
