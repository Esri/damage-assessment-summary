using DamageAssessmentSummary.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DamageAssessmentSummary
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
                    NewField nf = new NewField("New Field Name");
                    lv.Items.Add(nf);
                }
            }
        }
    }
}
