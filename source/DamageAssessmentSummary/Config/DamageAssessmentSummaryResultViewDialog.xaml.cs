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

namespace DamageAssessmentSummary.Config
{
    /// <summary>
    /// Interaction logic for DamageAssessmentSummaryResultViewDialog.xaml
    /// </summary>
    public partial class DamageAssessmentSummaryResultViewDialog : Window
    {
        public DataSource DataSource { get; private set; }

        public IDictionary<string,string> AdditionalFieldNames { get; private set; }

        //TODO...update this as values are modified for the user controled txt box
        //private IDictionary<string, IList<string>> FieldNameAliasMap { get; set; }
        private ObservableCollection<StringItems2> FieldNameAliasMap { get; set; }

        public string Caption { get; private set; }
        public MapWidget mapWidget { get; private set; }

        private ObservableCollection<StringItems2> expressions;

        private enum fieldTypes { ParcelID, Address, IncidentName, DescriptionOfDamage, CurrentAssessedValue };

        public DamageAssessmentSummaryResultViewDialog(IList<DataSource> dataSources, string initialCaption, string initialDataSourceId, string mapWidgetId, IEnumerable s)
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

            if (s != null)
            {
                rePopulateFieldList(s);
            }
            else
            {
                populateNewFieldList();
            }
        }

        private void rePopulateFieldList(IEnumerable s)
        {
            chkBoxListView.ItemsSource = s;
        }

        /// <summary>
        /// Create a list of all of the features fields
        /// </summary>
        private void populateFieldList(DataSource ds)
        {
            ////FieldNameAliasMap = new Dictionary<string, IList<string>>();

            //foreach (var field in ds.Fields)
            //    FieldNameAliasMap.Add(field.Name, new List<string>() { field.Alias, field.Alias });

            //chkBoxListView.ItemsSource = FieldNameAliasMap;

            FieldNameAliasMap = new ObservableCollection<StringItems2>();

            foreach (var field in ds.Fields)
            {
                StringItems2 si2 = new StringItems2(field.Name, new List<string>() { field.Alias, field.Alias });
                FieldNameAliasMap.Add(si2);
            }

            chkBoxListView.ItemsSource = FieldNameAliasMap;

            //IList<StringItems2> fieldNames = FieldNameAliasMap
            //    .Cast<StringItems2>().Where(item => item.isChecked == true).ToList();
            var fieldNames = from p in FieldNameAliasMap select p.key;

            cboFieldNames.ItemsSource = fieldNames;

            Operators operators = new Operators();

            cboOperators.ItemsSource = operators.operatorStrings;
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

            IList<StringItems2> displayItems = chkBoxListView.Items
                    .Cast<StringItems2>().Where(item => item.isChecked == true).ToList();

            AdditionalFieldNames = ConvertToDictionary(displayItems);

            foreach (NewField item in lvNewFields.Items)
            {
                if (item.Name != "New Field Name")
                    AdditionalFieldNames.Add(item.Name, item.Name);
            }

            DialogResult = true;
        }

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
        /// Update the UI when the selected datasource changes
        /// </summary>
        private void DataSourceSelector_SelectionChanged(object sender, EventArgs e)
        {
            DataSource dataSource = DataSourceSelector.SelectedDataSource;

            //IList<ESRI.ArcGIS.Client.Field> fields = dataSource.Fields;

            //list all fields from the datasource
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

        private void chkBoxListView_Unchecked(object sender, RoutedEventArgs e)
        {
            chkBoxSelectAll.IsChecked = false;
        }

        private void removeField_Checked(object sender, RoutedEventArgs e)
        {
            lvNewFields.Items.RemoveAt(lvNewFields.SelectedIndex);
        }

        //private void modifyField_Checked(object sender, RoutedEventArgs e)
        //{
        //    TextBox tb = (TextBox)((StackPanel)((CheckBox)sender).Parent).Children[0];
        //    tb.IsEnabled = true;
        //}

        //private void chkBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    //enable the ability to modify
        //    object o = ((CheckBox)sender).Parent;

        //    //need to handle cleaner...hack for now because it's quick
        //    CheckBox cb = (CheckBox)((StackPanel)((StackPanel)o).Children[1]).Children[1];
        //    cb.IsEnabled = true;
        //}

        //private void chkBox_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    //enable the ability to modify
        //    object o = ((CheckBox)sender).Parent;

        //    //need to handle cleaner...hack for now because it's quick
        //    CheckBox cb = (CheckBox)((StackPanel)((StackPanel)o).Children[1]).Children[1];
        //    cb.IsEnabled = false;
        //}

        //private void modifyField_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    TextBox tb = (TextBox)((StackPanel)((CheckBox)sender).Parent).Children[0];
        //    tb.IsEnabled = false;
        //}

        private void fieldName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string org = e.OriginalSource.ToString();
          
            //string n = ((CheckBox)((StackPanel)((StackPanel)((TextBox)sender).Parent).Parent).Children[0]).Content.ToString();

            //updateAdditionalFields(n, ((ClickSelectTextBox)sender).Text);
            
            //System.Diagnostics.Debug.WriteLine(n);
        }

