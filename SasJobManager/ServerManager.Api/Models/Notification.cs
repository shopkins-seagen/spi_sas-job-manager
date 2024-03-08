using SasJobManager.Lib.Models;
using System.Management.Automation;
using System.Net.Mail;

namespace ServerManager.Api.Models
{
    public class Notification
    {
        private SmtpClient _smtp;
        private MessageDetails _details;
        public Notification(string smtp,MessageDetails details)
        {
            _smtp = new SmtpClient(smtp);
            _details = details;
        }

        public async Task Send()
        {
            var mail = new MailMessage()
            {
                IsBodyHtml= true,
                Subject = $"SAS Job Manager Notification: {_details.Program} completed",
                //Body = $"<p>SAS programs started by <em>{_details.Sender}</em> completed. Worst log finding: <em>{_details.Status}</em>.<br>See <a href=\"{_details.SummaryFn}\">Summary</a> for details",
                Body=_details.Content,
                From = new MailAddress($"{_details.Sender}@seagen.com")
            };
            foreach(var r in _details.Recipients)
            {
                mail.To.Add($"{r}@seagen.com");
            }

            await _smtp.SendMailAsync(mail);
        }


    }
}
