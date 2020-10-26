using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Graphics.Imaging;

namespace RescaleHelper.Abstract
{
    public delegate void GetSetting(object _sender, Guid _guid);

    public interface IScalingSetting
    {

        event GetSetting GetSettingHandle;

        /// <summary>
        /// Command trigger GetSettingHandle Event
        /// </summary>
        ICommand DeleteCommand { get; set; }

        /// <summary>
        /// Object Guid to kee[ track and delete object if needed
        /// </summary>
        Guid Guid { get; set; }

        /// <summary>
        /// Error information if Height and Width have valid input
        /// </summary>
        string Info { get; }

        /// <summary>
        /// User input for Height values
        /// </summary>
        string Height { get; set; }

        /// <summary>
        /// User input for Width values
        /// </summary>
        string Width { get; set; }

        /// <summary>
        /// Parsed Height Value
        /// </summary>
        uint ScaleHeight { get; }

        /// <summary>
        /// Parsed Width Values
        /// </summary>
        uint ScaleWidth { get; }

        /// <summary>
        /// Are Height and Width acceptable Values
        /// </summary>
        bool Valid { get; }

        /// <summary>
        /// Specific Output directory for this setting
        /// </summary>
        string OutputDir { get; set; }

        /// <summary>
        /// Interpolation algorithm for scaling 
        /// </summary>
        BitmapInterpolationMode InterpolationModeSelected { get; set; }
    }
}
