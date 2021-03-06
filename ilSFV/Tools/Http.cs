using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace ilSFV
{
    /// <summary>
    /// Methods for HTTP
    /// </summary>
    public static class Http
    {
        private const string m_UserAgent = @"Mozilla/5.0 (Windows; U; Windows NT 6.0; en-US; rv:1.9.0.9) Gecko/2009040821 Firefox/3.0.9 (.NET CLR 3.5.30729)";

        private class RequestAndData
        {
            public HttpWebRequest Request;
            public byte[] Data;
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, IEnumerable<PostData> postData)
        {
            return Get(requestUriString, postData, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, IEnumerable<PostData> postData, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            return Get(GetQueryString(requestUriString, postData).AbsoluteUri, proxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString)
        {
            return Get(requestUriString, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, IWebProxy proxy)
        {
            return Get(requestUriString, (CookieContainer)null, proxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="cookies">The cookies.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, CookieContainer cookies)
        {
            return Get(requestUriString, cookies, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, CookieContainer cookies, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(proxy, "proxy");

            HttpWebResponse webResponse;
            return Get(requestUriString, cookies, out webResponse, proxy);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="webResponse">The web response.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, CookieContainer cookies, out HttpWebResponse webResponse)
        {
            return Get(requestUriString, cookies, out webResponse);
        }

        /// <summary>
        /// GETs a byte array of data from the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="webResponse">The web response.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>A byte array of data from the specified request URI string.</returns>
        public static byte[] Get(string requestUriString, CookieContainer cookies, out HttpWebResponse webResponse, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(proxy, "proxy");

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUriString);
            webRequest.Proxy = proxy;
            webRequest.CookieContainer = cookies;
            webRequest.UserAgent = m_UserAgent;
            webResponse = (HttpWebResponse)webRequest.GetResponse();

            byte[] data = new byte[256];
            using (Stream responseStream = webResponse.GetResponseStream())
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int read;
                    do
                    {
                        read = responseStream.Read(data, 0, 256);
                        memoryStream.Write(data, 0, read);
                    } while (read > 0);

                    data = memoryStream.ToArray();
                }
            }

            return data;
        }

        /// <summary>
        /// POSTs to the specified request URI string asynchronously.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public static IAsyncResult PostAsync(string requestUriString, IEnumerable<PostData> postData)
        {
            return PostAsync(requestUriString, postData, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string asynchronously.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="proxy">The proxy.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous request.</returns>
        public static IAsyncResult PostAsync(string requestUriString, IEnumerable<PostData> postData, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            byte[] data;
            HttpWebRequest webRequest = GetPostRequest(requestUriString, postData, out data, proxy);

            RequestAndData requestAndData = new RequestAndData { Request = webRequest, Data = data };

            return webRequest.BeginGetRequestStream(GetRequestStreamCallback, requestAndData);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] Post(string requestUriString, IEnumerable<PostData> postData, out CookieContainer cookies)
        {
            return Post(requestUriString, postData, out cookies, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="proxy">The proxy.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] Post(string requestUriString, IEnumerable<PostData> postData, out CookieContainer cookies, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            Uri uri = new Uri(requestUriString);
            return Post(uri, postData, out cookies, proxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] PostCookies(string requestUriString, IEnumerable<PostData> postData, CookieContainer cookies)
        {
            return PostCookies(requestUriString, postData, cookies, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="proxy">The proxy.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] PostCookies(string requestUriString, IEnumerable<PostData> postData, CookieContainer cookies, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            Uri uri = new Uri(requestUriString);
            return PostCookies(uri, postData, cookies, proxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        public static byte[] Post(string requestUriString, IEnumerable<PostData> postData)
        {
            return Post(requestUriString, postData, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// POSTs to the specified request URI string.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="proxy">The proxy.</param>
        public static byte[] Post(string requestUriString, IEnumerable<PostData> postData, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            Uri uri = new Uri(requestUriString);
            CookieContainer cookies;
            return Post(uri, postData, out cookies, proxy);
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        public static byte[] Post(Uri requestUri)
        {
            return Post(requestUri, WebRequest.DefaultWebProxy);
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="proxy">The proxy.</param>
        public static byte[] Post(Uri requestUri, IWebProxy proxy)
        {
            Guard.ArgumentNotNull(requestUri, "requestUri");
            Guard.ArgumentNotNull(proxy, "proxy");

            CookieContainer cookies;
            return Post(requestUri, null, out cookies, proxy);
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] Post(Uri requestUri, IEnumerable<PostData> postData, out CookieContainer cookies)
        {
            return Post(requestUri, postData, out cookies);
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="proxy">The proxy.</param>
        public static byte[] Post(Uri requestUri, IEnumerable<PostData> postData, out CookieContainer cookies, IWebProxy proxy)
        {
            Guard.ArgumentNotNull(requestUri, "requestUri");
            Guard.ArgumentNotNull(proxy, "proxy");

            byte[] data;
            HttpWebRequest webRequest = GetPostRequest(requestUri, postData, out data, proxy);

            using (Stream postStream = webRequest.GetRequestStream())
            {
                postStream.Write(data, 0, data.Length);
            }

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            using (Stream responseStream = webResponse.GetResponseStream())
            {
                data = new byte[256];
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int read;
                    do
                    {
                        read = responseStream.Read(data, 0, 256);
                        memoryStream.Write(data, 0, read);
                    } while (read > 0);

                    data = memoryStream.ToArray();
                }
            }

            cookies = webRequest.CookieContainer;

            return data;
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        public static byte[] PostCookies(Uri requestUri, IEnumerable<PostData> postData, CookieContainer cookies)
        {
            return PostCookies(requestUri, postData, cookies);
        }

        /// <summary>
        /// POSTs to the specified request URI.
        /// </summary>
        /// <param name="requestUri">The request URI.</param>
        /// <param name="postData">The post data.</param>
        /// <param name="cookies">The cookies.</param>
        /// <param name="proxy">The proxy.</param>
        public static byte[] PostCookies(Uri requestUri, IEnumerable<PostData> postData, CookieContainer cookies, IWebProxy proxy)
        {
            Guard.ArgumentNotNull(requestUri, "requestUri");
            Guard.ArgumentNotNull(proxy, "proxy");

            byte[] data;
            HttpWebRequest webRequest = GetPostRequest(requestUri, postData, out data, proxy);
            webRequest.CookieContainer = cookies;

            using (Stream postStream = webRequest.GetRequestStream())
            {
                postStream.Write(data, 0, data.Length);
            }

            HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse();

            using (Stream responseStream = webResponse.GetResponseStream())
            {
                data = new byte[256];
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    int read;
                    do
                    {
                        read = responseStream.Read(data, 0, 256);
                        memoryStream.Write(data, 0, read);
                    } while (read > 0);

                    data = memoryStream.ToArray();
                }
            }

            return data;
        }

        /// <summary>
        /// Gets the full URI including GET/POST arguments.
        /// </summary>
        /// <param name="requestUriString">The request URI string.</param>
        /// <param name="postData">The post data.</param>
        public static Uri GetQueryString(string requestUriString, IEnumerable<PostData> postData)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");

            StringBuilder getString = new StringBuilder();
            foreach (PostData item in postData)
                AddField(getString, item);

            if (!requestUriString.Contains("?"))
                getString.Insert(0, '?');
            else
                getString.Insert(0, '&');

            return new Uri(string.Format("{0}{1}", requestUriString, getString));
        }

        private static void AddField(StringBuilder postString, PostData item)
        {
            Guard.ArgumentNotNull(postString, "postString");
            Guard.ArgumentNotNull(item, "item");
            Guard.ArgumentNotNull(item.Value, "item.Value");

            if (postString.Length == 0)
                postString.AppendFormat("{0}={1}", item.Field, item.Value.Replace("&", "%26"));
            else
                postString.AppendFormat("&{0}={1}", item.Field, item.Value.Replace("&", "%26"));
        }

        private static HttpWebRequest GetPostRequest(string requestUriString, IEnumerable<PostData> postData, out byte[] data, IWebProxy proxy)
        {
            Guard.ArgumentNotNullOrEmptyString(requestUriString, "requestUriString");
            Guard.ArgumentNotNull(postData, "postData");
            Guard.ArgumentNotNull(proxy, "proxy");

            Uri uri = new Uri(requestUriString);
            return GetPostRequest(uri, postData, out data, proxy);
        }

        private static HttpWebRequest GetPostRequest(Uri requestUri, IEnumerable<PostData> postData, out byte[] data, IWebProxy proxy)
        {
            Guard.ArgumentNotNull(requestUri, "requestUri");
            Guard.ArgumentNotNull(proxy, "proxy");

            StringBuilder getString = new StringBuilder();
            foreach (PostData item in postData)
                AddField(getString, item);

            data = Encoding.ASCII.GetBytes(getString.ToString());

            // Prepare web request...
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(requestUri);
            webRequest.AllowAutoRedirect = true;
            webRequest.Proxy = proxy;
            webRequest.Method = "POST";
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = data.Length;
            webRequest.CookieContainer = new CookieContainer();
            webRequest.UserAgent = m_UserAgent;

            return webRequest;
        }

        private static void GetRequestStreamCallback(IAsyncResult asyncResult)
        {
            RequestAndData requestAndData = (RequestAndData)asyncResult.AsyncState;
            HttpWebRequest webRequest = requestAndData.Request;
            byte[] data = requestAndData.Data;

            using (Stream postStream = webRequest.EndGetRequestStream(asyncResult))
            {
                postStream.Write(data, 0, data.Length);
            }
        }

        /// <summary>
        /// Gets the file name from headers.
        /// </summary>
        /// <param name="headers">The headers.</param>
        /// <returns>The file name.</returns>
        public static string GetFileNameFromHeaders(WebHeaderCollection headers)
        {
            Guard.ArgumentNotNull(headers, "headers");

            string[] keys = headers.AllKeys;
            if (keys == null || keys.Length == 0)
                throw new Exception("No keys found.");

            List<string> keyList = new List<string>(keys);
            if (keyList.Contains("Content-disposition"))
            {
                string contentDisposition = headers["Content-disposition"];

                const string filename = "filename=\"";
                int idx = contentDisposition.IndexOf(filename);
                idx = idx + filename.Length;
                string fn = contentDisposition.Substring(idx, contentDisposition.Length - idx - 1);

                return fn;
            }

            throw new Exception("'Content-disposition' header not found.");
        }
    }
}