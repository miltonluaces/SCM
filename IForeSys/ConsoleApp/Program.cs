#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Net.Mail;
using System.Net.Security;
using System.IO;
using System.Security.Cryptography.X509Certificates;

#endregion

namespace ConsoleApp {

    public class Program {

        public static void Main(string[] args) {
            //Tcp();
            //Smtp();
            SmtpAttach();
            Console.In.ReadLine();
        }

        #region Tcp

        private static void Tcp() {
            try {
                // create an instance of TcpClient
                TcpClient tcpclient = new TcpClient();
                // HOST NAME POP SERVER and gmail uses port number 995 for POP  
                //tcpclient.Connect("pop.gmail.com", 995);
                tcpclient.Connect("imap.gmail.com", 993);
                // This is Secure Stream // opened the connection between client and POP Server
                SslStream sslstream = new SslStream(tcpclient.GetStream());
                // authenticate as client  

                sslstream.AuthenticateAsClient("imap.gmail.com");

                bool flag = sslstream.IsAuthenticated;   // check flag
                // Asssigned the writer to stream
                System.IO.StreamWriter sw = new StreamWriter(sslstream);
                // Assigned reader to stream
                System.IO.StreamReader reader = new StreamReader(sslstream);

                sw.WriteLine("tag LOGIN ailogsys@gmail.com newdeal*12");
                sw.Flush();

                sw.WriteLine("tag2 EXAMINE inbox");
                sw.Flush();

                sw.WriteLine("tag3 LOGOUT ");
                sw.Flush();

                string str = string.Empty;
                string strTemp = string.Empty;
                try {
                    while ((strTemp = reader.ReadLine()) != null) {
                        Console.WriteLine(strTemp);
                        // find the . character in line
                        if (strTemp == ".") {
                            //reader.Close();
                            break;
                        }
                        if (strTemp.IndexOf("-ERR") != -1) {
                            //reader.Close();
                            break;
                        }
                        str += strTemp;
                    }
                    Console.In.ReadLine();
                }
                catch (Exception ex) {
                    string s = ex.Message;
                }
                //reader.Close();

            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        #region Smtp

        private static void Smtp() {

            MailAddress fromAddress = new MailAddress("ailogsys@gmail.com", "From AILS");
            MailAddress toAddress = new MailAddress("mmartinezluaces@gmail.com", "To Prueba");
            string fromPassword = "newdeal*12";
            string subject = "Prueba 1";
            string body = "Uno dos tres probando";

            SmtpClient smtp = new SmtpClient {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            
            using (var message = new MailMessage(fromAddress, toAddress)  { Subject = subject,  Body = body } ) {
                ServicePointManager.ServerCertificateValidationCallback =  delegate(object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                smtp.Send(message);
            }
        }

        #endregion

        #region Smtp Attach

        private static void SmtpAttach() {

            try {
                MailAddress mailfrom = new MailAddress("ailogsys@gmail.com", "From AILS");
                MailAddress mailto = new MailAddress("mmartinezluaces@gmail.com", "To Prueba");
                MailMessage newmsg = new MailMessage(mailfrom, mailto);
                string password = "newdeal*12";
    
                newmsg.Subject = "Prueba 3 con attach";
                newmsg.Body = "Uno dos tres probando"; 

                 Attachment att = new Attachment(@"..\Files\Script.txt");
                newmsg.Attachments.Add(att);

                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(mailfrom.Address, password);
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
        #endregion

    }
 }
