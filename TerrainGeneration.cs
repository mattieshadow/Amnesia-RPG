using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using ThunderRoad;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using Unity.Jobs;
using Unity.Collections;

namespace ARPG
{
    public class ChunkGenLevelModule : LevelModule
    {
        bool done;
        AssetBundle bundle;
        TerrainInfo info;
        float timer;
        ChunkGeneration chunkGen;
        public override IEnumerator OnLoadCoroutine(Level level)
        {
            string raw = File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Terrain Generation/Jsons/Terrain.json"));
            info = Newtonsoft.Json.JsonConvert.DeserializeObject<TerrainInfo>(raw);
            Debug.Log("Level module loaded");
            return base.OnLoadCoroutine(level);
        }
        public override void Update(Level level)
        {
            base.Update(level);
            if (GameManager.GetCurrentLevel() == "arena")
            {
                if (!done)
                {
                    done = true;
                    foreach (AssetBundle bundles in AssetBundle.GetAllLoadedAssetBundles())
                        if (bundles.name == "chunks")
                            bundle = bundles;
                    if (!bundle)
                        bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "Terrain Generation/chunks.assets"));
                    GameObject prefab = GameObject.Instantiate(bundle.LoadAsset<GameObject>("StarterChunk"),
                        new Vector3(0, -20, 0), Quaternion.identity);
                    Debug.Log("Spawned starter chunk");
                    chunkGen = prefab.AddComponent<ChunkGeneration>().Setup(info.chunksToGenerate);
                    chunkGen.y = prefab.transform.position.y;
                    chunkGen.chunk1 = bundle.LoadAsset<GameObject>("chunk 1.prefab");
                    chunkGen.chunk2 = bundle.LoadAsset<GameObject>("chunk5.prefab");
                    chunkGen.chunk3 = bundle.LoadAsset<GameObject>("chunk8.prefab");
                    Debug.Log("Loaded the terrain");
                    foreach (string item in bundle.GetAllAssetNames())
                        Debug.Log(item);
                    Player.local.transform.position = prefab.transform.position;
                }
                if (Time.time - timer > info.cullingTimer)
                {
                    timer = Time.time;

                    NativeArray<Vector3> chunkPositions = new NativeArray<Vector3>(Manager.chunkDictionary.Count, Allocator.TempJob);

                    try
                    {
                        for (int i = 0; i < Manager.chunkDictionary.Count; i++)
                        {
                            chunkPositions[i] = Manager.chunkDictionary.Values.ElementAt(i);
                        }

                        CullingJob job = new CullingJob()
                        {
                            playerPos = Player.currentCreature.transform.position,
                            cullingDistance = info.cullingDistance,
                            chunkPositions = chunkPositions
                        };

                        JobHandle jobHandle = job.Schedule(Manager.chunkDictionary.Count, 10);
                        jobHandle.Complete();

                        chunkPositions.Dispose();

                    }
                    catch
                    {
                        Debug.LogError("Something went wrong with multithread please report this!");
                        chunkPositions.Dispose();
                    }
                    /*for (int i = 0; i < Manager.chunkDictionary.Count; i++)
                    {
                        if (i >= Manager.chunkDictionary.Count)
                            i = 0;
                        try
                        {
                            if (Vector3.Distance(Player.currentCreature.transform.position, Manager.chunkDictionary.Keys.ElementAt(i).transform.position) < info.cullingDistance)
                                Manager.chunkDictionary.Keys.ElementAt(i).SetActive(true);
                            if (Vector3.Distance(Player.currentCreature.transform.position, Manager.chunkDictionary.Keys.ElementAt(i).transform.position) > info.cullingDistance)
                                Manager.chunkDictionary.Keys.ElementAt(i).SetActive(false);
                            Task.Delay(info.cullingDelay);
                        }
                        catch
                        {
                            Debug.Log("Errored in culling");
                        }
                    }*/
                }
                if (GameManager.GetCurrentLevel() != "arena" && done)
                    done = false;
            }
        }

        public struct CullingJob : IJobParallelFor
        {
            [ReadOnly]
            public NativeArray<Vector3> chunkPositions;

            [ReadOnly]
            public Vector3 playerPos;

            [ReadOnly]
            public float cullingDistance;


            public void Execute(int index)
            {
                try
                {
                    float dist = Vector3.Distance(playerPos, chunkPositions[index]);

                    if (dist < cullingDistance)
                        Manager.chunkDictionary.Keys.ElementAt(index).SetActive(true);
                    if (dist > cullingDistance)
                        Manager.chunkDictionary.Keys.ElementAt(index).SetActive(false);
                }
                catch
                {
                    Debug.Log("Errored in culling");
                }
            }
        }
    }
    public class ChunkGeneration : MonoBehaviour
    {
        public Transform starterChunk;
        float counter, medium, medium2, zmedium, z2medium;
        int total;
        public GameObject[] chunks, chunkArray;
        public GameObject chunk1, chunk2, chunk3;
        public float chunkNumber, y;
        public bool moved;
        public GameObject[] chunkList;
        public ChunkGeneration Setup(float chunkNum)
        {
            chunkNumber = chunkNum;
            starterChunk = transform;
            return this;
        }
        void Update()
        {
            if (counter < chunkNumber)
            {
                SpawnChunkX(medium += 10, 1);
                SpawnChunkZ(zmedium += 10, 1);
                SpawnChunkRows(1, 1);
                Debug.Log("Spawned chunk set " + counter);
                counter += 1;
            }
            if (counter == chunkNumber && !moved)
            {
                Vector3 starterOG = starterChunk.transform.position;
                starterChunk.transform.position = new Vector3(chunkNumber / 2 * 10, y + 0.02f, chunkNumber / 2 * 10);
                int endChunk = UnityEngine.Random.Range(1, 4);
                if (endChunk == 1)
                {
                    Chunk chunk = new Chunk(starterOG, Instantiate(chunk1, starterOG, Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                if (endChunk == 2)
                {

                    Chunk chunk = new Chunk(starterOG, Instantiate(chunk2, starterOG, Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                if (endChunk == 3)
                {
                    Chunk chunk = new Chunk(starterOG, Instantiate(chunk3, starterOG, Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                moved = true;
                total += 1;
            }
        }
        void SpawnChunkX(float x, float xModifier)
        {
            int endChunk = UnityEngine.Random.Range(1, 4);
            if (endChunk == 1)
            {
                Chunk chunk = new Chunk(new Vector3(x * xModifier, y, 0), Instantiate(chunk1, new Vector3(starterChunk.position.x +
                    x * xModifier, starterChunk.position.y, 0), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }

            if (endChunk == 2)
            {
                Chunk chunk = new Chunk(new Vector3(x * xModifier, y, 0), Instantiate(chunk2, new Vector3(starterChunk.position.x +
                    x * xModifier, starterChunk.position.y, 0), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }

            if (endChunk == 3)
            {
                Chunk chunk = new Chunk(new Vector3(x * xModifier, y, 0), Instantiate(chunk3, new Vector3(starterChunk.position.x +
                    x * xModifier, starterChunk.position.y, 0), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }
            total += 1;
        }
        void SpawnChunkRows(float xModifier, float zModifier)
        {
            z2medium += 10;
            for (int i = 0; i < chunkNumber; i++)
            {
                medium2 += 10;
                int endChunk = UnityEngine.Random.Range(1, 4);
                if (endChunk == 1)
                {
                    Chunk chunk = new Chunk(new Vector3(medium2 * xModifier, y, z2medium * zModifier), Instantiate(chunk1,
                        new Vector3(medium2 * xModifier, y, z2medium * zModifier), Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                if (endChunk == 2)
                {
                    Chunk chunk = new Chunk(new Vector3(medium2 * xModifier, y, z2medium * zModifier), Instantiate(chunk2,
                        new Vector3(medium2 * xModifier, y, z2medium * zModifier), Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                if (endChunk == 3)
                {
                    Chunk chunk = new Chunk(new Vector3(medium2 * xModifier, y, z2medium * zModifier), Instantiate(chunk3,
                        new Vector3(medium2 * xModifier, y, z2medium * zModifier), Quaternion.identity));
                    Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
                }
                total += 1;
            }
            medium2 = 0;
        }
        void SpawnChunkZ(float z, float zModifier)
        {
            int endChunk = UnityEngine.Random.Range(1, 4);
            if (endChunk == 1)
            {
                Chunk chunk = new Chunk(new Vector3(0, y, z * zModifier), Instantiate(chunk1, new Vector3(0, y, starterChunk.position.z +
                    z * zModifier), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }
            if (endChunk == 2)
            {
                Chunk chunk = new Chunk(new Vector3(0, y, z * zModifier), Instantiate(chunk2, new Vector3(0, y, starterChunk.position.z +
                    z * zModifier), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }
            if (endChunk == 3)
            {
                Chunk chunk = new Chunk(new Vector3(0, y, z * zModifier), Instantiate(chunk3, new Vector3(0, y, starterChunk.position.z +
                    z * zModifier), Quaternion.identity));
                Manager.chunkDictionary.Add(chunk.chunk, chunk.position);
            }
            total += 1;
        }
    }
    public class Chunk
    {
        public Vector3 position;
        public GameObject chunk;
        public Chunk(Vector3 position, GameObject chunk)
        {
            this.chunk = chunk;
            this.position = position;
        }
    }

    public static class Manager
    {
        public static Dictionary<GameObject, Vector3> chunkDictionary = new Dictionary<GameObject, Vector3>();
    }
}
