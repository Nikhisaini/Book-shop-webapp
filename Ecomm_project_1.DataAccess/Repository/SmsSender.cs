using Ecomm_project_1.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Ecomm_project_1.DataAccess
{
    public class SmsSender : ISmsSender
    {
        private readonly ITwilioRestClient _client; 
        private readonly TwilioSettings _settings;

        public SmsSender(ITwilioRestClient client, IOptions<TwilioSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }

        public async Task SendSmsAsync(string number, string message)
        {
            if (!number.StartsWith("+"))
            {
                number = "+91" + number;
            }

            await MessageResource.CreateAsync(
                to: new PhoneNumber(number),
                from: new PhoneNumber(_settings.PhoneNumber),
                body: message,
                client: _client
            );
        }
    }
}