using System.Collections.Generic;
using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    Dictionary<GameObject, List<GameObject>> pools = new Dictionary<GameObject, List<GameObject>>();

    public GameObject Init(GameObject obj)
    {
        if (obj != null)
        {
            GameObject copy = null;
            if (pools.ContainsKey(obj))
            {
                if (pools[obj].FindAll((GameObject x) => !x.activeSelf).Count > 0)
                {
                    copy = pools[obj].Find((GameObject x) => !x.activeSelf);
                    copy.SetActive(true);
                    return copy;
                }
            }
            else
            {
                pools.Add(obj, new List<GameObject>());
            }
            copy = Instantiate(obj);
            pools[obj].Add(copy);
            copy.SetActive(true);
            return copy;
        }
        else
        {
            return null;
        }
    }
}
