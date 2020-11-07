﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestStruct : Structure
{
	ushort[] blocks = new ushort[24]{3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3};
	ushort?[] hps = new ushort?[24]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[24]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public TestStruct(){
		this.code = 0; 

		this.sizeX = 12;
		this.sizeY = 1;
		this.sizeZ = 2;

		this.offsetX = 0;
		this.offsetZ = 0;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.OverwriteAll;
		this.overwriteBlocks = new List<ushort>();

		Prepare(blocks, hps, states);
	}
}

public class TreeSmallA : Structure
{

	ushort[] blocks = new ushort[175]{0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,65534,65534,65534,0,65534,65534,65534,65534,65534,65534,65534,4,65534,65534,65534,65534,65534,65534,65534,0,65534,65534,65534,0,0,65534,65534,65534,0,65534,65534,65534,65534,65534,65534,65534,4,65534,65534,65534,65534,65534,65534,65534,0,65534,65534,65534,0,0,65534,65534,65534,0,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,65534,0,65534,65534,65534,0,0,0,0,0,0,0,65534,65534,65534,0,0,65534,65534,65534,0,0,65534,65534,65534,0,0,0,0,0,0};
	ushort?[] hps = new ushort?[175]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[175]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public TreeSmallA(){
		this.code = 1; 

		this.sizeX = 5;
		this.sizeY = 7;
		this.sizeZ = 5;

		this.offsetX = 2;
		this.offsetZ = 2;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.FreeSpace;
		this.overwriteBlocks = new List<ushort>();

		Prepare(blocks, hps, states);
	}
}

public class TreeMediumA : Structure
{

	ushort[] blocks = new ushort[567]{0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,4,4,4,0,0,0,0,0,0,4,4,4,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,0,0,0,0,0,0,0,4,4,0,0,0,0,0,0,0,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,4,4,0,0,0,0,0,0,0,4,4,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,65534,0,0,0,0,0,0,0,65534,4,4,0,0,0,0,0,0,65534,4,4,65534,0,0,0,0,0,0,65534,65534,65534,0,0,0,0,0,0,65534,65534,0,0,0,0,0,0,0,65534,65534,0,0,0,0,0,0,0,65534,65534,65534,65534,0,65534,65534,65534,65534,65534,65534,65534,0,0,0,0,65534,65534,4,4,65534,65534,0,65534,65534,65534,65534,4,4,65534,0,0,0,0,65534,65534,65534,4,65534,65534,0,0,0,65534,65534,65534,4,65534,0,0,0,0,0,0,0,65534,0,0,0,0,0,0,0,65534,4,65534,0,0,65534,65534,65534,65534,65534,4,65534,65534,0,65534,65534,4,4,4,4,65534,65534,0,65534,65534,65534,65534,4,4,65534,65534,0,0,65534,65534,65534,4,65534,65534,65534,0,0,65534,65534,65534,65534,65534,65534,65534,0,0,0,0,0,0,65534,0,0,0,0,0,0,0,0,65534,0,0,0,0,0,0,0,65534,65534,65534,0,0,0,65534,65534,65534,4,4,65534,0,0,0,65534,65534,65534,4,65534,65534,65534,0,0,65534,65534,65534,65534,65534,65534,0,0,0,0,0,0,65534,65534,0,65534,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,65534,65534,0,0,0,0,0,0,0,65534,65534,65534,65534,0,0,0,0,0,65534,4,65534,0,0,0,0,0,0,65534,65534,65534,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,65534,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0};
	ushort?[] hps = new ushort?[567]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[567]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public TreeMediumA(){
		this.code = 2; 

		this.sizeX = 7;
		this.sizeY = 9;
		this.sizeZ = 9;

		this.offsetX = 3;
		this.offsetZ = 5;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.FreeSpace;
		this.overwriteBlocks = new List<ushort>();

		Prepare(blocks, hps, states);
	}
}

public class DirtPileA : Structure
{
	ushort[] blocks = new ushort[126]{0,0,2,2,2,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,2,2,2,0,0,0,0,2,0,2,0,0,0,2,2,2,0,2,2,2,2,2,0,0,2,2,2,2,0,0,2,2,2,2,0,0,2,2,2,2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,2,2,0,0,0,2,2,2,0,0,0,2,2,2,2,0,0,2,2,2,0,0,0,2,2,0,0,0,0,0,0,0,0,0};
	ushort?[] hps = new ushort?[126]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[126]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public DirtPileA(){
		this.code = 3; 

		this.sizeX = 7;
		this.sizeY = 3;
		this.sizeZ = 6;

		this.offsetX = 0;
		this.offsetZ = 0;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.SpecificOverwrite;
		this.overwriteBlocks = new List<ushort>(){1,3};

		Prepare(blocks, hps, states);
	}
}

public class DirtPileB : Structure
{
	ushort[] blocks = new ushort[147] {0,0,0,2,2,0,0,0,2,2,2,2,2,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,0,0,0,2,2,2,0,0,0,0,0,2,0,0,0,0,0,0,2,2,2,0,2,2,2,2,2,2,0,2,2,2,2,2,2,0,2,2,2,2,2,2,0,0,2,2,2,2,2,0,0,0,0,2,2,0,0,0,0,0,0,0,0,0,0,0,0,2,2,0,0,0,2,2,2,2,2,0,2,2,2,2,2,2,0,2,2,2,2,2,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0};
	ushort?[] hps = new ushort?[147]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[147]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public DirtPileB(){
		this.code = 4; 

		this.sizeX = 7;
		this.sizeY = 3;
		this.sizeZ = 7;

		this.offsetX = 0;
		this.offsetZ = 0;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.SpecificOverwrite;
		this.overwriteBlocks = new List<ushort>(){1,3};

		Prepare(blocks, hps, states);
	}
}

public class BoulderNormalA : Structure
{
	ushort[] blocks = new ushort[112]{0,0,0,3,3,3,0,0,3,3,3,3,3,3,0,3,3,3,3,3,3,0,0,0,0,0,3,0,0,0,3,3,3,3,0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0,3,3,3,3,0,0,0,0,3,3,0,0,0,3,3,3,3,3,0,0,3,3,3,3,3,0,0,0,0,3,3,0,0,0,0,0,0,0,0,0,0,0,3,3,3,0,0,0,0,3,3,3,0,0,0,0,0,0,0,0,0};
	ushort?[] hps = new ushort?[112]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};
	ushort?[] states = new ushort?[112]{null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null,null};

	public BoulderNormalA(){
		this.code = 5; 

		this.sizeX = 4;
		this.sizeY = 4;
		this.sizeZ = 7;

		this.offsetX = 0;
		this.offsetZ = 0;

        this.blockdata = new ushort[sizeX, sizeY, sizeZ];
        this.hpdata = new ushort?[sizeX, sizeY, sizeZ];
        this.statedata = new ushort?[sizeX, sizeY, sizeZ];
        this.meta = new VoxelMetadata(sizeX, sizeY, sizeZ);

		this.considerAir = false;
		this.type = FillType.OverwriteAll;
		this.overwriteBlocks = new List<ushort>();

		Prepare(blocks, hps, states);
	}
}

/*
ADD TO THIS ENUM EVERY NEW STRUCTURE IMPLEMENTED
*/

public enum StructureCode{
	TestStruct,
	TreeSmallA,
	TreeMediumA,
	DirtPileA,
	DirtPileB,
	BoulderNormalA
}