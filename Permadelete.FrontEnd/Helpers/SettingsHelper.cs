using Newtonsoft.Json;
using System;
using System.IO;

namespace Permadelete.Helpers
{
    public class SettingsHelper
    {
        public static Settings GetSettings()
        {
            if (File.Exists(Constants.SETTINGS_PATH))
            {
                var json = File.ReadAllText(Constants.SETTINGS_PATH);

                try
                {
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
                catch (Exception)
                { }
            }

            return new Settings();
        }

        public static void SaveSettings(Settings settings)
        {
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(Constants.SETTINGS_PATH, json);
        }
    }

    public class Settings
    {
        public string Theme { get; set; }
        public Theme GetTheme() => ThemeHelper.GetThemeOrDefault(Theme);

        private int _defaultOverwritePasses = 1;
        public int DefaultOverwritePasses
        {
            get { return _defaultOverwritePasses; }
            set
            {
                if (value < 1 || value > 10)
                    return;
                _defaultOverwritePasses = value;
            }
        }

    }
}
