using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.ComponentModel;

namespace PetitNovalTools
{
    public partial class ImageToolWindow : ToolWindow
    {
        public ImageToolWindow()
        {
            InitializeComponent();
            this.PropertyChanged += ImageToolWindow_PropertyChanged;
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void ImageToolWindow_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property.Name == "IsVisible")
            {
                if (args.NewValue != null)
                {
                    var v = (bool)args.NewValue;
                    if (v)
                    {
                        this.DataContext = new ImageToolViewModel();
                    }
                    else
                    {
                        this.DataContext = null;
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void previewerPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs args)
        {
            if (args.Property.Name == "IsVisible")
            {
                if(args.NewValue != null)
                {
                    var v = (bool) args.NewValue;
                    if (v)
                    {
                        this.Width = 800;
                    }
                    else
                    {
                        this.Width = 320;
                    }
                }
            }
        }
    }
}
