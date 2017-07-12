using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web; 
using System.Web.Mvc;

namespace ShieldPortal.Models
{

    public class CompressAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {

            var encodingsAccepted = filterContext.HttpContext.Request.Headers["Accept-Encoding"];
            if (string.IsNullOrEmpty(encodingsAccepted)) return;

            encodingsAccepted = encodingsAccepted.ToLowerInvariant();
            var response = filterContext.HttpContext.Response;

            if (encodingsAccepted.Contains("deflate"))
            {
                response.AppendHeader("Content-encoding", "deflate");
                response.Filter = new DeflateStream(response.Filter, CompressionMode.Compress);
            }
            else if (encodingsAccepted.Contains("gzip"))
            {
                response.AppendHeader("Content-encoding", "gzip");
                response.Filter = new GZipStream(response.Filter, CompressionMode.Compress);
            }
        }
    }

    public class CompressedContent : HttpContent
    {
        private HttpContent originalContent;
        private string encodingType;

        public CompressedContent(HttpContent content, string encodingType)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (encodingType == null)
            {
                throw new ArgumentNullException("encodingType");
            }

            originalContent = content;
            originalContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
            this.encodingType = encodingType.ToLowerInvariant();

            if (this.encodingType != "gzip" && this.encodingType != "deflate")
            {
                throw new InvalidOperationException(string.Format("Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this.encodingType));
            }

            // copy the headers from the original content
            foreach (KeyValuePair<string, IEnumerable<string>> header in originalContent.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            if (encodingType == "gzip")
            {
                compressedStream = new GZipStream(stream, CompressionMode.Compress, leaveOpen: true);
            }
            else if (encodingType == "deflate")
            {
                compressedStream = new DeflateStream(stream, CompressionMode.Compress, leaveOpen: true);
            }

            return originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
            {
                if (compressedStream != null)
                {
                    compressedStream.Dispose();
                }
            });
        }
    }
}