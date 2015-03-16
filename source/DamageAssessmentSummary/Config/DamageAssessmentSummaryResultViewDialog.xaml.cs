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

namespace DamageAssessmentSummary.Config
{
    /// <summary>
    /// Interaction logic for DamageAssessmentSummaryResultViewDialog.xaml
    /// </summary>
    public partial class DamageAssessmentSummaryResultViewDialog : Window
    {

        public DataSource DataSource { get; private set; }

        public client.Field ParcelIDField { get; private set; }
        public client.Field AddressField { get; private set; }
        public client.Field IncidentNameField { get; private set; }
        public client.Field DescriptionOfDamageField { get; private set; }
        public client.Field CurrentAssessedValueField { get; private set; }

        public IList<string> AdditionalFieldNames { get; private set; }

        public string Caption { get; private set; }
        public MapWidget mapWidget { get; private set; }

        private enum fieldTypes { ParcelID, Address, IncidentName, DescriptionOfDamage, CurrentAssessedValue };

        public DamageAssessmentSummaryResultViewDialog(IList<DataSource> dataSources, string initialCaption, string initialDataSourceId, string mapWidgetId)
        {
            InitializeComponent();

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

            populateNewFieldList();
        }

        /// <summary>
        /// Create a list of all of the features fields
        /// </summary>
        private void populateFieldList(DataSource ds)
        {
            IList<string> fieldNames = new List<string>();
            foreach (var field in ds.Fields)
            {
                fieldNames.Add(field.Name);
            }

            chkBoxListView.ItemsSource = fieldNames;
        }

        /// <summary>
        /// Create a list of any note fields
        /// </summary>
        private void populateNewFieldList()
        {
            NewField nf = new NewField("New Field Name");
            lvNewFields.Items.Add(nf);
        }

        /// <summary>
        /// Pass the definitions to the ResultView
        /// </summary>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DataSource = DataSourceSelector.SelectedDataSource;
            Caption = CaptionTextBox.Text;

            ParcelIDField = (client.Field)ParcelIDFieldComboBox.SelectedItem;
            AddressField = (client.Field)AddressFieldComboBox.SelectedItem;
            IncidentNameField = (client.Field)IncidentNameFieldComboBox.SelectedItem;
            DescriptionOfDamageField = (client.Field)DescriptionOfDamageFieldComboBox.SelectedItem;
            CurrentAssessedValueField = (client.Field)CurrentAssessedValueFieldComboBox.SelectedItem;

            AdditionalFieldNames = ConvertToListOf<string>(chkBoxListView.SelectedItems);

            foreach (NewField item in lvNewFields.Items)
            {
                if (item.Name != "New Field Name")
                    AdditionalFieldNames.Add(item.Name);
            }

            DialogResult = true;
        }

        /// <summary>
        /// List Type conversion utility
        /// </summary>
        private IList<T> ConvertToListOf<T>(IList iList)
        {
            IList<T> result = new List<T>();

            foreach (T value in iList)
                result.Add(value);

            return result;
        }

