using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OsuDataProvider
{
    public delegate void SongChangeDelegate(object sender, SongSelectionChangedEventArgs args);

    public class SongSelectionChangedEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public string MapCreator { get; set; }
        public string DifficultyName { get; set; }
        public string PathToBackgroundImage { get; set; }
    }

    public interface ISongChangeBroadcastSeverice: IDisposable
    {
        event SongChangeDelegate OnSongChanged;
    }
}
