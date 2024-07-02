using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AloneSkylandLauncher.Model
{
    [Serializable]
    public class LauncherData
    {
        public string lastVersion;
        public string lastProfile;
        public List<string> profiles;

        public static LauncherData Load(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(LauncherData));
                    using (FileStream stream = new FileStream(path, FileMode.Open))
                    {
                        LauncherData data = (LauncherData)serializer.Deserialize(stream);
                        return data;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public void Save(string path)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LauncherData));
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, this);
                }
                Console.WriteLine("Данные успешно сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
            }
        }
        public static void Save(string path, LauncherData data)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(LauncherData));
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    serializer.Serialize(stream, data);
                }
                Console.WriteLine("Данные успешно сохранены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении: {ex.Message}");
            }
        }
    }
}
