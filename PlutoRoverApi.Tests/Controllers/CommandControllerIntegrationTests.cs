using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlutoRover;
using System.Threading.Tasks;

namespace PlutoRoverApi.Controllers.Tests
{
    [TestClass()]
    public class CommandControllerIntegrationTests
    {
        private async Task SetPosition(char Direction, short x, short y, Rover rover)
        {
            var positionController = new PositionController();
            var position = new RoverPosition() { Direction = Direction, Xcord = x, Ycord = y };
            await positionController.SetPositionAsync(position, rover);
        }

        [TestMethod()]
        public async Task Integration_Test_Junk_Commands()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("ZYZDDD%0919$$", curiosity); // entire command is garbage

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 0);
            Assert.AreEqual(position.Ycord, 0);
        }

        [TestMethod()]
        public async Task Integration_Test_Junk_Commands_MoveBackward()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 10, 10, curiosity);
            await commandController.Post("ZYZDBDD%B0919$$", curiosity); // there are two B commands

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 10);
            Assert.AreEqual(position.Ycord, 8);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_From_N00_F_N0001()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("F", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 0);
            Assert.AreEqual(position.Ycord, 1);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_From_N00_FFRFF_E22()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("FFRFF", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'E');
            Assert.AreEqual(position.Xcord, 2);
            Assert.AreEqual(position.Ycord, 2);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_From_N0000_FFFFF_N0005()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("FFFFF", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 0);
            Assert.AreEqual(position.Ycord, 5);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_From_N0000_FFBBFB_N0000()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("FFBBFB", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 0);
            Assert.AreEqual(position.Ycord, 0);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_N00_BB_N0099()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("BB", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 0);
            Assert.AreEqual(position.Ycord, 99);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_N0000_FFFFFRFFFFFRFFFFF_S0500()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("FFFFFRFFFFFRFFFFF", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'S');
            Assert.AreEqual(position.Xcord, 5);
            Assert.AreEqual(position.Ycord, 0);
        }

        [TestMethod()]
        public async Task Integration_Test_MoveForward_N0000_FFRFFLFFRFFLFF_N0406()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            await SetPosition('N', 0, 0, curiosity);
            await commandController.Post("FFRFFLFFRFFLFF", curiosity);

            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 4);
            Assert.AreEqual(position.Ycord, 6);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 24, Y = 30 }, new PlutoObstacle() { X = 22, Y = 30 } };
            await curiosity.SetObstacles(obstacles); // given more tme this would have been in a class called Pluto 
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_MoveForward()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 24, Y = 30 }, new PlutoObstacle() { X = 22, Y = 30 } };
            await curiosity.SetObstacles(obstacles); // 22,30 is an obstacle

            await SetPosition('N', 22, 28, curiosity); // setting it up at 22,28
            await commandController.Post("FFFF", curiosity); // asking to move ahead 4 times 

            // 1 # rover should move to 22,29 and wait. 
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 22);
            Assert.AreEqual(position.Ycord, 29);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_MoveBackward()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 24, Y = 30 }, new PlutoObstacle() { X = 22, Y = 30 } };
            await curiosity.SetObstacles(obstacles); // [24,30] is an obstacle

            await SetPosition('N', 24, 32, curiosity); // setting it up at 24,32
            await commandController.Post("BBBB", curiosity); // asking to move back 4 times 

            // 1 # rover should move to 24,31 and wait. 
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 24);
            Assert.AreEqual(position.Ycord, 31);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_CheckObstacleMet_MoveForward()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 24, Y = 30 }, new PlutoObstacle() { X = 22, Y = 30 } };
            await curiosity.SetObstacles(obstacles); // 22,30 is an obstacle

            await SetPosition('N', 22, 28, curiosity); // setting it up at 22,28
            await commandController.Post("FFFF", curiosity); // asking to move ahead 4 times 

            // 1 # rover should move to 22,29 and wait. 
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 22);
            Assert.AreEqual(position.Ycord, 29);

            // #2 we can also check if obstacle was met
            var obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 22);
            Assert.AreEqual(obstacle.Y, 30);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_CheckObstacleMet_MoveBackward()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 24, Y = 30 }, new PlutoObstacle() { X = 22, Y = 30 } };
            await curiosity.SetObstacles(obstacles); // [24,30] is an obstacle

            await SetPosition('N', 24, 32, curiosity); // setting it up at 24,32
            await commandController.Post("BBBB", curiosity); // asking to move back 4 times 

            // 1 # rover should move to 24,31 and wait. 
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'N');
            Assert.AreEqual(position.Xcord, 24);
            Assert.AreEqual(position.Ycord, 31);

            // #2 we can also check if obstacle was met
            var obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 24);
            Assert.AreEqual(obstacle.Y, 30);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_CheckObstacleMet_MoveAllDirections()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 5, Y = 5 }, new PlutoObstacle() { X = 7, Y = 7 } };
            await curiosity.SetObstacles(obstacles); // [5,5] [7,7] are obstacles

            await SetPosition('N', 3, 5, curiosity); // setting it up at 3,5
            await commandController.Post("RFFFF", curiosity); // asking to turn right and move forward 4 times 
             
            // check that position is as we expect E,4,5
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'E');
            Assert.AreEqual(position.Xcord, 4);
            Assert.AreEqual(position.Ycord, 5);

            //now check the obstacle 5,5
            var obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 5);
            Assert.AreEqual(obstacle.Y, 5);

            //now make it turn left go foward twice, turn left and foward 4 times
            await commandController.Post("LFFRFFFF", curiosity);

            // check that position is as we expect E,6,7
            position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'E');
            Assert.AreEqual(position.Xcord, 6);
            Assert.AreEqual(position.Ycord, 7);

            //now check the obstacle 7,7
            obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 7);
            Assert.AreEqual(obstacle.Y, 7);
        }

        [TestMethod()]
        public async Task Integration_Test_SetObstacle_CheckObstacleMet_MoveAllDirections_WithGarbageCommand()
        {
            var commandController = new CommandController();
            var curiosity = new Rover();
            var obstacles = new PlutoObstacle[] { new PlutoObstacle() { X = 5, Y = 5 }, new PlutoObstacle() { X = 7, Y = 7 } };
            await curiosity.SetObstacles(obstacles); // [5,5] [7,7] are obstacles

            await SetPosition('N', 3, 5, curiosity); // setting it up at 3,5
            await commandController.Post("&ZRUYFF98F1F", curiosity); // asking to turn right and move forward 4 times 

            // check that position is as we expect E,4,5
            var position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'E');
            Assert.AreEqual(position.Xcord, 4);
            Assert.AreEqual(position.Ycord, 5);

            //now check the obstacle 5,5
            var obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 5);
            Assert.AreEqual(obstacle.Y, 5);

            //now make it turn left go foward twice, turn left and foward 4 times
            await commandController.Post("<<L)(%F^^F8R817FF££FF", curiosity);

            // check that position is as we expect E,6,7
            position = await new PositionController().GetPositionAsync(curiosity);
            Assert.AreEqual(position.Direction, 'E');
            Assert.AreEqual(position.Xcord, 6);
            Assert.AreEqual(position.Ycord, 7);

            //now check the obstacle 7,7
            obstacle = await curiosity.EncounteredObstacle();
            Assert.AreEqual(obstacle.X, 7);
            Assert.AreEqual(obstacle.Y, 7);
        }
    }
}