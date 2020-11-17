using System;
using Random = UnityEngine.Random;

namespace ARPG
{
    public class Utils
    {
        public Enums.Faction CreateNewFaction(float RogueProbability)
        {
            Enums.Faction NewFaction = new Enums.Faction();
            NewFaction.FactionType = PickRandomFactionType();
            NewFaction.FactionRace = PickRandomFactionRace();
            NewFaction.ElementSpecialization = PickRandomFactionElement();
            NewFaction.IsRouge = PickRogue(RogueProbability);
            return NewFaction;
        }

        #region Things
        private Enums.Factions.FactionType PickRandomFactionType()
        {
            return (Enums.Factions.FactionType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Enums.Factions.FactionType)).Length);
        }

        private Enums.Factions.Race PickRandomFactionRace()
        {
            return (Enums.Factions.Race)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Enums.Factions.Race)).Length);
        }

        private Enums.ElementType PickRandomFactionElement()
        {
            return (Enums.ElementType)UnityEngine.Random.Range(0, Enum.GetNames(typeof(Enums.ElementType)).Length);
        }

        private bool PickRogue(float Probability)
        {
            float y = Random.Range(0, 100);
            if (y <= Probability)
                return true;
            else
                return false;
        }
        #endregion
    }
}
