using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary.Model
{
    /// <summary>
    /// store the key value pair...using this instead of the out of the box KeyValuePair class 
    /// as this will allow for 2 way binding
    /// </summary>
    [DataContract]
    public class StringItems
    {
        public StringItems()
        {
 
        }

        public StringItems(string Key, string Value)
        {
            key = Key;
            value = Value;
        }

        [DataMember]
        public string key { get; set; }

        [DataMember]
        public string value { get; set; }
    }

}
