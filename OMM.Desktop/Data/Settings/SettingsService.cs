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
                var installationPath = "";
                var registry_key = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
                try
                {

                    using (var key = Registry.LocalMachine.OpenSubKey(registry_key))
                    {
                        foreach (string subkey_name in key.GetSubKeyNames())
                        {
                            using (var subkey = key.OpenSubKey(subkey_name))
                            {
                                if (subkey.GetValue("DisplayName") is null || subkey.GetValue("DisplayName").ToString() != "osu!")
                                    continue;

                                installationPath = subkey.GetValue("DisplayIcon").ToString().Replace("osu!.exe", @"Songs");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Error: The osu! installation was not found.\nYou have to set your osu! songs folder path manually.\nYou can find a guide here https://github.com/Xarib/OsuMapMatcher/wiki/First-usage");
                    installationPath = @"C:\";
                }

                this.UserSettings = new UserSettings
                {
                    ExitOnAllTabsClosed = false,
                    PrefersUnicode = false,
                    Volume = 25,
                    SongFolderPath = installationPath,
                    HideResultWithSameBeatmapId = false,
                };
                this.Save();
            }

            this.UserSettings = this.Read();
        }

        public UserSettings UserSettings { get; }

        private UserSettings Read()
        {
            using (StreamReader sw = new StreamReader("settings.xml"))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(UserSettings));
                return xmls.Deserialize(sw) as UserSettings;
            }
        }

        public void Save()
        {
            using (StreamWriter sw = new StreamWriter("settings.xml"))
            {
                XmlSerializer xmls = new XmlSerializer(typeof(UserSettings));
                xmls.Serialize(sw, this.UserSettings);
            }
        }
    }
}
