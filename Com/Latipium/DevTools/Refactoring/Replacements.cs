//
// Replacements.cs
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace Com.Latipium.DevTools.Refactoring {
    /// <summary>
    /// A class containing the code to do the refactoring
    /// </summary>
    public static class Replacements {
        private static IEnumerable<string> Replace(this IEnumerable<string> e, string find, string replace) {
            return e.Select(s => Regex.Replace(s, find, replace));
        }

        private static XmlDocument Replace(this XmlDocument doc, string tagName, string newValue) {
            doc.GetElementsByTagName(tagName).Cast<XmlElement>().All(e => {
                e.InnerText = newValue;
                return true;
            });
            return doc;
        }

        /// <summary>
        /// Replaces the solution data.
        /// </summary>
        /// <param name="sln">The solution filename.</param>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceSolution(string sln, ReplacementData repl) {
            File.WriteAllLines(sln, File.ReadLines(sln).ToArray()
                               .Replace("^([ \t]*description[ \t]*=[ \t]*).*$", string.Format("$1{0}", repl.Description.Replace("\n", "\\n")))
                               .Replace(repl.OldGuid.ToString("B").ToUpper(), repl.NewGuid.ToString("B").ToUpper())
                               .Replace(string.Format("^(Project\\([^)]+\\)[ \t]*=[ \t]*\"){0}(\",[ \t]*\"){0}(\\.csproj\",[ \t]*\"{1}\")$", repl.OldNamespace, repl.NewGuid.ToString("B").ToUpper()), string.Format("$1{0}$2{0}$3", repl.NewNamespace))
            );
        }

        /// <summary>
        /// Replaces the project data.
        /// </summary>
        /// <param name="csproj">The project filename.</param>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceProject(string csproj, ReplacementData repl) {
            XmlDocument doc = new XmlDocument();
            doc.Load(csproj);
            doc.Replace("ProjectGuid", repl.NewGuid.ToString("B").ToUpper())
                .Replace("RootNamespace", repl.NewNamespace)
                .Replace("AssemblyName", repl.NewNamespace);
            doc.Save(csproj);
        }

        /// <summary>
        /// Replaces the assembly info.
        /// </summary>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceAssemblyInfo(ReplacementData repl) {
            File.WriteAllLines("AssemblyInfo.cs", File.ReadAllLines("AssemblyInfo.cs")
                               .Replace("^(//[ \t]+)[A-Za-z]+ [A-Za-z]+ <[a-zA-Z0-9_-]@[a-zA-Z0-9.]+>$", string.Format("$1{0} <{1}>", repl.AuthorName, repl.AuthorEmail))
                               .Replace("^(//[ \t]+Copyright \\(c\\) [0-9]+ )[A-Za-z]+ [A-Za-z]+$", string.Format("$1{0}", repl.AuthorName))
                               .Replace("(AssemblyTitle[ \t]*\\(\")[^\"]+(\"\\))", string.Format("$1{0}$2", repl.NewNamespace))
                               .Replace("(AssemblyDescription[ \t]*\\(\")[^\"]+(\"\\))", string.Format("$1{0}$2", repl.Description.Replace("\n", "\\n")))
                               .Replace("(AssemblyProduct[ \t]*\\(\")[^\"]+(\"\\))", string.Format("$1{0}$2", repl.Title))
                               .Replace("(AssemblyCopyright[ \t]*\\(\")[^\"]+(\"\\))", string.Format("$1{0}$2", repl.AuthorName))
            );
        }

        /// <summary>
        /// Replaces the nuspec data.
        /// </summary>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceNuspec(ReplacementData repl) {
            XmlDocument doc = new XmlDocument();
            doc.Load("app.nuspec");
            doc.Replace("id", repl.NewNamespace)
                .Replace("title", repl.Title)
                .Replace("authors", repl.AuthorName)
                .Replace("description", repl.Description)
                .Replace("summary", repl.Description)
                .Replace("projectUrl", repl.ProjectUrl)
                .Replace("licenseUrl", repl.LicenseUrl)
                .Replace("copyright", string.Format("Copyright (c) {0} {1}", DateTime.Now.Year, repl.AuthorName))
                .GetElementsByTagName("file").Cast<XmlElement>().Where(e => e.HasAttribute("src")).All(e => {
                e.SetAttribute("src", e.GetAttribute("src").Replace(repl.OldNamespace, repl.NewNamespace));
                return true;
            });
            doc.Save("app.nuspec");
        }

        /// <summary>
        /// Replaces the license data.
        /// </summary>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceLicense(ReplacementData repl) {
            File.WriteAllLines("LICENSE", File.ReadAllLines("LICENSE")
                               .Replace("^([ \t]+Copyright \\(c\\) [0-9]+ )[A-Za-z]+ [A-Za-z]+$", string.Format("$1{0}", repl.AuthorName))
                              );
        }

        /// <summary>
        /// Replaces the readme data.
        /// </summary>
        /// <param name="repl">The replacement data.</param>
        public static void ReplaceReadme(ReplacementData repl) {
            IEnumerable<string> readme = File.ReadAllLines("README.md");
            if (readme.First().StartsWith("# ")) {
                readme = new string[] {
                    string.Format("# {0}", repl.Title)
                }.Concat(readme.Skip(1));
            }
            File.WriteAllLines("README.md", readme);
        }

        /// <summary>
        /// Renames the solution and project files.
        /// </summary>
        /// <param name="sln">The solution filename.</param>
        /// <param name="csproj">The project filename.</param>
        /// <param name="repl">The replacement data.</param>
        public static void RenameFiles(string sln, string csproj, ReplacementData repl) {
            if (Path.GetFileNameWithoutExtension(sln) == repl.OldNamespace) {
                File.Move(sln, string.Format("{0}.sln", repl.NewNamespace));
            }
            if (Path.GetFileNameWithoutExtension(csproj) == repl.OldNamespace) {
                File.Move(csproj, string.Format("{0}.csproj", repl.NewNamespace));
            }
        }
    }
}

