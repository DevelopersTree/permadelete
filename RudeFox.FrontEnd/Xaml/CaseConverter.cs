using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RudeFox.Xaml
{
    public enum TextCase
    {
        UpperCase,
        LowerCase,
        TitleCase
    }
    public class CaseConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() != typeof(string))
                throw new ArgumentException("Value should be a string.");
            if (parameter.GetType() != typeof(TextCase))
                throw new ArgumentException("Parameter must be of type TextCase.");

            var targetCase = (TextCase)parameter;
            switch (targetCase)
            {
                case TextCase.UpperCase:
                    return (value as string).ToUpper();
                case TextCase.LowerCase:
                    return (value as string).ToLower();
                case TextCase.TitleCase:
                    {
                        var cultureInfo = Thread.CurrentThread.CurrentCulture;
                        return cultureInfo.TextInfo.ToTitleCase((string)value);
                    }
                default:
                    return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
