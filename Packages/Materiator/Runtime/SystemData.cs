using UnityEngine;

namespace Materiator
{
    public class SystemData
    {
        public static MateriatorSettings Settings = LoadSettings();
        public static MateriaTags MateriaTags = Settings.MateriaTags;

        [SerializeField] private static MateriatorSettings _settings { get; set; }

        public static MateriatorSettings LoadSettings()
        {
            if (_settings == null)
                _settings = Resources.Load<MateriatorSettings>("MateriatorSettings");

            return _settings;
        }
    }
}