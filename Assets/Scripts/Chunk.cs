﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Chunk : MonoBehaviour
{

	public VoxelData data;
	public static int chunkWidth = 16;
	public static int chunkDepth = 32;
	public BlockHandler blockHandler;

	// Sets UV mapping for a given block
	private Vector2[] AddTexture(int textureID){ 
		Vector2[] UVs = new Vector2[4];
		float x = textureID%Blocks.atlasSizeX;
		float y = Mathf.FloorToInt(textureID/Blocks.atlasSizeX);
 
		x *= 1f / Blocks.atlasSizeX;
		y *= 1f / Blocks.atlasSizeY;

		UVs[0] = new Vector2(x,y);
		UVs[1] = new Vector2(x,y+(1f/Blocks.atlasSizeY));
		UVs[2] = new Vector2(x+(1f/Blocks.atlasSizeX),y+(1f/Blocks.atlasSizeY));
		UVs[3] = new Vector2(x+(1f/Blocks.atlasSizeX),y);
	
		return UVs;
	}

	// Generates random holes in terrain
	public void GenerateRandomChunk(){
		int[,,] voxdata = new int[chunkWidth,chunkDepth,chunkWidth];
		int rnd;
		for(int x=0; x < chunkWidth;x++){
			for(int y=0; y < chunkDepth; y++){
				for(int z=0; z < chunkWidth; z++){
					rnd = Random.Range(0,4);
					if(rnd == 0)
						voxdata[x,y,z] = 0;
					else
						voxdata[x,y,z] = 1;
				}
			}
		}
		data = new VoxelData(voxdata);
	}

	public void BuildOnVoxelData(VoxelData vd){
		this.data = vd;	
	}

	public void BuildChunk(){
		Mesh mesh = new Mesh();

    	List<Vector3> vertices = new List<Vector3>();
    	List<int> triangles = new List<int>();
    	List<Vector2> UVs = new List<Vector2>();

    	for(int x=0; x<data.GetWidth(); x++){
    		for(int y=0; y<data.GetHeight(); y++){
    			for(int z=0; z<data.GetDepth(); z++){
    				// Air block
	    			if(data.GetCell(x,y,z) == 0){
	    				continue;
	    			}
	    			//Make Cube
			    	for(int i=0; i<6; i++){
			    		// Air Check
			    		if(blockHandler.blockTypes[data.GetNeighbor(x, y, z, (Direction)i)].transparent){
			    			// Make Face
					    	vertices.AddRange(CubeMeshData.faceVertices(i, 0.5f, new Vector3(x,y,z)));
					    	
					    	UVs.AddRange(AddTexture(1));
					    	
					    	int vCount = vertices.Count;

					    	triangles.Add(vCount -4);
					    	triangles.Add(vCount -4 +1);
					    	triangles.Add(vCount -4 +2);
					    	triangles.Add(vCount -4);
					    	triangles.Add(vCount -4 +2);
					    	triangles.Add(vCount -4 +3);
			    		}

			    	}

	    		}
	    	}
    	}


    	mesh.Clear(); 
    	mesh.vertices = vertices.ToArray();
    	mesh.triangles = triangles.ToArray();
    	mesh.uv = UVs.ToArray();
    	mesh.RecalculateNormals();

    	GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}
