using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary
{
    [DataContract]
    public class StringItems2 : INotifyPropertyChanged
    {
        bool _isChecked;
        bool _displayNameValue;
        bool _displayAliasValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public StringItems2()
        {
 
        }

        public StringItems2(string Key, List<string> Value)
        {
            key = Key;
            value = Value;
            isChecked = false;
            displayNameValue = false;
            displayAliasValue = true;
        }

        [DataMember]
        public string key { get; set; }

        [DataMember]
        public List<string> value { get; set; }

        [DataMember]
        public string sourceFieldName { get; set; }

        [DataMember]
        public string displayFieldName { get; set; }

        //[DataMember]
        //public ESRI.ArcGIS.Client.Field.FieldType fieldType { get; set; }

        [DataMember]
        public bool isChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged("isChecked");
                if (_isChecked)
                {
                    if (!displayNameValue)
                    {
                        displayAliasValue = true;
                        OnPropertyChanged("displayAliasValue");
                    }
                    if (!displayAliasValue)
                    {
                        displayNameValue = true;
                        OnPropertyChanged("displayNameValue");
                    }
                }
            }
        }

        [DataMember]
        public bool displayNameValue
        {
            get { return _displayNameValue; }
            set
            {
                _displayNameValue = value;
                OnPropertyChanged("displayNameValue");
                if (displayAliasValue != !value)
                {
                    displayAliasValue = !value;
                }
            }
        }

        [DataMember]
        public bool displayAliasValue
        {
            get { return _displayAliasValue; }
            set
            {
                _displayAliasValue = value;
                OnPropertyChanged("displayAliasValue");
                if (displayNameValue != !value)
                {
                    displayNameValue = !value;
                }
            }
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
