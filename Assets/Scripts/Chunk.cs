﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;

public class Chunk
{
	// Chunk Settings
	public VoxelData data;
	public VoxelMetadata metadata;
	public static readonly int chunkWidth = 16;
	public static readonly int chunkDepth = 100;
	public static float chunkWidthMult = 15.99f; 
	public ChunkPos pos;
	public string biomeName;
	public byte needsGeneration;
	public float4 features;
	public string lastVisitedTime;

	// Draw Flags
	/*
	private bool xPlusDrawn = false;
	private bool zPlusDrawn = false;
	private bool xMinusDrawn = false;
	private bool zMinusDrawn = false;
	*/
	public bool drawMain = false;

	// Unity Settings
	public ChunkRenderer renderer;
	public MeshFilter meshFilter;
	public MeshCollider meshCollider;
	public GameObject obj;
	public BlockEncyclopedia blockBook;
	public ChunkLoader loader;

	// Cache Information
    private List<Vector3> vertices = new List<Vector3>();
    private int[] specularTris;
    private int[] liquidTris;
    private int[] assetTris;
    private int[] triangles;
  	private List<Vector2> UVs = new List<Vector2>();

    // Assets Cache
    private List<ushort> cacheCodes = new List<ushort>();
    private List<Vector3> cacheVertsv3 = new List<Vector3>();
    private List<int> cacheTris = new List<int>();
    private List<Vector2> cacheUVv2 = new List<Vector2>();
    private List<int> indexVert = new List<int>();
    private List<int> indexUV = new List<int>();
    private List<int> indexTris = new List<int>();
    private List<Vector3> scalingFactor = new List<Vector3>();

    private Mesh mesh;

	public Chunk(ChunkPos pos, ChunkRenderer r, BlockEncyclopedia be, ChunkLoader loader, bool fromMemory=false){
		this.pos = pos;
		this.needsGeneration = 0;
		this.renderer = r;
		this.loader = loader;

		// Game Object Settings
		this.obj = new GameObject();
		this.obj.name = "Chunk " + pos.x + ", " + pos.z;
		this.obj.transform.SetParent(this.renderer.transform);
		this.obj.transform.position = new Vector3(pos.x * chunkWidth, 0f, pos.z * chunkWidth);


		if(fromMemory)
			this.data = new VoxelData();
		this.metadata = new VoxelMetadata();

		this.obj.AddComponent<MeshFilter>();
		this.obj.AddComponent<MeshRenderer>();
		this.obj.AddComponent<MeshCollider>();
		this.meshFilter = this.obj.GetComponent<MeshFilter>();
		this.meshCollider = this.obj.GetComponent<MeshCollider>();
		this.obj.GetComponent<MeshRenderer>().materials = this.renderer.GetComponent<MeshRenderer>().materials;
		this.blockBook = be;
		this.obj.layer = 8;

		this.mesh = new Mesh();
	}

	// Dummy Chunk Generation
	// CANNOT BE USED TO DRAW, ONLY TO ADD ELEMENTS AND SAVE
	public Chunk(ChunkPos pos){
		this.biomeName = "Plains";
		this.pos = pos;
		this.needsGeneration = 1;

		this.data = new VoxelData(new ushort[Chunk.chunkWidth*Chunk.chunkDepth*Chunk.chunkWidth]);

		this.metadata = new VoxelMetadata();
	}

	// Clone
	public Chunk Clone(){
		Chunk c = new Chunk(this.pos, this.renderer, this.blockBook, this.loader);

		c.biomeName = this.biomeName;
		c.data = new VoxelData(this.data.GetData());
		c.metadata = new VoxelMetadata(this.metadata);

		return c;
	}
	

	public void BuildOnVoxelData(VoxelData vd){
		this.data = vd;
	}

	public void BuildVoxelMetadata(VoxelMetadata vm){
		this.metadata = vm;
	}

	public void BuildSideBorder(bool reload=false, bool reloadXM=false, bool reloadXm=false, bool reloadZM=false, bool reloadZm=false){

	}

