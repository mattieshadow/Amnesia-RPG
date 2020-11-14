using ThunderRoad;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Data.SqlTypes;

namespace ARPG
{
    public class RuneSpell : SpellCastProjectile
    {
        protected override void OnProjectileCollision(ref CollisionStruct collisionInstance)
        {
            base.OnProjectileCollision(ref collisionInstance);
            Vector3 point = collisionInstance.contactPoint;
            Catalog.GetData<ItemPhysic>("Rune").SpawnAsync(item => item.transform.position = point);
        }
    }
    public class EnchantSpell : SpellCastGravity
    {
        public static Item enchantedItem;
        bool firing, returning, up, guarding;
        public float booster;
        public string enchantment;
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            Player.currentCreature.GetHand(spellCaster.ragdollHand.side).OnGrabEvent += Entry_OnGrabEvent;
        }
        private void Entry_OnGrabEvent(Side side, Handle handle, float axisPosition, HandleOrientation orientation, EventTime eventTime)
        {
            if (enchantedItem)
            {
                enchantedItem.disallowDespawn = false;
                if (enchantedItem.imbues.Count > 0)
                    enchantedItem.imbues.ForEach(i => i.energy = 0);
            }
            enchantedItem = handle.item;
            enchantedItem.disallowDespawn = true;
        }
        public override void FixedUpdateCaster()
        {
            base.FixedUpdateCaster();
            if (!enchantedItem)
                return;
            if (firing && !up && !guarding)
            {
                enchantedItem.transform.right = enchantedItem.rb.velocity.normalized;
                if (Physics.Raycast(spellCaster.magic.position, spellCaster.magic.forward, out RaycastHit hitInfo, 99))
                {
                    if (hitInfo.collider.gameObject.GetComponentInParent<Creature>() && hitInfo.collider.gameObject.GetComponentInParent<Creature>() != Player.currentCreature)
                        booster = 55;
                    else
                        booster = 7;
                    enchantedItem.rb.AddForce((hitInfo.point - enchantedItem.transform.position).normalized * Mathf.Clamp(StatManager.magicEfficiencyLvl * booster, 8, 8888) * enchantedItem.rb.mass, ForceMode.Force);
                    enchantedItem.Throw();
                    if (returning)
                        returning = false;
                }
            }
        }
        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (!enchantedItem)
                return;
            enchantedItem.imbues.ForEach(i => i.Transfer(Catalog.GetData<SpellCastCharge>(enchantment), StatManager.magicEfficiencyLvl / 2));
            if (returning)
            {
                enchantedItem.transform.right = enchantedItem.rb.velocity.normalized;
                enchantedItem.rb.AddForce((spellCaster.ragdollHand.transform.position - enchantedItem.transform.position).normalized * Mathf.Clamp(StatManager.magicEfficiencyLvl, 8, 50), ForceMode.Force);
                enchantedItem.Throw();
                enchantedItem.rb.useGravity = false;
                if (Vector3.Distance(enchantedItem.transform.position, spellCaster.ragdollHand.transform.position) < 0.5f)
                {
                    spellCaster.ragdollHand.Grab(enchantedItem.GetMainHandle(spellCaster.ragdollHand.side));
                    spellCaster.isFiring = false;
                    currentCharge = 0;
                    returning = false;
                    enchantedItem.rb.useGravity = true;
                }
            }
            if (guarding && firing)
            {
                enchantedItem.transform.right = enchantedItem.rb.velocity.normalized;
                enchantedItem.Throw();
                enchantedItem.transform.position = new Vector3(enchantedItem.transform.position.x, Player.local.GetHand(spellCaster.ragdollHand.side).transform.position.y, enchantedItem.transform.position.z);
                enchantedItem.transform.position = Player.local.GetHand(spellCaster.ragdollHand.side).transform.position + (enchantedItem.transform.position - Player.local.GetHand(spellCaster.ragdollHand.side).transform.position).normalized * 2;
                enchantedItem.transform.RotateAround(Player.local.GetHand(spellCaster.ragdollHand.side).transform.position, Vector3.up, StatManager.magicEfficiencyLvl * 70 * Time.deltaTime);
            }
            if (Vector3.Dot(spellCaster.magic.forward, Vector3.down) > 0.95f)
                guarding = true;
            else
                guarding = false;
            if (Vector3.Dot(spellCaster.magic.forward, Vector3.up) > 0.95f)
                up = true;
            else
                up = false;

        }
        public override void Fire(bool active)
        {
            base.Fire(active);
            firing = active;
            if (!enchantedItem)
                return;
            enchantedItem.rb.useGravity = active;
            if (up)
                returning = true;
        }
        public override void Unload()
        {
            base.Unload();
            Player.currentCreature.GetHand(spellCaster.ragdollHand.side).OnGrabEvent -= Entry_OnGrabEvent;
        }
    }
    class Ice : MonoBehaviour
    {
        RagdollHand hand;
        public void Setup(RagdollHand hand)
        {
            this.hand = hand;
        }
        void Update()
        {
            if (Vector3.Dot(hand.rb.velocity.normalized, hand.transform.right) <= -0.9f && hand.rb.velocity.sqrMagnitude > (Player.local.locomotion.velocity.sqrMagnitude - 10))
                foreach (Collider collider in Physics.OverlapCapsule(hand.transform.position + (-hand.transform.right * 0.7f), hand.transform.position + (-hand.transform.right * 1f), 0.65f))
                {
                    if (collider.attachedRigidbody)
                    {
                        Rigidbody rb = collider.attachedRigidbody;
                        rb.AddForce(-(hand.transform.position - rb.transform.position).normalized * hand.rb.velocity.sqrMagnitude, ForceMode.Acceleration);
                        if (rb.GetComponentInParent<Creature>() && rb.GetComponentInParent<Creature>() != Player.currentCreature)
                        {
                            Creature x = rb.gameObject.GetComponentInParent<Creature>();
                            if (x.gameObject.GetComponent<Frozen>())
                                x.gameObject.GetComponent<Frozen>().Lower(0.07f * Time.deltaTime);
                            else
                            {
                                Frozen frozen = x.gameObject.AddComponent<Frozen>();
                                frozen.Lower(0.07f * Time.deltaTime);
                            }
                        }
                    }
                }
        }
    }
    public class IceSpell : SpellCastProjectile
    {
        bool firing;
        float timer;
        public override void Load(SpellCaster spellCaster)
        {
            base.Load(spellCaster);
            if (!spellCaster.ragdollHand.gameObject.GetComponent<Ice>())
                spellCaster.ragdollHand.gameObject.AddComponent<Ice>().Setup(spellCaster.ragdollHand);
        }
        public override void Fire(bool active)
        {
            base.Fire(active);
            firing = active;
            timer = Time.time;
        }
        public override void OnImbueCollisionStart(ref CollisionStruct collisionInstance)
        {
            base.OnImbueCollisionStart(ref collisionInstance);
            if (collisionInstance.damageStruct.hitRagdollPart?.ragdoll.creature)
            {
                Creature x = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
                if (x.gameObject.GetComponent<Frozen>())
                    x.gameObject.GetComponent<Frozen>().Lower(Mathf.Clamp(collisionInstance.damageStruct.damage / 10, 0.01f, .25f));
                else
                {
                    Frozen frozen = x.gameObject.AddComponent<Frozen>();
                    frozen.Lower(Mathf.Clamp(collisionInstance.damageStruct.damage / 10, 0.01f, .25f));
                }
            }
        }
        public override void UpdateImbue()
        {
            base.UpdateImbue();
            foreach (Creature creature in Creature.list)
            {
                if (Vector3.Distance(creature.transform.position, imbue.transform.position) < 4 && creature != Player.currentCreature)
                {
                    if (!creature.gameObject.GetComponent<Frozen>())
                        creature.gameObject.AddComponent<Frozen>();
                    Frozen frozen = creature.gameObject.GetComponent<Frozen>();
                    frozen.Lower(0.05f * Time.deltaTime);
                }
            }
        }
        public override void UpdateCaster()
        {
            base.UpdateCaster();
            if (firing)
            {
                foreach (Collider collider in Physics.OverlapCapsule(spellCaster.ragdollHand.transform.position + (-spellCaster.ragdollHand.transform.forward * 0.7f), spellCaster.ragdollHand.transform.position + (-spellCaster.ragdollHand.transform.forward * 10f), 0.65f))
                {
                    if (collider.attachedRigidbody)
                    {
                        Rigidbody rb = collider.attachedRigidbody;
                        rb.AddForce(-(spellCaster.magic.position - rb.transform.position).normalized * currentCharge * 3 * rb.mass, ForceMode.Acceleration);
                        if (rb.GetComponentInParent<Creature>() && rb.GetComponentInParent<Creature>() != Player.currentCreature)
                        {
                            Creature x = rb.gameObject.GetComponentInParent<Creature>();
                            if (x.gameObject.GetComponent<Frozen>())
                                x.gameObject.GetComponent<Frozen>().Lower(0.02f * Time.deltaTime);
                            else
                            {
                                Frozen frozen = x.gameObject.AddComponent<Frozen>();
                                frozen.Lower(0.02f * Time.deltaTime);
                            }
                        }
                    }
                }
            }
        }
        public override void Unload()
        {
            base.Unload();
            if (spellCaster.ragdollHand.gameObject.GetComponent<Ice>())
            {
                Object.Destroy(spellCaster.ragdollHand.gameObject.GetComponent<Ice>());
            }
        }
    }
    class Frozen : MonoBehaviour
    {
        float intensity = 1;
        Creature creature;
        Dictionary<RagdollPart, float> partsMass;
        void Awake()
        {
            creature = GetComponentInParent<Creature>();
            partsMass = new Dictionary<RagdollPart, float>();
            foreach (RagdollPart part in creature.ragdoll.parts)
                partsMass.Add(part, part.rb.drag);
        }
        public void Lower(float amount)
        {
            intensity -= amount;
        }
        void Update()
        {
            creature.animator.speed = Mathf.Clamp(intensity, 0.07f, 1);
            creature.locomotion.speedMultiplier = Mathf.Clamp(intensity, 0.07f, 1);
            foreach (RagdollPart part in creature.ragdoll.parts)
                if (partsMass.TryGetValue(part, out float x))
                {
                    part.rb.drag = x / intensity;
                }
            if (creature.isKilled)
            {
                creature.locomotion.speedMultiplier = 1;
                creature.animator.speed = 1;
                foreach (RagdollPart part in creature.ragdoll.parts)
                    if (partsMass.TryGetValue(part, out float x))
                        part.rb.drag = x;
                Destroy(this);
            }
        }
    }
    public class Trap : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<TrapMono>();
        }
    }
    public class TrapMono : MonoBehaviour
    {
        float timer;
        Item trap;
        void Awake()
        {
            trap = gameObject.GetComponentInParent<Item>();
        }
        void Update()
        {
            if (Time.time - timer > 1)
            {
                timer = Time.time;
                Collider[] colliders = Physics.OverlapSphere(trap.transform.position, 3);
                foreach (Collider collider in colliders)
                {
                    if (collider.attachedRigidbody)
                        collider.attachedRigidbody.AddForce(-(trap.transform.position - collider.attachedRigidbody.transform.position).normalized * 10, ForceMode.Impulse);
                }
            }
        }
    }
}
