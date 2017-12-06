using System.Globalization;
using System.Text.RegularExpressions;

namespace JitBit.Console
{
    public static class Helpers
    {
        public static int? MatchCollectonToSeconds(MatchCollection test)
        {
            int? time = null;
            foreach (var item in test)
            {
                if (item.ToString().Contains("minute"))
                {
                    if (time == null)
                    {
                        time = 0;
                    }
                    time += 60 * PareseInt(item.ToString());
                }
                if (int.TryParse(item.ToString(), out var blah))
                {
                    if (time == null)
                    {
                        time = 0;
                    }
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
