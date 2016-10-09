//
// Authorizer.cs
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
using System.Linq;
using log4net;
using Newtonsoft.Json;
using Com.Latipium.DevTools.Apis;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Authorizing {
    public static class Authorizer {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Authorizer));

        public static void Handle(AuthorizeCIVerb verb) {
            ApiResponse response = new ApiQuery[] {
                new ApiQuery("createCIToken", verb.Namespace)
            }.Execute();
            if (response.Successful.Count == 1) {
                string token = response.Results.First();
                Log.DebugFormat("CI token is {0}", token);
                if (verb.Travis) {
                    try {
                        File.WriteAllText(".latipium-ci-token", token);
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.Arguments = "encrypt-file .latipium-ci-token --add";
                        psi.FileName = "travis";
                        using (Process proc = Process.Start(psi)) {
                            proc.WaitForExit();
                            if (proc.ExitCode != 0) {
                                Log.FatalFormat("Travis CI Command Line Client returned error code {0}", proc.ExitCode);
                            }
                        }
                    } finally {
                        File.Delete(".latipium-ci-token");
                    }
                } else {
                    Console.WriteLine(token);
                }
            } else {
                Log.Error("API returned error code");
                if (Log.IsDebugEnabled) {
                    Log.DebugFormat("Response was {0}", JsonConvert.SerializeObject(response));
                }
            }
        }
    }
}

