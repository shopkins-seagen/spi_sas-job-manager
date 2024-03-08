using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace SasJobManager.UI.Converter
{
    public class CpuColorColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush outOfRangeBrush = new SolidColorBrush(Colors.DodgerBlue); 
            try
            {
                SolidColorBrush[] colors = {
                new SolidColorBrush(Colors.DodgerBlue),
                new SolidColorBrush(Colors.Blue),
                new SolidColorBrush(Colors.DarkOrange),
                new SolidColorBrush(Colors.OrangeRed),
                new SolidColorBrush(Colors.Red) };

                return colors[System.Convert.ToInt32(Math.Max(Math.Ceiling((double)(int)value / 20), 1)) - 1];
            }
            catch (System.IndexOutOfRangeException)
            {
               outOfRangeBrush =  new SolidColorBrush(Colors.Red);
            }
            return outOfRangeBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
