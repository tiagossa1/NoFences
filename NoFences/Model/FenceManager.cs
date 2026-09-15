using System;
using System.IO;
using System.Xml.Serialization;

namespace NoFences.Model
{
    /// <summary>
    /// Manages the lifecycle and on-disk persistence of all fences, including
    /// loading previously saved fences on startup and creating, updating, or
    /// removing fences at runtime.
    /// </summary>
    public class FenceManager
    {
        /// <summary>
        /// Gets the single shared <see cref="FenceManager"/> instance for the application.
        /// </summary>
        public static FenceManager Instance { get; } = new FenceManager();

        private const string MetaFileName = "__fence_metadata.xml";

        private readonly string basePath;

        private FenceManager()
        {
            basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "NoFences");
            EnsureDirectoryExists(basePath);
        }

        /// <summary>
        /// Loads and displays every fence previously persisted under the application's
        /// local application data folder. Fences whose metadata cannot be read are skipped.
        /// </summary>
        public void LoadFences()
        {
            foreach (var dir in Directory.EnumerateDirectories(basePath))
            {
                var metaFile = Path.Combine(dir, MetaFileName);

                FenceInfo fence;
                try
                {
                    var serializer = new XmlSerializer(typeof(FenceInfo));
                    using (var reader = new StreamReader(metaFile))
                    {
                        fence = serializer.Deserialize(reader) as FenceInfo;
                    }
                }
                catch (Exception ex) when (ex is IOException || ex is InvalidOperationException || ex is UnauthorizedAccessException)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to load fence metadata from '{metaFile}': {ex}");
                    continue;
                }

                if (fence is null)
                    continue;

                new FenceWindow(fence).Show();
            }
        }

        /// <summary>
        /// Creates a new fence with the given <paramref name="name"/>, persists it, and shows it.
        /// </summary>
        /// <param name="name">The display name of the new fence.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is <see langword="null"/>.</exception>
        public void CreateFence(string name)
        {
            if (name is null)
                throw new ArgumentNullException(nameof(name));

            var fenceInfo = new FenceInfo(Guid.NewGuid())
            {
                Name = name,
                PosX = 100,
                PosY = 250,
                Height = 300,
                Width = 300
            };

            UpdateFence(fenceInfo);
            new FenceWindow(fenceInfo).Show();
        }

        /// <summary>
        /// Permanently deletes the persisted data for the given fence.
        /// </summary>
        /// <param name="info">The fence to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is <see langword="null"/>.</exception>
        public void RemoveFence(FenceInfo info)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));

            Directory.Delete(GetFolderPath(info), true);
        }

        /// <summary>
        /// Persists the current state of the given fence to disk.
        /// </summary>
        /// <param name="fenceInfo">The fence whose state should be saved.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="fenceInfo"/> is <see langword="null"/>.</exception>
        public void UpdateFence(FenceInfo fenceInfo)
        {
            if (fenceInfo is null)
                throw new ArgumentNullException(nameof(fenceInfo));

            var path = GetFolderPath(fenceInfo);
            EnsureDirectoryExists(path);

            var metaFile = Path.Combine(path, MetaFileName);
            var serializer = new XmlSerializer(typeof(FenceInfo));
            using (var writer = new StreamWriter(metaFile))
            {
                serializer.Serialize(writer, fenceInfo);
            }
        }

        private void EnsureDirectoryExists(string dir)
        {
            var di = new DirectoryInfo(dir);
            if (!di.Exists)
                di.Create();
        }

        private string GetFolderPath(FenceInfo fenceInfo)
        {
            return Path.Combine(basePath, fenceInfo.Id.ToString());
        }
    }
}
