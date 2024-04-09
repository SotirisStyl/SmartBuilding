using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding
{
    public interface IDoorManager
    {
        bool OpenDoor(int doorID);
        bool LockDoor(int doorID);
        bool OpenAllDoors();
        bool LockAllDoors();
        string GetStatus();
    }
}