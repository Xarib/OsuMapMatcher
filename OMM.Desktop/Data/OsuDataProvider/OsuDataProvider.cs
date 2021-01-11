using Microsoft.Extensions.Hosting;
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
        private readonly OsuMemoryReader reader;
        private const string path = "S:/osu!/Songs";

        public OsuDataProvider()
        {
            this.reader = new OsuMemoryReader();
        }

        public void Dispose()
        {
            this.timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            this.timer = new Timer(this.ReadData, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(250));

            //TODO update this
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Song folder not found!");
            }

            return Task.CompletedTask;
        }

        private void ReadData(object state)
        {
            if (this.reader.GetCurrentStatus(out int _) != OsuMemoryStatus.SongSelect || !reader.MapIdHasChanged(out int currentId))
                return;

            var folderName = this.reader.GetMapFolderName();
            var songPath = path + "/" + folderName;
            var fileName = this.reader.GetOsuFileName();
            using (StreamReader sr = new StreamReader(songPath + "/" + fileName))
            {
                if (!sr.TryReadLineStartingWith("0,0,", out var line))
                {
                    Console.WriteLine("No image");
                    return;
                }
                //line = line.Trim(new char[] { '0', ',', '"' });
                line = Regex.Replace(line, "^(-?\\d+,){0,2}\"|\"(,-?\\d+){0,2}$", "");
                Console.WriteLine($"{songPath}/{line}");
                
                var args = new SongSelectionChangedEventArgs
                {
                    PathToBackgroundImage = "\"Songs/" + folderName + "/" + line + "\"",
                };

                this.OnSongSelectionChanged(args);
            }
        }

        protected virtual void OnSongSelectionChanged(SongSelectionChangedEventArgs e)
        {
            var eventHandler = SongChanged;
            if (eventHandler != null)
            {
                eventHandler(this, e);
            }
        }

        public static event EventHandler<SongSelectionChangedEventArgs> SongChanged;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.timer?.Change(Timeout.Infinite, 0);

            this.reader.Dispose();

            return Task.CompletedTask;
        }
    }
}
