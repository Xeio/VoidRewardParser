using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace VoidRewardParser.Logic
{
    internal static class LocalizationManager
    {
        private static Lazy<Dictionary<string, string>> _localizedStrings = new Lazy<Dictionary<string, string>>(() => LoadLocalizedStrings());

        public static string MissionCompleteString
        {
            get { return Localize("VOID MISSION COMPLETE"); }
        }

        public static string Localize(string stringToLocalize)
        {
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
            string language = ConfigurationManager.AppSettings["Language"];
            if(language == "English")
            {
                //Cheat, we don't need to localize these
                return new Dictionary<string, string>();
            }
            if (Directory.Exists("Localization"))
            {
                string filePath = Path.Combine("Localization", language);
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
            throw new Exception($"Language file '{language}' not found");
        }
    }
}