	// Build the X- or Z- chunk border
	/*
	public void BuildSideBorder(bool reload=false, bool reloadXM=false, bool reloadXm=false, bool reloadZM=false, bool reloadZm=false){
		ushort thisBlock;
		ushort neighborBlock;
		bool skip;
		int meshVertCount = this.meshFilter.sharedMesh.vertices.Length;

		if(reload){
			this.xMinusDrawn = false;
			this.xPlusDrawn = false;
			this.zMinusDrawn = false;
			this.zPlusDrawn = false;
		}
		if(reloadXM)
			this.xPlusDrawn = false;
		if(reloadXm)
			this.xMinusDrawn = false;
		if(reloadZM)
			this.zPlusDrawn = false;
		if(reloadZm)
			this.zMinusDrawn = false;


		// X- Side analysis

		ChunkPos targetChunk = new ChunkPos(this.pos.x-1, this.pos.z);

		// Stop immediately if it's a final chunk
		if(loader.chunks.ContainsKey(targetChunk) && !xMinusDrawn){

			this.xMinusDrawn = true;

			for(int y=0; y<Chunk.chunkDepth; y++){
				for(int z=0; z<Chunk.chunkWidth; z++){
					skip = false;
					thisBlock = data.GetCell(0,y,z);
					neighborBlock = loader.chunks[targetChunk].data.GetCell(chunkWidth-1, y, z);

					// Air handler
					if(thisBlock == 0)
						continue;

	    			// Handles Liquid chunks
	    			if(CheckLiquids(thisBlock, neighborBlock))
	    				continue;

	    			// Main Drawing Handling
		    		if(CheckPlacement(neighborBlock)){
				    	LoadMesh(0, y, z, (int)Direction.West, thisBlock, false, ref skip, lookahead:meshVertCount);
				    	if(skip)
				    		break;
		    		}
				}
			}
		}
				
			
		// X+ Side analysis

		targetChunk = new ChunkPos(this.pos.x+1, this.pos.z);

		// Stop immediately if it's a final chunk
		if(loader.chunks.ContainsKey(targetChunk) && !xPlusDrawn){

			this.xPlusDrawn = true;

			for(int y=0; y<Chunk.chunkDepth; y++){
				for(int z=0; z<Chunk.chunkWidth; z++){
					skip = false;
					thisBlock = data.GetCell(chunkWidth-1,y,z);
					neighborBlock = loader.chunks[targetChunk].data.GetCell(0, y, z);

					// Air handler
					if(thisBlock == 0)
						continue;

	    			// Handles Liquid chunks
	    			if(CheckLiquids(thisBlock, neighborBlock))
	    				continue;

	    			// Main Drawing Handling
		    		if(CheckPlacement(neighborBlock)){
				    	LoadMesh(chunkWidth-1, y, z, (int)Direction.East, thisBlock, false, ref skip, lookahead:meshVertCount);
				    	if(skip)
				    		break;
		    		}	
				}
			}
		}
		

		// If the side being analyzed is the Z- Side

		targetChunk = new ChunkPos(this.pos.x, this.pos.z-1);
		// Stop immediately if it's a final chunk
		if(loader.chunks.ContainsKey(targetChunk) && !zMinusDrawn){

			this.zMinusDrawn = true;

			for(int y=0; y<Chunk.chunkDepth; y++){
				for(int x=0; x<Chunk.chunkWidth; x++){
					skip = false;
					thisBlock = data.GetCell(x,y,0);
					neighborBlock = loader.chunks[targetChunk].data.GetCell(x, y, chunkWidth-1);

					// Air handler
					if(thisBlock == 0)
						continue;

	    			// Handles Liquid chunks
	    			if(CheckLiquids(thisBlock, neighborBlock))
	    				continue;

	    			// Main Drawing Handling
		    		if(CheckPlacement(neighborBlock)){
				    	LoadMesh(x, y, 0, (int)Direction.South, thisBlock, false, ref skip, lookahead:meshVertCount);
				    	if(skip)
				    		break;
		    		}	
				}
			}
		}	

		// Z+ Side Analysis

		targetChunk = new ChunkPos(this.pos.x, this.pos.z+1);

		// Stop immediately if it's a final chunk
		if(loader.chunks.ContainsKey(targetChunk) && !zPlusDrawn){

			this.zPlusDrawn = true;

			for(int y=0; y<Chunk.chunkDepth; y++){
				for(int x=0; x<Chunk.chunkWidth; x++){
					skip = false;
					thisBlock = data.GetCell(x,y,chunkWidth-1);
					neighborBlock = loader.chunks[targetChunk].data.GetCell(x, y, 0);

					// Air handler
					if(thisBlock == 0)
						continue;

	    			// Handles Liquid chunks
	    			if(CheckLiquids(thisBlock, neighborBlock))
	    				continue;

	    			// Main Drawing Handling
		    		if(CheckPlacement(neighborBlock)){
				    	LoadMesh(x, y, chunkWidth-1, (int)Direction.North, thisBlock, false, ref skip, lookahead:meshVertCount);
				    	if(skip)
				    		break;
		    		}	
				}
			}
		}				

		// Only draw if there's something to draw
		if(vertices.Count > 0)
			AddToMesh();
		else{
			vertices.Clear();
    		triangles.Clear();
    		specularTris.Clear();
    		liquidTris.Clear();
    		UVs.Clear();
		}
	}
	*/


