﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXLoader : MonoBehaviour
{
	public Dictionary<ChunkPos, Dictionary<string, GameObject>> data;

    // Start is called before the first frame update
    void Start()
    {
		data = new Dictionary<ChunkPos, Dictionary<string, GameObject>>(); 
    }

    // Registers a new chunk to VFXLoader
    public void NewChunk(ChunkPos pos){
    	data.Add(pos, new Dictionary<string, GameObject>());
    }

    // Unregisters a chunk from VFXLoader
    public void RemoveChunk(ChunkPos pos){
        // Destroy all VFX once chunk is unloaded
        foreach(string key in this.data[pos].Keys)
            Destroy(this.data[pos][key]);

    	data.Remove(pos);
    }

    // Adds GameObject to Chunk Dict and sets it to active
    public void Add(ChunkPos pos, GameObject go, bool active=true){
    	if(data[pos].ContainsKey(go.name)){
            Destroy(this.data[pos][go.name]);
            data[pos].Remove(go.name);
        }

        data[pos].Add(go.name, go);

    	if(!active){
    		data[pos][go.name].SetActive(false);
    	}
    }

    // Removes an GameObject from Chunk Dict
    public void Remove(ChunkPos pos, string name){
        Destroy(this.data[pos][name]);
    	data[pos].Remove(name);
    }

}