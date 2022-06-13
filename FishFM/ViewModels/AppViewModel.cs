using Avalonia.Themes.Fluent;
using ReactiveUI;

namespace FishFM.ViewModels
{
    
    public class AppViewModel : ViewModelBase
    {
        private FluentThemeMode _themeMode = FluentThemeMode.Light;

        public FluentThemeMode ThemeMode
        {
            get => _themeMode;
            set => this.RaiseAndSetIfChanged(ref _themeMode, value); 
        }

    }
}