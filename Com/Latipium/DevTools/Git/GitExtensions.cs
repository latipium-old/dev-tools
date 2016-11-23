//
// GitExtensions.cs
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
using System.Reflection;
using System.Text.RegularExpressions;
using GitSharp;
using log4net;

namespace Com.Latipium.DevTools.Git {
    /// <summary>
    /// Git extension methods.
    /// </summary>
    public static class GitExtensions {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GitExtensions));

        /// <summary>
        /// Opens the git repository from a directory.
        /// </summary>
        /// <returns>The repository.</returns>
        /// <param name="path">The repository path.</param>
        public static Repository OpenRepository(this string path) {
            if (File.Exists(path)) {
                string content = File.ReadAllText(path).Replace("\n", "");
                if (content.StartsWith("gitdir: ")) {
                    string target = content.Split(new char[] { ' ' }, 2)[1];
                    path = Path.IsPathRooted(target) ? target : Path.Combine(path, "..", target);
                }
            }
            return (Repository) typeof(Repository).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] {
                typeof(GitSharp.Core.Repository)
            }, null).Invoke(new object[] {
                new GitSharp.Core.Repository(new DirectoryInfo(path))
            });
        }

        /// <summary>
        /// Gets the GitHub project name.
        /// </summary>
        /// <returns>The GitHub project name.</returns>
        /// <param name="repo">The repository.</param>
        public static string GetGitHubProject(this Repository repo) {
            IEnumerable<string> repos = repo.Config
                .Where(
                    cfg => Regex.IsMatch(cfg.Key, "^remote.[^.]+.url$") &&
                    Regex.IsMatch(cfg.Value, "^(?:(?:https?|ssh)://)?(?:git@)?github(?:\\.com)?[/:]([^/]+/[^/]+)(?:\\.git)?$"))
                .Select(
                    cfg => Regex.Replace(
                        Regex.Replace(cfg.Value, "^(?:(?:https?|ssh)://)?(?:git@)?github(?:\\.com)?[/:]([^/]+/[^/]+)\\.git$", "$1"),
                    "^(?:(?:https?|ssh)://)?(?:git@)?github(?:\\.com)?[/:]([^/]+/[^/]+)$", "$1"))
                .Distinct();
            switch (repos.Count()) {
                case 0:
                    Log.Fatal("Unable to determine GitHub repo url.");
                    Log.Fatal("Try --repository=group/project");
                    return null;
                case 1:
                    return repos.First();
                default:
                    Log.Fatal("Unable to determine GitHub repo url.");
                    Log.Fatal("It seems there are multiple GitHub remotes:");
                    foreach (string remote in repos) {
                        Log.FatalFormat("- {0}", remote);
                    }
                    return null;
            }
        }
    }
}

