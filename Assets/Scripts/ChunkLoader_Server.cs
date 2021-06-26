﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;

public class ChunkLoader_Server : MonoBehaviour
{
	// Basic ChunkLoader Data
	public int unloadAssetsCount = 25;
	public Dictionary<ChunkPos, Chunk> chunks = new Dictionary<ChunkPos, Chunk>();
	public ChunkPos currentChunk;
	public ChunkPos newChunk;
	public BlockEncyclopedia blockBook;
    public BUDScheduler budscheduler;
    public VFXLoader vfx;
    public TimeOfDay time;
    public WorldGenerator worldGen;
    public StructureHandler structHandler;
    public Server server;

    // Entity Handlers
    //public Dictionary<ulong, PlayerData> playerData = new Dictionary<ulong, PlayerData>();

    // Queues
    public List<ChunkPos> toLoad = new List<ChunkPos>();
    public List<ChunkPos> toUnload = new List<ChunkPos>();
    public Dictionary<ChunkPos, List<ulong>> loadedChunks = new Dictionary<ChunkPos, List<ulong>>();

	// World Generation
	public int worldSeed = -1; // 6 number integer
    public BiomeHandler biomeHandler;

	// Chunk Rendering
    public RegionFileHandler regionHandler;

	// Flags
    public int reloadMemoryCounter = 30;
    public bool RECEIVEDWORLDDATA = false;
    public bool INITIALIZEDWORLD = false;

    // Cache Data
    private ChunkPos cachePos = new ChunkPos(0,0);
    private Chunk cacheChunk;


    void OnApplicationQuit(){
        regionHandler.CloseAll();
    }

    void Start(){
        this.server = new Server(this, true);
        this.time.SetServer(this.server);
    }

    void Update(){ 
        if(this.RECEIVEDWORLDDATA && this.INITIALIZEDWORLD){
            // Decides what to do for current tick
            HandleServerCommunication();

            if(toLoad.Count > 0)
                LoadChunk();
            else if(Structure.reloadChunks.Count > 0)
                SavePregenChunk();
        }
        else if(!this.INITIALIZEDWORLD && this.RECEIVEDWORLDDATA){
            InitWorld();
        }
        else{
            HandleServerCommunication();
        }
    }

    private void InitWorld(){
        print("Initializing World");

        this.regionHandler = new RegionFileHandler(this);
        worldSeed = regionHandler.GetRealSeed();
        biomeHandler = new BiomeHandler(BiomeSeedFunction(worldSeed));
        this.worldGen = new WorldGenerator(worldSeed, BiomeSeedFunction(worldSeed), OffsetHashFunction(worldSeed), GenerationSeedFunction(worldSeed), biomeHandler, structHandler, this);
    
        // Sends the first player it's information
        PlayerData pdat = this.regionHandler.LoadPlayer(this.server.firstConnectedID); // CHANGE TO OTHER ACCOUNTID
        Vector3 playerPos = pdat.GetPosition();
        Vector3 playerDir = pdat.GetDirection();

        this.regionHandler.InitDataFiles(new ChunkPos((int)(playerPos.x/Chunk.chunkWidth), (int)(playerPos.z/Chunk.chunkWidth)));

        HandleServerCommunication();
        NetMessage message = new NetMessage(NetCode.SENDSERVERINFO);
        message.SendServerInfo(playerPos.x, playerPos.y, playerPos.z, playerDir.x, playerDir.y, playerDir.z);
        this.server.Send(message.GetMessage(), message.size, this.server.firstConnectedID); 
        this.INITIALIZEDWORLD = true;
        this.time.SetLock(false);
    }

    // Deals with the handling of Server received information
    private void HandleServerCommunication(){
        // If NetMessage queue is not empty
        int queueCount = this.server.queue.Count;

        if(queueCount > 0){
            for(ulong i=0; i<(ulong)queueCount; i++){
                this.server.HandleReceivedMessage(this.server.queue[0].GetData(), this.server.queue[0].GetID());
                this.server.queue.RemoveAt(0);
            }
        }
    }


    // Builds Structure data in non-indexed Chunks
    private void SavePregenChunk(){
        cacheChunk = Structure.reloadChunks[0];
        regionHandler.GetCorrectRegion(cacheChunk.pos);

        // If it's loaded
        if(chunks.ContainsKey(cacheChunk.pos)){
            cacheChunk.needsGeneration = 0;

            // Rough Application of Structures
            Structure.RoughApply(chunks[cacheChunk.pos], cacheChunk);
            chunks[cacheChunk.pos] = cacheChunk;

            this.regionHandler.SaveChunk(cacheChunk);

            SendChunkToRequestingClients(cacheChunk.pos);
        }

        // If is in an unloaded indexed chunk
        else if(this.regionHandler.IsIndexed(cacheChunk.pos)){
            Chunk c = new Chunk(cacheChunk.pos, server:true);
            this.regionHandler.LoadChunk(c);

            // Rough Application of Structures
            Structure.RoughApply(c, cacheChunk);

            this.regionHandler.SaveChunk(c);
        }

        // If is in an unloaded unknown chunk
        else{
            this.regionHandler.SaveChunk(cacheChunk);
        }


        Structure.reloadChunks.RemoveAt(0);
    }

