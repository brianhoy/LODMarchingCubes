using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SE;

public static class LookupTableCreator {
    public static string GenerateLookupTable() {
        GenerateOffsetLookupTable();
    }

    public static void GenerateUniqueEdgesLookupTable() {
        byte[][][] uniqueEdgesTable = new byte[64][][];

        for(int i = 0; i < 63; i++) {
            byte[][] gridCellOffsets = SE.Tables.MCLodTable[i];
        

            Dictionary<byte, byte> UniqueEdges = new Dictionary<byte, byte>();

            int uniques = 0;
            foreach(byte[] gridCellOffs in gridCellOffsets) {
                for(int j = 0; j < Tables.edgePairs.GetLength(0); i++) {
                    byte bA = gridCellOffs[Tables.edgePairs[j, 0]];
                    byte bB = gridCellOffs[Tables.edgePairs[j, 1]];

                    Vector3 A = ByteToVector3(bA);
                    Vector3 B = ByteToVector3(bB);

                    if(UniqueEdges.ContainsKey(bA) && UniqueEdges[bA] == bB) continue;
                    if(UniqueEdges.ContainsKey(bB) && UniqueEdges[bB] == bA) continue;
                    
                    UniqueEdges.Add(bA, bB);
                    uniques++;

                    int nMaxes = 0;
                    if(A.x == 1 || A.y == 1 || A.z == 1) nMaxes++;
                    if(B.x == 1 || B.y == 1 || B.z == 1) nMaxes++;
                    if(nMaxes == 2) {
                        
                    }
                }
            }

            uniqueEdgesTable[i] = new byte[uniques][];

            int n = 0;
            foreach(KeyValuePair<byte, byte> e in UniqueEdges) {
                byte[] pair = new byte[2];
                pair[0] = e.Key;
                pair[1] = e.Value;
                uniqueEdgesTable[i][n] = pair;
                n++;
            }
        }
    }

    private static Vector3 ByteToVector3(byte vec3) {
        Vector3 pos = Vector3.zero;
        byte b = 0;

        if((b & 1) == 1) pos.x -= 1;
        if((b & 2) == 2) pos.x += 1;
        if((b & 4) == 4) pos.y -= 1;
        if((b & 8) == 8) pos.y += 1;
        if((b & 16) == 16) pos.z -= 1;
        if((b & 32) == 32) pos.z += 1;

        return pos;
    }

    public static string GenerateOffsetLookupTable() {
        addLodOffsets();

        string table = "public static byte[][][] MCLodTable = new byte[][][] {\n";

        byte[][][] offsetTable = new byte[64][][];

        for(byte lod = 0; lod < 64; lod++) {
            Vector3[][] offsets = GetOffsetsForLod(lod);
            byte[][] bOffsets = ConvertToByteOffsets(offsets);
            offsetTable[lod] = bOffsets;

            table += "	new byte[][] { // lod " + lod + " (" + System.Convert.ToString(lod, 2) + ")\n";

            for(int i = 0; i < bOffsets.Length; i++) {
				table += "		new byte[] { ";
                for(int j = 0; j < bOffsets[0].Length; j++) {
					table += bOffsets[i][j];
					if(j != bOffsets[0].Length - 1) {
						table += ", ";
					}
                }
				table += " }";
				if(i != bOffsets.Length - 1) {
					table += ",";
				}
				table += "\n";

            }

			table += "	}";
            
			if(lod != 63) {
				table += ", ";
			}
			table += "\n";
        } 
		table += "};";

		Debug.Log("Lookup Table: \n" + table);

        SE.Tables.MCLodTable = offsetTable;

		return table;
    }

    public static Vector3[][] GetOffsetsForLod(byte lod) {
        int numLODFaces = 0;
        for(int i = 0; i < 6; i++) {
            if(((lod >> i) & 1) == 1) numLODFaces++;
        }

        if(numLODFaces == 0) {
            Vector3[][] offsets = LODOffsets[lod];

            return offsets;
        }
        else if(!LODRotations.ContainsKey(lod)) {
            return new Vector3[][] {};
        }
        else
        {
            int numGridCells = LODOffsets[numLODFaces].Length;

            Vector3[][] offsets = new Vector3[numGridCells][];
            //GridCell[] cells = new GridCell[numGridCells];
            Quaternion rotation = LODRotations[lod];

            for(int i = 0; i < numGridCells; i++) {
                string stroffsets = "Lod offsets for gridcell " + i + ": ";
				offsets[i] = new Vector3[8];
                for(int j = 0; j < 8; j++) {
                    Vector3 rotatedOffset = (rotation * LODOffsets[numLODFaces][i][j]);
                    stroffsets += rotatedOffset + ", ";
                    offsets[i][j] = rotatedOffset;
                }
                Debug.Log(stroffsets);
            }
            return offsets;
        }
    }

