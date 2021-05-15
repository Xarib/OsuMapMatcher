using OMM.Desktop.Data.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop.Data
{
    public class SongFolderWatcher : IDisposable
    {
        public HashSet<int> DownloadedMaps { get; set; } = new HashSet<int>();
        public event EventHandler<string> SongsFolderContentChange;
        private readonly FileSystemWatcher watcher;

        public SongFolderWatcher(ISettings settings)
        {
            if (!settings.UserSettings.SongFolderPath.EndsWith("Songs") || !Directory.Exists(settings.UserSettings.SongFolderPath))
                return;

            foreach (var set in Directory.GetDirectories(settings.UserSettings.SongFolderPath))
            {
                var dir = new DirectoryInfo(set);
                var mapSetFolderName = dir.Name.Trim();

                if (!char.IsDigit(mapSetFolderName[0]))
                    continue; //Ignore non mapset folders

                if (!GetFolderMapSetId(mapSetFolderName, out int mapSetId))
                    continue;

                this.DownloadedMaps.Add(mapSetId);
            }

            this.watcher = new FileSystemWatcher(settings.UserSettings.SongFolderPath);

            this.watcher.Created += OnCreated;
            this.watcher.Deleted += OnDeleted;

            this.watcher.EnableRaisingEvents = true;

            this.watcher.NotifyFilter = NotifyFilters.DirectoryName;
        }

        public bool IsDownloaded(int? setId)
        {
            if (setId is null)
                return false;

            return this.DownloadedMaps.Contains(setId.Value);
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            if (SongsFolderContentChange is null)
                return;

            Console.WriteLine($"DownloadTracker-Info: Download detected {e.FullPath}");

            if (!GetFolderMapSetId(e.Name, out int mapSetId))
                return;

            this.DownloadedMaps.Add(mapSetId);

            this.SongsFolderContentChange.Invoke(sender, e.FullPath);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            if (SongsFolderContentChange is null)
                return;

            Console.WriteLine($"DownloadTracker-Info: Deletion detected {e.FullPath}");

            if (!GetFolderMapSetId(e.Name, out int mapSetId))
                return;

            this.DownloadedMaps.Remove(mapSetId);

            this.SongsFolderContentChange.Invoke(sender, e.FullPath);
        }

        private static bool GetFolderMapSetId(string folderName, out int id)
        {
            var indexOfSpace = folderName.IndexOf(' ');
            
            if (indexOfSpace < 0)
            {
                id = -1;
                Console.Write($"Info: Unusual foldername '{folderName}' found\n");
                return false;
            }

            if (int.TryParse(folderName.Substring(0, indexOfSpace), out int mapSetId))
            {
                id = mapSetId;
                return true;
            }

            id = -1;
            return false;
        }

        public void Dispose()
        {
            this.watcher.Created -= OnCreated;
            this.watcher.Deleted -= OnDeleted;
            this.watcher.Dispose();
        }
    }
}