    // Loads Chunk data, but doesn't draw them
    private void LoadChunk(){
        for(int i = 0; i < 3; i++){
        	if(toLoad.Count > 0){
        		// Prevention
        		if(toUnload.Contains(toLoad[0])){
        			toUnload.Remove(toLoad[0]);
        			toLoad.RemoveAt(0);
        			return;
        		}

                if(chunks.ContainsKey(toLoad[0])){
                    toLoad.RemoveAt(0);
                    return;
                }

                bool isPregen;

                // Gets correct region file
                regionHandler.GetCorrectRegion(toLoad[0]);

                // If current chunk toLoad was already generated
                if(regionHandler.IsIndexed(toLoad[0])){

                    isPregen = regionHandler.GetsNeedGeneration(toLoad[0]);

                    // If chunk is Pre-Generated
                    if(isPregen){
                        chunks.Add(toLoad[0], new Chunk(toLoad[0], server:true));
                        vfx.NewChunk(toLoad[0], isServer:true);
                        regionHandler.LoadChunk(chunks[toLoad[0]]);
                        this.worldGen.SetVoxdata(chunks[toLoad[0]].data.GetData());
                        this.worldGen.SetCacheHP(chunks[toLoad[0]].metadata.GetHPData());
                        this.worldGen.SetCacheState(chunks[toLoad[0]].metadata.GetStateData());
                        chunks[toLoad[0]].BuildOnVoxelData(this.worldGen.AssignBiome(toLoad[0], pregen:true));
                        chunks[toLoad[0]].metadata = new VoxelMetadata(this.worldGen.GetCacheHP(), this.worldGen.GetCacheState());
                        chunks[toLoad[0]].needsGeneration = 0;
                        regionHandler.SaveChunk(chunks[toLoad[0]]);
                    }
                    // If it's just a normally generated chunk
                    else{
                        chunks.Add(toLoad[0], new Chunk(toLoad[0], server:true));
                        vfx.NewChunk(toLoad[0], isServer:true);
                        regionHandler.LoadChunk(chunks[toLoad[0]]);
                        chunks[toLoad[0]].needsGeneration = 0;
                    }
                }
                // If it's a new chunk to be generated
                else{
                    chunks.Add(toLoad[0], new Chunk(toLoad[0], server:true));
                    vfx.NewChunk(toLoad[0], isServer:true);
                    chunks[toLoad[0]].BuildOnVoxelData(this.worldGen.AssignBiome(toLoad[0]));
                    chunks[toLoad[0]].metadata = new VoxelMetadata(this.worldGen.GetCacheHP(), this.worldGen.GetCacheState());
                    chunks[toLoad[0]].needsGeneration = 0;
                    regionHandler.SaveChunk(chunks[toLoad[0]]);
                }


                SendChunkToRequestingClients(toLoad[0]);
        		toLoad.RemoveAt(0);
        	}
        }
    }

    // Severs the connection of a Client to a chunk
    public void UnloadChunk(ChunkPos pos, ulong id){
        this.loadedChunks[pos].Remove(id);

        // If connected clients to this chunk are none
        if(this.loadedChunks[pos].Count == 0){
            this.loadedChunks.Remove(pos);
            this.chunks.Remove(pos);
            this.vfx.RemoveChunk(pos, isServer:true);
        }
    }

    // Sends chunk information to all requesting clients
    private void SendChunkToRequestingClients(ChunkPos pos){
        NetMessage message;

        // If there was no request for this chunk yet
        if(!this.loadedChunks.ContainsKey(pos))
            return;

        foreach(ulong id in this.loadedChunks[pos]){
            message = new NetMessage(NetCode.SENDCHUNK);
            message.SendChunk(this.chunks[pos]);
            this.server.Send(message.GetMessage(), message.size, id);
        }
    }

    // Calculates the biomeSeed of BiomeHandler
    private float BiomeSeedFunction(int t){
        return 0.04f*(0.03f*Mathf.Sin(t));
    }

    // Calculates general offset hash
    private float OffsetHashFunction(int t){
        return (t*0.71928590287457694671f)%1;
    }

    // Calculates the generationSeed used in World Generation
    private float GenerationSeedFunction(int t){
        return Perlin.Noise(t/1000000f)+0.5f;
    }

    // Returns block code of a castcoord
    public ushort GetBlock(CastCoord c){
        if(this.chunks.ContainsKey(c.GetChunkPos())){
            return this.chunks[c.GetChunkPos()].data.GetCell(c.blockX, c.blockY, c.blockZ);
        }
        else{
            return (ushort)(ushort.MaxValue/2); // Error Code
        }
    }

    // Returns block code of a castcoord
    public ushort GetState(CastCoord c){
        if(this.chunks.ContainsKey(c.GetChunkPos())){
            return this.chunks[c.GetChunkPos()].metadata.GetState(c.blockX, c.blockY, c.blockZ);
        }
        else{
            return (ushort)(ushort.MaxValue/2); // Error Code
        }
    }

    // Returns block code of a castcoord
    public ushort GetHP(CastCoord c){
        if(this.chunks.ContainsKey(c.GetChunkPos())){
            return this.chunks[c.GetChunkPos()].metadata.GetHP(c.blockX, c.blockY, c.blockZ);
        }
        else{
            return (ushort)(ushort.MaxValue/2); // Error Code
        }
    }

    // Returns the heightmap value of a generated chunk in block position
    public int GetBlockHeight(ChunkPos pos, int blockX, int blockZ){
        // Checks if chunk doesn't exist
        if(!chunks.ContainsKey(pos)){
            this.toLoad.Add(pos);
            this.LoadChunk();
            return GetBlockHeight(pos, blockX, blockZ);
        }
        for(int i=Chunk.chunkDepth-1; i >= 0 ; i--){
            if(chunks[pos].data.GetCell(Mathf.Abs(blockX), i, Mathf.Abs(blockZ)) != 0){
                return i+2;
            }
        }

        if(blockX < 15)
            return GetBlockHeight(pos, blockX+1, blockZ);
        if(blockZ < 15)
            return GetBlockHeight(pos, blockX, blockZ+1);

        return GetBlockHeight(pos, 0, 0);
    }
}
