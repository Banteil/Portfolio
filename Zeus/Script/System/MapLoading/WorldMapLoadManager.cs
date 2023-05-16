using System.Collections.Generic;
using UnityEngine;

namespace Zeus
{
    public class WorldMapLoadManager : BaseObject<WorldMapLoadManager>
    {
        private Dictionary<Transform, List<GameObject>> loadedMap;

        private void OnCollision(TypeMapCheck mapCheck, Collider collider, bool enter)
        {
            if (!collider.TryGetComponent<MapDataObject>(out var mapDataObject))
                return;
            
            loadedMap ??= new Dictionary<Transform, List<GameObject>>();

            if (enter)
            {
                //mapcheck -> 읽어야하는 레벨. comp->충돌한 오브젝트의 레벨.
                if (mapDataObject.LoadLevel == TypeMapCheck.LEVEL4 && mapCheck == TypeMapCheck.LEVEL3)
                {
                    Debug.Log("LEVEL_4 => LEVEL_3");

                    foreach (var item in mapDataObject.LowObjectReference)
                    {
                        if (!item.RuntimeKeyIsValid())
                            continue;

                        AddressableDownLoader<GameObject>.DownLoad(item, result =>
                        {
                            if (loadedMap.ContainsKey(collider.transform))
                            {
                                Debug.LogError("????");
                            }
                            else
                            {
                                var ob = Instantiate(result);
                                ob.transform.parent = collider.transform;
                                ob.transform.SetPositionAndRotation(collider.transform.position, Quaternion.identity);
                                ob.SetActive(true);
                                collider.enabled = false;
                                var list = new List<GameObject>();
                                list.Add(ob);
                                loadedMap.Add(ob.transform.parent, list);
                            }
                        });
                    }
                }
                else if (mapDataObject.LoadLevel == TypeMapCheck.LEVEL3 && mapCheck == TypeMapCheck.LEVEL2)
                {
                    Debug.Log("LEVEL_3 => LEVEL_2");

                    foreach (var item in mapDataObject.LowObjectReference)
                    {
                        if (!item.RuntimeKeyIsValid())
                            continue;

                        AddressableDownLoader<GameObject>.DownLoad(item, result =>
                        {
                            //로드가 완료되면 패어런트에 붙여야하고.
                            var ob = Instantiate(result);
                            ob.transform.SetParent(collider.transform.parent, false);

                            if (loadedMap.ContainsKey(collider.transform.parent))
                            {
                                var list = loadedMap[collider.transform.parent];
                                var findItem = list.Find(_ => _.Equals(collider.gameObject));
                                if (findItem != null)
                                {
                                    list.Remove(findItem);
                                }

                                if (!collider.gameObject.TryGetComponent<BoxCollider>(out var parentCollider))
                                {
                                    Debug.LogError($"ParentCollider Not Found Collider Name : {collider.name}");
                                    return;
                                }

                                //위치 지정.
                                var offset = parentCollider.size * 0.25f; //4개로 나누므로 1/4
                                var position = result.transform.position;

                                if (list.Count == 0)
                                {
                                    position.x -= offset.x;
                                    position.z -= offset.z;
                                }
                                else if (list.Count == 1)
                                {
                                    position.x += offset.x;
                                    position.z -= offset.z;
                                }
                                else if (list.Count == 2)
                                {
                                    position.x -= offset.x;
                                    position.z += offset.z;
                                }
                                else if (list.Count == 3)
                                {
                                    position.x += offset.x;
                                    position.z += offset.z;
                                }

                                ob.transform.localPosition = position;
                                list.Add(ob);

                                //로드가 완료되면 윗층 오브젝트 파괴.
                                if (mapDataObject.LowObjectReference.Length == list.Count)
                                {
                                    Destroy(collider.gameObject);
                                }
                            }
                            else //없으면???
                            {
                                Debug.LogError("??????");
                            }

                            ob.SetActive(true);

                        });
                    }
                }
                else if (mapDataObject.LoadLevel == TypeMapCheck.LEVEL2 && mapCheck == TypeMapCheck.LEVEL1)
                {
                    //Debug.Log("LEVEL_2 => LEVEL_1");

                    GameObject parentOB = null;
                    //부모로 사용될 위치의 오브젝트를 찾아서 생성한다.
                    if (loadedMap.ContainsKey(collider.transform.parent))
                    {
                        var parentList = loadedMap[collider.transform.parent];
                        parentOB = parentList.Find(_ => _.Equals(collider.gameObject));
                        parentList.Remove(parentOB);
                    }

                    if (parentOB == null)
                    {
                        Debug.LogError("????");
                        return;
                    }

                    var newOb = new GameObject(parentOB.name.Replace("(Clone)", ""));
                    newOb.transform.SetParent(parentOB.transform.parent, false);
                    newOb.transform.SetPositionAndRotation(parentOB.transform.position, parentOB.transform.rotation);

                    foreach (var item in mapDataObject.LowObjectReference)
                    {
                        if (!item.RuntimeKeyIsValid())
                            continue;

                        AddressableDownLoader<GameObject>.DownLoad(item, result =>
                        {
                            //로드가 완료되면 패어런트에 붙여야하고.
                            var ob = Instantiate(result);
                            ob.transform.SetParent(newOb.transform, false);
                            var list = new List<GameObject>();
                            if (!loadedMap.ContainsKey(newOb.transform))
                            {
                                loadedMap.Add(newOb.transform, list);
                            }

                            list = loadedMap[newOb.transform];

                            if (!parentOB.TryGetComponent<BoxCollider>(out var parentCollider))
                            {
                                Debug.LogError($"ParentCollider Not Found Collider Name : {collider.name}");
                                return;
                            }

                            var offset = parentCollider.size * 0.25f; //4개로 나누므로 1/4
                            var position = Vector3.zero;

                            if (list.Count == 0)
                            {
                                position.x -= offset.x;
                                position.z -= offset.z;
                            }
                            else if (list.Count == 1)
                            {
                                position.x += offset.x;
                                position.z -= offset.z;
                            }
                            else if (list.Count == 2)
                            {
                                position.x -= offset.x;
                                position.z += offset.z;
                            }
                            else if (list.Count == 3)
                            {
                                position.x += offset.x;
                                position.z += offset.z;
                            }

                            ob.transform.localPosition = position;

                            ob.SetActive(true);

                            list.Add(ob);

                            if (mapDataObject.LowObjectReference.Length == list.Count)
                            {
                                Destroy(parentOB);
                            }
                        });
                    }
                }
            }

            //빠져나갈때. //최하위가 빠져나갔다.
            if (mapCheck == TypeMapCheck.LEVEL1 && mapDataObject.LoadLevel == TypeMapCheck.LEVEL1)
            {
                //LEVEL_1 => LEVEL_2 체크하고 실행.
                //Debug.Log("LEVEL_1 => LEVEL_2");
                //찾아본다.
                if (loadedMap.ContainsKey(mapDataObject.transform.parent))
                {
                    var list = loadedMap[mapDataObject.transform.parent];

                    mapDataObject.GridIn = enter;

                    if (!enter)
                    {
                        var gridinFind = false;
                        foreach (var item in list)
                        {
                            var mapData = item.GetComponent<MapDataObject>();
                            gridinFind = mapData.GridIn;

                            if (gridinFind)
                            {
                                break;
                            }
                        }

                        if (!gridinFind)
                        {
                            loadedMap.Remove(mapDataObject.transform.parent);

                            AddressableDownLoader<GameObject>.DownLoad(mapDataObject.HighObjecReference, result =>
                            {
                                //상위 오브젝트 로드 하고 지금꺼 모두 삭제.
                                var ob = Instantiate(result);
                                ob.transform.SetParent(mapDataObject.transform.parent.parent, false);
                                ob.transform.position = mapDataObject.transform.parent.position;
                                ob.SetActive(true);
                                Destroy(mapDataObject.transform.parent.gameObject);

                                //이미 다른 곳에서 뭉칠때 만들어져있을수있다.
                                if (loadedMap.ContainsKey(ob.transform.parent))
                                {
                                    var highList = loadedMap[ob.transform.parent];
                                    highList.Add(ob);
                                }
                                else //처음 만들어졌다.
                                {
                                    list.Clear();
                                    list.Add(ob);

                                    loadedMap.Add(ob.transform.parent, list);
                                }
                            });
                        }
                    }
                }
            }
            else if (mapCheck == TypeMapCheck.LEVEL2 && mapDataObject.LoadLevel == TypeMapCheck.LEVEL2)
            {
                //찾아본다.
                if (loadedMap.ContainsKey(mapDataObject.transform.parent))
                {
                    //Debug.Log("LEVEL_2 => LEVEL_3");
                    mapDataObject.GridIn = enter;

                    if (!enter)
                    {
                        var list = loadedMap[mapDataObject.transform.parent];

                        var gridinFind = false;
                        foreach (var item in list)
                        {
                            var mapData = item.GetComponent<MapDataObject>();
                            gridinFind = mapData.GridIn;

                            if (gridinFind)
                            {
                                break;
                            }
                        }

                        if (!gridinFind)
                        {
                            loadedMap.Remove(mapDataObject.transform.parent);

                            AddressableDownLoader<GameObject>.DownLoad(mapDataObject.HighObjecReference, result =>
                            {
                                //상위 오브젝트 로드 하고 지금꺼 모두 삭제.
                                var ob = Instantiate(result);
                                ob.transform.SetParent(mapDataObject.transform.parent, false);
                                ob.transform.SetPositionAndRotation(mapDataObject.transform.parent.position, Quaternion.identity);
                                ob.SetActive(true);

                                foreach (var item in list)
                                {
                                    Destroy(item);
                                }

                                //이미 다른 곳에서 뭉칠때 만들어져있을수있다.
                                if (loadedMap.ContainsKey(ob.transform.parent))
                                {
                                    Debug.LogError("????");
                                }
                                else //처음 만들어졌다.
                                {
                                    list.Clear();
                                    list.Add(ob);

                                    loadedMap.Add(ob.transform.parent, list);
                                }
                            });
                        }
                    }
                }
            }
            else if (mapCheck == TypeMapCheck.LEVEL3 && mapDataObject.LoadLevel == TypeMapCheck.LEVEL3)
            {
                if (enter)
                    return;

                //찾아본다.
                if (loadedMap.ContainsKey(mapDataObject.transform.parent))
                {
                    Debug.Log("LEVEL_3 => LEVEL_4");
                    var list = loadedMap[mapDataObject.transform.parent];
                    var compe = mapDataObject.transform.parent.GetComponent<Collider>();
                    foreach (var item in list)
                    {
                        Destroy(item);
                    }

                    loadedMap.Remove(mapDataObject.transform.parent);
                    compe.enabled = true;
                }
            }
            //Debug.Log($"OnCollision Check Level : {mapCheck} / Collision Collider : {collider} / Enter : {enter}");
        }

        internal void AddCollisionEvent(MapCheck mapCheck)
        {
            mapCheck.CallCollisionChecker = OnCollision;
        }
    }
}