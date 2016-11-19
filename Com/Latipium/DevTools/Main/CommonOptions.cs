//
// CommonOptions.cs
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
using CommandLine;

namespace Com.Latipium.DevTools.Main {
    /// <summary>
    /// The options common to all commands.
    /// </summary>
    public class CommonOptions {
        /// <summary>
        /// Gets or sets the command line format of the options.
        /// </summary>
        /// <value>The command line format.</value>
        public string CommandLineFormat {
            get;
            protected set;
        }

        /// <summary>
        /// Gets or sets the working directory.
        /// </summary>
        /// <value>The working directory.</value>
        [Option('C', "cwd", HelpText="Sets the directory to run in")]
        public string WorkingDirectory {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this application should run in verbose mode.
        /// </summary>
        /// <value><c>true</c> if it should be in verbose mode; otherwise, <c>false</c>.</value>
        [Option('v', "verbose", HelpText="Logs extra information to the console")]
        public bool VerboseMode {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Com.Latipium.DevTools.Main.CommonOptions"/> class.
        /// </summary>
        public CommonOptions() {
            CommandLineFormat = "[option [option ...]]";
            WorkingDirectory = ".";
        }
    }
}

