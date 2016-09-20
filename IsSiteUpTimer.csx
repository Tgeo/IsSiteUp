// Manually copied and formed from IsSiteUpTimer.cs.

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;

private static readonly string _url = Environment.GetEnvironmentVariable("URL");
private static readonly string _sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
private static readonly string _fromEmail = Environment.GetEnvironmentVariable("FROM_EMAIL");
private static readonly string _toEmail = Environment.GetEnvironmentVariable("TO_EMAIL");

public async static Task Run(TimerInfo timer, TraceWriter log)
{
    log.Info($"Checking URL: '{_url}'.");

    using (var httpClient = new HttpClient())
    {
        using (var response = await httpClient.GetAsync(_url))
        {
            if (!response.IsSuccessStatusCode)
                await sendEmailAsync(response.StatusCode, log);
        }
    }
}

private async static Task sendEmailAsync(HttpStatusCode statusCode, TraceWriter log)
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