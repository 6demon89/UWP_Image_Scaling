using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;

namespace RescaleHelper.Abstract
{
    public delegate void GetImage(object _sender, Guid _guid);

    public interface IImage
    {
        event GetImage GetImageHandle;

        /// <summary>
        /// Command trigger GetImageHandle Event
        /// </summary>
        ICommand DeleteCommand { get; set; }

        /// <summary>
        /// Guid auto generated and used to get secific item.
        /// Usage for example would be deleting element from the list
        /// </summary>
        Guid Guid { get; }

        /// <summary>
        /// Actual file name with extention
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Loaded image from file
        /// </summary>
        SoftwareBitmap Bitmap { get; set; }

        /// <summary>
        /// Is this model currently selected.
        /// This property is needed to know which file to scale
        /// for UI
        /// </summary>
        bool Selected { get; set; }
    }
}
