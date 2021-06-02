using System;
using System.ComponentModel.DataAnnotations;

namespace OMM.Desktop.Data.Settings
{
    public class UserSettings
    {
        [Required]
        public bool PrefersUnicode { get; set; }
        [Required]
        public bool ExitOnAllTabsClosed { get; set; }
        [Required]
        public string SongFolderPath { get; set; }
        [Required]
        [Range(0, 100, ErrorMessage = "Invalid Volume (0-100)")]
        public int Volume { get; set; }
        [Required]
        public bool HideResultWithSameBeatmapId { get; set; }
    }
}
