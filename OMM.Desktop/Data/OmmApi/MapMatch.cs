using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OMM.Desktop.Data.OmmApi
{
    public class MapMatch
    {
        public double AR { get; set; }
        public string Artist { get; set; }
        public string ArtistUnicode { get; set; }
        public int BeatmapId { get; set; }
        public int BeatmapsetId { get; set; }
        public int HitcircleCount { get; set; }
        public int SliderCount { get; set; }
        public int SpinnerCount { get; set; }
        public string Creator { get; set; }
        public double CS { get; set; }
        public double HP { get; set; }
        public double KDistance { get; set; }
        public double OD { get; set; }
        public RankStatus RankStatus { get; set; }
        public string CoverUrl { get; set; }
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public double Length { get; set; }
        public double Bpm { get; set; }
        public string DifficultyName { get; set; }
    }

    public enum RankStatus
    {
        Graveyard = -2,
        Wip = -1,
        Pending = 0,
        Ranked = 1,
        Approved = 2,
        Qualified = 3,
        Loved = 4,
    }
}
