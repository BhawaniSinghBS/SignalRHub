using Microsoft.AspNetCore.Mvc;
using SignalRClient.Helpers.HTTPHelper.ApiRequests;
using System.Collections.Concurrent;
using HubTimers = SignalRServer.HubTimers;

namespace SignalRHub.Server.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SignalRController : ControllerBase
    {

        //private readonly ILogger<SignalRController> _logger;
        public SignalRController(ILogger<SignalRController> logger, IConfiguration configuration)
        {

        }

        [HttpPost("MakeTCPCall")]
        public async Task<ActionResult> GetResponseOverSignalRFrom(TCPRequestDTO DataRequestFromClient)
        { 
            if (DataRequestFromClient == null || DataRequestFromClient.TCPRequest == null)
            {
                return BadRequest();
            }

            HubTimers.CallTcpAndSendRecivedBytesToSubscribers(DataRequestFromClient);// send first response immediately 
            if (!DataRequestFromClient.UseClientTimer)
            {// if client is using its timer no need to excecute with server timer
                // put to timer que for sending data continuesly to client

                string expectedtagForThisUserRequest = SignalRClient.SignalRClient.GetEncryptedTag(
                                               receiveType: DataRequestFromClient.SignalRRequestType,
                                               etityId: DataRequestFromClient.EntityId.ToString(),
                                               out string nonEncruptedTagOnlyToDebugForUserRequst,
                                               encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                                               timeInterValForRecivingData_inMS: (int)DataRequestFromClient.SignalRRequestType);

                HubTimers.TimePeriodAndRequestObj.AddOrUpdate(
                                   (int)DataRequestFromClient.RunAfterMilliconds,//for 1 second requests (key)
                                   key =>
                                   {
                                       // This function is called when the key is not present in the dictionary
                                       // Create a new HashSet and add the connection ID
                                       var newRequestListFor1Second = new ConcurrentBag<object>() { DataRequestFromClient };
                                       return newRequestListFor1Second;
                                   },
                                   (key, existingRequestsForThisKey) =>
                                   {
                                       // This function is called when the key is already present in the dictionary
                                       // Update the existing HashSet by adding the connection ID
                                       if (existingRequestsForThisKey != null && existingRequestsForThisKey.Any(a =>
                                       {

                                           var existingRequestsEach = a as SignalRRequestBaseDTO;
                                           // if this request already exists 

                                           string expectedTagForThisExistingRequest = SignalRClient.SignalRClient.GetEncryptedTag(
                                               receiveType: existingRequestsEach.SignalRRequestType,
                                               etityId: existingRequestsEach.EntityId.ToString(),
                                               out string nonEncruptedTagOnlyToDebugOfExistingRequest,
                                               encruptionKey: SignalRClient.ClientSettings.ClientSettings.TagEncruptionKey,
                                               timeInterValForRecivingData_inMS: (int)existingRequestsEach.SignalRRequestType);

                                           return existingRequestsEach != null && // if there is no request for this time call back period
                                                    DataRequestFromClient != null &&
                                                    expectedTagForThisExistingRequest == expectedtagForThisUserRequest;// if request for this time period is there is not request for this tag 
                                       })
                                       )
                                       {
                                           return existingRequestsForThisKey;
                                       }
                                       else
                                       {
                                           // if null
                                           existingRequestsForThisKey.Add(DataRequestFromClient);
                                           return existingRequestsForThisKey;
                                       }
                                   }
                               );
            }
            return Ok();
        }

    }
}