using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamageAssessmentSummary
{
    /// <summary>
    /// class used to store the values from the features that are passed to the list
    /// </summary>
    public class SiteDetails
    {
        public string LabelField { get; set; }
        public ESRI.ArcGIS.Client.Geometry.Geometry ZoomExtent { get; set; }
        public IList<StringItems> AdditionalFieldsAndValues { get; set; }
    }
}
