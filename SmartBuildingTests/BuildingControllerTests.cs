using NUnit.Framework;
using NUnit.Framework.Legacy;
using SmartBuilding;
using NSubstitute;
using System.Security.Cryptography.X509Certificates;
using NUnit.Framework.Internal;
using System.Security.Claims;
using NSubstitute.ReceivedExtensions;


namespace SmartBuildingTests
{
    [TestFixture]
    public class BuildingControllerTests
    {
        private BuildingController MakeBuildingController()
        {
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            bc = new BuildingController("c&t1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("out of hours");
            return bc;
        }

        [Test] //L1R1 : L1R2
        public void L1R1_L1R2_Constructor_buildingID_Set()
        {
            //arrange
            BuildingController bc;
            //act
            bc = MakeBuildingController();
            //assert
            ClassicAssert.AreEqual("c&t1", bc.GetBuildingID());
        }

        [Test] //L1R3
        public void L1R3_Contstructor_BuildingIDSet_UpperToLower()
        {
            //arrange
            BuildingController bc;
            //act
            bc = new BuildingController("HelloWorld");
            //assert
            ClassicAssert.AreEqual("helloworld", bc.GetBuildingID());
        }

        [Test] //L1R2
        public void L1R2_GetBuildingIDCalled_ReturnsBuildingID()
        {
            //arrange
            BuildingController bc = new BuildingController("helloWorld"); ;
            //act
            bc.SetBuildingID("toaster");
            string buildingID = bc.GetBuildingID();
            //assert
            ClassicAssert.AreEqual("toaster", bc.GetBuildingID());
        }

        [Test] //L1R4
        public void L1R4_SetBuildingID_ConvertsUpperToLower()
        {
            //arrange
            BuildingController bc = new BuildingController("helloWorld"); ;
            //act
            bc.SetBuildingID("TOASTER");
            string buildingID = bc.GetBuildingID();
            //assert
            ClassicAssert.AreEqual("toaster", bc.GetBuildingID());
        }

        [Test] //L1R5
        public void L1R5_Constructor_currentStateReturns_OutOfHours()
        {
            //arrange
            BuildingController bc;
            //act
            bc = new BuildingController("helloWorld");
            //assert
            ClassicAssert.AreEqual("out of hours", bc.GetCurrentState());
        }

        [TestCase("open")]
        [TestCase("closed")]
        [TestCase("out of hours")]
        [TestCase("fire drill")]
        [TestCase("fire alarm")]
        //[TestCase("Fire Alarm")]
        //[TestCase("OPEN")]//L1R7
        public void L1R7_SetCurrentState_ValidInput_ReturnsTrue(string input)
        {
            //arrange
            BuildingController bc = MakeBuildingController();

            //act
            bool didSucceed = bc.SetCurrentState(input);
            //act/assert
            ClassicAssert.IsTrue(didSucceed);
        }

        [TestCase("open")]
        [TestCase("closed")]
        [TestCase("out of hours")]
        [TestCase("fire drill")]
        [TestCase("fire alarm")]
        //[TestCase("Fire Alarm")]
        //[TestCase("OPEN")]//L1R7 - L1R6
        public void L1R6_L1R7_SetCurrentState_ValidInput_SetsState(string input)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(input);
            string stateFromBC = bc.GetCurrentState();
            //act/assert
            ClassicAssert.AreEqual(input.ToLower(), stateFromBC);
        }

        [TestCase("$%^(($£")]
        [TestCase("ope")]
        [TestCase("12345")]
        [TestCase("")]
        [TestCase("openSesame")] //L1R7
        public void L1R7_SetCurrentState_InvalidInput_ReturnsFalse(string input)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bool didSucceed = bc.SetCurrentState(input);
            //act/assert
            ClassicAssert.IsFalse(didSucceed);
        }

