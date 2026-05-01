using Ecomm_project_1.DataAccess.Repository.IRepository;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;
using Twilio.Clients;
using Ecomm_project_1.Utility;
using Microsoft.CodeAnalysis.Options;
using Microsoft.Extensions.Options;

namespace Ecomm_project_1.DataAccess.Repository
{
    public class TwilioService : ITwilioService
    {
        private readonly  ITwilioRestClient _client;
        private readonly TwilioSettings _settings;
        public TwilioService(ITwilioRestClient client, IOptions<TwilioSettings> settings)
        {
            _client = client;
            _settings = settings.Value;
        }
        public async Task SendOrderConfirmationSmsAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames)
        {
            if (!toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+91" + toPhoneNumber;
            }
            string products = string.Join(", ", productNames);

            await MessageResource.CreateAsync(
                body: $"Hi {customerName}, your order #{orderId} for [{products}] is confirmed!",
                from: new PhoneNumber(_settings.PhoneNumber),
                to: new PhoneNumber(toPhoneNumber),
                client: _client
            );
        }
        public async Task MakeOrderConfirmationCallAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames)
        {

            if (!toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+91" + toPhoneNumber;
            }
            string products = string.Join(". ", productNames);

            var response = new VoiceResponse();
            response.Say($"Hello {customerName}. Thank you for shopping with us.", voice: "Polly.Kajal-Neural",language: "en-IN");
            response.Pause(1);
            response.Say($"Your order number is {orderId}.", voice: "Polly.Kajal-Neural",language: "en-IN");
            response.Pause(1);
            response.Say($"You have ordered: {products}. We will notify you when it ships. Goodbye!", voice: "Polly.Kajal-Neural",language: "en-IN");

            await CallResource.CreateAsync(
                twiml: response.ToString(),
                from: new PhoneNumber(_settings.PhoneNumber),
                to: new PhoneNumber(toPhoneNumber),
                client: _client
            );
        }
        public async Task SendOrderConfirmationWhatsAppAsync(string toPhoneNumber, int orderId, string customerName, IEnumerable<string> productNames)
        {           
            if (!toPhoneNumber.StartsWith("+"))
            {
                toPhoneNumber = "+91" + toPhoneNumber;
            }
            string products = string.Join(", ", productNames);

            await MessageResource.CreateAsync(
                body: $"Hi {customerName}, your order #{orderId} for [{products}] is confirmed!",
                from: new PhoneNumber("whatsapp:" + _settings.WhatsAppNumber),
                to: new PhoneNumber("whatsapp:" + toPhoneNumber),
                client: _client
            );
        }
    }
}