	// Builds the chunk mesh data excluding the X- and Z- chunk border
	public void BuildChunk(bool load=false, bool pregenReload=false){

		NativeArray<ushort> blockdata = new NativeArray<ushort>(this.data.GetData(), Allocator.TempJob);
		NativeArray<ushort> statedata = new NativeArray<ushort>(this.metadata.GetStateData(), Allocator.TempJob);
		
		NativeList<int3> loadCoordList = new NativeList<int3>(0, Allocator.TempJob);
		NativeList<ushort> loadCodeList = new NativeList<ushort>(0, Allocator.TempJob);
		NativeList<int3> loadAssetList = new NativeList<int3>(0, Allocator.TempJob);

		NativeList<int> normalTris = new NativeList<int>(0, Allocator.TempJob);
		NativeList<int> specularTris = new NativeList<int>(0, Allocator.TempJob);
		NativeList<int> liquidTris = new NativeList<int>(0, Allocator.TempJob);
		NativeList<Vector3> verts = new NativeList<Vector3>(0, Allocator.TempJob);
		NativeList<Vector2> UVs = new NativeList<Vector2>(0, Allocator.TempJob);
		NativeArray<Vector3> cacheCubeVert = new NativeArray<Vector3>(4, Allocator.TempJob);
		NativeArray<Vector2> cacheCubeUV = new NativeArray<Vector2>(4, Allocator.TempJob);

		// Cached from Block Encyclopedia ECS
		NativeArray<bool> blockTransparent = new NativeArray<bool>(BlockEncyclopediaECS.blockTransparent, Allocator.TempJob);
		NativeArray<bool> objectTransparent = new NativeArray<bool>(BlockEncyclopediaECS.objectTransparent, Allocator.TempJob);
		NativeArray<bool> blockLiquid = new NativeArray<bool>(BlockEncyclopediaECS.blockLiquid, Allocator.TempJob);
		NativeArray<bool> objectLiquid = new NativeArray<bool>(BlockEncyclopediaECS.objectLiquid, Allocator.TempJob);
		NativeArray<bool> blockLoad = new NativeArray<bool>(BlockEncyclopediaECS.blockLoad, Allocator.TempJob);
		NativeArray<bool> objectLoad = new NativeArray<bool>(BlockEncyclopediaECS.objectLoad, Allocator.TempJob);
		NativeArray<bool> blockInvisible = new NativeArray<bool>(BlockEncyclopediaECS.blockInvisible, Allocator.TempJob);
		NativeArray<bool> objectInvisible = new NativeArray<bool>(BlockEncyclopediaECS.objectInvisible, Allocator.TempJob);
		NativeArray<byte> blockMaterial = new NativeArray<byte>(BlockEncyclopediaECS.blockMaterial, Allocator.TempJob);
		NativeArray<byte> objectMaterial = new NativeArray<byte>(BlockEncyclopediaECS.objectMaterial, Allocator.TempJob);
		NativeArray<int3> blockTiles = new NativeArray<int3>(BlockEncyclopediaECS.blockTiles, Allocator.TempJob);


		// Threading Job
		BuildChunkJob bcJob = new BuildChunkJob{
			load = load,
			data = blockdata,
			state = statedata,
			loadOutList = loadCoordList,
			loadAssetList = loadAssetList,
			verts = verts,
			UVs = UVs,
			normalTris = normalTris,
			specularTris = specularTris,
			liquidTris = liquidTris,
			cacheCubeVert = cacheCubeVert,
			cacheCubeUV = cacheCubeUV,
			blockTransparent = blockTransparent,
			objectTransparent = objectTransparent,
			blockLiquid = blockLiquid,
			objectLiquid = objectLiquid,
			blockLoad = blockLoad,
			objectLoad = objectLoad,
			blockInvisible = blockInvisible,
			objectInvisible = objectInvisible,
			blockMaterial = blockMaterial,
			objectMaterial = objectMaterial,
			blockTiles = blockTiles
		};
		JobHandle job = bcJob.Schedule();
		job.Complete();


		// MULTITHREADING FOR ASSET LAYER
		this.indexVert.Add(0);
		this.indexUV.Add(0);
		this.indexTris.Add(0);

		foreach(int3 coord in loadAssetList.ToArray()){
			ushort assetCode = this.data.GetCell(coord);

			if(!this.cacheCodes.Contains(assetCode)){
				this.cacheCodes.Add(assetCode);
				this.cacheVertsv3.AddRange(blockBook.objects[ushort.MaxValue-assetCode].mesh.vertices);
				this.cacheTris.AddRange(blockBook.objects[ushort.MaxValue-assetCode].mesh.GetTriangles(0));
				this.cacheUVv2.AddRange(blockBook.objects[ushort.MaxValue-assetCode].mesh.uv);
				this.indexVert.Add(this.indexVert[indexVert.Count-1] + blockBook.objects[ushort.MaxValue-assetCode].mesh.vertices.Length);
				this.indexTris.Add(this.indexTris[indexTris.Count-1] + blockBook.objects[ushort.MaxValue-assetCode].mesh.GetTriangles(0).Length);
				this.indexUV.Add(this.indexUV[indexUV.Count-1] + blockBook.objects[ushort.MaxValue-assetCode].mesh.uv.Length);
				this.scalingFactor.Add(BlockEncyclopediaECS.objectScaling[ushort.MaxValue-assetCode]);
			}
		}

		// ToLoad() Event Trigger
		foreach(int3 coord in loadCoordList.ToArray()){
			ushort assetCode = this.data.GetCell(coord);

			blockBook.objects[ushort.MaxValue-assetCode].OnLoad(this.pos, coord.x, coord.y, coord.z, this.loader);
		}


		NativeList<Vector3> meshVerts = new NativeList<Vector3>(0, Allocator.TempJob);
		NativeList<Vector2> meshUVs = new NativeList<Vector2>(0, Allocator.TempJob);
		NativeList<int> meshTris = new NativeList<int>(0, Allocator.TempJob);
		NativeList<ushort> blockCodes = new NativeList<ushort>(0, Allocator.TempJob);
		blockCodes.CopyFrom(this.cacheCodes.ToArray());
		NativeList<int> vertsOffset = new NativeList<int>(0, Allocator.TempJob);
		vertsOffset.CopyFrom(this.indexVert.ToArray());
		NativeList<int> trisOffset = new NativeList<int>(0, Allocator.TempJob);
		trisOffset.CopyFrom(this.indexTris.ToArray());
		NativeList<int> UVOffset = new NativeList<int>(0, Allocator.TempJob);
		UVOffset.CopyFrom(this.indexUV.ToArray());
		NativeArray<Vector3> loadedVerts = new NativeArray<Vector3>(this.cacheVertsv3.ToArray(), Allocator.TempJob);
		NativeArray<Vector2> loadedUV = new NativeArray<Vector2>(this.cacheUVv2.ToArray(), Allocator.TempJob);
		NativeArray<int> loadedTris = new NativeArray<int>(this.cacheTris.ToArray(), Allocator.TempJob);
		NativeArray<Vector3> scaling = new NativeArray<Vector3>(this.scalingFactor.ToArray(), Allocator.TempJob);

		PrepareAssetsJob paJob = new PrepareAssetsJob{
			vCount = verts.Length,

			meshVerts = meshVerts,
			meshTris = meshTris,
			meshUVs = meshUVs,
			scaling = scaling,

			coords = loadAssetList,
			blockCodes = blockCodes,
			blockdata = blockdata,

			vertsOffset = vertsOffset,
			trisOffset = trisOffset,
			UVOffset = UVOffset,

			loadedVerts = loadedVerts,
			loadedUV = loadedUV,
			loadedTris = loadedTris
		};
		job = paJob.Schedule();
		job.Complete();

		// Convert data back
		this.vertices.AddRange(verts.ToArray());

		this.vertices.AddRange(meshVerts.ToArray());
		this.triangles = normalTris.ToArray();
		this.assetTris = meshTris.ToArray();

		this.specularTris = specularTris.ToArray();
		this.liquidTris = liquidTris.ToArray();

		this.UVs.AddRange(UVs.ToArray());
		this.UVs.AddRange(meshUVs.ToArray());


		/*
		STILL HAS TO TRIGGER ONLOAD EVENTS
		*/

		// Dispose Bin
		verts.Dispose();
		normalTris.Dispose();
		specularTris.Dispose();
		liquidTris.Dispose();
		blockdata.Dispose();
		statedata.Dispose();
		loadCoordList.Dispose();
		blockTransparent.Dispose();
		objectTransparent.Dispose();
		blockLiquid.Dispose();
		objectLiquid.Dispose();
		blockLoad.Dispose();
		objectLoad.Dispose();
		blockInvisible.Dispose();
		objectInvisible.Dispose();
		blockMaterial.Dispose();
		objectMaterial.Dispose();
		blockTiles.Dispose();
		cacheCubeVert.Dispose();
		cacheCubeUV.Dispose();
		loadCodeList.Dispose();
		meshVerts.Dispose();
		meshTris.Dispose();
		blockCodes.Dispose();
		vertsOffset.Dispose();
		trisOffset.Dispose();
		UVOffset.Dispose();
		loadedVerts.Dispose();
		loadedTris.Dispose();
		loadedUV.Dispose();
		loadAssetList.Dispose();
		scaling.Dispose();
		UVs.Dispose();
		meshUVs.Dispose();


		BuildMesh();


		// Dispose Asset Cache
		cacheCodes.Clear();
		cacheVertsv3.Clear();
		cacheTris.Clear();
		cacheUVv2.Clear();
		indexVert.Clear();
		indexUV.Clear();
		indexTris.Clear();
		scalingFactor.Clear();
    	this.vertices.Clear(); // May need to change when doing sides
    	this.triangles = null;
    	this.specularTris = null;
    	this.liquidTris = null;
    	this.assetTris = null;
    	this.UVs.Clear();

		this.drawMain = true;
    }