    public static byte[][] ConvertToByteOffsets(Vector3[][] offsets) {
        byte[][] bOffsets = new byte[offsets.Length][];


        for(int i = 0; i < offsets.Length; i++) {
            string byteOffsetStr = "ByteOffsets: ";

            bOffsets[i] = new byte[8];
            for(int j = 0; j < 8; j++) {
            
                byte bOff = 0;
                if(Mathf.Round(offsets[i][j].x) == -1) bOff |= 1;
                if(Mathf.Round(offsets[i][j].x) == 1) bOff |= 2;
                if(Mathf.Round(offsets[i][j].y) == -1) bOff |= 4;
                if(Mathf.Round(offsets[i][j].y) == 1) bOff |= 8;
                if(Mathf.Round(offsets[i][j].z) == -1) bOff |= 16;
                if(Mathf.Round(offsets[i][j].z) == 1) bOff |= 32;
                bOffsets[i][j] = bOff;
                byteOffsetStr += "[reg: " + offsets[i][j] + " , byte: " + bOff + "], ";
            }
            Debug.Log(byteOffsetStr);
        }

        return bOffsets;
    }

    public static void addLodOffsets() {
        Vector3[][] regCellOffsets = new Vector3[8][];
        for(int i = 0; i < 8; i++) {
            regCellOffsets[i] = new Vector3[8];
            for(int j = 0; j < 8; j++) {
                regCellOffsets[i][j] = SE.Tables.CellOffsets[i] + SE.Tables.CellOffsets[j] - Vector3.one;
            }
        }
        LODOffsets[0] = regCellOffsets;
    }

    // total of 9 gridcells - can be reduced
    // first dimension: number of sides with LOD transitions
    // second dimension: number of gridcells
    // third dimension: number of offsets
    public readonly static Vector3[][][] LODOffsets = new Vector3[][][] {
        new Vector3[][] {            
        },

        new Vector3[][] {
                new Vector3[] { // top left corner cell
                    new Vector3(0f,0f,0f), new Vector3(1f,1f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,0f), 
                    new Vector3(0f,0f,1f), new Vector3(1f,1f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,1f) 
                }, 
                new Vector3[] { // bottom left corner cell
                    new Vector3(0f,-1f,0f), new Vector3(1f,-1f,1f), new Vector3(1f,-1f,1f), new Vector3(0f,0f,0f), 
                    new Vector3(0f,-1f,1f), new Vector3(1f,-1f,1f), new Vector3(1f,-1f,1f), new Vector3(0f,0f,1f) 
                }, 
                new Vector3[] { // top right corner cell
                    new Vector3(0f,0f,-1f), new Vector3(1f,1f,-1f), new Vector3(1f,1f,-1f), new Vector3(0f,1f,-1f), 
                    new Vector3(0f,0f,0f), new Vector3(1f,1f,-1f), new Vector3(1f,1f,-1f), new Vector3(0f,1f,0f) 
                },
                new Vector3[] { // bottom right corner cell
                    new Vector3(0f,-1f,-1f), new Vector3(1f,-1f,-1f), new Vector3(1f,-1f,-1f), new Vector3(0f,0f,-1f), 
                    new Vector3(0f,-1f,0f), new Vector3(1f,-1f,-1f), new Vector3(1f,-1f,-1f), new Vector3(0f,0f,0f) 
                },
                new Vector3[] { // left edge cell
                    new Vector3(0f,0f,1f), new Vector3(1f,-1f,1f), new Vector3(0f,0f,0f), new Vector3(0f,0f,0f), 
                    new Vector3(0f,0f,1f), new Vector3(1f,-1f,1f), new Vector3(1f,1f,1f), new Vector3(0f,0f,1f) 
                },
                new Vector3[] { // right edge cell
                    new Vector3(0f,0f,0f), new Vector3(1f,-1f,-1f), new Vector3(0f,0f,-1f), new Vector3(0f,0f,-1f), 
                    new Vector3(0f,0f,0f), new Vector3(1f,-1f,-1f), new Vector3(1f,1f,-1f), new Vector3(0f,0f,0f) 
                },
                new Vector3[] { // bottom edge cell
                    new Vector3(0f,-1f,0f), new Vector3(1f,-1f,-1f), new Vector3(1f,-1f,-1f), new Vector3(0f,0f,0f), 
                    new Vector3(0f,-1f,0f), new Vector3(1f,-1f,1f), new Vector3(1f,-1f,1f), new Vector3(0f,0f,0f)	
                },
                new Vector3[] { // top edge cell
                    new Vector3(0f,0f,0f), new Vector3(1f,1f,-1f), new Vector3(1f,1f,-1f), new Vector3(0f,1f,0f), 
                    new Vector3(0f,0f,0f), new Vector3(1f,1f,1f), new Vector3(1f,1f,1f), new Vector3(0f,1f,0f)	
                },
                new Vector3[] { // middle cell
                    new Vector3(0f,0f,0f), new Vector3(1f,-1f,-1f), new Vector3(1f,1f,-1f), new Vector3(0f,0f,0f), 
                    new Vector3(0f,0f,0f), new Vector3(1f,-1f,1f), new Vector3(1f,1f,1f), new Vector3(0f,0f,0f)
                }
        },

        new Vector3[][] {
            new Vector3[] {
                new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, -1f), new Vector3(-1f, 1f, -1f),
                new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f)
            },
            new Vector3[] {
                new Vector3(-1f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(1f, 1f, -1f), new Vector3(-1f, 1f, -1f),
                new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(-1f, 0f, 0f)
            },
            new Vector3[] {
                new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(-1f, 0f, 0f),
                new Vector3(-1f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f)
            },
            new Vector3[] {
                new Vector3(0f, -1f, 0f), new Vector3(1f, -1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(0f, 0f, 0f),
                new Vector3(0f, -1f, 0f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f)
            },
            new Vector3[] {
                new Vector3(0f, -1f, -1f), new Vector3(1f, -1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(0f, 0f, -1f),
                new Vector3(0f, -1f, 0f), new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f)
            },
            new Vector3[] {
                new Vector3(0f, -1f, 0f), new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 0f),
                new Vector3(0f, -1f, 1f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 1f)
            }
        },

