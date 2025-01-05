using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace starinc.io.gallaryx
{
    public class AddressableManager : Singleton<AddressableManager>
    {
        public AssetReference SpawnablePrefab;

        /// <summary>
        /// 어드레서블 다운로드 함수
        /// </summary>
        /// <param name="key"></param>
        public void DownloadDependenciesAsync(object key)
        {
            Addressables.GetDownloadSizeAsync(key).Completed += (opSize) =>
            {
                if (opSize.Status == AsyncOperationStatus.Succeeded && opSize.Result > 0)
                {
                    Addressables.DownloadDependenciesAsync(key, true).Completed += (opDownload) =>
                    {
                        if (((AsyncOperationHandle)opDownload).Status != AsyncOperationStatus.Succeeded) return;

                        //다운로드 완료 처리
                    };
                }
                else
                {
                    //이미 다운로드가 완료된 상태 처리
                }
            };
        }

        public void LoadAssetAsync(object key)
        {
            try
            {
                Addressables.LoadAssetAsync<GameObject>(key).Completed += (op) =>
                {
                    if (((AsyncOperationHandle<GameObject>)op).Status == AsyncOperationStatus.Succeeded) return;

                    //로드 완료 처리
                };
            }
            catch (Exception e) { Debug.LogError(e.Message); }
        }

        public void CreatePrefeb()
        {
            List<AsyncOperationHandle<GameObject>> handles = new List<AsyncOperationHandle<GameObject>>();

            AsyncOperationHandle<GameObject> handle = SpawnablePrefab.InstantiateAsync();
            handles.Add(handle);
        }

        public void Release(GameObject gameObject)
        {
            Addressables.ReleaseInstance(gameObject);
        }
    }
}
