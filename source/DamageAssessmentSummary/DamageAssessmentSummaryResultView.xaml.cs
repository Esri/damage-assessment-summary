using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using ESRI.ArcGIS.OperationsDashboard;
using client = ESRI.ArcGIS.Client;
using System.Windows.Data;
using System.IO;
using System.Diagnostics;
using ESRI.ArcGIS.Client.Geometry;

namespace DamageAssessmentSummary
{
    /// <summary>
    /// A Widget is a dockable add-in class for Operations Dashboard for ArcGIS that implements IWidget. By returning true from CanConfigure, 
    /// this widget provides the ability for the user to configure the widget properties showing a settings Window in the Configure method.
    /// By implementing IDataSourceConsumer, this Widget indicates it requires a DataSource to function and will be notified when the 
    /// data source is updated or removed.
    /// </summary>
    [Export("ESRI.ArcGIS.OperationsDashboard.Widget")]
    [ExportMetadata("DisplayName", "Damage Assessment Summary")]
    [ExportMetadata("Description", "Select key fields from a feature service and export to CSV")]
    [ExportMetadata("ImagePath", "/DamageAssessmentSummary;component/Images/Widget32.png")]
    [ExportMetadata("DataSourceRequired", true)]
    [DataContract]
    public partial class DamageAssessmentSummaryResultView : UserControl, IWidget, IDataSourceConsumer, IMapWidgetConsumer
    {
        [DataMember(Name = "dataSourceId")]
        public string DataSourceId { get; set; }

        [DataMember(Name = "_dataSource")]
        private DataSource dataSource = null;

        [DataMember(Name = "mapWidget")]
        private MapWidget mapWidget = null;

        [DataMember(Name = "parcelIDFieldName")]
        private string ParcelIDFieldName { get; set; }

        [DataMember(Name = "addressFieldName")]
        private string AddressFieldName { get; set; }

        [DataMember(Name = "incidentNameFieldName")]
        private string IncidentNameFieldName { get; set; }

        [DataMember(Name = "descriptionOfDamageFieldName")]
        private string DescriptionOfDamageFieldName { get; set; }

        [DataMember(Name = "currentAssessedValueFieldName")]
        private string CurrentAssessedValueFieldName { get; set; }

        [DataMember(Name = "additionalFields")]
        private IList<string> AdditionalFields { get; set; }

        public DamageAssessmentSummaryResultView()
        {
            InitializeComponent();
        }

        #region IMapWidgetConsumer

        public string MapWidgetId { get; set; }

        #endregion

        #region IWidget Members

        private string _caption = "Damage Summary";
        /// <summary>
        /// The text that is displayed in the widget's containing window title bar. This property is set during widget configuration.
        /// </summary>
        [DataMember(Name = "caption")]
        public string Caption
        {
            get
            {
                return _caption;
            }

            set
            {
                if (value != _caption)
                {
                    _caption = value;
                }
            }
        }

        /// <summary>
        /// The unique identifier of the widget, set by the application when the widget is added to the configuration.
        /// </summary>
        [DataMember(Name = "id")]
        public string Id { get; set; }

        /// <summary>
        /// OnActivated is called when the widget is first added to the configuration, or when loading from a saved configuration, after all 
        /// widgets have been restored. Saved properties can be retrieved, including properties from other widgets.
        /// Note that some widgets may have properties which are set asynchronously and are not yet available.
        /// </summary>
        public void OnActivated()
        {
            //this will get the first map widget from the instance
            foreach (IWidget w in OperationsDashboard.Instance.Widgets)
            {
                if (w is MapWidget && mapWidget == null)
                    mapWidget = (MapWidget)w;
            }
        }

        /// <summary>
        ///  OnDeactivated is called before the widget is removed from the configuration.
        /// </summary>
        public void OnDeactivated()
        {

        }

        /// <summary>
        ///  Determines if the Configure method is called after the widget is created, before it is added to the configuration. Provides an opportunity to gather user-defined settings.
        /// </summary>
        /// <value>Return true if the Configure method should be called, otherwise return false.</value>
        public bool CanConfigure
        {
            get { return true; }
        }

