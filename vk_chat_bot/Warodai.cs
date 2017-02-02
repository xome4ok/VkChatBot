using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace WarodaiWrapper
{
    /// <summary>
    /// Wrapper for Warodai dictionary in EDICT2 format
    /// </summary>
    public class Warodai
    {
        private List<WarodaiEntry> entries;

        public Warodai(string filepath)
        {
            entries = new List<WarodaiEntry>();

            var lines = System.IO.File.ReadAllLines(filepath);

            foreach (var line in lines)
            {
                // format is 
                // KANJI1;KANJI2 [READING1;READING2] /SENSE1 /SENSE2 /SENSE3
                var splitted = line.Split(new char[] { '[', ']' });

                var kanjisString = "";
                var readingsString = "";
                var sensesString = "";

                if (splitted.Length > 1)
                {
                    // so it have reading in squared brackets
                    kanjisString = splitted[0];
                    readingsString = splitted[1];
                    sensesString = splitted[2];
                }
                else if (splitted.Length == 1)
                {
                    //then it doesn't have reading!
                    var tmp = splitted[0].Split(new char[] { '/' });
                    kanjisString = tmp[0];
                    sensesString = string.Join("/", tmp.Skip(1));
                }
                else
                {
                    throw new FormatException("Parts of splitted dict entry doesn't match any expectations.");
                }

                //remove curly brackets surrounding ~smth patterns in sense descriptions
                sensesString = sensesString.Replace("{", "").Replace("}", "");

                var kanjis = kanjisString.Trim().Split(new char[] { ';' }).ToList();
                var readings = readingsString.Trim().Split(new char[] { ';' }).ToList();
                var senses = sensesString.Trim().Split(new char[] { '/' }).ToList();

                kanjis.ForEach(x => x.Trim());
                readings.ForEach(x => x.Trim());
                senses.ForEach(x => x.Trim());

                kanjis.RemoveAll(e => e == "");
                readings.RemoveAll(e => e == "");
                senses.RemoveAll(e => e == "");

                var entry = new WarodaiEntry(kanjis, readings, senses);

                entries.Add(entry);
            }
        }

        public List<WarodaiEntry> GetEntries()
        {
            return entries;
        }

        /// <summary>
        /// Looks up for query and returns certain amount of matches
        /// </summary>
        /// <param name="query">Query to look for</param>
        /// <param name="takeResults">Amount of results in output</param>
        /// <returns>List of entries matching the query</returns>
        public List<WarodaiEntry> Lookup(string query, int takeResults)
        {
            var result = new List<WarodaiEntry>();

            query = query.ToLower();

            if (!Regex.IsMatch(query, @"(\s?[^\s\p{IsCyrillic}])") || !Regex.IsMatch(query, @"(\s?[^\s\p{IsBasicLatin}])"))
            {
                //if user enters russian or english text, then look only in senses
                /* 
                 * descr. from Warodai.ru/help 
                 * - remove numbers, e.g. "1)"
                 * - remove everything in brackets, e.g. "(см.)"
                 * - split on ","
                 * - match query to splitted portions
                 * - strong position of word or sentence is full match on one of the splitted portions
                 * - weak positions are all other matches (partial)
                 * - entries with query in strong position go in the beginning of the result
                 */

                Func<string, string[]> cleanAndSplit =
                    str =>
                    {
                        var noNumbers = Regex.Replace(str, @"\d\)", "");
                        var noBrackets = Regex.Replace(noNumbers, @"\(  # First '('
                                                    (?:                 
                                                    [^()]               # Match all non-braces
                                                    |
                                                    (?<open> \( )       # Match '(', and capture into 'open'
                                                    |
                                                    (?<-open> \) )      # Match ')', and delete the 'open' capture
                                                    )+
                                                    (?(open)(?!))       # Fails if 'open' stack isn't empty!

                                                    \)                  # Last ')'"
                    , "");
                        var noEndingParen = Regex.Replace(noBrackets, @"\.\)$", "");
                        var toLower = noEndingParen.ToLower();
                        var splitted = noEndingParen.Split(',');
                        return splitted;
                    };

                // find entries, where query is mentioned as is
                result = entries.FindAll(x => x.Senses.Any(
                    s =>
                    {
                        var splitted = cleanAndSplit(s);
                        return splitted.Any(c => c == query);
                    }
                ));

                //find the other entries
                result = result.Union(entries.FindAll(x => x.Senses.Any(
                    s =>
                    {
                        var splitted = cleanAndSplit(s);
                        return splitted.Any(c => c.Contains(query));
                    }))).ToList();
            }
            else
            {
                //if user enters something else than cyrillic, 
                //consider it's kana|kanji and look in Kanjis and Readings first

                //find entries, where query is mentioned as is
                result = entries.FindAll(x =>
                                           x.Kanjis.Any(k => k == query)
                                        || x.Readings.Any(r => r == query)
                                        );

                //find entries, where words begin with query
                result = result.Union(entries.FindAll(x =>
                                             x.Kanjis.Any(k => k.StartsWith(query))
                                          || x.Readings.Any(r => r.StartsWith(query))
                    )).ToList();

                //find every other entry containing query
                result = result.Union(entries.FindAll(w => w.Kanjis.Any(k => k.Contains(query))
                            || w.Readings.Any(r => r.Contains(query)))).ToList();
            }

            return result.Take(takeResults).ToList();
        }

    }

    public class WarodaiEntry
    {
        public List<string> Kanjis { get; }
        public List<string> Readings { get; }
        public List<string> Senses { get; }

        public WarodaiEntry(List<string> kanjis, List<string> readings, List<string> senses)
        {
            Kanjis = kanjis;
            Readings = readings;
            Senses = senses;
        }

        public override string ToString()
        {
            return string.Format("{0}【{1}】\n{2}",
                string.Join("・", Readings),
                string.Join("・", Kanjis),
                string.Join("\n", Senses));
        }
    }
}
