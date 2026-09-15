using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NoFences.Util
{
    /// <summary>
    /// Generates and caches icon thumbnails for supported image files, decoding
    /// them asynchronously in the background to keep the UI responsive.
    /// </summary>
    public class ThumbnailProvider
    {
        // Supported .NET images as per https://docs.microsoft.com/en-us/dotnet/api/system.drawing.image.fromfile
        private static readonly string[] SupportedExtensions =
        {
            ".bmp",
            ".gif",
            ".jpg",
            ".jpeg",
            ".png",
            ".tiff",
            ".tif"
        };

        private class ThumbnailState
        {
            public Icon icon;
        }

        // Only allow 4 concurrent images to be decoded to try and prevent OOM errors
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(4);
        private readonly IDictionary<string, ThumbnailState> iconCache = new Dictionary<string, ThumbnailState>();
        private readonly object cacheLock = new object();

        /// <summary>
        /// Raised on a background thread when a higher-quality thumbnail finishes
        /// decoding and replaces the initial associated-icon placeholder.
        /// </summary>
        public event EventHandler IconThumbnailLoaded;

        /// <summary>
        /// Determines whether the given file extension can be decoded into a thumbnail.
        /// </summary>
        /// <param name="path">The file path to check.</param>
        /// <returns><see langword="true"/> if a thumbnail can be generated for the file; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        public bool IsSupported(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            return SupportedExtensions.Any(ext => path.EndsWith(ext));
        }

        /// <summary>
        /// Returns the cached thumbnail for <paramref name="path"/>, or the file's
        /// associated icon while the real thumbnail is generated in the background.
        /// </summary>
        /// <param name="path">The file path to generate a thumbnail for.</param>
        /// <returns>The current best available icon for the file.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        public Icon GenerateThumbnail(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            lock (cacheLock)
            {
                if (iconCache.TryGetValue(path, out var cached))
                    return cached.icon;
            }

            return SubmitGeneratorTask(path).icon;
        }

        private ThumbnailState SubmitGeneratorTask(string path)
        {
            var state = new ThumbnailState { icon = Icon.ExtractAssociatedIcon(path) };

            lock (cacheLock)
            {
                iconCache[path] = state;
            }

            Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    using (var ms = new MemoryStream(File.ReadAllBytes(path)))
                    using (var img = Image.FromStream(ms))
                    using (var thumb = (Bitmap)img.GetThumbnailImage(32, 32, () => false, IntPtr.Zero))
                    {
                        var icon = Icon.FromHandle(thumb.GetHicon());
                        state.icon = icon;
                        IconThumbnailLoaded?.Invoke(this, EventArgs.Empty);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            return state;
        }
    }
}
