using Newtonsoft.Json;
using Permadelete.ApplicationManagement;
using Permadelete.Services;
using System;
using System.IO;
using System.Windows.Media;

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
        public int DefaultOverwritePasses { get; set; } = 1;
    }
}
