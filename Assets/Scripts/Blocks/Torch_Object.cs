﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch_Object : BlocklikeObject
{
	public GameObject fireVFX;

	public Torch_Object(){
		this.name = "Torch";
		this.solid = false;
		this.transparent = true;
		this.invisible = false;
		this.liquid = false;
		this.prefabName = "Torch_Object";
		this.centeringOffset = new Vector3(0f,-0.3f,0f);
		this.scaling = new Vector3(1f, 2f, 1f);

		this.fireVFX = GameObject.Find("----- PrefabVFX -----/FireVFX");
	}

	public override int OnInteract(ChunkPos pos, int blockX, int blockY, int blockZ, ChunkLoader cl){
		return 0;
	}

	public override int OnPlace(ChunkPos pos, int blockX, int blockY, int blockZ){
		this.fireVFX = GameObject.Instantiate(this.fireVFX, new Vector3(pos.x*Chunk.chunkWidth + blockX, blockY + 0.2f, pos.z*Chunk.chunkWidth + blockZ), Quaternion.identity);
		this.fireVFX.name = "Torch (" + pos.x + ", " + pos.z + ") [" + blockX + ", " + blockY + ", " + blockZ + "]";
		return 0;
	}
}