using RescaleHelper.Abstract;
using RescaleHelper.Contol;
using RescaleHelper.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Image = RescaleHelper.Model.Image;

namespace RescaleHelper.ViewModel
{

    public class ScalingViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<IScalingSetting> ScaleSetting { get; set; }
        public ObservableCollection<IImage> BitmapFileList { get; set; }

        public string ScalingButtonContent { get; set; } = "Rescale";

        public SoftwareBitmapSource BitmapSource { get; set; }
        public string OutputPath { get; set; } = "Select Folder";
        private Image _SelectedImage;
        public Image SelectedImage
        {
            get => _SelectedImage;
            set
            {
                _SelectedImage = value;
                SoftwareSourceConverter();
                OnPropertyChanged("SelectedImage");
            }
        }

        #region ICommands
        public ICommand AddFileCommand { get; set; }
        public ICommand ClearFileListCommand { get; set; }
        public ICommand ScaleCommand { get; set; }
        public ICommand OutputDirCommand { get; set; }
        public ICommand AddSettingCommand { get; set; }
        #endregion

        public ScalingViewModel()
        {
            ScaleSetting = new ObservableCollection<IScalingSetting>();
            var setting = new ScalingSetting();
            setting.GetSettingHandle += Setting_DeleteHandle;
            ScaleSetting.Add(setting);

            BitmapFileList = new ObservableCollection<IImage>();
            AddFileCommand = new RelayCommand(async () => await AddSoftwareBitmap());
            ClearFileListCommand = new RelayCommand(() => ClearSoftwareBitmap());
            ScaleCommand = new RelayCommand(async () => await ScaleFiles());
            OutputDirCommand = new RelayCommand(async () => await SetOutputDir());
            AddSettingCommand = new RelayCommand(() => AddSetting());
        }

        private async void DisplayNoWifiDialog(string _text)
        {
            ContentDialog noWifiDialog = new ContentDialog
            {
                Title = "Info",
                Content = _text,
                CloseButtonText = "Ok"
            };

            ContentDialogResult result = await noWifiDialog.ShowAsync();
        }

        private void Setting_DeleteHandle(object _sender, Guid _guid)
        {
            var setting = ScaleSetting.FirstOrDefault(x => x.Guid == _guid);
            if (setting is null) return;
            ScaleSetting.Remove(setting);
            OnPropertyChanged("ScaleSetting");
        }

        private void AddSetting()
        {
            var setting = new ScalingSetting();
            setting.GetSettingHandle += Setting_DeleteHandle;
            ScaleSetting.Add(setting);
            OnPropertyChanged("ScaleSetting");
        }

        private async void SoftwareSourceConverter()
        {
            if (SelectedImage is null) return;
            if (SelectedImage.Bitmap is null) return;
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(SelectedImage.Bitmap);
            BitmapSource = source;
            OnPropertyChanged("BitmapSource");
        }

        private async Task SetOutputDir()
        {
            var path = await GetDirectoryFromUser();
            if (string.IsNullOrEmpty(path)) return;
            OutputPath = path;
            OnPropertyChanged("OutputPath");
        }

        private async Task<string> GetDirectoryFromUser()
        {
            FolderPicker picker = new FolderPicker();
            picker.SuggestedStartLocation = PickerLocationId.Desktop;
            picker.FileTypeFilter.Add("*");
            var output = await picker.PickSingleFolderAsync();
            if (output is null) return "";
            return output.Path;

        }

        private async Task AddSoftwareBitmap()
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpeg");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var files = await fileOpenPicker.PickMultipleFilesAsync();

            if (files.Count == 0) return;

            foreach (var inputFile in files)
            {
                SoftwareBitmap softwareBitmap;
                using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                    softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                }
                var image = new Image
                {
                    Bitmap = softwareBitmap,
                    Name = inputFile.Name
                };
                image.GetImageHandle += Image_GetImageHandle;
                BitmapFileList.Add(image);
            }
        }

        private void Image_GetImageHandle(object _sender, Guid _guid)
        {
            var img = BitmapFileList.FirstOrDefault(x => x.Guid == _guid);
            if (SelectedImage != null)
                if (img.Guid == SelectedImage.Guid)
                {
                    BitmapSource = null;
                    OnPropertyChanged("BitmapSource");
                    SelectedImage = null;
                    OnPropertyChanged("SelectedImage");
                }
            if (img is null) return;
            BitmapFileList.Remove(img);
            OnPropertyChanged("BitmapFileList");
        }

        public async Task<StorageFolder> GetFolder(IScalingSetting _settings)
        {
            StorageFolder storage = null;
            if (!string.IsNullOrEmpty(_settings.OutputDir))
            {
                storage = await StorageFolder.GetFolderFromPathAsync(_settings.OutputDir);
            }
            if (storage is null)
            {
                //Probably not the nicest way to solve it XD
                try
                {
                    storage = await StorageFolder.GetFolderFromPathAsync(OutputPath);
                }
                catch
                {
                    var path = await GetDirectoryFromUser();
                    //Probably here would be nice a dialog window to inform user that operation is canceled
                    if (string.IsNullOrEmpty(path))
                        return null;
                    storage = await StorageFolder.GetFolderFromPathAsync(path);
                    OutputPath = path;
                }
            }
            return storage;
        }

        private async Task ScaleFiles()
        {
            foreach (var pic in BitmapFileList)
            {
                foreach (var item in ScaleSetting.Where(x => x.Info == "").ToList())
                {
                    StorageFolder storage = await GetFolder(item);
                    if (storage is null) return;
                    string filename = $"{pic.Name}_{item.Height}_{item.Width}.jpeg";
                    var FileToWriteExists = await storage.TryGetItemAsync(filename);
                    if (FileToWriteExists != null) continue;
                    var FileToWrite = await storage.CreateFileAsync(filename);

                    using (IRandomAccessStream stream = await FileToWrite.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                        encoder.SetSoftwareBitmap(pic.Bitmap);

                        encoder.BitmapTransform.ScaledWidth = item.ScaleWidth;
                        encoder.BitmapTransform.ScaledHeight = item.ScaleHeight;
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;
                        encoder.IsThumbnailGenerated = true;
                        try
                        {
                            await encoder.FlushAsync();
                        }
                        catch (Exception err)
                        {
                            const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                            switch (err.HResult)
                            {
                                case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                                    encoder.IsThumbnailGenerated = false;
                                    break;
                                default:
                                    throw;
                            }
                        }

                        if (encoder.IsThumbnailGenerated == false)
                        {
                            await encoder.FlushAsync();
                        }
                    }
                }
            }
            if (BitmapFileList.Count > 0)
                DisplayNoWifiDialog($"{BitmapFileList.Count} Files were rescalled");
            DisplayNoWifiDialog("No images were found.Please add images to the list");
        }

        private void ClearSoftwareBitmap()
        {
            BitmapFileList.Clear();
            OnPropertyChanged("BitmapFileList");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.High,
                () =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                });
        }

    }
}
