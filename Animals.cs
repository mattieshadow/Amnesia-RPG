using ThunderRoad;
using UnityEngine;
namespace ARPG
{
    public class SheepItem : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<SheepAI>();
        }
    }
    public class SheepAI : MonoBehaviour
    {
        Animation animation;
        bool scared;
        float scaredTimer, rotationTimer;
        Item itemSheep;
        float health = 1, movementSpeed;
        EffectInstance bleedEffect;
        Creature attacker;
        void Awake()
        {
            animation = GetComponentInChildren<Animation>();
            itemSheep = GetComponentInParent<Item>();
            itemSheep.disallowDespawn = true;
            health = Random.Range(25, 75);
            rotationTimer = Time.time;
            movementSpeed = Random.Range(0.5f, 3);
            bleedEffect = Catalog.GetData<EffectData>("Bleeding").Spawn(transform.position, Quaternion.identity, transform);
            Debug.Log("Sheep awoken!");
        }
        void Update()
        {
            foreach (Creature creature in Creature.list)
                if (creature.gameObject.GetComponent<VampirismBoth>())
                    if (Vector3.Distance(creature.ragdoll.GetPart(RagdollPart.Type.Head).transform.position, transform.position) < 0.3f && !attacker)
                    {
                        attacker = creature;
                        bleedEffect.Play();
                    }
                    else
                    {
                        bleedEffect.Stop();
                    }
            if (Vector3.Distance(Player.currentCreature.transform.position, transform.position) < 1)
            {
                scared = true;
                scaredTimer = Time.time;
            }
            if (Time.time - scaredTimer > 10 && scared)
                scared = false;
            if (scared)
            {
                if (!animation.isPlaying)
                    animation.Play("Armature_Jump");
                itemSheep.transform.position = itemSheep.transform.position + (gameObject.transform.forward * movementSpeed * Time.deltaTime);
                if (Physics.Raycast(itemSheep.transform.position + (itemSheep.transform.up * 0.1f), itemSheep.transform.forward, out RaycastHit hit, 999))
                {
                    Debug.Log(Vector3.Distance(hit.point, itemSheep.transform.position));
                }
            }
            if (health <= 0)
                itemSheep.Despawn();
            if (Time.time - rotationTimer > 1)
            {
                rotationTimer = Time.time;
                itemSheep.transform.LookAt(transform);
            }
        }
        void OnCollisionEnter(Collision collision)
        {
            float damage = collision.relativeVelocity.magnitude;
            scared = true;
            scaredTimer = Time.time;
            if (damage > 4)
                health -= damage;
        }
    }
}
