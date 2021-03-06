 /*
 Copyright 2015 Esri

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.​
*/

﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ConfigureSummaryReport.Model
{
    [DataContract]
    public class Expression : INotifyPropertyChanged
    {
        private bool _isVisible;
        private string _expression;

        public Expression()
        {
            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
        }

        public Expression(string FieldName, string Op, string Value) 
        {
            fieldName = FieldName;
            op = Op;
            value = Value;

            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
        }

        public Expression(string AdvancedExpression)
        {
            advancedExpression = AdvancedExpression;

            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
        }

        [DataMember]
        public string fieldName { get; set; }

        [DataMember]
        public fieldType FieldType { get; set; }

        [DataMember]
        public string op { get; set; }

        [DataMember]
        public string value { get; set; }

        [DataMember]
        public string expression
        {
            get 
            {
                if (advancedExpression == null)
                {
                    string formatString = "{0} {1} {2}";

                    //Currently only special handeling for String fields   
                    switch (this.FieldType)
                    {
                        case fieldType.Blob:
                            break;
                        case fieldType.Date:
                            break;
                        case fieldType.Double:
                            break;
                        case fieldType.GUID:
                            break;
                        case fieldType.Geometry:
                            break;
                        case fieldType.GlobalID:
                            break;
                        case fieldType.Integer:
                            break;
                        case fieldType.OID:
                            break;
                        case fieldType.Raster:
                            break;
                        case fieldType.Single:
                            break;
                        case fieldType.SmallInteger:
                            break;
                        case fieldType.String:
                            formatString = "{0} {1} '{2}'";
                            break;
                        case fieldType.Unknown:
                            break;
                        case fieldType.XML:
                            break;
                        default:
                            break;
                    }

                    return String.Format(formatString, fieldName, getOperatorFromString(op), value);
                }
                else 
                {
                    return advancedExpression;
                }
            }
            set { _expression = value; }
        }

        [DataMember]
        public string advancedExpression { get; set; }

        [DataMember]
        public string appendedOperator { get; set; }

        [DataMember]
        public ObservableCollection<string> appendedOperators { get; set; }

        [DataMember]
        public bool isVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value;
                OnPropertyChanged("isVisible");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public string getOperatorFromString(string opString)
        {
            Dictionary<string, string> opStringMap = new Dictionary<string, string>(){
                {"Equal To", "="},
                {"Not Equal To", "<>"},
                {"Less Than", "<"},
                {"Greator Than", ">"}
            };
            return opStringMap[opString];
        }

        public enum fieldType { Blob, Date, Double, GUID, Geometry, GlobalID, Integer, OID, Raster,
            Single, SmallInteger, String, Unknown, XML }
    }
}