    // Builds meshes from verts, UVs and tris from different layers
    private void BuildMesh(){
    	mesh.Clear();

    	if(this.vertices.Count >= ushort.MaxValue){
    		mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
    	}

    	mesh.subMeshCount = 4;

    	mesh.vertices = this.vertices.ToArray();
    	mesh.SetTriangles(triangles, 0);

    	this.meshCollider.sharedMesh = mesh;

    	mesh.SetTriangles(specularTris, 1);
    	mesh.SetTriangles(liquidTris, 2);
    	mesh.SetTriangles(assetTris, 3);

    	mesh.uv = this.UVs.ToArray();

    	mesh.RecalculateNormals();

    	this.meshFilter.sharedMesh = mesh;
    }

    // Adds verts, UVs and tris to meshes
    private void AddToMesh(){
    	List<Vector3> newVerts = new List<Vector3>();
    	List<int>[] newTris = {new List<int>(), new List<int>(), new List<int>(), new List<int>()};
    	List<Vector2> newUVs = new List<Vector2>();
    	mesh = new Mesh();
    	mesh.subMeshCount = 4;

    	// Add to buffer the current mesh data
    	newVerts.AddRange(this.meshFilter.sharedMesh.vertices);
    	newTris[0].AddRange(this.meshFilter.sharedMesh.GetTriangles(0));
    	newTris[1].AddRange(this.meshFilter.sharedMesh.GetTriangles(1));
    	newTris[2].AddRange(this.meshFilter.sharedMesh.GetTriangles(2));
    	//newTris[3].AddRange()
    	newUVs.AddRange(this.meshFilter.sharedMesh.uv);

    	// Add to buffer the data received from side analysis
    	newVerts.AddRange(this.vertices.ToArray());


    	newTris[0].AddRange(triangles);
    	newTris[1].AddRange(specularTris);
    	newTris[2].AddRange(liquidTris);

    	foreach(float2 f in this.UVs){
    		newUVs.Add((Vector2)f);
    	}

    	mesh.vertices = newVerts.ToArray();
    	mesh.SetTriangles(newTris[0].ToArray(), 0);
    	mesh.uv = newUVs.ToArray();

    	this.meshCollider.sharedMesh = mesh;

    	mesh.SetTriangles(newTris[1].ToArray(), 1);
    	mesh.SetTriangles(newTris[2].ToArray(), 2);

    	mesh.RecalculateNormals();

    	this.meshFilter.sharedMesh = mesh;

    	this.vertices.Clear();
    	this.UVs.Clear();
    }


}



