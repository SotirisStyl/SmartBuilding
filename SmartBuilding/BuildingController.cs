using System.Linq;
using System.Runtime.Intrinsics.X86;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using NSubstitute;
using NUnit.Framework;

namespace SmartBuilding
{
    public class BuildingController
    {
        //declaring the variables needed for BuildingController: id and variable for checking transitions
        private string buildingID;
        private string currentState;
        private string previousState;
        private string temp;

        //declaring the variables of the 5 dependencies
        private ILightManager lightManager;
        private IFireAlarmManager fireAlarmManager;
        private IDoorManager doorManager;
        private IWebService webService;
        private IEmailService emailService;

        //function to handle L1R1
        public BuildingController(string id)
        {
            buildingID = id.ToLower();
            currentState = "out of hours";
        }

        //Function to handle L2R3
        public BuildingController(string id, string startState)
        {
            string[] Valid_States = { "closed", "out of hours", "open" };
            //Throwing Exception for L2R3
            if (!Valid_States.Contains(startState))
            {
                throw new ArgumentException("Argument Exception: BuildingController can only be initialised to the following states 'open', 'closed', 'out of hours'");
            }
            //Else if the starting state is valid handle the buildingId and currentState
            else
            {   
                buildingID = id;
                currentState = startState;
            }
        }

        //function to handle L3R1
        public BuildingController(string id, ILightManager iLightManager,
            IFireAlarmManager iFireAlarmManager, IDoorManager iDoorManager,
            IWebService iWebService, IEmailService iEmailService)
        {
            buildingID = id.ToLower();
            currentState = "out of hours";
            lightManager = iLightManager;
            fireAlarmManager = iFireAlarmManager;
            doorManager = iDoorManager;
            webService = iWebService;
            emailService = iEmailService;

        }
        //function to handle L3R3
        public string GetStatusReport()
        {
            //declaring variables for GetStatus accordingly
            string ls = lightManager.GetStatus();
            string ds = doorManager.GetStatus();
            string fa = fireAlarmManager.GetStatus();

            //declaring error
            string error = "invalid";
            //declaring split variables to handle each of the variable's first word accordingly
            string[] lsSplit = ls.Split(',');
            string[] dsSplit = ds.Split(',');
            string[] faSplit = fa.Split(',');
            //declaring an empty word to create the string LogEngineerRequired recieves at L4R3
            string word = "";

            //checks for checking if the words provided at L3R3 and L4R3 tests are correct
            if (lsSplit[0] == "Lights" || dsSplit[0] == "Doors" || faSplit[0] == "FireAlarm")
            {
                string statusReport = $"Lights,{ls},Doors,{ds},FireAlarm,{fa}";

                //checks for creating the word needed for the L4R3
                if (lsSplit.Contains("FAULT"))
                    word += "Lights,";
                if (dsSplit.Contains("FAULT"))
                    word += "Doors,";
                if (faSplit.Contains("FAULT"))
                    word += "FireAlarm,";

                //handling the checks of L4R3 to call the LogEngineerRequired() function
                if (lsSplit.Contains("FAULT") || dsSplit.Contains("FAULT") || faSplit.Contains("FAULT"))
                {
                    webService.LogEngineerRequired(word);
                }

                //returning the report
                return statusReport;
            }
            //if the structure of the strings provided is wrong send an error
            return error;
        }
        //function to handle L1R2
        public string GetBuildingID()
        {
            return buildingID;
        }
        //function to handle L1R4
        public void SetBuildingID(string id)
        {
            buildingID = id.ToLower();
        }

        //function to handle L1R6
        public string GetCurrentState()
        {
            return currentState;
        }

        public bool SetCurrentState(string newState)
        {
            // string array for valid stages
            string[] validStates = { "closed", "out of hours", "open", "fire drill", "fire alarm" };
            //check if the provided state is one of the valid changes
            if (validStates.Contains(newState.ToLower()))
            {
                previousState = currentState.ToLower();
                currentState = newState.ToLower();
            }
            else
            {
                return false;
            }
            //check if the current state is same as the previous state for L2R2
            if (previousState == currentState)
            {
                return true;
            }
            //check for the "open" state for L3R5
            if (newState == "open" && doorManager != null && doorManager.OpenAllDoors())
            {
                previousState = currentState;
                return true;
            }
            //check for the "open" state for L3R4
            else if (newState == "open" && doorManager != null && !doorManager.OpenAllDoors())
            {
                return false;
            }
            //check for the "closed" state for L4R1
            if (newState == "closed" && doorManager != null && lightManager != null)
            {
                doorManager.LockAllDoors();
                lightManager.SetAllLights(false);
            }
            //check for the "fire alarm" state for L4R2
            try
            {
                if (newState == "fire alarm" && fireAlarmManager != null && doorManager != null && lightManager != null && webService != null)
                {
                    fireAlarmManager.SetAlarm(true);
                    doorManager.OpenAllDoors();
                    lightManager.SetAllLights(true);
                    webService.LogFireAlarm("fire alarm");
                }
            }
            //throw exception for L4R4
            catch (Exception exception)
            {
                emailService.SendMail("smartbuilding@uclan.ac.uk", "failed to log alarm", exception.Message);
            }
            
            //switch case to handle the valid transitions from previous state to current state
            switch (previousState)
            {
                case "out of hours":
                    if (currentState == "closed" || currentState == "open" || currentState == "fire drill" 
                        || currentState == "fire alarm")
                    {
                        temp = previousState;
                        previousState = currentState;
                        currentState = newState.ToLower();
                        return true;
                    }
                    break;
                case "closed":
                    if (currentState == "out of hours" || currentState == "fire drill" 
                        || currentState == "fire alarm")
                    {
                        temp = previousState;
                        previousState = currentState;
                        currentState = newState.ToLower();
                        return true;
                    }
                    break;
                case "open":
                    if (currentState == "out of hours" || currentState == "fire drill"
                        || currentState == "fire alarm")
                    {
                        temp = previousState;
                        previousState = currentState;
                        currentState = newState.ToLower();
                        return true;
                    }
                    
                    break;
                case "fire drill":
                    if (temp == currentState)
                    {
                        previousState = currentState;
                        currentState = newState.ToLower();
                        return true;
                    }
                    break;
                case "fire alarm":
                    if (temp == currentState)
                    {
                        previousState = currentState;
                        currentState = newState.ToLower();
                        return true;
                    }
                    break;
            }
            
            //if all cases fail return false
            return false;
        }

    }
}

