using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Salesforce.VisualStudio.Services.ConnectedService.Views
{
    /// <summary>
    /// An IValueConverter that handles mapping enum values to Visibility values.
    /// </summary>
    internal class EnumToVisibilityConverter : IValueConverter
    {
        public EnumToVisibilityConverter()
        {
            this.NonVisibleVisibility = Visibility.Collapsed;
        }

        public Visibility NonVisibleVisibility { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            if (value.Equals(parameter))
            {
                return Visibility.Visible;
            }
            else
            {
                return this.NonVisibleVisibility;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
