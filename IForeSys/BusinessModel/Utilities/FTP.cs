#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
 
#endregion

namespace AibuSet {

    internal class FTP {

        #region Fields

        private string ftpServerIP;
        private string ftpUserID;
        private string ftpPassword;
        private string localDir;
        private string uri;

        #endregion

        #region Constructor

        internal FTP() {
            this.ftpServerIP = "69.162.77.130:21";
            this.uri = "ftp://" + ftpServerIP + "/Clients";
            this.ftpUserID = "mml@ailogsys.com";
            this.ftpPassword = "NewDealFtp*12";
            this.localDir = @"..\\";

        }

        #endregion

        #region internal Methods

        internal string GetDirList() {
            FtpWebRequest reqFTP = GetFtpWebRequest(uri, WebRequestMethods.Ftp.ListDirectory);
            char[] trimChars = {'\0' , '\r', '\n'};
            return GetString(reqFTP, "dirList.txt").Trim(trimChars);
        }

        internal void Upload(string path, string file) {
            FtpWebRequest reqFTP = GetReadFtpWebRequest(uri + "/" + file, WebRequestMethods.Ftp.UploadFile);
            ReadFile(reqFTP, path, file); 
        }

        internal void Download(string path, string file) {
           FtpWebRequest reqFTP = GetFtpWebRequest(uri + "/" + file, WebRequestMethods.Ftp.DownloadFile);
           WriteFile(reqFTP, path, file);
        }

        #endregion

        #region Private Methods

        private FtpWebRequest GetFtpWebRequest(string uri, string method) {
            Uri serverUri = new Uri(uri);
            if (serverUri.Scheme != Uri.UriSchemeFtp) { return null; }
            FtpWebRequest reqFTP;
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.KeepAlive = false;
            reqFTP.Method = method;
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.UsePassive = true;
            return reqFTP;
        }

        private FtpWebRequest GetReadFtpWebRequest(string file, string method) {
            FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(file);
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            reqFTP.KeepAlive = false;
            reqFTP.Method = method;
            reqFTP.UseBinary = true;
            reqFTP.Proxy = null;
            reqFTP.UsePassive = true;
            return reqFTP;
        }

        private void WriteFile(FtpWebRequest reqFTP, string path, string file) {
            try {
                if (path != "") { file = path + @"\" + file; }
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream writeStream = new FileStream(file, FileMode.Create);
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0) {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                writeStream.Close();
                response.Close();
            }
            catch (WebException wEx) { Console.WriteLine("Download Error" + wEx.Message); }
            catch (Exception ex) { Console.WriteLine("Download Error" + ex.Message); }
        }

        private void ReadFile(FtpWebRequest reqFTP, string path, string file) {
            List<byte> dataList = new List<byte>();
            FileStream fs = new FileStream(path + "/" + file, FileMode.Open);
            int Length = 2048;
            Byte[] buffer = new Byte[Length];
            int bytesRead = fs.Read(buffer, 0, Length);
            while (bytesRead > 0) {
                dataList.AddRange(buffer);
                bytesRead = fs.Read(buffer, 0, Length);
            }
            byte[] data = dataList.ToArray();

            reqFTP.ContentLength = data.Length;
            Stream reqStream = reqFTP.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            Console.WriteLine("Upload " + file + " complete. " + response.StatusDescription);
            response.Close();
        }

        private string GetString(FtpWebRequest reqFTP, string file) {
            try {
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                int Length = 2048;
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                StringBuilder sb = new StringBuilder();
                foreach (Byte b in buffer) { sb.Append(Convert.ToChar(b)); }
                response.Close();
                return sb.ToString();
            }
            catch (WebException wEx) { Console.WriteLine("Download Error" + wEx.Message); }
            catch (Exception ex) { Console.WriteLine("Download Error" + ex.Message); }
            return SysEnvironment.GetInstance().SysFileName; 
        }
        
        private string ToString(byte[] bytes) {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes) { sb.Append(Convert.ToChar(b)); }
            return sb.ToString();
        }

        #endregion

        #region Obsolete

        internal void DownloadObsolete(string file) {
            try {
                //string uri = "ftp://" + ftpServerIP + "/" + file;
                string uri = "ftp://" + ftpServerIP;
                Uri serverUri = new Uri(uri);
                if (serverUri.Scheme != Uri.UriSchemeFtp) { return; }
                FtpWebRequest reqFTP = GetFtpWebRequest(uri, WebRequestMethods.Ftp.ListDirectory);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
                reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                reqFTP.KeepAlive = false;
                //reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;                                
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                reqFTP.UseBinary = true;
                //reqFTP.Proxy = GlobalProxySelection.GetEmptyWebProxy();     
                reqFTP.Proxy = null;
                reqFTP.UsePassive = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream responseStream = response.GetResponseStream();
                FileStream writeStream = new FileStream(localDir + "\\" + file, FileMode.Create);
                int Length = 2048;
                //StreamReader sr = new StreamReader(response.GetResponseStream(), System.Text.Encoding.ASCII);
                //string line = "";
                //while (sr.Peek() > -1) { line = sr.ReadLine(); }
                Byte[] buffer = new Byte[Length];
                int bytesRead = responseStream.Read(buffer, 0, Length);
                while (bytesRead > 0) {
                    writeStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, Length);
                }
                writeStream.Close();
                response.Close();
            }
            catch (WebException wEx) { Console.WriteLine("Download Error" + wEx.Message); }
            catch (Exception ex) {
                Console.WriteLine("Download Error" + ex.Message);
            }
        }

        #endregion
    }
}
