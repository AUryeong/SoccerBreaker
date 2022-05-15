using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomUtil
{
    public static T SelectOne<T>(List<T> ts)
    {
        return ts[Random.Range(0, ts.Count)];
    }
}
