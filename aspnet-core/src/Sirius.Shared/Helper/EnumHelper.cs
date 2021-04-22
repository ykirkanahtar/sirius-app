using System;
using System.Collections.Generic;
using System.Linq;
using Abp.Localization.Sources;

namespace Sirius.Shared.Helper
{
    public static class EnumHelper
    {
        public static List<KeyValuePair<string, string>> GetLocalizedEnumNames(Type enumType, ILocalizationSource localizationSource)
        {
            return Enum.GetNames(enumType).Select(item => new KeyValuePair<string, string>(item, localizationSource.GetString(item))).ToList();
        }
    }
}