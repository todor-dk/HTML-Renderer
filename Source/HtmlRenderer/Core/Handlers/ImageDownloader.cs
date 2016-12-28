﻿// "Therefore those skilled at the unorthodox
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
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Handlers
{
    /// <summary>
    /// On download file async complete, success or fail.
    /// </summary>
    /// <param name="imageUri">The online image uri</param>
    /// <param name="filePath">the path to the downloaded file</param>
    /// <param name="error">the error if download failed</param>
    /// <param name="canceled">is the file download request was canceled</param>
    public delegate void DownloadFileAsyncCallback(Uri imageUri, string filePath, Exception error, bool canceled);

    /// <summary>
    /// Handler for downloading images from the web.<br/>
    /// Single instance of the handler used for all images downloaded in a single html, this way if the html contains more
    /// than one reference to the same image it will be downloaded only once.<br/>
    /// Also handles corrupt, partial and canceled downloads by first downloading to temp file and only if successful moving to cached
    /// file location.
    /// </summary>
    internal sealed class ImageDownloader : IDisposable
    {
        /// <summary>
        /// the web client used to download image from URL (to cancel on dispose)
        /// </summary>
        private readonly List<WebClient> Clients = new List<WebClient>();

        /// <summary>
        /// dictionary of image cache path to callbacks of download to handle multiple requests to download the same image
        /// </summary>
        private readonly Dictionary<string, List<DownloadFileAsyncCallback>> ImageDownloadCallbacks = new Dictionary<string, List<DownloadFileAsyncCallback>>();

        /// <summary>
        /// Makes a request to download the image from the server and raises the <see cref="cachedFileCallback"/> when it's down.<br/>
        /// </summary>
        /// <param name="imageUri">The online image uri</param>
        /// <param name="filePath">the path on disk to download the file to</param>
        /// <param name="async">is to download the file sync or async (true-async)</param>
        /// <param name="cachedFileCallback">This callback will be called with local file path. If something went wrong in the download it will return null.</param>
        public void DownloadImage(Uri imageUri, string filePath, bool async, DownloadFileAsyncCallback cachedFileCallback)
        {
            ArgChecker.AssertArgNotNull(imageUri, "imageUri");
            ArgChecker.AssertArgNotNull(cachedFileCallback, "cachedFileCallback");

            // to handle if the file is already been downloaded
            bool download = true;
            lock (this.ImageDownloadCallbacks)
            {
                if (this.ImageDownloadCallbacks.ContainsKey(filePath))
                {
                    download = false;
                    this.ImageDownloadCallbacks[filePath].Add(cachedFileCallback);
                }
                else
                {
                    this.ImageDownloadCallbacks[filePath] = new List<DownloadFileAsyncCallback> { cachedFileCallback };
                }
            }

            if (download)
            {
                var tempPath = Path.GetTempFileName();
                if (async)
                    ThreadPool.QueueUserWorkItem(this.DownloadImageFromUrlAsync, new DownloadData(imageUri, tempPath, filePath));
                else
                    this.DownloadImageFromUrl(imageUri, tempPath, filePath);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.ReleaseObjects();
        }

        #region Private/Protected methods

        /// <summary>
        /// Download the requested file in the URI to the given file path.<br/>
        /// Use async sockets API to download from web, <see cref="OnDownloadImageAsyncCompleted"/>.
        /// </summary>
        private void DownloadImageFromUrl(Uri source, string tempPath, string filePath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    this.Clients.Add(client);
                    client.DownloadFile(source, tempPath);
                    this.OnDownloadImageCompleted(client, source, tempPath, filePath, null, false);
                }
            }
            catch (Exception ex)
            {
                this.OnDownloadImageCompleted(null, source, tempPath, filePath, ex, false);
            }
        }

        /// <summary>
        /// Download the requested file in the URI to the given file path.<br/>
        /// Use async sockets API to download from web, <see cref="OnDownloadImageAsyncCompleted"/>.
        /// </summary>
        /// <param name="data">key value pair of URL and file info to download the file to</param>
        private void DownloadImageFromUrlAsync(object data)
        {
            var downloadData = (DownloadData)data;
            try
            {
                var client = new WebClient();
                this.Clients.Add(client);
                client.DownloadFileCompleted += this.OnDownloadImageAsyncCompleted;
                client.DownloadFileAsync(downloadData.Uri, downloadData.TempPath, downloadData);
            }
            catch (Exception ex)
            {
                this.OnDownloadImageCompleted(null, downloadData.Uri, downloadData.TempPath, downloadData.FilePath, ex, false);
            }
        }

        /// <summary>
        /// On download image complete to local file.<br/>
        /// If the download canceled do nothing, if failed report error.
        /// </summary>
        private void OnDownloadImageAsyncCompleted(object sender, AsyncCompletedEventArgs e)
        {
            var downloadData = (DownloadData)e.UserState;
            try
            {
                using (var client = (WebClient)sender)
                {
                    client.DownloadFileCompleted -= this.OnDownloadImageAsyncCompleted;
                    this.OnDownloadImageCompleted(client, downloadData.Uri, downloadData.TempPath, downloadData.FilePath, e.Error, e.Cancelled);
                }
            }
            catch (Exception ex)
            {
                this.OnDownloadImageCompleted(null, downloadData.Uri, downloadData.TempPath, downloadData.FilePath, ex, false);
            }
        }

        /// <summary>
        /// Checks if the file was downloaded and raises the cachedFileCallback from <see cref="ImageDownloadCallbacks"/>
        /// </summary>
        private void OnDownloadImageCompleted(WebClient client, Uri source, string tempPath, string filePath, Exception error, bool cancelled)
        {
            if (!cancelled)
            {
                if (error == null)
                {
                    var contentType = CommonUtils.GetResponseContentType(client);
                    if (contentType == null || !contentType.StartsWith("image", StringComparison.OrdinalIgnoreCase))
                    {
                        error = new Exception("Failed to load image, not image content type: " + contentType);
                    }
                }

                if (error == null)
                {
                    if (File.Exists(tempPath))
                    {
                        try
                        {
                            File.Move(tempPath, filePath);
                        }
                        catch (Exception ex)
                        {
                            error = new Exception("Failed to move downloaded image from temp to cache location", ex);
                        }
                    }

                    error = File.Exists(filePath) ? null : (error ?? new Exception("Failed to download image, unknown error"));
                }
            }

            List<DownloadFileAsyncCallback> callbacksList;
            lock (this.ImageDownloadCallbacks)
            {
                if (this.ImageDownloadCallbacks.TryGetValue(filePath, out callbacksList))
                    this.ImageDownloadCallbacks.Remove(filePath);
            }

            if (callbacksList != null)
            {
                foreach (var cachedFileCallback in callbacksList)
                {
                    try
                    {
                        cachedFileCallback(source, filePath, error, cancelled);
                    }
                    catch
                    {
                    }
                }
            }
        }

        /// <summary>
        /// Release the image and client objects.
        /// </summary>
        private void ReleaseObjects()
        {
            this.ImageDownloadCallbacks.Clear();
            while (this.Clients.Count > 0)
            {
                try
                {
                    var client = this.Clients[0];
                    client.CancelAsync();
                    client.Dispose();
                    this.Clients.RemoveAt(0);
                }
                catch
                {
                }
            }
        }

        #endregion

        #region Inner class: DownloadData

        private sealed class DownloadData
        {
            public readonly Uri Uri;
            public readonly string TempPath;
            public readonly string FilePath;

            public DownloadData(Uri uri, string tempPath, string filePath)
            {
                this.Uri = uri;
                this.TempPath = tempPath;
                this.FilePath = filePath;
            }
        }

        #endregion
    }
}
