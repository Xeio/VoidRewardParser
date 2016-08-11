using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace VoidRewardParser.Logic
{
    internal static class LocalizationManager
    {
        private static Lazy<Dictionary<string, string>> _localizedStrings = new Lazy<Dictionary<string, string>>(() => LoadLocalizedStrings());

        public static string Language
        {
            get { return ConfigurationManager.AppSettings["Language"]; }
        }

        public static string MissionComplete
        {
            get { return Localize("VOID MISSION COMPLETE"); }
        }

        public static string MissionSuccess
        {
            get { return Localize("MISSION SUCCESS"); }
        }

        public static string Localize(string stringToLocalize)
        {
            if (string.IsNullOrWhiteSpace(stringToLocalize)) return stringToLocalize;
            string localizedString;
            _localizedStrings.Value.TryGetValue(stringToLocalize, out localizedString);
            if (!string.IsNullOrWhiteSpace(localizedString))
            {
                return localizedString;
            }
            return stringToLocalize;
        }

        public static Dictionary<string, string> LoadLocalizedStrings()
        {
            if (Language == "English")
            {
                //Cheat, we don't need to localize these
                return new Dictionary<string, string>();
            }
            if (Directory.Exists("Localization"))
            {
                string filePath = Path.Combine("Localization", Language);
                filePath = Path.ChangeExtension(filePath, ".txt");
                if (File.Exists(filePath))
                {
                    var localizedStrings = new Dictionary<string, string>();
                    foreach (string line in File.ReadAllLines(filePath))
                    {
                        string[] splitline = line.Split(new []{ ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if(splitline.Length == 2)
                        {
                            localizedStrings.Add(splitline[0], splitline[1]);
                        }
                    }
                    return localizedStrings;
                }
            }
            throw new Exception($"Language file '{Language}' not found");
        }
    }
}
