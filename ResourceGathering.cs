using ThunderRoad;
using UnityEngine;

namespace ARPG
{
    public class ResourceGathering : LevelModule
    {
    }

    public class Tree : MonoBehaviour
    {
        public string[] ItemsUsedToGather; // Use Item ID
        public int MinWood;
        public int MaxWood;
    }

    public class Rock : MonoBehaviour
    {
        public string[] ItemsUsedToGather; // Use Item ID
        public int MinStone;
        public int MaxStone;
    }
}
