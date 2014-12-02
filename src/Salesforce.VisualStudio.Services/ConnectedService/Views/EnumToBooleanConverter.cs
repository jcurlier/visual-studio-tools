using System;
using System.Globalization;
using System.Windows.Data;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    internal class EnumToBooleanConverter : IValueConverter
    {
        public EnumToBooleanConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return value.Equals(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            return value.Equals(true) ? parameter : Binding.DoNothing;
        }
    }
}
