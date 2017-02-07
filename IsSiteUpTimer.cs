using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IsSiteUp
{
    /// <summary>
    /// Running on a timer, determines whether a site is up (not returning a failing status code).
    /// If it is down, an email is sent.
    /// </summary>
    public static class IsSiteUpTimer
    {
        private static readonly string _url = Environment.GetEnvironmentVariable("URL");
        private static readonly string _sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
        private static readonly string _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
        private static readonly string _toEmail = Environment.GetEnvironmentVariable("TO_EMAIL");

        public async static Task Run(dynamic timer, dynamic log)
        {
            log.Info($"Checking URL: '{_url}'.");

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(_url))
                {
                    if (response.IsSuccessStatusCode)
                        return;

                    // Try a second time if the first time fails.
                    // Implemented to prevent one-off transport level errors
                    // from filling up my inbox :).
                    using (var secondResponse = await httpClient.GetAsync(_url))
                    {
                        if (!secondResponse.IsSuccessStatusCode)
                            await sendEmailAsync(response.StatusCode, log);
                    }
                }
            }
        }

        private async static Task sendEmailAsync(HttpStatusCode statusCode, dynamic log)
        {
            dynamic sg = new SendGridAPIClient(_sendGridApiKey);

            var fromEmail = new Email(_fromEmail);
            string subject = $"Website '{_url}' is down!";
            var toEmail = new Email(_toEmail);
            var emailContent = new Content("text/plain", $"Website '{_url}' is down with status code '{statusCode}'. Fix it!");
            var mail = new Mail(fromEmail, subject, toEmail, emailContent);

            dynamic response = await sg.client.mail.send.post(requestBody: mail.Get());
            log.Info($"Status code from sendgrid send: '{response.StatusCode}'.");
        }
    }
}