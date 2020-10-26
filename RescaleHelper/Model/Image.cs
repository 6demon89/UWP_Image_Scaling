using System;
using System.Windows.Input;
using RescaleHelper.Abstract;
using RescaleHelper.Contol;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace RescaleHelper.Model
{
    public class Image : IImage
    {
        public Image()
        {
            //Test if this will generate new guid
            m_Guid = Guid.NewGuid();
            DeleteCommand = new RelayCommand(() => GetImageHandle?.Invoke(this, Guid));
        }

        public string Name { get; set; }

        public bool Selected { get; set; }

        private SoftwareBitmap m_Bitmap;

        public SoftwareBitmap Bitmap
        {
            get => m_Bitmap;
            set
            {
                if (value.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                value.BitmapAlphaMode == BitmapAlphaMode.Straight)
                {
                    value = SoftwareBitmap.Convert(value, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }
                m_Bitmap = value;
            }
        }

        private Guid m_Guid;

        public event GetImage GetImageHandle;

        public Guid Guid { get => m_Guid; }

        public ICommand DeleteCommand { get; set; }
    }
}
