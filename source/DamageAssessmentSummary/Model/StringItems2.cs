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
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ConfigureSummaryReport.Model;

namespace ConfigureSummaryReport.Model
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

        [DataMember]
        public Model.Expression.fieldType FieldType { get; set; }

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
