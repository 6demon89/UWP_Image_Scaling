using RescaleHelper.Abstract;
using RescaleHelper.Contol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace RescaleHelper.Model
{
    public class ScalingSetting : INotifyPropertyChanged, IScalingSetting
    {
        public ScalingSetting()
        {
            Guid = Guid.NewGuid();
            DeleteCommand = new RelayCommand(() => GetSettingHandle?.Invoke(this, Guid));
        }

        public ICommand DeleteCommand { get; set; }

        public string Info
        {
            get
            {
                if (ScaleHeight == 0 || ScaleWidth == 0)
                    return "Please set Width and Height (numerics only)";
                return "";
            }
        }

        public bool Valid
        {
            get => Info == "" ? true : false;
        }

        public uint ScaleHeight { get; private set; }
        public string Height
        {
            get => ScaleHeight.ToString();
            set
            {
                if (uint.TryParse(value, out var number))
                {
                    ScaleHeight = number;
                }
                OnPropertyChanged("Height");
                OnPropertyChanged("Info");
            }
        }

        public uint ScaleWidth { get; private set; }
        public string Width
        {
            get => ScaleWidth.ToString();
            set
            {
                if (uint.TryParse(value, out var number))
                {
                    ScaleWidth = number;
                }
                OnPropertyChanged("Width");
                OnPropertyChanged("Info");
            }
        }

        public BitmapInterpolationMode InterpolationModeSelected { get; set; } = BitmapInterpolationMode.Cubic;

        public string IntepoloationModelSelector
        {
            get => InterpolationModeSelected.ToString();
            set
            {
                if (Enum.IsDefined(typeof(BitmapInterpolationMode), value))
                {
                    InterpolationModeSelected = Enum.Parse<BitmapInterpolationMode>(value);
                }
            }
        }
        public List<string> InterpolationMode
        {
            get => Enum.GetNames(typeof(BitmapInterpolationMode)).ToList();
        }

        public string OutputDir { get; set; }
        public Guid Guid { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event GetSetting GetSettingHandle;

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
