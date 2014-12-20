using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{
    public abstract class CuLeSetType
    {

        public enum Multiplicity { single, many }
        public enum SignatureType { field, tuple, relation, emptySet, State }

        //public Multiplicity m_multiplicity;
        //public SignatureType m_signatureType;

        public List<ErrorMessage> m_errorMessages;

        public abstract SignatureType GetSignatureType { get; }

        public abstract string GetSignatureString{ get; }



    }

    public abstract class ElementsCuLeSetType : CuLeSetType
    {
        protected string m_signatureName;
        public abstract Multiplicity GetMultiplicity{ get;}

        public string SignatureName
        {
            get
            {
                return m_signatureName;
            }
        }



    }

    public abstract class FieldCuLeSetType : ElementsCuLeSetType
    {


        protected string m_StateName;

        protected string m_fieldCaption;
        protected Type m_fieldType;

        public override SignatureType GetSignatureType
        {
            get
            {
                return CuLeSetType.SignatureType.field;
            }
        }

        public string StateName
        {
            get
            {
                return m_StateName;
            }
        }

        //public string TableName()
        //{
        //    return m_TableName;
        //}

        //public string ColumnName()
        //{
        //    return m_ColumnName;
        //}


    }

    public class SingleFieldCuLeSetType : FieldCuLeSetType
    {

        
        
        public override Multiplicity GetMultiplicity
        {
            get
            {
                return Multiplicity.single;
            }
        }

        public override string GetSignatureString
        {
            get
            {
                return this.m_signatureName;
            }
        }

        public Type FieldType
        {
            get
            {
                return m_fieldType;
            }
        }

        public String FieldCaption
        {
            get
            {
                return m_fieldCaption;
            }
        }

        public SingleFieldCuLeSetType(string i_fieldType, string i_State, string i_fieldCaption)
        {
            Type l_type = System.Type.GetType(i_fieldType);

            if (l_type != null)
            {
                this.m_signatureName = l_type.ToString();
                this.m_fieldType = l_type;
                this.m_StateName = i_State;
                this.m_fieldCaption = i_fieldCaption.ToLower();

                if (this.m_fieldCaption == "char")
                {
                    this.m_fieldCaption = "string";
                }

            }
            else
            {
                throw new System.ArgumentException("Unknown Type " + i_fieldType);
            }
        }


    }

    public class MultipleFieldCuLeSetType : FieldCuLeSetType
    {
        public override Multiplicity GetMultiplicity
        {
            get
            {
                return Multiplicity.many;
            }
        }

        public override string GetSignatureString
        {
            get
            {
                return this.m_signatureName + "{}";
            }
        }

        public MultipleFieldCuLeSetType(string i_fieldType, string i_State)
        {
            Type l_type = System.Type.GetType(i_fieldType);

            if (l_type != null)
            {
                this.m_signatureName = l_type.ToString();
                this.m_StateName = i_State;
            }
            else
            {
                throw new System.ArgumentException("Unknown Type " + i_fieldType);
            }

        }

    }

    public abstract class TupleCuLeSetType : ElementsCuLeSetType
    {
        public override SignatureType GetSignatureType
        {
            get
            {           
            return CuLeSetType.SignatureType.tuple;
            }
        }

    }

    public class SingleTupleCuLeSetType : TupleCuLeSetType
    {

        public override CuLeSetType.Multiplicity GetMultiplicity
        {
            get
            {
            return Multiplicity.single;
            }
        }
        public SingleTupleCuLeSetType(string lv_signature_name)
        {
            this.m_signatureName = lv_signature_name;
        }

        public override string GetSignatureString
        {
            get
            {
                return this.m_signatureName;
            }
        }

    }

    public class MultipleTupleCuLeSetType : TupleCuLeSetType
    {
        public override Multiplicity GetMultiplicity
        {
            get
            {
                return Multiplicity.many;
            }
        }

        public override string GetSignatureString
        {
            get
            {
                return this.m_signatureName + "{}";
            }
        }

        public MultipleTupleCuLeSetType(string i_signature_name, DataTableCollection i_Tables)
        {
            if (i_Tables.Contains(i_signature_name))
            {
                this.m_signatureName = i_signature_name;
            }
            else
            {
                throw new System.ArgumentException("Table Name Argument Error");
            }

        }
    }

    public class RelationCuLeSetType : CuLeSetType
    {
        private Multiplicity m_ParentMultiplicity;
        private Multiplicity m_ChildMultiplicity;

        private string m_ParentSignature;
        private string m_ChildSignature;

        public Multiplicity ParentMultiplicity
        {
            get
            {
                return m_ParentMultiplicity;
            }
        }

        public Multiplicity ChildMultiplicity
        {
            get
            {
                return m_ChildMultiplicity;
            }
        }

        public string ParentSignature
        {
            get
            {
                return m_ParentSignature;
            }
        }

        public string ChildSignature
        {
            get
            {
                return m_ChildSignature;
            }
        }

        public override string GetSignatureString
        {
            get
            {
                string l_ChildSignature;
                string l_ParentSignature;


                if (this.m_ChildMultiplicity == Multiplicity.single)
                {
                    l_ChildSignature = this.m_ChildSignature;
                }
                else
                {
                    l_ChildSignature = this.m_ChildSignature + "{}";
                }

                if (this.m_ParentMultiplicity == Multiplicity.single)
                {
                    l_ParentSignature = this.m_ParentSignature;
                }
                else
                {
                    l_ParentSignature = this.m_ParentSignature + "{}";
                }

                return l_ParentSignature + "--->" + l_ChildSignature;
            }

        }


        public override CuLeSetType.SignatureType GetSignatureType
        {
            get
            {
                return SignatureType.relation;
            }
        }


        public RelationCuLeSetType(string i_RelationName, DataTableCollection i_Tables, DataRelationCollection i_Relations)
        {


            DataRelation l_DataRelation = i_Relations[i_Relations.IndexOf(i_RelationName)];

            if (l_DataRelation == null)
            {
                throw new System.ArgumentException("Relation Argument Error");
            }


            this.m_ParentSignature = l_DataRelation.ParentTable.TableName;
            this.m_ChildSignature = l_DataRelation.ChildTable.TableName;


            this.m_ChildMultiplicity = Multiplicity.many;

            if (l_DataRelation.ParentKeyConstraint != null)
            {
                this.m_ParentMultiplicity = Multiplicity.single;
            }
            else
            {
                this.m_ParentMultiplicity = Multiplicity.many;
            }

            if (l_DataRelation.ChildKeyConstraint == null)
            {
                throw new System.ArgumentException("Relation Argument Error");
            }

            this.m_ChildMultiplicity = Multiplicity.many;

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
                        this.m_ChildMultiplicity = Multiplicity.single;
                    }


                }
            }

        }

        public RelationCuLeSetType(string i_ParentSignature, Multiplicity i_ParentMultiplicity, string i_ChildSignature, Multiplicity i_ChildMultiplicity, DataTableCollection i_Tables)
        {
            if (!i_Tables.Contains(i_ParentSignature))
            {
                throw new System.ArgumentException("Relation Argument Error, Parent Table not found");
            }

            if (!i_Tables.Contains(i_ChildSignature))
            {
                throw new System.ArgumentException("Relation Argument Error, Child Table not found");
            }

            this.m_ParentSignature = i_ParentSignature;
            this.m_ChildSignature = i_ChildSignature;

            this.m_ParentMultiplicity = i_ParentMultiplicity;
            this.m_ChildMultiplicity = i_ChildMultiplicity;



        }




    }


    public class EmptySetCuLeSetType : CuLeSetType
    {
        public override CuLeSetType.SignatureType GetSignatureType
        {
            get
            {
                return SignatureType.emptySet;
            }
        }
        public EmptySetCuLeSetType()
        {
        }

        public override string GetSignatureString
        {
            get
            {
                return SignatureType.emptySet.ToString();
            }
        }

    }


    //public class SystemStateCuLeSetType : CuLeSetType
    //{

    //    private string m_StateName;

    //    public string StateName
    //    {
    //        get
    //        {
    //            return m_StateName;
    //        }
    //    }

    //    public SystemStateCuLeSetType(string i_StateName)
    //    {

    //        this.m_StateName = i_StateName;
    //    }

    //    public override string GetSignatureString
    //    {
    //        get
    //        {
    //            return m_StateName;
    //        }
    //    }

    //    public override SignatureType GetSignatureType
    //    {
    //        get
    //        {
    //            return SignatureType.State;
    //        }
    //    }

    //}

    public abstract class CuLeSet
    {

        protected CuLeSetType m_CuLeSetType;

        public CuLeSetType CuLeSetType
        {
            get
            {
                return m_CuLeSetType;
            }
        }

        public string GetSignatureString()
        {
            if (m_CuLeSetType != null)
            {
                return m_CuLeSetType.GetSignatureString;
            }
            else
            {
                return "Unknown";
            }
        }

        abstract public string  ToAlloy(string i_State);

        abstract public List<ConstantValueCuLeSet> GetConstantValueCuLeSets();


    }

    abstract public class ArgumentNameCuLeSet : CuLeSet
    {
        // protected string m_ArgumentName;

        public abstract string ArgumentName();
    }

    public class DBTableCuLeSet : ArgumentNameCuLeSet
    {
        private string m_TableName;

        private string m_StateName;

        public String StateName()
        {
            return m_StateName;
        }

        public String TableName()
        {
            return m_TableName;
        }


        public override string ArgumentName()
        {
            return m_TableName;
        }


        public DBTableCuLeSet(CuLeSetType i_CuLeSetType, String i_TableName, string i_StateName)
        {
            m_CuLeSetType = i_CuLeSetType;
            m_TableName = i_TableName;

            m_StateName = i_StateName;

        }

        public override string ToAlloy(String i_State)
        {

            string l_state = null;

            if (m_StateName != null)
            {
                l_state = m_StateName;
            }
            else if ( i_State != null)
            {
                l_state = i_State;
            }


            if (l_state == null)
            {
                return ("this/" + m_TableName);
            }
            else
            {
                return (l_state + "." + m_TableName);
            }
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {

            return new List<ConstantValueCuLeSet>();

        }

    }

    public class LocalVariableCuLeSet : ArgumentNameCuLeSet
    {

        private string m_variableName;

        public string VariableName
        {
            get
            {
                return m_variableName;
            }
        }

        public override string ArgumentName()
        {
            return m_variableName;
        }

        public  LocalVariableCuLeSet(CuLeSetType i_CuLeSetType, string i_VariableName)
        {
            m_CuLeSetType = i_CuLeSetType;
            m_variableName = i_VariableName;

        }

        public override string ToAlloy( string i_State)
        {
            return m_variableName;
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return new List<ConstantValueCuLeSet>();
        }

    }

    public class SystemFieldCuLeSet : LocalVariableCuLeSet
    {

        public SystemFieldCuLeSet(CuLeSetType i_CuLeSetType, string i_VariableName)
            : base(i_CuLeSetType, i_VariableName)
        {
        }

        public override string ToAlloy(string i_State)
        {
            return (i_State + "." + this.VariableName);
        }


    }

    public class TupleListCuLeSet : CuLeSet
    {

        private List<String> m_tuples;
        private string m_tableName;

        public List<String> Tuples()
        {
            return m_tuples;
        }

        public TupleListCuLeSet(CuLeSetType i_CuLeSetType, List<String> i_TupleNames, string i_tableName)
        {
            m_CuLeSetType = i_CuLeSetType;
            m_tuples = i_TupleNames;
            m_tableName = i_tableName;
        }

        public override string ToAlloy(string i_State)
        {
            if (m_tuples == null)
            {
                return "( )";
            }

            string l_tuples = "";

            foreach (string l_curr_tuple in m_tuples)
            {
                if (l_tuples == "")
                {
                    l_tuples = l_tuples + l_curr_tuple + " ";
                }
                else
                {
                    l_tuples = ", " + l_tuples + l_curr_tuple + " ";
                }
            }

            l_tuples = "( " + l_tuples + ")";

            return l_tuples;

        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return new List<ConstantValueCuLeSet>();
        }

    }

    public class EmptyCuLeSet : CuLeSet
    {

        public EmptyCuLeSet()
        {
            m_CuLeSetType = new EmptySetCuLeSetType();
        }

        public override string ToAlloy(string i_State)
        {
            return "none";
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return new List<ConstantValueCuLeSet>();
        }

    }

    public abstract class AttributeCuLeSet : CuLeSet
    {

        protected CuLeSet m_ChildSet;
        protected String m_State;

        public CuLeSet ChildSet()
        {
            return m_ChildSet;
        }

        public abstract string AttributeName{ get;}

        public string State { get { return m_State; } }


        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_ChildSet.GetConstantValueCuLeSets();
        }


    }

    public class TableFieldAttributeCuLeSet : AttributeCuLeSet
    {

        private string m_TableName;
        private string m_fieldName;

        public string TableName()
        {
            return m_TableName;
        }
        public string FieldName()
        {
            return m_fieldName;
        }

        public override string AttributeName
        {
            get
            {
                return m_fieldName;
            }
        }

        public TableFieldAttributeCuLeSet(CuLeSetType i_CuLeSetType, CuLeSet i_ChildSet, string i_fieldName, string i_TableName, string i_State)
        {

            m_CuLeSetType = i_CuLeSetType;
            m_ChildSet = i_ChildSet;
            m_fieldName = i_fieldName;
            m_TableName = i_TableName;

            m_State = i_State;
        }

        public override string ToAlloy(string i_State)
        {
            string l_State = null;

            if (m_State != null)
            {
                l_State = m_State;
            }
            else if (i_State != null)
            {
                l_State = i_State;
            }

            if (l_State == null)
            {
                return m_ChildSet.ToAlloy(null) + ".(" + m_fieldName + " )";
            }
            else
            {
                return m_ChildSet.ToAlloy(l_State
                    ) + ".(" + m_fieldName + "." + l_State + " )";
            }
            //if (this.CuLeSetType is FieldCuLeSetType)
            //{
            //    FieldCuLeSetType l_FieldCuLeSetType = (FieldCuLeSetType)this.CuLeSetType;

            //    if (l_FieldCuLeSetType.StateName != null)
            //    {
            //        return m_ChildSet.ToAlloy() + ".(" + m_fieldName + "." + l_FieldCuLeSetType.StateName+ " )";
            //    }
            //}

            //return m_ChildSet.ToAlloy() + ".(" + m_fieldName + "." + m_State + " )";
        }
    }

    public abstract class RelationAttributeCuLeSet : AttributeCuLeSet
    {

        protected string m_RelationName;

        public string RelationName()
        {
            return m_RelationName;
        }

        public override string AttributeName
        {
            get
            {
                return m_RelationName;
            }
        }


    }

    public class RegularRelationAttributeCuLeSet : RelationAttributeCuLeSet
    {

        public RegularRelationAttributeCuLeSet(CuLeSetType i_CuLeSetType, CuLeSet i_ChildSet, string i_RelationName, string i_State)
        {
            m_ChildSet = i_ChildSet;
            m_CuLeSetType = i_CuLeSetType;
            m_RelationName = i_RelationName;

            m_State = i_State;
        }

        public override string ToAlloy(string i_State)
        {

            string l_state = null;

            if (m_State != null)
            {
                l_state = m_State;
            }
            else if (i_State != null)
            {
                l_state = i_State;
            }

            if (l_state != null)
            {
                return m_ChildSet.ToAlloy(l_state) + ".(" + m_RelationName + "." + l_state + " )";
            }
            else
            {
                return m_ChildSet.ToAlloy(null) + ".(" + m_RelationName + ")";
            }
        }

    }

    public class TransposeRelationAttributeCuLeSet : RelationAttributeCuLeSet
    {
        public TransposeRelationAttributeCuLeSet(CuLeSetType i_CuLeSetType, CuLeSet i_ChildSet, string i_RelationName, string i_State)
        {
            m_ChildSet = i_ChildSet;
            m_CuLeSetType = i_CuLeSetType;
            m_RelationName = i_RelationName;

            m_State = i_State;
        }

        public override string ToAlloy(string i_State)
        {

            string l_state = null;

            if (m_State != null)
            {
                l_state = m_State;
            }
            else if (i_State != null)
            {
                l_state = i_State;
            }

            if (l_state != null)
            {
                return m_ChildSet.ToAlloy(l_state) + ".~(" + m_RelationName + "." + l_state + " )";
            }
            else
            {
                return m_ChildSet.ToAlloy(null) + ".~(" + m_RelationName + " )";
            }
        }

    }

    public class WhereClauseCuLeSet : CuLeSet
    {
        protected CuLeSet m_ChildSet;
        protected WhereExpression m_WhereExpression;

        public WhereClauseCuLeSet(CuLeSet i_ChildCuLeSet, WhereClause i_WhereClause)
        {
            m_ChildSet = i_ChildCuLeSet;
            m_WhereExpression = i_WhereClause.WhereExpression;

            base.m_CuLeSetType = i_ChildCuLeSet.CuLeSetType;


        }

        public CuLeSet ChildSet()
        {
            return m_ChildSet;
        }

        public WhereExpression WhereExpression
        {
            get
            {
                return m_WhereExpression;
            }
        }

        //public string AttributeName
        //{
        //    get
        //    {
        //        return ;
        //    }
        //}

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {

            List<ConstantValueCuLeSet> o_ConstantValueCuLeSetList = new List<ConstantValueCuLeSet>();

            o_ConstantValueCuLeSetList.AddRange(m_ChildSet.GetConstantValueCuLeSets());

            o_ConstantValueCuLeSetList.AddRange(m_WhereExpression.GetConstantValueCuLeSets());


            return o_ConstantValueCuLeSetList;
        }


        override public string ToAlloy(string i_State)
        {
            int lv_random = this.GetHashCode();

            string lv_WhereClauseIndex = "whereClauseIndex" + lv_random.ToString();

            return "{ " + lv_WhereClauseIndex + " : " + m_ChildSet.ToAlloy(i_State) + " | " + m_WhereExpression.ToAlloy(lv_WhereClauseIndex, i_State) + " }";

        }

    }
    public class SetOperationCuLeSet : CuLeSet
    {


        private CuLeSet m_CuLeSetA;
        private CuLeSet m_CuLeSetB;

        private CuLeSetDef.SetOperation m_setOperation;



        public CuLeSet CuLeSetA()
        {
            return m_CuLeSetA;
        }

        public CuLeSet CuLeSetB()
        {
            return m_CuLeSetB;
        }

        public CuLeSetDef.SetOperation GetSetOperation()
        {
            return m_setOperation;
        }

        public SetOperationCuLeSet(CuLeSetType i_CuLeSetType, CuLeSet i_CuLeSetA, CuLeSet i_CuLeSetB, CuLeSetDef.SetOperation i_SetOperation)
        {

            m_CuLeSetA = i_CuLeSetA;
            m_CuLeSetB = i_CuLeSetB;
            m_CuLeSetType = i_CuLeSetType;
            m_setOperation = i_SetOperation;

        }

        public override string ToAlloy(string i_State)
        {

            if (m_setOperation == CuLeSetDef.SetOperation.Intersect)
            {
                return m_CuLeSetA.ToAlloy(i_State) + " & " + m_CuLeSetB.ToAlloy(i_State);
            }
            else if (m_setOperation == CuLeSetDef.SetOperation.Union)
            {
                return m_CuLeSetA.ToAlloy(i_State) + " + " + m_CuLeSetB.ToAlloy(i_State);
            }

            else if (m_setOperation == CuLeSetDef.SetOperation.Minus)
            {
                return m_CuLeSetA.ToAlloy(i_State) + " - " + m_CuLeSetB.ToAlloy(i_State);
            }

            throw new SyntaxErrorException("Unknown Set Operation Error");

        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_CuLeSetA.GetConstantValueCuLeSets().Concat(m_CuLeSetA.GetConstantValueCuLeSets()).ToList();
        }

    }

    public class ConstantValueCuLeSet : CuLeSet, IComparable<ConstantValueCuLeSet>, IEquatable<ConstantValueCuLeSet>
    {

        private object m_Value;
        private System.Type m_type;
        private String m_TypeString;
        private String m_ValueString;


        public object GetValue()
        {
            return m_Value;        
        }

        public String GetTypeString()
        {

              return m_TypeString;
        }

        public ConstantValueCuLeSet(string i_typeString, string i_ValueString)
        {


            m_type = Statement.GetTypeFromStatement(i_typeString);

            SingleFieldCuLeSetType l_SingleFieldCuLeSetType = new SingleFieldCuLeSetType(m_type.ToString(), null, i_typeString);


            try
            {
                m_Value = Convert.ChangeType(i_ValueString, m_type);

            }
            catch (Exception e)
            {

                throw new System.ArgumentException("CuLeSetDef Error");

            }


            m_CuLeSetType = l_SingleFieldCuLeSetType;
            m_TypeString = i_typeString;

            if (m_TypeString == "char")
            {
                m_TypeString = "string";
            }
            m_ValueString = i_ValueString;

        }

        public System.Type Type
        {
            get
            {
                return m_type;
            }
        }


        public override string ToAlloy(string i_State)
        {

            String lv_string = m_ValueString.Replace(".", "\"");

            lv_string = lv_string.Replace("/", "_");

            lv_string = lv_string.Replace(":", "_");



            return "Const_" + m_TypeString + "_" + lv_string;

        }

        public int CompareTo(ConstantValueCuLeSet i_OtherConstantValueCuLeSet)
        {

            if (i_OtherConstantValueCuLeSet == null)
            {
                throw new System.ArgumentException("Argument is null");
            }


            if (!(typeof(IComparable).IsAssignableFrom(this.Type)))
            {
                throw new System.ArgumentException("Value type is non comparable");
            }

            if (!(typeof(IComparable).IsAssignableFrom(i_OtherConstantValueCuLeSet.Type)))
            {
                throw new System.ArgumentException("Value type is non comparable");
            }

            IComparable l_valueA = (IComparable)this.GetValue();
            IComparable l_valueB = (IComparable)i_OtherConstantValueCuLeSet.GetValue();

            return (l_valueA.CompareTo(l_valueB));

        }

        public bool Equals(ConstantValueCuLeSet other)
        {

            if (other == null)
            {
                return false;
            }

            if (other.GetTypeString() != this.GetTypeString())
            {
                return false;
            }

            if (other.GetSignatureString() != this.GetSignatureString())
            {
                return false;
            }

            if (other.GetValue().ToString() != this.GetValue().ToString())
            {
                return false;
            }

            return true;

        }


        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            List<ConstantValueCuLeSet> l_ListOfConstantValueCuLeSets = new List<ConstantValueCuLeSet>();

            l_ListOfConstantValueCuLeSets.Add(this);

            return l_ListOfConstantValueCuLeSets;
        }

        public static int GetDifference(ConstantValueCuLeSet i_ConstantValueCuLeSet_low, ConstantValueCuLeSet i_ConstantValueCuLeSet_high)
        {
            if (i_ConstantValueCuLeSet_low == null || i_ConstantValueCuLeSet_high == null)
            {
                throw new System.ArgumentException("Argument is null");
            }

            if (i_ConstantValueCuLeSet_low.GetTypeString() != i_ConstantValueCuLeSet_high.GetTypeString() )

            {
                throw new System.ArgumentException("Different Value types are not comparable");
            }


            //if (!(typeof(IComparable).IsAssignableFrom(this.Type)))
            //{
            //    throw new System.ArgumentException("Value type is non comparable");
            //}

            //if (!(typeof(IComparable).IsAssignableFrom(i_OtherConstantValueCuLeSet.Type)))
            //{
            //    throw new System.ArgumentException("Value type is non comparable");
            //}

            switch (i_ConstantValueCuLeSet_low.GetTypeString())
            {
                case "string":

                    throw new System.ArgumentException("Unable to calculate Difference between strings");

                case "int":

                    int int_low = (int)i_ConstantValueCuLeSet_low.GetValue();
                    int int_high = (int)i_ConstantValueCuLeSet_high.GetValue();

                    return (int_high - int_low);


                    
                case "double":

                    double double_low = (double)i_ConstantValueCuLeSet_low.GetValue();
                    double double_high = (double)i_ConstantValueCuLeSet_high.GetValue();

                    double_low = double_low * 100;
                    double_high = double_high * 100;

                    return ((int)(double_high - double_low));

                case "date":

                    DateTime date_low = (DateTime)i_ConstantValueCuLeSet_low.GetValue();
                    DateTime date_high = (DateTime)i_ConstantValueCuLeSet_high.GetValue();

                    TimeSpan lv_dateSpan = date_high - date_low;

                    return lv_dateSpan.Days;


                case "time":

                    DateTime time_low = (DateTime)i_ConstantValueCuLeSet_low.GetValue();
                    DateTime time_high = (DateTime)i_ConstantValueCuLeSet_high.GetValue();

                    TimeSpan lv_timeSpan = time_high - time_low;

                    return lv_timeSpan.Seconds;

                default:

                    throw new System.ArgumentException("Unknown Data Type");

            }

     

        }
    }

    //public class SystemStateCuLeSet : CuLeSet, IEquatable<SystemStateCuLeSet>
    //{

       

    //    public SystemStateCuLeSet(string i_StateName)
    //    {

    //        m_CuLeSetType = new SystemStateCuLeSetType(i_StateName);

    //    }

    //    public bool Equals(SystemStateCuLeSet other)
    //    {

    //        if( other == null)
    //        {
    //            return false;
    //        }

    //        return (this.GetSignatureString() == other.GetSignatureString());

    //    }

    //    public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
    //    {
    //        return new List<ConstantValueCuLeSet>();
    //    }

    //    public override string ToAlloy(string i_State)
    //    {
    //        return this.CuLeSetType.GetSignatureString;
    //    }

    //}

     public abstract class WhereExpression
     {
         public abstract string ToAlloy(string i_whereClauseString, string i_State);

         public abstract List<ConstantValueCuLeSet> GetConstantValueCuLeSets();

         public abstract List<BinaricWhereExpression> GetAllBinaricWhereExpressions();

     }
    
     public class BinaricWhereExpression : WhereExpression
     {
         private string m_AttrbuteName;
         private CuLeSet m_Set;
         private SetOperator m_SetOperator;
         private String m_State;
         

         public CuLeSet Set
         {
             get
             {
                 return m_Set;
             }
         }

         public string AttributeName
         {
             get
             {
                 return m_AttrbuteName;
             }
         }

         public SetOperator SetOperator
         {
             get
             {
                 return m_SetOperator;
             }
         }

        // public BinaricWhereExpression ( BinaricWhereExpressionDef i_BinaricWhereExpressionDef)
        // {
        //     m_AttrbuteName = i_BinaricWhereExpressionDef.AttributeName;
        //     m_SetOperator = i_BinaricWhereExpressionDef.SetOperator;

        //     CuLeSetFactory l_CuLeSetFactory = new CuLeSetFactory();      

        //}

         public BinaricWhereExpression(string i_AttributeName, SetOperator i_SetOperator, CuLeSet i_CuLeSet, string i_State)
         {
             m_AttrbuteName = i_AttributeName;
             m_SetOperator = i_SetOperator;
             m_Set = i_CuLeSet;
             m_State = i_State;

            // CuLeSetFactory l_CuLeSetFactory = new CuLeSetFactory();

         }

         public override List<BinaricWhereExpression> GetAllBinaricWhereExpressions()
         {
             List<BinaricWhereExpression> o_BinaricWhereExpressionList = new List<BinaricWhereExpression>();

             o_BinaricWhereExpressionList.Add(this);

             return o_BinaricWhereExpressionList;
         }



         public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
         {
             return m_Set.GetConstantValueCuLeSets();
         }


         public override string ToAlloy(string i_WhereClauseIndex, string i_State)
         {

             string l_state = null;

             if (m_State != null)
             {
                 l_state = m_State;
             }
             else if (i_State != null)
             {
                 l_state = i_State;
             }
             
             string l_attributeString = i_WhereClauseIndex + ".(" + m_AttrbuteName + "." + l_state + ")";

             switch (m_SetOperator)
             {

                 case SetOperator.EQ:

                     return "( " + l_attributeString + " = " + m_Set.ToAlloy(l_state) + " )";

                 case SetOperator.GE:

                     return "( gte [ " + l_attributeString + ", " + m_Set.ToAlloy(l_state) + " ] )";

                 case SetOperator.GT:

                     return "( gt [ " + l_attributeString + ", " + m_Set.ToAlloy(l_state) + " ] )";

                 case SetOperator.IsSubsetOf:

                     return "( " + l_attributeString + " in " + m_Set.ToAlloy(l_state) + " )";

                 case SetOperator.NE:

                     return "( not ( " + l_attributeString + " = " + m_Set.ToAlloy(l_state) + " ) )";

                 case SetOperator.SE:

                     return "( lte [ " + l_attributeString + ", " + m_Set.ToAlloy(l_state) + " ] )";

                 case SetOperator.ST:

                     return "( lt [ " + l_attributeString + ", " + m_Set.ToAlloy(l_state) + " ] )";

                 default:
                     throw new SystemException("Unknown Set Operator");

             }
         }
     }

     public class OperatorWhereExpression : WhereExpression
     {

         private RuleOperator m_RuleOperator;

         private WhereExpression m_WhereExpressionA;
         private WhereExpression m_WhereExpressionB;



         public OperatorWhereExpression(WhereExpression i_whereExpressionA, RuleOperator i_ruleOperator, WhereExpression i_whereExpressionB)
         {
             m_RuleOperator = i_ruleOperator;

             m_WhereExpressionA = i_whereExpressionA;

             m_WhereExpressionB = i_whereExpressionB;
         }

         public RuleOperator RuleOperator
         {
             get
             {
                 return m_RuleOperator;
             }
         }

         public WhereExpression ExpressionA
         {
             get
             {
                 return m_WhereExpressionA;
             }
         }

         public WhereExpression ExpressionB
         {
             get
             {
                 return m_WhereExpressionB;
             }
         }

         public override string ToAlloy(string i_whereCauseIndex, string i_State)
         {

             switch (m_RuleOperator)
             {
                 case RuleOperator.AND:

                     return "( " + m_WhereExpressionA.ToAlloy(i_whereCauseIndex, i_State) + " and " + m_WhereExpressionB.ToAlloy(i_whereCauseIndex, i_State) + " )";

                 case RuleOperator.OR:

                     return "( " + m_WhereExpressionA.ToAlloy(i_whereCauseIndex, i_State) + " or " + m_WhereExpressionB.ToAlloy(i_whereCauseIndex, i_State) + " )";

                 default:

                     throw new ArgumentException("Unknown Rule Operator");

             }
         }

         public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
         {
             List<ConstantValueCuLeSet> o_ConstantValueCuLeSetList = new List<ConstantValueCuLeSet>();

             o_ConstantValueCuLeSetList.AddRange(m_WhereExpressionA.GetConstantValueCuLeSets());
             o_ConstantValueCuLeSetList.AddRange(m_WhereExpressionB.GetConstantValueCuLeSets());

             return o_ConstantValueCuLeSetList;
         }

         public override List<BinaricWhereExpression> GetAllBinaricWhereExpressions()
         {
             List<BinaricWhereExpression> o_BinaricWhereExpressionList = new List<BinaricWhereExpression>();

             o_BinaricWhereExpressionList.AddRange(this.m_WhereExpressionA.GetAllBinaricWhereExpressions());
             o_BinaricWhereExpressionList.AddRange(this.m_WhereExpressionB.GetAllBinaricWhereExpressions());

             return o_BinaricWhereExpressionList;
         }

     }

     public class WhereClause
     {

         private WhereExpression m_WhereExpression;

         public WhereExpression WhereExpression
         {
             get
             {
                 return m_WhereExpression;
             }
         }

         public WhereClause(WhereExpression i_WhereExpression)
         {
             m_WhereExpression = i_WhereExpression;
         }

         
     }

    public class CuLeSetFactory
    {

        public List<ErrorMessage> m_errorMessages;

         private DataTableCollection m_Tables;
         private DataRelationCollection m_Relations;
         private List<String> m_States;

         public CuLeSetFactory(DataTableCollection i_Tables, DataRelationCollection i_Relations, List<String> i_States)
        {
            this.m_errorMessages = new List<ErrorMessage>();

            this.m_Relations = i_Relations;

            this.m_Tables = i_Tables;

            this.m_States = i_States;

            if (m_States == null)
            {
                m_States = new List<String>();
            }
        }


        public CuLeSet CreateCuLeSet(CuLeSetDef i_CuLeSetDef, CuLeQuantifierList i_CuLeQuantifierList, String i_State)
        {

            if( i_State != null)
            {
                if( ! ( this.m_States.Contains( i_State) ) )
                {
                   m_errorMessages.Add(new ErrorMessage("Unknown State " +  i_State, i_CuLeSetDef.SourceLocation));
                   return null;
                }

            }

            if (i_CuLeSetDef.ParseTreeNode == null)
            {
                throw new System.ArgumentException("CuLeSetDef must include parse tree node");
            }

            if (i_CuLeSetDef.ParseTreeNode.ChildNodes == null)
            {
                throw new System.ArgumentException("CuLeSetDef parse tree node is a leaf");
            }

            switch (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].Term.Name)
            {

                case "Set":

                    switch (i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name)
                    {
                        case ".":

                            bool lv_flag_attributeIsRelation = false;
                            bool lv_flag_attributeIsField = false;
                            bool lv_flag_transpose = false;

                            ParseTreeNode l_IdNode = null;

                            string lv_attributeName;

                            if (i_CuLeSetDef.ParseTreeNode.ChildNodes[2].Term.Name != "Attribute")
                            {
                                throw new System.ArgumentException("CuLeSetDef Attribute not found");
                            }

                            switch (i_CuLeSetDef.ParseTreeNode.ChildNodes[2].ChildNodes[0].Term.Name)
                            {
                                case "Id":

                                    l_IdNode = i_CuLeSetDef.ParseTreeNode.ChildNodes[2].ChildNodes[0];
                                    break;

                                case "~":

                                    lv_flag_transpose = true;
                                    l_IdNode = i_CuLeSetDef.ParseTreeNode.ChildNodes[2].ChildNodes[1];
                                    break;

                            }

                            if (l_IdNode == null)
                            {
                                throw new System.ArgumentException("CuLeSetDef Attribute - Id not found");
                            }

                            if (l_IdNode.Term.Name != "Id")
                            {
                                throw new System.ArgumentException("CuLeSetDef Attribute - Id not found");
                            }


                            if (l_IdNode.ChildNodes[0].Term.Name != "id_simple")
                            {
                                throw new System.ArgumentException("CuLeSetDef Attribute - Id not found");
                            }

                            lv_attributeName = l_IdNode.ChildNodes[0].Token.Text;


                            CuLeSetDef ChildCuLeSetDef = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[0]);

                            if (ChildCuLeSetDef == null)
                            {
                                return null;
                            }

                            CuLeSet l_ChildSet = this.CreateCuLeSet(ChildCuLeSetDef, i_CuLeQuantifierList, i_State);


                            if (l_ChildSet == null)
                            {
                                return null;
                            }

                            // Only Tuple Sets have actual Attributes
                            // System States may have Attributes - but the Attribute must be a DB Table
                            // Fields only have Attributes if the Attribute is a System State.

                            //if (!(l_ChildSet.CuLeSetType is TupleCuLeSetType) && !(l_ChildSet.CuLeSetType is SystemStateCuLeSetType) && !(l_ChildSet.CuLeSetType is FieldCuLeSetType))
                            if (!(l_ChildSet.CuLeSetType is TupleCuLeSetType) && !(l_ChildSet.CuLeSetType is FieldCuLeSetType))
                            {
                                m_errorMessages.Add(new ErrorMessage(lv_attributeName + " is not a known attribute of " + l_ChildSet.CuLeSetType.GetSignatureString, i_CuLeSetDef.SourceLocation));
                                return null;
                            }

                            if (i_State == null && this.m_States != null && this.m_States.Count > 0)
                            {
                                string l_all_states = null;

                                foreach (string l_state in this.m_States)
                                {
                                    if (l_all_states == null)
                                    {
                                        l_all_states = "{ " + l_state;
                                    }
                                    else
                                    {
                                        l_all_states = l_all_states + ", " + l_state;
                                    }
                                }

                                l_all_states = l_all_states + " }";

                                m_errorMessages.Add(new ErrorMessage("CuLe Set definition Error - State not specified " + l_all_states, i_CuLeSetDef.SourceLocation));
                                return null;
                            }

                            // For a Tuple - attribute is either FieldName or Relation.

                            if (l_ChildSet.CuLeSetType is TupleCuLeSetType)
                            {

                                TupleCuLeSetType l_TupleCuLeSetType = (TupleCuLeSetType)l_ChildSet.CuLeSetType;

                                if (m_Relations.Contains(lv_attributeName) == true)
                                {


                                    DataRelation l_dataRelation = m_Relations[m_Relations.IndexOf(lv_attributeName)];

                                    if (l_dataRelation != null)
                                    {
                                        if (lv_flag_transpose == true)
                                        {
                                            if (l_dataRelation.ParentTable.TableName == l_TupleCuLeSetType.SignatureName)
                                            {
                                                lv_flag_attributeIsRelation = true;
                                            }
                                        }
                                        else
                                        {
                                            if (l_dataRelation.ChildTable.TableName == l_TupleCuLeSetType.SignatureName)
                                            {
                                                lv_flag_attributeIsRelation = true;
                                            }
                                        }
                                    }

                                }


                                if (m_Tables.Contains(l_TupleCuLeSetType.SignatureName))
                                {
                                    DataTable l_DataTable = m_Tables[m_Tables.IndexOf(l_TupleCuLeSetType.SignatureName)];

                                    if (l_DataTable != null)
                                    {
                                        if (l_DataTable.Columns.Contains(lv_attributeName))
                                        {

                                            string lv_column_name = l_DataTable.Columns[lv_attributeName].ColumnName;

                                            if (lv_column_name == lv_attributeName)
                                            {

                                                lv_flag_attributeIsField = true;
                                            }
                                        }
                                    }
                                }

                                if (lv_flag_attributeIsField == true && lv_flag_attributeIsRelation == true)
                                {
                                    m_errorMessages.Add(new ErrorMessage(lv_attributeName + " is ambiguous, as it refers to both a field and a relation of type " + l_ChildSet.GetSignatureString(), i_CuLeSetDef.SourceLocation));
                                    return null;
                                }

                                if (lv_flag_attributeIsField == false && lv_flag_attributeIsRelation == false)
                                {
                                    if (lv_flag_transpose == true)
                                    {
                                        lv_attributeName = "~" + lv_attributeName;
                                    }
                                    m_errorMessages.Add(new ErrorMessage(lv_attributeName + " is not a known attribute of " + l_ChildSet.GetSignatureString(), i_CuLeSetDef.SourceLocation));
                                    return null;
                                }

                                if (lv_flag_attributeIsField == true && lv_flag_transpose == true)
                                {
                                    m_errorMessages.Add(new ErrorMessage(lv_attributeName + " canot perform transpose on table fields ", i_CuLeSetDef.SourceLocation));
                                    return null;
                                }
                               

                                if (lv_flag_attributeIsField == true)
                                {

                                    CuLeSetType l_CuLeSetType;

                                    DataTable l_DataTable = m_Tables[m_Tables.IndexOf(l_TupleCuLeSetType.SignatureName)];

                                    if (l_DataTable != null)
                                    {
                                        if (l_DataTable.Columns.Contains(lv_attributeName))
                                        {
                                            DataColumn l_dataColumn = l_DataTable.Columns[l_DataTable.Columns.IndexOf(lv_attributeName)];

                                            if (l_TupleCuLeSetType.GetMultiplicity == CuLeSetType.Multiplicity.single)
                                            {
                                                l_CuLeSetType = new SingleFieldCuLeSetType(l_dataColumn.DataType.ToString(), i_State, l_dataColumn.Caption);
                                                return new TableFieldAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, l_DataTable.TableName, i_State);
                                                //return new SingleFieldCuLeSetType(l_dataColumn.DataType.ToString());
                                            }
                                            else if (l_TupleCuLeSetType.GetMultiplicity == CuLeSetType.Multiplicity.many)
                                            {
                                                l_CuLeSetType = new MultipleFieldCuLeSetType(l_dataColumn.DataType.ToString(), i_State);
                                                return new TableFieldAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, l_DataTable.TableName, i_State);
                                                //return new MultipleFieldCuLeSetType(l_dataColumn.DataType.ToString());
                                            }
                                        }
                                    }

                                }


                                else if (lv_flag_attributeIsRelation == true)
                                {
                                    CuLeSetType l_CuLeSetType;
                                    DataRelation l_dataRelation = m_Relations[m_Relations.IndexOf(lv_attributeName)];

                                    if (l_dataRelation != null)
                                    {

                                        RelationCuLeSetType l_RelationCuLeSet = new RelationCuLeSetType(lv_attributeName, m_Tables, m_Relations);

                                        if (l_RelationCuLeSet == null)
                                        {
                                            throw new System.ArgumentException("Relation Defenition Error");
                                        }

                                        if (lv_flag_transpose == false)
                                        {

                                            if (l_RelationCuLeSet.ChildSignature != l_TupleCuLeSetType.SignatureName)
                                            {
                                                throw new System.ArgumentException("Relation Defenition Error");
                                            }

                                            if (l_RelationCuLeSet.ParentMultiplicity == CuLeSetType.Multiplicity.single)
                                            {
                                                if (l_TupleCuLeSetType.GetMultiplicity == CuLeSetType.Multiplicity.single)
                                                {
                                                    l_CuLeSetType = new SingleTupleCuLeSetType(l_RelationCuLeSet.ParentSignature);
                                                    return new RegularRelationAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, i_State);
                                                    //return new SingleTupleCuLeSetType(l_RelationCuLeSet.ParentSignature());
                                                }
                                            }

                                            l_CuLeSetType = new MultipleTupleCuLeSetType(l_RelationCuLeSet.ParentSignature, m_Tables);
                                            return new RegularRelationAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, i_State);

                                            // return new MultipleTupleCuLeSetType(l_RelationCuLeSet.ParentSignature(), i_Tables);
                                        }
                                        else  // transpose relation
                                        {
                                            if (l_RelationCuLeSet.ParentSignature != l_TupleCuLeSetType.SignatureName)
                                            {
                                                throw new System.ArgumentException("Relation Defenition Error");
                                            }

                                            if (l_RelationCuLeSet.ChildMultiplicity == CuLeSetType.Multiplicity.single)
                                            {
                                                if (l_TupleCuLeSetType.GetMultiplicity == CuLeSetType.Multiplicity.single)
                                                {

                                                    l_CuLeSetType = new SingleTupleCuLeSetType(l_RelationCuLeSet.ChildSignature);
                                                    return new TransposeRelationAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, i_State);
                                                    //return new SingleTupleCuLeSetType(l_RelationCuLeSet.ChildSignature());

                                                }
                                            }

                                            l_CuLeSetType = new MultipleTupleCuLeSetType(l_RelationCuLeSet.ChildSignature, m_Tables);
                                            return new TransposeRelationAttributeCuLeSet(l_CuLeSetType, l_ChildSet, lv_attributeName, i_State);
                                            //return new MultipleTupleCuLeSetType(l_RelationCuLeSet.ChildSignature(), i_Tables);
                                        }
                                    }

                                }
                            }
                                

                            // for Fields, the Attribute is always a System State

                            else if (l_ChildSet.CuLeSetType is FieldCuLeSetType)
                            {
                                FieldCuLeSetType l_FieldCuLeSetType = (FieldCuLeSetType)l_ChildSet.CuLeSetType;

                                m_errorMessages.Add(new ErrorMessage(lv_attributeName + " is not a known attribute of " + l_FieldCuLeSetType.SignatureName, i_CuLeSetDef.SourceLocation));
                                return null;

                                //FieldCuLeSetType l_FieldCuLeSetType = (FieldCuLeSetType)l_ChildSet.CuLeSetType;
                                //if (!this.m_States.Contains(lv_attributeName))
                                //{
                                //    m_errorMessages.Add(new ErrorMessage(lv_attributeName + " is not a known attribute of " + l_FieldCuLeSetType.SignatureName, i_CuLeSetDef.SourceLocation));
                                //    return null;
                                //}

                                //ChildCuLeSetDef = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[0]);

                                //if (ChildCuLeSetDef == null)
                                //{
                                //    return null;
                                //}

                                //l_ChildSet = this.CreateCuLeSet(ChildCuLeSetDef, i_CuLeQuantifierList, lv_attributeName);

                                //return l_ChildSet;

                            }
                    

                            throw new System.ArgumentException("Attribute Defenition Error");


                        case "Union":
                        case "Intersect":
                        case "Minus":

                            if (i_CuLeSetDef.ParseTreeNode.ChildNodes[2].Term.Name != "Set")
                            {
                                throw new System.ArgumentException("CuLeSetDef Set for " + i_CuLeSetDef.ParseTreeNode.ChildNodes[0].Term.Name + " not found");
                            }

                            CuLeSetDef ChildCuLeSetDefA = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[0]);

                            if (ChildCuLeSetDefA == null)
                            {
                                throw new System.ArgumentException("CuLeSetDef Error");
                            }

                            CuLeSet ChildSetA = this.CreateCuLeSet(ChildCuLeSetDefA, i_CuLeQuantifierList, i_State);

                            CuLeSetDef ChildCuLeSetDefB = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[2]);

                            if (ChildCuLeSetDefB == null)
                            {
                                throw new System.ArgumentException("CuLeSetDef Error");
                            }

                            CuLeSet ChildSetB = this.CreateCuLeSet(ChildCuLeSetDefB, i_CuLeQuantifierList, i_State);

                            if (ChildSetA == null)
                            {
                                return null;
                            }
                            if (ChildSetB == null)
                            {
                                return null;
                            }

                            CuLeSetDef.SetOperation l_SetOperation;

                            if (i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name == "Union")
                            {
                                l_SetOperation = CuLeSetDef.SetOperation.Union;
                            }
                            else if ( i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name == "Intersect")
                            {
                                l_SetOperation = CuLeSetDef.SetOperation.Intersect;
                            }

                            else if (i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name == "Minus")
                            {
                                l_SetOperation = CuLeSetDef.SetOperation.Minus;
                            }

                            else
                            {
                                throw new System.ArgumentException("CuLeSetDef Error");
                            }

                            return this.PerformSetOperation(ChildSetA, ChildSetB, l_SetOperation, ChildCuLeSetDefA.SourceLocation, i_CuLeQuantifierList);

                        case "WhereClause":

                            ChildCuLeSetDef = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[0]);

                            if (ChildCuLeSetDef == null)
                            {
                                return null;
                            }

                            l_ChildSet = this.CreateCuLeSet(ChildCuLeSetDef, i_CuLeQuantifierList, i_State);

                            if (this.m_errorMessages != null && this.m_errorMessages.Count > 0)
                            {
                                return null;
                            }


                            

                            WhereClauseDef l_WhereClauseDef = new WhereClauseDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[1]);

                            WhereClause l_WhereClause = this.CreateWhereClause(l_WhereClauseDef, i_CuLeQuantifierList, i_State);

                            return (this.CreateWhereClauseCuLeSet(l_WhereClause, l_ChildSet, ChildCuLeSetDef.SourceLocation, i_State));
  

                        default:

                            throw new System.ArgumentException("CuLeSetDef Error");
                    }

                case "EMPTYSET":

                    return new EmptyCuLeSet();

                //return new EmptySetCuLeSetType();





                case "{":

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name != "TupleList")
                    {
                        throw new System.ArgumentException("TupleList Error");
                    }

                    List<CuLeQuantifier> lv_CuLeQuantifierList = new List<CuLeQuantifier>();
                    List<String> lv_tupleNames = new List<string>();

                    string lv_TableName = null;

                    foreach (ParseTreeNode l_TupleParseTreeNode in i_CuLeSetDef.ParseTreeNode.ChildNodes[1].ChildNodes)
                    {
                        if (l_TupleParseTreeNode.Term.Name != "Tuple")
                        {
                            throw new System.ArgumentException("TupleList Error");
                        }

                        string lv_tupleName = l_TupleParseTreeNode.ChildNodes[0].ChildNodes[0].Token.Text;


                        if (i_CuLeQuantifierList != null && i_CuLeQuantifierList.Contains(lv_tupleName) )
                        {
                            lv_CuLeQuantifierList.Add(i_CuLeQuantifierList.GetQuantifier(lv_tupleName));
                            lv_tupleNames.Add(lv_tupleName);
                        }
                        else
                        {
                            m_errorMessages.Add(new ErrorMessage("Unknown local variable " + lv_tupleName, i_CuLeSetDef.ParseTreeNode.ChildNodes[0].Token.Location));
                            return null;
                        }
                    }

                    if (lv_CuLeQuantifierList.Count == 0)
                    {
                        return new EmptyCuLeSet();
                        //return new EmptySetCuLeSetType();
                    }

                    else
                    {
                        for (int i = 0; i < lv_CuLeQuantifierList.Count; i++)
                        {
                            CuLeQuantifier l_CuLeQuantifier = lv_CuLeQuantifierList[i];

                            if (lv_TableName == null)
                            {
                                lv_TableName = l_CuLeQuantifier.Signature;
                            }

                            if (lv_TableName != l_CuLeQuantifier.Signature)
                            {
                                m_errorMessages.Add(new ErrorMessage("Cannot include variables of type " + lv_TableName + " and of type " + l_CuLeQuantifier.Signature + " in the same set", i_CuLeSetDef.SourceLocation));
                                return null;
                            }
                        }

                        if (lv_TableName != null)
                        {
                            CuLeSetType l_CuLeSetType = new MultipleTupleCuLeSetType(lv_TableName, m_Tables);

                            return new TupleListCuLeSet(l_CuLeSetType, lv_tupleNames, lv_TableName);

                            //return new MultipleTupleCuLeSetType(lv_TableName, i_Tables);
                        }
                        else
                        {
                            throw new System.ArgumentException("Tuple List Error");
                        }

                    }

                case "ArgumentName":

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[0].Term.Name != "Id")
                    {
                        throw new System.ArgumentException("ArgumentName Argument has no Id");
                    }

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Term.Name != "id_simple")
                    {
                        throw new System.ArgumentException("Argument has no Id");
                    }

                    string lv_argumentName = i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[0].ChildNodes[0].Token.Text;

                    bool lv_flag_argumentIsTable = false;
                    bool lv_flag_argumentIsTuple = false;


                    if (m_Tables.Contains(lv_argumentName))
                    {
                        lv_flag_argumentIsTable = true;
                    }

                    if (i_CuLeQuantifierList != null && i_CuLeQuantifierList.Contains(lv_argumentName))
                    {
                        lv_flag_argumentIsTuple = true;
                    }


                    if (lv_flag_argumentIsTable == true && lv_flag_argumentIsTuple == true)
                    {

                        m_errorMessages.Add(new ErrorMessage(lv_argumentName + " is ambiguous, as it refers to both a DB Table and a local variable", i_CuLeSetDef.SourceLocation));
                        return null;
                    }



                    if (lv_flag_argumentIsTable == false && lv_flag_argumentIsTuple == false)
                    {
                        m_errorMessages.Add(new ErrorMessage(lv_argumentName + " is unknown", i_CuLeSetDef.SourceLocation));
                        return null;
                    }

                    if (lv_flag_argumentIsTable == true)
                    {

                        if (i_State == null && this.m_States != null && this.m_States.Count > 0)
                        {
                            string l_all_states = null;

                            foreach (string l_state in this.m_States)
                            {
                                if (l_all_states == null)
                                {
                                    l_all_states = "{ " + l_state;
                                }
                                else
                                {
                                    l_all_states = l_all_states + ", " + l_state;
                                }
                            }

                            l_all_states = l_all_states + " }";

                            m_errorMessages.Add(new ErrorMessage("CuLe Set definition Error - State not specified " + l_all_states, i_CuLeSetDef.SourceLocation));
                            return null;
                        }


                        DataTable lv_DataTable = m_Tables[m_Tables.IndexOf(lv_argumentName)];

                        if (lv_DataTable != null)
                        {

                            MultipleTupleCuLeSetType l_MultipleTupleCuLeSetType = new MultipleTupleCuLeSetType(lv_argumentName, m_Tables);

                            //

                            return new DBTableCuLeSet(l_MultipleTupleCuLeSetType, lv_argumentName, null);


                        }
                        else
                        {
                            throw new System.ArgumentException("TableName Argument Error");
                        }
                    }

                    else if (lv_flag_argumentIsTuple == true)
                    {

                        CuLeQuantifier lv_CuLeQuantifier = i_CuLeQuantifierList.GetQuantifier(lv_argumentName);

                        if (lv_CuLeQuantifier != null)
                        {
                            if (lv_CuLeQuantifier is CuLeSingleBaseValueQuantifier)
                            {

                                System.Type l_Type = Statement.GetTypeFromStatement(lv_CuLeQuantifier.Signature);
                                SingleFieldCuLeSetType l_CuLeSetType = new SingleFieldCuLeSetType(l_Type.ToString(), i_State, lv_CuLeQuantifier.Signature);
                                if (lv_CuLeQuantifier is CuLeSystemField)
                                {
                                    return new SystemFieldCuLeSet(l_CuLeSetType, lv_argumentName);
                                }
                                else
                                {
                                    return new LocalVariableCuLeSet(l_CuLeSetType, lv_argumentName);
                                }
                            }
                            else if (lv_CuLeQuantifier is CuLeSingleTupleQuantifier)
                            {
                                SingleTupleCuLeSetType l_CuLeSetType = new SingleTupleCuLeSetType(lv_CuLeQuantifier.Signature);
                                return new LocalVariableCuLeSet(l_CuLeSetType, lv_argumentName);
                            }
                            else if (lv_CuLeQuantifier is CuLeMultiBaseValueQuantifier)
                            {

                                System.Type l_Type = Statement.GetTypeFromStatement(lv_CuLeQuantifier.Signature);
                                MultipleFieldCuLeSetType l_CuLeSetType = new MultipleFieldCuLeSetType(l_Type.ToString(), i_State);
                                return new LocalVariableCuLeSet(l_CuLeSetType, lv_argumentName);
                            }
                            else if (lv_CuLeQuantifier is CuLeMultiTupleQuantifier)
                            {
                                MultipleTupleCuLeSetType l_CuLeSetType = new MultipleTupleCuLeSetType(lv_CuLeQuantifier.Signature, m_Tables);
                                return new LocalVariableCuLeSet(l_CuLeSetType, lv_argumentName);
                            }
                            else
                            {
                                throw new System.ArgumentException("Tuple Name Argument Error");

                            }

                            //return new SingleTupleCuLeSetType(lv_CuLeQuantifier.m_TableName);
                        }
                        else
                        {
                            throw new System.ArgumentException("Tuple Name Argument Error");
                        }
                    }




                    ////if (lv_flag_argumentIsRelation == true)
                    ////{
                    ////    return new RelationCuLeSet(lv_argumentName, i_Tables, i_Relations);
                    ////}

                    //if (lv_flag_argumentIsState == true)
                    //{

                    //    return new SystemStateCuLeSet(lv_argumentName);

                    //}


                    throw new System.ArgumentException("CuLeSetDef Error");

                case "Constant":

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes.Count != 5) throw new System.ArgumentException("Constant Value Argument Error");

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[0].Term.Name != "CONSTANT") throw new System.ArgumentException("Constant Value Argument Error");
                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[1].Term.Name != "typeName") throw new System.ArgumentException("Constant Value Argument Error");
                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[2].Term.Name != "(") throw new System.ArgumentException("Constant Value Argument Error");
                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[3].Term.Name != "id_simple") throw new System.ArgumentException("Constant Value Argument Error");
                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[4].Term.Name != ")") throw new System.ArgumentException("Constant Value Argument Error");


                    string lv_typeString = i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[1].ChildNodes[0].Token.Text;
                    string lv_ValueString = i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[3].Token.Value.ToString();
                    System.Type l_type = Statement.GetTypeFromStatement(lv_typeString);

                    SingleFieldCuLeSetType l_SingleFieldCuLeSetType = new SingleFieldCuLeSetType(l_type.ToString(), i_State, lv_typeString);

                    Object l_Value;

                    try
                    {
                        l_Value = Convert.ChangeType(lv_ValueString, l_SingleFieldCuLeSetType.FieldType);

                    }
                    catch (Exception e)
                    {

                        m_errorMessages.Add(new ErrorMessage("Unable to convert " + i_CuLeSetDef.ParseTreeNode.ChildNodes[0].ChildNodes[3].Token.Value.ToString() + " to Type " + l_SingleFieldCuLeSetType.FieldType.ToString(), i_CuLeSetDef.SourceLocation));
                        return null;


                    }


                    return new ConstantValueCuLeSet(lv_typeString, lv_ValueString);

                case "(":

                    if ( i_CuLeSetDef.ParseTreeNode.ChildNodes[2].Term.Name  == ")")
                    {

                        CuLeSetDef l_ChildCuLeSetDef = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[1]);
                        return CreateCuLeSet(l_ChildCuLeSetDef, i_CuLeQuantifierList, i_State);
                    }
                    else
                    {
                          throw new System.ArgumentException("CuLeSetDef Error");
                    }

                case "<":

                    if (i_CuLeSetDef.ParseTreeNode.ChildNodes[2].Term.Name != ">" || (i_CuLeSetDef.ParseTreeNode.ChildNodes[1].Term.Name != "State") || i_CuLeSetDef.ParseTreeNode.ChildNodes[3].Term.Name != "Set")
                    {
                        throw new System.ArgumentException("CuLeSetDef Error");
                    }
                    else
                    {

                        string l_StateName = i_CuLeSetDef.ParseTreeNode.ChildNodes[1].ChildNodes[0].ChildNodes[0].Token.Value.ToString();

                        if (i_State != null && i_State != l_StateName)
                        {
                            m_errorMessages.Add(new ErrorMessage("Ambigious State Definition " + i_State + ", " + i_State, i_CuLeSetDef.SourceLocation));
                            return null;
                        }
                        

                        CuLeSetDef l_ChildCuLeSetDef = new CuLeSetDef(i_CuLeSetDef.ParseTreeNode.ChildNodes[3]);

                        return CreateCuLeSet(l_ChildCuLeSetDef, i_CuLeQuantifierList, l_StateName);
                    }

                default:

                    throw new System.ArgumentException("CuLeSetDef Error");

            }


        }

        private CuLeSet PerformSetOperation(CuLeSet i_ChildSetA, CuLeSet i_ChildSetB, CuLeSetDef.SetOperation i_SetOperation, SourceLocation i_SourceLocation, CuLeQuantifierList i_CuLeQuantifierList)
        {


            if (i_ChildSetA.CuLeSetType is ElementsCuLeSetType)
            {
                ElementsCuLeSetType l_ElementsChildCuLeSetA = (ElementsCuLeSetType)i_ChildSetA.CuLeSetType;

                if (l_ElementsChildCuLeSetA.GetMultiplicity == CuLeSetType.Multiplicity.single)
                {
                   // m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + " on Single Element of type " + l_ElementsChildCuLeSetA.SignatureName, i_SourceLocation));
                   // return null;
                }

                if (i_ChildSetB.CuLeSetType is ElementsCuLeSetType)
                {
                    ElementsCuLeSetType l_ElementsChildCuLeSetB = (ElementsCuLeSetType)i_ChildSetB.CuLeSetType;

                    if (l_ElementsChildCuLeSetB.GetMultiplicity == CuLeSetType.Multiplicity.single)
                    {
                       // m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + " on Single Element of type " + l_ElementsChildCuLeSetB.SignatureName, i_SourceLocation));
                       // return null;
                    }


                    if (l_ElementsChildCuLeSetA.SignatureName != l_ElementsChildCuLeSetB.SignatureName
                        || l_ElementsChildCuLeSetA.GetSignatureType != l_ElementsChildCuLeSetB.GetSignatureType)
                    {
                        m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + " on Set of type " + l_ElementsChildCuLeSetA.SignatureName + " and Set of Type " + l_ElementsChildCuLeSetB.SignatureName, i_SourceLocation));
                        return null;
                    }

                    if (l_ElementsChildCuLeSetA.GetSignatureType == CuLeSetType.SignatureType.field
                        && l_ElementsChildCuLeSetB.GetSignatureType == CuLeSetType.SignatureType.field)
                    {
                        CuLeSetType l_CuLeSetType = new MultipleFieldCuLeSetType(l_ElementsChildCuLeSetA.SignatureName, null);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);
                        //return new MultipleFieldCuLeSetType(l_ElementsChildCuLeSetA.SignatureName());

                    }
                    else if (l_ElementsChildCuLeSetA.GetSignatureType == CuLeSetType.SignatureType.tuple
                        && l_ElementsChildCuLeSetB.GetSignatureType == CuLeSetType.SignatureType.tuple)
                    {
                        CuLeSetType l_CuLeSetType = new MultipleTupleCuLeSetType(l_ElementsChildCuLeSetA.SignatureName, m_Tables);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);
                        //return new MultipleTupleCuLeSetType(l_ElementsChildCuLeSetA.SignatureName(), i_Tables);
                    }
                    else
                    {
                        throw new System.ArgumentException("CuLeSetDef Error");
                    }

                    throw new System.ArgumentException("CuLeSetDef Error");

                }
                else if (i_ChildSetB.CuLeSetType is EmptySetCuLeSetType)
                {
                    if (l_ElementsChildCuLeSetA.GetSignatureType == CuLeSetType.SignatureType.field)
                    {

                        CuLeSetType l_CuLeSetType = new MultipleFieldCuLeSetType(l_ElementsChildCuLeSetA.SignatureName, null);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);
                        //return new MultipleFieldCuLeSetType(l_ElementsChildCuLeSetA.SignatureName());
                    }
                    else if (l_ElementsChildCuLeSetA.GetSignatureType == CuLeSetType.SignatureType.tuple)
                    {
                        CuLeSetType l_CuLeSetType = new MultipleTupleCuLeSetType(l_ElementsChildCuLeSetA.SignatureName, m_Tables);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);
                        //return new MultipleTupleCuLeSetType(l_ElementsChildCuLeSetA.SignatureName(), i_Tables);
                    }
                    else
                    {
                        throw new System.ArgumentException("CuLeSetDef Error");
                    }

                }
                else if (i_ChildSetB.CuLeSetType is RelationCuLeSetType)
                {

                    RelationCuLeSetType l_RelationChildSetB = (RelationCuLeSetType)i_ChildSetB.CuLeSetType;

                    m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + "on set of type " + l_ElementsChildCuLeSetA.GetSignatureString + "and Relation of Type " + l_RelationChildSetB.GetSignatureString, i_SourceLocation));
                    return null;

                }
                else  // unknown Set Type ChildSetB
                {
                    throw new System.ArgumentException("CuLeSetDef Error");
                }

                throw new System.ArgumentException("CuLeSetDef Error");

            }
            else if (i_ChildSetA.CuLeSetType is RelationCuLeSetType)
            {
                if (i_ChildSetB.CuLeSetType is RelationCuLeSetType)
                {
                    RelationCuLeSetType RelationCuLeSetA = (RelationCuLeSetType)i_ChildSetA.CuLeSetType;
                    RelationCuLeSetType RelationCuLeSetB = (RelationCuLeSetType)i_ChildSetB.CuLeSetType;

                    if (RelationCuLeSetA.ParentSignature == RelationCuLeSetB.ParentSignature
                        && RelationCuLeSetA.ParentMultiplicity == RelationCuLeSetB.ParentMultiplicity
                        && RelationCuLeSetA.ChildSignature == RelationCuLeSetB.ChildSignature
                        && RelationCuLeSetA.ChildMultiplicity == RelationCuLeSetB.ChildMultiplicity)
                    {
                        CuLeSetType l_CuLeSetType = new RelationCuLeSetType(RelationCuLeSetA.ParentSignature, RelationCuLeSetA.ParentMultiplicity, RelationCuLeSetA.ChildSignature, RelationCuLeSetA.ChildMultiplicity, m_Tables);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);
                        //return new RelationCuLeSetType(RelationCuLeSetA.ParentSignature(), RelationCuLeSetA.ParentMultiplicity(), RelationCuLeSetA.ChildSignature(), RelationCuLeSetA.ChildMultiplicity(), i_Tables);
                    }
                    else
                    {
                        m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + "on Relation of type " + i_ChildSetA.GetSignatureString() + "and Relation of Type " + i_ChildSetB.GetSignatureString(), i_SourceLocation));
                        return null;
                    }

                }
                else if (i_ChildSetB.CuLeSetType is ElementsCuLeSetType)
                {
                    m_errorMessages.Add(new ErrorMessage("Cannot perform " + i_SetOperation.ToString() + "on Relation of type " + i_ChildSetA.GetSignatureString() + "and Set of Type " + i_ChildSetB.GetSignatureString(), i_SourceLocation));
                    return null;
                }
                else if (i_ChildSetB.CuLeSetType is EmptySetCuLeSetType)
                {
                    if (i_SetOperation == CuLeSetDef.SetOperation.Union)
                    {
                        RelationCuLeSetType RelationCuLeSetA = (RelationCuLeSetType)i_ChildSetA.CuLeSetType;

                        CuLeSetType l_CuLeSetType = new RelationCuLeSetType(RelationCuLeSetA.ParentSignature, RelationCuLeSetA.ParentMultiplicity, RelationCuLeSetA.ChildSignature, RelationCuLeSetA.ChildMultiplicity, m_Tables);
                        return new SetOperationCuLeSet(l_CuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);

                        //return new RelationCuLeSetType(RelationCuLeSetA.ParentSignature(), RelationCuLeSetA.ParentMultiplicity(), RelationCuLeSetA.ChildSignature(), RelationCuLeSetA.ChildMultiplicity(), i_Tables);
                    }
                    else //(i_SetOperation == SetOperation.Intersect)
                    {

                        return new EmptyCuLeSet();

                        //return new EmptySetCuLeSetType();
                    }

                }
                else
                {
                    throw new System.ArgumentException("CuLeSetDef Error");
                }

            }
            else if (i_ChildSetA.CuLeSetType is EmptySetCuLeSetType)
            {
                if (i_ChildSetB.CuLeSetType is ElementsCuLeSetType)
                {
                    if (i_SetOperation == CuLeSetDef.SetOperation.Union)
                    {
                        return i_ChildSetB;
                    }
                    else // (i_SetOperation == SetOperation.Intersect)
                    {
                        return i_ChildSetA;
                    }
                }
                else if (i_ChildSetB.CuLeSetType is RelationCuLeSetType)
                {
                    RelationCuLeSetType RelationCuLeSetB = (RelationCuLeSetType)i_ChildSetB.CuLeSetType;

                    RelationCuLeSetType l_RelationCuLeSetType = new RelationCuLeSetType(RelationCuLeSetB.ParentSignature, RelationCuLeSetB.ParentMultiplicity, RelationCuLeSetB.ChildSignature, RelationCuLeSetB.ChildMultiplicity, m_Tables);

                    return new SetOperationCuLeSet(l_RelationCuLeSetType, i_ChildSetA, i_ChildSetB, i_SetOperation);

                    // return new RelationCuLeSetType(RelationCuLeSetB.ParentSignature(), RelationCuLeSetB.ParentMultiplicity(), RelationCuLeSetB.ChildSignature(), RelationCuLeSetB.ChildMultiplicity(), i_Tables);
                }
                else if (i_ChildSetB.CuLeSetType is EmptySetCuLeSetType)
                {
                    return new EmptyCuLeSet();
                    //return new EmptySetCuLeSetType();
                }
                else
                {
                    throw new System.ArgumentException("CuLeSetDef Error");
                }

            }
            else // unknown Set Type ChildSetA
            {
                throw new System.ArgumentException("CuLeSetDef Error");
            }

            throw new System.ArgumentException("CuLeSetDef Error");

        }


        private WhereExpression CreateWhereExpression(WhereExpressionDef i_WhereExpressionDef, CuLeQuantifierList i_QuantifierList, string i_State  )
        {

            if (i_WhereExpressionDef is BinaricWhereExpressionDef)
            {
                BinaricWhereExpressionDef l_BinaricWhereExpressionDef = (BinaricWhereExpressionDef)i_WhereExpressionDef;

                string lv_AttributeName = l_BinaricWhereExpressionDef.AttributeName;
                SetOperator l_SetOperator = l_BinaricWhereExpressionDef.SetOperator;

                CuLeSet l_CuLeSet = this.CreateCuLeSet(l_BinaricWhereExpressionDef.CuLeSetDef, i_QuantifierList, i_State);

                return new BinaricWhereExpression(lv_AttributeName, l_SetOperator, l_CuLeSet, i_State);
            }

            else if (i_WhereExpressionDef is OperatorWhereExpressionDef)
            {

                OperatorWhereExpressionDef l_OperatorWhereExpressionDef = (OperatorWhereExpressionDef)i_WhereExpressionDef;

                WhereExpression l_WhereExpressionA = this.CreateWhereExpression(l_OperatorWhereExpressionDef.WhereExpressionDefA, i_QuantifierList, i_State);
                WhereExpression l_WhereExpressionB = this.CreateWhereExpression(l_OperatorWhereExpressionDef.WhereExpressionDefB, i_QuantifierList, i_State);

                RuleOperator l_RuleOperator = l_OperatorWhereExpressionDef.RuleOperator;

                return new OperatorWhereExpression(l_WhereExpressionA, l_RuleOperator, l_WhereExpressionB);

            }

            else
            {
                throw new SyntaxErrorException("Where Expression Definition Error");

            }

        }

        private WhereClause CreateWhereClause(WhereClauseDef i_WhereClauseDef, CuLeQuantifierList i_CuLeQuantifiers, string i_State)
        {
            WhereExpression l_WhereExpression = this.CreateWhereExpression( i_WhereClauseDef.WhereExpressionDef, i_CuLeQuantifiers, i_State);
            WhereClause o_WhereClause = new WhereClause(l_WhereExpression);

            return o_WhereClause;
        }

        private WhereClauseCuLeSet CreateWhereClauseCuLeSet(WhereClause i_WhereClause, CuLeSet i_ChildCuLeSet, SourceLocation i_SourceLocation, string i_state)
        {


            if (!(i_ChildCuLeSet.CuLeSetType.GetSignatureType == CuLeSetType.SignatureType.tuple))
            {
                m_errorMessages.Add(new ErrorMessage(i_ChildCuLeSet.ToAlloy(null) + " Cannot perform Where Clause on Set of Type " + i_ChildCuLeSet.CuLeSetType.GetSignatureType.ToString() , i_SourceLocation));
                return null;
            }

            if( !( i_ChildCuLeSet.CuLeSetType is TupleCuLeSetType))
            {
                throw new System.ArgumentException("CuLeSetDef Error");
            }

            TupleCuLeSetType l_TupleCuLeSetType = (TupleCuLeSetType)i_ChildCuLeSet.CuLeSetType;

            if( ! ( l_TupleCuLeSetType.GetMultiplicity  == CuLeSetType.Multiplicity.many))
            {
                m_errorMessages.Add(new ErrorMessage(i_ChildCuLeSet.ToAlloy(null) + " Cannot perform Where Clause on single element"  , i_SourceLocation));
                return null;
            }

            string lv_tableName = l_TupleCuLeSetType.SignatureName;


            if (!(m_Tables.Contains(lv_tableName)))
            {
                m_errorMessages.Add(new ErrorMessage(i_ChildCuLeSet.ToAlloy(null) + " Cannot perform Where Clause on unknown Table " + lv_tableName, i_SourceLocation));
                return null;
            }

            DataTable l_DataTable = m_Tables[lv_tableName];

            List<BinaricWhereExpression> l_AllBinaricWhereExpressions = i_WhereClause.WhereExpression.GetAllBinaricWhereExpressions();

            CuLeRuleFactory l_CuLeRuleFactory = new CuLeRuleFactory( m_Tables, m_Relations, null );

            foreach (BinaricWhereExpression l_BinaricWhereExpression in l_AllBinaricWhereExpressions)
            {
                if (!(l_DataTable.Columns.Contains(l_BinaricWhereExpression.AttributeName)))
                {
                    m_errorMessages.Add(new ErrorMessage(l_BinaricWhereExpression.AttributeName + " is not a field of Table " + lv_tableName, i_SourceLocation));
                    return null;
                }

                DataColumn l_DataColumn = l_DataTable.Columns[l_BinaricWhereExpression.AttributeName];

                SingleFieldCuLeSetType l_CuLeSetType = new SingleFieldCuLeSetType(l_DataColumn.DataType.ToString(), null, l_DataColumn.Caption);

                TableFieldAttributeCuLeSet l_TableFieldAttributeCuLeSet = new TableFieldAttributeCuLeSet(l_CuLeSetType, i_ChildCuLeSet, l_BinaricWhereExpression.AttributeName, lv_tableName, i_state);

                SetOperatorCuLeRule l_SetOperatorCuLeRule = l_CuLeRuleFactory.CreateSetOperatorCuLeRule(l_TableFieldAttributeCuLeSet, l_BinaricWhereExpression.Set, l_BinaricWhereExpression.SetOperator, i_SourceLocation, i_state);

                if (l_CuLeRuleFactory.m_errorMessages != null && l_CuLeRuleFactory.m_errorMessages.Count != 0)
                {

                    this.m_errorMessages.AddRange(l_CuLeRuleFactory.m_errorMessages);
                    return null;

                }


            }


            WhereClauseCuLeSet o_WhereClauseCuLeSet = new WhereClauseCuLeSet(i_ChildCuLeSet, i_WhereClause);

            return o_WhereClauseCuLeSet;


        }
    }
}
