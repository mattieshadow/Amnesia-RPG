using Newtonsoft.Json;
using UnityEngine;
using ThunderRoad;
using System.IO;
namespace ARPG
{
    public struct Traits
    {
        string trait;
        float power;
        bool player;
        Traits(string trait, float power, bool player)
        {
            this.trait = trait;
            this.power = power;
            this.player = player;
        }
        void Assign()
        {
            switch (trait)
            {
                case "Power":
                    break;
                case "Vampirism":
                    break;
                case "Icy":
                    break;
                case "Inferno":
                    break;
            }
        }
    }
    public class SpiderPlayer : MonoBehaviour
    {
        void Awake()
        {
            
        }
        void Update()
        {

        }
    }
    public class VampirismBoth : MonoBehaviour
    {
        Creature vampire;
        Creature biteVictim;
        public static float blood;
        EffectInstance biteEffect;
        bool isPlayer;
        float damage, timer;
        void Awake()
        {
            Debug.Log("Vampirism awoken.");
            vampire = GetComponentInParent<Creature>();
            isPlayer = gameObject.name == "PlayerDefaultMale" || gameObject.name == "PlayerDefaultFemale";
            if (isPlayer)
            {
                try
                {
                    VampireHolder save = JsonConvert.DeserializeObject<VampireHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/vampiresave.json")));
                    blood = save.blood;
                }
                catch
                {
                    Debug.LogWarning("No save found for vampire, first time being a vampire?");
                }
            }
        }
        void Update()
        {
            if (isPlayer)
                if (Time.time - timer > RPGManager.statInfo.updateLevelTime)
                {
                    timer = Time.time;
                    VampireSave.blood = blood;
                    File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/vampiresave.json"), JsonConvert.SerializeObject(new VampireSave(), Formatting.Indented));
                }
            foreach (Item item in Item.list)
                if (Vector3.Distance(item.transform.position, vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.position + (vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.up * 0.1f)) < 0.25f)
                    foreach (Paintable paintable in item.paintables)
                        if (Vector3.Distance(paintable.transform.position, vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.position + (vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.up * 0.1f)) < 0.10f)
                        {
                            paintable.Clear();
                        }
            if (isPlayer && blood > 1 && vampire.currentHealth < vampire.maxHealth)
            {
                blood -= 0.01f * Time.deltaTime;
                vampire.currentHealth += 0.05f * Time.deltaTime;
            }
            blood -= 0.0025f * Time.deltaTime;
            foreach (Creature creature in Creature.list)
                if (Vector3.Distance(vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.position, creature.ragdoll.GetPart(RagdollPart.Type.Neck).transform.position) < 0.3f && creature != vampire && !biteVictim)
                {
                    biteVictim = creature;
                }
            if (biteVictim)
            {
                if (biteEffect is null)
                    biteEffect = Catalog.GetData<EffectData>("Bleeding").Spawn(biteVictim.ragdoll.GetPart(RagdollPart.Type.Neck).transform.position, Quaternion.identity, biteVictim.ragdoll.GetPart(RagdollPart.Type.Neck).transform);
                biteEffect.Play();
                if (isPlayer)
                    damage = TraitsManager.vampirismLvl * Time.deltaTime;
                else
                    damage = Random.Range(1 * Time.deltaTime, 3 * Time.deltaTime);
                if (biteVictim.currentHealth > damage)
                {
                    biteVictim.currentHealth -= damage;
                    vampire.currentHealth += damage;
                }
                else
                {
                    biteVictim.Kill();
                    if (isPlayer)
                        TraitsManager.vampirismExp += damage;
                }
                if (isPlayer)
                {
                    int divider;
                    if (biteVictim.isKilled)
                        divider = 100;
                    else
                        divider = 50;
                    TraitsManager.vampirismExp += damage / divider;
                    if (blood < 100)
                        blood += damage / divider;
                }
                if (Vector3.Distance(vampire.ragdoll.GetPart(RagdollPart.Type.Head).transform.position, biteVictim.ragdoll.GetPart(RagdollPart.Type.Neck).transform.position) > 0.3f)
                {
                    biteVictim = null;
                    biteEffect.Despawn();
                    biteEffect = null;
                }
            }
        }
    }
    public class IcyPlayer : MonoBehaviour
    {
        float damageStored, limit;
        bool isPlayer;
        void Awake()
        {
            isPlayer = gameObject.name == "PlayerDefaultMale" || gameObject.name == "PlayerDefaultFemale";
            if (!RPGManager.bindedIceEvent)
            {
                EventManager.onCreatureHit += EventManager_onCreatureHit;
            }
        }
        private void EventManager_onCreatureHit(Creature creature, ref CollisionStruct collisionStruct)
        {
            if (this)
            {
                if (damageStored < limit)
                    damageStored += collisionStruct.damageStruct.damage;
            }
            else
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
        }
        void Update()
        {
            limit = TraitsManager.icyLvl * RPGManager.statInfo.startingMaxExp;
        }
    }
    public class PowerPlayer : MonoBehaviour
    {
        float damageStored, limit;
        bool isPlayer;
        void Awake()
        {
            isPlayer = gameObject.name == "PlayerDefaultMale" || gameObject.name == "PlayerDefaultFemale";
            if (!RPGManager.bindedIceEvent)
            {
                EventManager.onCreatureHit += EventManager_onCreatureHit;
            }
        }
        private void EventManager_onCreatureHit(Creature creature, ref CollisionStruct collisionStruct)
        {
            if (this)
            {
                if (damageStored < limit && Vector3.Distance(collisionStruct.contactPoint, transform.position) < TraitsManager.powerLvl)
                    damageStored += collisionStruct.damageStruct.damage;
            }
            else
                EventManager.onCreatureHit -= EventManager_onCreatureHit;
        }
        void Update()
        {
            limit = TraitsManager.powerLvl * 4;
        }
    }
    public class VampireSave
    {
        [JsonProperty]
        public static float blood;
    }
    public class VampireHolder
    {
        public float blood;
    }
    public class PowerSave
    {
        [JsonProperty]
        public static float damageStored;
    }
    public class PowerHolder
    {
        public float damageStored;
    }
    public class TraitsManager
    {
        [JsonProperty]
        public static bool vampirism;
        [JsonProperty]
        public static float vampirismLvl;
        [JsonProperty]
        public static float vampirismExp;
        [JsonProperty]
        public static bool icy;
        [JsonProperty]
        public static float icyLvl;
        [JsonProperty]
        public static float icyExp;
        [JsonProperty]
        public static bool inferno;
        [JsonProperty]
        public static float infernoLvl;
        [JsonProperty]
        public static float infernoExp;
        [JsonProperty]
        public static bool power;
        [JsonProperty]
        public static float powerLvl;
        [JsonProperty]
        public static float powerexp;
    }
    public class TraitsHolder
    {      
        public bool vampirism;       
        public float vampirismLvl;       
        public float vampirismExp;       
        public bool icy;      
        public float icyLvl;       
        public float icyExp;
        public bool inferno;
        public float infernoLvl;
        public float infernoExp;
        public bool power;
        public float powerLvl;
        public float powerexp;
    }
}