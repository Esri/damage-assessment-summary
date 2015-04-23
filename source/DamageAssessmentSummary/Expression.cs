using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary
{
    public class Expression
    {
        private string _op = "";

        public ObservableCollection<string> _appendedOperators = new ObservableCollection<string>() { "AND", "OR" };

        public Expression(string FieldName, string Op, string Value) 
        {
            fieldName = FieldName;
            op = Op;
            value = Value;

            appendedOperator = "AND";
        }

        public Expression(string AdvancedExpression)
        {
            advancedExpression = AdvancedExpression;
        }

        public string fieldName { get; set; }

        public string op 
        {
            get {
                Operators operators = new Operators();
                return operators.getOperatorFromString(_op); 
            }
            set { _op = value;} 
        }

        public string value { get; set; }

        public string expression
        {
            get 
            {
                //will need to handle conversions and whatnot
                return String.Format("{0} {1} {2}", fieldName, op, value);
            }
        }

        public string advancedExpression
        {
            get;
            set;
        }

        public string appendedOperator
        {
            get;
            set;
        }
    }
}
