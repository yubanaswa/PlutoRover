using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoRover;
using System.Threading.Tasks;
using NMock;

namespace PlutoRoverApi.Controllers.Tests
{
    [TestClass()]
    public class RoverUnitTests
    {
        private MockFactory _factory = new MockFactory();

        [TestCleanup]
        public void Cleanup()
        {
            _factory.ClearExpectations();
        }

        [TestMethod()]
        public async Task RoverObject_UnitTest_GetPosition()
        {
            var mockRover = _factory.CreateMock<IRover>();
            mockRover.Expects.One.Method(p => p.GetPosition()).WillReturn(
                Task.Run(() => new RoverPosition() { Direction = 'N', Xcord = 0, Ycord = 0 })
                );

            var roverPosition = await mockRover.MockObject.GetPosition();
            Assert.AreEqual(roverPosition.Direction, 'N');
            Assert.AreEqual(roverPosition.Xcord, 0);
            Assert.AreEqual(roverPosition.Ycord, 0);
        }
                
        [TestMethod()]
        public async Task RoverObject_UnitTest_SetPosition()
        {
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.SetPosition(new RoverPosition() { Direction = 'N', Xcord = 0, Ycord = 0 })).
                Will( /*basically do nothing*/)
            );            
        }

        [TestMethod()]
        public async Task RoverObject_UnitTest_Move_Right()
        {
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.Move('R')).
                Will( /*basically do nothing*/)
            );
        }

        [TestMethod()]
        public async Task RoverObject_UnitTest_Move_Left()
        {
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.Move('L')).
                Will( /*basically do nothing*/)
            );
        }

        [TestMethod()]
        public async Task RoverObject_UnitTest_Move_Forward()
        {
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.Move('F')).
                Will( /*basically do nothing*/)
            );

            var commandController = new CommandController();
            await commandController.Post("FFFF", mockRover.MockObject);
        }

        [TestMethod()]
        public async Task RoverObject_UnitTest_Move_Backward()
        {
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.Move('B')).
                Will( /*basically do nothing*/)
            );

            var commandController = new CommandController();
            await commandController.Post("BBB", mockRover.MockObject); 
        }

        [TestMethod()]
        public async Task RoverApi_UnitTest_Test_Junk_Commands()
        {
            var commandController = new CommandController();
            var mockRover = _factory.CreateMock<IRover>();
            await Task.Run(() =>
                mockRover.Expects.One.Method(p => p.Move('B')).
                Will( /*basically do nothing*/)
            );
            
            await commandController.Post("ZYZDDD%0919$$", mockRover.MockObject); // entire command is garbage            
        }        
    }
}