        [TestCase("$%^(($£")]
        [TestCase("ope")]
        [TestCase("12345")]
        [TestCase("")]
        [TestCase("openSesame")] //L1R7
        public void L1R7_SetCurrentState_InvalidInput_DoesntSetState(string input)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(input);
            string stateFromBC = bc.GetCurrentState();
            //act/assert
            ClassicAssert.AreEqual("out of hours", stateFromBC);
        }

        [TestCase("open")]
        [TestCase("closed")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")] //L2R1
        public void L2R1_SetCurrentState_ValidChangeFromOOH_ReturnsTrue(string state)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bool didChange = bc.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(didChange);
        }

        [TestCase("out of hours")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")] //L2R1
        public void L2R1_SetCurrentState_ValidChangeFromOpen_ReturnsTrue(string state)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState("open");
            bool didChange = bc.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(didChange);
        }

        [TestCase("out of hours")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")] //L2R1
        public void L2R1_SetCurrentState_ValidChangeFromClosed_ReturnsTrue(string state)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState("closed");
            bool didChange = bc.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(didChange);
        }

        [TestCase("out of hours")]
        [TestCase("closed")]
        [TestCase("open")] //L2R1
        public void L2R1_SetCurrentState_ValidChangeFromFA_ReturnsTrue(string lastState)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(lastState);
            bc.SetCurrentState("fire alarm");
            bool didChange = bc.SetCurrentState(lastState);
            //assert
            ClassicAssert.IsTrue(didChange);
        }

        [TestCase("out of hours", "closed")]
        [TestCase("out of hours", "open")]
        [TestCase("closed", "out of hours")]
        [TestCase("closed", "open")]
        [TestCase("open", "closed")]
        [TestCase("open", "out of hours")]
        public void L1R7_L2R1_SetCurrentState_InvalidChangeFromFA_ReturnsFalse(string lastState, string invalidState)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(lastState);
            bc.SetCurrentState("fire alarm");
            bool didChange = bc.SetCurrentState(invalidState);
            //assert
            ClassicAssert.IsFalse(didChange);
        }

        //fire drill state tests 
        [TestCase("out of hours")]
        [TestCase("closed")]
        [TestCase("open")] //L2R1
        public void L2R1_SetCurrentState_ValidChangeFromFD_ReturnsTrue(string lastState)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(lastState);
            bc.SetCurrentState("fire drill");
            bool didChange = bc.SetCurrentState(lastState);
            //assert
            ClassicAssert.IsTrue(didChange);
        }

        [TestCase("out of hours", "closed")]
        [TestCase("out of hours", "open")]
        [TestCase("closed", "out of hours")]
        [TestCase("closed", "open")]
        [TestCase("open", "closed")]
        [TestCase("open", "out of hours")]
        public void L1R7_L2R1_SetCurrentState_InvalidChangeFromFD_ReturnsFalse(string lastState, string invalidState)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(lastState);
            bc.SetCurrentState("fire drill");
            bool didChange = bc.SetCurrentState(invalidState);
            //assert
            ClassicAssert.IsFalse(didChange);
        }

        //Testing requirement L2R2
        [TestCase("out of hours")]
        [TestCase("open")]
        [TestCase("closed")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")]
        public void L2R2_SetCurrentState_ToSameState_ReturnsTrue(string state)
        {
            //arrange
            BuildingController bc = MakeBuildingController();
            //act
            bc.SetCurrentState(state);
            bool didReturnTrue = bc.SetCurrentState(state);
            //Assert
            ClassicAssert.IsTrue(didReturnTrue);
        }

        [TestCase("opokdsd")]
        [TestCase("")]
        [TestCase("openn")]
        [TestCase("open sesame a longer than average string")]
        [TestCase("openn")]
        [TestCase("out of hours/")] //L2R3 Tests
        public void L2R3_Constructor_InvalidState_ThrowsException(string state)
        {
            //Arrange
            BuildingController bc;

            // Act/Assert
            var ex = Assert.Catch<ArgumentException>(() => bc = new BuildingController("C&T1", state));
            StringAssert.Contains("Argument Exception: BuildingController can only be initialised to the following states 'open', 'closed', 'out of hours'", ex.Message);

        }

        [TestCase("open")]
        [TestCase("out of hours")]
        [TestCase("closed")]
        [TestCase("CLOSED")]
        [TestCase("oPeN")]
        [TestCase("OuT oF hOuRs")] //L2R3 Tests
        public void L2R3_Constructor_ValidState_SetsState(string state)
        {
            //Arrange
            BuildingController bc;

            //Act
            bc = new BuildingController("C&T1", state);
            String gotState = bc.GetCurrentState();

            //Assert
            ClassicAssert.AreEqual(state.ToLower(), gotState);
        }

        [Test] //L3R1 Tests
        public void L3R1_Constructor_Injects_FakeDependencies()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();

            //Act
            bc = new BuildingController("c&t1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string testID = bc.GetBuildingID();

            //Assert
            ClassicAssert.AreEqual(testID, "c&t1");
        }

        [TestCase("", "", "")]
        [TestCase("test", "test", "test")]
        [TestCase("Lights,OK,", "Doors,OK,OK,", "FireAlarm,OK,")] //L3R3 Tests
        [TestCase("Lights,OK,FAULT,FAULT,OK", "Doors,OK,OK,FAULT,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT,", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "FireAlarm,OK,")] //L3R3 Tests
        public void L3R3_GetStatusReport_WhenCalled_ReturnsValidAppendedString(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            ClassicAssert.AreEqual(lightStatus + doorStatus + alarmStatus, statusReport);
        }

        [TestCase("Lights,OK,", "Doors,OK,OK,,", "FireAlarm,OK,")] //L3R3 Tests
        [TestCase("Lights,OK,FAULT,FAULT,OK", "Doors,OK,OK,FAULT,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT,", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "FireAlarm,OK,")] //L3R3 Tests
        public void L3R3_GetStatusReport_WhenCalled_ContainsLightStatus(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            StringAssert.Contains("Lights", statusReport);
        }

        [TestCase("Lights,OK,", "Doors,OK,OK,,", "FireAlarm,OK,")] //L3R3 Tests
        [TestCase("Lights,OK,FAULT,FAULT,OK", "Doors,OK,OK,FAULT,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT,", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "FireAlarm,OK,")] //L3R3 Tests
        public void L3R3_GetStatusReport_WhenCalled_ContainsDoorStatus(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            StringAssert.Contains("Doors", statusReport);
        }

        [TestCase("Lights,OK,", "Doors,OK,OK,,", "FireAlarm,OK,")] //L3R3 Tests
        [TestCase("Lights,OK,FAULT,FAULT,OK", "Doors,OK,OK,FAULT,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT,", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "Doors,OK,OK,OK,OK,OK,OK,OK,OK,OK,OK,", "FireAlarm,OK,")] //L3R3 Tests
        public void L3R3_GetStatusReport_WhenCalled_ContainsFireStatus(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            StringAssert.Contains("FireAlarm", statusReport);
        }

        [Test] //L3R4 test
        public void L3R4_SetCurrentState_ToOpen_OpenAllDoorsReturnsFalse_ReturnsFalse()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();

            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.OpenAllDoors().Returns(false);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bool didFail = bc.SetCurrentState("open");

            //Assert
            ClassicAssert.IsFalse(didFail);
        }

        //L3R5
        [Test]
        public void L3R5_SetCurrentState_ToOpen_When_nOpenAllDoorsIsTrue_ReturnsTrue()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();

            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.OpenAllDoors().Returns(true);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bool didOpen = bc.SetCurrentState("open");

            //Assert
            ClassicAssert.IsTrue(didOpen);
            doorManager.Received().OpenAllDoors();
        }

        [Test] //L4R1 test
        public void L4R1_SetCurrentState_ToClosed_DoorsLockedLightsOff()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.OpenAllDoors().Returns(true);
            doorManager.LockAllDoors().Returns(true);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            bc = new BuildingController("c&t1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("out of hours");

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("closed");

            //Assert
            doorManager.Received().LockAllDoors();
            lightManager.Received().SetAllLights(false);
        }

        [Test] //L4R2 web log test
        public void L4R2_SetCurrentState_ToFireAlarm_LogFireAlarmCalled()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("fire alarm");

            //Assert
            webService.Received().LogFireAlarm("fire alarm");
        }

        [Test] //L4R2 doors test
        public void L4R2_SetCurrentState_ToFireAlarm_OpenAllDoorsCalled()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("fire alarm");

            //Assert
            doorManager.Received().OpenAllDoors();
        }

        [Test] //L4R2 alarm test
        public void L4R2_SetCurrentState_ToFireAlarm_SetAlarmAlarmCalled()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("fire alarm");

            //Assert
            fireAlarmManager.Received().SetAlarm(true);
        }

        [Test] //L4R2 lights test
        public void L4R2_SetCurrentState_ToFireAlarm_SetAllLightsCalled()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("fire alarm");

            //Assert
            lightManager.Received().SetAllLights(true);
        }

        [TestCase("Lights,FAULT,", "Doors,OK,", "FireAlarm,OK,")]
        [TestCase("Lights,OK,", "Doors,FAULT,", "FireAlarm,OK,")]
        [TestCase("Lights,OK,", "Doors,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,", "Doors,FAULT,", "FireAlarm,FAULT,")]  //L4R3 door engineer test
        public void L4R3_GetStatusReport_FaultDetected_CallsEngineer(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            webService.Received().LogEngineerRequired(Arg.Any<string>());
        }

        [TestCase("Lights,OK,", "Doors,FAULT,", "FireAlarm,OK,")]
        [TestCase("Lights,FAULT,", "Doors,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,", "Doors,OK,OK,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,", "Doors,FAULT,OK,FAULT,", "FireAlarm,OK,")]
        [TestCase("Lights,FAULT,", "Doors,OK,OK,FAULT,", "FireAlarm,OK,")]  //L4R3 door engineer test
        public void L4R3_GetStatusReport_FaultyDoors_CallsDoorsEngineer(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            webService.Received().LogEngineerRequired(Arg.Is<string>(x => (x.Contains("Doors"))));
        }

        [TestCase("Lights,FAULT,", "Doors,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,", "Doors,OK,", "FireAlarm,OK,")]
        [TestCase("Lights,OK,OK,OK,FAULT,", "Doors,OK,OK,FAULT,", "FireAlarm,OK,")]
        [TestCase("Lights,OK,FAULT,", "Doors,FAULT,OK,FAULT,", "FireAlarm,OK,")]
        [TestCase("Lights,FAULT,OK,OK,OK", "Doors,OK,OK,OK,", "FireAlarm,FAULT,")]  //L4R3 door engineer test
        public void L4R3_GetStatusReport_FaultyLights_CallsLightEngineer(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            webService.Received().LogEngineerRequired(Arg.Is<string>(x => (x.Contains("Lights"))));
        }

        [TestCase("Lights,OK,", "Doors,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,", "Doors,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,OK,FAULT,", "Doors,OK,OK,OK,", "FireAlarm,FAULT,")]
        [TestCase("Lights,OK,OK,", "Doors,FAULT,OK,FAULT,", "FireAlarm,FAULT,")]
        [TestCase("Lights,FAULT,OK,OK,OK", "Doors,OK,OK,OK,", "FireAlarm,FAULT,")]  //L4R3 door engineer test
        public void L4R3_GetStatusReport_FaultyAlarm_CallsAlarmEngineer(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            webService.Received().LogEngineerRequired(Arg.Is<string>(x => (x.Contains("FireAlarm"))));
        }

        [TestCase("Lights,OK,", "Doors,OK,", "FireAlarm,OK,")]
        [TestCase("Lights,OK,OK,OK,", "Doors,OK,OK,OK,", "FireAlarm,OK,OK,OK,")] //L4R3
        public void L4R3_GetStatusReport_NoFaultDetected_NoEngineerCalled(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            lightManager.GetStatus().Returns(lightStatus);
            IDoorManager doorManager = Substitute.For<IDoorManager>();
            doorManager.GetStatus().Returns(doorStatus);
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            fireAlarmManager.GetStatus().Returns(alarmStatus);

            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            string statusReport = bc.GetStatusReport();

            //Assert
            webService.DidNotReceive().LogEngineerRequired(Arg.Any<string>());
        }

        [Test] //L4R4 if exception thrown email sent
        public void L4R4_SetCurrentState_ToFireAndExceptionThrown_EmailSent()
        {
            //Arrange
            BuildingController bc;
            IWebService webService = Substitute.For<IWebService>();
            IEmailService emailService = Substitute.For<IEmailService>();
            ILightManager lightManager = Substitute.For<ILightManager>();
            IFireAlarmManager fireAlarmManager = Substitute.For<IFireAlarmManager>();
            IDoorManager doorManager = Substitute.For<IDoorManager>();


            webService.When(x => x.LogFireAlarm(Arg.Any<string>())).Do(x => { throw new Exception("fake exception thrown"); });


            //Act
            bc = new BuildingController("C&T1", lightManager, fireAlarmManager, doorManager, webService, emailService);
            bc.SetCurrentState("fire alarm");

            //Assert
            emailService.Received().SendMail("smartbuilding@uclan.ac.uk", "failed to log alarm", Arg.Is<string>(x => (x.Contains("fake exception thrown"))));
        }
        /*
        //Test case for L1R1 for when buildngId is assigned
        [TestCase("building1")]
        [TestCase("building2")]
        [TestCase("building3")]
        [TestCase("building4")]
        public void L1R1_Constructor_IfCalled_AssignBuildingId(string buildingId)
        {
            //Arrange
            BuildingController controller;
            //Act
            controller = new BuildingController(buildingId);
            //Assert
            ClassicAssert.AreEqual(buildingId, controller.GetBuildingID());
        }

        //Test case for L1R2 for when buildingId is assigned
        [TestCase("building1")]
        [TestCase("building2")]
        [TestCase("building3")]
        [TestCase("building4")]
        public void L1R2_Constructor_IfCalled_AssignBuildingId(string buildingId)
        {
            //Arrange
            BuildingController controller;
            //Act
            controller = new BuildingController(buildingId);
            //Assert
            ClassicAssert.AreEqual(buildingId, controller.GetBuildingID());
        }

        //Test case for L1R3 to check if the assigned buildingId is set from uppercase to lowercase
        [TestCase("building1")]
        [TestCase("bUilding2")]
        [TestCase("bUiLdInG3")]
        [TestCase("buIldiNg1")]
        public void L1R3_Constructor_Set_UperrToLower(string buildingId)
        {
            //Arrange
            BuildingController controller;
            //Act
            controller = new BuildingController(buildingId);
            //Assert
            ClassicAssert.AreEqual(buildingId.ToLower(), controller.GetBuildingID());
        }

        //Test case for L1R4 setting the buildingId and then checking if GetBuildingId gives you the same result
        [TestCase("building1")]
        [TestCase("bUilding2")]
        [TestCase("bUiLdInG3")]
        [TestCase("buIldiNg1")]
        public void L1R4_Constructor_Set_SetBuildingId(string buildingId)
        {
            //Arrange
            BuildingController controller = new BuildingController("InitialBuilding");
            //Act
            controller.SetBuildingID(buildingId);
            //Assert
            ClassicAssert.AreEqual(buildingId.ToLower(), controller.GetBuildingID());
        }

        //Test case for L1R5 for checking the initial state of the program
        [TestCase("building1")]
        [TestCase("bUilding2")]
        [TestCase("bUiLdInG3")]
        [TestCase("buIldiNg1")]
        public void L1R5_Constructor_Set_OutOfHours(string buildingId)
        {
            //Arrange
            BuildingController controller;
            //Act
            controller = new BuildingController(buildingId);
            //Assert
            ClassicAssert.AreEqual("out of hours", controller.GetCurrentState());

        }

        //Test case for L1R6 for checking if the returned intial state is correct
        [TestCase("building1")]
        [TestCase("bUilding2")]
        [TestCase("bUiLdInG3")]
        [TestCase("buIldiNg1")]
        public void L1R6_Constructor_Return_GetCurrentState(string buildingId)
        {
            //Arrange
            BuildingController controller = new BuildingController(buildingId);
            //Act
            string result = controller.GetCurrentState();
            //Assert
            ClassicAssert.AreEqual("out of hours", result);
        }

        //Test case for L1R7 for checking if the set initial state is incorrect
        [TestCase("invalid state")]
        [TestCase(" ")]
        [TestCase(" InValidd123 ")]
        [TestCase("1234")]
        public void L1R7_Costructor_SetCurrentState_ReturnsFalse(string state)
        {
            //Arrange
            BuildingController controller = new BuildingController("building123");
            //Act
            controller.SetCurrentState(state.ToLower());
            //Assert
            ClassicAssert.AreNotEqual(state.ToLower(), controller.GetCurrentState());
        }

        //Test case for L1R7 for checking if the set initial state is correct
        [TestCase("closed")]
        [TestCase("Out Of Hours")]
        [TestCase("opeN")]
        [TestCase("fire Drill")]
        public void L1R7_Costructor_SetCurrentState_ReturnsTrue(string state)
        {
            //Arrange
            BuildingController controller = new BuildingController("building123");
            //Act
            controller.SetCurrentState(state.ToLower());
            //Assert
            ClassicAssert.AreEqual(state.ToLower(), controller.GetCurrentState());
        }

        //Test case for L2R1 for when we try a valid transition from out of hours 
        [TestCase("open")]
        [TestCase("closed")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")]
        public void L2R1_SetCurrentState_ValidFromOutOfHours_ReturnsTrue(string state)
        {
            //ARRANGE
            BuildingController controller = new BuildingController("building");

            //ACT
            controller.SetCurrentState("out of hours");
            bool validChange = controller.SetCurrentState(state);

            //ASSERT
            ClassicAssert.IsTrue(validChange);
        }


        //Test case for L2R1 for when we try a valid transition from closed
        [TestCase("out of hours")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")]
        public void L2R1_SetCurrentState_ValidFromClosed_ReturnsTrue(string state)
        {
            //ARRANGE
            BuildingController controller = new BuildingController("building");

            //ACT
            controller.SetCurrentState("closed");
            bool validChange = controller.SetCurrentState(state);

            //ASSERT
            ClassicAssert.IsTrue(validChange);
        }

        //Test case for L2R1 for when we try a valid transition from open
        [TestCase("out of hours")]
        [TestCase("fire alarm")]
        [TestCase("fire drill")]
        public void L2R1_SetCurrentState_ValidFromOpen_ReturnsTrue(string state)
        {
            //ARRANGE
            BuildingController controller = new BuildingController("building");

            //ACT
            controller.SetCurrentState("open");
            bool validChange = controller.SetCurrentState(state);

            //ASSERT
            ClassicAssert.IsTrue(validChange);
        }

        //Test case for L2R1 for when we try a valid transition from fire drill
        [TestCase("out of hours", "out of hours")]
        [TestCase("closed", "closed")]
        [TestCase("open", "open")]
        public void L2R1_SetCurrentState_ValidFromFA_ReturnsTrue(string lastState, string validState)
        {
            //arrange
            BuildingController controller = new BuildingController("building");
            //act
            controller.SetCurrentState(lastState);
            controller.SetCurrentState("fire alarm");
            bool validChange = controller.SetCurrentState(validState);
            //assert
            ClassicAssert.IsTrue(validChange);
        }

        //Test case for L2R1 for when we try a valid transition from fire alarm
        [TestCase("out of hours", "out of hours")]
        [TestCase("closed", "closed")]
        [TestCase("open", "open")]
        public void L2R1_SetCurrentState_ValidFromFD_ReturnsTrue(string lastState, string validState)
        {
            //arrange
            BuildingController controller = new BuildingController("building");
            //act
            controller.SetCurrentState(lastState);
            controller.SetCurrentState("fire drill");
            bool validChange = controller.SetCurrentState(validState);
            //assert
            ClassicAssert.IsTrue(validChange);
        }

        //Test case for L2R1 for when we try an invalid transition from fire alarm
        [TestCase("out of hours", "closed")]
        [TestCase("closed", "open")]
        [TestCase("open", "closed")]
        public void L2R1_SetCurrentState_InvalidFromFA_ReturnsFalse(string lastState, string invalidState)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState(lastState);
            controller.SetCurrentState("fire alarm");
            bool validChange = controller.SetCurrentState(invalidState);
            //assert
            ClassicAssert.IsFalse(validChange);
        }

        //Test case for L2R1 for when we try an invalid transition from fire alarm
        [TestCase("out of hours", "closed")]
        [TestCase("closed", "open")]
        [TestCase("open", "closed")]
        public void L2R1_SetCurrentState_InvalidFromFireDrill_ReturnsFalse(string lastState, string invalidState)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState(lastState);
            controller.SetCurrentState("fire drill");
            bool validChange = controller.SetCurrentState(invalidState);
            //assert
            ClassicAssert.IsFalse(validChange);
        }

        //Test case for L2R2 for when we try a transition from one state to the same
        [TestCase("out of hours")]
        public void L2R2_SetCurrentState_SameStateOutOfHours_ReturnsTrue(string state)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState("out of hours");
            bool result = controller.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L2R2 for when we try a transition from one state to the same
        [TestCase("closed")]
        public void L2R2_SetCurrentState_SameStateClosed_ReturnsTrue(string state)
        {
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState("closed");
            bool result = controller.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L2R2 for when we try a transition from one state to the same
        [TestCase("open")]
        public void L2R2_SetCurrentState_SameStateOpen_ReturnsTrue(string state)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState("open");
            bool result = controller.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L2R2 for when we try a transition from one state to the same
        [TestCase("fire drill")]
        public void L2R2_SetCurrentState_SameStateFireDrill_ReturnsTrue(string state)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState("fire drill");
            bool result = controller.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L2R2 for when we try a transition from one state to the same
        [TestCase("fire alarm")]
        public void L2R2_SetCurrentState_SameStateFireAlarm_ReturnsTrue(string state)
        {
            //arrange
            BuildingController controller = new BuildingController("building123");
            //act
            controller.SetCurrentState("fire alarm");
            bool result = controller.SetCurrentState(state);
            //assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L2R3, when the new Constructor BuildingController("","") checks if the starting state is valid
        [TestCase("Building132", "open")]
        [TestCase("Building123", "closed")]
        [TestCase("BuilDing152", "out of hours")]
        public void L2R3_Constructor2_Check_ValidStartState(string id, string state)
        {
            //arrange
            BuildingController controller;
            //act
            controller = new BuildingController(id, state);
            //assert
            ClassicAssert.AreEqual(state.ToLower(), controller.GetCurrentState());
        }

        //Test case for L2R3, when the new Constructor BuildingController("","") checks if the starting state is invalid and throws exception
        [TestCase("Building132", "invalid")]
        [TestCase("Building123", "fire drill")]
        [TestCase("BuilDing152", "fire alarm")]
        [TestCase("BuilDing152", null)]
        public void L2R3_Constructor2_InValidStartState_ThrowException(string id, string state)
        {
            //arrange
            BuildingController controller;
            //act & assert
            var ex = Assert.Catch<ArgumentException>(() => controller = new BuildingController(id, state));
            StringAssert.Contains("Argument Exception: BuildingController can only be initialised to the following states 'open', 'closed', 'out of hours'", ex.Message);
        }

        //Test case for L3R1 Creating the 5 new dependencies and the new constructor BuildingController("", lm, fam, dm, ws, es)
        [Test]
        public void L3R1_Contruector3_AllowDependencyInjection_True()
        {
            //arrange
            BuildingController controller;
            string buildingId = "building1";
            ILightManager lm = Substitute.For<ILightManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            //act
            controller = new BuildingController(buildingId, lm, fam, dm, ws, es);
            //assert
            ClassicAssert.AreEqual(buildingId.ToLower(), controller.GetBuildingID());
        }

        //Test case for L3R3 when called returns a valid string based on combination "Ligts,..,Doors,...,FireAlarm"
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT", "FireAlarm,FAULT")]
        [TestCase("Lights,OK,FAULT,FAULT,FAULT,FAULT", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT", "FireAlarm,FAULT")]
        [TestCase("Lights,OK,FAULT,FAULT,FAULT,OK", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT", "FireAlarm,FAULT")]
        public void L3R3_GetStatusReport_IfCalled_ReturnsValidString(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();

            ILightManager lm = Substitute.For<ILightManager>();
            lm.GetStatus().Returns(lightStatus);

            IDoorManager dm = Substitute.For<IDoorManager>();
            dm.GetStatus().Returns(doorStatus);

            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            fam.GetStatus().Returns(alarmStatus);

            //Act
            controller = new BuildingController("buildingID", lm, fam, dm, ws, es);
            string report = controller.GetStatusReport();


            //Assert
            ClassicAssert.AreEqual(report, $"Lights,{lightStatus},Doors,{doorStatus},FireAlarm,{alarmStatus}");
        }

        //Test case for L3R4 when SetCurrentState moves to "open" state it returns false
        [Test]
        public void L3R4_SetCurrentState_DoorManagerOpenAllDoors_ReturnFalse()
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            ILightManager lm = Substitute.For<ILightManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            dm.OpenAllDoors().Returns(false);

            //act
            controller = new BuildingController("id", lm, fam, dm, ws, es);
            bool result = controller.SetCurrentState("open");

            //Assert
            ClassicAssert.IsFalse(result);
        }

        //Test case for L3R5 when SetCurrentState moves to "open" state it returns true and changes the previous state to current state
        [Test]
        public void L3R5_SetCurrentState_DoorManagerOpenAllDoors_ReturnTrue()
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            ILightManager lm = Substitute.For<ILightManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            dm.OpenAllDoors().Returns(true);

            //act
            controller = new BuildingController("id", lm, fam, dm, ws, es);
            bool result = controller.SetCurrentState("open");

            //Assert
            ClassicAssert.IsTrue(result);
        }

        //Test case for L4R1 when SetCurrentState moves to "closed" state lock all doors and turn off all lights
        [Test]
        public void L4R1_SetCurrentState_ClosedState_DoorsLockedAndLightsTurnedOff()
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            ILightManager lm = Substitute.For<ILightManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            //act
            controller = new BuildingController("id", lm, fam, dm, ws, es);
            bool result = controller.SetCurrentState("closed");

            //assert
            dm.Received().LockAllDoors();
            lm.Received().SetAllLights(false);
        }

        //Test case for L4R2 when SetCurrentState moves to "fire alarm" state:
        //set alarm to true, open all doors, open all lights and log fire alarm with the message "fire alarm"
        [Test]
        public void L4R2_SetCurrentState_FireAlarmState_SetAlarmOpenAllDoorsSetAllLightsLogFireAlarm()
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            ILightManager lm = Substitute.For<ILightManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            //act
            controller = new BuildingController("id", lm, fam, dm, ws, es);
            bool result = controller.SetCurrentState("fire alarm");

            //assert
            fam.Received().SetAlarm(true);
            dm.Received().OpenAllDoors();
            lm.Received().SetAllLights(true);
            ws.Received().LogFireAlarm("fire alarm");
        }

        //Test case for L4R3 check faulty devices
        [TestCase("Lights,FAULT,FAULT,FAULT,FAULT,FAULT", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT", "FireAlarm,FAULT")]
        [TestCase("Lights,OK,FAULT,FAULT,FAULT,FAULT", "Doors,FAULT,FAULT,FAULT,FAULT,FAULT,FAULT", "FireAlarm,FAULT")]
        [TestCase("Lights,OK,OK,FAULT,FAULT,OK", "Doors,OK,OK,OK,OK,OK,FAULT", "FireAlarm,OK")]
        public void L4R3_GetStatusReport_IfCalled_ReturnsValidString(string lightStatus, string doorStatus, string alarmStatus)
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();

            ILightManager lm = Substitute.For<ILightManager>();
            lm.GetStatus().Returns(lightStatus);

            IDoorManager dm = Substitute.For<IDoorManager>();
            dm.GetStatus().Returns(doorStatus);

            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();
            fam.GetStatus().Returns(alarmStatus);

            //Act
            controller = new BuildingController("buildingID", lm, fam, dm, ws, es);
            string report = controller.GetStatusReport();

            //Assert
            ws.Received().LogEngineerRequired(Arg.Any<string>());
        }

        //Test case for L4R4 send exception email when WebService.LogFireAlarm( ) is called
        [Test]
        public void L4R4_SetCurrentState_ToFireAlarmStateAndExceptionThrown_ShouldSendEmail()
        {
            //Arrange
            BuildingController controller;
            IWebService ws = Substitute.For<IWebService>();
            IEmailService es = Substitute.For<IEmailService>();
            ILightManager lm = Substitute.For<ILightManager>();
            IDoorManager dm = Substitute.For<IDoorManager>();
            IFireAlarmManager fam = Substitute.For<IFireAlarmManager>();

            // webService.ToThrow = new Exception("Exception thrown");
            ws.LogFireAlarm(Arg.Any<string>()).Returns(x => { throw new Exception("fake exception thrown"); });

            //Act
            controller = new BuildingController("buildingID", lm, fam, dm, ws, es);
            controller.SetCurrentState("fire alarm");

            //Assert
            es.Received().SendMail("smartbuilding@uclan.ac.uk", "failed to log alarm", Arg.Is<string>(x => (x.Contains("fake exception thrown"))));
        }*/
    }
}
