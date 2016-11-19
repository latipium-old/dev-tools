//
// CalculateVersionVerb.cs
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
using System.Collections.Generic;
using CommandLine;

namespace Com.Latipium.DevTools.Main {
    /// <summary>
    /// The command verb for the version command.
    /// </summary>
    public class CalculateVersionVerb : CommonOptions {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        /// <value>The files.</value>
        [ValueList(typeof(List<string>))]
        public IList<string> Files {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the git directory.
        /// </summary>
        /// <value>The git directory.</value>
        [Option('g', "gitDir", HelpText="The directory of the git repository")]
        public string GitDir {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Com.Latipium.DevTools.Main.CalculateVersionVerb"/> class.
        /// </summary>
        public CalculateVersionVerb() {
            CommandLineFormat = "[option [option ...]] [file [file ...]]";
            GitDir = ".git";
        }
    }
}

