using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using MahApps.Metro.IconPacks;

namespace SasJobManager.UI.Converter
{
    public class ToggleIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (bool)value == true ? PackIconFontAwesomeKind.SquareRegular:
                PackIconFontAwesomeKind.CheckSquareRegular;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
