using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary.Model
{
    [DataContract]
    public class Operators
    {
        ObservableCollection<string> _operatorStrings;
        private ObservableCollection<string> _appendedOperators;

        public Operators()
        {
            ////TODO make sure this doesn't overwrite if OR is saved
            currentAppendedOperator = "AND";
        }

        
        [DataMember]
        public ObservableCollection<string> operatorStrings
        {
            get
            {
                if (_operatorStrings == null)
                {
                    _operatorStrings = new ObservableCollection<string>(){
                        "Equal To",
                        "Not Equal To",
                        "Less Than",
                        "Greator Than"
                    };
                }
                return _operatorStrings;
            }
            set
            {
                //TODO is this necessary
                _operatorStrings = value;
            }
        }

        [DataMember]
        public ObservableCollection<string> appendedOperators
        {
            get
            {
                if (_appendedOperators == null)
                {
                    _appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
                }
                return _appendedOperators;
            }

            set
            {
                //TODO is this necessary
                _appendedOperators = value;
            }
        }

        //[System.ComponentModel.DefaultValue("AND")]
        [DataMember]
        public string currentAppendedOperator
        {
            get;
            set;
        }


    }
}
