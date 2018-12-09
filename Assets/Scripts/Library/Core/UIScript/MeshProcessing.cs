using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshProcessing : MonoBehaviour {

    Shader shader;
    public void Processing()
    {
        shader = Shader.Find("Standard");
        int c = transform.childCount;
        for (int i = 0; i < c; i++)
            ProcessSon(transform.GetChild(i));
    }
    void ProcessSon(Transform trans)
    {
 
        var mf= trans.GetComponent<MeshFilter>();
        if(mf!=null)
        {
            var mesh = mf.mesh;
            if(mesh!=null)
            {
                if ( mesh.vertexCount>8)
                {
                    try
                    {
                        var mr = trans.GetComponent<MeshRenderer>();
                        mr.material = new Material(shader);
                        var mc = trans.GetComponent<MeshCollider>();
                        if (mc == null)
                        {
                            mc = trans.gameObject.AddComponent<MeshCollider>();
                        }
                        mc.convex = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(name);
                    }
                }
            }
        }
    }
}