        /// <summary>
        ///  Provides functionality for the widget to be configured by the end user through a dialog.
        /// </summary>
        /// <param name="owner">The application window which should be the owner of the dialog.</param>
        /// <param name="dataSources">The complete list of DataSources in the configuration.</param>
        /// <returns>True if the user clicks ok, otherwise false.</returns>
        public bool Configure(Window owner, IList<DataSource> dataSources)
        {
            // Show the configuration dialog.
            Config.DamageAssessmentSummaryResultViewDialog dialog = new Config.DamageAssessmentSummaryResultViewDialog(dataSources, Caption, DataSourceId, MapWidgetId) { Owner = owner };
            if (dialog.ShowDialog() != true)
                return false;

            // Retrieve the selected values for the properties from the configuration dialog.
            Caption = dialog.Caption;
            DataSourceId = dialog.DataSource.Id;
            ParcelIDFieldName = dialog.ParcelIDField.Name;
            AddressFieldName = dialog.AddressField.Name;
            IncidentNameFieldName = dialog.IncidentNameField.Name;
            DescriptionOfDamageFieldName = dialog.DescriptionOfDamageField.Name;
            CurrentAssessedValueFieldName = dialog.CurrentAssessedValueField.Name;

            mapWidget = dialog.mapWidget;

            AdditionalFields = new List<string>(){
                AddressFieldName,
                ParcelIDFieldName,
                IncidentNameFieldName,
                DescriptionOfDamageFieldName,
                CurrentAssessedValueFieldName
            };

            foreach (var item in dialog.AdditionalFieldNames)
                AdditionalFields.Add(item);

            //get the datasource based on ID
            dataSource = OperationsDashboard.Instance.DataSources.FirstOrDefault((_dataSource) => _dataSource.Id == DataSourceId);

            //Query the datasource and populate the list
            getData(dataSource);

            return true;
        }

        #endregion

        #region IDataSourceConsumer Members/Helpers

        /// <summary>
        /// Returns the ID(s) of the data source(s) consumed by the widget.
        /// </summary>
        public string[] DataSourceIds
        {
            get { return new string[] { DataSourceId }; }
        }

        /// <summary>
        /// Called when a DataSource is removed from the configuration. 
        /// </summary>
        /// <param name="dataSource">The DataSource being removed.</param>
        public void OnRemove(DataSource dataSource)
        {
            // Respond to data source being removed.
            DataSourceId = null;
        }

        /// <summary>
        /// Called when a DataSource found in the DataSourceIds property is updated.
        /// </summary>
        /// <param name="dataSource">The DataSource being updated.</param>
        public void OnRefresh(DataSource dataSource)
        {
            //If a new instance of this view is opened....pass the items from the original view to the new view
            // otherwise do a standard query to the data
            if (!syncViews())
                getData(dataSource);
        }

        /// <summary>
        /// Called on Refresh...this is to check if we need to perform another query on the data
        /// </summary>
        private bool syncViews()
        {
            //keep the inital view and the new view in sync
            bool synced = false;
            DamageAssessmentSummaryResultView loadedView = null;

            IList<DamageAssessmentSummaryResultView> newViews = new List<DamageAssessmentSummaryResultView>();

            foreach (var item in OperationsDashboard.Instance.Widgets)
            {
                if (item is DamageAssessmentSummaryResultView)
                {
                    if (((DamageAssessmentSummaryResultView)item).lvSiteDetails.Items.Count > 0)
                        loadedView = (DamageAssessmentSummaryResultView)item;

                    if (((DamageAssessmentSummaryResultView)item).lvSiteDetails.Items.Count == 0)
                        newViews.Add((DamageAssessmentSummaryResultView)item);
                }
            }

            //if we have a view that is populated and a new view sync the lists
            if (loadedView != null && newViews.Count() > 0)
            {
                foreach (var view in newViews)
                {
                    if (view.lvSiteDetails.Items.Count == 0)
                    {
                        view.lvSiteDetails.ItemsSource = loadedView.lvSiteDetails.ItemsSource;
                        synced = true;
                    }
                }
            }

            //if no new view but the current view has items (standard refersh calls) set synced flag to true to
            // to avoid another query of the data thus overwriting values the user has changed
            if (newViews.Count == 0 && loadedView.lvSiteDetails.Items.Count > 0)
                synced = true;

            return synced;
        }

