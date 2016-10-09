//
// ElectronApp.cs
//
// Author:
//       Zach Deibert <zachdeibert@gmail.com>
//
// Copyright (c) 2016 Zach Deibert
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Zip;
using log4net;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Apis {
    public static class ElectronApp {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ElectronApp));

        public static bool IsInstalled {
            get {
                return File.Exists(Path.Combine(Entry.ShareDir, ApiPlatform.VersionId, ApiPlatform.Executable));
            }
        }

        private static void Copy(string inDir, string outDir) {
            Log.DebugFormat("Recursively copying directory {0} to {1}", inDir, outDir);
            Directory.CreateDirectory(outDir);
            foreach (string dir in Directory.EnumerateDirectories(inDir)) {
                Copy(dir, Path.Combine(outDir, Path.GetFileName(dir)));
            }
            foreach (string file in Directory.EnumerateFiles(inDir)) {
                Log.DebugFormat("Copying {0}", file);
                File.Copy(file, Path.Combine(outDir, Path.GetFileName(file)));
            }
        }

        public static void Install() {
            Log.Debug("Installing electron app...");
            WebRequest req = WebRequest.Create(ApiPlatform.DownloadUrl);
            using (WebResponse res = req.GetResponse()) {
                using (Stream download = res.GetResponseStream()) {
                    using (ZipInputStream zip = new ZipInputStream(download)) {
                        ZipEntry entry;
                        while ((entry = zip.GetNextEntry()) != null) {
                            if (entry.IsFile) {
                                string file = Path.Combine(Entry.ShareDir, ApiPlatform.VersionId, entry.Name);
                                Log.DebugFormat("Extracting {0}", file);
                                Directory.CreateDirectory(Path.GetDirectoryName(file));
                                using (Stream output = new FileStream(file, FileMode.CreateNew)) {
                                    zip.CopyTo(output);
                                }
                            }
                        }
                    }
                }
            }
            if (ApiPlatform.ExecutableBitNeeded) {
                Log.Debug("Marking executable with x-bit");
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.Arguments = string.Format("+x {0}/{1}/{2}", Entry.ShareDir, ApiPlatform.VersionId, ApiPlatform.Executable);
                psi.FileName = "/bin/chmod";
                using (Process proc = Process.Start(psi)) {
                    proc.WaitForExit();
                }
            }
            Copy(Path.Combine(Entry.ShareDir, "oauth"), Path.Combine(Entry.ShareDir, ApiPlatform.VersionId, ApiPlatform.ResourcesDir));
        }

        public static void EnsureInstalled() {
            if (!IsInstalled) {
                Install();
            }
        }

        public static string Run(string query) {
            Log.DebugFormat("Running query {0}", query);
            EnsureInstalled();
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(Entry.ShareDir, ApiPlatform.VersionId, ApiPlatform.Executable);
            psi.RedirectStandardInput = true;
            psi.RedirectStandardOutput = true;
            psi.UseShellExecute = false;
            Log.Debug("Launching electron");
            using (Process proc = Process.Start(psi)) {
                proc.StandardInput.WriteLine(query);
                try {
                    return proc.StandardOutput.ReadLine();
                } finally {
                    proc.WaitForExit();
                }
            }
        }
    }
}

