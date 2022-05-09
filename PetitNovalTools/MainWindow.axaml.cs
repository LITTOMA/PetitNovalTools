using Avalonia.Controls;
using System.ComponentModel;

namespace PetitNovalTools
{
    public partial class MainWindow : Window
    {
        ImageToolWindow imageTool = new ImageToolWindow();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public void OpenImageTool()
        {
            imageTool.Show();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            imageTool.ForceClose();
            base.OnClosing(e);
        }
    }
}
