﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class VoxelData
{
	ushort[] data;

	public VoxelData(){
		this.data = new ushort[Chunk.chunkWidth*Chunk.chunkDepth*Chunk.chunkWidth];
	}

	public VoxelData(ushort[] data){
		this.data = (ushort[])data.Clone();
	}

	public static VoxelData CutUnderground(VoxelData a, NativeArray<ushort> b, int upper=-1, int lower=-1){
		if(upper == -1)
			upper = Chunk.chunkDepth;
		if(lower == -1)
			lower = 0;

		for(int x=0;x<Chunk.chunkWidth;x++){
			for(int y=lower;y<upper;y++){
				for(int z=0;z<Chunk.chunkWidth;z++){
					if(a.GetCell(x,y,z) >= 1 && b[(x*Chunk.chunkWidth*Chunk.chunkDepth)+y+(z*Chunk.chunkDepth)] == 1)
						a.SetCell(x,y,z,0);
				}
			} 
		}
		b.Dispose();
		return a;
	}

	public static VoxelData CutUnderground(VoxelData a, VoxelData b, int upper=-1, int lower=-1){
		if(upper == -1)
			upper = Chunk.chunkDepth;
		if(lower == -1)
			lower = 0;

		for(int x=0;x<Chunk.chunkWidth;x++){
			for(int y=lower;y<upper;y++){
				for(int z=0;z<Chunk.chunkWidth;z++){
					if(a.GetCell(x,y,z) >= 1 && b.GetCell(x,y,z) == 1)
						a.SetCell(x,y,z,0);
				}
			} 
		}
		return a;
	}

	public ushort GetCell(int x, int y, int z){
		return data[x*Chunk.chunkWidth*Chunk.chunkDepth+y*Chunk.chunkWidth+z];
	}

	public void SetCell(int x, int y, int z, ushort blockCode){
		data[x*Chunk.chunkWidth*Chunk.chunkDepth+y*Chunk.chunkWidth+z] = blockCode;
	}

	public ushort[] GetData(){
		return this.data;
	}

	public override string ToString(){
		string str = "";
		foreach(var item in data){
			str += item.ToString();
		}

		return base.ToString() + " -> " + str;
	}

	public ushort GetNeighbor(int x, int y, int z, Direction dir){
		DataCoordinate offsetToCheck = offsets[(int)dir];
		DataCoordinate neighborCoord = new DataCoordinate(x + offsetToCheck.x, y + offsetToCheck.y, z + offsetToCheck.z);
		
		if(neighborCoord.x < 0 || neighborCoord.x >= Chunk.chunkWidth || neighborCoord.z < 0 || neighborCoord.z >= Chunk.chunkWidth || neighborCoord.y < 0 || neighborCoord.y >= Chunk.chunkDepth){
			return 0;
		} 
		else{
			return GetCell(neighborCoord.x, neighborCoord.y, neighborCoord.z);
		}
		

	}

	struct DataCoordinate{
		public int x;
		public int y;
		public int z;

		public DataCoordinate(int x, int y, int z){
			this.x = x;
			this.y = y;
			this.z = z;
		}
	}

	DataCoordinate[] offsets = {
		new DataCoordinate(0,0,1),
		new DataCoordinate(1,0,0),
		new DataCoordinate(0,0,-1),
		new DataCoordinate(-1,0,0),
		new DataCoordinate(0,1,0),
		new DataCoordinate(0,-1,0)
	};
}

public enum Direction{
	North,
	East,
	South,
	West,
	Up,
	Down
}