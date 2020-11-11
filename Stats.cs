using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Collections;
namespace ARPG
{
    public class Entry : LevelModule
    {
        static StatHolder info2;
        static WeaponStatHolder info3;
        public override IEnumerator OnLoadCoroutine(Level level)
        {
            EventManager.onCreatureKill += EventManager_onCreatureKill;
            Debug.Log("Loaded ARPG");
            LoadFromSave();
            return base.OnLoadCoroutine(level);
        }
        private void EventManager_onCreatureKill(Creature creature, Player player, ref CollisionStruct collisionStruct, EventTime eventTime)
        {
            if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Neck) || (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Head)) &&
                collisionStruct.damageStruct.damageType == DamageType.Pierce || collisionStruct.damageStruct.damage > creature.maxHealth * 0.8f)
                return;
            int x = Mathf.RoundToInt(UnityEngine.Random.Range(0, 100));
            if (x <= 10 && !creature.gameObject.GetComponent<Faking>())
            {
                creature.gameObject.AddComponent<Faking>();
            }
        }
        public override void Update(Level level)
        {
            base.Update(level);
            if (Player.currentCreature && !Player.currentCreature.gameObject.GetComponent<StatsHandler>())
                Player.currentCreature.gameObject.AddComponent<StatsHandler>();
        }
        void LoadFromSave()
        {
            try
            {
                info2 = JsonConvert.DeserializeObject<StatHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/statsave.json")));
                StatManager.magicEfficiencyExp = info2.magicEfficiencyExp;
                StatManager.magicDamageExp = info2.magicDamageExp;
                StatManager.physicalSpeedExp = info2.physicalSpeedExp;
                StatManager.physicalJumpExp = info2.physicalJumpExp;
                StatManager.combatStrengthExp = info2.combatStrengthExp;
                StatManager.combatHealthExp = info2.combatHealthExp;
                StatManager.miscFocusExp = info2.miscFocusExp;
                StatManager.combatRangedExp = info2.combatRangedExp;
                StatManager.survivalExp = info2.survivalExp;
                StatManager.magicEfficiencyLvl = info2.magicEfficiencyLvl;
                StatManager.magicDamageLvl = info2.magicDamageLvl;
                StatManager.physicalSpeedLvl = info2.physicalSpeedLvl;
                StatManager.physicalJumpLvl = info2.physicalJumpLvl;
                StatManager.combatStrengthLvl = info2.combatStrengthLvl;
                StatManager.combatHealthLvl = info2.combatHealthLvl;
                StatManager.miscFocusLvl = info2.miscFocusLvl;
                StatManager.combatRangedLvl = info2.combatRangedLvl;
                StatManager.survivalLvl = info2.survivalLvl;
                info3 = JsonConvert.DeserializeObject<WeaponStatHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/weaponsave.json")));
                WeaponStatManager.daggerLvl = info3.daggerLvl;
                WeaponStatManager.daggerExp = info3.daggerExp;
                WeaponStatManager.swordLvl = info3.swordLvl;
                WeaponStatManager.swordExp = info3.swordsExp;
                WeaponStatManager.spearLvl = info3.spearLvl;
                WeaponStatManager.spearExp = info3.spearExp;
                WeaponStatManager.axeLvl = info3.axeLvl;
                WeaponStatManager.axeExp = info3.axeExp;
                WeaponStatManager.bluntLvl = info3.bluntLvl;
                WeaponStatManager.bluntExp = info3.bluntExp;
            }
            catch
            {
                Debug.Log("No save file found.");
            }
        }
    }
    public class StatsHandler : MonoBehaviour
    {
        StatsInfo info;
        static float prevMana, prevFocus, magicDamage, combatDamage;
        float timer1;
        public enum Stat
        {
            MagicEfficicency,
            MagicDamage,
            PhysicalSpeed,
            PhysicalJump,
            CombatDamage,
            CombatHealth,
            MiscFocus,
            CombatRanged,
            Survival
        }
        void Awake()
        {
            info = JsonConvert.DeserializeObject<StatsInfo>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Jsons/Stats.json")));
            prevMana = Player.currentCreature.mana.currentMana;
            prevFocus = Player.currentCreature.mana.currentFocus;
            EventManager.onCreatureHit += EventManager_onCreatureHitEvent;
            timer1 = Time.time;
            Player.currentCreature.mana.currentFocus = Player.currentCreature.mana.maxFocus;
            Player.currentCreature.mana.currentMana = Player.currentCreature.mana.maxMana;
            RPGManager.statInfo = info;
            UpdateLevels();
            ValidateLevels();
            SaveGame();
            PerkCheck();
            UpdateStats();
            Debug.LogWarning("Stats awoken!");
        }
        private void EventManager_onCreatureHitEvent(Creature creature, ref CollisionStruct collisionStruct)
        {
            if (Player.currentCreature && !creature.isKilled)
            {
                if (creature != Player.currentCreature)
                {
                    if (Vector3.Distance(Player.currentCreature.transform.position, collisionStruct.contactPoint) < 5)
                        StatManager.combatStrengthExp += collisionStruct.damageStruct.damage / 25;
                    if (collisionStruct.damageStruct.damageType == DamageType.Energy && collisionStruct.IsDoneByPlayer())
                        StatManager.magicDamageExp += collisionStruct.damageStruct.damage / 25;
                    if (collisionStruct.damageStruct.damageType == DamageType.Energy)
                        if (creature.currentHealth > magicDamage)
                            creature.currentHealth -= magicDamage;
                        else
                            creature.Kill();
                    foreach (Item item in Item.list)
                        if (Vector3.Distance(item.transform.position, collisionStruct.contactPoint) < 0.2f && item.lastHandler.creature == Player.currentCreature && item.handlers.Count < 1)
                        {
                            StatManager.combatRangedExp += collisionStruct.damageStruct.damage;
                            if (creature.currentHealth > ((StatManager.combatRangedLvl * 2) - 2))
                                creature.currentHealth -= (StatManager.combatRangedLvl * 2) - 2;
                            else
                                creature.Kill();
                        }
                    if (collisionStruct.sourceCollider.gameObject.GetComponentInParent<Item>() && collisionStruct.sourceCollider.gameObject.GetComponentInParent<Item>().data.categoryPath.Length > 0)
                        foreach (string category in collisionStruct.sourceCollider.gameObject.GetComponentInParent<Item>().data.categoryPath)
                        {
                            if (category.Contains("Daggers") || category.Contains("Dagger"))
                            {
                                WeaponStatManager.daggerExp += collisionStruct.damageStruct.damage * .1f;
                                if (creature.currentHealth > Mathf.Min(WeaponStatManager.daggerLvl, collisionStruct.damageStruct.damage / 2))
                                    creature.currentHealth -= Mathf.Min(WeaponStatManager.daggerLvl, collisionStruct.damageStruct.damage / 2);
                                else
                                    creature.Kill();
                            }
                            if (category.Contains("Spears") || category.Contains("Spear"))
                            {
                                WeaponStatManager.spearExp += collisionStruct.damageStruct.damage * .1f;
                                if (creature.currentHealth > Mathf.Min(WeaponStatManager.spearLvl, collisionStruct.damageStruct.damage / 2))
                                    creature.currentHealth -= Mathf.Min(WeaponStatManager.spearLvl, collisionStruct.damageStruct.damage / 2);
                                else
                                    creature.Kill();
                            }
                            if (category.Contains("Blunt") || category.Contains("Bludgeon"))
                            {
                                WeaponStatManager.bluntExp += collisionStruct.damageStruct.damage * .1f;
                                if (creature.currentHealth > Mathf.Min(WeaponStatManager.bluntLvl, collisionStruct.damageStruct.damage / 2))
                                    creature.currentHealth -= Mathf.Min(WeaponStatManager.bluntLvl, collisionStruct.damageStruct.damage / 2);
                                else
                                    creature.Kill();
                            }
                            if (category.Contains("Swords") || category.Contains("Blade") || category.Contains("Greatsword"))
                            {
                                WeaponStatManager.swordExp += collisionStruct.damageStruct.damage * .1f;
                                if (creature.currentHealth > Mathf.Min(WeaponStatManager.swordLvl, collisionStruct.damageStruct.damage / 2))
                                    creature.currentHealth -= Mathf.Min(WeaponStatManager.swordLvl, collisionStruct.damageStruct.damage / 2);
                                else
                                    creature.Kill();
                            }
                            if (category.Contains("Axes") || category.Contains("Axe"))
                            {
                                WeaponStatManager.axeExp += collisionStruct.damageStruct.damage * .1f;
                                if (creature.currentHealth > Mathf.Min(WeaponStatManager.axeLvl, collisionStruct.damageStruct.damage / 2))
                                    creature.currentHealth -= Mathf.Min(WeaponStatManager.axeLvl, collisionStruct.damageStruct.damage / 2);
                                else
                                    creature.Kill();
                            }
                        }
                }
                else
                {
                    StatManager.combatHealthExp += collisionStruct.damageStruct.damage;
                }
            }
        }
        void Update()
        {
            if (Player.currentCreature)
            {
                if (Player.currentCreature.mana.currentMana < prevMana)
                {
                    StatManager.magicEfficiencyExp += Time.deltaTime;
                    prevMana = Player.currentCreature.mana.currentMana;
                }
                if (Player.currentCreature.mana.currentFocus < prevFocus)
                {
                    StatManager.miscFocusExp += Time.deltaTime;
                    prevFocus = Player.currentCreature.mana.currentFocus;
                }
                if (Player.local.locomotion.velocity != Vector3.zero)
                    StatManager.physicalSpeedExp += 0.05f * Time.deltaTime;
                if (Player.local.locomotion.velocity.y != 0f)
                    StatManager.physicalJumpExp += Time.deltaTime;
                if (Time.time - timer1 > info.updateLevelTime)
                {
                    UpdateLevels();
                    ValidateLevels();
                    SaveGame();
                    PerkCheck();
                    UpdateStats();
                    timer1 = Time.time;
                }
            }
        }
        void SaveGame()
        {
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/statsave.json"), JsonConvert.SerializeObject(new StatManager(), Formatting.Indented));
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/weaponsave.json"), JsonConvert.SerializeObject(new WeaponStatManager(), Formatting.Indented));
        }
        public void UpdateStats()
        {
            if (WeaponStatManager.daggerExp > RPGManager.statInfo.startingMaxExp * WeaponStatManager.daggerLvl)
            {
                WeaponStatManager.daggerExp = 0;
                WeaponStatManager.daggerLvl += 1;
            }
            if (WeaponStatManager.swordExp > RPGManager.statInfo.startingMaxExp * WeaponStatManager.swordLvl)
            {
                WeaponStatManager.swordExp = 0;
                WeaponStatManager.swordLvl += 1;
            }
            if (WeaponStatManager.spearExp > RPGManager.statInfo.startingMaxExp * WeaponStatManager.spearLvl)
            {
                WeaponStatManager.spearExp = 0;
                WeaponStatManager.spearLvl += 1;
            }
            if (WeaponStatManager.axeExp > RPGManager.statInfo.startingMaxExp * WeaponStatManager.axeLvl)
            {
                WeaponStatManager.axeExp = 0;
                WeaponStatManager.axeLvl += 1;
            }
            if (WeaponStatManager.bluntExp > RPGManager.statInfo.startingMaxExp * WeaponStatManager.bluntLvl)
            {
                WeaponStatManager.bluntExp = 0;
                WeaponStatManager.bluntLvl += 1;
            }
        }
        void UpdateLevels()
        {
            if (StatManager.magicEfficiencyExp > (info.startingMaxExp * StatManager.magicEfficiencyLvl))
            {
                StatManager.magicEfficiencyLvl += 1;
                StatManager.magicEfficiencyExp = 0;
            }
            if (StatManager.magicDamageExp > (info.startingMaxExp * StatManager.magicDamageLvl))
            {
                StatManager.magicDamageLvl += 1;
                StatManager.magicDamageExp = 0;
            }
            if (StatManager.physicalSpeedExp > (info.startingMaxExp * StatManager.physicalSpeedLvl))
            {
                StatManager.physicalSpeedLvl += 1;
                StatManager.physicalSpeedExp = 0;
            }
            if (StatManager.physicalJumpExp > (info.startingMaxExp * StatManager.physicalJumpLvl))
            {
                StatManager.physicalJumpLvl += 1;
                StatManager.physicalJumpExp = 0;
            }
            if (StatManager.combatStrengthExp > (info.startingMaxExp * StatManager.combatStrengthLvl))
            {
                StatManager.combatStrengthLvl += 1;
                StatManager.combatStrengthExp = 0;
            }
            if (StatManager.combatHealthExp > (info.startingMaxExp * StatManager.combatHealthLvl))
            {
                StatManager.combatHealthLvl += 1;
                StatManager.combatHealthExp = 0;
            }
            if (StatManager.miscFocusExp > (info.startingMaxExp * StatManager.miscFocusLvl))
            {
                StatManager.miscFocusLvl += 1;
                StatManager.miscFocusExp = 0;
            }
            if (StatManager.combatRangedExp > (info.startingMaxExp * StatManager.combatRangedLvl))
            {
                StatManager.combatRangedLvl += 1;
                StatManager.combatRangedExp = 0;
            }
        }
        void PerkCheck()
        {
            /*if (StatManager.miscFocusLvl > 50 && !Player.currentCreature.gameObject.GetComponent<FocusReflexes>())
                Player.currentCreature.gameObject.AddComponent<FocusReflexes>();
            if (StatManager.miscFocusLvl > 20 && !Player.currentCreature.gameObject.GetComponent<FocusEnhance>())
                Player.currentCreature.gameObject.AddComponent<FocusEnhance>();*/
            if (StatManager.combatHealthLvl > 20 && !Player.currentCreature.gameObject.GetComponent<CombatRegeneration>())
                Player.currentCreature.gameObject.AddComponent<CombatRegeneration>();
        }
        void ValidateLevels()
        {
            Player.currentCreature.maxHealth = 16 + ((StatManager.combatHealthLvl * 1f) - 1f);
            Player.currentCreature.mana.maxMana = 50 + ((StatManager.magicEfficiencyLvl * 2) - 2);
            Player.currentCreature.mana.chargeSpeedMultiplier = 1 + ((StatManager.magicEfficiencyLvl * 0.1f) - 0.1f);
            Player.currentCreature.mana.maxFocus = 20 + ((StatManager.miscFocusLvl * 2) - 2);
            combatDamage = 1f + ((StatManager.combatStrengthLvl * 0.5f) - 0.5f);
            magicDamage = ((StatManager.magicDamageLvl * 2) - 2);
            float speedIncrease = Mathf.Min(1 + ((StatManager.physicalSpeedLvl * 0.08f) - 0.08f), 3.5f);
            Player.local.locomotion.runSpeedMultiplier = speedIncrease;
            Player.local.locomotion.speedMultiplier = Mathf.Min(speedIncrease * 0.8f, 1.5f);
            float jumpIncrease = Mathf.Min(0.5f + ((StatManager.physicalJumpLvl * 0.1f) - 0.1f), 2);
            Player.local.locomotion.jumpMaxDuration = jumpIncrease / 4f;
            Player.local.locomotion.jumpGroundForce = jumpIncrease / 3.5f;
            Player.local.locomotion.airSpeed = jumpIncrease;
            Player.local.locomotion.jumpClimbMultiplier = Mathf.Min(jumpIncrease / 2, 1);
            Player.currentCreature.mana.manaRegen = 4 + ((StatManager.magicEfficiencyLvl * 0.5f) - 0.5f);
            GameManager.options.handlePositionDamperMultiplier = Mathf.RoundToInt(40 + ((StatManager.combatStrengthLvl * 1f) - 1f));
            GameManager.options.handlePositionSpringMultiplier = Mathf.RoundToInt(40 + ((StatManager.combatStrengthLvl * 1f) - 1f));
            GameManager.options.handleRotationDamperMultiplier = Mathf.RoundToInt(40 + ((StatManager.combatStrengthLvl * 1f) - 1f));
            GameManager.options.handleRotationSpringMultiplier = Mathf.RoundToInt(40 + ((StatManager.combatStrengthLvl * 1f) - 1f));
            if (StatManager.combatStrengthLvl >= 20)
                GameManager.options.handleForceXYZ = true;
            else
                GameManager.options.handleForceXYZ = false;
            GameManager.options.Apply();
            CreatureData maleData = Catalog.GetData<CreatureData>("PlayerDefaultMale");
            CreatureData femaleData = Catalog.GetData<CreatureData>("PlayerDefaultFemale");
            maleData.forcePositionSpringDamper = new Vector2(3750 + (combatDamage * StatManager.combatStrengthLvl), 100);
            maleData.forceRotationSpringDamper = new Vector2(750 + (combatDamage * StatManager.combatStrengthLvl), 50);
            maleData.climbingForceMaxPosition = 1000 + (combatDamage * StatManager.combatStrengthLvl);
            maleData.climbingForceMaxRotation = 100 + (combatDamage * StatManager.combatStrengthLvl);
            maleData.gripForceMaxPosition = 750 + (combatDamage * StatManager.combatStrengthLvl);
            maleData.gripForceMaxRotation = 75 + (combatDamage * StatManager.combatStrengthLvl);
            maleData.health = Convert.ToInt16(16 + ((StatManager.combatHealthLvl * 1) - 1f));
            maleData.focus = 20 + ((StatManager.miscFocusLvl * 2) - 2);
            femaleData.forcePositionSpringDamper = new Vector2(3750 + (combatDamage * StatManager.combatStrengthLvl), 100);
            femaleData.forceRotationSpringDamper = new Vector2(750 + (combatDamage * StatManager.combatStrengthLvl), 50);
            femaleData.climbingForceMaxPosition = 1000 + (combatDamage * StatManager.combatStrengthLvl);
            femaleData.climbingForceMaxRotation = 100 + (combatDamage * StatManager.combatStrengthLvl);
            femaleData.gripForceMaxPosition = 750 + (combatDamage * StatManager.combatStrengthLvl);
            femaleData.gripForceMaxRotation = 75 + (combatDamage * StatManager.combatStrengthLvl);
            femaleData.health = Convert.ToInt16(16 + ((StatManager.combatHealthLvl * 1f) - 1f));
            femaleData.focus = 20 + ((StatManager.miscFocusLvl * 2) - 2);
            Catalog.GetData<SpellPowerSlowTime>("SlowTime").scale = Mathf.Clamp(50 - (StatManager.miscFocusLvl * 2), 0.05f, 1);
            SpellTelekinesis tk = Catalog.GetData<SpellTelekinesis>("Telekinesis");
            tk.maxAngle = 40 + ((StatManager.magicEfficiencyLvl * 1) - 1);
            tk.throwMultiplier = 2 + ((StatManager.magicEfficiencyLvl * .3f) - .3f);
            tk.manaConsumption = 5 - ((StatManager.magicEfficiencyLvl * .1f) - .1f);
            tk.maxVelocity = 5 + ((StatManager.magicEfficiencyLvl * 0.25f) - 0.25f);
            tk.pushDefaultForce = 20 + ((StatManager.magicEfficiencyLvl * 1) - 1);
            tk.pushRagdollForce = 30 + ((StatManager.magicEfficiencyLvl * 1) - 1);
            tk.maxCatchDistance = 6 + ((StatManager.magicEfficiencyLvl * .5f) - .5f);
            tk.pushRagdollOtherPartsForce = 8 + ((StatManager.magicEfficiencyLvl * 1f) - 1f);
            tk.pullAndRepelMaxSpeed = 8 + ((StatManager.magicEfficiencyLvl * .5f) - .5f);
            tk.repelMaxDistance = 12 + ((StatManager.magicEfficiencyLvl * .5f) - .5f);
            SpellCastGravity gravity = Catalog.GetData<SpellCastGravity>("Gravity");
            gravity.manaConsumption = 20 - ((StatManager.magicEfficiencyLvl * 1f) - 1f);
            gravity.halfSphereRadius = 2 + ((StatManager.magicEfficiencyLvl * .03f) - .03f);
            gravity.pushMaxForce = 8 + ((StatManager.magicEfficiencyLvl * 0.2f) - 0.2f);
            SpellMergeGravity gravityMerge = Catalog.GetData<SpellMergeGravity>("GravityMerge");
            gravityMerge.mergingLiftRadius = 5 + ((StatManager.magicEfficiencyLvl * .1f) - .1f);
            gravityMerge.bubbleDuration = 15 + ((StatManager.magicEfficiencyLvl * .4f) - .4f);
            if (Player.currentCreature.currentHealth > Player.currentCreature.maxHealth)
                Player.currentCreature.currentHealth = Player.currentCreature.maxHealth;
        }
    }
    public class WeaponStatManager
    {
        [JsonProperty]
        public static int daggerLvl;
        [JsonProperty]
        public static int swordLvl;
        [JsonProperty]
        public static int spearLvl;
        [JsonProperty]
        public static int axeLvl;
        [JsonProperty]
        public static int bluntLvl;
        [JsonProperty]
        public static float daggerExp;
        [JsonProperty]
        public static float swordExp;
        [JsonProperty]
        public static float spearExp;
        [JsonProperty]
        public static float axeExp;
        [JsonProperty]
        public static float bluntExp;
    }
    public class WeaponStatHolder
    {
        [JsonProperty]
        public int daggerLvl;
        [JsonProperty]
        public int swordLvl;
        [JsonProperty]
        public int spearLvl;
        [JsonProperty]
        public int axeLvl;
        [JsonProperty]
        public int bluntLvl;
        [JsonProperty]
        public float daggerExp;
        [JsonProperty]
        public float swordsExp;
        [JsonProperty]
        public float spearExp;
        [JsonProperty]
        public float axeExp;
        [JsonProperty]
        public float bluntExp;
    }
    public class StatManager
    {
        [JsonProperty]
        public static int magicEfficiencyLvl;
        [JsonProperty]
        public static int magicDamageLvl;
        [JsonProperty]
        public static int physicalSpeedLvl;
        [JsonProperty]
        public static int physicalJumpLvl;
        [JsonProperty]
        public static int combatStrengthLvl;
        [JsonProperty]
        public static int combatHealthLvl;
        [JsonProperty]
        public static int miscFocusLvl;
        [JsonProperty]
        public static int combatRangedLvl;
        [JsonProperty]
        public static int survivalLvl;
        [JsonProperty]
        public static float magicEfficiencyExp;
        [JsonProperty]
        public static float magicDamageExp;
        [JsonProperty]
        public static float physicalSpeedExp;
        [JsonProperty]
        public static float combatStrengthExp;
        [JsonProperty]
        public static float physicalJumpExp;
        [JsonProperty]
        public static float combatHealthExp;
        [JsonProperty]
        public static float miscFocusExp;
        [JsonProperty]
        public static float combatRangedExp;
        [JsonProperty]
        public static float survivalExp;
    }
    public class StatHolder
    {
        [JsonProperty]
        public int magicEfficiencyLvl { get; set; }
        [JsonProperty]
        public int magicDamageLvl { get; set; }
        [JsonProperty]
        public int physicalSpeedLvl { get; set; }
        [JsonProperty]
        public int physicalJumpLvl { get; set; }
        [JsonProperty]
        public int combatStrengthLvl { get; set; }
        [JsonProperty]
        public int combatHealthLvl { get; set; }
        [JsonProperty]
        public int miscFocusLvl { get; set; }
        [JsonProperty]
        public int combatRangedLvl { get; set; }
        [JsonProperty]
        public int survivalLvl { get; set; }
        [JsonProperty]
        public float magicEfficiencyExp { get; set; }
        [JsonProperty]
        public float magicDamageExp { get; set; }
        [JsonProperty]
        public float physicalSpeedExp { get; set; }
        [JsonProperty]
        public float combatStrengthExp { get; set; }
        [JsonProperty]
        public float physicalJumpExp { get; set; }
        [JsonProperty]
        public float combatHealthExp { get; set; }
        [JsonProperty]
        public float miscFocusExp { get; set; }
        [JsonProperty]
        public float combatRangedExp { get; set; }
        [JsonProperty]
        public float survivalExp { get; set; }
    }
    public class PHPEntry : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<PHPStatViewer>();
        }
    }
    public class PHPStatViewer : MonoBehaviour
    {
        Item php;
        float timer;
        Animator animator;
        void Awake()
        {
            php = GetComponentInParent<Item>();
            timer = Time.time;
            animator = GetComponentInChildren<Animator>();
            animator.SetBool("PHPRelease", true);
        }
        void Update()
        {
            if (Time.time - timer > 10)
            {
                timer = Time.time;
                php.GetCustomReference("Top").GetComponent<Text>().text = "Stats";
                php.GetCustomReference("Middle").GetComponent<Text>().text = "Magic efficiency: " + StatManager.magicEfficiencyLvl + ". Magic damage: " + StatManager.magicDamageLvl;
                php.GetCustomReference("Bottom").GetComponent<Text>().text = "Combat damage: " + StatManager.combatStrengthLvl + ". Combat health: " + StatManager.combatHealthLvl;
            }
        }
    }
}