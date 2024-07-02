using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AloneSkylandLauncher.Controller
{
    public class ProfileManager
    {
        public static string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".AloneSkyland\\profiles\\");
        public static List<string> profiles = new List<string>();
        ComboBox profBox;

        public ProfileManager(ComboBox comboBox) {
            this.profBox = comboBox;
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
                addProfile("Гость");
            }
            else if (Directory.GetDirectories(dataPath).Length == 0)
            {
                addProfile("Гость");
            }
        }
        public void addProfile(string profile, bool updateCombo=true)
        {
            if (profiles.Contains(profile)) return;
            profiles.Add(profile);
            if (updateCombo)
            {
                profBox.Items.Add(profile);
                profBox.SelectedIndex = 0;
            }
            initProfiles();
        }
        public void deleteProfile(string profile, bool updateCombo=true)
        {
            if (!profiles.Contains(profile)) return;
            profiles.Remove(profile);
            if(updateCombo)
                profBox.Items.Remove(profile);
            Directory.Delete(dataPath+profile, true);
        }
        public void initProfiles()
        {
            foreach (var profile in profiles)
            {
                if (!Directory.Exists(dataPath + profile))
                {
                    Directory.CreateDirectory(dataPath + profile);
                }
            }
        }
    }
}
