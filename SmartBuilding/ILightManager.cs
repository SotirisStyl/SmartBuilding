using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding
{
    public interface ILightManager
    {
        void SetLight(bool isOn, int lightId);
        bool SetAllLights(bool isOn);
        string GetStatus();
    }
}