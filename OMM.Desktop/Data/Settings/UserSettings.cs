using System;
using System.ComponentModel.DataAnnotations;

namespace OMM.Desktop.Data.Settings
{
    public class UserSettings
    {
        [Required]
        [Range(typeof(bool), "true", "false",
        ErrorMessage = "You can have it on or off")]
        public bool PrefersUnicode { get; set; }
        [Required]
        [Range(typeof(bool), "true", "false",
        ErrorMessage = "You can have it on or off")]
        public bool ExitOnAllTabsClosed { get; set; }
        [Required]
        public string SongFolderPath { get; set; }
    }
}
