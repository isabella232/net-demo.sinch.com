using System.Threading.Tasks;
using Sinch.ServerSdk.Messaging.Models;
using Sinch.WebApiClient;

namespace demo.sinch.com.Models {
    public interface ICalloutApiEndpoints {
        [HttpPost("calling/v1/callouts/")]
        Task<SendSmsResponse> AddParticipant([ToBody] CalloutRequest request);
    }

    public class Destination {
        public string type { get; set; }
        public string endpoint { get; set; }
    }

    public class ConferenceCallout {
        public string cli { get; set; }
        public Destination destination { get; set; }
        public string domain { get; set; }
        public string custom { get; set; }
        public string locale { get; set; }
        public string greeting { get; set; }
        public string conferenceId { get; set; }
        public bool enableDice { get; set; }
    }

    public class CalloutRequest {
        public string method { get; set; }
        public ConferenceCallout conferenceCallout { get; set; }
    }
}