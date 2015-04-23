using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary
{
    /// <summary>
    /// store the key value pair...using this instead of the out of the box KeyValuePair class 
    /// as this will allow for 2 way binding
    /// </summary>
    public class StringItems
    {
        public StringItems(string Key, string Value)
        {
            key = Key;
            value = Value;
        }

        public string key { get; set; }
        public string value { get; set; }
    }

}
