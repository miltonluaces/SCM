#region Imports

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ionic.Zip;
using System.IO;
using System.IO.Compression;

#endregion

namespace AibuSet {

    internal class Zipper {

        internal void Zip(string path, string fileName) {
            using (ZipFile zip = new ZipFile()) {
                zip.AddFile(path + @"\" + fileName);
                zip.Save(path + @"\" + fileName + ".zip");
            }
        }

        internal void Zip(string path, IList<string> fileNames, string zipFileName) {
            using (ZipFile zip = new ZipFile()) {
                foreach (string fileName in fileNames) {
                    zip.AddFile(path + @"\" + fileName);
                }
                zip.Save(path + @"\" + zipFileName + ".zip");
            }
        }

        internal void Unzip(string path, string fileName) {
            if (path != "") { fileName = path + @"\" + fileName; }
            try {
                using (ZipFile zip = ZipFile.Read(fileName)) {
                    zip.ExtractAll(path, ExtractExistingFileAction.OverwriteSilently);
                    zip.Dispose();
                }
            }
            catch (Exception ex) { throw new Exception("Error" + ex.Message + " " + ex.StackTrace); }
        }
    }
}
