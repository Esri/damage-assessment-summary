using ConfigureSummaryReport.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ConfigureSummaryReport.Model;

namespace ConfigureSummaryReport.Controls
{
    /// <summary>
    /// enables serveral additional features for a standard TextBox
    /// </summary>
    public class ClickableTextBox : TextBox
    {
        public ClickableTextBox()
        {
            this.PreviewMouseLeftButtonDown += previewMouseLeftButtonDown;
            this.GotKeyboardFocus += gotFocus;
            this.MouseDoubleClick += gotFocus;
            this.TextChanged += textChanged;
        }

        void gotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = e.OriginalSource as TextBox;
            if (tb != null) tb.SelectAll();
        }

        void previewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        //TODO may have to re-think this as I am now using this in more than one spot
        private static void textChanged(object sender, TextChangedEventArgs e)
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
                    if (child is ClickableTextBox)
                    {
                        ((ClickableTextBox)child).FontStyle = FontStyles.Normal;
                        DependencyObject p = VisualTreeHelper.GetParent(child);
                        if(p is StackPanel)
                            foreach (var c in ((StackPanel)p).Children)
                            {
                                if (c is CheckBox)
                                    ((CheckBox)c).IsEnabled = true;
                            }     
                    }

                    NewField nf = new NewField("Note Field Name");
                    ObservableCollection<NewField> source = lv.ItemsSource as ObservableCollection<NewField>;
                    source.Add(nf);
                }
            }
        }
    }
}