        /// <summary>
        /// Query and return all items from the datasource 
        /// </summary>
        private async void getData(DataSource ds)
        {
            //Find the Geometry field name
            string geomFieldName = "";
            for (int i = 0; i < ds.Fields.Count; i++)
            {
                if (ds.Fields[i].Type == client.Field.FieldType.Geometry)
                {
                    geomFieldName = dataSource.Fields[i].FieldName;
                    break;
                }
            }

            //query/return all features
            var result = await ds.ExecuteQueryAsync(new Query("1=1", null, true));

            //Create a list to store the returned results
            List<SiteDetails> items = new List<SiteDetails>();

            if (result == null || result.Features == null)
                return;
            else
            {
                foreach (var item in result.Features)
                {
                    try
                    {
                        items.Add(new SiteDetails()
                        {
                            ZoomExtent = item.Geometry,
                            AdditionalFieldsAndValues = createNewFieldList(item),
                            LabelField = (item.Attributes[AdditionalFields[0]] != null) ? item.Attributes[AdditionalFields[0]].ToString() : ""
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

            //bind the items to the control
            lvSiteDetails.ItemsSource = items;
        }

        /// <summary>
        /// Create a list that contains the 
        /// </summary>
        private IList<StringItems> createNewFieldList(client.Graphic item)
        {
            IList<StringItems> fieldList = new List<StringItems>();

            foreach (var fieldName in AdditionalFields)
            {
                try
                {
                    if (item.Attributes[fieldName] != null)
                        fieldList.Add(new StringItems(fieldName + ":", item.Attributes[fieldName].ToString()));
                    else
                        fieldList.Add(new StringItems(fieldName + ":", ""));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            return fieldList;
        }
        #endregion

        #region Buttons
        /// <summary>
        /// Zoom or pan to the current feature
        /// </summary>
        /// 
        private void ZoomToFeature_Click(object sender, RoutedEventArgs e)
        {
            //Get the geometry from SiteDetails
            Geometry g = ((SiteDetails)((Button)sender).DataContext).ZoomExtent;

            //Get the map
            ESRI.ArcGIS.Client.Map map = mapWidget.Map;

            //If current resolution is close to min resolution pan to the feature, otherwise zoom to
            if (map.Resolution.ToString("#.000000") == map.MinimumResolution.ToString("#.000000"))
                map.PanTo(g);
            else
            {
                if (g is MapPoint)
                    map.ZoomToResolution(map.MinimumResolution, (MapPoint)g);
                else
                    map.ZoomTo(g);
            }
        }

        /// <summary>
        /// Generate the csv from the feature service
        /// </summary>
        private void generateSummary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //generate summary
                IList<SiteDetails> details = (IList<SiteDetails>)lvSiteDetails.ItemsSource;

                //build csv string
                var sb = new StringBuilder();

                string fs = createFormatString(AdditionalFields);

                IList<string> t = AdditionalFields.ToList();
                t.Add(Environment.NewLine);
                var headerLine = string.Format(fs, t.ToArray());
                sb.Append(headerLine);

                foreach (SiteDetails item in lvSiteDetails.ItemsSource)
                {
                    IList<string> values = createValueList(item.AdditionalFieldsAndValues);
                    var newLine = string.Format(fs, values.ToArray());
                    sb.Append(newLine);

                    System.Diagnostics.Debug.WriteLine(newLine);
                }
                t.Remove(Environment.NewLine);
                //replace any invalid characters with an empty space and trim any spaces
                string fileName = Path.GetInvalidFileNameChars().Aggregate(Caption, (current, c) => current.Replace(c.ToString(), string.Empty)).Trim();

                //alert the user of the name change
                if (fileName != Caption)
                    MessageBox.Show(String.Format("{0} contains invalid characters...saving csv as: {1}", Caption, fileName));

                //construct the output file path
                string filePath = String.Format("{0}\\{1}.csv", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);

                //write the contents to the .csv
                File.WriteAllText(filePath, sb.ToString());

                Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Construct format string to join with the features values 
        /// </summary>
        private string createFormatString(IList<string> d)
        {
            string s = "";

            for (int i = 0; i < d.Count; i++)
                s += ("\"{" + i + "}\",");

            s = s.TrimEnd(',') + "{" + d.Count + "}";
            return s;
        }

        /// <summary>
        /// Verify that values are retrieved from the features in the correct order
        /// </summary>
        private IList<string> createValueList(IList<StringItems> si)
        {
            IList<string> valList = new List<string>();
            for (int i = 0; i < AdditionalFields.Count; i++)
            {
                for (int ii = 0; ii < si.Count; ii++)
                {
                    if (AdditionalFields[i] == si[ii].key.TrimEnd(':'))
                    {
                        valList.Add(si[ii].value);
                        break;
                    }
                }
                if (valList.Count == i)
                    valList.Add("");
            }
            valList.Add(Environment.NewLine);
            return valList;
        }

        #endregion
    }

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
