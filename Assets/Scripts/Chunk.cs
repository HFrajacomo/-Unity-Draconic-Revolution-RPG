﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{

	// Chunk Settings
	public VoxelData data;
	public VoxelMetadata metadata;
	public static int chunkWidth = 16;
	public static int chunkDepth = 100;
	public ChunkPos pos;
	public string biomeName;
	public Point4D features;

	// Unity Settings
	public ChunkRenderer renderer;
	public MeshFilter meshFilter;
	public MeshCollider meshCollider;
	public GameObject obj = new GameObject();
	public BlockEncyclopedia blockBook;

	// Cache Information
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> transparentTris = new List<int>();
    private List<int> liquidTris = new List<int>();
    private List<int> triangles = new List<int>();
    private List<Vector2> UVs = new List<Vector2>();
    private Mesh mesh;

	public Chunk(ChunkPos pos, ChunkRenderer r, BlockEncyclopedia be){
		this.pos = pos;
		this.obj.name = "Chunk " + pos.x + ", " + pos.z;
		this.renderer = r;
		this.obj.transform.SetParent(this.renderer.transform);
		this.obj.transform.position = new Vector3(pos.x * chunkWidth, 0f, pos.z * chunkWidth);

		this.metadata = new VoxelMetadata(); // May change when chunk loads

		this.obj.AddComponent<MeshFilter>();
		this.obj.AddComponent<MeshRenderer>();
		this.obj.AddComponent<MeshCollider>();
		this.meshFilter = this.obj.GetComponent<MeshFilter>();
		this.meshCollider = this.obj.GetComponent<MeshCollider>();
		this.obj.GetComponent<MeshRenderer>().materials = this.renderer.GetComponent<MeshRenderer>().materials;
		this.blockBook = be;
		this.obj.layer = 8;
	}

	public void BuildOnVoxelData(VoxelData vd){
		this.data = vd;
	}


	public void BuildChunk(){
		int thisBlock;
		int neighborBlock;

    	for(int x=0; x<data.GetWidth(); x++){
    		for(int y=0; y<data.GetHeight(); y++){
    			for(int z=0; z<data.GetDepth(); z++){
    				thisBlock = data.GetCell(x,y,z);

    				// If is a full block
    				if(thisBlock >= 0){

	    				// If invisible block
		    			if(blockBook.blocks[thisBlock].invisible){
		    				continue;
		    			}
		    			//Make Cube
				    	for(int i=0; i<6; i++){
				    		// Air Check
				    		neighborBlock = data.GetNeighbor(x, y, z, (Direction)i);

				    		// If neighbor is a block
				    		if(neighborBlock >= 0){

				    			// Handles Liquid chunks
				    			if(blockBook.blocks[thisBlock].liquid && blockBook.blocks[neighborBlock].liquid)
				    				continue;

					    		if(blockBook.blocks[neighborBlock].transparent || blockBook.blocks[neighborBlock].invisible){
					    			
					    			vertices.AddRange(CubeMeshData.faceVertices(i, 0.5f, new Vector3(x,y,z)));
					    			int vCount = vertices.Count;

					    			// Handling Liquid and non-liquid blocks
					    			if(!blockBook.blocks[thisBlock].liquid){
							    		UVs.AddRange(blockBook.blocks[thisBlock].AddTexture((Direction)i));
								    	triangles.Add(vCount -4);
								    	triangles.Add(vCount -4 +1);
								    	triangles.Add(vCount -4 +2);
								    	triangles.Add(vCount -4);
								    	triangles.Add(vCount -4 +2);
								    	triangles.Add(vCount -4 +3);
					    			}
							    	else{
							    		UVs.AddRange(blockBook.blocks[thisBlock].LiquidTexture(x, z));
								    	liquidTris.Add(vCount -4);
								    	liquidTris.Add(vCount -4 +1);
								    	liquidTris.Add(vCount -4 +2);
								    	liquidTris.Add(vCount -4);
								    	liquidTris.Add(vCount -4 +2);
								    	liquidTris.Add(vCount -4 +3);							    	
							    	}
							    	


					    		}
					    	}
					    	// If neighbor is an asset
					    	else{
					    		neighborBlock = (neighborBlock * -1) - 1;

				    			// Handles Liquid chunks
				    			if(blockBook.blocks[thisBlock].liquid && blockBook.objects[neighborBlock].liquid)
				    				continue;

					    		if(blockBook.objects[neighborBlock].transparent || blockBook.objects[neighborBlock].invisible){
					    			// Make Face
							    	vertices.AddRange(CubeMeshData.faceVertices(i, 0.5f, new Vector3(x,y,z)));
							    	int vCount = vertices.Count;

			    					// Handling Liquid and non-liquid blocks
					    			if(!blockBook.blocks[thisBlock].liquid){
								    	UVs.AddRange(blockBook.blocks[thisBlock].AddTexture((Direction)i));
								    	triangles.Add(vCount -4);
								    	triangles.Add(vCount -4 +1);
								    	triangles.Add(vCount -4 +2);
								    	triangles.Add(vCount -4);
								    	triangles.Add(vCount -4 +2);
								    	triangles.Add(vCount -4 +3);
							  		}
							  		else{
							    		UVs.AddRange(blockBook.blocks[thisBlock].LiquidTexture(x, z));
								    	liquidTris.Add(vCount -4);
								    	liquidTris.Add(vCount -4 +1);
								    	liquidTris.Add(vCount -4 +2);
								    	liquidTris.Add(vCount -4);
								    	liquidTris.Add(vCount -4 +2);
								    	liquidTris.Add(vCount -4 +3);							  			
							  		}
					    		}
					    	}					    		
					    }

				    }
				    // If is an object-type block
				    else{
				    	thisBlock = (thisBlock * -1) - 1;

				    	if(blockBook.objects[thisBlock].invisible){
				    		continue;
				    	}

				    	for(int i=0; i<6; i++){
				    		neighborBlock = data.GetNeighbor(x, y, z, (Direction)i);

				    		// If is a full block
				    		if(neighborBlock >= 0){

				    			// Handles Liquid chunks
				    			if(blockBook.objects[thisBlock].liquid && blockBook.blocks[neighborBlock].liquid)
				    				continue;

					    		if(blockBook.blocks[neighborBlock].transparent || blockBook.blocks[neighborBlock].invisible){

					    			int vCount = vertices.Count;

					    			// If block has special Rotation Rules
					    			if(blockBook.objects[thisBlock].needsRotation)
					    				vertices.AddRange(blockBook.objects[thisBlock].ToWorldSpace(new Vector3(x,y,z), blockBook.objects[thisBlock].ApplyRotation(this, x,y,z)));
					    			else
										vertices.AddRange(blockBook.objects[thisBlock].ToWorldSpace(new Vector3(x,y,z)));

					    			foreach(int tri in blockBook.objects[thisBlock].mesh.triangles)
					    				transparentTris.Add(tri + vCount);

					    			UVs.AddRange(blockBook.objects[thisBlock].mesh.uv);
				    				break;
				    			}
				    		}

				    		// If is an object type block
				    		else{
					    		neighborBlock = (neighborBlock * -1) - 1;

				    			// Handles Liquid chunks
				    			if(blockBook.objects[thisBlock].liquid && blockBook.objects[neighborBlock].liquid)
				    				continue;

					    		if(blockBook.objects[neighborBlock].transparent || blockBook.objects[neighborBlock].invisible){
					    			int vCount = vertices.Count;

					    			// If block has special Rotation Rules
					    			if(blockBook.objects[thisBlock].needsRotation)
					    				vertices.AddRange(blockBook.objects[thisBlock].ToWorldSpace(new Vector3(x,y,z), blockBook.objects[thisBlock].ApplyRotation(this, x,y,z)));
					    			else
										vertices.AddRange(blockBook.objects[thisBlock].ToWorldSpace(new Vector3(x,y,z)));
					    			
					    			foreach(int tri in blockBook.objects[thisBlock].mesh.triangles)
					    				transparentTris.Add(tri + vCount);

					    			UVs.AddRange(blockBook.objects[thisBlock].mesh.uv);
				    				break;
				    			}				    			
				    		}
				    	}

				    }
	    		}
	    	}
    	}

		BuildMesh();
    }

    // Builds meshes from verts, UVs and tris from different layers
    private void BuildMesh(){
    	mesh = new Mesh();
    	mesh.Clear();
    	mesh.subMeshCount = 3;

    	mesh.vertices = vertices.ToArray();
    	mesh.SetTriangles(triangles.ToArray(), 0);
    	this.meshCollider.sharedMesh = mesh;

    	mesh.SetTriangles(transparentTris.ToArray(), 1);
    	mesh.SetTriangles(liquidTris.ToArray(), 2);

    	mesh.uv = UVs.ToArray();
    	mesh.RecalculateNormals();

    	vertices.Clear();
    	triangles.Clear();
    	transparentTris.Clear();
    	liquidTris.Clear();
    	UVs.Clear();

    	this.meshFilter.sharedMesh = mesh;
    }
}
