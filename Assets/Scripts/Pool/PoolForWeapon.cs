using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PoolForWeapon : MonoBehaviour
{
    public int count;
    public GameObject[] prefab;
    public Transform parent;
    public List<Transform> list;
    public static PoolForWeapon Instance
    {
        get { return instance; }
    }
    private static PoolForWeapon instance;
    void Awake()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        }


        instance = this;
        list = new List<Transform>();
        StartCoroutine(Fill());
        DontDestroyOnLoad(gameObject);
    }
    IEnumerator Fill()
    {
        for (int i = 0; i < count; i++)
        {
            Transform obj = Instantiate(prefab[Random.Range(0, prefab.Length - 1)], parent).transform;
            obj.transform.position = Vector3.zero;
            list.Add(obj);
            obj.gameObject.SetActive(false);
            yield return null;
        }
    }
    public Transform GetObjectToPosition(Vector3 position)
    {
        Transform obj;
        if (list.Count > 0)
        {
            obj = list[0];
            list.RemoveAt(0);
        }
        else
        {
            obj = Instantiate(prefab[Random.Range(0, prefab.Length - 1)], parent).transform;
        }
        obj.gameObject.SetActive(true);
        obj.transform.position = position;
        obj.GetComponent<ParticleSystem>().Play();
        return obj;
    }
    public void ReturnObjectToPool(Transform obj)
    {
        if (list.Count < count)
        {  
            list.Add(obj);
            obj.gameObject.SetActive(false);
            obj.GetComponent<ParticleSystem>().Stop();
            obj.position = Vector3.zero;
        }
        else
        {
            Destroy(obj.gameObject);
        }
    }
}
