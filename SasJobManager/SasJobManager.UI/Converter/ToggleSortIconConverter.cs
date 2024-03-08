using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MahApps.Metro.IconPacks;
using System.Globalization;

namespace SasJobManager.UI.Converter
{
    public class ToggleSortIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? PackIconFontAwesomeKind.SortAlphaUpAltSolid :
                PackIconFontAwesomeKind.SortAlphaDownSolid;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}