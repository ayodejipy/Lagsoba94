using System.Linq;

namespace Lagsoba94.Helpers
{
    public static class HtmlStringHelper
    {
        public static int GetWordCount(string input)
        {
            // strip off all html tags and attributes
            char[] array = new char[input.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < input.Length; i++)
            {
                char let = input[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }
                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }

            var noHtml = new string(array, 0, arrayIndex);

            // strip off spaces and sort into array
            string[] split_text = noHtml.Split(' ');

            // count array
           return split_text.Count();
        }
    }
}