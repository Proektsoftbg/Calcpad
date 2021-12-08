using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Calcpad.web.Helpers
{
    public static class EnumListMapper<T>
    {
        public static IEnumerable<SelectListItem> Map(T selected)
        {
            string[] names = Enum.GetNames(typeof(T));
            var values = Enum.GetValues(typeof(T));
            SelectListItem[] items = new SelectListItem[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                string name = TextHelper.SplitWords(names[i], ' ');
                T value = (T)values.GetValue(i);
                items[i] = new SelectListItem(name, value.ToString(), value.Equals(selected));
            }
            return items;
        }
    }
}
