using System.Collections.Generic;

namespace FrostyP_Game_Manager
{
    public class CameraPreset
    {
        public string name;
        public List<CameraSetting> settings;

        public CameraPreset(string _name, List<CameraSetting> _settings)
        {
            name = _name;
            settings = _settings;
        }
    }
}
