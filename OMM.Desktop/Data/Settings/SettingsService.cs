using System;
using System.Configuration;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

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
                this.UserSettings = new UserSettings
                {
                    ExitOnAllTabsClosed = false,
                    PrefersUnicode = false,
                    SongFolderPath = @"%localappdata%\osu!\Songs",
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