        /// <summary>
        /// Update the UI when the selected datasource changes
        /// </summary>
        private void DataSourceSelector_SelectionChanged(object sender, EventArgs e)
        {
            DataSource dataSource = DataSourceSelector.SelectedDataSource;

            IList<ESRI.ArcGIS.Client.Field> fields = dataSource.Fields;

            //list all fields from the datasource
            ParcelIDFieldComboBox.ItemsSource = fields;
            //automatically select the field if it matches the pre-configured source field names
            ParcelIDFieldComboBox.SelectedItem = fields[getDefaultFieldIndex(fieldTypes.ParcelID, fields)];

            AddressFieldComboBox.ItemsSource = fields;
            AddressFieldComboBox.SelectedItem = fields[getDefaultFieldIndex(fieldTypes.Address, fields)];

            IncidentNameFieldComboBox.ItemsSource = fields;
            IncidentNameFieldComboBox.SelectedItem = fields[getDefaultFieldIndex(fieldTypes.IncidentName, fields)];

            DescriptionOfDamageFieldComboBox.ItemsSource = fields;
            DescriptionOfDamageFieldComboBox.SelectedItem = fields[getDefaultFieldIndex(fieldTypes.DescriptionOfDamage, fields)];

            CurrentAssessedValueFieldComboBox.ItemsSource = fields;
            CurrentAssessedValueFieldComboBox.SelectedItem = fields[getDefaultFieldIndex(fieldTypes.CurrentAssessedValue, fields)];

            populateFieldList(dataSource);
        
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
        /// this method attempts to find the default field for the type of info we are after
        /// </summary>
        private int getDefaultFieldIndex(fieldTypes fieldType, IList<client.Field> fields)
        {
            string defaultName = "";

            switch (fieldType)
            {
                case fieldTypes.ParcelID:
                    defaultName = "PARCELID";
                    break;
                case fieldTypes.Address:
                    defaultName = "FULLADDR";
                    break;
                case fieldTypes.IncidentName:
                    defaultName = "INCIDENTNM";
                    break;
                case fieldTypes.DescriptionOfDamage:
                    defaultName = "DESCDAMAGE";
                    break;
                case fieldTypes.CurrentAssessedValue:
                    defaultName = "PREDISVAL";
                    break;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                if (fields[i].Name == defaultName)
                {
                    return i;
                }
            }

            //return 0...(the first field) if default field not found in the fields collection
            return 0;
        }

        private void chkBoxListView_Unchecked(object sender, RoutedEventArgs e)
        {
            chkBoxSelectAll.IsChecked = false;
        }

        private void chkBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            if (chkBoxSelectAll.IsChecked.HasValue && chkBoxSelectAll.IsChecked.Value)
                chkBoxListView.SelectAll();
            else
                chkBoxListView.UnselectAll();
        }

        private void removeField_Checked(object sender, RoutedEventArgs e)
        {
            lvNewFields.Items.RemoveAt(lvNewFields.SelectedIndex);
        }
    }

    public class NewField
    {
        private string _name;

        public NewField(string name)
        {
            Name = name;
        }
        public string Name 
        {
            get { return _name;}
            set 
            { 
                _name = value;
            }
        }
    }

    /// <summary>
    /// this enables serveral features for a standard TextBox...used for New Fields
    /// </summary>
    public class ClickSelectTextBox : TextBox
    {
        public ClickSelectTextBox()
        {
            AddHandler(PreviewMouseLeftButtonDownEvent,
              new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            AddHandler(GotKeyboardFocusEvent,
              new RoutedEventHandler(SelectAllText), true);
            AddHandler(MouseDoubleClickEvent,
              new RoutedEventHandler(SelectAllText), true);
            AddHandler(TextChangedEvent,
              new TextChangedEventHandler(MyTextChanged), true);
        }

        private static void MyTextChanged(object sender, TextChangedEventArgs e)
        {
            DependencyObject child = e.OriginalSource as UIElement;
            DependencyObject parent = e.OriginalSource as UIElement;

            while (parent != null && !(parent is ListView))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var lv = (ListView)parent;

                if (lv.SelectedIndex == lv.Items.Count - 1)
                {
                    NewField nf = new NewField("New Field Name");
                    lv.Items.Add(nf);
                }
            }
        }


        private static void SelectivelyIgnoreMouseButton(object sender,
                                                         MouseButtonEventArgs e)
        {
            DependencyObject parent = e.OriginalSource as UIElement;
            while (parent != null && !(parent is TextBox))
                parent = VisualTreeHelper.GetParent(parent);

            if (parent != null)
            {
                var textBox = (TextBox)parent;
                if (!textBox.IsKeyboardFocusWithin)
                {
                    textBox.Focus();
                    e.Handled = true;
                }
            }
        }

        private static void SelectAllText(object sender, RoutedEventArgs e)
        {
            var textBox = e.OriginalSource as TextBox;
            if (textBox != null)
                textBox.SelectAll();
        }
    }

}
