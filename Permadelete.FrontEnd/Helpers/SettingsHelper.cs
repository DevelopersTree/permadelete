using Newtonsoft.Json;
using System;
using System.IO;

namespace Permadelete.Helpers
{
    public class SettingsHelper
    {
        private static string GetSettingsPath()
        {
#if WINDOWS_STORE
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var localState = "Packages\\50203DevelopersTree.Permadelete_65cpy1vfn42n6\\LocalState";
            return Path.Combine(appData, localState, Constants.SETTINGS_PATH);
#else
            return Constants.SETTINGS_PATH;
#endif
        }

        public static Settings GetSettings()
        {
            if (File.Exists(GetSettingsPath()))
            {
                var json = File.ReadAllText(GetSettingsPath());

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
            File.WriteAllText(GetSettingsPath(), json);
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
