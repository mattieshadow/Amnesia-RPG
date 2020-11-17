using UnityEngine;
using ThunderRoad;
namespace ARPG
{
    public class FocusEnhance : MonoBehaviour
    {
        Item itemLeft, itemRight;
        bool enhancingLeft = false, enhancingRight = false;
        float previousDragLeft, previousAngularLeft, previousDragRight, previousAngularRight, previousMassRight, previousMassLeft;
        void Awake()
        {
            Debug.Log("Focus perk added.");
        }
        void Update()
        {
            if (Player.currentCreature.handLeft.grabbedHandle?.item && Player.currentCreature.handLeft.grabbedHandle.item != itemLeft)
            {
                itemLeft = Player.currentCreature.handLeft.grabbedHandle.item;
                itemLeft.OnHeldActionEvent += ItemLeft_OnHeldActionEventLeft;
                Bookmark(Side.Left);
            }
            if (Player.currentCreature.handRight.grabbedHandle?.item && Player.currentCreature.handRight.grabbedHandle.item != itemRight)
            {
                itemRight = Player.currentCreature.handRight.grabbedHandle.item;
                itemRight.OnHeldActionEvent += ItemRight_OnHeldActionEventRight;
                Bookmark(Side.Right);
            }
            if (itemLeft && itemLeft.handlers.Count is 0)
                UnbuffItem(Side.Left);
            if (itemRight && itemRight.handlers.Count is 0)
                UnbuffItem(Side.Right);
        }
        private void ItemLeft_OnHeldActionEventLeft(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
                Enhance(Side.Left);
            if (action == Interactable.Action.AlternateUseStop)
                Dehance(Side.Left);
        }
        private void ItemRight_OnHeldActionEventRight(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
        {
            if (action == Interactable.Action.AlternateUseStart)
                Enhance(Side.Right);
            if (action == Interactable.Action.AlternateUseStop)
                Dehance(Side.Right);
        }
        void Dehance(Side side)
        {
            if (side == Side.Left)
            {
                if (!itemLeft)
                    return;
                enhancingLeft = false;
                itemLeft.rb.angularDrag = previousAngularLeft;
                itemLeft.rb.drag = previousDragLeft;
                itemLeft.rb.mass = previousMassLeft;
            }
            else
            {
                if (!itemRight)
                    return;
                enhancingRight = false;
                itemRight.rb.angularDrag = previousAngularRight;
                itemRight.rb.drag = previousDragRight;
                itemRight.rb.mass = previousMassRight;
            }
        }
        void Enhance(Side side)
        {
            if (side == Side.Left)
            {
                if (!itemLeft)
                    return;
                enhancingLeft = true;
                itemLeft.rb.angularDrag = previousAngularLeft / StatManager.miscFocusLvl;
                itemLeft.rb.drag = previousDragLeft / StatManager.miscFocusLvl;
                itemLeft.rb.mass = previousMassLeft / StatManager.miscFocusLvl;
            }
            else
            {
                if (!itemRight)
                    return;
                enhancingRight = true;
                itemRight.rb.angularDrag = previousAngularRight / StatManager.miscFocusLvl;
                itemRight.rb.drag = previousDragRight / StatManager.miscFocusLvl;
                itemRight.rb.mass = previousMassRight / StatManager.miscFocusLvl;
            }
        }
        void Bookmark(Side side)
        {
            if (side == Side.Left)
            {
                if (!itemLeft)
                    return;
                previousAngularLeft = itemLeft.rb.angularDrag;
                previousDragLeft = itemLeft.rb.drag;
                previousMassLeft = itemLeft.rb.mass;
            }
            else
            {
                if (!itemRight)
                    return;
                previousAngularRight = itemRight.rb.angularDrag;
                previousDragRight = itemRight.rb.angularDrag;
                previousMassRight = itemRight.rb.mass;
            }
        }
        void UnbuffItem(Side side)
        {
            if (side == Side.Left)
            {
                if (!itemLeft)
                    return;
                itemLeft.rb.angularDrag = previousAngularLeft;
                itemLeft.rb.drag = previousDragLeft;
                itemLeft.OnHeldActionEvent -= ItemLeft_OnHeldActionEventLeft;
                itemLeft = null;
                enhancingLeft = false;
            }
            else
            {
                if (!itemRight)
                    return;
                itemRight.rb.angularDrag = previousAngularRight;
                itemRight.rb.drag = previousDragRight;
                itemRight.OnHeldActionEvent -= ItemRight_OnHeldActionEventRight;
                itemRight = null;
                enhancingRight = true;
            }
        }
    }
    public class FocusReflexes : MonoBehaviour
    {
        Item incitingItem;
        float timer;
        bool reset;
        void Awake()
        {
            Debug.Log("Reflexes perk added.");
        }
        void Update()
        {
            if (Player.currentCreature)
            {
                if (!incitingItem)
                    foreach (Item item in Item.list)
                        if (item.data.id is "DynamicProjectile" || item.data.id is "Arrow")
                            if (!item.holder && Vector3.Distance(item.transform.position, Player.currentCreature.ragdoll.headPart.transform.position) < 5 && item.lastHandler.creature != Player.currentCreature)
                                incitingItem = item;
                if (incitingItem && Vector3.Distance(incitingItem.transform.position, Player.currentCreature.ragdoll.headPart.transform.position) > 2)
                {
                    incitingItem = null;
                    GameManager.SetSlowMotion(true, 0.5f, Catalog.GetData<SpellPowerSlowTime>("SlowTime").exitCurve, null);
                    timer = Time.time;
                    reset = true;
                }
                if (incitingItem)
                    if (incitingItem.data.id is "DynamicProjectile" || incitingItem.data.id is "Arrow")
                        GameManager.SetSlowMotion(true, Vector3.Distance(incitingItem.transform.position, Player.currentCreature.ragdoll.headPart.transform.position) / Mathf.Min(StatManager.miscFocusLvl, 6), Catalog.GetData<SpellPowerSlowTime>("SlowTime").enterCurve, null);
                if (Time.time - timer > 0.5f && reset)
                {
                    GameManager.SetSlowMotion(false, 1, Catalog.GetData<SpellPowerSlowTime>("SlowTime").exitCurve, null);
                    reset = false;
                }
            }
        }
    }
    public class CombatRegeneration : MonoBehaviour
    {
        void Awake()
        {
            Debug.Log("Regeneration perk added.");
        }
        void Update()
        {
            if (Player.currentCreature.currentHealth < Player.currentCreature.maxHealth)
            {
                Player.currentCreature.currentHealth += (StatManager.combatHealthLvl * 0.01f) * Time.deltaTime;
            }
        }
    }
}