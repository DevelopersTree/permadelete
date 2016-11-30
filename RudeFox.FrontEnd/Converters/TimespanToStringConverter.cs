using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RudeFox.Converters
{
    class TimespanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is TimeSpan))
                throw new ArgumentException("Value must be a Timespan.");

            TimeSpan time = (TimeSpan)value;

            if (time > TimeSpan.FromHours(1))
                return $"{time.Hours} hours and {time.Minutes} minutes";
            else if (time > TimeSpan.FromMinutes(30))
                return $"{time.Minutes} minutes";
            else if (time > TimeSpan.FromMinutes(1))
                return $"{time.Minutes} minutes and {time.Seconds.RoundOff()} seconds";
            else if (time > TimeSpan.FromSeconds(5))
                return $"{time.Seconds.RoundOff()} seconds";
            else
                return "Less than 5 seconds";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
