//
// ReplacementData.cs
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

namespace Com.Latipium.DevTools.Refactoring {
    /// <summary>
    /// A class containing all of the data that gets replaced when refactoring.
    /// </summary>
    public class ReplacementData {
        /// <summary>
        /// The old root namespace.
        /// </summary>
        public string OldNamespace;

        /// <summary>
        /// The new root namespace.
        /// </summary>
        public string NewNamespace;

        /// <summary>
        /// The title of the project.
        /// </summary>
        public string Title;

        /// <summary>
        /// The name of the author.
        /// </summary>
        public string AuthorName;

        /// <summary>
        /// The email address of the author.
        /// </summary>
        public string AuthorEmail;

        /// <summary>
        /// The description of the module.
        /// </summary>
        public string Description;

        /// <summary>
        /// The old GUID.
        /// </summary>
        public Guid OldGuid;

        /// <summary>
        /// The new GUID.
        /// </summary>
        public Guid NewGuid;

        /// <summary>
        /// The license URL.
        /// </summary>
        public string LicenseUrl;

        /// <summary>
        /// The project URL.
        /// </summary>
        public string ProjectUrl;
    }
}

