//
// Publisher.cs
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
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using Newtonsoft.Json;
using Com.Latipium.DevTools.Apis;
using Com.Latipium.DevTools.Main;

namespace Com.Latipium.DevTools.Publishing {
    public static class Publisher {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Publisher));

        private static string Upload(string filename) {
            /* // Mono doesn't support elliptic curve cryptography
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> res = client.PutAsync("https://apis.latipium.com/upload", new ByteArrayContent(File.ReadAllBytes(filename)));
            res.Wait();
            if (!res.Result.IsSuccessStatusCode) {
                Log.FatalFormat("Server returned fatal error {0}", res.Result.ReasonPhrase);
                return null;
            }
            Task<string> str = res.Result.Content.ReadAsStringAsync();
            str.Wait();
            return str.Result;
            */
            return HttpsWorkarounds.PutZip("https://apis.latipium.com/upload", File.ReadAllBytes(filename));
        }

        private static string CreateQuery(string uploadToken) {
            return JsonConvert.SerializeObject(new ApiRequest(
                new ApiQuery("publishModule", uploadToken)
            ));
        }

        private static void RunQuery(string ciToken, string query) {
            /* // Mono doesn't support elliptic curve cryptography
            HttpClient client = new HttpClient();
            Task<HttpResponseMessage> res = client.PostAsync(string.Format("https://apis.latipium.com/ci/{0}", ciToken), new StringContent(query));
            res.Wait();
            if (!res.Result.IsSuccessStatusCode) {
                Log.FatalFormat("Server returned fatal error {0}", res.Result.ReasonPhrase);
            }
            Task<string> str = res.Result.Content.ReadAsStringAsync();
            ApiResponse apiRes = JsonConvert.DeserializeObject<ApiResponse>(str.Result);
            */
            string str = HttpsWorkarounds.PostJson(string.Format("https://apis.latipium.com/ci/{0}", ciToken), query);
            ApiResponse apiRes = JsonConvert.DeserializeObject<ApiResponse>(str);
            // End of workaround code
            if (apiRes.Successful.Count == 1) {
                Log.Info("Upload successful");
            } else {
                Log.Fatal("Upload failed!");
            }
        }

        public static void Handle(PublishVerb verb) {
            string ciToken;
            if (verb.TravisFile) {
                ciToken = File.ReadAllText(".latipium-ci-token");
            } else {
                ciToken = verb.CIToken;
            }
            string uploadToken = Upload(verb.FileName);
            if (uploadToken != null) {
                string query = CreateQuery(uploadToken);
                RunQuery(ciToken, query);
            }
        }
    }
}

