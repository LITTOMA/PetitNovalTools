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
            this.DataContext = new ImageToolViewModel();
#if DEBUG
            this.AttachDevTools();
#endif
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
