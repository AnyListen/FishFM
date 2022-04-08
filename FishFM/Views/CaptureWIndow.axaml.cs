using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace FishFM.Views
{
    public partial class CaptureWindow : Window
    {
        public CaptureWindow()
        {
            Width = Screens.Primary.Bounds.Width;
            Height = Screens.Primary.Bounds.Height;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void InputElement_OnKeyDown(object? sender, KeyEventArgs e)
        {
            
        }

        private void TopLevel_OnOpened(object? sender, EventArgs e)
        {
            WindowState = WindowState.FullScreen;
        }
    }
}