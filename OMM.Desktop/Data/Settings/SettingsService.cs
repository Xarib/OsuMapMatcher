using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace OMM.Desktop.Data.Settings
{
    public interface ISettings
    {
        public UserSettings UserSettings { get; }
        public void Save();
    }

    public class SettingsService : ISettings
    {
        public SettingsService()
        {

            if (!File.Exists("settings.xml"))
            {
                var songFolderPath = "";
                var registry_key = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                try
                {

#pragma warning disable CA1416 // Validate platform compatibility; Justification: Program is currently windows only
                    using var key = Registry.LocalMachine.OpenSubKey(registry_key);
                    foreach (string subkey_name in key.GetSubKeyNames())
                    {
                        using var subkey = key.OpenSubKey(subkey_name);
                        if (subkey.GetValue("DisplayName") is null || subkey.GetValue("DisplayName").ToString() != "osu!")
                            continue;

                        songFolderPath = subkey.GetValue("DisplayIcon").ToString().Replace("osu!.exe", "Songs").Replace(@"\", "/");
                    }
#pragma warning restore CA1416 // Validate platform compatibility
                }
                catch (Exception)
                {
                    Console.WriteLine("Info: The osu! installation was not found.\nYou have to set your osu! songs folder path manually.\nYou can find a guide here https://github.com/Xarib/OsuMapMatcher/wiki/First-usage");
                    songFolderPath = @"C:\";
                }

                this.UserSettings = new UserSettings
                {
                    ExitOnAllTabsClosed = false,
                    PrefersUnicode = false,
                    Volume = 25,
                    SongFolderPath = songFolderPath,
                    HideResultWithSameBeatmapId = false,
                };
                this.Save();
            }

            this.UserSettings = this.Read();
        }

        public UserSettings UserSettings { get; }

        private UserSettings Read()
        {
            using StreamReader sw = new StreamReader("settings.xml");

            XmlSerializer xmls = new XmlSerializer(typeof(UserSettings));
            return xmls.Deserialize(sw) as UserSettings;
        }

        public void Save()
        {
            using StreamWriter sw = new StreamWriter("settings.xml");

            XmlSerializer xmls = new XmlSerializer(typeof(UserSettings));
            xmls.Serialize(sw, this.UserSettings);
        }
    }
}
