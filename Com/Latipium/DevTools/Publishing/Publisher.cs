//
// Publisher.cs
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
using System.IO;
using log4net;
using NuGet;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Publishing {
    /// <summary>
    /// The implementation of the publish command.
    /// </summary>
    public static class Publisher {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Publisher));

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="verb">The command verb.</param>
        public static void Handle(PublishVerb verb) {
            if (verb.FileName == null) {
                string[] packages = Directory.GetFiles(".", "*.nupkg");
                switch (packages.Length) {
                    case 0:
                        Log.Fatal("No packages found!");
                        return;
                    case 1:
                        verb.FileName = packages[0];
                        break;
                    default:
                        Log.Fatal("Too many packages found in project directory.");
                        Log.Fatal("Try deleting all of the '*.nupkg' and rebuilding the project.");
                        return;
                }
            }
            int packageSize;
            using (Stream stream = new FileStream(verb.FileName, FileMode.Open, FileAccess.Read)) {
                stream.Seek(0, SeekOrigin.End);
                packageSize = (int) stream.Position;
            }
            PackageServer server = new PackageServer("https://www.nuget.org/api/v2/package", "Latipium DevTools (https://github.com/latipium/dev-tools)");
            ZipPackage package = new ZipPackage(verb.FileName);
            try {
                server.PushPackage(string.IsNullOrWhiteSpace(verb.ApiKey) ? Environment.GetEnvironmentVariable("LATIPIUM_NUGET_KEY") : verb.ApiKey, package, packageSize, 1800, false);
            } catch (Exception ex) {
                Log.Error("Upload failed.", ex);
                IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
                IPackage tmp;
                if (repo.TryFindPackage(package.Id, package.Version, out tmp)) {
                    return;
                } else {
                    Log.Error("However, it appears the package has been saved on the server.");
                    Log.Error("Ignoring upload error.");
                }
            }
            Log.Info("Package uploaded!");
            if (verb.Delete) {
                File.Delete(verb.FileName);
                Log.Info("Package deleted.");
            }
        }
    }
}

