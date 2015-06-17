using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ConfigureSummaryReport.Model
{
    /// <summary>
    /// class used to store the values from the features that are passed to the list
    /// </summary>
    [DataContract]
    public class SiteDetails
    {
        [DataMember]
        public string LabelField { get; set; }

        //[DataMember]
        //public ESRI.ArcGIS.Client.Geometry.Geometry ZoomExtent { get; set; }
        [DataMember]
        public string ZoomExtent { get; set; }

        [DataMember]
        public IList<StringItems> AdditionalFieldsAndValues { get; set; }

        [DataMember]
        public string f { get; set; }

        [DataMember]
        public bool isExpanded { get; set; }

        [DataMember]
        public bool isParentExpandable { get; set; }
    }
}
