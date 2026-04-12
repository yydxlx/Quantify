using System;
using System.Collections;
using System.Collections.Generic;
using ClientBase.Coroutine;
//using Cocoon.BestHTTP;
using UnityEngine;
using UnityEngine.Networking;

namespace ClientBase
{
    public static class DownloadUtility
    {

        /// <summary>
        /// 下载结果
        /// </summary>
        public enum EDownloadResult
        {
            Success = 0,
            Error = 1,
        }

        /// <summary>
        /// Downloads from WWW.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="E_OnCallback">The e on callback.</param>
        /// <param name="E_OnTimeout">The e on timeout.</param>
        /// <param name="E_OnProgress">The e on progress.</param>
        public static void DownloadFromWWW(string url, Action<WWW> E_OnCallback, Action<int> E_OnTimeout,Action<float> E_OnProgress)
        {
            CoroutineManager.RunCoroutine(DownloadFromWWWAsync(1, url, E_OnCallback, E_OnTimeout,E_OnProgress));
        }

        private static IEnumerator<float> DownloadFromWWWAsync(int tryCount, string url, Action<WWW> E_OnCallback, Action<int> E_OnTimeout, Action<float> E_OnProgress)
        {
            if (tryCount <= ClientBaseConst.MaxTryCountDownload)
            {
                //logger.LogInfo("DownloadUtility::DownloadFromWWWAsync Start download url = " + url + " , TryCount = " + tryCount);
                WWW www = new WWW(url);
                bool bTimeout = false;
                float waitingTime = 0.0f;
                float progress = 0.0f;
                while ((!www.isDone) && www.error == null)
                {
                    if (www.progress == progress)
                        waitingTime += Time.unscaledDeltaTime;
                    else
                    {
                        waitingTime = 0.0f;
                        progress = www.progress;
                        E_OnProgress.InvokeSafely(www.progress);
                    }
                    if (waitingTime >= ClientBaseConst.DownloadTimeout)
                    {
                        bTimeout = true;
                        E_OnTimeout.InvokeSafely(tryCount);
                        break;
                    }
                    yield return CoroutineManager.WaitForOneFrame;
                }
                if (www.error != null || bTimeout)
                {
                    //CoroutineManager.RunCoroutine(DownloadFromWWWAsync(tryCount + 1, url, E_OnCallback, E_OnTimeout, E_OnProgress));//暂时不做重发3次
                    Debug.LogError("超时或者error. url = " + url + "error:" + www.error);
                }
                else
                {
                    E_OnCallback.InvokeSafely(www);
                    www.Dispose();
                    www = null;
                }
            }
            else
            {
                Debug.LogError("WWW下载失败. url = " + url);
            }
        }

        /// <summary>
        /// Post data from WWW
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pPostData"></param>
        /// <param name="callback"></param>
        public static void BasePostFromWWW(string url, string pPostData, Action<WWW> E_OnCallback)
        {
            CoroutineManager.RunCoroutine(BasePostFromWWW(1, url, pPostData, E_OnCallback));
        }

