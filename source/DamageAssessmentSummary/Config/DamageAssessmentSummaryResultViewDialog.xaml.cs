using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using ESRI.ArcGIS.OperationsDashboard;
using client = ESRI.ArcGIS.Client;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Collections;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Globalization;
using DamageAssessmentSummary.Model;

namespace DamageAssessmentSummary.Config
{
    /// <summary>
    /// Interaction logic for ConfigureSummaryResultViewDialog.xaml
    /// </summary>
    public partial class DamageAssessmentSummaryResultViewDialog : Window
    {
        public DataSource DataSource { get; private set; }
        public string activeWhereClause {get; private set;}
        public string Caption { get; private set; }
        public MapWidget mapWidget { get; private set; }
        public ObservableCollection<NewField> NoteFields;
        public ObservableCollection<Model.Expression> expressions;
        public ObservableCollection<StringItems2> FieldNameAliasMap { get; private set; }
        public IDictionary<string, string> AdditionalFieldNames { get; private set; }

        #region Dialog

        public DamageAssessmentSummaryResultViewDialog(IList<DataSource> dataSources, string initialCaption, string initialDataSourceId, string mapWidgetId, ObservableCollection<Model.Expression> Expressions, ObservableCollection<StringItems2> SelectFieldList, ObservableCollection<NewField> noteFields)
        {
            InitializeComponent();

            //these will rehydrate state of the UI
            InitializeExpressions(Expressions);
            InitializeFieldList(SelectFieldList);
            InitializeNoteFields(noteFields);

            // When re-configuring, initialize the widget config dialog from the existing settings.
            CaptionTextBox.Text = initialCaption;
            if (!string.IsNullOrEmpty(initialDataSourceId))
            {
                DataSource dataSource = OperationsDashboard.Instance.DataSources.FirstOrDefault(ds => ds.Id == initialDataSourceId);
                if (dataSource != null)
                    DataSourceSelector.SelectedDataSource = dataSource;
            }

            // Retrieve a list of all map widgets from the application and bind this to a combo box 
            // for the user to select a map from.
            IEnumerable<ESRI.ArcGIS.OperationsDashboard.IWidget> mapws = OperationsDashboard.Instance.Widgets.Where(w => w is MapWidget);

            // Disable the combo box if no map widgets found.
            if (mapws.Count() < 1)
            {
                //do something if no map is found
                //don't think we have to worry about this as the Widget requires a datasource 
            }
            else
            {
                // If an existing MapWidgetId is already set, select this in the list. If not set, then select the first in the list.
                MapWidget currentWidget = OperationsDashboard.Instance.Widgets.FirstOrDefault(widget => widget.Id == mapWidgetId) as MapWidget;
                if (currentWidget == null)
                    mapWidget = (MapWidget)mapws.First();
                else
                    mapWidget = currentWidget;
            }
        }
        /// <summary>
        /// Update the UI when the selected datasource changes
        /// </summary>
        private void DataSourceSelector_SelectionChanged(object sender, EventArgs e)
        {
            //list all fields from the datasource
            DataSource dataSource = DataSourceSelector.SelectedDataSource;

            if (FieldNameAliasMap == null)
            {
                FieldNameAliasMap = new ObservableCollection<StringItems2>();

                foreach (var field in dataSource.Fields)
                {
                    StringItems2 si2 = new StringItems2(field.Name, new List<string>() { field.Alias, field.Alias });
                    si2.FieldType = getFieldType(field.Type);
                    FieldNameAliasMap.Add(si2);
                }

                fieldListControl.FieldNameAliasMap = FieldNameAliasMap;
            }

            filterControl.FieldNameAliasMap = FieldNameAliasMap;
            filterControl.dataSource = dataSource;
        }

        private void ValidateInput(object sender, TextChangedEventArgs e)
        {
            if (OKButton == null)
                return;

            OKButton.IsEnabled = false;
            if (string.IsNullOrEmpty(CaptionTextBox.Text))
                return;

            OKButton.IsEnabled = true;
        }

        /// <summary>
        /// Pass the definitions to the ResultView
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DataSource = DataSourceSelector.SelectedDataSource;
            Caption = CaptionTextBox.Text;
            activeWhereClause = filterControl.ActiveWhereClause;
            NoteFields = noteFieldControl.Fields;
            expressions = filterControl.expressions;

            List<StringItems2> displayItems = fieldListControl.getDisplayItems();

            if (displayItems.Count > 0)
            {
                AdditionalFieldNames = ConvertToDictionary(displayItems);

                //Add any note fields that don't have the default name
                foreach (NewField item in NoteFields)
                {
                    if (item.Name != "New Note Field Name")
                        AdditionalFieldNames.Add(item.Name, item.Name);
                }

                DialogResult = true;
            }
            else
            {
                MessageBox.Show("No display fields selected.\nPlease select at least one field to display.");
            }
        }

        #endregion

        #region Init

        /// <summary>
        /// Create a list of any note fields or a new note fields collection
        /// </summary>
        private void InitializeNoteFields(ObservableCollection<NewField> noteFields)
        {
            noteFieldControl.InitializeNoteFields(noteFields);
        }

        private void InitializeFieldList(ObservableCollection<StringItems2> SelectFieldList)
        {
            FieldNameAliasMap = SelectFieldList;
            if (FieldNameAliasMap != null)
            {
                filterControl.FieldNameAliasMap = FieldNameAliasMap;
                fieldListControl.FieldNameAliasMap = FieldNameAliasMap;
            }
        }

        private void InitializeExpressions(ObservableCollection<Model.Expression> Expressions)
        {
            filterControl.InitializeExpressions(Expressions);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// List Type conversion utility
        /// </summary>
        private IDictionary<string, string> ConvertToDictionary(IList<StringItems2> iList)
        {
            IDictionary<string, string> result = new Dictionary<string,string>();

            foreach (StringItems2 value in iList)
            {
                foreach (StringItems2 item in FieldNameAliasMap)
                {
                    if (item.key == value.key)
                    {
                        if(item.displayAliasValue)
                            result.Add(item.key, item.value[1]);
                        if (item.displayNameValue)
                            result.Add(item.key, item.key);
                        break;
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Gets the local fieldType from a Client.Field.FieldType
        /// </summary>
        /// <param name="ft"></param>
        /// <returns>a serializable local field type value</returns>
        private Model.Expression.fieldType getFieldType(ESRI.ArcGIS.Client.Field.FieldType ft)
        {
            return (Model.Expression.fieldType)Enum.Parse(typeof(Model.Expression.fieldType), ft.ToString());
        }

        #endregion
    }
}
