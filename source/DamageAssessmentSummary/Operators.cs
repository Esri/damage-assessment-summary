using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary
{
    public class Operators
    {
        public Operators()
        {
            currentAppendedOperator = "AND";
        }

        private Dictionary<string, string> opStringMap = new Dictionary<string,string>(){
            {"Equal To", "="},
            {"Not Equal To", "<>"},
            {"Less Than", "<"},
            {"Greator Than", ">"}
        };

        private ObservableCollection<string> _appendedOperators = new ObservableCollection<string>() { "AND", "OR" };

        //public List<string> getOpertorStrings()
        //{
        //    //return new List<string>(){
        //    //    "Contains",
        //    //    "Equal",
        //    //    "Not Equal",
        //    //    "Is blank",
        //    //    "Is not blank"
        //    //};
        //    return new List<string>(){
        //        "Equal To",
        //        "Not Equal To",
        //        "Less Than",
        //        "Greator Than"
        //    };
        //}

        public ObservableCollection<string> operatorStrings
        {
            get
            {
                return new ObservableCollection<string>(){
                    "Equal To",
                    "Not Equal To",
                    "Less Than",
                    "Greator Than"
                };
            }
        }

        public ObservableCollection<string> appendedOperators
        {
            get
            {
                return _appendedOperators;
            }
        }

        public string currentAppendedOperator
        {
            get;
            set;
        }

        public string getOperatorFromString(string opString)
        {
            return opStringMap[opString];
        }
    }
}
