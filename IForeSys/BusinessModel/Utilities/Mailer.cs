#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

 
#endregion

namespace AibuSet {

    internal class Mailer {

        #region Fields

        private string ailAddress;
        private string ailPassword;

        #endregion

        #region Constructor

        internal Mailer() {
            ailAddress = "ailogsys@gmail.com";
            ailPassword = "CityZurich@14";
        }

        #endregion

        #region internal Methods

        internal void Send(string cliName, string cliAddress, string subject, string body, string attachFileName) {

            try {
                MailAddress mailfrom = new MailAddress(ailAddress, "From AILS");
                MailAddress mailto = new MailAddress(cliAddress, "To " + cliName);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);
             
                newmsg.Subject = subject;
                newmsg.Body = body;

                Attachment att = new Attachment(@"..\Files\" + attachFileName);
                newmsg.Attachments.Add(att);

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(mailfrom.Address, ailPassword);
                smtp.EnableSsl = true;
                using (var message = newmsg) {
                    ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    smtp.Send(message);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }

        internal void SendSms(string cliName, string phoneNumber, string carrier, string message) {

            try {
                string cliAddress = phoneNumber.Trim() + carrier.Trim();
                MailAddress mailfrom = new MailAddress(ailAddress, "From AILS");
                MailAddress mailto = new MailAddress(cliAddress, "To " + cliName);
                MailMessage newmsg = new MailMessage(mailfrom, mailto);
             
                newmsg.Body = message;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(mailfrom.Address, ailPassword);
                smtp.EnableSsl = true;
                using (var mess = newmsg) {
                    ServicePointManager.ServerCertificateValidationCallback = delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                    smtp.Send(mess);
                }
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }
        }



        #endregion
    }
}