/*
MULTITHREADING
*/
[BurstCompile]
public struct BuildChunkJob : IJob{
	// BuildChunk() input vars
	[ReadOnly]
	public bool load;

	[ReadOnly]
	public NativeArray<ushort> data; // Voxeldata
	[ReadOnly]
	public NativeArray<ushort> state; // VoxelMetadata.state

	// OnLoad Event Trigger List
	public NativeList<int3> loadOutList;
	public NativeList<int3> loadAssetList;

	// Rendering Primitives
	public NativeList<Vector3> verts;
	public NativeList<Vector2> UVs;

	// Render Thread Triangles
	public NativeList<int> normalTris;
	public NativeList<int> specularTris;
	public NativeList<int> liquidTris;

	// Cache
	public NativeArray<Vector3> cacheCubeVert;
	public NativeArray<Vector2> cacheCubeUV;

	// Block Encyclopedia Data
	[ReadOnly]
	public NativeArray<bool> blockTransparent;
	[ReadOnly]
	public NativeArray<bool> objectTransparent;
	[ReadOnly]
	public NativeArray<bool> blockLiquid;
	[ReadOnly]
	public NativeArray<bool> objectLiquid;
	[ReadOnly]
	public NativeArray<bool> blockLoad;
	[ReadOnly]
	public NativeArray<bool> objectLoad;
	[ReadOnly]
	public NativeArray<bool> blockInvisible;
	[ReadOnly]
	public NativeArray<bool> objectInvisible;
	[ReadOnly]
	public NativeArray<byte> blockMaterial;
	[ReadOnly]
	public NativeArray<byte> objectMaterial;
	[ReadOnly]
	public NativeArray<int3> blockTiles;


