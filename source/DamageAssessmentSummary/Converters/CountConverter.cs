using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ConfigureSummaryReport
{
    public class CountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ItemCollection)
            {
                ItemCollection items = (ItemCollection)value;
                for (int i = 0; i < items.Count -1; i++)
                {
                    object o = items.GetItemAt(i);
                }
            }
            return System.Windows.Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            //throw new NotImplementedException();
            return null;
        }
    }

    public class testConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Dictionary<string, string> opStringMap = new Dictionary<string, string>(){
                {"Equal To", "="},
                {"Not Equal To", "<>"},
                {"Less Than", "<"},
                {"Greator Than", ">"}
            };
            return opStringMap[value.ToString()];
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
