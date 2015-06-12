using ESRI.ArcGIS.OperationsDashboard;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ConfigureSummaryReport.Model;
//using ConfigureSummaryReport.Config;

namespace ConfigureSummaryReport.Controls
{
    /// <summary>
    /// Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        private ObservableCollection<StringItems2> _FieldNameAliasMap;

        //private ObservableCollection<NewField> _NoteFields;

        private string _ActiveWhereClause;

        public FilterControl()
        {
            InitializeComponent();
        }

        public DataSource dataSource { get; set; }
        public ObservableCollection<StringItems2> FieldNameAliasMap 
        {
            get { return _FieldNameAliasMap; }
            set
            {
                cboFieldNames.ItemsSource = from p in value select p.value[0];
                _FieldNameAliasMap = value;
            } 
        }
        //public ObservableCollection<NewField> NoteFields
        //{
        //    get { return _NoteFields; }
        //    set { _NoteFields = value; }
        //}

        //public bool UseAliasNameSelected
        //{
        //    get;
        //    set;
        //}

        public string ActiveWhereClause 
        {
            get
            {
                if (_ActiveWhereClause == null)
                    setActiveWhereClause();
                return _ActiveWhereClause;
            }
            set 
            {
                _ActiveWhereClause = value;
            } 
        }
        public ObservableCollection<Model.Expression> expressions
        {
            get;
            set;
        }

        public void InitializeExpressions(ObservableCollection<Model.Expression> Expressions)
        {
            expressions = Expressions;

            if (expressions != null)
            {
                lvExpressions.ItemsSource = expressions;

                expressions.CollectionChanged -= expressions_CollectionChanged;
                expressions.CollectionChanged += expressions_CollectionChanged;

                if (expressions.Count > 0)
                {
                    if (lvExpressions.Visibility == System.Windows.Visibility.Hidden)
                        lvExpressions.Visibility = System.Windows.Visibility.Visible;
                }
            }

            Operators operators = new Operators();
            cboOperators.ItemsSource = operators.operatorStrings;
        }

        private void btnAddAdvancedExpression_Click(object sender, RoutedEventArgs e)
        {
            Model.Expression exp = new Model.Expression(txtAdvancedExpression.Text);

            if (expressions == null)
            {
                expressions = new ObservableCollection<Model.Expression>();
                lvExpressions.ItemsSource = expressions;
                expressions.CollectionChanged += expressions_CollectionChanged;
            }

            expressions.Add(exp);

            lvExpressions.Visibility = System.Windows.Visibility.Visible;
        }

        private void btnAddExpression_Click(object sender, RoutedEventArgs e)
        {
            //get the actual field name from the map here
            string fieldName = "";
            Model.Expression.fieldType ft = Model.Expression.fieldType.Unknown;

            if (cboFieldNames.SelectedValue.ToString() == _FieldNameAliasMap[cboFieldNames.SelectedIndex].value[0])
            {
                fieldName = _FieldNameAliasMap[cboFieldNames.SelectedIndex].key;
                ft = _FieldNameAliasMap[cboFieldNames.SelectedIndex].FieldType;
            }
            else
            {
                for (int i = 0; i < cboFieldNames.Items.Count; i++)
                {
                    if (_FieldNameAliasMap[i].value[0] == cboFieldNames.SelectedValue.ToString())
                    {
                        fieldName = _FieldNameAliasMap[i].key;
                        ft = _FieldNameAliasMap[i].FieldType;
                    }
                }
            }

            string op = cboOperators.SelectedValue as string;
            string value = txtSimpleExpression.Text;

            bool validValue = validateValue(ft, value);

            if(validValue)
            { 
                Model.Expression exp = new Model.Expression(fieldName, op, value);

                foreach (StringItems2 item in FieldNameAliasMap)
                {
                    if (item.key == fieldName)
                    {
                        exp.FieldType = item.FieldType;
                        break;
                    }
                }

                if (expressions == null)
                {
                    expressions = new ObservableCollection<Model.Expression>();
                    lvExpressions.ItemsSource = expressions;
                    //TODO...may want to clean this up between when the diag is opened and closed
                    expressions.CollectionChanged += expressions_CollectionChanged;
                }

                expressions.Add(exp);

                if (lvExpressions.Visibility == System.Windows.Visibility.Hidden)
                    lvExpressions.Visibility = System.Windows.Visibility.Visible;
            }     
        }

        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            ListViewItem i = FindAnchestor<ListViewItem>((DependencyObject)e.OriginalSource);
            expressions.Remove((Model.Expression)i.Content);

