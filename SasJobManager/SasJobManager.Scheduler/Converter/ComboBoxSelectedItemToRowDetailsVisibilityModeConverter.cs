using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;

namespace SasJobManager.Scheduler.Converter
{
	public class ComboBoxSelectedItemToRowDetailsVisibilityModeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value == null)
				return GridViewRowDetailsVisibilityMode.Collapsed;

			if (((EnumMemberViewModel)value).Name == "Collapsed")
			{
				return GridViewRowDetailsVisibilityMode.Collapsed;
			}
			else if (((EnumMemberViewModel)value).Name == "Visible")
			{
				return GridViewRowDetailsVisibilityMode.Visible;
			}
			else
			{
				return GridViewRowDetailsVisibilityMode.VisibleWhenSelected;
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return DependencyProperty.UnsetValue;
		}
	}
}
