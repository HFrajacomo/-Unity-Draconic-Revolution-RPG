﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkLoader : MonoBehaviour
{
	public GameObject prefabChunk;
	public const int renderDistance = 5;
	public Dictionary<ChunkPos, Chunk> chunks = new Dictionary<ChunkPos, Chunk>();

    // Start is called before the first frame update
    void Start()
    {
		//chunk = GameObject.Find("Chunk");	

        for(int x=-renderDistance; x<renderDistance;x++){
        	for(int z=-renderDistance; z< renderDistance;z++){
        		ChunkPos position = new ChunkPos(x*Chunk.chunkWidth, z*Chunk.chunkWidth);
        		//chunks.Add(position, new Chunk(x*Chunk.chunkWidth, z*Chunk.chunkWidth));
        		//Chunk c = Instantiate(chunks[position], new Vector3(position.x, 0, position.z), Quaternion.identity);
        		BuildChunk(position);
        	}
        }
    }

    private void BuildChunk(ChunkPos pos){
    	Chunk chunk;
    	GameObject chunkGO = Instantiate(prefabChunk, new Vector3(pos.x, 0, pos.z), Quaternion.identity);
    	chunk = chunkGO.GetComponent<Chunk>();
    	chunk.GenerateRandomChunk();
    	chunk.BuildChunk();
    	chunk.gameObject.SetActive(true);
    }

}

public struct ChunkPos{
	public int x;
	public int z;

	public ChunkPos(int a, int b){
		this.x = a;
		this.z = b;
	}
}
