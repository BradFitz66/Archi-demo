using Archi.Core.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformUtility
{
    /// <summary>
    /// Clear all children of a transform
    /// </summary>
    /// <param name="transform">The transform to clear</param>
    public static void ClearChildren(this Transform transform)
    {
        Transform[] child=new Transform[transform.childCount];
        for(int i=0; i<child.Length; i++)
        {
            child[i] = transform.GetChild(i);
        }
        foreach (Transform t in child)
        {
            GameObject.DestroyImmediate(t.gameObject);
        }

    }
}
