using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DamageAssessmentSummary
{
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //int count = System.Convert.ToInt32(value);

            //if (count > 1)
            //{
            //    return System.Windows.Visibility.Visible;
            //}
            //return System.Windows.Visibility.Hidden;

            if (value is ItemCollection)
            {
                ItemCollection items = (ItemCollection)value;
                for (int i = 0; i < items.Count -1; i++)
                {
                    object o = items.GetItemAt(i);
                }
                //if (items.Count > 1)
                //{
                //    return System.Windows.Visibility.Visible;
                //}
            }
            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }
}
