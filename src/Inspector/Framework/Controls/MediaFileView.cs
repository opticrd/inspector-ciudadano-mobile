using NativeMedia;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Inspector.Framework.Controls
{
    public class MediaFileView : ContentView
    {
        public MediaFileView()
        {
            OnChanged();
        }

        public static readonly BindableProperty FileProperty =
            BindableProperty.Create(
                nameof(File),
                typeof(IMediaFile),
                typeof(MediaFileView),
                null,
                BindingMode.OneWay,
                propertyChanged: (b, o, n) => ((MediaFileView)b).OnChanged());

        public IMediaFile File
        {
            get => (IMediaFile)GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        private async void OnChanged()
        {
            var file = File;

            if (file?.Type == null)
            {
                Content = null;
                return;
            }

            var stream = await File.OpenReadAsync();

            var name = (string.IsNullOrWhiteSpace(File.NameWithoutExtension)
                ? Guid.NewGuid().ToString()
                : File.NameWithoutExtension)
                + $".{File.Extension}";

            var path = await SaveToCacheAsync(stream, name);

            switch (file.Type)
            {
                case MediaFileType.Image:
                    Content = new Image { Source = path };
                    break;
                case MediaFileType.Video:
                    Content = new MediaElement { Source = path };
                    break;
                default:
                    break;
            }

            File.Dispose();
        }

        public async Task<string> SaveToCacheAsync(Stream data, string fileName)
        {
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            var stream = System.IO.File.Create(filePath);
            await data.CopyToAsync(stream);
            stream.Close();

            return filePath;
        }
    }
}