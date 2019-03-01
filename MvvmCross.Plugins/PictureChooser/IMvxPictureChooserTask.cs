// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MvvmCross.Plugin.PictureChooser
{
    public interface IMvxPictureChooserTask
    {
        /// <summary>
        /// Allow to Choose a picture from library.
        /// </summary>
        /// <param name="maxPixelDimension">The maximum pixel dimension.</param>
        /// <param name="percentQuality">The quality in percent.</param>
        /// <param name="pictureAvailable">Action to invoke when picture is available. Where parameters are the picture stream and picture display name.</param>
        /// <param name="assumeCancelled">Action to invoke on cancellation</param>
        void ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality, Action<Stream, string> pictureAvailable,
                                      Action assumeCancelled);

        /// <summary>
        /// Allow to Choose a picture from library.
        /// </summary>
        /// <param name="maxPixelDimension">The maximum pixel dimension.</param>
        /// <param name="percentQuality">The quality in percent.</param>
        /// <param name="pictureAvailable">Action to invoke when picture is available.</param>
        /// <param name="assumeCancelled">Action to invoke on cancellation</param>
        void ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality, Action<Stream> pictureAvailable,
                                      Action assumeCancelled);

        /// <summary>
        /// Allow to Choose multiple pictures from library with picture name.
        /// </summary>
        /// <param name="maxPixelDimension">The maximum pixel dimension.</param>
        /// <param name="percentQuality">The quality in percent.</param>
        /// <param name="picturesAvailable">Action to invoke when pictures are available. Where parameters are the streams and display names of pictures.</param>
        /// <param name="assumeCancelled">Action to invoke on cancellation</param>
        void ChoosePicturesFromLibraryWithNames(int maxPixelDimension, int percentQuality, Action<Dictionary<Stream, string>> picturesAvailable,
            Action assumeCancelled);

        /// <summary>
        /// Allow to Choose multiple pictures from library.
        /// </summary>
        /// <param name="maxPixelDimension">The maximum pixel dimension.</param>
        /// <param name="percentQuality">The quality in percent.</param>
        /// <param name="picturesAvailable">Action to invoke when pictures are available.</param>
        /// <param name="assumeCancelled">Action to invoke on cancellation</param>
        void ChoosePicturesFromLibrary(int maxPixelDimension, int percentQuality, Action<List<Stream>> picturesAvailable,
            Action assumeCancelled);

        /// <summary>
        /// Allow to take a picture using the device camera
        /// </summary>
        /// <param name="maxPixelDimension">Maximum pixel dimension.</param>
        /// <param name="percentQuality">Image quality in percent.</param>
        /// <param name="pictureAvailable"> Action to invoke when picture is available.</param>
        /// <param name="assumeCancelled">Action to invoke on cancellation.</param>
        void TakePicture(int maxPixelDimension, int percentQuality, Action<Stream> pictureAvailable,
                         Action assumeCancelled);

        /// <summary>
        /// Returns null if cancelled
        /// </summary>
        Task<Stream> ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality);

        /// <summary>
        /// Returns null if cancelled
        /// </summary>
        Task<Dictionary<Stream, string>> ChoosePicturesFromLibrary(int maxPixelDimension, int percentQuality);

        /// <summary>
        /// Returns null if cancelled
        /// </summary>
        Task<Stream> TakePicture(int maxPixelDimension, int percentQuality);

        void ContinueFileOpenPicker(object args);
    }
}
