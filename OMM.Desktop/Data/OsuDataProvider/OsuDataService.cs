using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OsuDataProvider
{
    public interface IOsuDataService
    {
        public event EventHandler<Either<SongSelectionChangedEventArgs, List<string>>> SongChanged;
        public void OnSongSelectionChanged(Either<SongSelectionChangedEventArgs, List<string>> args);
        public int OldId { get; set; }
    }

    public class OsuDataService : IOsuDataService
    {
        public event EventHandler<Either<SongSelectionChangedEventArgs, List<string>>> SongChanged;

        public int OldId { get; set; } = 0;

        public void OnSongSelectionChanged(Either<SongSelectionChangedEventArgs, List<string>> args)
        {
            var eventHandler = SongChanged;
            if (eventHandler != null)
            {
                eventHandler(this, args);
            }
        }
    }
}
