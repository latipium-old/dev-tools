//
// HelpVerb.cs
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
    public class HelpVerb : CommonOptions {
        [ValueOption(0)]
        public string VerbName {
            get;
            set;
        }

        private CommonOptions FindVerb() {
            return (CommonOptions) typeof(Options).GetProperties()
                .First(
                    p => p.GetCustomAttributes(typeof(VerbOptionAttribute), false)
                    .Cast<VerbOptionAttribute>()
                    .Any(
                        a => a.LongName == VerbName))
                .GetValue(Entry.RootOptions, null);
        }

        public string GetUsage() {
            HelpText help = new HelpText();
            help.AddDashesToOption = true;
            help.Copyright = "Copyright (c) 2016 Zach Deibert";
            help.Heading = string.Format("Latipium Development Tools ({0})", Assembly.GetExecutingAssembly().GetName().Version);
            help.MaximumDisplayWidth = 120;
            if (string.IsNullOrEmpty(VerbName)) {
                help.AddPreOptionsLine("Usage: Com.Latipium.DevTools <command> [option [option ...]]");
                help.AddOptions(new CommonOptions());
            } else {
                try {
                    CommonOptions verb = FindVerb();
                    help.AddPreOptionsLine(string.Format("Usage: Com.Latipium.DevTools {0} {1}", VerbName, verb.CommandLineFormat));
                    help.AddOptions(verb);
                } catch (InvalidOperationException) {
                    help.AddDashesToOption = false;
                    help.AddPreOptionsLine("Usage: Com.Latipium.DevTools help [command]");
                    help.AddPreOptionsLine("Commands:");
                    help.AddOptions(Entry.RootOptions);
                }
            }
            return help;
        }

        public HelpVerb() {
            CommandLineFormat = "[command]";
        }
    }
}

