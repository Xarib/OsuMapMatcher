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
        public int? BeatmapSetId { get; set; }
        public double Bpm { get; set; }
        public double BpmMax { get; set; }
        public double CS { get; set; }
        public string DifficultyName { get; set; }
        public double HP { get; set; }
        public double KDistance { get; set; }
        public double Length { get; set; }
        public string Mapper { get; set; }
        public double OD { get; set; }
        public string Title { get; set; }
        public string TitleUnicode { get; set; }
        public int TotalHitCircles { get; set; }
        public int TotalSliders { get; set; }
        public int TotalSpinners { get; set; }

        public string ImageLink { get; set; }
        public string OsuDirectLink { get; set; }
        public string MapLink { get; set; }
        public string TrackPreview { get; set; }
    }
}
