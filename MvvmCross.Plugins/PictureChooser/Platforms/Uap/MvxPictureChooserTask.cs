// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MS-PL license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace MvvmCross.Plugin.PictureChooser.Platforms.Uap
{
    [Preserve(AllMembers = true)]
    public class MvxPictureChooserTask : IMvxPictureChooserTask
    {
        public void ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality, Action<Stream, string> pictureAvailable, Action assumeCancelled)
        {
            TakePictureCommon(StorageFileFromDisk, maxPixelDimension, percentQuality, pictureAvailable, assumeCancelled);
        }

        public void ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality, Action<Stream> pictureAvailable, Action assumeCancelled)
        {
            TakePictureCommon(StorageFileFromDisk, maxPixelDimension, percentQuality, (stream, name) => pictureAvailable(stream), assumeCancelled);
        }

        public void ChoosePicturesFromLibraryWithNames(int maxPixelDimension, int percentQuality, Action<Dictionary<Stream, string>> picturesAvailable, Action assumeCancelled)
        {
            TakePicturesCommon(StorageFilesFromDisk, maxPixelDimension, percentQuality, picturesAvailable, assumeCancelled);
        }

        public void ChoosePicturesFromLibrary(int maxPixelDimension, int percentQuality, Action<List<Stream>> picturesAvailable, Action assumeCancelled)
        {
            var picturesAvailableDictionaryAction = new Action<Dictionary<Stream, string>>(streamDictionary =>
            {
                if (streamDictionary == null || streamDictionary.Keys.Count == 0)
                {
                    picturesAvailable.Invoke(null);
                    return;
                }

                var streamList = streamDictionary.Keys.ToList();
                picturesAvailable.Invoke(streamList);
            });

            ChoosePicturesFromLibraryWithNames(maxPixelDimension, percentQuality, picturesAvailableDictionaryAction, assumeCancelled);
        }

        public void TakePicture(int maxPixelDimension, int percentQuality, Action<Stream> pictureAvailable, Action assumeCancelled)
        {
            TakePictureCommon(StorageFileFromCamera, maxPixelDimension, percentQuality, (stream, name) => pictureAvailable(stream), assumeCancelled);
        }

        public Task<Stream> ChoosePictureFromLibrary(int maxPixelDimension, int percentQuality)
        {
            var task = new TaskCompletionSource<Stream>();
            ChoosePictureFromLibrary(maxPixelDimension, percentQuality, task.SetResult, () => task.SetResult(null));
            return task.Task;
        }

        public Task<Dictionary<Stream, string>> ChoosePicturesFromLibrary(int maxPixelDimension, int percentQuality)
        {
            var task = new TaskCompletionSource<Dictionary<Stream, string>>();
            ChoosePicturesFromLibraryWithNames(maxPixelDimension, percentQuality, task.SetResult, () => task.SetResult(null));
            return task.Task;
        }

        public Task<Stream> TakePicture(int maxPixelDimension, int percentQuality)
        {
            var task = new TaskCompletionSource<Stream>();
            TakePicture(maxPixelDimension, percentQuality, task.SetResult, () => task.SetResult(null));
            return task.Task;
        }

        public void ContinueFileOpenPicker(object args)
        {
        }

        private void TakePictureCommon(Func<Task<StorageFile>> storageFile, int maxPixelDimension, int percentQuality, 
            Action<Stream, string> pictureAvailable, Action assumeCancelled)
        {
            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await Process(storageFile, maxPixelDimension, percentQuality, pictureAvailable, assumeCancelled);
            });
        }

        private async Task Process(Func<Task<StorageFile>> storageFile, int maxPixelDimension, int percentQuality, Action<Stream, string> pictureAvailable, Action assumeCancelled)
        {
            var file = await storageFile();
            if (file == null)
            {
                assumeCancelled();
                return;
            }

            var rawFileStream = await file.OpenAsync(FileAccessMode.Read);
            var resizedStream = await ResizeJpegStreamAsync(maxPixelDimension, percentQuality, rawFileStream);

            pictureAvailable(resizedStream.AsStreamForRead(), file.DisplayName);
        }

        private void TakePicturesCommon(Func<Task<IReadOnlyList<StorageFile>>> storageFiles, int maxPixelDimension, int percentQuality,
            Action<Dictionary<Stream, string>> picturesAvailable, Action assumeCancelled)
        {
            var dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await ProcessFiles(storageFiles, maxPixelDimension, percentQuality, picturesAvailable, assumeCancelled);
            });
        }

        private async Task ProcessFiles(Func<Task<IReadOnlyList<StorageFile>>> storageFile, int maxPixelDimension, int percentQuality, Action<Dictionary<Stream, string>> picturesAvailable, Action assumeCancelled)
        {
            var files = await storageFile();
            if (files == null || files.Count == 0)
            {
                assumeCancelled();
                return;
            }

            var streams = new Dictionary<Stream, string>();

            foreach (var file in files)
            {
                var rawFileStream = await file.OpenAsync(FileAccessMode.Read);
                var resizedStream = await ResizeJpegStreamAsync(maxPixelDimension, percentQuality, rawFileStream);

                streams.Add(resizedStream.AsStreamForRead(), file.DisplayName);
            }

            picturesAvailable(streams);
        }

        private static async Task<StorageFile> StorageFileFromCamera()
        {
            var dialog = new CameraCaptureUI();
            var file = await dialog.CaptureFileAsync(CameraCaptureUIMode.Photo);
            return file;
        }

        private static async Task<StorageFile> StorageFileFromDisk()
        {
            var filePicker = GetFilePicker();

            return await filePicker.PickSingleFileAsync();
        }

        private static async Task<IReadOnlyList<StorageFile>> StorageFilesFromDisk()
        {
            var filePicker = GetFilePicker();

            return await filePicker.PickMultipleFilesAsync();
        }

        private static FileOpenPicker GetFilePicker()
        {
            var filePicker = new FileOpenPicker();

            filePicker.FileTypeFilter.Add(".jpg");
            filePicker.FileTypeFilter.Add(".jpeg");
            filePicker.ViewMode = PickerViewMode.Thumbnail;
            filePicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

            return filePicker;
        }

        private async Task<IRandomAccessStream> ResizeJpegStreamAsync(int maxPixelDimension, int percentQuality, IRandomAccessStream input)
        {
            var decoder = await BitmapDecoder.CreateAsync(input);

            int targetHeight;
            int targetWidth;
            MvxPictureDimensionHelper.TargetWidthAndHeight(maxPixelDimension, (int)decoder.PixelWidth, (int)decoder.PixelHeight, out targetWidth, out targetHeight);

            var transform = new BitmapTransform() { ScaledHeight = (uint)targetHeight, ScaledWidth = (uint)targetWidth };
            var pixelData = await decoder.GetPixelDataAsync(
                BitmapPixelFormat.Rgba8,
                BitmapAlphaMode.Straight,
                transform,
                ExifOrientationMode.RespectExifOrientation,
                ColorManagementMode.DoNotColorManage);

            var destinationStream = new InMemoryRandomAccessStream();
            var bitmapPropertiesSet = new BitmapPropertySet();
            bitmapPropertiesSet.Add("ImageQuality", new BitmapTypedValue((double)percentQuality / 100.0, PropertyType.Single));
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, destinationStream, bitmapPropertiesSet);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Premultiplied, (uint)targetWidth, (uint)targetHeight, decoder.DpiX, decoder.DpiY, pixelData.DetachPixelData());
            await encoder.FlushAsync();
            destinationStream.Seek(0L);
            return destinationStream;
        }
    }
}
