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
using PetitNovalTools.Formats;

namespace PetitNovalTools
{
    public class ImageToolViewModel : ReactiveObject
    {
        private const string CONV_MODE_P2B = "PNG转BIN";
        private const string CONV_MODE_B2P = "BIN转PNG";

        private bool showPreviewer;
        private string currentFolder;
        private string selectedMode;
        private ObservableCollection<string> files;
        private string selectedFile;
        private Bitmap previewImage;
        private double progress;

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
        public double Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
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

        private async void TryLoadFolder()
        {
            if (!Directory.Exists(CurrentFolder))
            {
                return;
            }

            if (SelectedMode == CONV_MODE_B2P)
            {
                var binFiles = await Task.Run(() => ScanBinaryImages(CurrentFolder));
                Files = new ObservableCollection<string>(binFiles);
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
            Progress = 0;
            for (int i = 0; i < binFiles.Length; i++)
            {
                var binFile = binFiles[i];
                if (Formats.BinaryImage.IsValidBinaryImage(binFile))
                {
                    result.Add(binFile);
                }
                Progress = (double)i++ / (double)binFiles.Length * 100;
            }
            Progress = 100f;
            return result;
        }

        public async void OpenFolder()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            CurrentFolder = await dialog.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);
        }

        public async void ConvertFiles()
        {
            OpenFolderDialog dialog = new OpenFolderDialog();
            var saveFolder = await dialog.ShowAsync((Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime).MainWindow);

            if (saveFolder == null)
                return;

            if (SelectedMode == CONV_MODE_B2P)
            {
                await Task.Run(() => ConvertBinsToPngs(saveFolder));
            }
            else if (SelectedMode == CONV_MODE_P2B)
            {
                await Task.Run(() => ConvertPngsToBins(saveFolder));
            }
        }

        private void ConvertPngsToBins(string saveFolder)
        {
            var currentFolderFullPath = Path.GetFullPath(CurrentFolder);
            var saveFolderFullPath = Path.GetFullPath(saveFolder);

            Progress = 0;
            int handledFileCount = 0;
            Parallel.ForEach(Files, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, file =>
            {
                var fullPath = Path.GetFullPath(file);
                var savePath = Path.ChangeExtension(fullPath.Replace(currentFolderFullPath, saveFolderFullPath), ".bin");
                var saveFileFolder = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveFileFolder))
                    Directory.CreateDirectory(saveFileFolder);

                var image = SixLabors.ImageSharp.Image.Load(file);
                if (image != null)
                {
                    BinaryImage binaryImage = new BinaryImage(image);
                    binaryImage.Save(savePath);
                }

                lock (this)
                {
                    handledFileCount++;
                    Progress = (double)handledFileCount / Files.Count * 100;
                }
            });
            Progress = 100f;
        }

        private void ConvertBinsToPngs(string saveFolder)
        {
            var currentFolderFullPath = Path.GetFullPath(CurrentFolder);
            var saveFolderFullPath = Path.GetFullPath(saveFolder);

            Progress = 0;
            int handledFileCount = 0;
            Parallel.ForEach(Files, new ParallelOptions() { MaxDegreeOfParallelism = 4 }, file =>
            {
                var fullPath = Path.GetFullPath(file);
                var savePath = Path.ChangeExtension(fullPath.Replace(currentFolderFullPath, saveFolderFullPath), ".png");
                var saveFileFolder = Path.GetDirectoryName(savePath);
                if (!Directory.Exists(saveFileFolder))
                    Directory.CreateDirectory(saveFileFolder);

                BinaryImage binaryImage = new BinaryImage(file);
                binaryImage.Image.SaveAsPng(savePath);

                lock (this)
                {
                    handledFileCount++;
                    Progress = (double)handledFileCount / Files.Count * 100;
                }
            });
            Progress = 100f;
        }
    }
}
