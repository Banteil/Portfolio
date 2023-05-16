using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Zeus
{
    public static class AddressableDownLoader<T> where T : new()
    {
        public static void DownLoad(AssetLabelReference assetLabelReference, Action<T> callback)
        {
            AsyncOperationHandle<IList<T>> handle = new AsyncOperationHandle<IList<T>>();

            Addressables.GetDownloadSizeAsync(assetLabelReference).Completed += (AsyncOperationHandle<long> downSizeHandle) =>
            {
                Debug.Log("downSizeHandle.Result === " + downSizeHandle.Result);

                if (downSizeHandle.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(assetLabelReference).Completed += (AsyncOperationHandle Handle) =>
                    {
                        handle = Addressables.LoadAssetsAsync<T>(assetLabelReference, callback);
                    };
                }
                else
                {
                    handle = Addressables.LoadAssetsAsync<T>(assetLabelReference, callback);
                }

                Addressables.Release(handle);
            };
        }

        public static void DownLoad(string key, Action<T> callback)
        {
            AsyncOperationHandle<IList<T>> handle = new AsyncOperationHandle<IList<T>>();

            Addressables.GetDownloadSizeAsync(key).Completed += (AsyncOperationHandle<long> downSizeHandle) =>
            {
                Debug.Log("downSizeHandle.Result === " + downSizeHandle.Result);

                if (downSizeHandle.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(key).Completed += (AsyncOperationHandle Handle) =>
                    {
                        handle = Addressables.LoadAssetsAsync<T>(key, callback);
                    };
                }
                else
                {
                    handle = Addressables.LoadAssetsAsync<T>(key, callback);
                }

                Addressables.Release(handle);
            };
        }
    }
}
