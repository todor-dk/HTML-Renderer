using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HtmlRenderer.DomParseTester.Internal
{
    internal class TextConverter : IValueConverter
    {
        public static readonly TextConverter Current = new TextConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string txt = value as string;
            if (txt == null)
                return value;

            StringBuilder sb = new StringBuilder();
            foreach(char ch in txt)
            {
                if ((ch == '\n') || (ch == '\r') || (ch == '\f'))
                    sb.Append('\u2199');
                else if (ch == '\t')
                    sb.Append('\u221F');
                else if (ch < 32)
                    sb.Append('\u00B7');
                else
                    sb.Append(ch);
            }
            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
