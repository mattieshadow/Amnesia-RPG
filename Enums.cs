namespace ARPG
{
    public class Enums
    { 
        /*************
           *  Elements  *
           *************/
        public enum ElementType
        {
            Fire,
            Lightning,
            Water,
            Air,
            Ice
        }

        /************
           *  Factions  *
           ************/
        public class Factions
        {
            public enum FactionType
            {
                Iceborn,
                FarShore,
                Flameborn
            }
            public enum Race
            {
                DracenKin,
                Nord,
                CaveGoblin,
                Dwarf,
                Goblin,
                ForestElf,
                Salisma,
                Formicans,
                Elium,
                Vampyre,
                Human,
                Orc,
                Ogre
            }
        }

        public class Faction
        {
            public Factions.FactionType FactionType;
            public Factions.Race FactionRace;
            public ElementType ElementSpecialization;
            public bool IsRouge = false;
        }
    }
}
