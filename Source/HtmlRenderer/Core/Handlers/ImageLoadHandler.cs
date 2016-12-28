// "Therefore those skilled at the unorthodox
// are infinite as heaven and earth,
// inexhaustible as the great rivers.
// When they come to an end,
// they begin again,
// like the days and months;
// they die and are reborn,
// like the four seasons."
//
// - Sun Tsu,
// "The Art of War"

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;
using TheArtOfDev.HtmlRenderer.Core.Entities;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// Handler for all loading image logic.<br/>
    /// <p>
    /// Loading by <see cref="HtmlImageLoadEventArgs"/>.<br/>
    /// Loading by file path.<br/>
    /// Loading by URI.<br/>
    /// </p>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Supports sync and async image loading.
    /// </para>
    /// <para>
    /// If the image object is created by the handler on calling dispose of the handler the image will be released, this
    /// makes release of unused images faster as they can be large.<br/>
    /// Disposing image load handler will also cancel download of image from the web.
    /// </para>
    /// </remarks>
    internal sealed class ImageLoadHandler : IDisposable
    {
        #region Fields and Consts

        /// <summary>
        /// the container of the html to handle load image for
        /// </summary>
        private readonly HtmlContainerInt HtmlContainer;

        /// <summary>
        /// callback raised when image load process is complete with image or without
        /// </summary>
        private readonly ActionInt<RImage, RRect, bool> LoadCompleteCallback;

        /// <summary>
        /// Must be open as long as the image is in use
        /// </summary>
        private FileStream ImageFileStream;

        /// <summary>
        /// the image instance of the loaded image
        /// </summary>
        private RImage _Image;

        /// <summary>
        /// the image rectangle restriction as returned from image load event
        /// </summary>
        private RRect ImageRectangle;

        /// <summary>
        /// to know if image load event callback was sync or async raised
        /// </summary>
        private bool AsyncCallback;

        /// <summary>
        /// flag to indicate if to release the image object on box dispose (only if image was loaded by the box)
        /// </summary>
        private bool ReleaseImageObject;

        /// <summary>
        /// is the handler has been disposed
        /// </summary>
        private bool Disposed;

        #endregion

        /// <summary>
        /// Init.
        /// </summary>
        /// <param name="htmlContainer">the container of the html to handle load image for</param>
        /// <param name="loadCompleteCallback">callback raised when image load process is complete with image or without</param>
        public ImageLoadHandler(HtmlContainerInt htmlContainer, ActionInt<RImage, RRect, bool> loadCompleteCallback)
        {
            ArgChecker.AssertArgNotNull(htmlContainer, "htmlContainer");
            ArgChecker.AssertArgNotNull(loadCompleteCallback, "loadCompleteCallback");

            this.HtmlContainer = htmlContainer;
            this.LoadCompleteCallback = loadCompleteCallback;
        }

        /// <summary>
        /// the image instance of the loaded image
        /// </summary>
        public RImage Image
        {
            get { return this._Image; }
        }

        /// <summary>
        /// the image rectangle restriction as returned from image load event
        /// </summary>
        public RRect Rectangle
        {
            get { return this.ImageRectangle; }
        }

        /// <summary>
        /// Set image of this image box by analyzing the src attribute.<br/>
        /// Load the image from inline base64 encoded string.<br/>
        /// Or from calling property/method on the bridge object that returns image or URL to image.<br/>
        /// Or from file path<br/>
        /// Or from URI.
        /// </summary>
        /// <remarks>
        /// File path and URI image loading is executed async and after finishing calling <see cref="ImageLoadComplete"/>
        /// on the main thread and not thread-pool.
        /// </remarks>
        /// <param name="src">the source of the image to load</param>
        /// <param name="attributes">the collection of attributes on the element to use in event</param>
        /// <returns>the image object (null if failed)</returns>
        public void LoadImage(string src, Dictionary<string, string> attributes)
        {
            try
            {
                var args = new HtmlImageLoadEventArgs(src, attributes, this.OnHtmlImageLoadEventCallback);
                this.HtmlContainer.RaiseHtmlImageLoadEvent(args);
                this.AsyncCallback = !this.HtmlContainer.AvoidAsyncImagesLoading;

                if (!args.Handled)
                {
                    if (!string.IsNullOrEmpty(src))
                    {
                        if (src.StartsWith("data:image", StringComparison.CurrentCultureIgnoreCase))
                        {
                            this.SetFromInlineData(src);
                        }
                        else
                        {
                            this.SetImageFromPath(src);
                        }
                    }
                    else
                    {
                        this.ImageLoadComplete(false);
                    }
                }
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.Image, "Exception in handling image source", ex);
                this.ImageLoadComplete(false);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Disposed = true;
            this.ReleaseObjects();
        }

        #region Private methods

        /// <summary>
        /// Set the image using callback from load image event, use the given data.
        /// </summary>
        /// <param name="path">the path to the image to load (file path or uri)</param>
        /// <param name="image">the image to load</param>
        /// <param name="imageRectangle">optional: limit to specific rectangle of the image and not all of it</param>
        private void OnHtmlImageLoadEventCallback(string path, object image, RRect imageRectangle)
        {
            if (!this.Disposed)
            {
                this.ImageRectangle = imageRectangle;

                if (image != null)
                {
                    this._Image = this.HtmlContainer.Adapter.ConvertImage(image);
                    this.ImageLoadComplete(this.AsyncCallback);
                }
                else if (!string.IsNullOrEmpty(path))
                {
                    this.SetImageFromPath(path);
                }
                else
                {
                    this.ImageLoadComplete(this.AsyncCallback);
                }
            }
        }

        /// <summary>
        /// Load the image from inline base64 encoded string data.
        /// </summary>
        /// <param name="src">the source that has the base64 encoded image</param>
        private void SetFromInlineData(string src)
        {
            this._Image = this.GetImageFromData(src);
            if (this._Image == null)
                this.HtmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed extract image from inline data");
            this.ReleaseImageObject = true;
            this.ImageLoadComplete(false);
        }

        /// <summary>
        /// Extract image object from inline base64 encoded data in the src of the html img element.
        /// </summary>
        /// <param name="src">the source that has the base64 encoded image</param>
        /// <returns>image from base64 data string or null if failed</returns>
        private RImage GetImageFromData(string src)
        {
            var s = src.Substring(src.IndexOf(':') + 1).Split(new[] { ',' }, 2);
            if (s.Length == 2)
            {
                int imagePartsCount = 0, base64PartsCount = 0;
                foreach (var part in s[0].Split(new[] { ';' }))
                {
                    var pPart = part.Trim();
                    if (pPart.StartsWith("image/", StringComparison.InvariantCultureIgnoreCase))
                        imagePartsCount++;
                    if (pPart.Equals("base64", StringComparison.InvariantCultureIgnoreCase))
                        base64PartsCount++;
                }

                if (imagePartsCount > 0)
                {
                    byte[] imageData = base64PartsCount > 0 ? Convert.FromBase64String(s[1].Trim()) : new UTF8Encoding().GetBytes(Uri.UnescapeDataString(s[1].Trim()));
                    return this.HtmlContainer.Adapter.ImageFromStream(new MemoryStream(imageData));
                }
            }

            return null;
        }

        /// <summary>
        /// Load image from path of image file or URL.
        /// </summary>
        /// <param name="path">the file path or uri to load image from</param>
        private void SetImageFromPath(string path)
        {
            var uri = CommonUtils.TryGetUri(path);
            if (uri != null && uri.Scheme != "file")
            {
                this.SetImageFromUrl(uri);
            }
            else
            {
                var fileInfo = CommonUtils.TryGetFileInfo(uri != null ? uri.AbsolutePath : path);
                if (fileInfo != null)
                {
                    this.SetImageFromFile(fileInfo);
                }
                else
                {
                    this.HtmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed load image, invalid source: " + path);
                    this.ImageLoadComplete(false);
                }
            }
        }

        /// <summary>
        /// Load the image file on thread-pool thread and calling <see cref="ImageLoadComplete"/> after.
        /// </summary>
        /// <param name="source">the file path to get the image from</param>
        private void SetImageFromFile(FileInfo source)
        {
            if (source.Exists)
            {
                if (this.HtmlContainer.AvoidAsyncImagesLoading)
                    this.LoadImageFromFile(source.FullName);
                else
                    ThreadPool.QueueUserWorkItem(state => this.LoadImageFromFile(source.FullName));
            }
            else
            {
                this.ImageLoadComplete();
            }
        }

        /// <summary>
        /// Load the image file on thread-pool thread and calling <see cref="ImageLoadComplete"/> after.<br/>
        /// Calling <see cref="ImageLoadComplete"/> on the main thread and not thread-pool.
        /// </summary>
        /// <param name="source">the file path to get the image from</param>
        private void LoadImageFromFile(string source)
        {
            try
            {
                var imageFileStream = File.Open(source, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                lock (this.LoadCompleteCallback)
                {
                    this.ImageFileStream = imageFileStream;
                    if (!this.Disposed)
                        this._Image = this.HtmlContainer.Adapter.ImageFromStream(this.ImageFileStream);
                    this.ReleaseImageObject = true;
                }

                this.ImageLoadComplete();
            }
            catch (Exception ex)
            {
                this.HtmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed to load image from disk: " + source, ex);
                this.ImageLoadComplete();
            }
        }

        /// <summary>
        /// Load image from the given URI by downloading it.<br/>
        /// Create local file name in temp folder from the URI, if the file already exists use it as it has already been downloaded.
        /// If not download the file.
        /// </summary>
        private void SetImageFromUrl(Uri source)
        {
            var filePath = CommonUtils.GetLocalfileName(source);
            if (filePath.Exists && filePath.Length > 0)
            {
                this.SetImageFromFile(filePath);
            }
            else
            {
                this.HtmlContainer.GetImageDownloader().DownloadImage(source, filePath.FullName, !this.HtmlContainer.AvoidAsyncImagesLoading, this.OnDownloadImageCompleted);
            }
        }

        /// <summary>
        /// On download image complete to local file use <see cref="LoadImageFromFile"/> to load the image file.<br/>
        /// If the download canceled do nothing, if failed report error.
        /// </summary>
        private void OnDownloadImageCompleted(Uri imageUri, string filePath, Exception error, bool canceled)
        {
            if (!canceled && !this.Disposed)
            {
                if (error == null)
                {
                    this.LoadImageFromFile(filePath);
                }
                else
                {
                    this.HtmlContainer.ReportError(HtmlRenderErrorType.Image, "Failed to load image from URL: " + imageUri, error);
                    this.ImageLoadComplete();
                }
            }
        }

        /// <summary>
        /// Flag image load complete and request refresh for re-layout and invalidate.
        /// </summary>
        private void ImageLoadComplete(bool async = true)
        {
            // can happen if some operation return after the handler was disposed
            if (this.Disposed)
                this.ReleaseObjects();
            else
                this.LoadCompleteCallback(this._Image, this.ImageRectangle, async);
        }

        /// <summary>
        /// Release the image and client objects.
        /// </summary>
        private void ReleaseObjects()
        {
            lock (this.LoadCompleteCallback)
            {
                if (this.ReleaseImageObject && this._Image != null)
                {
                    this._Image.Dispose();
                    this._Image = null;
                }

                if (this.ImageFileStream != null)
                {
                    this.ImageFileStream.Dispose();
                    this.ImageFileStream = null;
                }
            }
        }

        #endregion
    }
}