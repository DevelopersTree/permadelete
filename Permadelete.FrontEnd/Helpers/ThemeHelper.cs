using Permadelete.ApplicationManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Permadelete.Helpers
{
    public class ThemeHelper
    {
        public const string DEFAULT_THEME_NAME = "Blue Dark";

        private const string LIGHT_THEME_BACKGROUND = "#FFFEFEFE";
        private const string LIGHT_THEME_TEXT_COLOR = "#F333";

        private const string DARK_THEME_BACKGROUND = "#F333";
        private const string DARK_THEME_TEXT_COLOR = "#FFFEFEFE";

        public static IEnumerable<Theme> GetAvailableThemes()
        {
            return LoadThemes().Select(t => new Theme
            {
                Name = t.Name,
                AccentColor = (Color)ColorConverter.ConvertFromString(t.AccentColor),
                AccentLightColor = (Color)ColorConverter.ConvertFromString(t.AccentLightColor),
                TextColor = (Color)ColorConverter.ConvertFromString(t.TextColor),
                BackgroundColor = (Color)ColorConverter.ConvertFromString(t.BackgroundColor),
            }).ToArray();
        }

        public static Theme GetThemeOrDefault(string theme)
        {
            var themes = GetAvailableThemes();

            return themes.FirstOrDefault(t => t.Name?.Equals(theme, StringComparison.OrdinalIgnoreCase) == true) ??
                   themes.FirstOrDefault(t => t.Name?.Equals(DEFAULT_THEME_NAME, StringComparison.OrdinalIgnoreCase) == true);
        }

        private static IEnumerable<SerializedTheme> LoadThemes()
        {
            return new List<SerializedTheme>
            {
                new SerializedTheme
                {
                    Name = "Magenta Light",
                    AccentColor= "#FF8E44AD",
                    AccentLightColor = "#FF9B59B6",
                    BackgroundColor = LIGHT_THEME_BACKGROUND,
                    TextColor = LIGHT_THEME_TEXT_COLOR,
                },
                new SerializedTheme
                {
                    Name = "Magenta Dark",
                    AccentColor= "#FF8E44AD",
                    AccentLightColor = "#FF9B59B6",
                    BackgroundColor = DARK_THEME_BACKGROUND,
                    TextColor = DARK_THEME_TEXT_COLOR,
                },
                new SerializedTheme
                {
                    Name = "Blue Light",
                    AccentColor= "#1E90FF",
                    AccentLightColor = "#00BFFF",
                    BackgroundColor = LIGHT_THEME_BACKGROUND,
                    TextColor = LIGHT_THEME_TEXT_COLOR,
                },
                new SerializedTheme
                {
                    Name = "Blue Dark",
                    AccentColor= "#1E90FF",
                    AccentLightColor = "#00BFFF",
                    BackgroundColor = DARK_THEME_BACKGROUND,
                    TextColor = DARK_THEME_TEXT_COLOR,
                },
            };
        }

        public static void ApplyTheme(Theme theme)
        {
            UpdateSolidColorBrush("AccentColor", theme.AccentColor);
            UpdateSolidColorBrush("AccentLightColor", theme.AccentLightColor);
            UpdateSolidColorBrush("WindowBackgroundColor", theme.BackgroundColor);
            UpdateSolidColorBrush("ForegroundColor", theme.TextColor);
            UpdateThatch();
        }

        private static void UpdateSolidColorBrush(string name, Color color)
        {
            App.Current.Resources.Remove(name);
            App.Current.Resources[name] = color;

            var brushName = name + "Brush";

            App.Current.Resources.Remove(brushName);
            App.Current.Resources[brushName] = new SolidColorBrush(color);
        }

        private static void UpdateThatch()
        {
            var path1 = new Path
            {
                Data = Geometry.Parse("M 0 15 L 15 0"),
                Stroke = (Brush)App.Current.Resources["AccentLightColorBrush"],
                Opacity = 0.5
            };

            var path2 = new Path
            {
                Data = Geometry.Parse("M 0 0 L 15 15"),
                Stroke = (Brush)App.Current.Resources["AccentLightColorBrush"],
                Opacity = 0.5
            };

            var grid = new Grid { Background = (Brush)App.Current.Resources["AccentColorBrush"] };
            grid.Children.Add(path1);
            grid.Children.Add(path2);

            var brush = new VisualBrush(grid)
            {
                TileMode = TileMode.Tile,
                Viewport = System.Windows.Rect.Parse("0,0,15,15"),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = System.Windows.Rect.Parse("0,0,15,15"),
                ViewportUnits = BrushMappingMode.Absolute,
            };

            App.Current.Resources["TitlebarBackgroundThatch"] = brush;
        }
    }

    class SerializedTheme
    {
        public string Name { get; set; }
        public string AccentColor { get; set; }
        public string AccentLightColor { get; set; }
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }
    }

    public class Theme
    {
        public string Name { get; set; }
        public Color AccentColor { get; set; }
        public Color AccentLightColor { get; set; }
        public Color TextColor { get; set; }
        public Color BackgroundColor { get; set; }
    }
}
