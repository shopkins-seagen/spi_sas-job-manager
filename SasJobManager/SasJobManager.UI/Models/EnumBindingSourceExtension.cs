using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace SasJobManager.UI.Models
{
    public class EnumBindingExtension : MarkupExtension
    {
        public Type EnumType { get; private set; }
        public EnumBindingExtension(Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new Exception($"{enumType} is not an enum");
            }
            EnumType = enumType;
        }
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(EnumType);
        }
    }
}
