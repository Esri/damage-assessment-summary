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
    /// Interaction logic for FieldListControl.xaml
    /// </summary>
    public partial class FieldListControl : UserControl
    {
        ObservableCollection<StringItems2> _FieldNameAliasMap;

        public FieldListControl()
        {
            InitializeComponent();
        }

        enum selectType { isChecked, displayName, displayAlias }

        public DataSource dataSource { get; set; }

        public void InitializeFieldList()
        {
                chkBoxListView.ItemsSource = FieldNameAliasMap;
        }

        public ObservableCollection<StringItems2> FieldNameAliasMap
        {
            get 
            {
                return _FieldNameAliasMap;
            }
            set
            {
                chkBoxListView.ItemsSource = value;
                _FieldNameAliasMap = value;
            }
        }

        private void chkBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            selectAll(sender, selectType.isChecked);       
        }

        private void chkBoxSelectAllFieldNames_Click(object sender, RoutedEventArgs e)
        {
            selectAll(sender, selectType.displayName);     
        }

        private void chkBoxSelectAllAliasNames_Click(object sender, RoutedEventArgs e)
        {
            selectAll(sender, selectType.displayAlias); 
        }

        private void selectAll(object sender, selectType selType)
        {
            CheckBox cb = (CheckBox)sender;

            switch (selType)
            {
                case selectType.isChecked:
                    foreach (StringItems2 item in chkBoxListView.Items)
                        item.isChecked = (cb.IsChecked.HasValue && cb.IsChecked.Value);
                    break;
                case selectType.displayName:
                    foreach (StringItems2 item in chkBoxListView.Items)
                        item.displayNameValue = (cb.IsChecked.HasValue && cb.IsChecked.Value);
                    break;
                case selectType.displayAlias:
                    foreach (StringItems2 item in chkBoxListView.Items)
                        item.displayAliasValue = (cb.IsChecked.HasValue && cb.IsChecked.Value);
                    break;
                default:
                    break;
            }
        }

        public List<StringItems2> getDisplayItems()
        {
            List<StringItems2> displayItems = chkBoxListView.Items
                .Cast<StringItems2>().Where(item => item.isChecked == true).ToList();
            return displayItems;
        }
    }
}
