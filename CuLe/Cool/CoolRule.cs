using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;
using System.Data;

namespace CuLe
{

    public abstract class CuLeRule
    {
        abstract public string ToAlloy(string i_State);

        abstract public List<ConstantValueCuLeSet> GetConstantValueCuLeSets();

        protected string m_state;

        public string State
        {
            get
            {
                return m_state;
            }
        }

    }


    public class SetOperatorCuLeRule : CuLeRule
    {
        private CuLeSet m_SetA;
        private CuLeSet m_SetB;
        private SetOperator m_SetOperator;

        public CuLeSet SetA
        {
            get
            {
                return m_SetA;
            }
        }

        public CuLeSet SetB
        {
            get
            {
                return m_SetB;
            }
        }

        public SetOperator SetOperator
        {
            get
            {
                return m_SetOperator;
            }
        }

        public SetOperatorCuLeRule(CuLeSet i_SetA, CuLeSet i_SetB, SetOperator i_SetOperator, string i_state)
        {

            m_SetA = i_SetA;
            m_SetB = i_SetB;
            m_SetOperator = i_SetOperator;

            m_state = i_state;
        }

        public override string ToAlloy(string i_State)
        {

            switch (m_SetOperator)
            {

                case SetOperator.EQ:

                    

                    return "( " + m_SetA.ToAlloy(i_State) + " = " + m_SetB.ToAlloy(i_State) + " )";

                case SetOperator.GE:

                    return "( gte [ " + m_SetA.ToAlloy(i_State) + ", " + m_SetB.ToAlloy(i_State) + " ] )";

                    //return "( " + m_SetA.ToAlloy() + " >= " + m_SetB.ToAlloy() + " )";

                case SetOperator.GT:

                    return "( gt [ " + m_SetA.ToAlloy(i_State) + ", " + m_SetB.ToAlloy(i_State) + " ] )";

                    //return "( " + m_SetA.ToAlloy() + " > " + m_SetB.ToAlloy() + " )";

                case SetOperator.IsSubsetOf:

                    return "( " + m_SetA.ToAlloy(i_State) + " in " + m_SetB.ToAlloy(i_State) + " )";

                case SetOperator.NE:

                    return "( not ( " + m_SetA.ToAlloy(i_State) + " = " + m_SetB.ToAlloy(i_State) + " ) )";

                case SetOperator.SE:

                    return "( lte [ " + m_SetA.ToAlloy(i_State) + ", " + m_SetB.ToAlloy(i_State) + " ] )";

                    //return "( " + m_SetA.ToAlloy() + " <= " + m_SetB.ToAlloy() + " )";

                case SetOperator.ST:

                    return "( lt [ " + m_SetA.ToAlloy(i_State) + ", " + m_SetB.ToAlloy(i_State) + " ] )";

                    //return "( " + m_SetA.ToAlloy() + " < " + m_SetB.ToAlloy() + " )";

                default:
                    throw new SystemException("Unknown Set Operator");

            }
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_SetA.GetConstantValueCuLeSets().Concat(m_SetB.GetConstantValueCuLeSets()).ToList();
        }

    }

    public class NegationCuLeRule : CuLeRule
    {
        private CuLeRule m_CuLeRule;

        public CuLeRule CuLeRule()
        {
            return m_CuLeRule;
        }

        public NegationCuLeRule(CuLeRule i_CuLeRule)
        {
            m_CuLeRule = i_CuLeRule;
        }

        public override string ToAlloy(string i_State)
        {
            return "( not " + m_CuLeRule.ToAlloy(i_State) + " )";
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_CuLeRule.GetConstantValueCuLeSets();
        }
    }

    public class RuleOperatorCuLeRule : CuLeRule
    {
        private RuleOperator m_RuleOperator;

        private CuLeRule m_CuLeRule_A;
        private CuLeRule m_CuLeRule_B;

        public RuleOperator RuleOperator
        {
            get
            {
                return m_RuleOperator;
            }
        }
        public CuLeRule CuLeRule_A()
        {
            return m_CuLeRule_A;
        }

        public CuLeRule CuLeRule_B()
        {
            return m_CuLeRule_B;
        }

        public RuleOperatorCuLeRule(CuLeRule i_CuLeRule_A, CuLeRule i_CuLeRule_B, RuleOperator i_RuleOperator)
        {
            m_CuLeRule_A = i_CuLeRule_A;
            m_CuLeRule_B = i_CuLeRule_B;
            m_RuleOperator = i_RuleOperator;
        }

