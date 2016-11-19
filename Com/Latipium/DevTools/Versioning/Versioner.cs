//
// Versioner.cs
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
using System.Linq;
using log4net;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Versioning {
    /// <summary>
    /// The implementation of the version command.
    /// </summary>
    public static class Versioner {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Versioner));

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="verb">The command verb.</param>
        public static void Handle(CalculateVersionVerb verb) {
            if (Log.IsDebugEnabled) {
                Log.DebugFormat("Entering versioner verb with parameters gitDir={0}, files={1}", verb.GitDir, verb.Files.Aggregate((a,
                                                                                                                                b) => a + ", " + b));
            }
            GitVersion version = new GitVersion(verb.GitDir);
            foreach (string filename in verb.Files) {
                FileVersioner file = new FileVersioner(filename, version.Version);
                if (file.Exists) {
                    Log.DebugFormat("Replacing old version of file {0} with version {1}", filename, version);
                    file.ReplaceAll();
                } else {
                    Log.DebugFormat("Creating file {0} with new version information", filename);
                    file.Create();
                }
            }
        }
    }
}

