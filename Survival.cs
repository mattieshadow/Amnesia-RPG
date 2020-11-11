using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;
using UnityEngine.Accessibility;
namespace ARPG
{
    public class PlayerSurvival : MonoBehaviour
    {
        /*float timer1, timer2;
        public static float food, thirst, stamina;
        public static Vector3 playerMouth;
        public static bool starving;
        void Awake()
        {
            timer1 = Time.time;
            timer2 = Time.time;
            food = StatManager.survivalLvl;
            playerMouth = Player.currentCreature.ragdoll.headPart.transform.position;
            foreach (AssetBundle bundle in AssetBundle.GetAllLoadedAssetBundles())
                if (bundle.name == "amnesiarpgprops")
                    SurvivalManager.bundle = bundle;
                else
                    SurvivalManager.bundle = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath, "ARPG/Items/amnesiarpgprops.assets"));
        }
        void Update()
        {
            if (Time.time - timer1 > 20)
            {
                food -= 0.1f;
                thirst -= 0.1f;
                timer1 = Time.time;
            }
            if (food < 10)
                starving = true;
            if (starving && Time.time - timer2 > 60)
            {
                GameObject hungry = Instantiate(SurvivalManager.bundle.LoadAsset<GameObject>("Hungry"), playerMouth, Quaternion.identity);
                hungry.AddComponent<DeletionMono>();
            }
        }
    }
    class DeletionMono : MonoBehaviour
    {
        float timeLeft;
        void Awake()
        {
            timeLeft = Time.time;
        }
        void Update()
        {
            if (Time.time - timeLeft > 4)
            {
                gameObject.SetActive(false);
            }
        }
    }
    public class Bread : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<BreadMono>();
        }
    }
    public class Water : ItemModule
    {
        public override void OnItemLoaded(Item item)
        {
            base.OnItemLoaded(item);
            item.gameObject.AddComponent<WaterMono>();
        }
    }
    public class WaterMono : MonoBehaviour
    {
        Animator animator;
        Item water;
        float cooldown;
        int stage = 1;
        void Awake()
        {
            water = GetComponentInParent<Item>();
            animator = GetComponentInChildren<Animator>();
        }
        void Update()
        {
            if (cooldown > 0)
                cooldown -= Time.deltaTime;
            if (Vector3.Distance(water.transform.position, PlayerSurvival.playerMouth) < 0.5f && cooldown <= 0)
            {
                Sip();
                cooldown++;
            }
        }
        void Sip()
        {
            if (stage is 1)
            {
                animator.SetBool("Water Pour 1", true);
                PlayerSurvival.thirst += Mathf.Clamp(10 * StatManager.survivalLvl, 10, 20);
                StatManager.survivalExp += 1;
            }
            if (stage is 2)
            {
                animator.SetBool("Water Pour 2", true);
                PlayerSurvival.thirst += Mathf.Clamp(10 * StatManager.survivalLvl, 10, 20);
                StatManager.survivalExp += 1;
            }
            if (stage is 3)
            {
                animator.SetBool("Water Pour 3", true);
                PlayerSurvival.thirst += Mathf.Clamp(10 * StatManager.survivalLvl, 10, 20);
                StatManager.survivalExp += 1;
            }
            if (stage is 4)
            {
                animator.SetBool("Water Pour 4", true);
                PlayerSurvival.thirst += Mathf.Clamp(10 * StatManager.survivalLvl, 10, 20);
                StatManager.survivalExp += 1;
            }
            if (stage is 5)
            {
                animator.SetBool("Water Pour 5", true);
                PlayerSurvival.thirst += Mathf.Clamp(10 * StatManager.survivalLvl, 10, 20);
                StatManager.survivalExp += 1;
            }
            stage++;
        }
        void Refill()
        {

        }
    }
    public class BreadMono : MonoBehaviour
    {
        Animator animator;
        Item bread;
        float cooldown;
        int stage = 1;
        void Awake()
        {
            bread = GetComponentInParent<Item>();
            animator = GetComponentInChildren<Animator>();
        }
        void Update()
        {
            if (cooldown > 0)
                cooldown -= Time.deltaTime;
            if (Vector3.Distance(transform.position, PlayerSurvival.playerMouth) < 0.5f && cooldown <= 0)
            {
                Bite();
                cooldown++;
            }
        }
        void Bite()
        {
            if (stage is 1)
            {
                Debug.Log("Set one");
                animator.SetBool("BreadEat1", true);
                PlayerSurvival.food += Mathf.Clamp(5 * StatManager.survivalLvl, 5, 10);
                StatManager.survivalExp += 1;
            }
            if (stage is 2)
            {
                Debug.Log("Set two");
                animator.SetBool("BreadEat2", true);
                PlayerSurvival.food += Mathf.Clamp(5 * StatManager.survivalLvl, 5, 10);
                StatManager.survivalExp += 1;
            }
            if (stage is 3)
            {
                Debug.Log("Set three");
                animator.SetBool("BreadEat3", true);
                PlayerSurvival.food += Mathf.Clamp(5 * StatManager.survivalLvl, 5, 10);
                StatManager.survivalExp += 1;
            }
            if (stage is 4)
            {
                Debug.Log("Set four");
                animator.SetBool("BreadEat4", true);
                PlayerSurvival.food += Mathf.Clamp(5 * StatManager.survivalLvl, 5, 10);
                StatManager.survivalExp += 1;
                bread.Despawn();
            }
            stage++;
        }
    }
    public static class SurvivalManager
    {
        public static AssetBundle bundle;
    }
        */
    }
}