        public override string ToAlloy(string i_State)
        {
            switch (m_RuleOperator)
            {
                case RuleOperator.AND:

                    return "( " + m_CuLeRule_A.ToAlloy(i_State) + " and " + m_CuLeRule_B.ToAlloy(i_State) + " )";

                case RuleOperator.OR:

                    return "( " + m_CuLeRule_A.ToAlloy(i_State) + " or " + m_CuLeRule_B.ToAlloy(i_State) + " )";

                default:

                    throw new ArgumentException("Unknown Rule Operator");

            }
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_CuLeRule_A.GetConstantValueCuLeSets().Concat(m_CuLeRule_B.GetConstantValueCuLeSets()).ToList();
        }

    }

    public abstract class ConditionalCuLeRule : CuLeRule
    {
    }

    public abstract class ImplicationCuLeRule : ConditionalCuLeRule
    {

        protected CuLeRule m_CuLeRule_IF;
        protected CuLeRule m_CuLeRule_THEN;


        public CuLeRule CuLeRule_IF()
        {
            return m_CuLeRule_IF;
        }

        public CuLeRule CuLeRule_THEN()
        {
            return m_CuLeRule_THEN;
        }


    }

    public class IF_ImplicationCuLeRule : ImplicationCuLeRule
    {


        public IF_ImplicationCuLeRule(CuLeRule i_CuLeRule_IF, CuLeRule i_CuLeRule_THEN)
        {

            m_CuLeRule_IF = i_CuLeRule_IF;
            m_CuLeRule_THEN = i_CuLeRule_THEN;
        }

        public override string ToAlloy(String i_State)
        {
            return "( " + m_CuLeRule_IF.ToAlloy(i_State) + " implies " + m_CuLeRule_THEN.ToAlloy(i_State) + " )";
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_CuLeRule_IF.GetConstantValueCuLeSets().Concat(m_CuLeRule_THEN.GetConstantValueCuLeSets()).ToList();
        }
    }

    public class IF_ELSE_ImplicationCuLeRule : ImplicationCuLeRule
    {
        protected CuLeRule m_CuLeRule_ELSE;

        public CuLeRule CuLeRule_ELSE()
        {
            return m_CuLeRule_ELSE;
        }

        public IF_ELSE_ImplicationCuLeRule(CuLeRule i_CuLeRule_IF, CuLeRule i_CuLeRule_THEN, CuLeRule i_CuLeRule_ELSE)
        {
            m_CuLeRule_IF = i_CuLeRule_IF;
            m_CuLeRule_THEN = i_CuLeRule_THEN;
            m_CuLeRule_ELSE = i_CuLeRule_ELSE;

        }

        public override string ToAlloy(String i_State)
        {
            return "( " + m_CuLeRule_IF.ToAlloy(i_State) + " => " + m_CuLeRule_THEN.ToAlloy(i_State) + " else " + m_CuLeRule_ELSE.ToAlloy(i_State) + " )";
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return m_CuLeRule_IF.GetConstantValueCuLeSets().Concat(m_CuLeRule_THEN.GetConstantValueCuLeSets()).ToList().Concat(m_CuLeRule_ELSE.GetConstantValueCuLeSets()).ToList();
        }

    }

    public class EquivalenceCuLeRule : ConditionalCuLeRule
    {
        protected CuLeRule m_CuLeRule_A;
        protected CuLeRule m_CuLeRule_B;

        public CuLeRule CuLeRule_A()
        {
            return m_CuLeRule_A;
        }

        public CuLeRule CuLeRule_B()
        {
            return m_CuLeRule_B;
        }

        public EquivalenceCuLeRule(CuLeRule i_CuLeRule_A, CuLeRule i_CuLeRule_B)
        {
            m_CuLeRule_A = i_CuLeRule_A;
            m_CuLeRule_B = i_CuLeRule_B;
        }

        public override string ToAlloy(String i_State)
        {
            return "( " + m_CuLeRule_A.ToAlloy(i_State) + " iff " + m_CuLeRule_B.ToAlloy(i_State) + " )";
        }

