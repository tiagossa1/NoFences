using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using System.IO;
using NoFences.Win32;
using NoFences.Util;

namespace NoFences.Model
{
    /// <summary>
    /// Represents a single file or folder entry shown inside a fence.
    /// </summary>
    public class FenceEntry
    {
        /// <summary>
        /// Gets the full file-system path of the entry.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets whether the entry refers to a file or a folder.
        /// </summary>
        public EntryType Type { get; }

        /// <summary>
        /// Gets the display name of the entry, derived from its file name without extension.
        /// </summary>
        public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);

        private FenceEntry(string path, EntryType type)
        {
            Path = path;
            Type = type;
        }

        /// <summary>
        /// Creates a <see cref="FenceEntry"/> for the given path.
        /// </summary>
        /// <param name="path">The full file or folder path.</param>
        /// <returns>
        /// A new <see cref="FenceEntry"/> if <paramref name="path"/> points to an existing
        /// file or folder; otherwise <see langword="null"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <see langword="null"/>.</exception>
        public static FenceEntry FromPath(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            if (File.Exists(path))
                return new FenceEntry(path, EntryType.File);
            else if (Directory.Exists(path))
                return new FenceEntry(path, EntryType.Folder);
            else return null;
        }

        /// <summary>
        /// Extracts the icon or thumbnail representing this entry.
        /// </summary>
        /// <param name="thumbnailProvider">The provider used to generate file thumbnails.</param>
        /// <returns>The icon to display for this entry.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="thumbnailProvider"/> is <see langword="null"/>.</exception>
        public Icon ExtractIcon(ThumbnailProvider thumbnailProvider)
        {
            if (thumbnailProvider is null)
                throw new ArgumentNullException(nameof(thumbnailProvider));

            if (Type == EntryType.File)
            {
                if (thumbnailProvider.IsSupported(Path))
                    return thumbnailProvider.GenerateThumbnail(Path);
                else
                    return Icon.ExtractAssociatedIcon(Path);
            }
            else
            {
                return IconUtil.FolderLarge;
            }
        }

        /// <summary>
        /// Opens the entry asynchronously using the associated shell handler
        /// (the default application for files, or Windows Explorer for folders).
        /// Failures are logged rather than thrown, since this runs fire-and-forget
        /// from UI event handlers.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous open operation.</returns>
        public Task Open()
        {
            return Task.Run(() =>
            {
                try
                {
                    if (Type == EntryType.File)
                        Process.Start(Path);
                    else if (Type == EntryType.Folder)
                        Process.Start("explorer.exe", Path);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Failed to start: {e}");
                }
            });
        }
    }
}
