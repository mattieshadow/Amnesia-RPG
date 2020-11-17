using Newtonsoft.Json;
using ThunderRoad;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace ARPG
{
    public class Entry : LevelModule
    {
        static StatHolder stats;
        static WeaponStatHolder weaponstats;
        static TraitsHolder traits;
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
            if (Player.currentCreature && !Player.currentCreature.gameObject.GetComponent<Main>())
                Player.currentCreature.gameObject.AddComponent<Main>();
        }
        void LoadFromSave()
        {
            try
            {
                stats = JsonConvert.DeserializeObject<StatHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/statsave.json")));
                StatManager.magicEfficiencyExp = stats.magicEfficiencyExp;
                StatManager.magicDamageExp = stats.magicDamageExp;
                StatManager.physicalSpeedExp = stats.physicalSpeedExp;
                StatManager.physicalJumpExp = stats.physicalJumpExp;
                StatManager.combatStrengthExp = stats.combatStrengthExp;
                StatManager.combatHealthExp = stats.combatHealthExp;
                StatManager.miscFocusExp = stats.miscFocusExp;
                StatManager.combatRangedExp = stats.combatRangedExp;
                StatManager.survivalExp = stats.survivalExp;
                StatManager.magicEfficiencyLvl = stats.magicEfficiencyLvl;
                StatManager.magicDamageLvl = stats.magicDamageLvl;
                StatManager.physicalSpeedLvl = stats.physicalSpeedLvl;
                StatManager.physicalJumpLvl = stats.physicalJumpLvl;
                StatManager.combatStrengthLvl = stats.combatStrengthLvl;
                StatManager.combatHealthLvl = stats.combatHealthLvl;
                StatManager.miscFocusLvl = stats.miscFocusLvl;
                StatManager.combatRangedLvl = stats.combatRangedLvl;
                StatManager.survivalLvl = stats.survivalLvl;
            }
            catch
            {
                Debug.Log("No save file found for stats.");
            }
            try
            {
                weaponstats = JsonConvert.DeserializeObject<WeaponStatHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/weaponsave.json")));
                WeaponStatManager.daggerLvl = weaponstats.daggerLvl;
                WeaponStatManager.daggerExp = weaponstats.daggerExp;
                WeaponStatManager.swordLvl = weaponstats.swordLvl;
                WeaponStatManager.swordExp = weaponstats.swordsExp;
                WeaponStatManager.spearLvl = weaponstats.spearLvl;
                WeaponStatManager.spearExp = weaponstats.spearExp;
                WeaponStatManager.axeLvl = weaponstats.axeLvl;
                WeaponStatManager.axeExp = weaponstats.axeExp;
                WeaponStatManager.bluntLvl = weaponstats.bluntLvl;
                WeaponStatManager.bluntExp = weaponstats.bluntExp;
            }
            catch
            {
                Debug.Log("No save file found for weapon stats.");
            }
            try
            {
                traits = JsonConvert.DeserializeObject<TraitsHolder>(File.ReadAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/traitssave.json")));
                TraitsManager.icy = traits.icy;
                TraitsManager.icyExp = traits.icyExp;
                TraitsManager.icyLvl = traits.icyLvl;
                TraitsManager.inferno = traits.inferno;
                TraitsManager.infernoExp = traits.infernoExp;
                TraitsManager.infernoLvl = traits.infernoLvl;
                TraitsManager.vampirism = traits.vampirism;
                TraitsManager.vampirismLvl = traits.vampirismLvl;
                TraitsManager.vampirismExp = traits.vampirismExp;
            }
            catch
            {
                Debug.Log("No save file found for traits.");
            }
        }
    }
    public class Main : MonoBehaviour
    {
        StatsInfo info;
        static float prevMana, prevFocus, magicDamage, combatDamage;
        float timer1;
        Dictionary<string, float> statBoosters;
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
            if (!RPGManager.bindedMainExpEvent)
                EventManager.onCreatureHit += EventManager_onCreatureHitEvent;
            timer1 = Time.time;
            Player.currentCreature.mana.currentFocus = Player.currentCreature.mana.maxFocus;
            Player.currentCreature.mana.currentMana = Player.currentCreature.mana.maxMana;
            RPGManager.statInfo = info;
            statBoosters = new Dictionary<string, float>();
            PerkTraitCheck();
            ValidateBooster();
            if (statBoosters.Values.Count < 1)
                statBoosters.Add("Default", 1);
            UpdateLevels();
            UpdateStats();
            ValidateLevels();
            SaveGame();
            Debug.LogWarning("Stats awoken!");
            RPGManager.bindedMainExpEvent = true;
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
                if (Player.local.locomotion.velocity.y > .25f)
                    StatManager.physicalJumpExp += 0.05f * Time.deltaTime;
                if (Time.time - timer1 > info.updateLevelTime)
                {
                    UpdateLevels();
                    ValidateLevels();
                    SaveGame();
                    ValidateBooster();
                    PerkTraitCheck();
                    UpdateStats();
                    timer1 = Time.time;
                }
            }
        }
        void SaveGame()
        {
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/statsave.json"), JsonConvert.SerializeObject(new StatManager(), Formatting.Indented));
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/weaponsave.json"), JsonConvert.SerializeObject(new WeaponStatManager(), Formatting.Indented));
            File.WriteAllText(Path.Combine(Application.streamingAssetsPath, "Mods/Amnesia RPG/Saves/traitssave.json"), JsonConvert.SerializeObject(new TraitsManager(), Formatting.Indented));
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
            if (TraitsManager.vampirismExp > (info.startingMaxExp * TraitsManager.vampirismLvl))
            {
                TraitsManager.vampirismLvl += 1;
                TraitsManager.vampirismExp = 0;
            }
            if (TraitsManager.icyExp > (info.startingMaxExp * TraitsManager.icyLvl))
            {
                TraitsManager.icyLvl += 1;
                TraitsManager.icyExp = 0;
            }
            if (TraitsManager.infernoExp > (info.startingMaxExp * TraitsManager.infernoLvl))
            {
                TraitsManager.infernoLvl += 1;
                TraitsManager.infernoExp = 0;
            }
        }
        void PerkTraitCheck()
        {
            if (StatManager.miscFocusLvl > 50 && !Player.currentCreature.gameObject.GetComponent<FocusReflexes>())
                Player.currentCreature.gameObject.AddComponent<FocusReflexes>();
            if (StatManager.miscFocusLvl > 20 && !Player.currentCreature.gameObject.GetComponent<FocusEnhance>())
                Player.currentCreature.gameObject.AddComponent<FocusEnhance>();
            if (StatManager.combatHealthLvl > 20 && !Player.currentCreature.gameObject.GetComponent<CombatRegeneration>())
                Player.currentCreature.gameObject.AddComponent<CombatRegeneration>();
            if (TraitsManager.vampirism && !Player.currentCreature.gameObject.GetComponent<VampirismBoth>())
                Player.currentCreature.gameObject.AddComponent<VampirismBoth>();
            if (TraitsManager.icy && !Player.currentCreature.gameObject.GetComponent<IcyPlayer>())
                Player.currentCreature.gameObject.AddComponent<IcyPlayer>();
        }
        void ValidateBooster()
        {
            if (Player.currentCreature.gameObject.GetComponent<VampirismBoth>())
            {
                if (!statBoosters.ContainsKey("Vampirism"))
                    statBoosters.Add("Vampirism", VampirismBoth.blood / 50);
                else
                {
                    statBoosters.Remove("Vampirism");
                    statBoosters.Add("Vampirism", VampirismBoth.blood / 50);
                }
            }
        }
        void ValidateLevels()
        {
            Player.currentCreature.maxHealth = 16 + ((StatManager.combatHealthLvl * 1f) - 1f) * statBoosters.Values.Sum();
            Player.currentCreature.mana.maxMana = 50 + ((StatManager.magicEfficiencyLvl * 2) - 2) * statBoosters.Values.Sum();
            Player.currentCreature.mana.chargeSpeedMultiplier = 1 + ((StatManager.magicEfficiencyLvl * 0.1f) - 0.1f) * statBoosters.Values.Sum();
            Player.currentCreature.mana.maxFocus = 20 + ((StatManager.miscFocusLvl * 2) - 2) * statBoosters.Values.Sum();
            combatDamage = 1f + ((StatManager.combatStrengthLvl * 0.5f) - 0.5f) * statBoosters.Values.Sum();
            magicDamage = ((StatManager.magicDamageLvl * 2) - 2) * statBoosters.Values.Sum();
            float speedIncrease = Mathf.Min(0.7f + ((StatManager.physicalSpeedLvl * 0.08f) - 0.08f) * statBoosters.Values.Sum(), 3.5f);
            Player.local.locomotion.runSpeedMultiplier = (speedIncrease / 1.2f) * statBoosters.Values.Sum();
            Player.local.locomotion.speedMultiplier = Mathf.Min(speedIncrease * 0.8f, 1f) * statBoosters.Values.Sum();
            float jumpIncrease = Mathf.Min(0.5f + ((StatManager.physicalJumpLvl * 0.1f) * statBoosters.Values.Sum() - 0.1f), 1);
            Player.local.locomotion.jumpMaxDuration = jumpIncrease / 3.5f * statBoosters.Values.Sum();
            Player.local.locomotion.jumpGroundForce = jumpIncrease / 1.7f * statBoosters.Values.Sum();
            Player.local.locomotion.airSpeed = jumpIncrease;
            Player.local.locomotion.jumpClimbMultiplier = Mathf.Min(jumpIncrease / 2, 1);
            Player.currentCreature.mana.manaRegen = 4 + ((StatManager.magicEfficiencyLvl * 0.5f) - 0.5f) * statBoosters.Values.Sum();
            GameManager.options.handlePositionDamperMultiplier = Mathf.RoundToInt(20 + ((StatManager.combatStrengthLvl * 1f) - 1f) * statBoosters.Values.Sum());
            GameManager.options.handlePositionSpringMultiplier = Mathf.RoundToInt(20 + ((StatManager.combatStrengthLvl * 1f) - 1f) * statBoosters.Values.Sum());
            GameManager.options.handleRotationDamperMultiplier = Mathf.RoundToInt(20 + ((StatManager.combatStrengthLvl * 1f) - 1f) * statBoosters.Values.Sum());
            GameManager.options.handleRotationSpringMultiplier = Mathf.RoundToInt(20 + ((StatManager.combatStrengthLvl * 1f) - 1f) * statBoosters.Values.Sum());
            if (StatManager.combatStrengthLvl >= 20)
                GameManager.options.handleForceXYZ = true;
            else
                GameManager.options.handleForceXYZ = false;
            GameManager.options.Apply();
            CreatureData maleData = Catalog.GetData<CreatureData>("PlayerDefaultMale");
            CreatureData femaleData = Catalog.GetData<CreatureData>("PlayerDefaultFemale");
            maleData.forcePositionSpringDamper = new Vector2(3750 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum(), 100);
            maleData.forceRotationSpringDamper = new Vector2(750 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum(), 50);
            maleData.climbingForceMaxPosition = 1000 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            maleData.climbingForceMaxRotation = 100 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            maleData.gripForceMaxPosition = 750 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            maleData.gripForceMaxRotation = 75 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            maleData.health = Convert.ToInt16(16 + ((StatManager.combatHealthLvl * 1) - 1f) * statBoosters.Values.Sum());
            maleData.focus = 20 + ((StatManager.miscFocusLvl * 2) - 2) * statBoosters.Values.Sum();
            femaleData.forcePositionSpringDamper = new Vector2(3750 + (combatDamage * StatManager.combatStrengthLvl), 100) * statBoosters.Values.Sum();
            femaleData.forceRotationSpringDamper = new Vector2(750 + (combatDamage * StatManager.combatStrengthLvl), 50) * statBoosters.Values.Sum();
            femaleData.climbingForceMaxPosition = 1000 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            femaleData.climbingForceMaxRotation = 100 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            femaleData.gripForceMaxPosition = 750 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            femaleData.gripForceMaxRotation = 75 + (combatDamage * StatManager.combatStrengthLvl) * statBoosters.Values.Sum();
            femaleData.health = Convert.ToInt16(16 + ((StatManager.combatHealthLvl * 1f) - 1f) * statBoosters.Values.Sum());
            femaleData.focus = 20 + ((StatManager.miscFocusLvl * 2) - 2) * statBoosters.Values.Sum();
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