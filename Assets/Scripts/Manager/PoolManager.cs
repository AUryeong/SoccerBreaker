using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{

    Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

    public void AddPooling(GameObject origin, Transform parent)
    {
        if (!pools.ContainsKey(origin))
        {
            pools.Add(origin, new List<GameObject>());
        }
        for (int i = 0; i < parent.childCount; i++)
        {
            GameObject obj = parent.GetChild(i).gameObject;
            if(obj != origin)
            pools[origin].Add(obj);
        }
    }
    public GameObject Init(GameObject origin)
    {
        if (origin != null)
        {
            GameObject copy = null;
            if (pools.ContainsKey(origin))
            {
                if (pools[origin].FindAll((GameObject x) => !x.activeSelf).Count > 0)
                {
                    copy = pools[origin].Find((GameObject x) => !x.activeSelf);
                    copy.SetActive(true);
                    return copy;
                }
            }
            else
            {
                pools.Add(origin, new List<GameObject>());
            }
            copy = Instantiate(origin);
            pools[origin].Add(copy);
            copy.SetActive(true);
            return copy;
        }
        else
        {
            return null;
        }
    }
}
