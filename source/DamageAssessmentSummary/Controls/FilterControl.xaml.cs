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
using DamageAssessmentSummary.Model;

namespace DamageAssessmentSummary.Controls
{
    /// <summary>
    /// Interaction logic for FilterControl.xaml
    /// </summary>
    public partial class FilterControl : UserControl
    {
        private ObservableCollection<StringItems2> _FieldNameAliasMap;
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
            string fieldName = cboFieldNames.SelectedValue as string;
            string op = cboOperators.SelectedValue as string;
            string value = txtSimpleExpression.Text;

            Model.Expression exp = new Model.Expression(fieldName, op, value);

            foreach (StringItems2 item in FieldNameAliasMap)
            {
                if (item.value[0] == fieldName)
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

        private void validateExpression_Click(object sender, RoutedEventArgs e)
        {
            if (lvExpressions.Items.Count > 0)
            {
                setActiveWhereClause();

                Query q = new Query(ActiveWhereClause);

                validateExpression(q);
            }
            else
            {
                MessageBox.Show("No expression to validate.\nPlease add an expression.");
            }

        }

        private async void validateExpression(Query q)
        {
            try
            {
                var qr = await dataSource.ExecuteQueryAsync(q);

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
