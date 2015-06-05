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

namespace ConfigureSummaryReport.Controls
{
    /// <summary>
    /// Interaction logic for NoteFieldControl.xaml
    /// </summary>
    public partial class NoteFieldControl : UserControl
    {
        private ObservableCollection<NewField> _NewFields;

        public NoteFieldControl()
        {
            InitializeComponent();
        }

        public ObservableCollection<NewField> Fields 
        {
            get
            {
                //this will get a null on init...will only have a value when 
                // resetting the state after a save and reopen
                //setting up a default for init
                if (_NewFields == null)
                    _NewFields = new ObservableCollection<NewField>() { new NewField("New Note Field Name") };

                return _NewFields;
            }
            set { _NewFields = value; } 
        }

        /// <summary>
        /// Create a list of any note fields or a new note fields collection
        /// </summary>
        public void InitializeNoteFields(ObservableCollection<NewField> noteFields)
        {
            this.Fields = noteFields;
            lvNewFields.ItemsSource = this.Fields;
        }

        private void removeField_Checked(object sender, RoutedEventArgs e)
        {
            ObservableCollection<NewField> fields = (ObservableCollection<NewField>)lvNewFields.ItemsSource;
            fields.RemoveAt(lvNewFields.SelectedIndex);
        }
    }
}
