//
// ApiPlatform.cs
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

namespace Com.Latipium.DevTools.Apis {
    public static class ApiPlatform {
        public const string ElectronVersion = "v1.4.2";
        public static readonly string PlatformName;
        public static readonly string PlatformArchitecture;
        public static readonly string DownloadUrl;
        public static readonly string ResourcesDir;
        public static readonly string Executable;
        public static readonly bool ExecutableBitNeeded;
        public static readonly string VersionId;

        static ApiPlatform() {
            try {
                switch (Environment.OSVersion.Platform) {
                    case PlatformID.Win32NT:
                    case PlatformID.Win32S:
                    case PlatformID.Win32Windows:
                    case PlatformID.WinCE:
                    case PlatformID.Xbox:
                        PlatformName = "win32";
                        ResourcesDir = "resources\\app";
                        Executable = "electron.exe";
                        ExecutableBitNeeded = false;
                        break;
                    case PlatformID.MacOSX:
                        PlatformName = "darwin";
                        ResourcesDir = "Electron.app/Contents/Resources/app";
                        Executable = "Electron.app/Contents/MacOS/electron";
                        ExecutableBitNeeded = true;
                        break;
                    default:
                        PlatformName = "linux";
                        ResourcesDir = "resources/app";
                        Executable = "electron";
                        ExecutableBitNeeded = true;
                        ProcessStartInfo psi = new ProcessStartInfo();
                        psi.Arguments = "-m";
                        psi.FileName = "/bin/uname";
                        psi.RedirectStandardOutput = true;
                        psi.UseShellExecute = false;
                        using (Process proc = Process.Start(psi)) {
                            PlatformArchitecture = proc.StandardOutput.ReadLine().Contains("arm") ? "arm" : (Environment.Is64BitOperatingSystem ? "x64" : "ia32");
                            proc.WaitForExit();
                        }
                        return;
                }
                PlatformArchitecture = Environment.Is64BitOperatingSystem ? "x64" : "ia32";
            } finally {
                VersionId = string.Format("electron-{0}-{1}-{2}", ElectronVersion, PlatformName, PlatformArchitecture);
                DownloadUrl = string.Format("https://github.com/electron/electron/releases/download/{0}/{1}.zip", ElectronVersion, VersionId);
            }
        }
    }
}

