using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using UnityEngine;

public class TestSceneData : MonoBehaviour
{
    [SerializeReference]
    public SceneReferenceData DataWithSerializeAttr;
    [SerializeReference]
    public SceneReference ReferenceWithSerializeAttr;

    public SceneReferenceData Data;
    public SceneReference Reference;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
