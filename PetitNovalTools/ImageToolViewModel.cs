using Avalonia;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace PetitNovalTools
{
    public class ImageToolViewModel : ReactiveObject
    {
        private const string CONV_MODE_P2B = "BIN转PNG";
        private const string CONV_MODE_B2P = "PNG转BIN";

        private bool showPreviewer;
        private string currentFolder;
        private string selectedMode;
        private ObservableCollection<string> files;
        private string selectedFile;
        private Bitmap previewImage;

        public bool ShowPreviewer
        {
            get => showPreviewer;
            set => this.RaiseAndSetIfChanged(ref showPreviewer, value);
        }
        public string CurrentFolder
        {
            get => currentFolder;
            set => this.RaiseAndSetIfChanged(ref currentFolder, value);
        }
        public List<string> ConversionModes => new List<string>
        {
            CONV_MODE_B2P,
            CONV_MODE_P2B
        };
        public string SelectedMode
        {
            get => selectedMode;
            set => this.RaiseAndSetIfChanged(ref selectedMode, value);
        }
        public ObservableCollection<string> Files
        {
            get => files;
            set => this.RaiseAndSetIfChanged(ref files, value);
        }
        public string SelectedFile
        {
            get => selectedFile;
            set => this.RaiseAndSetIfChanged(ref selectedFile, value);
        }
        public Bitmap PreviewImage
        {
            get => previewImage;
            set => this.RaiseAndSetIfChanged(ref previewImage, value);
        }

        public ImageToolViewModel()
        {
            PropertyChanged += OnPropertyChanged;
            SelectedMode = CONV_MODE_B2P;
        }

        private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedFile))
            {
                if (ShowPreviewer)
                {
                    if (File.Exists(SelectedFile))
                    {
                        if (SelectedMode == CONV_MODE_B2P)
                        {
                            var bimg = new Formats.BinaryImage(SelectedFile);
                            using (MemoryStream ms = new MemoryStream())
                            {
                                bimg.Image.SaveAsPng(ms);
                                ms.Position = 0;
                                PreviewImage = new Bitmap(ms);
                            }
                        }
                        else if (SelectedMode == CONV_MODE_P2B)
                        {
                            PreviewImage = new Bitmap(SelectedFile);
                        }
                        else
                        {
                            PreviewImage = null;
                        }
                    }
                }
            }
            else if (e.PropertyName == nameof(CurrentFolder))
            {
                TryLoadFolder();
            }
            else if (e.PropertyName == nameof(SelectedMode))
            {
                TryLoadFolder();
            }
        }

        private void TryLoadFolder()
        {
            if (!Directory.Exists(CurrentFolder))
            {
                return;
            }

            if (SelectedMode == CONV_MODE_B2P)
            {
                Files = new ObservableCollection<string>(ScanBinaryImages(CurrentFolder));
            }
            else if (SelectedMode == CONV_MODE_P2B)
            {
                Files = new ObservableCollection<string>(Directory.GetFiles(CurrentFolder, "*.png", SearchOption.AllDirectories));
            }
        }

        private List<string> ScanBinaryImages(string dir)
        {
            var binFiles = Directory.GetFiles(dir, "*.bin", SearchOption.AllDirectories);
            var result = new List<string>();
            foreach (var binFile in binFiles)
            {
                if (Formats.BinaryImage.IsValidBinaryImage(binFile))
                {
                    result.Add(binFile);
                }
            }
            return result;
        }

        public async void OpenFolder()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            CurrentFolder = await dialog.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);
        }
    }
}
