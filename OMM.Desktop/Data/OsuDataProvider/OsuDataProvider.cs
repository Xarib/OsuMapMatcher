using Microsoft.Extensions.Hosting;
using OMM.Desktop.Data.Settings;
using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OsuDataProvider
{
    public class OsuDataProvider : IHostedService, IDisposable
    {
        private Timer timer;
        private readonly IOsuMemoryReader reader;
        private readonly ISettings settings;
        private readonly IOsuDataService osuDataService;

        public OsuDataProvider(ISettings settings, IOsuDataService osuDataService)
        {
            this.settings = settings;
            this.osuDataService = osuDataService;
            this.reader = OsuMemoryReader.Instance;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = new Timer(this.ReadData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));

            return Task.CompletedTask;
        }

        private void ReadData(object state)
        {
            int currentId;
            if (this.reader.GetCurrentStatus(out int _) != OsuMemoryStatus.SongSelect || (currentId = this.reader.GetMapId()) == osuDataService.OldId)
                return;

            osuDataService.OldId = currentId;

            var path = settings.UserSettings.SongFolderPath;
            if (!path.EndsWith("Songs", StringComparison.OrdinalIgnoreCase) || !Directory.Exists(path))
            {
                Console.WriteLine("Song folder not found!");
                osuDataService.OnSongSelectionChanged(new List<string> { "Song folder not found! Go to settings and set the correct path." });
                return;
            }

            var folderName = this.reader.GetMapFolderName();
            var songPath = path + "/" + folderName;
            var fileName = this.reader.GetOsuFileName();

            if (string.IsNullOrWhiteSpace(fileName))
            {
                Console.WriteLine("Song folder not found");
                return;
            }

            using (StreamReader sr = new StreamReader(songPath + "/" + fileName))
            {
                var keyValuePair = new Dictionary<string, string>();
                if (sr.TryReadLineStartingWith("[Metadata]", out var line))
                {
                    while (!string.IsNullOrWhiteSpace(line = sr.ReadLine()))
                    {
                        var pair = line.Split(':');
                        keyValuePair.Add(pair[0], pair[1]);
                    }
                }

                if (!sr.TryReadLineStartingWith("0,0,", out line))
                {
                    Console.WriteLine("Mapper borked the osu file");
                    return;
                }
                line = Regex.Replace(line, "^(-?\\d+,){0,2}\"|\"(,-?\\d+){0,2}$", "");
                Console.WriteLine($"{songPath}/{line}");

                osuDataService.OnSongSelectionChanged(new SongSelectionChangedEventArgs
                {
                    BeatmapId = currentId,
                    PathToBackgroundImage = "\"Songs/" + folderName + "/" + line + "\"",
                    Artist = keyValuePair.GetValueOrDefault("Artist"),
                    ArtistUnicode = keyValuePair.GetValueOrDefault("ArtistUnicode"),
                    DifficultyName = keyValuePair.GetValueOrDefault("Version"),
                    MapCreator = keyValuePair.GetValueOrDefault("Creator"),
                    Title = keyValuePair.GetValueOrDefault("Title"),
                    TitleUnicode = keyValuePair.GetValueOrDefault("TitleUnicode"),
                });
            }
        }

        public void Dispose()
        {
            this.timer?.Dispose();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
