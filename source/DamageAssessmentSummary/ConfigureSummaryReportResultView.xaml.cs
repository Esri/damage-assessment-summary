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
using System.Collections.ObjectModel;
using ConfigureSummaryReport.Model;

namespace ConfigureSummaryReport
{
    /// <summary>
    /// A Widget is a dockable add-in class for Operations Dashboard for ArcGIS that implements IWidget. By returning true from CanConfigure, 
    /// this widget provides the ability for the user to configure the widget properties showing a settings Window in the Configure method.
    /// By implementing IDataSourceConsumer, this Widget indicates it requires a DataSource to function and will be notified when the 
    /// data source is updated or removed.
    /// </summary>
    [Export("ESRI.ArcGIS.OperationsDashboard.Widget")]
    [ExportMetadata("DisplayName", "Summary Report Tool")]
    [ExportMetadata("Description", "Select key fields from a feature service and export to CSV")]
    [ExportMetadata("ImagePath", "/SummaryReport;component/Images/Widget32.png")]
    [ExportMetadata("DataSourceRequired", true)]
    [DataContract]
    public partial class ConfigureSummaryReportResultView : UserControl, IWidget, IDataSourceConsumer, IMapWidgetConsumer
    {
        [DataMember(Name = "dataSourceId")]
        public string DataSourceId { get; set; }

        [DataMember(Name = "_dataSource")]
        private ESRI.ArcGIS.OperationsDashboard.DataSource dataSource = null;

        [DataMember(Name = "mapWidget")]
        private MapWidget mapWidget = null;

        //{ Key=FieldName, Value=AliasOrUserName }
        [DataMember(Name = "additionalFields")]
        private Dictionary<string, string> AdditionalFields { get; set; }

        [DataMember(Name = "whereClause")]
        private string WhereClause { get; set; }

        [DataMember(Name = "expressions")]
        private ObservableCollection<Model.Expression> Expressions { get; set; }

        [DataMember(Name = "fieldNameAliasMap")]
        private ObservableCollection<StringItems2> FieldNameAliasMap { get; set; }

        [DataMember(Name = "newFields")]
        private ObservableCollection<NewField> NewFields { get; set; }

        [DataMember(Name = "useAliasName")]
        private bool UseAliasName { get; set; }

        [DataMember(Name = "allSelected")]
        private bool AllSelected { get; set; }

        [DataMember(Name = "useExpandable")]
        private bool UseExpandable { get; set; }

        public ConfigureSummaryReportResultView()
        {
            InitializeComponent();
        }

        #region IMapWidgetConsumer

        public string MapWidgetId { get; set; }

        #endregion

        #region IWidget Members

        private string _caption = "Summary Report";

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
            Config.ConfigureSummaryReportResultViewDialog dialog = new Config.ConfigureSummaryReportResultViewDialog(dataSources, Caption, DataSourceId, MapWidgetId, Expressions, FieldNameAliasMap, NewFields, UseAliasName, AllSelected, UseExpandable) { Owner = owner };
            if (dialog.ShowDialog() != true)
                return false;

            // Retrieve the selected values for the properties from the configuration dialog.
            Caption = dialog.Caption;
            DataSourceId = dialog.DataSource.Id;
            Expressions = dialog.expressions;
            WhereClause = dialog.activeWhereClause;
            FieldNameAliasMap = dialog.FieldNameAliasMap;
            NewFields = dialog.NoteFields;
            UseAliasName = dialog.UseAliasNameSelected;
            AllSelected = dialog.AllSelected;
            UseExpandable = dialog.fieldListControl.useExpandableList;

            mapWidget = dialog.mapWidget;

            AdditionalFields = new Dictionary<string,string>(){};