	// Builds the chunk mesh data excluding the X- and Z- chunk border
	public void Execute(){
		ushort thisBlock;
		ushort neighborBlock;

		for(int x=0; x<Chunk.chunkWidth; x++){
			for(int y=0; y<Chunk.chunkDepth; y++){
				for(int z=0; z<Chunk.chunkWidth; z++){
					thisBlock = data[x*Chunk.chunkWidth*Chunk.chunkDepth+y*Chunk.chunkWidth+z];

	    			// If air
	    			if(thisBlock == 0){
	    				continue;
	    			}

	    			// Runs OnLoad event
	    			if(load)
	    				// If is a block
		    			if(thisBlock <= ushort.MaxValue/2){
		    				if(blockLoad[thisBlock]){
		    					loadOutList.Add(new int3(x,y,z));
		    				}
		    			}
		    			// If Asset
		    			else{
		    				if(objectLoad[ushort.MaxValue-thisBlock]){
		    					loadOutList.Add(new int3(x,y,z));
		    				}
		    			}

	    			// --------------------------------

			    	for(int i=0; i<6; i++){
			    		neighborBlock = GetNeighbor(x, y, z, i);
			    		
			    		// Chunk Border and floor culling here! ----------
			    		
			    		if((x == 0 && 3 == i) || (z == 0 && 2 == i)){
			    			continue;
			    		}
			    		if((x == Chunk.chunkWidth-1 && 1 == i) || (z == Chunk.chunkWidth-1 && 0 == i)){
			    			continue;
			    		}
			    		if(y == 0 && 5 == i){
			    			continue;
			    		}

			    		////////// -----------------------------------

			    		
		    			// Handles Liquid chunks
		    			if(CheckLiquids(thisBlock, neighborBlock))
		    				continue;
		    			

		    			// Main Drawing Handling
			    		if(CheckPlacement(neighborBlock)){
					    	if(!LoadMesh(x, y, z, i, thisBlock, load, cacheCubeVert, cacheCubeUV)){
					    		break;
					    	}
			    		}
				    } // faces loop
	    		} // z loop
	    	} // y loop
	    } // x loop
    }

    // Gets neighbor element
	private ushort GetNeighbor(int x, int y, int z, int dir){
		int3 neighborCoord = new int3(x, y, z) + VoxelData.offsets[dir];
		
		if(neighborCoord.x < 0 || neighborCoord.x >= Chunk.chunkWidth || neighborCoord.z < 0 || neighborCoord.z >= Chunk.chunkWidth || neighborCoord.y < 0 || neighborCoord.y >= Chunk.chunkDepth){
			return 0;
		} 

		return data[neighborCoord.x*Chunk.chunkWidth*Chunk.chunkDepth+neighborCoord.y*Chunk.chunkWidth+neighborCoord.z];
	}

    // Checks if neighbor is transparent or invisible
    private bool CheckPlacement(int neighborBlock){
    	if(neighborBlock <= ushort.MaxValue/2)
    		return blockTransparent[neighborBlock] || blockInvisible[neighborBlock];
    	else
			return objectTransparent[ushort.MaxValue-neighborBlock] || objectInvisible[ushort.MaxValue-neighborBlock];
    }

