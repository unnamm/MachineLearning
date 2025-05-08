using Common;
using Common.Config;
using Common.Message;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using ML;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace UI.ViewModel
{
    public partial class ContentViewModel : ObservableObject
    {
        public Log LogInstance { get; }

        [ObservableProperty] private BitmapImage _loadImage;
        [ObservableProperty] private double _valueOK;
        [ObservableProperty] private double _valueNG;
        [ObservableProperty] private string _imageName;

        private readonly Execute _execute;

        public ContentViewModel(Log log, Execute exe)
        {
            _execute = exe;
            LogInstance = log;
        }

        [RelayCommand]
        public async Task ButtonLoadImage()
        {
            OpenFileDialog ofd = new()
            {
                Filter = "Image files (*.jpg)|*.jpg|All Files (*.*)|*.*"
            };

            if (ofd.ShowDialog() == true)
            {
                ImageName = Path.GetFileName(ofd.FileName);
                LoadImage = CreateBitmapFromFile(ofd.FileName);

                var (label, value) = await _execute.PredictAsync(ofd.FileName);
                ValueNG = value[0] * 100;
                ValueOK = value[1] * 100;

                LogInstance.Write($"{ImageName} : {label}");
            }
        }

        [RelayCommand]
        public Task ButtonOK()
        {
            return FileSave(true);
        }

        [RelayCommand]
        public Task ButtonNG()
        {
            return FileSave(false);
        }

        [RelayCommand]
        public async Task ButtonBuild()
        {
            try
            {
                SettingData config = new();
                config.Load();
                LogInstance.Write("start build model");
                await _execute.BuildModel(config.FolderPath, config.ModelName);
                LogInstance.Write("builded model and load new model");
                await _execute.LoadAsync(config.ModelName);

                ClearUI();
            }
            catch (DirectoryNotFoundException ex)
            {
                LogInstance.Write($"{ex.Message} | Setting FolderPath");
            }
            catch (Exception ex)
            {
                LogInstance.Write(ex.Message);
            }
        }

        [RelayCommand]
        public void ButtonOpenFolder()
        {
            try
            {
                SettingData config = new();
                config.Load();

                var process = Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe";
                Process.Start(process, config.FolderPath);
            }
            catch (Exception ex)
            {
                LogInstance.Write(ex.Message);
            }
        }

        private void ClearUI()
        {
            LoadImage = null;
            ImageName = string.Empty;
            ValueOK = 0;
            ValueNG = 0;
        }

        private async Task FileSave(bool boolValue)
        {
            SettingData config = new();
            config.Load();
            var imagePath = Path.Combine(config.FolderPath, boolValue.ToString(), $"{DateTime.Now:yyyy-MM-dd-hh-mm-ss}.jpg");

            var fileStream = new FileStream(imagePath, FileMode.Create);
            var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(LoadImage));
            encoder.Save(fileStream);
            await fileStream.DisposeAsync();

            WeakReferenceMessenger.Default.Send(new DialogMessage("", $"Save {boolValue}"));
        }

        private static BitmapImage CreateBitmapFromFile(string filePath)
        {
            BitmapImage image = new();
            image.BeginInit();
            image.UriSource = new Uri(filePath);
            image.EndInit();
            return image;
        }
    }
}
