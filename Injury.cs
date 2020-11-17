using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;
using System.IO;
using System.Collections;
namespace ARPG
{
    public class InjurySystem : LevelModule
    {
        public bool braindeadInjury, bleedingoutAllowed, concussionAllowed, legInjuries, armInjuries, breakableNecks, spineParalysis;
        public float legBreakDamage, armBreakDamage, neckBreakDamage, damageToParalyze, concussionSkullBreakDamage, damageToDestroyLarynx, minStamina = 0.4f, bleedMult = 1.7f, secondsBetweenConcussionTrips, timeBeforeDespawn;
        public int maxConcussionStacks, maxArmandLegStacks;
        public override IEnumerator OnLoadCoroutine(Level level)
        {
            EventManager.onCreatureHit += EventManager_onCreatureHitEvent;
            InjuryManager.bleedDamage = bleedMult;
            InjuryManager.minStamina = minStamina;
            InjuryManager.tripTime = secondsBetweenConcussionTrips;
            InjuryManager.maxConcussionStacks = maxConcussionStacks;
            InjuryManager.maxArmandLegStacks = maxArmandLegStacks;
            InjuryManager.timeBeforeDespawn = timeBeforeDespawn;
            Debug.LogWarning("Injury system awoken");
            return base.OnLoadCoroutine(level);
        }
        private void EventManager_onCreatureHitEvent(Creature creature, ref CollisionStruct collisionStruct)
        {
            if (creature.data.id == "HumanMale" || creature.data.id == "HumanFemale")
            {
                if (!creature.isKilled)
                {
                    if (collisionStruct.damageStruct.damageType == DamageType.Blunt && collisionStruct.damageStruct.damage > 3)
                    {
                        creature.currentHealth *= 0.75f;
                    }
                    if (concussionAllowed && collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Head) && collisionStruct.damageStruct.damage > concussionSkullBreakDamage && collisionStruct.damageStruct.damageType == DamageType.Blunt)
                    {
                        float x = collisionStruct.damageStruct.damage / concussionSkullBreakDamage;
                        creature.gameObject.GetComponent<Systems>().currentStacks += Mathf.RoundToInt(x);
                    }
                    if (braindeadInjury && collisionStruct.damageStruct.damageType == DamageType.Pierce && collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Head))
                    {
                        creature.Kill();
                    }
                    if (collisionStruct.damageStruct.damageType != DamageType.Energy)
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Neck))
                        {
                            creature.gameObject.GetComponent<Systems>().destroyedLarynx = true;
                            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                        }
                    if (bleedingoutAllowed && collisionStruct.damageStruct.damageType != DamageType.Energy && collisionStruct.damageStruct.damageType != DamageType.Blunt)
                    {
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Neck))
                        {
                            EffectInstance effect = Catalog.GetData<EffectData>("Blood").Spawn(collisionStruct.contactPoint, Quaternion.identity, collisionStruct.damageStruct.hitRagdollPart.transform);
                            effect.SetIntensity(1);
                            effect.Play();
                            if (!creature.gameObject.GetComponent<Bleedingout>())
                            {
                                creature.gameObject.AddComponent<Bleedingout>();
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage = collisionStruct.damageStruct.damage;
                                src.stacks = 1;
                                src.effects.Add(effect);
                            }
                            else
                            {
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage += collisionStruct.damageStruct.damage * 2f;
                                src.stacks += 1;
                                src.effects.Add(effect);
                            }
                        }
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Head))
                        {
                            EffectInstance effect = Catalog.GetData<EffectData>("Blood").Spawn(collisionStruct.contactPoint, Quaternion.identity, collisionStruct.damageStruct.hitRagdollPart.transform);
                            effect.Play();
                            if (!creature.gameObject.GetComponent<Bleedingout>())
                            {
                                creature.gameObject.AddComponent<Bleedingout>();
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage = collisionStruct.damageStruct.damage / 3f;
                                src.stacks = 1;
                                src.effects.Add(effect);
                            }
                            else
                            {
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage += collisionStruct.damageStruct.damage / 2;
                                src.stacks += 1;
                                src.effects.Add(effect);
                            }
                        }
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Torso))
                        {
                            EffectInstance effect = Catalog.GetData<EffectData>("Blood").Spawn(collisionStruct.contactPoint, Quaternion.identity, collisionStruct.damageStruct.hitRagdollPart.transform);
                            effect.Play();
                            if (!creature.gameObject.GetComponent<Bleedingout>())
                            {
                                creature.gameObject.AddComponent<Bleedingout>();
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage = collisionStruct.damageStruct.damage / 5.5f;
                                src.stacks = 1;
                                src.effects.Add(effect);
                            }
                            else
                            {
                                Bleedingout src = creature.gameObject.GetComponent<Bleedingout>();
                                src.bleedDamage += collisionStruct.damageStruct.damage / 3;
                                src.stacks += 1;
                                src.effects.Add(effect);
                            }
                        }
                    }
                    if (legInjuries && collisionStruct.damageStruct.damage > legBreakDamage)
                    {
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg))
                            if (!creature.gameObject.GetComponent<BrokenLeftLeg>())
                            {
                                creature.gameObject.AddComponent<BrokenLeftLeg>();
                                creature.gameObject.GetComponent<BrokenLeftLeg>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / legBreakDamage);
                            }
                            else
                            {
                                creature.gameObject.GetComponent<BrokenLeftLeg>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / legBreakDamage);
                            }
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.RightLeg))
                            if (!creature.gameObject.GetComponent<BrokenRightLeg>())
                            {
                                creature.gameObject.AddComponent<BrokenRightLeg>();
                                creature.gameObject.GetComponent<BrokenRightLeg>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / legBreakDamage);
                            }
                            else
                            {
                                creature.gameObject.GetComponent<BrokenRightLeg>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / legBreakDamage);
                            }
                    }
                    if (armInjuries && collisionStruct.damageStruct.damage > armBreakDamage)
                    {
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.LeftArm))
                            if (!creature.gameObject.GetComponent<BrokenLeftArm>())
                            {
                                creature.gameObject.AddComponent<BrokenLeftArm>();
                                creature.gameObject.GetComponent<BrokenLeftArm>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / armBreakDamage);
                            }
                            else
                            {
                                creature.gameObject.GetComponent<BrokenLeftArm>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / armBreakDamage);
                            }
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.RightArm))
                            if (!creature.gameObject.GetComponent<BrokenRightArm>())
                            {
                                creature.gameObject.AddComponent<BrokenRightArm>();
                                creature.gameObject.GetComponent<BrokenRightArm>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / armBreakDamage);
                            }
                            else
                            {
                                creature.gameObject.GetComponent<BrokenRightArm>().currentStacks += Mathf.RoundToInt(collisionStruct.damageStruct.damage / armBreakDamage);
                            }
                    }
                    if (breakableNecks && collisionStruct.damageStruct.damage > neckBreakDamage && collisionStruct.damageStruct.damageType == DamageType.Blunt)
                    {
                        if (collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Head) || collisionStruct.damageStruct.hitRagdollPart == creature.ragdoll.GetPart(RagdollPart.Type.Neck))
                        {
                            creature.Kill();
                        }
                    }
                }
            }
        }
        public override void Update(Level level)
        {
            base.Update(level);
            if (Player.currentCreature != null)
            {
                foreach (Creature victim in Creature.list)
                {
                    if (victim.isKilled && !victim.gameObject.GetComponent<DespawnScript>())
                    {
                        victim.gameObject.AddComponent<DespawnScript>();
                    }
                    if (!victim.isKilled && !victim.gameObject.GetComponent<Systems>())
                    {
                        victim.gameObject.AddComponent<Systems>();
                    }
                    if (victim.gameObject.GetComponent<BrokenLeftLeg>() && victim.gameObject.GetComponent<BrokenRightLeg>() && !victim.isKilled)
                        if (victim.gameObject.GetComponent<BrokenLeftLeg>().currentStacks >= InjuryManager.maxArmandLegStacks && victim.gameObject.GetComponent<BrokenRightLeg>().currentStacks >= InjuryManager.maxArmandLegStacks)
                            victim.ragdoll.SetState(Ragdoll.State.Destabilized);
                }
            }
        }
    }
    public class BrokenRightArm : MonoBehaviour
    {
        Creature creature;
        public int currentStacks;
        float timer;
        bool permanent = false;
        private void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
        }
        private void Update()
        {
            if (creature.isKilled)
            {
                creature.Kill();
                Destroy(creature.gameObject.GetComponent<BrokenRightArm>());
            }
            BreakingArms();
            if (Time.time - timer > 5 * currentStacks)
            {
                currentStacks -= 1;
                timer = Time.time;
            }
            if (currentStacks >= InjuryManager.maxArmandLegStacks)
                permanent = true;
            if (permanent)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightArm).DisableCharJointLimit();
            }
        }
        private void BreakingArms()
        {
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.25f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightHand).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.5f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightArm).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.75f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightArm).DisableCharJointLimit();
            }
        }
    }
    public class BrokenLeftArm : MonoBehaviour
    {
        Creature creature;
        public int currentStacks;
        float timer;
        bool permanent = false;
        private void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
        }
        private void Update()
        {
            if (creature.isKilled)
            {
                creature.Kill();
                Destroy(creature.gameObject.GetComponent<BrokenLeftArm>());
            }
            BreakingArms();
            if (Time.time - timer > 5 * currentStacks)
            {
                currentStacks -= 1;
                timer = Time.time;
            }
            if (currentStacks >= InjuryManager.maxArmandLegStacks)
                permanent = true;
            if (permanent)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).DisableCharJointLimit();
            }
        }
        private void BreakingArms()
        {
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.25f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftHand).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.5f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.75f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).DisableCharJointLimit();
            }
        }
    }
    public class BrokenLeftLeg : MonoBehaviour
    {
        Creature creature;
        public int currentStacks;
        float timer;
        bool permanent = false;
        private void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
        }
        private void Update()
        {
            if (creature.isKilled)
            {
                creature.Kill();
                Destroy(creature.gameObject.GetComponent<BrokenLeftLeg>());
            }
            BreakingLegs();
            if (Time.time - timer > 5 * currentStacks)
            {
                currentStacks -= 1;
                timer = Time.time;
            }
            if (currentStacks >= InjuryManager.maxArmandLegStacks)
                permanent = true;
            if (permanent)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).DisableCharJointLimit();
            }
        }
        private void BreakingLegs()
        {
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.25f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftFoot).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.5f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.75f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg).DisableCharJointLimit();
            }
        }
    }
    public class BrokenRightLeg : MonoBehaviour
    {
        Creature creature;
        public int currentStacks;
        float timer;
        bool permanent = false;
        private void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
        }
        private void Update()
        {
            if (creature.isKilled)
            {
                Destroy(creature.gameObject.GetComponent<BrokenRightLeg>());
            }
            BreakingLegs();
            if (Time.time - timer > 5 * currentStacks)
            {
                currentStacks -= 1;
                timer = Time.time;
            }
            if (currentStacks >= InjuryManager.maxArmandLegStacks)
                permanent = true;
            if (permanent)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).DisableCharJointLimit();
                creature.ragdoll.GetPart(RagdollPart.Type.RightFoot).DisableCharJointLimit();
            }
        }
        private void BreakingLegs()
        {
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.25f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightFoot).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.5f)
            {
                creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).DisableCharJointLimit();
            }
            if (currentStacks > InjuryManager.maxArmandLegStacks * 0.75f)
            {

                creature.ragdoll.GetPart(RagdollPart.Type.RightLeg).DisableCharJointLimit();
            }
        }
    }
    public class Systems : MonoBehaviour
    {
        Creature creature;
        public bool left;
        public bool right;
        bool isPlayer;
        public bool destroyedLarynx;
        public int currentStacks;
        public bool concussion;
        float time = 0;
        public void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
            if (creature == Player.currentCreature)
                isPlayer = true;
        }
        public void Update()
        {
            try
            {
                if (!isPlayer)
                {
                    if (creature.isKilled)
                    {
                        destroyedLarynx = false;
                        currentStacks = 0;
                    }
                    if (creature.currentHealth > creature.maxHealth * InjuryManager.minStamina)
                    {
                        creature.animator.speed = creature.currentHealth / creature.maxHealth;
                        if (!left && !right)
                            creature.locomotion.speedMultiplier = creature.currentHealth / creature.maxHealth;
                    }
                    if (!InjuryManager.slowDeaths && creature.isKilled)
                        creature.animator.speed = 1;
                    if (InjuryManager.slowDeaths && creature.isKilled)
                        creature.animator.speed = InjuryManager.slowDeathSpeed;
                    if (creature.currentHealth <= creature.maxHealth && InjuryManager.npcRegeneration)
                        creature.currentHealth += creature.maxHealth * (InjuryManager.npcRegenPerSecond / 100) * Time.deltaTime;
                    if (destroyedLarynx)
                        creature.groundStabilizationMinDuration = 1f;
                    if (currentStacks >= InjuryManager.maxConcussionStacks)
                        creature.Kill();
                    if (currentStacks > 0)
                        concussion = true;
                    else
                        concussion = false;
                    if (concussion)
                    {
                        if (Time.time - time > InjuryManager.tripTime / currentStacks)
                        {
                            time = Time.time;
                            if (!creature.isKilled)
                                creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                            currentStacks -= 1;
                        }
                    }
                }
            }
            catch
            {
                Debug.Log("Something went wrong in Injury Reborn; Stamina: npc");
            }
            try
            {
                if (isPlayer)
                {
                    if (creature.currentHealth <= creature.maxHealth && InjuryManager.playerRegeneration)
                        creature.currentHealth += creature.maxHealth * (InjuryManager.playerRegenPerSecond / 100) * Time.deltaTime;
                }
            }
            catch
            {
                Debug.Log("Something went wrong in Injury Reborn; Stamina: player");
            }
        }
    }
    public class Bleedingout : MonoBehaviour
    {
        Creature creature;
        public float bleedDamage;
        public int stacks;
        public List<EffectInstance> effects = new List<EffectInstance>();
        public void Awake()
        {
            creature = gameObject.GetComponentInParent<Creature>();
        }
        public void Update()
        {
            if (creature.isKilled || creature == null)
            {
                Destroy(creature.gameObject.GetComponent<Bleedingout>());
                foreach (EffectInstance effect in effects)
                    effect.Despawn();
            }
            if (creature.currentHealth > 0)
                creature.currentHealth -= (bleedDamage + (stacks * InjuryManager.bleedDamage)) * Time.deltaTime;
            else
            {
                creature.Kill();
            }
        }
    }
    public class Faking : MonoBehaviour
    {
        Creature creature;
        float timer, timer2;
        bool done;
        void Awake()
        {
            creature = GetComponentInParent<Creature>();
            timer = Time.time;
            timer2 = Time.time;
        }
        void Update()
        {
            creature.ragdoll.SetState(Ragdoll.State.Inert);
            if (Time.time - timer2 > 1 && !done)
            {
                creature.Resurrect(creature.maxHealth, Player.currentCreature);
                done = true;
            }
            if (Time.time - timer2 < 24.9f && done && creature.isKilled)
                Destroy(this);
            if (Time.time - timer > 25 && !creature.isKilled)
            {
                creature.ragdoll.StandUp();
                creature.brain.Load(creature.brain.instance.id);
                Destroy(this);
            }
        }
    }
    public class DespawnScript : MonoBehaviour
    {
        Creature victim;
        float thetime;
        public void Awake()
        {
            victim = gameObject.GetComponentInParent<Creature>();
            thetime = Time.time;
        }
        public void Update()
        {
            if (victim == null)
            {
                Destroy(victim.gameObject.GetComponent<DespawnScript>());
            }
            if (Time.time - thetime > InjuryManager.timeBeforeDespawn)
                if (victim.isKilled)
                {
                    victim.Despawn();
                    Destroy(victim.gameObject.GetComponent<DespawnScript>());
                }
                else
                {
                    Destroy(victim.gameObject.GetComponent<DespawnScript>());
                }
        }
    }
    public static class InjuryManager
    {
        public static bool slowDeaths;
        public static bool npcRegeneration;
        public static bool playerRegeneration;
        public static float bleedDamage = 1.7f;
        public static float minStamina = 0.4f;
        public static float tripTime = 50;
        public static float slowDeathSpeed = 0.2f;
        public static float npcRegenPerSecond = 1;
        public static float playerRegenPerSecond = 1;
        public static float timeBeforeDespawn;
        public static int maxConcussionStacks;
        public static int maxArmandLegStacks;
    }
}