﻿using Cysharp.Threading.Tasks;
using UnityEngine.Networking;


namespace BM
{
    public static class DownloadBundleHelper
    {
        public static async UniTask<byte[]> DownloadDataByUrl(string url)
        {
            for (int i = 0; i < AssetComponentConfig.ReDownLoadCount; i++)
            {
                byte[] data = await DownloadData(url);
                if (data != null)
                {
                    return data;
                }
            }
            AssetLogHelper.LogError("下载资源失败: " + url);
            return null;
        }

        private static async UniTask<byte[]> DownloadData(string url)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                UnityWebRequestAsyncOperation webRequestAsync = webRequest.SendWebRequest();
                UniTaskCompletionSource waitDown = new UniTaskCompletionSource();
                webRequestAsync.completed += (asyncOperation) =>
                {
                    waitDown.TrySetResult();
                };
                await waitDown.Task;
#if UNITY_2020_1_OR_NEWER
                if (webRequest.result != UnityWebRequest.Result.Success)
#else
                if (!string.IsNullOrEmpty(webRequest.error))
#endif
                {
                    AssetLogHelper.Log("下载Bundle失败 重试\n" + webRequest.error + "\nURL：" + url);
                    return null;
                }
                return webRequest.downloadHandler.data;
            }
        }
        
        
    }
}