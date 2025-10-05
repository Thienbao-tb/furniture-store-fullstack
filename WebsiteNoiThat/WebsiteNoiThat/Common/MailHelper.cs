using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;

namespace WebsiteNoiThat.Common
{
    public class MailHelper
    {
        /*
        // ❌ BẢN CŨ — KHÓA CỨNG GMAIL (HÃY ĐỂ COMMENT)
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"].ToString();
            var fromEmailDisplayName = ConfigurationManager.AppSettings["FromEmailDisplayName"].ToString();
            var fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"].ToString();
            var smtpHost = ConfigurationManager.AppSettings["SMTPHost"].ToString();
            var smtpPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();

            bool enabledSsl = bool.Parse(ConfigurationManager.AppSettings["EnabledSSL"].ToString());

            string body = content;
            MailMessage message = new MailMessage(new MailAddress(fromEmailAddress, fromEmailDisplayName), new MailAddress(toEmailAddress));
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = body;

            var client = new SmtpClient();

            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
            client.Host = "smtp.gmail.com";
            client.Port = 587;

            client.Send(message);
        }
        */

        // ✅ BẢN MỚI — DÙNG CẤU HÌNH TỪ Web.config
        public void SendMail(string toEmailAddress, string subject, string content)
        {
            // Bắt buộc TLS 1.2 (đặc biệt trên IIS/.NET cũ)
            ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

            var fromEmailAddress = ConfigurationManager.AppSettings["FromEmailAddress"];
            var fromEmailDisplay = ConfigurationManager.AppSettings["FromEmailDisplayName"] ?? "NoiThatGo";
            var fromEmailPassword = ConfigurationManager.AppSettings["FromEmailPassword"];

            var smtpHost = ConfigurationManager.AppSettings["SMTPHost"];    // vd: smtp.gmail.com
            var smtpPortStr = ConfigurationManager.AppSettings["SMTPPort"];    // vd: 587
            var sslStr = ConfigurationManager.AppSettings["EnabledSSL"];  // "true"/"false"

            if (string.IsNullOrWhiteSpace(fromEmailAddress) ||
                string.IsNullOrWhiteSpace(fromEmailPassword) ||
                string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(smtpPortStr))
            {
                throw new InvalidOperationException(
                    "Thiếu cấu hình SMTP trong Web.config (FromEmailAddress/FromEmailPassword/SMTPHost/SMTPPort)."
                );
            }

            int smtpPort = int.Parse(smtpPortStr);
            bool enableSsl = string.IsNullOrWhiteSpace(sslStr) ? true : bool.Parse(sslStr);

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(fromEmailAddress, fromEmailDisplay);
                message.To.Add(new MailAddress(toEmailAddress));
                message.Subject = subject;
                message.Body = content;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(smtpHost, smtpPort))
                {
                    client.EnableSsl = enableSsl;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(fromEmailAddress, fromEmailPassword);
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Timeout = 20000;

                    client.Send(message);
                }
            }
        }
    }
}
