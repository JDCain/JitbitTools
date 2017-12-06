using System.Globalization;
using System.Text.RegularExpressions;

namespace JitBit.Console
{
    public static class Helpers
    {
        public static int MatchCollectonToSeconds(MatchCollection test)
        {
            var time = 0;
            foreach (var item in test)
            {
                if (item.ToString().Contains("minute"))
                {
                    time += 60 * PareseInt(item.ToString());
                }
                if (int.TryParse(item.ToString(), out var blah))
                {
                    time += PareseInt(item.ToString());
                }
            }
            return time;
        }

        public static int PareseInt(string item)
        {
            return int.Parse(Regex.Match(item.ToString(), @"\d+").Value, NumberFormatInfo.InvariantInfo);
        }
    }
}
