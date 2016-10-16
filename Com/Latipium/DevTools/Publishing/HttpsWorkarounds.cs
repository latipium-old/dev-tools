//
// HttpsWorkarounds.cs
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
using System.Text;
using CurlSharp;
using log4net;

namespace Com.Latipium.DevTools.Publishing {
    static class HttpsWorkarounds {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpsWorkarounds));

        private static CurlCode OnSslContext(CurlSslContext ctx, object extraData) {
            return CurlCode.Ok;
        }

        private static bool IsText(byte[] buf, int len) {
            int ascii = 0;
            int nonascii = 0;
            for (int i = 0; i < len; ++i) {
                if (buf[i] >= ' ' && buf[i] <= '~') {
                    ++ascii;
                } else {
                    ++nonascii;
                }
            }
            return ascii > nonascii;
        }

        private static int OnWrite(byte[] buf, int size, int nmemb, object extraData) {
            int len = size * nmemb;
            ((Stream) extraData).Write(buf, 0, len);
            return len;
        }

        private static int OnRead(byte[] buf, int size, int nmemb, object extraData) {
            return ((Stream) extraData).Read(buf, 0, size * nmemb);
        }

        private static void OnDebug(CurlInfoType infoType, string msg, object extraData) {
            if (msg != null) {
                foreach (string line in msg.Trim('\n').Split('\n')) {
                    byte[] data = Encoding.UTF8.GetBytes(line);
                    if (IsText(data, data.Length)) {
                        Log.Debug(line);
                    }
                }
            }
        }

        private static string HttpRequest(string uri, byte[] put, string post) {
            using (CurlEasy easy = new CurlEasy()) {
                easy.ReadFunction = OnRead;
                easy.WriteFunction = OnWrite;
                easy.SslContextFunction = OnSslContext;
                easy.DebugFunction = OnDebug;
                easy.Url = uri;
                easy.SslVerifyPeer = false;
                easy.SetOpt(CurlOption.SslVerifyhost, false);
                if (post != null) {
                    easy.Post = true;
                    easy.PostFieldSize = post.Length;
                    easy.PostFields = post;
                }
                if (put != null) {
                    easy.Put = true;
                    easy.InfileSize = put.Length;
                }
                easy.SetOpt(CurlOption.Verbose, Log.IsDebugEnabled);
                using (MemoryStream write = new MemoryStream()) {
                    easy.WriteData = write;
                    MemoryStream read = null;
                    if (put != null) {
                        read = new MemoryStream(put, false);
                        easy.ReadData = read;
                    }
                    Log.DebugFormat("Performing cURL request to {0}", uri);
                    easy.Perform();
                    if (put != null) {
                        read.Close();
                        read.Dispose();
                    }
                    return Encoding.UTF8.GetString(write.GetBuffer(), 0, (int) write.Position);
                }
            }
        }

        public static string PutZip(string uri, byte[] content) {
            return HttpRequest(uri, content, null);
        }

        public static string PostJson(string uri, string json) {
            return HttpRequest(uri, null, json);
        }

        static HttpsWorkarounds() {
            Curl.GlobalInit(CurlInitFlag.All);
            AppDomain.CurrentDomain.DomainUnload += (sender, e) => Curl.GlobalCleanup();
        }
    }
}

