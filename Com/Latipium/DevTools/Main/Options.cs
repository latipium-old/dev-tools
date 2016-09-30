//
// Options.cs
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
using System.Reflection;
using CommandLine;
using CommandLine.Text;

namespace Com.Latipium.DevTools.Main {
    public class Options {
        [VerbOption("help", HelpText="Shows help for a command")]
        public HelpVerb HelpVerb {
            get;
            set;
        }

        [VerbOption("version", HelpText="Calculates versioning information from Git")]
        public CalculateVersionVerb VersionVerb {
            get;
            set;
        }

        [VerbOption("package", HelpText="Creates a package file from the code")]
        public CreatePackageVerb PackageVerb {
            get;
            set;
        }

        [VerbOption("publish", HelpText="Uploads a package file to the server")]
        public PublishVerb PublishVerb {
            get;
            set;
        }

        [VerbOption("authorize", HelpText="Authorize a CI build using your Latipium account")]
        public AuthorizeCIVerb AuthorizeVerb {
            get;
            set;
        }

        public Options() {
            HelpVerb = new HelpVerb();
            VersionVerb = new CalculateVersionVerb();
            PackageVerb = new CreatePackageVerb();
            PublishVerb = new PublishVerb();
            AuthorizeVerb = new AuthorizeCIVerb();
        }
    }
}

