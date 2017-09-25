using UnityEngine;
using System.Collections.Generic;

namespace SE.Octree {

public class Root {
    public Node RootNode;
    public Dictionary<uint, Node> IDNodes; // index: ID | value: Node
    public Dictionary<Vector4, Node> Nodes; //index: Vector4 (xyz=position, w=Depth) | value: Node
}

} 