using Permadelete.Helpers;
using Permadelete.Mvvm;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Permadelete.ViewModels
{
    public class SettingsVM : BindableBase
    {
        #region Fields
        private bool _isLoaded;
        #endregion

        #region Constructor
        public SettingsVM()
        {
            OpenMoreInfoCommand = new DelegateCommand(p => Process.Start(Constants.HOW_PERMADELETE_WORKS));
        }
        #endregion

        #region Properties
        private List<SelectableVM<Theme>> _themes;
        public List<SelectableVM<Theme>> Themes
        {
            get { return _themes; }
            set { Set(ref _themes, value); }
        }

        private int _defaultOverwritePasses;
        public int DefaultOverwritePasses
        {
            get { return _defaultOverwritePasses; }
            set
            {
                if (Set(ref _defaultOverwritePasses, value))
                {
                    Save();
                }
            }
        }

        private SelectableVM<Theme> _selectedTheme;
        public SelectableVM<Theme> SelectedTheme
        {
            get { return _selectedTheme; }
            set
            {

                if (Themes is null)
                    return;

                if (Set(ref _selectedTheme, value))
                {
                    foreach (var theme in Themes)
                    {
                        theme.IsSelected = false;
                    }

                    value.IsSelected = true;
                    ThemeHelper.ApplyTheme(value.Data);
                    Save();
                }
            }
        }

        #endregion

        #region Commands
        public DelegateCommand OpenMoreInfoCommand { get; }
        #endregion

        #region Methods
        public void Load()
        {
            Themes = ThemeHelper.GetAvailableThemes()
                    .Select(t => new SelectableVM<Theme>(t))
                    .ToList();

            var settings = SettingsHelper.GetSettings();

            SelectedTheme = Themes.FirstOrDefault(t => t.Data.Name.Equals(settings.Theme, System.StringComparison.OrdinalIgnoreCase)) ??
                            Themes.First();

            DefaultOverwritePasses = settings.DefaultOverwritePasses;

            _isLoaded = true;
        }

        private void Save()
        {
            if (!_isLoaded) return;

            var settings = SettingsHelper.GetSettings();

            settings.Theme = SelectedTheme.Data.Name;
            settings.DefaultOverwritePasses = DefaultOverwritePasses;

            SettingsHelper.SaveSettings(settings);
        }
        #endregion

    }
}