            if (expressions.Count == 0)
                lvExpressions.Visibility = System.Windows.Visibility.Hidden;
        }

        void expressions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (expressions.Count > 0)
            {
                //All items should show the appended operators except the last one
                for (int i = 0; i < expressions.Count - 1; i++)
                    expressions[i].isVisible = true;

                //the last one should not be visible
                expressions[expressions.Count - 1].isVisible = false;
            }
        }

        //returns true for any fields that query is supported on for this addin
        private bool validateValue(Model.Expression.fieldType ft, string value)
        {
            switch (ft)
            {
                case Model.Expression.fieldType.Blob:
                    MessageBox.Show("Query not supported on BLOB field type.");
                    return false;
                case Model.Expression.fieldType.Date:
                    DateTime dt;
                    if (DateTime.TryParse(value, out dt))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for Date field type.");
                        return false;
                    }
                case Model.Expression.fieldType.Double:
                    Double d;
                    if (Double.TryParse(value, out d))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for Double field type.");
                        return false;
                    }
                case Model.Expression.fieldType.GUID:
                    Guid g;
                    if (Guid.TryParse(value, out g))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for GUID field type.");
                        return false;
                    }
                case Model.Expression.fieldType.Geometry:
                    MessageBox.Show("Query not supported on GEOMETRY field type.");
                    return false;
                case Model.Expression.fieldType.GlobalID:
                    Guid guid;
                    if (Guid.TryParse(value, out guid))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for GlobalID field type.");
                        return false;
                    }
                case Model.Expression.fieldType.Integer:
                    Int32 i;
                    if (Int32.TryParse(value, out i))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for Integer field type.");
                        return false;
                    }
                case Model.Expression.fieldType.OID:
                    Int32 oid;
                    if (Int32.TryParse(value, out oid))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for OID field type.");
                        return false;
                    }
                case Model.Expression.fieldType.Raster:
                    MessageBox.Show("Query not supported on RASTER field type.");
                    return false;
                case Model.Expression.fieldType.Single:
                    Int16 ii;
                    if (Int16.TryParse(value, out ii))
                        return true;
                    else
                    {
                        MessageBox.Show("Cannot add Expression.\nUnable to parse value: " + value + " for Single Integer field type.");
                        return false;
                    }
                case Model.Expression.fieldType.SmallInteger:
                    Int16 iii;
                    if (Int16.TryParse(value, out iii))
                        return true;
                    else
                        return false;
                case Model.Expression.fieldType.String:
                    return true;
                case Model.Expression.fieldType.Unknown:
                    MessageBox.Show("Query not supported on UNKNOWN field type.");
                    return false;
                case Model.Expression.fieldType.XML:
                    MessageBox.Show("Query not supported on XML field type.");
                    return false;
                default:
                    return false;
            }
        }

        private void validateExpression_Click(object sender, RoutedEventArgs e)
        {
            if (lvExpressions.Items.Count > 0)
            {
                setActiveWhereClause();

                Query q = new Query(ActiveWhereClause);

                validateExpression(q);

                OperationsDashboard.Instance.RefreshDataSource(dataSource);
            }
            else
            {
                MessageBox.Show("No expression to validate.\nPlease add an expression.");
            }

        }
        private bool hasErrors = false;
        public void validateOnOk()
        {
            if (lvExpressions.Items.Count > 0)
            {
                //validate each expression indivually so we could know 
                // the bad parts to remove
                foreach (Model.Expression item in lvExpressions.ItemsSource)
                {
                    Query q = new Query(item.expression);
                    finalValidation(q, item);
                }

                setActiveWhereClause();


                if (hasErrors)
                {
                    MessageBox.Show("Removed invalid expression(s).");
                    hasErrors = false;
                }
            }
        }

        private async void finalValidation(Query q, Model.Expression e)
        {
            try
            {
                var qr = await dataSource.ExecuteQueryAsync(q);
                if (qr.Error != null)
                {
                    hasErrors = true;
                    expressions.Remove(e);

                    if (expressions.Count == 0)
                        lvExpressions.Visibility = System.Windows.Visibility.Hidden;
                }

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void validateExpression(Query q)
        {
            try
            {
                var qr = await dataSource.ExecuteQueryAsync(q);

                if (qr.Error != null)
                {
                    OperationsDashboard.Instance.RefreshDataSource(dataSource);
                    MessageBox.Show(qr.Error.Message);
                    validateOnOk();
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


        private static T FindAnchestor<T>(DependencyObject current) where T : DependencyObject
        {
            do
            {
                if (current is T)
                {
                    //System.Diagnostics.Debug.WriteLine(current.ToString());
                    return (T)current;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            while (current != null);
            return null;
        }

        private void setActiveWhereClause()
        {
            string whereClause = "";

            string finalAppendedOperator = "";

            if (lvExpressions.Items.Count > 0)
            {
                foreach (Model.Expression item in lvExpressions.ItemsSource)
                {
                    string expression = "";

                    if (item.advancedExpression != null)
                        expression = item.advancedExpression;
                    if (item.expression != null)
                        expression = item.expression;

                    whereClause += String.Format("{0} {1} ", item.expression, item.appendedOperator);
                    finalAppendedOperator = item.appendedOperator;
                }

                ActiveWhereClause = whereClause.Substring(0, whereClause.Length - (finalAppendedOperator.Length + 2));
            }
            else
                ActiveWhereClause = "1=1";
        }
    }
}
