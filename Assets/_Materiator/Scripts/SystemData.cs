using UnityEngine;

namespace Materiator
{
    public class SystemData
    {
        public static MateriatorSettings Settings = LoadSettings();
        public static MateriaTags MateriaTags = Settings.MateriaTags;

        public static MateriatorSettings LoadSettings()
        {
            return Resources.Load<MateriatorSettings>("MateriatorSettings");
        }
    }
}