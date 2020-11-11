using UnityEngine;
using ThunderRoad;
using System.Threading.Tasks;
namespace ARPG
{
    public class RuneSpell : SpellCastProjectile
    {
        protected override void OnProjectileCollision(ref CollisionStruct collisionInstance)
        {
            base.OnProjectileCollision(ref collisionInstance);
            Item ite;
            Catalog.GetData<ItemPhysic>("Rune").SpawnAsync(item => { Debug.Log(item.itemId); ite = item; });
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
                    if (collider.gameObject.GetComponentInParent<Creature>())
                        Detonate();
                    Debug.Log(collider);
                }
            }
        }
        void Detonate()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, 3);
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.GetComponentInParent<Creature>())
                {
                    Creature creature = collider.gameObject.GetComponentInParent<Creature>();
                    foreach (RagdollPart part in creature.ragdoll.parts)
                        part.rb.AddForce(-(gameObject.transform.position - part.transform.position).normalized * 2, ForceMode.Impulse);
                }
            }
            gameObject.GetComponentInParent<Item>().Despawn();
            Debug.Log("Detonated");
        }
    }
}
