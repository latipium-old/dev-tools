//
// Entry.cs
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
using CommandLine;
using log4net;
using log4net.Config;
using Com.Latipium.DevTools.Authorizing;
using Com.Latipium.DevTools.Packaging;
using Com.Latipium.DevTools.Publishing;
using Com.Latipium.DevTools.Versioning;

namespace Com.Latipium.DevTools.Main {
    public static class Entry {
        public static CommonOptions Options;
        internal static readonly Options RootOptions = new Options();
        private static readonly ILog Log = LogManager.GetLogger(typeof(Entry));

        private static void InitializeLogging() {
            using (Stream logConfig = new FileStream(Options.VerboseMode ? "logging-verbose.xml" : "logging.xml", FileMode.Open)) {
                XmlConfigurator.Configure(logConfig);
            }
            Log.Debug("Logging initialized in verbose mode");
        }

        private static void OnVerbCommand(string cmd, object verb) {
            if (!(verb is HelpVerb)) {
                RootOptions.HelpVerb.VerbName = cmd;
            }
            Options = (CommonOptions) verb;
        }

        private static bool ShowHelpIfNeeded() {
            if (Options is HelpVerb) {
                Console.WriteLine(((HelpVerb) Options).GetUsage());
                return true;
            }
            return false;
        }

        public static int Main(string[] args) {
            bool success = false;
            try {
                success = Parser.Default.ParseArguments(args, RootOptions, OnVerbCommand);
            } catch (NullReferenceException) {
            }
            if (success) {
                InitializeLogging();
                if (!ShowHelpIfNeeded()) {
                    Environment.CurrentDirectory = Options.WorkingDirectory;
                    if (Options is CalculateVersionVerb) {
                        Versioner.Handle((CalculateVersionVerb) Options);
                    } else if (Options is CreatePackageVerb) {
                        Packager.Handle((CreatePackageVerb) Options);
                    } else if (Options is PublishVerb) {
                        Publisher.Handle((PublishVerb) Options);
                    } else if (Options is AuthorizeCIVerb) {
                        Authorizer.Handle((AuthorizeCIVerb) Options);
                    } else {
                        Log.Fatal("Internal Error");
                    }
                }
                return 0;
            } else {
                Console.WriteLine(RootOptions.HelpVerb.GetUsage());
                return 1;
            }
        }
    }
}

