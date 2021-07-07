using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using static System.Environment;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using Unity.Mathematics;

using Random = UnityEngine.Random;

public class Server
{
	public int maxPlayers = 8;
	public int port = 33000;
	private bool isLocal = false;
	public Socket masterSocket;

	public Dictionary<ulong, Socket> connections;
	public Dictionary<ulong, Socket> temporaryConnections;
	public Dictionary<ulong, bool> lengthPacket;
	public Dictionary<ulong, int> packetIndex;
	public Dictionary<ulong, int> packetSize;
	public Dictionary<ulong, byte[]> dataBuffer;
	public Dictionary<ulong, DateTime> timeoutTimers;

	public Dictionary<ulong, HashSet<ulong>> connectionGraph;
	private Dictionary<ChunkPos, HashSet<ulong>> playersInChunk;

	private IPEndPoint serverIP;
	private ulong currentCode = ulong.MaxValue-1;
	private const int receiveBufferSize = 4096*4096;
	private byte[] receiveBuffer = new byte[receiveBufferSize];
	public Dictionary<ulong, int> playerRenderDistances = new Dictionary<ulong, int>();
	private SocketError err = new SocketError();

	public List<NetMessage> queue = new List<NetMessage>();

	public ulong firstConnectedID = ulong.MaxValue;
	private int timeoutSeconds = 10;

	// Unity Reference
	public ChunkLoader_Server cl;

