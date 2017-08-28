using HtmlRenderer.DomParseTester.DomComparing.ViewModels;
using HtmlRenderer.TestLib;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace HtmlRenderer.DomParseTester.DomComparing
{
    public class HierarchyCompareResultTextDecorationConverter : IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is CompareResult))
                return null;
            object[] decorations = parameter as object[];
            if ((decorations == null) || (decorations.Length < 2))
                return null;
            CompareResult result = (CompareResult)value;
            if (result == CompareResult.InvalidChild)
                return decorations[0];
            if (result != CompareResult.Equal)
                return decorations[1];
            return null;
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if ((values == null) || (values.Length != 2))
                return null;

            if (!(values[0] is HierarchyResult))
                return null;
            if (!(values[1] is CompareResult))
                return null;

            object[] decorations = parameter as object[];
            if ((decorations == null) || (decorations.Length < 2))
                return null;

            HierarchyResult hr = (HierarchyResult)values[0];
            if (hr == HierarchyResult.InvalidChild)
                return decorations[0];
            if (hr != HierarchyResult.Valid)
                return decorations[1];

            CompareResult cr = (CompareResult)values[1];
            if (cr == CompareResult.InvalidChild)
                return decorations[0];
            if (cr != CompareResult.Equal)
                return decorations[1];
            return null;
        }
        
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
