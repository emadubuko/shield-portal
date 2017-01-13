namespace DAL.Utilities
{
    public class Utilities
    {
        public static string PasCaseConversion(string PascalWord)
        {
            System.Text.RegularExpressions.Regex _regex = new System.Text.RegularExpressions.Regex("([A-Z]+[a-z]+)");
            string result = _regex.Replace(PascalWord, m => (m.Value.Length > 3 ? m.Value : m.Value.ToLower()) + " ");
            return result;
        }
    }
}