        private void updateAdditionalFields(string n, string s)
        {
            foreach (StringItems2 item in FieldNameAliasMap)
            {
                if (item.value[0] == n)
                {
                    item.value[1] = s;
                    break;
                }
            }
        }

        private void chkBox_Checked(object sender, RoutedEventArgs e)
        {
         
        }

        private void chkBox_Checked_1(object sender, RoutedEventArgs e)
        {
            object o = ((CheckBox)sender).Parent;
        }

        private void chkBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            foreach (StringItems2 item in chkBoxListView.Items)
                item.isChecked = (cb.IsChecked.HasValue && cb.IsChecked.Value);
        }

        private void chkBoxSelectAllFieldNames_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            foreach (StringItems2 item in chkBoxListView.Items)
                item.displayNameValue = (cb.IsChecked.HasValue && cb.IsChecked.Value);
        }

        private void chkBoxSelectAllAliasNames_Click(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            foreach (StringItems2 item in chkBoxListView.Items)
                item.displayAliasValue = (cb.IsChecked.HasValue && cb.IsChecked.Value);
        }

        private void btnAddExpression_Click(object sender, RoutedEventArgs e)
        {
            string fieldName = cboFieldNames.SelectedValue as string;
            string op = cboOperators.SelectedValue as string;
            string value = txtSimpleExpression.Text;

            Expression exp = new Expression(fieldName, op, value);

            if(expressions == null)  
            {
                expressions = new ObservableCollection<StringItems2>();
                lvExpressions.ItemsSource = expressions; 
            }

            //TODO this should add expression or something
            expressions.Add(new StringItems2(exp.expression, exp._appendedOperators.ToList()));

           

            if (lvExpressions.Visibility == System.Windows.Visibility.Hidden)
                lvExpressions.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem i = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            expressions.Remove((StringItems2)i.Content);

            if (expressions.Count == 0) 
                lvExpressions.Visibility = System.Windows.Visibility.Hidden;

        }

        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    System.Diagnostics.Debug.WriteLine(current.ToString());
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void btnAddAdvancedExpression_Click(object sender, RoutedEventArgs e)
        {
            Expression exp = new Expression(txtAdvancedExpression.Text);

            if (expressions == null)
            {
                expressions = new ObservableCollection<StringItems2>();
                lvExpressions.ItemsSource = expressions;
            }

            //TODO this should add expression or something 
            expressions.Add(new StringItems2(exp.advancedExpression, exp._appendedOperators.ToList()));

            lvExpressions.Visibility = System.Windows.Visibility.Visible;
        }

        private void validateExpression_Click(object sender, RoutedEventArgs e)
        {
            foreach (ListViewItem item in lvExpressions.ItemsSource)
            {
                
            }

            ////verify that results are returned
            //string whereClause = string.Join(" ", expressions.ToArray());
            //string lastThree = whereClause.Substring(whereClause.Length -3, whereClause.Length);
            //string lastFour = whereClause.Substring(whereClause.Length -4, whereClause.Length);
            
            //if(lastThree == " OR") 
            //    whereClause = whereClause.Substring(0, whereClause.Length -3);
            //if(lastFour == " AND")
            //    whereClause = whereClause.Substring(0, whereClause.Length -4);

            //Query q = new Query(whereClause);

            //validateExpression(q);

        }

        private async void validateExpression(Query q)
        {
            try
            {
                var qr = await DataSource.ExecuteQueryAsync(q);

                if (qr.Error != null)
                {
                    MessageBox.Show(qr.Error.Message);
                    return;
                }

                if (qr == null || qr.Features == null)
                {
                    return;
                }
                else
                {
                    MessageBox.Show(String.Format("Expression Valid...{0} features returned", qr.Features.Count));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
