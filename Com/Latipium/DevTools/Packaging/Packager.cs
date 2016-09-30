//
// Packager.cs
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
using System.Linq;
using log4net;
using NuGet;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Packaging {
    public static class Packager {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Packager));

        private static void PreprocessVerb(CreatePackageVerb verb) {
            if (string.IsNullOrEmpty(verb.SpecFile)) {
                verb.SpecFile = Directory.EnumerateFiles(".", "*.nuspec").First();
            }
            Directory.CreateDirectory(verb.OutputDirectory);
        }

        public static void Handle(CreatePackageVerb verb) {
            PreprocessVerb(verb);
            SpecTransformer spec = new SpecTransformer(verb.SpecFile, verb.Configuration);
            Manifest manifest = Manifest.ReadFrom(spec.ParsedStream, true);
            Log.DebugFormat("Building package {0} version {1}", manifest.Metadata.Id, manifest.Metadata.Version);
            PackageBuilder builder = new PackageBuilder();
            builder.Populate(manifest.Metadata);
            builder.PopulateFiles(".", manifest.Files);
            string filename = Path.Combine(verb.OutputDirectory, string.Format("{0}.{1}.nupkg", builder.Id, builder.Version));
            Log.DebugFormat("Saving package to {0}", filename);
            using (Stream stream = new FileStream(filename, FileMode.CreateNew)) {
                builder.Save(stream);
            }
        }
    }
}

