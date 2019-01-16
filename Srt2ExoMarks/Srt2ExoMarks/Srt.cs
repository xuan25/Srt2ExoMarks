using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Srt2ExoMarks
{
    class Srt
    {
        public class SrtItem
        {
            public class Time
            {
                public int Hour, Minute, Second, Millisecond;

                public Time()
                {
                    Hour = 0;
                    Minute = 0;
                    Second = 0;
                    Millisecond = 0;
                }

                public Time(int hour, int minute, int second, int millisecond)
                {
                    if (hour < 0)
                    {
                        Hour = 0;
                        Minute = 0;
                        Second = 0;
                        Millisecond = 0;
                    }
                    else
                    {
                        Hour = hour;
                        Minute = minute;
                        Second = second;
                        Millisecond = millisecond;
                    }
                }

                public double ToSeconds()
                {
                    return (double)Millisecond / 1000 + Second + Minute * 60 + Hour * 3600;
                }

                public static Time ConvertFromString(string str)
                {
                    Match match = Regex.Match(str, "(?<Hour>-?[0-9]{2,}):(?<Minute>[0-9]{2}):(?<Second>[0-9]{2}),(?<Millisecond>[0-9]{0,3})");
                    if (match.Success)
                        return new Time(int.Parse(match.Groups["Hour"].Value), int.Parse(match.Groups["Minute"].Value), int.Parse(match.Groups["Second"].Value), (int)(double.Parse("0." + match.Groups["Millisecond"].Value) * 1000));
                    else
                        return null;
                }
            }

            public ulong Index;
            public Time StartTime;
            public Time EndTime;
            public string Content;

            public SrtItem(string srtItem)
            {
                Match match = Regex.Match(srtItem, "(?<Index>[0-9]+)\\r?\\n(?<StartTime>-?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3}) --> (?<EndTime>-?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3})\\r?\\n(?<Content>.*)");
                if (match.Success)
                {
                    Index = ulong.Parse(match.Groups["Index"].Value);
                    StartTime = Time.ConvertFromString(match.Groups["StartTime"].Value);
                    EndTime = Time.ConvertFromString(match.Groups["EndTime"].Value);
                    Content = match.Groups["Content"].Value;
                }
            }
        }

        public List<SrtItem> Items;

        public Srt(string srt)
        {
            Items = new List<SrtItem>();
            MatchCollection matchCollection = Regex.Matches(srt, "[0-9]+\\r?\\n-?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3} --> -?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3}\\r?\\n.*?\\r?\\n");
            foreach (Match match in matchCollection)
            {
                string content = match.Value;
                SrtItem srtItem = new SrtItem(content);
                Items.Add(srtItem);
            }
        }

        public Srt(Stream srt)
        {
            Items = new List<SrtItem>();
            MatchCollection matchCollection = Regex.Matches(new StreamReader(srt).ReadToEnd(), "[0-9]+\\r?\\n-?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3} --> -?[0-9]{2,}:[0-9]{2}:[0-9]{2},[0-9]{0,3}\\r?\\n.*?\\r?\\n");
            foreach (Match match in matchCollection)
            {
                string content = match.Value;
                SrtItem srtItem = new SrtItem(content);
                Items.Add(srtItem);
            }
            srt.Position = 0;
        }
    }
}
