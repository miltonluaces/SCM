#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Mail;

#endregion

namespace AibuSet {

    internal class SysUpdate {
        
        private WebClient wc;

        internal SysUpdate() {
            wc = new WebClient(); 
        }

        internal void LoadFromWeb() {
            //string url = "http://sourceforge.net/projects/naturaldocs/files/Stable%20Releases/1.52/NaturalDocs-1.52.zip/download?use_mirror=freefr&download=";
            string url = "https://docs.google.com/viewer?a=v&pid=sites&srcid=ZGVmYXVsdGRvbWFpbnxtaWx0b25tYXJ0aW5lemx1YWNlc3xneDo2MjYxMmFkOGI5NmY4NTIy";
            string path = "H:\\perl\\cv_English.pdf"; 
            Uri uri = new Uri(url);
            try { wc.DownloadFileAsync(uri, path); }
            //try { wc.DownloadFile(uri, path); }
            catch (Exception ex) { Console.WriteLine(ex.StackTrace); }
        }
    }
}
