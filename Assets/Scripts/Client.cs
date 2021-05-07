﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.SceneManagement;

public class Client
{
	// Internet Objects
	private Socket socket;
	private SocketError err;

	// Data Management
	private const int sendBufferSize = 4096; // 4KB
	private const int receiveBufferSize = 4096*4096; // 2MB
	private byte[] receiveBuffer;

	// Address Information
	public IPAddress ip = new IPAddress(0x1800A8C0);
	public int port = 33000;

	// Unity References
	public ChunkLoader cl;

	
	public Client(ChunkLoader cl){
		this.socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
		receiveBuffer = new byte[receiveBufferSize];
		this.cl = cl;
		this.Connect();	
	}
	
	
	public void Connect(){
		this.socket.Connect(this.ip, this.port);
		this.socket.BeginReceive(receiveBuffer, 0, receiveBufferSize, 0, out this.err, new AsyncCallback(ReceiveCallback), null);		
	}

	// Receive call handling
	private void ReceiveCallback(IAsyncResult result){
		try{
			int bytesReceived = this.socket.EndReceive(result);

			if(bytesReceived <= 0){
				return;
			}

			byte[] data = new byte[bytesReceived];
			Array.Copy(receiveBuffer, data, bytesReceived);

			Debug.Log("Received: " + (NetCode)data[0] + " > " + bytesReceived);

			this.HandleReceivedMessage(data);

			this.socket.BeginReceive(receiveBuffer, 0, receiveBufferSize, 0, out this.err, new AsyncCallback(ReceiveCallback), null);
		}
		catch(Exception e){
			Debug.Log(e.ToString());
		}
	}

	// Sends a byte[] to the server
	public bool Send(byte[] data, int length){
		try{
			this.socket.BeginSend(data, 0, length, 0, out this.err, new AsyncCallback(SendCallback), this.socket);
			Debug.Log("Sent: " + (NetCode)data[0]);
			return true;
		}
		catch(Exception e){
			Debug.Log("SEND ERROR: " + e.ToString());
			return false;
		}
	}

	// Send callback to end package
	public void SendCallback(IAsyncResult result){
		this.socket.EndSend(result);
	}

	/* 
	=========================================================================
	Handling of NetMessages
	*/

	// Discovers what to do with a Message received from Server
	private void HandleReceivedMessage(byte[] data){
		switch((NetCode)data[0]){
			case NetCode.ACCEPTEDCONNECT:
				AcceptConnect();
				break;
			case NetCode.SENDSERVERINFO:
				SendServerInfo(data);
				break;
			case NetCode.SENDCHUNK:
				SendChunk(data);
				break;
			case NetCode.DISCONNECT:
				Disconnect();
				break;
			case NetCode.DIRECTBLOCKUPDATE:
				DirectBlockUpdate(data);
				break;
			default:
				Debug.Log("UNKNOWN NETMESSAGE RECEIVED: " + (NetCode)data[0]);
				break;
		}
	}

	// Permits the loading of Game Scene
	private void AcceptConnect(){
		this.cl.CONNECTEDTOSERVER = true;
		Debug.Log("Connected to Server at " + this.socket.RemoteEndPoint.ToString());
	}

	// Receives Player Information as int on startup
	private void SendServerInfo(byte[] data){
		int x, y, z;

		x = NetDecoder.ReadInt(data, 1);
		y = NetDecoder.ReadInt(data, 5);
		z = NetDecoder.ReadInt(data, 9);

		this.cl.PLAYERSPAWNED = true;
		this.cl.playerX = x;
		this.cl.playerY = y;
		this.cl.playerZ = z;
	}

	// Receives a Chunk
	private void SendChunk(byte[] data){
		ChunkPos cp = NetDecoder.ReadChunkPos(data, 1);
		int blockDataSize = NetDecoder.ReadInt(data, 9);
		int hpDataSize = NetDecoder.ReadInt(data, 13);
		int stateDataSize = NetDecoder.ReadInt(data, 17);

		this.cl.chunks[cp] = new Chunk(cp, this.cl.rend, this.cl.blockBook, this.cl);

		Compression.DecompressBlocksClient(this.cl.chunks[cp], data, initialPos:21);
		Compression.DecompressMetadataHPClient(this.cl.chunks[cp], data, initialPos:21+blockDataSize);
		Compression.DecompressMetadataStateClient(this.cl.chunks[cp], data, initialPos:21+blockDataSize+hpDataSize);
	
		if(!this.cl.toDraw.Contains(cp))
			this.cl.toDraw.Add(cp);
	}

	// Receives a disconnect call from server
	private void Disconnect(){
		this.socket.Close();
		SceneManager.LoadScene(0);
	}

	// Receives a Direct Block Update from server
	private void DirectBlockUpdate(byte[] data){
		ChunkPos pos;
		int x, y, z;
		ushort blockCode, state, hp;
		BUDCode type;

		pos = NetDecoder.ReadChunkPos(data, 1);
		x = NetDecoder.ReadInt(data, 9);
		y = NetDecoder.ReadInt(data, 13);
		z = NetDecoder.ReadInt(data, 17);
		int facing = NetDecoder.ReadInt(data, 21);

		CastCoord current = new CastCoord(new Vector3(x,y,z));

		blockCode = NetDecoder.ReadUshort(data, 25);
		state = NetDecoder.ReadUshort(data, 27);
		hp = NetDecoder.ReadUshort(data, 29);
		type = (BUDCode)NetDecoder.ReadInt(data, 31);

		switch(type){
			case BUDCode.PLACE:
				if(this.cl.chunks.ContainsKey(pos)){
					this.cl.chunks[pos].data.SetCell(x, y, z, blockCode);
					this.cl.chunks[pos].metadata.SetState(x, y, z, state);
					this.cl.chunks[pos].metadata.SetHP(x, y, z, hp);
					this.cl.AddToDraw(pos);
				}
				break;
			case BUDCode.BREAK:
				if(this.cl.chunks.ContainsKey(pos)){
					this.cl.chunks[pos].data.SetCell(x, y, z, 0);
					this.cl.chunks[pos].metadata.SetState(x, y, z, ushort.MaxValue);
					this.cl.chunks[pos].metadata.SetHP(x, y, z, ushort.MaxValue);
					this.cl.AddToDraw(pos);
				}	
				break;
			case BUDCode.CHANGE:
				if(this.cl.chunks.ContainsKey(pos)){
					this.cl.chunks[pos].data.SetCell(x, y, z, blockCode);
					this.cl.chunks[pos].metadata.SetState(x, y, z, state);
					this.cl.chunks[pos].metadata.SetHP(x, y, z, hp);
					this.cl.AddToDraw(pos);
				}
				break;
			default:
				break;
		}


	}

}