        new Vector3[][] { // 3 sides
            new Vector3[] {
                new Vector3(-1f, -1f, 0f), new Vector3(0f, -1f, 0f), new Vector3(0f, 0f, 0f), new Vector3(-1f, 0f, 0f),
                new Vector3(-1f, -1f, 1f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
            },
           new Vector3[] {
                new Vector3(-1f, 0f, -1f), new Vector3(0f, 0f, -1f), new Vector3(1f, 1f, -1f), new Vector3(-1f, 1f, -1f),
                new Vector3(-1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f),
            },
            new Vector3[] {
                new Vector3(0f, -1f, -1f), new Vector3(1f, -1f, -1f), new Vector3(1f, 1f, -1f), new Vector3(0f, 0f, -1f),
                new Vector3(0f, -1f, 0f), new Vector3(1f, -1f, 1f), new Vector3(1f, 1f, 1f), new Vector3(0f, 0f, 0f),
            }
        }
    };

    public readonly static Dictionary<byte, Quaternion> LODRotations =
        new Dictionary<byte, Quaternion>() { 
            // Single sided LOD
            // -x, +x, -y, y, -z, z
            {1, Quaternion.AngleAxis(180, Vector3.up)}, 
            {2, Quaternion.identity},
            {4, Quaternion.AngleAxis(-90, new Vector3(0, 0, 1))}, 
            {8, Quaternion.AngleAxis(90, new Vector3(0, 0, 1))},
            {16, Quaternion.AngleAxis(90, Vector3.up)},
            {32, Quaternion.AngleAxis(-90, Vector3.up)},
        
            // Double sided LOD
            // +x+y, -x+y, -x-y, +x-y
            {2 + 8, Quaternion.identity}, 
            {1 + 8, Quaternion.AngleAxis(180, Vector3.up)}, 
            {1 + 4, Quaternion.AngleAxis(180, new Vector3(0, 0, 1))}, 
            {2 + 4, Quaternion.AngleAxis(-90, new Vector3(0, 0, 1))},
            // +z+y, -z+y, -z-y, +z-y
            {32 + 8, Quaternion.AngleAxis(-90, Vector3.up)}, 
            {16 + 8, Quaternion.AngleAxis(90, Vector3.up)}, 
            {16 + 4, Quaternion.AngleAxis(90, Vector3.up) * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1))}, 
            {32 + 4, Quaternion.AngleAxis(-90, Vector3.up) * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1))},
            // +z+x, -z+x, -z-x, +z-x
            {32 + 2, Quaternion.AngleAxis(90, new Vector3(1, 0, 0))},
            {16 + 2, Quaternion.AngleAxis(90, new Vector3(1, 0, 0)) * Quaternion.AngleAxis(-90, new Vector3(0, 0, 1))},
            {16 + 1, Quaternion.AngleAxis(90, new Vector3(1, 0, 0)) * Quaternion.AngleAxis(-180, new Vector3(0, 0, 1))},
            {32 + 1, Quaternion.AngleAxis(90, new Vector3(1, 0, 0)) * Quaternion.AngleAxis(90, new Vector3(0, 0, 1))},

            // Triple Sided LOD
            // +x+y+z -x+y+z +x-y+z -x-y+z 
            {2 + 8 + 32, Quaternion.identity},
            {1 + 8 + 32, Quaternion.AngleAxis(-90, new Vector3(0, 1, 0))},
            {2 + 4 + 32, Quaternion.AngleAxis(90, new Vector3(1, 0, 0))},
            {1 + 4 + 32, Quaternion.AngleAxis(-90, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(90, new Vector3(1, 0, 0))},
            // +x+y-z -x+y-z +x-y-z -x-y-z
            {2 + 8 + 16, Quaternion.AngleAxis(90, new Vector3(0, 1, 0))},
            {1 + 8 + 16, Quaternion.AngleAxis(180, new Vector3(0, 1, 0))},
            {2 + 4 + 16, Quaternion.AngleAxis(90, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(90, new Vector3(1, 0, 0))},
            {1 + 4 + 16, Quaternion.AngleAxis(180, new Vector3(0, 1, 0)) * Quaternion.AngleAxis(90, new Vector3(1, 0, 0))},
    };

}