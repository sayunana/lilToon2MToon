using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace sayunana
{
    public class Helper
    {
        public enum Language
        {
            日本語,
            English
        }

        public static Language SystemLanguage = Language.日本語;
        private static Dictionary<string, string>  _enTrnDic;

        public static string Translate(string s)
        {
            if (SystemLanguage == Language.日本語)
            {
                return s;
            }
            if(_enTrnDic == null){ TranslateTextLoad();}

            return SystemLanguage switch
            {
                Language.English => _enTrnDic.ContainsKey(s) ? _enTrnDic[s] : s,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static void TranslateTextLoad()
        {
            var en = Resources.Load<TextAsset>("lilToon2MToon-en").ToString();
            _enTrnDic = Deserialize(en);
        }
        
        private static Dictionary<string, string> Deserialize(string json)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            json = json.Replace("{", "").Replace("}", "");

            var columns = json.Split(",\r\n");

            foreach (var column in columns)
            {
                var t=  column.Split(":");
                var key = t[0];
                var value = t[1];
                string pattern = $@"{Regex.Escape("\"")}(.*?){Regex.Escape("\"")}";
                var keyOut = Regex.Match(key, pattern);
                var valueOut = Regex.Match(value, pattern);
                dic.Add(keyOut.Groups[1].Value, valueOut.Groups[1].Value);
            }
            return dic;
        }
    }
}