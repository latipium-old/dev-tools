//
// Authorizer.cs
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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using GitSharp;
using log4net;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using YamlDotNet.Serialization;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Authorizing {
    /// <summary>
    /// The implementation of the authorize command.
    /// </summary>
    public static class Authorizer {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Authorizer));

        private static string GetRepo(AuthorizeCIVerb verb) {
            if (verb.ProjectId != null) {
                return verb.ProjectId;
            }
            if (File.Exists(verb.GitDir)) {
                string content = File.ReadAllText(verb.GitDir).Replace("\n", "");
                if (content.StartsWith("gitdir: ")) {
                    verb.GitDir = content.Split(new char[] { ' ' }, 2)[1];
                }
            }
            Repository repo = new Repository(verb.GitDir);
            IEnumerable<string> repos = repo.Config
                .Where(
                    cfg => Regex.IsMatch(cfg.Key, "^remote.[^.]+.url$") &&
                    Regex.IsMatch(cfg.Value, "^(?:(?:https?|ssh)://)?(?:git@)?github(?:\\.com)?[/:]([^/]+/[^/]+)(?:\\.git)?$"))
                .Select(
                    cfg => Regex.Replace(cfg.Value, "^(?:(?:https?|ssh)://)?(?:git@)?github(?:\\.com)?[/:]([^/]+/[^/]+)(?:\\.git)?$", "$1"))
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

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="verb">The command verb.</param>
        public static void Handle(AuthorizeCIVerb verb) {
            string repo = GetRepo(verb);
            if (repo == null) {
                return;
            }
            Log.DebugFormat("Detected repository {0}", repo);
            HttpWebRequest req = WebRequest.CreateHttp(string.Format("https://api.travis-ci.org/repos/{0}/key", repo));
            req.UserAgent = "Latipium DevTools (https://github.com/latipium/dev-tools)";
            req.Accept = "application/vnd.travis-ci.2+json";
            TravisPublicKey key;
            using (WebResponse res = req.GetResponse()) {
                using (Stream stream = res.GetResponseStream()) {
                    using (StreamReader reader = new StreamReader(stream)) {
                        key = JsonConvert.DeserializeObject<TravisPublicKey>(reader.ReadToEnd());
                    }
                }
            }
            Log.DebugFormat("Downloaded public key:\n{0}", key.key);
            Log.DebugFormat("Key fingerprint: {0}", key.fingerprint);
            Pkcs1Encoding engine = new Pkcs1Encoding(new RsaEngine());
            AsymmetricKeyParameter keyParam;
            using (StringReader reader = new StringReader(key.key)) {
                keyParam = (AsymmetricKeyParameter) new PemReader(reader).ReadObject();
            }
            engine.Init(true, keyParam);
            byte[] apiKey = Encoding.UTF8.GetBytes(string.Format("LATIPIUM_NUGET_KEY={0}", verb.ApiKey));
            string encKey = Convert.ToBase64String(engine.ProcessBlock(apiKey, 0, apiKey.Length));
            Deserializer deser = new Deserializer();
            Dictionary<object, object> yaml;
            using (Stream stream = new FileStream(".travis.yml", FileMode.OpenOrCreate, FileAccess.Read)) {
                using (StreamReader reader = new StreamReader(stream)) {
                    yaml = (Dictionary<object, object>) deser.Deserialize(reader);
                }
            }
            if (yaml == null) {
                yaml = new Dictionary<object, object>();
            }
            Dictionary<object, object> env;
            if (yaml.ContainsKey("env")) {
                env = (Dictionary<object, object>) yaml["env"];
            } else {
                env = new Dictionary<object, object>();
                yaml["env"] = env;
            }
            env["secure"] = encKey;
            Serializer ser = new Serializer();
            using (Stream stream = new FileStream(".travis.yml", FileMode.Truncate, FileAccess.Write)) {
                using (StreamWriter writer = new StreamWriter(stream)) {
                    ser.Serialize(writer, yaml);
                }
            }
        }
    }
}

