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

﻿using System;
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
                    _NewFields = new ObservableCollection<NewField>() { new NewField("Note Field Name") };

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
