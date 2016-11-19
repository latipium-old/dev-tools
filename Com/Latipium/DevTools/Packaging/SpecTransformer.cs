//
// SpecTransformer.cs
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
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Com.Latipium.DevTools.Model;

namespace Com.Latipium.DevTools.Packaging {
    /// <summary>
    /// Transforms the specification file.
    /// </summary>
    public class SpecTransformer {
        /// <summary>
        /// The filename.
        /// </summary>
        public readonly string Filename;
        /// <summary>
        /// The configuration to run in.
        /// </summary>
        public readonly RunConfiguration Config;

        /// <summary>
        /// Gets the preparsed text.
        /// </summary>
        /// <value>The preparsed text.</value>
        public string PreparsedText {
            get {
                using (TextReader reader = new StreamReader(Filename)) {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Gets the configured text.
        /// </summary>
        /// <value>The configured text.</value>
        public string ConfiguredText {
            get {
                return Regex.Replace(PreparsedText, "{{[ \t]*configuration[ \t]*}}", Config.ToString());
            }
        }

        /// <summary>
        /// Gets the configured document.
        /// </summary>
        /// <value>The configured document.</value>
        public XmlDocument ConfiguredDocument {
            get {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ConfiguredText);
                return doc;
            }
        }

        /// <summary>
        /// Gets the configured version.
        /// </summary>
        /// <value>The configured version.</value>
        public Version ConfiguredVersion {
            get {
                return Assembly.LoadFile(
                    ConfiguredDocument.GetElementsByTagName("file", "http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd")
                    .Cast<XmlElement>()
                    .Select(
                        e => e.GetAttribute("src"))
                    .First(
                        s => s.EndsWith(".dll") || s.EndsWith(".exe")))
                        .GetName()
                        .Version;
            }
        }

        /// <summary>
        /// Gets the parsed text.
        /// </summary>
        /// <value>The parsed text.</value>
        public string ParsedText {
            get {
                return Regex.Replace(ConfiguredText, "{{[ \t]*version[ \t]*}}", ConfiguredVersion.ToString());
            }
        }

        /// <summary>
        /// Gets the parsed stream.
        /// </summary>
        /// <value>The parsed stream.</value>
        public Stream ParsedStream {
            get {
                return new MemoryStream(Encoding.UTF8.GetBytes(ParsedText));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Com.Latipium.DevTools.Packaging.SpecTransformer"/> class.
        /// </summary>
        /// <param name="file">The file.</param>
        /// <param name="config">The run configuration.</param>
        public SpecTransformer(string file, RunConfiguration config) {
            Filename = file;
            Config = config;
        }
    }
}

