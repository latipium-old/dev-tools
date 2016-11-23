//
// Refactorer.cs
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
using System.Net;
using System.Xml;
using GitSharp;
using log4net;
using Newtonsoft.Json;
using Com.Latipium.DevTools.Git;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Refactoring {
    /// <summary>
    /// The implementation of the refactor command.
    /// </summary>
    public static class Refactorer {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Refactorer));

        private static string GetSolution() {
            string[] solutions = Directory.GetFiles(".", "*.sln");
            switch (solutions.Length) {
                case 0:
                    Log.Fatal("No solution files found!");
                    return null;
                case 1:
                    return solutions[0];
                default:
                    Log.Fatal("Multiple solution files found (there should only be 1!):");
                    foreach (string sln in solutions) {
                        Log.FatalFormat("- {0}", sln);
                    }
                    return null;
            }
        }

        private static string GetProject() {
            string[] projects = Directory.GetFiles(".", "*.csproj");
            switch (projects.Length) {
                case 0:
                    Log.Fatal("No project files found!");
                    return null;
                case 1:
                    return projects[0];
                default:
                    Log.Fatal("Multiple project files found (there should only be 1!):");
                    foreach (string proj in projects) {
                        Log.FatalFormat("- {0}", proj);
                    }
                    return null;
            }
        }

        private static string GetRepo(RefactorVerb verb) {
            if (verb.ProjectId != null) {
                return verb.ProjectId;
            }
            return verb.GitDir.OpenRepository().GetGitHubProject();
        }

        private static UserResponse GetUser(string userId) {
            HttpWebRequest req = WebRequest.CreateHttp(string.Format("https://api.github.com/users/{0}", userId));
            req.UserAgent = "Latipium DevTools (https://github.com/latipium/dev-tools)";
            req.Accept = "application/vnd.github.v3+json";
            using (WebResponse res = req.GetResponse()) {
                using (Stream stream = res.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        return JsonConvert.DeserializeObject<UserResponse>(reader.ReadToEnd());
                    }
                }
            }
        }

        private static RepoResponse GetRepo(string repoId) {
            HttpWebRequest req = WebRequest.CreateHttp(string.Format("https://api.github.com/repos/{0}", repoId));
            req.UserAgent = "Latipium DevTools (https://github.com/latipium/dev-tools)";
            req.Accept = "application/vnd.github.v3+json";
            using (WebResponse res = req.GetResponse()) {
                using (Stream stream = res.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        return JsonConvert.DeserializeObject<RepoResponse>(reader.ReadToEnd());
                    }
                }
            }
        }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="verb">The command verb.</param>
        public static void Handle(RefactorVerb verb) {
            string sln = GetSolution();
            string csproj = GetProject();
            string fullRepoId = GetRepo(verb);
            if (fullRepoId != null) {
                string[] repoId = fullRepoId.Split(new char[] { '/' }, 2);
                Log.DebugFormat("Detected repository {0}", fullRepoId);
                UserResponse author = GetUser(repoId[0]);
                RepoResponse repo = GetRepo(fullRepoId);
                ReplacementData repl = new ReplacementData();
                repl.NewNamespace = verb.Namespace;
                repl.Title = verb.Title;
                repl.AuthorName = author.name;
                repl.AuthorEmail = author.email;
                repl.Description = repo.description;
                XmlDocument doc = new XmlDocument();
                doc.Load(csproj);
                repl.OldNamespace = doc.GetElementsByTagName("RootNamespace").Cast<XmlElement>().First().InnerText;
                repl.OldGuid = Guid.Parse(doc.GetElementsByTagName("ProjectGuid").Cast<XmlElement>().First().InnerText);
                repl.NewGuid = Guid.NewGuid();
                repl.LicenseUrl = string.Format("{0}/blob/master/LICENSE", repo.html_url);
                repl.ProjectUrl = repo.homepage == null ? repo.html_url : repo.homepage;
                if (Log.IsDebugEnabled) {
                    Log.DebugFormat("Replacement data for {0}, {1}, AssemblyInfo.cs, app.nuspec, LICENSE, and README.md:", sln, csproj);
                    Log.DebugFormat("- Namespace:    {0} => {1}", repl.OldNamespace, repl.NewNamespace);
                    Log.DebugFormat("- Title:        {0}", repl.Title);
                    Log.DebugFormat("- Author Name:  {0}", repl.AuthorName);
                    Log.DebugFormat("- Author Email: {0}", repl.AuthorEmail);
                    Log.DebugFormat("- Description:  {0}", repl.Description);
                    Log.DebugFormat("- GUID:         {0} => {1}", repl.OldGuid, repl.NewGuid);
                    Log.DebugFormat("- Project URL:  {0}", repl.ProjectUrl);
                    Log.DebugFormat("- License URL:  {0}", repl.LicenseUrl);
                }
                Replacements.ReplaceSolution(sln, repl);
                Replacements.ReplaceProject(csproj, repl);
                Replacements.ReplaceAssemblyInfo(repl);
                Replacements.ReplaceNuspec(repl);
                Replacements.ReplaceLicense(repl);
                Replacements.ReplaceReadme(repl);
                Replacements.RenameFiles(sln, csproj, repl);
            }
        }
    }
}

