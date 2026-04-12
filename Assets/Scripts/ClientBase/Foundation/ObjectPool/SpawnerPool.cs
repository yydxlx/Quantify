using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ClientBase
{
    public class SpawnerPool
    {
        public string PoolName = string.Empty;
        public GameObject SpawnObj = null;
        public readonly Stack AvailableObjects = new Stack();
        public readonly List<GameObject> SpawnedObjects = new List<GameObject>();
        private static Transform SpawnContainer;
        public SpawnerPool(string poolName, GameObject objToSpawn)
        {
            this.PoolName = poolName;
            this.SpawnObj = objToSpawn;
            GameObject _SpawnContainer = GameObject.Find("/SpawnContainer");
            if (_SpawnContainer == null)
                _SpawnContainer = new GameObject("SpawnContainer");
            SpawnContainer = _SpawnContainer.transform;
        }
        public GameObject Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            GameObject go;
            if (AvailableObjects.Count == 0)
            {
                go = Object.Instantiate(SpawnObj);
                go.name = string.Format("{0} ({1})", PoolName, SpawnedObjects.Count);
            }
            else
            {
                go = AvailableObjects.Pop() as GameObject;
            }
            go.transform.SetParent(parent);
            go.transform.localPosition = position;
            go.transform.localRotation = rotation;
            go.transform.localScale = Vector3.one;
            
            SpawnedObjects.Add(go);
            go.SetActive(true);
            return go;
        }
        public GameObject Spawn(Vector3 position, Transform parent)
        {
            GameObject go; 
            if (AvailableObjects.Count == 0)
            {
                go = Object.Instantiate(SpawnObj);
                go.name = string.Format("{0} ({1})", PoolName, SpawnedObjects.Count);
            }
            else
            {
                go = AvailableObjects.Pop() as GameObject;
            }
            go.transform.SetParent(parent);
            go.transform.localPosition = position;

            SpawnedObjects.Add(go);
            go.SetActive(true);
            return go;
        }
        public GameObject Spawn()
        {
            GameObject go;
            if (AvailableObjects.Count == 0)
            {
                go = Object.Instantiate(SpawnObj);
                go.name = string.Format("{0} ({1})", PoolName, SpawnedObjects.Count);
            }
            else
            {
                go = AvailableObjects.Pop() as GameObject;
            }
            SpawnedObjects.Add(go);
            go.SetActive(true);
            return go;
        }
        public void Prespawn(int count)
        {
            GameObject[] PreSpawned = new GameObject[count];
            for (int i = 0; i < count; i++)
                PreSpawned[i] = Spawn();
            for (int i = 0; i < count; i++)
                Despawn(PreSpawned[i]);
        }
        public void Despawn(GameObject go)
        {
            go.transform.SetParent(SpawnContainer);
            AvailableObjects.Push(go);
            SpawnedObjects.Remove(go);
            go.SetActive(false);
        }
        public void DespawnList(List<GameObject> goObjList)
        {
            if (goObjList == null)
                return;
            foreach (GameObject go in goObjList)
                Despawn(go);
        }
        public void DespawnAll()
        {
            int count = SpawnedObjects.Count;
            if (count == 0)
                return;
            for (int i = count - 1; i >= 0; i--)
            {
                Despawn(SpawnedObjects[i]);
            }
        }
        public void ClearPool()
        {
            DespawnAll();
            int count = AvailableObjects.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                GameObject go = AvailableObjects.Pop() as GameObject;
                Object.Destroy(go);
            }
        }
        public List<GameObject> GetActiveSpawns()
        {
            return SpawnedObjects.Where(x => (x.activeSelf && x.activeInHierarchy)).ToList();
        }
    }
}