            foreach (var item in dialog.AdditionalFieldNames)
            {
                AdditionalFields.Add(item.Key,item.Value);
            }

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
        public void OnRefresh(DataSource ds)
        {
            dataSource = ds;
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
            ConfigureSummaryReportResultView loadedView = null;

            IList<ConfigureSummaryReportResultView> newViews = new List<ConfigureSummaryReportResultView>();

            foreach (var item in OperationsDashboard.Instance.Widgets)
            {
                if (item is ConfigureSummaryReportResultView)
                {
                    if (((ConfigureSummaryReportResultView)item).lvSiteDetails.Items.Count > 0)
                        loadedView = (ConfigureSummaryReportResultView)item;

                    if (((ConfigureSummaryReportResultView)item).lvSiteDetails.Items.Count == 0)
                        newViews.Add((ConfigureSummaryReportResultView)item);
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

            //TODO changed this to support working with the selected layer...would delete note field itesm
            //if no new view but the current view has items (standard refersh calls) set synced flag to true to
            // to avoid another query of the data thus overwriting values the user has changed
            if (dataSource != null)
            {
                if (!dataSource.IsSelectable)
                    if (newViews.Count == 0 && loadedView.lvSiteDetails.Items.Count > 0)
                        synced = true;
            }
            else
            {
                if (newViews.Count == 0 && loadedView.lvSiteDetails.Items.Count > 0)
                    synced = true;
            }

                return synced;
        }

        /// <summary>
        /// Query and return all items from the datasource 
        /// </summary>
        private async void getData(DataSource ds)
        {
            //query/return all features
            var result = await ds.ExecuteQueryAsync(new Query(WhereClause, null, true));
           
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
                        //TODO thinking I should just handle the projections here
                        // that way it only happens once rather than on each zoom
                        SiteDetails newItem = new SiteDetails()
                        {
                            f = item.Attributes[ds.ObjectIdFieldName].ToString(),
                            ZoomExtent = item.Geometry.ToJson(),
                            AdditionalFieldsAndValues = createNewFieldList(item),
                            LabelField = (item.Attributes[AdditionalFields.Keys.ToList()[0]] != null) ? item.Attributes[AdditionalFields.Keys.ToList()[0]].ToString() : "",
                            isParentExpandable = UseExpandable
                        };

                        if(UseExpandable)
                            if(ds.IsSelectable)
                                newItem.isExpanded = isItemExpanded(newItem);

                        items.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                    }
                }
            }

            //bind the items to the control
            lvSiteDetails.ItemsSource = items;
        }

        private bool isItemExpanded(SiteDetails sd)
        {
            try
            {
                for (int i = 0; i < lvSiteDetails.Items.Count; i++)
                {
                    bool isMatch = true;

                    SiteDetails currentSD = (SiteDetails)lvSiteDetails.Items[i];
                    int cnt = 3;
                    if (currentSD.AdditionalFieldsAndValues.Count < 3)
                        cnt = currentSD.AdditionalFieldsAndValues.Count;
                    //only check the first couple of fields
                    for (int ii = 0; ii < cnt; ii++)
                    {
                        //TODO..could be that they just edited the value
                        if (currentSD.AdditionalFieldsAndValues[ii].value != sd.AdditionalFieldsAndValues[ii].value)
                            isMatch = false;
                    }

                    if (isMatch)
                        return currentSD.isExpanded;
                }
                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
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
                    if (item.Attributes[fieldName.Key] != null)
                        fieldList.Add(new StringItems(fieldName.Value + ":", item.Attributes[fieldName.Key].ToString()));
                    else
                        fieldList.Add(new StringItems(fieldName.Value + ":", ""));
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
            try
            {
                //Get the geometry from SiteDetails
                Geometry g = Geometry.FromJson(((SiteDetails)((Button)sender).DataContext).ZoomExtent);

                //Get the map
                ESRI.ArcGIS.Client.Map map = mapWidget.Map;

                if (g.SpatialReference.WKID != map.SpatialReference.WKID)
                {
                    //TODO...need to handle this
                }

                //If current resolution is close to min resolution pan to the feature, otherwise zoom to
                if (map.Resolution.ToString("#.000000") == map.MinimumResolution.ToString("#.000000"))
                    map.PanTo(g);
                else
                {
                    if (g is MapPoint)
                        map.ZoomToResolution(map.MinimumResolution, (MapPoint)g);
                    else
                    {
                        map.ZoomTo(g.Extent.Expand(1.23));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

                IList<string> t = AdditionalFields.Values.ToList();
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
        private string createFormatString(IDictionary<string,string> d)
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
                    if (AdditionalFields.Values.ToList()[i] == si[ii].key.TrimEnd(':'))
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

        private void HighlightFeature_Click(object sender, RoutedEventArgs e)
        {
            SiteDetails sd = ((Button)sender).DataContext as SiteDetails;

            Geometry g = Geometry.FromJson(sd.ZoomExtent);

            ESRI.ArcGIS.Client.FeatureLayer fl = mapWidget.FindFeatureLayer(dataSource);

            for (int i = 0; i < fl.Graphics.Count; i++)
            {
                client.Graphic feature = fl.Graphics[i];
                int featureOid;
                int.TryParse(feature.Attributes[dataSource.ObjectIdFieldName].ToString(), out featureOid);
                if (sd.f == featureOid.ToString())
                {
                    fl.ClearSelection();
                    feature.Select();
                }
            }
        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                    return (T)current;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }
    }

    public class QueueDisplayDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container)
        {
            SiteDetails siteItem = item as SiteDetails;
            var element = container as FrameworkElement;
            if (siteItem != null)
            {
                if(siteItem.isParentExpandable)
                    return element.FindResource("ExpanderTemplate") as DataTemplate;
                else
                    return element.FindResource("ExpanderTemplate2") as DataTemplate;
            }
            else
                return element.FindResource("ExpanderTemplate") as DataTemplate;
        }
    }

}