        public override List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            return (m_CuLeRule_A.GetConstantValueCuLeSets().Concat(m_CuLeRule_B.GetConstantValueCuLeSets()).ToList());
        }

    }



    public class CuLeRuleFactory
    {

        public List<ErrorMessage> m_errorMessages;

        private DataTableCollection m_Tables;

        private DataRelationCollection m_Relations;

        private List<String> m_States;

        public CuLeRuleFactory(DataTableCollection i_Tables, DataRelationCollection i_Relations, List<String> i_States)
        {
            m_errorMessages = new List<ErrorMessage>();

            m_Tables = i_Tables;

            m_Relations = i_Relations;

            m_States = i_States;
        }

        public CuLeRule CreateCuLeRule(CuLeRuleDef i_CuLeRuleDef,  CuLeQuantifierList i_CuLeQuantifierList, string i_State)
        {



            if (i_CuLeRuleDef is SetOperatorCuLeRuleDef)
            {

                SetOperatorCuLeRuleDef l_SetOperatorCuLeRuleDef = (SetOperatorCuLeRuleDef)i_CuLeRuleDef;

                CuLeSetFactory l_CuLeSetFactory = new CuLeSetFactory(m_Tables, m_Relations, this.m_States);

                CuLeSet SetA = l_CuLeSetFactory.CreateCuLeSet(l_SetOperatorCuLeRuleDef.SetDefA(), i_CuLeQuantifierList, i_State);
                CuLeSet SetB = l_CuLeSetFactory.CreateCuLeSet(l_SetOperatorCuLeRuleDef.SetDefB(), i_CuLeQuantifierList, i_State);

                if (l_CuLeSetFactory.m_errorMessages != null)
                {
                    if (l_CuLeSetFactory.m_errorMessages.Count != 0)
                    {
                        this.m_errorMessages = l_CuLeSetFactory.m_errorMessages;

                    }
                }

                if (SetA != null && SetB != null)
                {

                    return CreateSetOperatorCuLeRule(SetA, SetB, l_SetOperatorCuLeRuleDef.SetOperator, l_SetOperatorCuLeRuleDef.SourceLocation, i_State );

                }

                if (this.m_errorMessages != null)
                {
                    return null;
                }

                return null;
            }

            if (i_CuLeRuleDef is NegationCuLeRuleDef)
            {
                NegationCuLeRuleDef l_NegationCuLeRuleDef = (NegationCuLeRuleDef)i_CuLeRuleDef;

                CuLeRule l_CuLeRule_A = this.CreateCuLeRule(l_NegationCuLeRuleDef.CuLeRuleDef_A(), i_CuLeQuantifierList, i_State);

                return new NegationCuLeRule(l_CuLeRule_A);

                // CuLeRule l_CuLeRule = this.CreateCuLeRule(l_NegationCuLeRuleDef.CuLeRuleDef_A, i_Tables, i_Relations, i_CuLeQuantifierList)

            }

            if (i_CuLeRuleDef is RuleOperatorCuLeRuleDef)
            {
                RuleOperatorCuLeRuleDef l_RuleOperatorCuLeRuleDef = (RuleOperatorCuLeRuleDef)i_CuLeRuleDef;

                CuLeRule l_CuLeRule_A = this.CreateCuLeRule(l_RuleOperatorCuLeRuleDef.CuLeRuleDef_A(), i_CuLeQuantifierList, i_State);
                CuLeRule l_CuLeRule_B = this.CreateCuLeRule(l_RuleOperatorCuLeRuleDef.CuLeRuleDef_B(), i_CuLeQuantifierList, i_State);

                return new RuleOperatorCuLeRule(l_CuLeRule_A, l_CuLeRule_B, l_RuleOperatorCuLeRuleDef.RuleOperator);

            }

            if (i_CuLeRuleDef is ConditionalCuLeRuleDef)
            {

                if (i_CuLeRuleDef is ImplicationCuLeRuleDef)
                {
                    if (i_CuLeRuleDef is IF_ImplicationCuLeRuleDef)
                    {

                        IF_ImplicationCuLeRuleDef l_IF_ImplicationCuLeRuleDef = (IF_ImplicationCuLeRuleDef)i_CuLeRuleDef;

                        CuLeRule l_CuLeRule_IF = this.CreateCuLeRule(l_IF_ImplicationCuLeRuleDef.CuLeRuleDef_IF(), i_CuLeQuantifierList, i_State);
                        CuLeRule l_CuLeRule_THEN = this.CreateCuLeRule(l_IF_ImplicationCuLeRuleDef.CuLeRuleDef_THEN(), i_CuLeQuantifierList, i_State);

                        return new IF_ImplicationCuLeRule(l_CuLeRule_IF, l_CuLeRule_THEN);

                    }
                    if (i_CuLeRuleDef is IF_ELSE_ImplicationCuLeRuleDef)
                    {
                        IF_ELSE_ImplicationCuLeRuleDef l_IF_ELSE_ImplicationCuLeRuleDef = (IF_ELSE_ImplicationCuLeRuleDef)i_CuLeRuleDef;

                        CuLeRule l_CuLeRule_IF = this.CreateCuLeRule(l_IF_ELSE_ImplicationCuLeRuleDef.CuLeRuleDef_IF(), i_CuLeQuantifierList, i_State);
                        CuLeRule l_CuLeRule_THEN = this.CreateCuLeRule(l_IF_ELSE_ImplicationCuLeRuleDef.CuLeRuleDef_THEN(), i_CuLeQuantifierList, i_State);
                        CuLeRule l_CuLeRule_ELSE = this.CreateCuLeRule(l_IF_ELSE_ImplicationCuLeRuleDef.CuLeRuleDef_ELSE(), i_CuLeQuantifierList, i_State);

                        return new IF_ELSE_ImplicationCuLeRule(l_CuLeRule_IF, l_CuLeRule_THEN, l_CuLeRule_ELSE);
                    }

                }
                if (i_CuLeRuleDef is EquivalenceCuLeRuleDef)
                {

                    EquivalenceCuLeRuleDef l_EquivalenceCuLeRuleDef = (EquivalenceCuLeRuleDef)i_CuLeRuleDef;

                    CuLeRule l_CuLeRule_A = this.CreateCuLeRule(l_EquivalenceCuLeRuleDef.CuLeRuleDef_A(), i_CuLeQuantifierList, i_State);
                    CuLeRule l_CuLeRule_B = this.CreateCuLeRule(l_EquivalenceCuLeRuleDef.CuLeRuleDef_B(), i_CuLeQuantifierList, i_State);

                    return new EquivalenceCuLeRule(l_CuLeRule_A, l_CuLeRule_B);

                }

            }

            throw new ArgumentException("Rule Definition Error");


        }// End of Create CuLeRule

        public SetOperatorCuLeRule CreateSetOperatorCuLeRule(CuLeSet i_SetA, CuLeSet i_SetB, SetOperator i_setOperator, SourceLocation i_sourceLocation, string i_state)
        {

            ElementsCuLeSetType lv_ElementsCuLeSetTypeA;
            ElementsCuLeSetType lv_ElementsCuLeSetTypeB;



            switch (i_setOperator)
            {

                case SetOperator.IsSubsetOf:

                    if (i_SetA.CuLeSetType.GetSignatureType == CuLeSetType.SignatureType.emptySet)
                    {
                        return new SetOperatorCuLeRule(i_SetA, i_SetB, i_setOperator, i_state);
                    }

                    if (i_SetA.CuLeSetType.GetSignatureType != i_SetB.CuLeSetType.GetSignatureType)
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of type " + i_SetA.CuLeSetType.GetSignatureType + " cannot be a Subset of a set of type " + i_SetB.CuLeSetType.GetSignatureType, i_sourceLocation));
                        return null;
                    }

                    if (!(i_SetA.CuLeSetType is ElementsCuLeSetType && i_SetB.CuLeSetType is ElementsCuLeSetType))
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of type " + i_SetA.CuLeSetType.GetSignatureType + " cannot be a Subset of a set of type " + i_SetB.CuLeSetType.GetSignatureType, i_sourceLocation));
                        return null;
                    }

                    lv_ElementsCuLeSetTypeA = (ElementsCuLeSetType)i_SetA.CuLeSetType;
                    lv_ElementsCuLeSetTypeB = (ElementsCuLeSetType)i_SetB.CuLeSetType;

                    if (lv_ElementsCuLeSetTypeA.SignatureName != lv_ElementsCuLeSetTypeB.SignatureName)
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of type " + lv_ElementsCuLeSetTypeA.SignatureName + " cannot be a Subset of a set of type " + lv_ElementsCuLeSetTypeB.SignatureName, i_sourceLocation));
                        return null;
                    }

                    if (lv_ElementsCuLeSetTypeA.GetMultiplicity == CuLeSetType.Multiplicity.many && lv_ElementsCuLeSetTypeA.GetMultiplicity == CuLeSetType.Multiplicity.single)
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of multiple elements cannot be a assigned as a Subset of a single element", i_sourceLocation));
                        return null;
                    }

                    return new SetOperatorCuLeRule(i_SetA, i_SetB, i_setOperator, i_state);

                case SetOperator.EQ:
                case SetOperator.NE:


                    if (i_SetA.CuLeSetType.GetSignatureType != i_SetB.CuLeSetType.GetSignatureType)
                    {
                        if (i_SetA.CuLeSetType.GetSignatureType != CuLeSetType.SignatureType.emptySet && i_SetB.CuLeSetType.GetSignatureType != CuLeSetType.SignatureType.emptySet)
                        {
                            m_errorMessages.Add(new ErrorMessage("Set of type " + i_SetA.CuLeSetType.GetSignatureType + " cannot be a compared to a set of type " + i_SetB.CuLeSetType.GetSignatureType, i_sourceLocation));
                            return null;
                        }
                    }

                    if (i_SetA.CuLeSetType is ElementsCuLeSetType && i_SetB.CuLeSetType is ElementsCuLeSetType)
                    {

                        lv_ElementsCuLeSetTypeA = (ElementsCuLeSetType)i_SetA.CuLeSetType;
                        lv_ElementsCuLeSetTypeB = (ElementsCuLeSetType)i_SetB.CuLeSetType;


                        if (lv_ElementsCuLeSetTypeA.GetMultiplicity != lv_ElementsCuLeSetTypeA.GetMultiplicity)
                        {
                            m_errorMessages.Add(new ErrorMessage("Set of multiple elements cannot be compared to a single element", i_sourceLocation));
                            return null;
                        }

                        if (lv_ElementsCuLeSetTypeA.SignatureName != lv_ElementsCuLeSetTypeB.SignatureName)
                        {
                            m_errorMessages.Add(new ErrorMessage("Set of type " + lv_ElementsCuLeSetTypeA.SignatureName + " cannot be compared to a set of type " + lv_ElementsCuLeSetTypeB.SignatureName, i_sourceLocation));
                            return null;
                        }

                        if (i_SetA.CuLeSetType is FieldCuLeSetType && i_SetB.CuLeSetType is FieldCuLeSetType)
                        {
                            SingleFieldCuLeSetType lv_FieldCuLeSetTypeA = (SingleFieldCuLeSetType)i_SetA.CuLeSetType;
                            SingleFieldCuLeSetType lv_FieldCuLeSetTypeB = (SingleFieldCuLeSetType)i_SetB.CuLeSetType;

                            if (lv_FieldCuLeSetTypeA.FieldType != lv_FieldCuLeSetTypeB.FieldType)
                            {
                                m_errorMessages.Add(new ErrorMessage("Set of type " + lv_FieldCuLeSetTypeA.FieldType.Name + " cannot be compared to a Set of type " + lv_FieldCuLeSetTypeB.FieldType.Name, i_sourceLocation));
                                return null;
                            }

                            if (lv_FieldCuLeSetTypeA.FieldCaption != lv_FieldCuLeSetTypeB.FieldCaption)
                            {
                                m_errorMessages.Add(new ErrorMessage("Set of type " + lv_FieldCuLeSetTypeA.FieldCaption + " cannot be compared to a set of type " + lv_FieldCuLeSetTypeB.FieldCaption, i_sourceLocation));
                                return null;
                            }
                        }

                        
                    }

                    return new SetOperatorCuLeRule(i_SetA, i_SetB, i_setOperator, i_state);


                case SetOperator.GE:
                case SetOperator.GT:
                case SetOperator.SE:
                case SetOperator.ST:


                    if (!(i_SetA.CuLeSetType is ElementsCuLeSetType && i_SetB.CuLeSetType is ElementsCuLeSetType))
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of type " + i_SetA.CuLeSetType.GetSignatureType + " cannot be compared to a Set of type " + i_SetA.CuLeSetType.GetSignatureType, i_sourceLocation));
                        return null;
                    }

                    lv_ElementsCuLeSetTypeA = (ElementsCuLeSetType)i_SetA.CuLeSetType;
                    lv_ElementsCuLeSetTypeB = (ElementsCuLeSetType)i_SetB.CuLeSetType;

                    if (lv_ElementsCuLeSetTypeA.SignatureName != lv_ElementsCuLeSetTypeB.SignatureName)
                    {
                        m_errorMessages.Add(new ErrorMessage("Set of type " + lv_ElementsCuLeSetTypeA.SignatureName + " cannot be compared to a set of type " + lv_ElementsCuLeSetTypeB.SignatureName, i_sourceLocation));
                        return null;
                    }

                    if (lv_ElementsCuLeSetTypeA.GetMultiplicity == CuLeSetType.Multiplicity.many || lv_ElementsCuLeSetTypeA.GetMultiplicity == CuLeSetType.Multiplicity.many)
                    {
                        m_errorMessages.Add(new ErrorMessage("Cannot compare set of multiple elements", i_sourceLocation));
                        return null;
                    }

                    if (!(i_SetA.CuLeSetType is FieldCuLeSetType && i_SetA.CuLeSetType is FieldCuLeSetType))
                    {

                        m_errorMessages.Add(new ErrorMessage("Ordered Set comparison can only be applied to single value elements ", i_sourceLocation));
                        return null;
                    }

                    if (!(i_SetA.CuLeSetType is SingleFieldCuLeSetType && i_SetA.CuLeSetType is SingleFieldCuLeSetType))
                    {
                        m_errorMessages.Add(new ErrorMessage("Ordered Set comparison cannot be applied to sets of multiple elements", i_sourceLocation));
                        return null;
                    }

                    SingleFieldCuLeSetType lv_SingleFieldCuLeSetTypeA = (SingleFieldCuLeSetType)i_SetA.CuLeSetType;
                    SingleFieldCuLeSetType lv_SingleFieldCuLeSetTypeB = (SingleFieldCuLeSetType)i_SetB.CuLeSetType;

                    if (lv_SingleFieldCuLeSetTypeA.FieldType != lv_SingleFieldCuLeSetTypeB.FieldType)
                    {
                        m_errorMessages.Add(new ErrorMessage("Ordered Set comparison cannot be applied to element of type " + lv_SingleFieldCuLeSetTypeA.FieldType.Name + " and element of type " + lv_SingleFieldCuLeSetTypeB.FieldType.Name, i_sourceLocation));
                        return null;
                    }

                    if (lv_SingleFieldCuLeSetTypeA.FieldCaption != lv_SingleFieldCuLeSetTypeB.FieldCaption)
                    {
                        m_errorMessages.Add(new ErrorMessage("Ordered Set comparison cannot be applied to element of type " + lv_SingleFieldCuLeSetTypeA.FieldCaption + " and element of type " + lv_SingleFieldCuLeSetTypeB.FieldCaption, i_sourceLocation));
                        return null;
                    }

                    switch (lv_SingleFieldCuLeSetTypeA.FieldType.FullName)
                    {

                        case "System.DateTime":
                        case "System.Decimal":
                        case "System.Double":
                        case "System.Int16":
                        case "System.Int32":
                        case "System.Int64":

                            return new SetOperatorCuLeRule(i_SetA, i_SetB, i_setOperator, i_state);

                        default:

                            m_errorMessages.Add(new ErrorMessage("Ordered Set comparison cannot be applied to elements of type " + lv_SingleFieldCuLeSetTypeA.FieldType.FullName, i_sourceLocation));
                            return null;

                    }

            }

            return null;


       }

    }

   




    public class CuLeRuleGroup
    {

        public List<CuLeQuantifier> m_CuLeQuantifiers;
        public List<CuLeRule> m_CuLeRules;

        private string m_name;
        //public List<ConstantValueCuLeSet> m_ConstantValueCuLeSets;

        public String ToAlloyBusinessRule()
        {

            string o_AlloyString = "";
            string l_RulesString = "";
            string l_QuantifierString = "";


           // string l_quantifiersString = "";


           

            if (m_CuLeQuantifiers != null)
            {

                foreach (CuLeQuantifier l_CuLeQuantifier in m_CuLeQuantifiers)
                {
                    if (!(l_CuLeQuantifier is CuLeSystemField))
                    {
                        l_QuantifierString = l_QuantifierString + ", " + l_CuLeQuantifier.ToAlloy("State_X");
                    }
                }
            }               
            
            if (m_CuLeRules != null)
            {
                foreach (CuLeRule l_CuLeRule in m_CuLeRules)
                {

                    if (l_RulesString == "")
                    {
                        l_RulesString = l_RulesString + l_CuLeRule.ToAlloy("State_X");
                    }
                    else
                    {
                        l_RulesString = l_RulesString + " and " + l_CuLeRule.ToAlloy("State_X");
                    }
                }

                l_RulesString = " ( " + l_RulesString + " ) ";
            }


            o_AlloyString = "// Business_Rule_" + this.m_name + "\n";

            o_AlloyString = o_AlloyString + "assert Business_Rule_" + this.m_name + " { all State_X:State " + l_QuantifierString + "| " + l_RulesString + " }";

            o_AlloyString = o_AlloyString + "\n";

            o_AlloyString = o_AlloyString + "fact { all State_X:State " + l_QuantifierString + "| " + " (State_X = first) implies " + l_RulesString + " }";

            o_AlloyString = o_AlloyString + "\n";

            o_AlloyString = o_AlloyString + "check Business_Rule_" + this.m_name;

            return o_AlloyString;
        }


        public String ToAlloyAssertion(string i_State)
        {

            string o_AlloyString = null;
            string l_RulesString = null;
            string l_QuantifierString = null;

            if (m_CuLeQuantifiers != null)
            {

                 

                foreach (CuLeQuantifier l_CuLeQuantifier in m_CuLeQuantifiers)
                {
                    if (!(l_CuLeQuantifier is CuLeSystemField))
                    {
                        if (l_QuantifierString == null)
                        {
                            l_QuantifierString = l_CuLeQuantifier.ToAlloy(i_State);
                        }
                        else
                        {
                            l_QuantifierString = l_QuantifierString + ", " + l_CuLeQuantifier.ToAlloy(i_State);
                        }
                    }
                }

                l_QuantifierString = "all " + l_QuantifierString;
            }

            if (m_CuLeRules != null)
            {
                foreach (CuLeRule l_CuLeRule in m_CuLeRules)
                {

                    l_RulesString = l_RulesString + l_CuLeRule.ToAlloy(i_State) + "\n";

                    //if (l_RulesString == "")
                    //{
                    //    l_RulesString = l_RulesString + l_CuLeRule.ToAlloy(i_State);
                    //}
                    //else
                    //{
                    //    l_RulesString = l_RulesString + " and " + l_CuLeRule.ToAlloy(i_State);
                    //}
                }

               // l_RulesString = " ( " + l_RulesString + " ) ";
            }

            o_AlloyString = l_RulesString;

            if (l_QuantifierString != null)
            {
                o_AlloyString = l_QuantifierString + "| " + o_AlloyString;
            }

            //o_AlloyString = "// Business_Rule_" + this.m_name + "\n";

            //o_AlloyString = o_AlloyString + "assert Business_Rule_" + this.m_name + " { all State_X:State " + l_QuantifierString + "| " + l_RulesString + " }";

            //o_AlloyString = o_AlloyString + "\n";

            //o_AlloyString = o_AlloyString + "fact { all State_X:State " + l_QuantifierString + "| " + " (State_X = first) implies " + l_RulesString + " }";

            //o_AlloyString = o_AlloyString + "\n";

            //o_AlloyString = o_AlloyString + "check Business_Rule_" + this.m_name;



            return o_AlloyString ;

        }
        public CuLeRuleGroup(string i_name)
        {
            m_CuLeQuantifiers = new List<CuLeQuantifier>();
            m_CuLeRules = new List<CuLeRule>();
            m_name = i_name;
           // m_ConstantValueCuLeSets = new List<ConstantValueCuLeSet>();            

        }

        public List<ConstantValueCuLeSet> GetConstantValueCuLeSets()
        {
            List<ConstantValueCuLeSet> l_ListOfConstantValueCuLeSets = new List<ConstantValueCuLeSet>();

            if (m_CuLeRules == null)
            {
                return l_ListOfConstantValueCuLeSets;
            }


            foreach (CuLeRule l_CuLeRule in m_CuLeRules)
            {
                l_ListOfConstantValueCuLeSets = (l_ListOfConstantValueCuLeSets.Concat(l_CuLeRule.GetConstantValueCuLeSets()).ToList());
            }

            return l_ListOfConstantValueCuLeSets.Distinct().ToList();
        }

    }
}


