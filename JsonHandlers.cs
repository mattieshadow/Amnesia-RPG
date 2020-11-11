namespace ARPG
{
    public class StatsInfo
    {
        public float startingMaxExp;
        public int updateLevelTime;
    }
    public class SurvivalInfo
    {

    }
    public class TerrainInfo
    {
        public float chunksToGenerate;
        public float cullingDistance;
        public int cullingDelay;
        public float cullingTimer;
    }
    public static class RPGManager
    {
        public static StatsInfo statInfo;
        public static TerrainInfo terrainInfo;
    }
}