    // Checks if Liquids are side by side
    private bool CheckLiquids(int thisBlock, int neighborBlock){
    	bool thisLiquid;
    	bool neighborLiquid;


    	if(thisBlock <= ushort.MaxValue/2)
    		thisLiquid = blockLiquid[thisBlock];
    	else
    		thisLiquid = objectLiquid[ushort.MaxValue-thisBlock];

    	if(neighborBlock <= ushort.MaxValue/2)
    		neighborLiquid = blockLiquid[neighborBlock];
    	else
    		neighborLiquid = objectLiquid[ushort.MaxValue-neighborBlock];

    	return thisLiquid && neighborLiquid;
    }


    // Imports Mesh data and applies it to the chunk depending on the Renderer Thread
    // Load is true when Chunk is being loaded and not reloaded
    // Returns true if loaded a blocktype mesh and false if it's an asset to be loaded later
    private bool LoadMesh(int x, int y, int z, int dir, ushort blockCode, bool load, NativeArray<Vector3> cacheCubeVert, NativeArray<Vector2> cacheCubeUV, int lookahead=0){
    	byte renderThread;

    	if(blockCode <= ushort.MaxValue/2)
    		renderThread = blockMaterial[blockCode];
    	else
    		renderThread = objectMaterial[ushort.MaxValue-blockCode];
    	
    	// If object is Normal Block
    	if(renderThread == 0){
    		faceVertices(cacheCubeVert, dir, 0.5f, new Vector3(x,y,z));
			verts.AddRange(cacheCubeVert);
			int vCount = verts.Length + lookahead;

			AddTexture(cacheCubeUV, dir, blockCode);
    		UVs.AddRange(cacheCubeUV);

    		
	    	normalTris.Add(vCount -4);
	    	normalTris.Add(vCount -4 +1);
	    	normalTris.Add(vCount -4 +2);
	    	normalTris.Add(vCount -4);
	    	normalTris.Add(vCount -4 +2);
	    	normalTris.Add(vCount -4 +3); 
	    	
	    	return true;
    	}

    	// If object is Specular Block
    	else if(renderThread == 1){
    		faceVertices(cacheCubeVert, dir, 0.5f, new Vector3(x,y,z));
			verts.AddRange(cacheCubeVert);
			int vCount = verts.Length + lookahead;

			AddTexture(cacheCubeUV, dir, blockCode);
    		UVs.AddRange(cacheCubeUV);

    		
	    	specularTris.Add(vCount -4);
	    	specularTris.Add(vCount -4 +1);
	    	specularTris.Add(vCount -4 +2);
	    	specularTris.Add(vCount -4);
	    	specularTris.Add(vCount -4 +2);
	    	specularTris.Add(vCount -4 +3);
	    	
	    	return true;   		
    	}

    	// If object is Liquid
    	else if(renderThread == 2){
    		faceVertices(cacheCubeVert, dir, 0.5f, new Vector3(x,y,z));
			verts.AddRange(cacheCubeVert);
			int vCount = verts.Length + lookahead;

			LiquidTexture(cacheCubeUV, x, z);
    		UVs.AddRange(cacheCubeUV);

    		
	    	liquidTris.Add(vCount -4);
	    	liquidTris.Add(vCount -4 +1);
	    	liquidTris.Add(vCount -4 +2);
	    	liquidTris.Add(vCount -4);
	    	liquidTris.Add(vCount -4 +2);
	    	liquidTris.Add(vCount -4 +3);

	    	return true;    		
    	}

    	// If object is an Asset
    	else{
			loadAssetList.Add(new int3(x,y,z));
    		return false;
    	}
    }

