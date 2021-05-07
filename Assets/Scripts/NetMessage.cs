﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NetMessage
{
	public NetCode code;
	public int size;
	private static byte[] buffer = new byte[4096*4096]; // 2MB

	// Constructor
	public NetMessage(NetCode code){
		this.code = code;
		NetMessage.buffer[0] = (byte)code;
		this.size = 1;
	}

	// Gets the buffer
	public byte[] GetMessage(){
		return NetMessage.buffer;
	}

	// Gets size of buffer
	public int GetSize(){
		return this.size;
	}


	/*
	Individual Building of NetMessages
	*/

	// Client sending initial information to server after connection was accepted
	public void SendClientInfo(int playerRenderDistance, int seed, string worldName){
		// TODO: Add character name to here
		// {CODE}[Render] [Seed] [stringSize (int)] [worldName]
		NetDecoder.WriteInt(playerRenderDistance, NetMessage.buffer, 1);
		NetDecoder.WriteInt(seed, NetMessage.buffer, 5);
		NetDecoder.WriteInt(worldName.Length, NetMessage.buffer, 9);
		NetDecoder.WriteString(worldName, NetMessage.buffer, 13);
		this.size = 13 + worldName.Length;
	}

	// Server sending player character position
	public void SendServerInfo(int x, int y, int z){
		NetDecoder.WriteInt(x, NetMessage.buffer, 1);
		NetDecoder.WriteInt(y, NetMessage.buffer, 5);
		NetDecoder.WriteInt(z, NetMessage.buffer, 9);
		this.size = 13;
	}

	// Client asking for a chunk information to Server
	public void RequestChunkLoad(ChunkPos pos){
		NetDecoder.WriteChunkPos(pos, NetMessage.buffer, 1);
		this.size = 9;
	}

	// Client asking for Server to unload a chunk
	public void RequestChunkUnload(ChunkPos pos){
		NetDecoder.WriteChunkPos(pos, NetMessage.buffer, 1);
		this.size = 9;
	}

	// Server sending chunk information to Client
	public void SendChunk(Chunk c){
		// {CODE} [ChunkPos] [blockSize] [hpSize] [stateSize] | Respective data
		int blockDataSize = Compression.CompressBlocks(c, NetMessage.buffer, 21);
		int hpDataSize = Compression.CompressMetadataHP(c, NetMessage.buffer, 21+blockDataSize);
		int stateDataSize = Compression.CompressMetadataState(c, NetMessage.buffer, 21+blockDataSize+hpDataSize);
		
		NetDecoder.WriteChunkPos(c.pos, NetMessage.buffer, 1);
		NetDecoder.WriteInt(blockDataSize, NetMessage.buffer, 9);
		NetDecoder.WriteInt(hpDataSize, NetMessage.buffer, 13);
		NetDecoder.WriteInt(stateDataSize, NetMessage.buffer, 17);

		this.size = 21+blockDataSize+hpDataSize+stateDataSize;
	}

	// Sends a BUD packet to the server
	public void SendBUD(BUDSignal bud, int timeOffset){
		NetDecoder.WriteInt((int)bud.type, NetMessage.buffer, 1);
		NetDecoder.WriteInt(bud.x, NetMessage.buffer, 5);
		NetDecoder.WriteInt(bud.y, NetMessage.buffer, 9);
		NetDecoder.WriteInt(bud.z, NetMessage.buffer, 13);
		NetDecoder.WriteInt(bud.budX, NetMessage.buffer, 17);
		NetDecoder.WriteInt(bud.budY, NetMessage.buffer, 21);
		NetDecoder.WriteInt(bud.budZ, NetMessage.buffer, 25);
		NetDecoder.WriteInt(bud.facing, NetMessage.buffer, 29);
		NetDecoder.WriteInt(timeOffset, NetMessage.buffer, 33);
		this.size = 37;
	}

	// Client or Server a single voxel data to each other
	public void DirectBlockUpdate(BUDCode type, ChunkPos pos, int x, int y, int z, int facing, ushort blockCode, ushort state, ushort hp){
		NetDecoder.WriteChunkPos(pos, NetMessage.buffer, 1);
		NetDecoder.WriteInt(x, NetMessage.buffer, 9);
		NetDecoder.WriteInt(y, NetMessage.buffer, 13);
		NetDecoder.WriteInt(z, NetMessage.buffer, 17);
		NetDecoder.WriteInt(facing, NetMessage.buffer, 21);
		NetDecoder.WriteUshort(blockCode, NetMessage.buffer, 25);
		NetDecoder.WriteUshort(state, NetMessage.buffer, 27);
		NetDecoder.WriteUshort(hp, NetMessage.buffer, 29);
		NetDecoder.WriteInt((int)type, NetMessage.buffer, 31);
		this.size = 35;
	}

	// Clients sends their position to Server
	public void ClientPlayerPosition(float x, float y, float z, float rotX, float rotY, float rotZ){
		NetDecoder.WriteFloat(x, NetMessage.buffer, 1);
		NetDecoder.WriteFloat(y, NetMessage.buffer, 5);
		NetDecoder.WriteFloat(z, NetMessage.buffer, 9);
		NetDecoder.WriteFloat(rotX, NetMessage.buffer, 13);
		NetDecoder.WriteFloat(rotY, NetMessage.buffer, 17);
		NetDecoder.WriteFloat(rotZ, NetMessage.buffer, 21);
		this.size = 25;
	}

	// Client sends a voxel coordinate to trigger OnInteraction in server
	public void Interact(ChunkPos pos, int x, int y, int z, int facing){
		NetDecoder.WriteChunkPos(pos, NetMessage.buffer, 1);
		NetDecoder.WriteInt(x, NetMessage.buffer, 9);
		NetDecoder.WriteInt(y, NetMessage.buffer, 13);
		NetDecoder.WriteInt(z, NetMessage.buffer, 17);
		NetDecoder.WriteInt(facing, NetMessage.buffer, 21);
		this.size = 25;
	}

}

public enum NetCode{
	ACCEPTEDCONNECT, // No call
	SENDCLIENTINFO,
	SENDSERVERINFO,
	REQUESTCHUNKLOAD,
	REQUESTCHUNKUNLOAD,
	SENDCHUNK,
	SENDBUD,
	DIRECTBLOCKUPDATE,
	INTERACT,
	CLIENTPLAYERPOSITION,
	DISCONNECT  // No call
}