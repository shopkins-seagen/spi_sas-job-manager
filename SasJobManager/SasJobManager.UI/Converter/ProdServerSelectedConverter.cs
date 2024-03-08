using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SasJobManager.UI.Converter
{
    public class ProdServerSelectedConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values[0].ToString()=="production" ? FontWeights.Bold : FontWeights.Normal;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
