using PlutoRover;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Web.Http;

namespace PlutoRoverApi.Controllers
{
    /// <summary>
    /// Web API 2 controller that implements the functionality of sending the command
    /// This API handles only commands and cannot be used to query the state of the rover object
    /// </summary>
    public class CommandController : ApiController
    {
        // POST: send a Command to the Rover API
        // command can be a string having multiple commands
        // public async Task Post([FromBody]string command)
        public async Task Post([FromBody]string command, [FromBody] Rover rover)
        {
            // assertion here
            Debug.Assert(rover != null);

            foreach (char _moveCommand in command.ToCharArray())
            {
                switch (_moveCommand)
                {
                    case 'F':                        
                    case 'B':                        
                    case 'L':                        
                    case 'R':
                        try
                        {
                            await rover.Move(_moveCommand);
                        }
                        catch (System.TimeoutException ex)
                        {
                            // timeout happened - seems that our signal did not reach Pluto
                        }
                        catch (System.Exception ex)
                        {
                            // any other exception -- handler not implemented
                        }
                        break;
                    default:
                        // do nothing; we are not expecting this command
                        break;
                }
            }
        }        
    }
}
