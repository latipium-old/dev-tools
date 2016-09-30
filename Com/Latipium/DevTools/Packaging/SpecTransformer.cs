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
    public class SpecTransformer {
        public readonly string Filename;
        public readonly RunConfiguration Config;

        public string PreparsedText {
            get {
                using (TextReader reader = new StreamReader(Filename)) {
                    return reader.ReadToEnd();
                }
            }
        }

        public string ConfiguredText {
            get {
                return Regex.Replace(PreparsedText, "{{[ \t]*configuration[ \t]*}}", Config.ToString());
            }
        }

        public XmlDocument ConfiguredDocument {
            get {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(ConfiguredText);
                return doc;
            }
        }

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

        public string ParsedText {
            get {
                return Regex.Replace(ConfiguredText, "{{[ \t]*version[ \t]*}}", ConfiguredVersion.ToString());
            }
        }

        public Stream ParsedStream {
            get {
                return new MemoryStream(Encoding.UTF8.GetBytes(ParsedText));
            }
        }

        public SpecTransformer(string file, RunConfiguration config) {
            Filename = file;
            Config = config;
        }
    }
}

