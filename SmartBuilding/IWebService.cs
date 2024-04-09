using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartBuilding
{
    public interface IWebService
    {
        string LogStateChange(string logDetails);
        string LogEngineerRequired(string logDetails);
        string LogFireAlarm(string logDetails);
    }
}