        private static IEnumerator<float> BasePostFromWWW(int tryCount, string pUrl, string pPostData, Action<WWW> E_OnCallback)
        {
            //logger.LogInfo("BasePostFromWWW Try Count = {0}, PostData = {1}, Url = {2}".F(tryCount, pPostData, pUrl));
            if (tryCount < ClientBaseConst.MaxTryCountDownload)
            {
                //将request文本转为byte数组  
                byte[] bs = System.Text.UTF8Encoding.UTF8.GetBytes(pPostData);
                //向HTTP服务器提交Post数据  
                WWW getData = new WWW(pUrl, bs);
                bool bTimeout = false;
                float waitingTime = 0.0f;
                float progress = 0.0f;

                //yield return getData;
                while ((!getData.isDone) && getData.error == null && !bTimeout)
                {
                    if (progress == getData.progress)
                        waitingTime += Time.deltaTime;
                    else
                    {
                        progress = getData.progress;
                        waitingTime = 0.0f;
                    }

                    if (waitingTime >= ClientBaseConst.DownloadTimeout)
                    {
                        bTimeout = true;
                        break;
                    }
                    yield return CoroutineManager.WaitForOneFrame;
                }

                if (getData.error != null || bTimeout)
                {
                    Debug.LogError(getData.error);
                    CoroutineManager.RunCoroutine(BasePostFromWWW(tryCount + 1, pUrl, pPostData, E_OnCallback));
                }
                else
                {
                    E_OnCallback.InvokeSafely(getData);
                    getData.Dispose();
                }
            }
            else
            {
                Debug.LogError("DownloadManager::DownloadFromWWWAsync download failed.");
                E_OnCallback(null);
            }
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Downloads from WWW.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="E_OnCallback">The e on callback.</param>
        /// <param name="E_OnTimeout">The e on timeout.</param>
        /// <param name="E_OnProgress">The e on progress.</param>
        public static void DownloadFromRequest(string url, Action<EDownloadResult, UnityWebRequest> E_OnCallback, Action<int> E_OnTimeout, Action<float> E_OnProgress)
        {
            CoroutineManager.RunCoroutine(DownloadFromRequestAsync(1, url, E_OnCallback, E_OnTimeout, E_OnProgress));
        }

        private static IEnumerator<float> DownloadFromRequestAsync(int tryCount, string url, Action<EDownloadResult, UnityWebRequest> E_OnCallback, Action<int> E_OnTimeout, Action<float> E_OnProgress)
        {
            if (tryCount <= ClientBaseConst.MaxTryCountDownload)
            {
                //logger.LogInfo("DownloadUtility::DownloadFromWWWAsync Start download url = " + url + " , TryCount = " + tryCount);
                //WWW www = new WWW(url);
                UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
                DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
                request.downloadHandler = dH;
                request.SendWebRequest();
                bool bTimeout = false;
                float waitingTime = 0.0f;
                float progress = 0.0f;
                while ((!request.isDone) && request.error == null)
                {
                    if (request.downloadProgress == progress)
                        waitingTime += Time.unscaledDeltaTime;
                    else
                    {
                        waitingTime = 0.0f;
                        progress = request.downloadProgress;
                        E_OnProgress.InvokeSafely(request.downloadProgress);
                    }
                    if (waitingTime >= ClientBaseConst.DownloadTimeout)
                    {
                        bTimeout = true;
                        E_OnTimeout.InvokeSafely(tryCount);
                        break;
                    }
                    yield return CoroutineManager.WaitForOneFrame;
                }
                if (request.error != null || bTimeout)
                {
                    //CoroutineManager.RunCoroutine(DownloadFromRequestAsync(tryCount + 1, url, E_OnCallback, E_OnTimeout, E_OnProgress));//暂时不做重发3次
                    Debug.LogError("超时或者error. url = " + url);
                }
                else
                {
                    E_OnCallback.InvokeSafely(EDownloadResult.Success, request);
                    request.Dispose();
                    request = null;
                }
            }
            else
            {
                Debug.LogError("Request下载失败. url = " + url);
                E_OnCallback.InvokeSafely(EDownloadResult.Error, null);
            }
        }

        /// <summary>
        /// Post data from WWW
        /// </summary>
        /// <param name="url"></param>
        /// <param name="pPostData"></param>
        /// <param name="callback"></param>
        public static void BasePostFromRequest(string url, string pPostData, Action<UnityWebRequest> E_OnCallback, Action<float> E_OnProgress)
        {
            CoroutineManager.RunCoroutine(BasePostFromRequest(1, url, pPostData, E_OnCallback, E_OnProgress));
        }

        private static IEnumerator<float> BasePostFromRequest(int tryCount, string pUrl, string pPostData, Action<UnityWebRequest> E_OnCallback, Action<float> E_OnProgress)
        {
            //logger.LogInfo("BasePostFromWWW Try Count = {0}, PostData = {1}, Url = {2}".F(tryCount, pPostData, pUrl));
            
            if (tryCount < ClientBaseConst.MaxTryCountDownload)
            {
                //将request文本转为byte数组  
                byte[] bs = System.Text.UTF8Encoding.UTF8.GetBytes(pPostData);
                //向HTTP服务器提交Post数据  
                UnityWebRequest request = new UnityWebRequest(pUrl, UnityWebRequest.kHttpVerbGET);
                DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
                request.downloadHandler = dH;
                request.uploadHandler = new UploadHandlerRaw(bs);
                request.SendWebRequest();

                //WWW getData = new WWW(pUrl, bs);
                bool bTimeout = false;
                float waitingTime = 0.0f;
                float progress = 0.0f;

                //yield return getData;
                while ((!request.isDone) && request.error == null && !bTimeout)
                {
                    if (progress == request.downloadProgress)
                        waitingTime += Time.deltaTime;
                    else
                    {
                        progress = request.downloadProgress;
                        waitingTime = 0.0f;
                        E_OnProgress.InvokeSafely(request.downloadProgress);
                    }

                    if (waitingTime >= ClientBaseConst.DownloadTimeout)
                    {
                        bTimeout = true;
                        break;
                    }
                    yield return CoroutineManager.WaitForOneFrame;
                }

                if (request.error != null || bTimeout)
                {
                    Debug.LogError(request.error);
                    CoroutineManager.RunCoroutine(BasePostFromRequest(tryCount + 1, pUrl, pPostData, E_OnCallback, E_OnProgress));
                }
                else
                {
                    E_OnCallback.InvokeSafely(request);
                    request.Dispose();
                }
            }
            else
            {
                Debug.LogError("DownloadManager::DownloadFromRequestAsync download failed.");
                E_OnCallback(null);
            }
        }

        private static IEnumerator SendUrl(string url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.Send();
                if (www.error != null)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    if (www.responseCode == 200)//200表示接受成功
                    {
                        Debug.Log(www.downloadHandler.text);
                    }
                }
            }
        }

