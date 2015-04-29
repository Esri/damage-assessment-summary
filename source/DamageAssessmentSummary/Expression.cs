using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DamageAssessmentSummary
{
    [DataContract]
    public class Expression : INotifyPropertyChanged
    {
        private string _op = "";
        private bool _isVisible;
        //public Visibility _appendedOperatorVisible;

        public Expression()
        {
            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
            //appendedOperatorVisible = Visibility.Hidden;
        }

        public Expression(string FieldName, string Op, string Value) 
        {
            fieldName = FieldName;
            op = Op;
            value = Value;

            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
            //appendedOperatorVisible = Visibility.Hidden;
        }

        public Expression(string AdvancedExpression)
        {
            advancedExpression = AdvancedExpression;

            appendedOperator = "AND";
            appendedOperators = new ObservableCollection<string>() { "AND", "OR" };
            isVisible = false;
            //appendedOperatorVisible = Visibility.Hidden;
        }

        [DataMember]
        public string fieldName { get; set; }

        //[DataMember]
        //public ESRI.ArcGIS.Client.Field.FieldType fieldType { get; set; }

        [DataMember]
        public string op
        {
            get
            {
                return _op;
            }
            set { _op = value; }
        }

        [DataMember]
        public string value { get; set; }

        public string _p;
        [DataMember]
        public string expression
        {
            get 
            {
                string formatString = "{0} {1} {2}";

                ////Currently only special handeling for String fields
                //switch (this.fieldType)
                //{
                //    case ESRI.ArcGIS.Client.Field.FieldType.Blob:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Date:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Double:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.GUID:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Geometry:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.GlobalID:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Integer:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.OID:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Raster:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Single:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.SmallInteger:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.String:
                //        formatString = "{0} {1} '{2}'";
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.Unknown:
                //        break;
                //    case ESRI.ArcGIS.Client.Field.FieldType.XML:
                //        break;
                //    default:
                //        break;
                //}
                //will need to handle conversions and whatnot
                
                return String.Format(formatString, fieldName, getOperatorFromString(op), value);
                //return "A";
            }
            set { _p = value; }
        }

        [DataMember]
        public string advancedExpression
        {
            get;
            set;
        }

        [DataMember]
        public string appendedOperator
        {
            get;
            set;
        }

        [DataMember]
        public ObservableCollection<string> appendedOperators
        {
            get;
            set;
        }

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

        //FI
        //[DataMember]
        //public Visibility appendedOperatorVisible
        //{
        //    get { return _appendedOperatorVisible; }
        //    set
        //    {
        //        _appendedOperatorVisible = value;
        //        //OnPropertyChanged("appendedOperatorVisible");
        //    }
        //}

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
    }
}
