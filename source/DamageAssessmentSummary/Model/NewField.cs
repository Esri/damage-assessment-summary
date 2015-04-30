using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary.Model
{
    [DataContract]
    public class NewField
    {
        public NewField() { }
        public NewField(string name) { Name = name; }

        [DataMember]
        public string Name { get; set; }
    }
}
