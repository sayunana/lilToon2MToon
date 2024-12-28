using System;
using System.Collections.Generic;
using Newtonsoft.Json;
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
            _enTrnDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(en);
        }
    }
}