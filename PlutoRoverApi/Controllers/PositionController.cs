using System.Web.Http;
using System.Threading.Tasks;
using PlutoRover;
using System.Diagnostics;

namespace PlutoRoverApi.Controllers
{
    /// <summary>
    /// Web API 2 controller that implements the IPositionController interface
    /// This API handles only queries and cannot affect the state of the rover object
    /// </summary>
    public class PositionController : ApiController
    {        
        public async Task<RoverPosition> GetPositionAsync([FromBody] Rover rover)
        {
            // assertion here
            Debug.Assert(rover != null);

            // no exception handling as this will be caught by the test framework
            var position = await rover.GetPosition();
            return position;
        }

        public async Task SetPositionAsync(RoverPosition position, [FromBody] Rover rover)
        {
            // assertion here
            Debug.Assert(rover != null);

            // no exception handling as this will be caught by the test framework
            await rover.SetPosition(position);
        }

        public async Task<PlutoObstacle> GetObstacleAsync([FromBody] Rover rover)
        {
            // assertion here
            Debug.Assert(rover != null);

            // no exception handling as this will be caught by the test framework
            var obstacle = await rover.EncounteredObstacle();
            return obstacle;
        }
    }
}
