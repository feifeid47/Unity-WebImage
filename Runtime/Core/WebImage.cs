using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Feif.UI
{
    public class WebImage : Image
    {
        /// <summary>
        /// 图片下载器
        /// 如果未赋值，将会使用默认下载器
        /// </summary>
        public static Func<string, Task<Sprite>> Downloader;

        /// <summary>默认图片</summary>
        public Sprite defaultSprite;

        /// <summary>图片url</summary>
        public string Url { get => url; }

        /// <summary>图片下载成功事件</summary>
        public event Action<string> OnDownloadSuccess;
        /// <summary>图片下载失败事件</summary>
        public event Action<string> OnDownloadFailed;

        [SerializeField]
        private string url;

        /// <summary>
        /// 设置图片url并同时下载图片
        /// </summary>
        /// <param name="url">图片下载地址</param>
        /// <param name="reset">是否先将图片设置为默认图片</param>
        public async void SetUrl(string url, bool reset = false)
        {
            if (reset) SetAsDefault();
            this.url = url;
            Sprite downloadResult = null;
            downloadResult = Downloader == null ? await DefaultDownloader(url) : await Downloader(url);
            if (downloadResult != null)
            {
                OnDownloadSuccess?.Invoke(url);
            }
            else
            {
                OnDownloadFailed?.Invoke(url);
            }
            if (this.url != url) return;
            sprite = downloadResult == null ? defaultSprite : downloadResult;
        }

        /// <summary>
        /// 刷新，重新下载当前图片
        /// </summary>
        /// <param name="reset">是否先将图片设为默认图片</param>
        public void Refresh(bool reset = false)
        {
            SetUrl(url, reset);
        }

        /// <summary>
        /// 设为默认图片
        /// </summary>
        public void SetAsDefault()
        {
            sprite = defaultSprite;
        }

        /// <summary>
        /// 默认下载器
        /// </summary>
        public static Task<Sprite> DefaultDownloader(string url)
        {
            var completionSource = new TaskCompletionSource<Sprite>();
            if (string.IsNullOrEmpty(url))
            {
                completionSource.SetResult(null);
                return completionSource.Task;
            }
            var request = UnityWebRequestTexture.GetTexture(url);
            request.SendWebRequest().completed += _ =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var texture = (request.downloadHandler as DownloadHandlerTexture).texture;
                if (texture == null)
                {
                    completionSource.SetResult(null);
                    request.Dispose();
                    return;
                }
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                completionSource.SetResult(sprite);
                request.Dispose();
            };
            return completionSource.Task;
        }
    }
}