        //    /// <summary>
        //    /// Downloads from HTTP.
        //    /// </summary>
        //    /// <param name="url">The URL.</param>
        //    /// <param name="E_OnCallback">The e on callback.</param>
        //    /// <param name="E_OnTimeout">The e on timeout.</param>
        //    public static void DownloadFromHttp(string url, Action<EDownloadResult, byte[]> E_OnCallback, Action<int> E_OnTimeout)
        //    {
        //        CoroutineManager.RunCoroutine(DownloadFromHttpAsync(1, url, E_OnCallback, E_OnTimeout));
        //    }

        //    private static IEnumerator<float> DownloadFromHttpAsync(int tryCount, string url, Action<EDownloadResult, byte[]> E_OnCallback, Action<int> E_OnTimeout)
        //    {
        //        if (tryCount > ClientBaseConst.MaxTryCountDownload)
        //        {
        //            Debug.LogError("DownloadManager::DownloadFromHttpAsync download failed. url = " + url);
        //        }
        //        else
        //        {
        //            // Create and send our request
        //            var request = new HTTPRequest(new Uri(url)).Send();

        //            // Wait while it's finishes and add some fancy dots to display something while the user waits for it.
        //            // A simple "yield return StartCoroutine(request);" would do the job too.
        //            while (request.State < HTTPRequestStates.Finished)
        //                yield return CoroutineManager.WaitForOneFrame;

        //            switch (request.State)
        //            {
        //                case HTTPRequestStates.Finished:
        //                    E_OnCallback.InvokeSafely(EDownloadResult.Success, request.Response.Data);
        //                    break;
        //                case HTTPRequestStates.Error:
        //                case HTTPRequestStates.Aborted:
        //                    E_OnCallback.InvokeSafely(EDownloadResult.Error, null);
        //                    break;
        //                case HTTPRequestStates.TimedOut:
        //                case HTTPRequestStates.ConnectionTimedOut:
        //                    E_OnTimeout.InvokeSafely(tryCount);
        //                    CoroutineManager.RunCoroutine(DownloadFromHttpAsync(tryCount + 1, url, E_OnCallback, E_OnTimeout));
        //                    break;
        //            }
        //        }
        //    }

        //    /// <summary>
        //    /// Post data by http.
        //    /// </summary>
        //    /// <param name="url"></param>
        //    /// <param name="pPostData"></param>
        //    /// <param name="E_OnCallback"></param>
        //    public static void BasePostFromHttp(string url, string pPostData,
        //        System.Action<EDownloadResult, byte[]> E_OnCallback)
        //    {
        //        CoroutineManager.RunCoroutine(BasePostFromHttpAsync(1, url, pPostData, E_OnCallback));
        //    }

        //    private static IEnumerator<float> BasePostFromHttpAsync(int tryCount, string url, string pPostData,
        //        System.Action<EDownloadResult, byte[]> E_OnCallback)
        //    {
        //        logger.LogInfo("BasePostFromHttpAsync Try Count = {0}, PostData = {1}, Url = {2}".F(tryCount, pPostData, url));
        //        if (tryCount < ClientBaseConst.MaxTryCountPostData)
        //        {
        //            var request = new HTTPRequest(new Uri(url), HTTPMethods.Post);

        //            request.RawData = pPostData.ToBinary();
        //            request.Send();

        //            while (request.State < HTTPRequestStates.Finished)
        //                yield return CoroutineManager.WaitForOneFrame;

        //            switch (request.State)
        //            {
        //                case HTTPRequestStates.Finished:
        //                    E_OnCallback.InvokeSafely(EDownloadResult.Success, request.Response.Data);
        //                    break;
        //                case HTTPRequestStates.Error:
        //                case HTTPRequestStates.Aborted:
        //                    E_OnCallback.InvokeSafely(EDownloadResult.Error, null);
        //                    break;
        //                case HTTPRequestStates.TimedOut:
        //                case HTTPRequestStates.ConnectionTimedOut:
        //                    CoroutineManager.RunCoroutine(BasePostFromHttpAsync(tryCount + 1, url, pPostData, E_OnCallback));
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //            E_OnCallback.InvokeSafely(EDownloadResult.Error, null);
        //        }

        //    }
    }


}