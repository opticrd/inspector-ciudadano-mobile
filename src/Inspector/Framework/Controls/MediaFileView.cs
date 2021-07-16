using Inspector.Models;
using NativeMedia;
using System;
using System.IO;
using System.Threading.Tasks;
using Xamarin.CommunityToolkit.UI.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using MediaFile = Inspector.Models.MediaFile;

namespace Inspector.Framework.Controls
{
    public class MediaFileView : ContentView
    {
        public MediaFileView()
        {
            //OnFileChanged();
            //OnAttachmentChanged();
        }

        public static readonly BindableProperty FileProperty =
            BindableProperty.Create(
                nameof(File),
                typeof(IMediaFile),
                typeof(MediaFileView),
                null,
                BindingMode.OneWay,
                propertyChanged: (b, o, n) => ((MediaFileView)b).OnFileChanged());

        public IMediaFile File
        {
            get => (IMediaFile)GetValue(FileProperty);
            set => SetValue(FileProperty, value);
        }

        public static readonly BindableProperty AttachmentProperty =
            BindableProperty.Create(
                nameof(Attachment),
                typeof(MediaFile),
                typeof(MediaFileView),
                null,
                BindingMode.OneWay,
                propertyChanged: (b, o, n) => ((MediaFileView)b).OnAttachmentChanged());

        public MediaFile Attachment
        {
            get => (MediaFile)GetValue(AttachmentProperty);
            set => SetValue(AttachmentProperty, value);
        }

        private async void OnFileChanged()
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

        private async void OnAttachmentChanged()
        {
            if (Attachment?.Data == null)
            {
                Content = null;
                return;
            }

            switch (Attachment.Type)
            {
                case FileType.Image:

                    var imageSource = ImageSource.FromStream(() => Attachment.Data);

                    Content = new Image { Source = imageSource };
                    break;
                case FileType.Video:

                    //var path = await SaveToCacheAsync(Attachment.Data, "video");

                    Content = new MediaElement 
                    { 
                        Source = new Xamarin.CommunityToolkit.Core.StreamMediaSource() 
                        { 
                            Stream = new Func<System.Threading.CancellationToken, Task<Stream>>((cancellationToken) => 
                            { 
                                return Task.FromResult<Stream>(Attachment.Data); 
                            })
                        }
                    };
                    break;
                default:
                    break;
            }
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