	public Server(ChunkLoader_Server cl){
    	ParseArguments();

    	// Parses config file if is a Dedicated Server
    	if(!this.isLocal){
    		ParseConfigFile();
    	}

		// Initiates Server data
    	connections = new Dictionary<ulong, Socket>();
    	temporaryConnections = new Dictionary<ulong, Socket>();
    	timeoutTimers = new Dictionary<ulong, DateTime>();
    	lengthPacket = new Dictionary<ulong, bool>();
    	packetIndex = new Dictionary<ulong, int>();
    	packetSize = new Dictionary<ulong, int>();
    	dataBuffer = new Dictionary<ulong, byte[]>();
    	connectionGraph = new Dictionary<ulong, HashSet<ulong>>();
    	playersInChunk = new Dictionary<ChunkPos, HashSet<ulong>>();

    	this.cl = cl;

    	if(!this.isLocal){
        	this.masterSocket = new Socket(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
    		this.serverIP = new IPEndPoint(0, this.port);
    	}
        else{
        	Debug.Log("Received Local");
        	this.masterSocket = new Socket(IPAddress.Loopback.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        	this.serverIP = new IPEndPoint(0x0100007F, this.port);
        }


        Debug.Log("Starting Server");

        this.masterSocket.Bind(this.serverIP);
        this.masterSocket.Listen(byte.MaxValue);

        this.masterSocket.BeginAccept(new AsyncCallback(ConnectCallback), this.masterSocket);
	}

	// Receives command line args and parses them
	private void ParseArguments(){
		string[] args = GetCommandLineArgs();

		foreach(string arg in args){
			switch(arg){
				case "-Local":
					this.isLocal = true;
					break;
				default:
					break;
			}
		}
	}

	// Deals with the reading/writing of config file on Dedicated Servers
	private void ParseConfigFile(){
		Stream file;
		byte[] allBytes;

		// If there is a config file
		if(File.Exists("server.cfg")){
			string text;
			string[] temp;
			Dictionary<string, string> argsDictionary = new Dictionary<string, string>();

			// Parses all arguments
			file = File.Open("server.cfg", FileMode.Open);
			allBytes = new byte[file.Length];
			file.Read(allBytes, 0, (int)file.Length);
			text = System.Text.Encoding.Default.GetString(allBytes);

			foreach(string argument in text.Split('\n')){
				if(argument == "")
					continue;

				temp = argument.Split('=');
				argsDictionary.Add(temp[0], temp[1]);
			}

			if(argsDictionary.ContainsKey("world_name")){
				// If it's not filled, generate a new name
				if(argsDictionary["world_name"] == ""){
					argsDictionary["world_name"] = GenerateRandomName();
					file.Seek(0, SeekOrigin.End);
					allBytes = System.Text.Encoding.ASCII.GetBytes(argsDictionary["world_name"]);
					file.Write(allBytes, 0, allBytes.Length);
				}

				World.SetWorldName(argsDictionary["world_name"]);
				World.SetWorldSeed(Random.Range(1,999999));
			}

			file.Close();
		}
		// If a config file needs tto be created
		else{
			file = File.Open("server.cfg", FileMode.Create);
			allBytes = System.Text.Encoding.ASCII.GetBytes("world_name=");
			file.Write(allBytes, 0, allBytes.Length);
			file.Close();
			Application.Quit();
		}
	}

	// Generates a random 8 letter code
	private string GenerateRandomName(){
		StringBuilder sb = new StringBuilder();

		for(int i=0; i<8; i++){
			sb.Append((char)Random.Range(65, 122));
		}

		return sb.ToString();
	}

    // Callback for connections received
    private void ConnectCallback(IAsyncResult result){
    	Socket client = this.masterSocket.EndAccept(result);
    	ulong temporaryCode = GetCurrentCode();

    	this.temporaryConnections[temporaryCode] = client;
    	this.lengthPacket[temporaryCode] = true;
    	this.packetIndex[temporaryCode] = 0;

    	Debug.Log(client.RemoteEndPoint.ToString() + " has connected with temporary ID " + currentCode);
    	NetMessage message = new NetMessage(NetCode.ACCEPTEDCONNECT);
    	this.Send(message.GetMessage(), message.size, temporaryCode, temporary:true);

    	this.temporaryConnections[temporaryCode].BeginReceive(receiveBuffer, 0, 4, 0, out this.err, new AsyncCallback(ReceiveCallback), temporaryCode);
    	this.masterSocket.BeginAccept(new AsyncCallback(ConnectCallback), null);
    }

	// Sends a byte[] to the a client given it's ID
	public void Send(byte[] data, int length, ulong id, bool temporary=false){
		try{
			if(!temporary){
				IAsyncResult lenResult = this.connections[id].BeginSend(this.LengthPacket(length), 0, 4, 0, out this.err, null, id);
				this.connections[id].EndSend(lenResult);

				IAsyncResult result = this.connections[id].BeginSend(data, 0, length, 0, out this.err, null, id);
				this.connections[id].EndSend(result);

				NetMessage.Broadcast(NetBroadcast.SENT, data[0], id, length);
			}
			else{
				IAsyncResult lenResult = this.temporaryConnections[id].BeginSend(this.LengthPacket(length), 0, 4, 0, out this.err, null, id);
				this.temporaryConnections[id].EndSend(lenResult);

				IAsyncResult result = this.temporaryConnections[id].BeginSend(data, 0, length, 0, out this.err, null, id);
				this.temporaryConnections[id].EndSend(result);

				NetMessage.Broadcast(NetBroadcast.SENT, data[0], id, length);				
			}
		}
		catch(Exception e){
			Debug.Log("SEND ERROR: " + e.ToString());
		}
	}

	// Sends a message to all IDs connected
	public void SendAll(byte[] data, int length){
		foreach(ulong code in this.connections.Keys)
			this.Send(data, length, code);
	}

	// Receive call handling
	private void ReceiveCallback(IAsyncResult result){
		try{
			ulong currentID = (ulong)result.AsyncState;
			bool isTemporary = false;
			int bytesReceived;

			// If receives something before attributing AccountID
			if(this.temporaryConnections.ContainsKey(currentID))
				isTemporary = true;

			// Gets packet size
			if(isTemporary)
				bytesReceived = this.temporaryConnections[currentID].EndReceive(result);
			else
				bytesReceived = this.connections[currentID].EndReceive(result);


			// If has received a size packet
			if(this.lengthPacket[currentID]){
				int size = NetDecoder.ReadInt(receiveBuffer, 0);
				this.dataBuffer[currentID] = new byte[size];
				this.packetSize[currentID] = size;
				this.lengthPacket[currentID] = false;
				this.packetIndex[currentID] = 0;

				if(isTemporary)
					this.temporaryConnections[currentID].BeginReceive(receiveBuffer, 0, size, 0, out this.err, new AsyncCallback(ReceiveCallback), currentID);
				else
					this.connections[currentID].BeginReceive(receiveBuffer, 0, size, 0, out this.err, new AsyncCallback(ReceiveCallback), currentID);
				return;
			}

			// If has received a segmented packet
			if(bytesReceived+this.packetIndex[currentID] < this.packetSize[currentID]){
				Array.Copy(receiveBuffer, 0, this.dataBuffer[currentID], this.packetIndex[currentID], bytesReceived);
				this.packetIndex[currentID] = this.packetIndex[currentID] + bytesReceived;
    			
    			if(isTemporary)
    				this.temporaryConnections[currentID].BeginReceive(receiveBuffer, 0, this.packetSize[currentID]-this.packetIndex[currentID], 0, out this.err, new AsyncCallback(ReceiveCallback), currentID);
    			else
    				this.connections[currentID].BeginReceive(receiveBuffer, 0, this.packetSize[currentID]-this.packetIndex[currentID], 0, out this.err, new AsyncCallback(ReceiveCallback), currentID);
				return;
			}

			// If has received the entire package
			Array.Copy(receiveBuffer, 0, this.dataBuffer[currentID], this.packetIndex[currentID], bytesReceived);

			NetMessage.Broadcast(NetBroadcast.RECEIVED, this.dataBuffer[currentID][0], currentID, this.packetSize[currentID]);

			NetMessage receivedMessage = new NetMessage(this.dataBuffer[currentID], currentID);
			this.queue.Add(receivedMessage);

			this.lengthPacket[currentID] = true;
			this.packetIndex[currentID] = 0;
			this.packetSize[currentID] = 0;

			if(!isTemporary)
    			this.connections[currentID].BeginReceive(receiveBuffer, 0, 4, 0, out this.err, new AsyncCallback(ReceiveCallback), currentID);
		}
		catch(SocketException e){
			Debug.Log(e.Message + "\n" + e.StackTrace);
		}
	}

	// Gets the first valid temporary ID
	public ulong GetCurrentCode(){
		ulong code = ulong.MaxValue-1;

		while(this.temporaryConnections.ContainsKey(code))
			code--;

		return code;
	}

	/* ===========================
	Handling of NetMessages
	*/

	// Discovers what to do with a Message received from Server
	public void HandleReceivedMessage(byte[] data, ulong id){
		try{
			NetMessage.Broadcast(NetBroadcast.PROCESSED, data[0], id, 0);
		}
		catch{
			return;
		}

		switch((NetCode)data[0]){
			case NetCode.SENDCLIENTINFO:
				SendClientInfo(data, id);
				break;
			case NetCode.REQUESTCHUNKLOAD:
				RequestChunkLoad(data, id);
				break;
			case NetCode.REQUESTCHUNKUNLOAD:
				RequestChunkUnload(data, id);
				break;
			case NetCode.SENDBUD:
				SendBUD(data);
				break;
			case NetCode.DIRECTBLOCKUPDATE:
				DirectBlockUpdate(data, id);
				break;
			case NetCode.CLIENTPLAYERPOSITION:
				ClientPlayerPosition(data, id);
				break;
			case NetCode.DISCONNECT:
				Disconnect(id);
				break;
			case NetCode.INTERACT:
				Interact(data);
				break;
			case NetCode.HEARTBEAT:
				Heartbeat(id);
				break;
			case NetCode.CLIENTCHUNK:
				ClientChunk(data, id);
				break;
			default:
				Debug.Log("UNKNOWN NETMESSAGE RECEIVED");
				break;
		}
	}	

	// Captures client info
	private void SendClientInfo(byte[] data, ulong id){
		NetMessage message = new NetMessage(NetCode.SENDSERVERINFO);
		ulong accountID = NetDecoder.ReadUlong(data, 1);
		int renderDistance = NetDecoder.ReadInt(data, 9); 
		int seed = NetDecoder.ReadInt(data, 13);
		int stringSize = NetDecoder.ReadInt(data, 17);
		string worldName = NetDecoder.ReadString(data, 21, stringSize);

		playerRenderDistances[accountID] = renderDistance;

		// If World Seed hasn't been set yet
		if(this.cl.worldSeed == -1)
			World.worldSeed = seed;
		
		if(this.isLocal)
			World.worldName = worldName;

		// Sends Player Info
		if(this.cl.RECEIVEDWORLDDATA){
			PlayerData pdat = this.cl.regionHandler.LoadPlayer(accountID);
			pdat.SetOnline(true);
			Vector3 playerPos = pdat.GetPosition();
			Vector3 playerDir = pdat.GetDirection();
			message.SendServerInfo(playerPos.x, playerPos.y, playerPos.z, playerDir.x, playerDir.y, playerDir.z);
			this.Send(message.GetMessage(), message.size, id, temporary:true);
		}

		// If AccountID is already online, erase all memory from that connection
		if(this.connections.ContainsKey(accountID)){
			Disconnect(accountID);
		}

		// Assigns a fixed ID
		this.connections.Add(accountID, this.temporaryConnections[id]);
		this.temporaryConnections.Remove(id);

    	this.lengthPacket[accountID] = true;
    	this.packetIndex[accountID] = 0;
    	this.connectionGraph.Add(accountID, new HashSet<ulong>());

		this.cl.RECEIVEDWORLDDATA = true;

		if(this.firstConnectedID == ulong.MaxValue)
			this.firstConnectedID = accountID;

		this.timeoutTimers.Add(accountID, DateTime.Now);

		Debug.Log("Temporary ID: " + id + " was assigned to ID: " + accountID);

    	this.connections[accountID].BeginReceive(receiveBuffer, 0, 4, 0, out this.err, new AsyncCallback(ReceiveCallback), accountID);
	}

	// Gets chunk information to player
	private void RequestChunkLoad(byte[] data, ulong id){
		ChunkPos pos = NetDecoder.ReadChunkPos(data, 1);

		// If is loaded
		if(this.cl.chunks.ContainsKey(pos)){
			if(!this.cl.loadedChunks.ContainsKey(pos))
				this.cl.loadedChunks.Add(pos, new HashSet<ulong>());

			if(!this.cl.loadedChunks[pos].Contains(id))
				this.cl.loadedChunks[pos].Add(id);

			NetMessage message = new NetMessage(NetCode.SENDCHUNK);
			message.SendChunk(this.cl.chunks[pos]);

			this.Send(message.GetMessage(), message.size, id);
		}
		else{
			// If it's not loaded yet
			if(!this.cl.toLoad.Contains(pos))
				this.cl.toLoad.Add(pos);

			// If was already issued a SendChunk call
			if(this.cl.loadedChunks.ContainsKey(pos)){
				if(!this.cl.loadedChunks[pos].Contains(id))
					this.cl.loadedChunks[pos].Add(id);
			}
			else{
				this.cl.loadedChunks.Add(pos, new HashSet<ulong>(){id});
			}
		}

		NetMessage playerMessage = new NetMessage(NetCode.ENTITYDATA);

		// Sends logged in players data
		if(this.playersInChunk.ContainsKey(pos)){
			foreach(ulong code in this.playersInChunk[pos]){
				if(code == id)
					continue;
				if(this.cl.regionHandler.allPlayerData[code].IsOnline()){
					this.connectionGraph[code].Add(id);
					Debug.Log("Added connection " + code + " <- " + id + " [338]");
					playerMessage.EntityData(this.cl.regionHandler.allPlayerData[code]);
					this.Send(playerMessage.GetMessage(), playerMessage.size, id);
				}
			}
		}
	}

	// Deletes the connection between a client and a chunk
	private void RequestChunkUnload(byte[] data, ulong id){
		ChunkPos pos = NetDecoder.ReadChunkPos(data, 1);
        NetMessage killMessage = new NetMessage(NetCode.ENTITYDELETE);

        if(this.playersInChunk.ContainsKey(pos)){
	        foreach(ulong code in this.playersInChunk[pos]){
	        	if(code == id)
	        		continue;

	        	this.connectionGraph[code].Remove(id);
				Debug.Log("Removed connection " + code + " <- " + id + " [357]");

	        	killMessage.EntityDelete(EntityType.PLAYER, code);
	        	this.Send(killMessage.GetMessage(), killMessage.size, id);
	        }
	    }

        this.cl.UnloadChunk(pos, id);
	}

	// Processes a simple BUD request
	private void SendBUD(byte[] data){
		BUDSignal bud;
		BUDCode code;
		int x, y, z, budX, budY, budZ, facing, offset;

		code = (BUDCode)NetDecoder.ReadInt(data, 1);
		x = NetDecoder.ReadInt(data, 5);
		y = NetDecoder.ReadInt(data, 9);
		z = NetDecoder.ReadInt(data, 13);
		budX = NetDecoder.ReadInt(data, 17);
		budY = NetDecoder.ReadInt(data, 21);
		budZ = NetDecoder.ReadInt(data, 25);
		facing = NetDecoder.ReadInt(data, 29);
		offset = NetDecoder.ReadInt(data, 33);

		bud = new BUDSignal(code, x, y, z, budX, budY, budZ, facing);

		this.cl.budscheduler.ScheduleBUD(bud, offset);
	}

	// Sends a direct action BUD to a block
	private void DirectBlockUpdate(byte[] data, ulong id){
		ChunkPos pos;
		int x, y, z, facing;
		ushort blockCode, state, hp;
		BUDCode type;
		NetMessage message;

		pos = NetDecoder.ReadChunkPos(data, 1);
		x = NetDecoder.ReadInt(data, 9);
		y = NetDecoder.ReadInt(data, 13);
		z = NetDecoder.ReadInt(data, 17);
		facing = NetDecoder.ReadInt(data, 21);

		blockCode = NetDecoder.ReadUshort(data, 25);
		state = NetDecoder.ReadUshort(data, 27);
		hp = NetDecoder.ReadUshort(data, 29);
		type = (BUDCode)NetDecoder.ReadInt(data, 31);

		CastCoord lastCoord = new CastCoord(pos, x, y, z);

		switch(type){
			case BUDCode.PLACE:
				// if chunk is still loaded
				if(this.cl.chunks.ContainsKey(lastCoord.GetChunkPos())){

					// if it's a block
					if(blockCode <= ushort.MaxValue/2){
						// if placement rules fail
						if(!cl.blockBook.blocks[blockCode].PlacementRule(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl)){
							return;
						}
					}
					// if it's an object
					else{
						if(!cl.blockBook.objects[ushort.MaxValue-blockCode].PlacementRule(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl)){
							return;
						}
					}

					// Check if is trying to put block on players
					if(this.playersInChunk.ContainsKey(pos)){
						foreach(ulong code in this.playersInChunk[pos]){
							if(code == id)
								continue;

							if(!this.cl.regionHandler.allPlayerData[code].CheckValidPlacement(x, y, z))
								return;
						}
					}

					// If doesn't have special place handling
					if(!cl.blockBook.CheckCustomPlace(blockCode)){
						// Actually places block/asset into terrain
						cl.chunks[lastCoord.GetChunkPos()].data.SetCell(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, blockCode);
						//cl.budscheduler.ScheduleReload(lastCoord.GetChunkPos(), 0);
						EmitBlockUpdate(BUDCode.CHANGE, lastCoord.GetWorldX(), lastCoord.GetWorldY(), lastCoord.GetWorldZ(), 0, cl);


						// Applies OnPlace Event
						if(blockCode <= ushort.MaxValue/2)
							cl.blockBook.blocks[blockCode].OnPlace(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl);
						else{
							cl.blockBook.objects[ushort.MaxValue-blockCode].OnPlace(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl);
						}

						// Sends the updated voxel to loaded clients
						message = new NetMessage(NetCode.DIRECTBLOCKUPDATE);
						message.DirectBlockUpdate(BUDCode.PLACE, lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, blockCode, this.cl.chunks[lastCoord.GetChunkPos()].metadata.GetState(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ), this.cl.chunks[lastCoord.GetChunkPos()].metadata.GetHP(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ));
						SendToClients(lastCoord.GetChunkPos(), message);
						
						this.cl.regionHandler.SaveChunk(this.cl.chunks[pos]);
					}

					// If has special handling
					else{
						// Actually places block/asset into terrain
						this.cl.chunks[lastCoord.GetChunkPos()].data.SetCell(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, blockCode);

						if(blockCode <= ushort.MaxValue/2){
							cl.blockBook.blocks[blockCode].OnPlace(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl);
						}
						else{
							cl.blockBook.objects[ushort.MaxValue-blockCode].OnPlace(lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, cl);
						}
				
					}

				}
				break;
			case BUDCode.SETSTATE:
				if(this.cl.chunks.ContainsKey(lastCoord.GetChunkPos()))
					this.cl.chunks[lastCoord.GetChunkPos()].metadata.SetState(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, blockCode);
				break;

			case BUDCode.BREAK:
				// If doesn't has special break handling
				if(!this.cl.blockBook.CheckCustomBreak(blockCode)){

					// Actually breaks new block and updates chunk
					this.cl.chunks[pos].data.SetCell(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, 0);
					this.cl.chunks[pos].metadata.Reset(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ);

					// Triggers OnBreak
					if(blockCode <= ushort.MaxValue/2)
						this.cl.blockBook.blocks[blockCode].OnBreak(pos, lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, this.cl);
					else
						this.cl.blockBook.objects[ushort.MaxValue - blockCode].OnBreak(pos, lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, this.cl);

					EmitBlockUpdate(BUDCode.BREAK, lastCoord.GetWorldX(), lastCoord.GetWorldY(), lastCoord.GetWorldZ(), 0, this.cl);
					
				}
				// If has special break handlings
				else{

					if(blockCode <= ushort.MaxValue/2){
						this.cl.blockBook.blocks[blockCode].OnBreak(pos, lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, this.cl);
					}
					else{
						this.cl.blockBook.objects[ushort.MaxValue - blockCode].OnBreak(pos, lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, this.cl);
					}
				}

				// Sends the updated voxel to loaded clients
				message = new NetMessage(NetCode.DIRECTBLOCKUPDATE);
				message.DirectBlockUpdate(BUDCode.BREAK, lastCoord.GetChunkPos(), lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ, facing, 0, ushort.MaxValue, ushort.MaxValue);
				SendToClients(lastCoord.GetChunkPos(), message);				
				this.cl.regionHandler.SaveChunk(this.cl.chunks[pos]);

				break;

			case BUDCode.LOAD:
				// HP is set as the Chunk Coordinates vs World Coordinates flag
				if(hp == ushort.MaxValue)
					lastCoord = new CastCoord(new Vector3(lastCoord.blockX, lastCoord.blockY, lastCoord.blockZ));
				
				blockCode = this.cl.GetBlock(lastCoord);

				if(this.cl.chunks.ContainsKey(lastCoord.GetChunkPos())){
					
					if(blockCode <= ushort.MaxValue/2){
						this.cl.blockBook.blocks[blockCode].OnLoad(lastCoord, this.cl);
					}
					else{
						this.cl.blockBook.objects[ushort.MaxValue - blockCode].OnLoad(lastCoord, this.cl);
					}
				}
				break;

			default:
				break;
		}
	}

	// Receives player position and adds it to PlayerPositions Dict
	private void ClientPlayerPosition(byte[] data, ulong id){
		float3 pos, dir;
		NetMessage graphMessage = new NetMessage(NetCode.ENTITYDATA);
	
		pos = NetDecoder.ReadFloat3(data, 1);
		dir = NetDecoder.ReadFloat3(data, 13);

		this.cl.regionHandler.allPlayerData[id].SetPosition(pos.x, pos.y, pos.z);
		this.cl.regionHandler.allPlayerData[id].SetDirection(dir.x, dir.y, dir.z);

		// Propagates data to all network
		foreach(ulong code in this.connectionGraph[id]){
			graphMessage.EntityData(this.cl.regionHandler.allPlayerData[id]);
			this.Send(graphMessage.GetMessage(), graphMessage.size, code);
		}
	}

	// Receives a disconnect call from client
	private void Disconnect(ulong id, bool voluntary=true){
		List<ChunkPos> toRemove = new List<ChunkPos>();
		NetMessage killMessage = new NetMessage(NetCode.ENTITYDELETE);
		killMessage.EntityDelete(EntityType.PLAYER, id);

		// Captures and removes all chunks
		foreach(KeyValuePair<ChunkPos, HashSet<ulong>> item in this.cl.loadedChunks){
			if(this.cl.loadedChunks[item.Key].Contains(id)){
				toRemove.Add(item.Key);
			}
		}

		foreach(ChunkPos pos in toRemove){
			this.cl.UnloadChunk(pos, id);
		}

		this.connections[id].Close();
		this.connections.Remove(id);
		this.timeoutTimers.Remove(id);
		this.lengthPacket.Remove(id);
		this.dataBuffer.Remove(id);
		this.packetIndex.Remove(id);
		this.packetSize.Remove(id);
		this.playerRenderDistances.Remove(id);
		this.connectionGraph.Remove(id);
		
		foreach(ulong code in this.cl.regionHandler.allPlayerData.Keys){
			if(code == id)
				continue;
			// If iterates through non-online user
			if(!this.cl.regionHandler.allPlayerData[code].IsOnline())
				continue;
			// If finds connection to it, erase
			if(this.connectionGraph[code].Contains(id)){
				Debug.Log("Removed connection " + code + " <- " + id + " [589]");
				this.connectionGraph[code].Remove(id);
				this.Send(killMessage.GetMessage(), killMessage.size, code);
			}
		}

		this.cl.regionHandler.SavePlayers();
		this.cl.regionHandler.allPlayerData[id].SetOnline(false);

		if(this.playersInChunk[this.cl.regionHandler.allPlayerData[id].GetChunkPos()].Count > 1)
			this.playersInChunk[this.cl.regionHandler.allPlayerData[id].GetChunkPos()].Remove(id);
		else
			this.playersInChunk.Remove(this.cl.regionHandler.allPlayerData[id].GetChunkPos());


		if(voluntary)
			Debug.Log("ID: " + id + " has disconnected");
		else
			Debug.Log("ID: " + id + " has lost connection");

		if(this.isLocal)
			Application.Quit();
	}

	// Receives an Interaction command from client
	private void Interact(byte[] data){
		ChunkPos pos = NetDecoder.ReadChunkPos(data, 1);
		int x = NetDecoder.ReadInt(data, 9);
		int y = NetDecoder.ReadInt(data, 13);
		int z = NetDecoder.ReadInt(data, 17);
		int facing = NetDecoder.ReadInt(data, 21);
		int callback;

		CastCoord current = new CastCoord(pos, x, y, z);

		ushort blockCode = this.cl.GetBlock(current);

		if(blockCode <= ushort.MaxValue/2)
			callback = this.cl.blockBook.blocks[blockCode].OnInteract(pos, current.blockX, current.blockY, current.blockZ, this.cl);
		else
			callback = this.cl.blockBook.objects[ushort.MaxValue - blockCode].OnInteract(pos, current.blockX, current.blockY, current.blockZ, this.cl);

		// Actual handling of message
		CallbackHandler(callback, pos, current, facing);		
	}

	// Receives a heartbeat from a client to reset it's timeoutTimers
	private void Heartbeat(ulong id){
		this.timeoutTimers[id] = DateTime.Now;
	}

	// Receives the client's current chunk and finds the connections it has with other players
	private void ClientChunk(byte[] data, ulong id){
		ChunkPos lastPos = NetDecoder.ReadChunkPos(data, 1);
		ChunkPos newPos = NetDecoder.ReadChunkPos(data, 9);

		// Removes last ChunkPos if exists
		if(lastPos != newPos){
			if(this.playersInChunk.ContainsKey(lastPos)){
				if(this.playersInChunk[lastPos].Count > 1)
					this.playersInChunk[lastPos].Remove(id);
				else
					this.playersInChunk.Remove(lastPos);
			}
		}

		// Add new ChunkPos
		if(!this.playersInChunk.ContainsKey(newPos))
			this.playersInChunk.Add(newPos, new HashSet<ulong>(){id});
		else
			this.playersInChunk[newPos].Add(id);

		// Finds the connections
		ChunkPos targetPos;
		NetMessage killMessage = new NetMessage(NetCode.ENTITYDELETE);

		foreach(ulong code in this.cl.regionHandler.allPlayerData.Keys){
			// If iterates through itself
			if(code == id)
				continue;
			// If iterates through non-online user
			if(!this.cl.regionHandler.allPlayerData[code].IsOnline())
				continue;

			// Check if code should still be connected
			if(this.connectionGraph[id].Contains(code)){
				targetPos = this.cl.regionHandler.allPlayerData[code].GetChunkPos();
				if(!this.cl.loadedChunks[newPos].Contains(code)){
					this.connectionGraph[id].Remove(code);
					Debug.Log("Removed connection " + id + " <- " + code + " [675]");
					killMessage.EntityDelete(EntityType.PLAYER, id);
					this.Send(killMessage.GetMessage(), killMessage.size, code);
				}
			}
			// Check if code should be connected
			else{
				if(this.cl.loadedChunks[newPos].Contains(code)){
					NetMessage liveMessage = new NetMessage(NetCode.ENTITYDATA);

					this.connectionGraph[id].Add(code);
					Debug.Log("Added connection " + code + " <- " + id + " [684]");
					liveMessage.EntityData(this.cl.regionHandler.allPlayerData[id]);
					this.Send(liveMessage.GetMessage(), liveMessage.size, code);					
				}				
			}
		}
	}


	// Auxiliary Functions

	// Send input message to all Clients connected to a given Chunk
	public void SendToClients(ChunkPos pos, NetMessage message){
		foreach(ulong i in this.cl.loadedChunks[pos]){
			this.Send(message.GetMessage(), message.size, i);
		}
	}

	// Send input message to all Clients connected to a given Chunk except the given one
	public void SendToClientsExcept(ChunkPos pos, NetMessage message, ulong exception){
		foreach(ulong i in this.cl.loadedChunks[pos]){
			if(i == exception)
				continue;
			this.Send(message.GetMessage(), message.size, i);
		}
	}

	/*
	Main Callback function for block interactions
	(REFER TO THESE CODES WHENEVER ADDING NEW BLOCK INTERACTIONS)
	(MAY BE NEEDED IN ORDER TO IMPLEMENT NEW POST HANDLERS FOR NEW BLOCKS)
	*/
	private void CallbackHandler(int code, ChunkPos targetChunk, CastCoord thisPos, int facing){
		// 0: No further actions necessary
		if(code == 0)
			return;
		// 1: Saves chunk and sends a DIRECTBLOCKUPDATE to all connected clients
		else if(code == 1){
			ushort blockCode = this.cl.GetBlock(thisPos);
			ushort state = this.cl.GetState(thisPos);
			ushort hp = this.cl.GetHP(thisPos);

			this.cl.regionHandler.SaveChunk(this.cl.chunks[targetChunk]);
			NetMessage message = new NetMessage(NetCode.DIRECTBLOCKUPDATE);
			message.DirectBlockUpdate(BUDCode.CHANGE, targetChunk, thisPos.blockX, thisPos.blockY, thisPos.blockZ, facing, blockCode, state, hp);
			SendToClients(targetChunk, message);
		}

	}

	// Handles the emittion of BUD to neighboring blocks
	public void EmitBlockUpdate(BUDCode type, int x, int y, int z, int tickOffset, ChunkLoader_Server cl){
		CastCoord thisPos = GetCoordinates(x, y, z);
		BUDSignal cachedBUD;

		CastCoord[] neighbors = {
		thisPos.Add(1,0,0),
		thisPos.Add(-1,0,0),
		thisPos.Add(0,1,0),
		thisPos.Add(0,-1,0),
		thisPos.Add(0,0,1),
		thisPos.Add(0,0,-1)
		};

		int[] facings = {2,0,4,5,1,3};

		int faceCounter=0;

		foreach(CastCoord c in neighbors){
			// Ignores void updates
			if(c.blockY < 0 || c.blockY > Chunk.chunkDepth-1){
				continue;
			}

			cachedBUD = new BUDSignal(type, c.GetWorldX(), c.GetWorldY(), c.GetWorldZ(), thisPos.GetWorldX(), thisPos.GetWorldY(), thisPos.GetWorldZ(), facings[faceCounter]);
			cl.budscheduler.ScheduleBUD(cachedBUD, tickOffset);			 
		
			faceCounter++;
		}
	}

	private CastCoord GetCoordinates(int x, int y, int z){
		return new CastCoord(new Vector3(x ,y ,z));
	}

	// Returns an int-sized byte[] with the length packet
	private byte[] LengthPacket(int a){
		byte[] output = new byte[4];

		output[0] = (byte)(a >> 24);
		output[1] = (byte)(a >> 16);
		output[2] = (byte)(a >> 8);
		output[3] = (byte)a;

		return output;
	}

	// Checks timeout in all sockets
	public void CheckTimeout(){
		List<ulong> toRemove = new List<ulong>();

		foreach(ulong id in this.timeoutTimers.Keys){
			if((DateTime.Now - this.timeoutTimers[id]).Seconds > this.timeoutSeconds){
				toRemove.Add(id);
			}
		}

		foreach(ulong id in toRemove)
			Disconnect(id, voluntary:false);
	}

}