	// Sets UV mapping for a direction
	private void AddTexture(NativeArray<Vector2> array, int dir, ushort blockCode){
		int textureID;

		if(dir == 4)
			textureID = blockTiles[blockCode].x;
		else if(dir == 5)
			textureID = blockTiles[blockCode].y;
		else
			textureID = blockTiles[blockCode].z;

		// If should use normal atlas
		if(blockMaterial[blockCode] == 0){
			float x = textureID%Blocks.atlasSizeX;
			float y = Mathf.FloorToInt(textureID/Blocks.atlasSizeX);
	 
			x *= 1f / Blocks.atlasSizeX;
			y *= 1f / Blocks.atlasSizeY;

			array[0] = new Vector2(x,y+(1f/Blocks.atlasSizeY));
			array[1] = new Vector2(x+(1f/Blocks.atlasSizeX),y+(1f/Blocks.atlasSizeY));
			array[2] = new Vector2(x+(1f/Blocks.atlasSizeX),y);
			array[3] = new Vector2(x,y);
		}
		// If should use transparent atlas
		else if(blockMaterial[blockCode] == 1){
			float x = textureID%Blocks.transparentAtlasSizeX;
			float y = Mathf.FloorToInt(textureID/Blocks.transparentAtlasSizeY);
	 
			x *= 1f / Blocks.transparentAtlasSizeX;
			y *= 1f / Blocks.transparentAtlasSizeY;

			array[0] = new Vector2(x,y+(1f/Blocks.transparentAtlasSizeY));
			array[1] = new Vector2(x+(1f/Blocks.transparentAtlasSizeX),y+(1f/Blocks.transparentAtlasSizeY));
			array[2] = new Vector2(x+(1f/Blocks.transparentAtlasSizeX),y);
			array[3] = new Vector2(x,y);
		}
	}

	// Gets UV Map for Liquid blocks
	private void LiquidTexture(NativeArray<Vector2> array, int x, int z){
		int size = Chunk.chunkWidth;
		int tileSize = 1/size;

		array[0] = new Vector2(x*tileSize,z*tileSize);
		array[1] = new Vector2(x*tileSize,(z+1)*tileSize);
		array[2] = new Vector2((x+1)*tileSize,(z+1)*tileSize);
		array[3] = new Vector2((x+1)*tileSize,z*tileSize);
	}

	// Cube Mesh Data get verts
	public static void faceVertices(NativeArray<Vector3> fv, int dir, float scale, Vector3 pos)
	{
		for (int i = 0; i < fv.Length; i++)
		{
			fv[i] = (CubeMeshData.vertices[CubeMeshData.faceTriangles[dir*4+i]] * scale) + pos;
		}
	}
}


[BurstCompile]
public struct PrepareAssetsJob : IJob{
	// Output
	public NativeList<Vector3> meshVerts;
	public NativeList<Vector2> meshUVs;
	public NativeList<int> meshTris;

	[ReadOnly]
	public int vCount;

	// Input
	[ReadOnly]
	public NativeArray<ushort> blockdata;
	[ReadOnly]
	public NativeList<int3> coords;
	[ReadOnly]
	public NativeList<ushort> blockCodes;
	[ReadOnly]
	public NativeList<int> vertsOffset;
	[ReadOnly]
	public NativeList<int> trisOffset;
	[ReadOnly]
	public NativeList<int> UVOffset;
	[ReadOnly]
	public NativeArray<Vector3> scaling;

	// Loaded Mesh Data
	[ReadOnly]
	public NativeArray<Vector3> loadedVerts;
	[ReadOnly]
	public NativeArray<Vector2> loadedUV;
	[ReadOnly]
	public NativeArray<int> loadedTris;

	public void Execute(){
		int i;
		int currentVertAmount = vCount;

		for(int j=0; j < coords.Length; j++){
			i = GetIndex(blockdata[coords[j].x*Chunk.chunkWidth*Chunk.chunkDepth+coords[j].y*Chunk.chunkWidth+coords[j].z]);

			if(i == -1)
				continue;

			// Vertices
			Vector3 vertPos = new Vector3(coords[j].x, coords[j].y, coords[j].z);
			for(int vertIndex=vertsOffset[i]; vertIndex < vertsOffset[i+1]; vertIndex++){
				Vector3 resultVert = Vector3Mult(loadedVerts[vertIndex], scaling[i], vertPos);
				meshVerts.Add(resultVert);
			}

			// UVs
			for(int UVIndex=UVOffset[i]; UVIndex < UVOffset[i+1]; UVIndex++){
				meshUVs.Add(loadedUV[UVIndex]);
			}

			// Triangles
			for(int triIndex=trisOffset[i]; triIndex < trisOffset[i+1]; triIndex++){
				meshTris.Add(loadedTris[triIndex] + currentVertAmount);
			}	
			currentVertAmount += (vertsOffset[i+1] - vertsOffset[i]);		
		}
	}

	// Check if a blockCode is contained in blockCodes List
	private int GetIndex(ushort code){
		for(int i=0; i < blockCodes.Length; i++){
			if(blockCodes[i] == code){
				return i;
			}
		}
		return -1;
	}

	private Vector3 Vector3Mult(Vector3 a, Vector3 b, Vector3 plus){
		return new Vector3(a.x * b.x + plus.x, a.y * b.y + plus.y, a.z * b.z + plus.z);
	}

}