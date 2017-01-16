using System;
using System.Text.RegularExpressions;

namespace DAL.Utilities
{
    public class Utilities
    {
        public static string PasCaseConversion(string PascalWord)
        {
            return Regex.Replace(PascalWord, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
            //System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("([A-Z]+[a-z]+)");
            //string result = _regex.Replace(PascalWord, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");
            //return result;
        }

        public static string PasCaseConversion(object PascalWord)
        {
            if (PascalWord != null && !string.IsNullOrEmpty(Convert.ToString(PascalWord)))
                return PasCaseConversion(Convert.ToString(PascalWord));
            else
                return "";
        }
    }
}
