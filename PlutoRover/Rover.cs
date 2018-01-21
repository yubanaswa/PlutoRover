using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace PlutoRover
{
    [Serializable]
    public class RoverPosition
    {
        public char Direction;
        public short Xcord;
        public short Ycord;
    }

    public class PlutoObstacle
    {
        public short X;
        public short Y;
    }

    /// <summary>
    /// Interface for the rover object 
    /// enables us to create a mock rover object (on earth) which can be injected
    /// </summary>
    public interface IRover
    {
        Task<RoverPosition> GetPosition();

        Task SetPosition(RoverPosition position);

        Task Move(char command);

        Task SetObstacles(PlutoObstacle[] obstacles);

        Task<PlutoObstacle> EncounteredObstacle();
    }

    public class Rover : IRover
    {
        // private vars

        /// <summary>
        /// Maximum X coord of the grid
        /// </summary>
        private const int maxX = 100;

        /// <summary>
        /// Maximum Y coord of the grid
        /// </summary>
        private const int maxY = 100;

        /// <summary>
        /// using a file to serialize the position, as the rover is a stateful object 
        /// and it must retain the information of the current position
        /// </summary>
        private const string POSITION_FILE = "roverposition.txt";

        /// <summary>
        /// using a file to serialize the obstacle positions
        /// </summary>
        private const string OBSTACLES_FILE = "plutoobstacles.txt";

        /// <summary>
        /// Obstacles in the grid
        /// </summary>
        private List<PlutoObstacle> _obstacles = null;

        /// <summary>
        /// Variable to be set when obstacle is actually met while moving
        /// or else this will remain NULL
        /// </summary>
        private PlutoObstacle _obstacleEncounteredWhileMoving = null;

        /// <summary>
        /// The current position
        /// </summary>
        private RoverPosition _position;

        /// <summary>
        /// Constructs a rover object - actually nothing to do in this case
        /// </summary>
        public Rover()
        {            
        }

        // Public methods
                
        /// <summary>
        /// Get the position of the rover 
        /// </summary>
        /// <returns></returns>
        public async Task<RoverPosition> GetPosition()
        {
            RoverPosition position = await _GetCurrentPosition();
            return position;
        }

        /// <summary>
        /// You can set a position - makes testing code easy, not for the real life scenario
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public async Task SetPosition(RoverPosition position)
        {
            _position = position;
            await _SavePosition();
        }
        
        /// <summary>
        /// Rover can only take a single command at a time
        /// so multiple commands can be sent out in a series 
        /// and they will be sequentially carried out
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public async Task Move(char command)
        {
            _position = await _GetCurrentPosition();

            switch (command)
            {
                case 'F':
                    await _MoveForward(); break;
                case 'B':
                    await _MoveBackward(); break;
                case 'L':
                    await _TurnLeft(); break;
                case 'R':
                    await _TurnRight(); break;
                default:
                    // do nothing;
                    break;
            }

            await _SavePosition();
        }

        /// <summary>
        /// You can inject obstacles from the test case, 
        /// a simulation of the real universe of Pluto
        /// </summary>
        /// <param name="obstacles"></param>
        /// <returns></returns>
        public async Task SetObstacles(PlutoObstacle[] obstacles)
        {
            if (obstacles == null)
                return; // nothing to do

            // add them to the list
            _obstacles = new List<PlutoObstacle>();
            foreach (var obstacle in obstacles)
            {
                _obstacles.Add(obstacle);
            }

            // now serialize
            await _SaveObstacles();
        }

        /// <summary>
        /// Check if the Rover encountered any obstacle
        /// </summary>
        /// <returns></returns>
        public Task<PlutoObstacle> EncounteredObstacle()
        {
            var obstacle = Task.Run(() =>
            {
                return _obstacleEncounteredWhileMoving;
            });

            return obstacle;
        }

        // Private methods

        private async Task _MoveForward()
        {
            // start making the move
            var newPosition = new RoverPosition() { Direction = _position.Direction, Xcord = _position.Xcord, Ycord = _position.Ycord };
            
            await Task.Run(async () =>
            {
                switch (_position.Direction)
                {
                    case 'N': newPosition.Ycord++; break;
                    case 'S': newPosition.Ycord--; break;
                    case 'E': newPosition.Xcord++; break;
                    case 'W': newPosition.Xcord--; break;
                    default: break;
                }

                PlutoObstacle obstacle = _CheckForObstacle(newPosition);
                if (obstacle != null)
                {
                    _SetObstacleEncounteredInternal(obstacle);
                }
                else
                {
                    // actually made the move        
                    _position.Direction = newPosition.Direction;
                    _position.Xcord = newPosition.Xcord;
                    _position.Ycord = newPosition.Ycord;
                    await _WrapPosition();
                }
                
            });                     
        }
        
        private async Task _MoveBackward()
        {
            // start making the move
            var newPosition = new RoverPosition() { Direction = _position.Direction, Xcord = _position.Xcord, Ycord = _position.Ycord };

            await Task.Run(async () =>
            {
                switch (_position.Direction)
                {
                    case 'N': newPosition.Ycord--; break;
                    case 'S': newPosition.Ycord++; break;
                    case 'E': newPosition.Xcord--; break;
                    case 'W': newPosition.Xcord++; break;
                    default: break;
                }

                PlutoObstacle obstacle = _CheckForObstacle(newPosition);
                if (obstacle != null)
                {
                    _SetObstacleEncounteredInternal(obstacle);
                }
                else
                {
                    // actually made the move        
                    _position.Direction = newPosition.Direction;
                    _position.Xcord = newPosition.Xcord;
                    _position.Ycord = newPosition.Ycord;
                    await _WrapPosition();
                }
            });            

            await _WrapPosition();
        }

        private async Task _TurnLeft()
        {
            await Task.Run(() =>
            {
                switch (_position.Direction)
                {
                    case 'N': _position.Direction = 'W'; break;
                    case 'S': _position.Direction = 'E'; break;
                    case 'E': _position.Direction = 'N'; break;
                    case 'W': _position.Direction = 'S'; break;
                    default: break;
                }
                
            });

            await _WrapPosition();

        }

        private async Task _TurnRight()
        {
            var newPosition = _position;
            await Task.Run(() => {
                switch (_position.Direction)
                {
                    case 'N': _position.Direction = 'E'; break;
                    case 'S': _position.Direction = 'W'; break;
                    case 'E': _position.Direction = 'S'; break;
                    case 'W': _position.Direction = 'N'; break;
                    default: break;
                }               
            });            
        }

        private async Task _WrapPosition()
        {
            await Task.Run(() => {
                if (_position.Xcord > 100) _position.Xcord = 0;
                if (_position.Ycord > 100) _position.Ycord = 0;
                if (_position.Xcord < 0) _position.Xcord = 100;
                if (_position.Ycord < 0) _position.Ycord = 100;
            });
        }

        private async Task _SavePosition()
        {
            await Task.Run(() => {
                // Create a FileStream that will write data to file.
                using (var writerFileStream =
                    new FileStream(POSITION_FILE, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writerFileStream, _position);
                }
            });        
        }
                              
        private async Task<RoverPosition> _GetCurrentPosition()
        {
            var position = new RoverPosition() { Direction = 'N', Xcord = 0, Ycord = 0 };

            await Task.Run(() => {                
                if (File.Exists(POSITION_FILE))
                {
                    try
                    {
                        using (var readerFileStream = new FileStream(POSITION_FILE,
                            FileMode.Open, FileAccess.Read))
                        {
                            if (readerFileStream == null)
                                return position;

                            BinaryFormatter formatter = new BinaryFormatter();
                            position = (RoverPosition)formatter.Deserialize(readerFileStream);
                        }
                    }
                    catch (Exception)
                    {

                    }
                }
                return position;
            });

            return position;
        }
                
        private async Task _SaveObstacles()
        {
            await Task.Run(() => {

                if (_obstacles == null || _obstacles.Count > 0)
                {
                    // no obstacles
                    return;
                }

                // Create a FileStream that will write data to file.
                using (var writerFileStream =
                    new FileStream(OBSTACLES_FILE, FileMode.Create, FileAccess.Write))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(writerFileStream, _obstacles);
                }
            });
        }
               
        private PlutoObstacle[] _GetObstaclesInternal()
        {
            if (File.Exists(OBSTACLES_FILE))
            {
                try
                {
                    using (var readerFileStream = new FileStream(POSITION_FILE,
                        FileMode.Open, FileAccess.Read))
                    {
                        if (readerFileStream != null)
                        {
                            BinaryFormatter formatter = new BinaryFormatter();
                            _obstacles = (List<PlutoObstacle>)formatter.Deserialize(readerFileStream);
                        }
                    }
                }
                catch (Exception)
                {

                }
            }

            return _obstacles == null ? null : _obstacles.ToArray();
        }

        private void _SetObstacleEncounteredInternal(PlutoObstacle obstacle)
        {
            _obstacleEncounteredWhileMoving = obstacle;
        }

        private PlutoObstacle _CheckForObstacle(RoverPosition newPosition)
        {
            var obstacles = _GetObstaclesInternal();
            if (obstacles == null) return null;

            foreach (var obstacle in obstacles)
            {
                if (newPosition.Xcord == obstacle.X && newPosition.Ycord == obstacle.Y)
                    return new PlutoObstacle() { X = obstacle.X, Y = obstacle.Y };
            }

            return null; // not encountered any
        }

    }
}
