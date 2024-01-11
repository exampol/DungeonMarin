namespace BlazorD.Client.Models {
    class LootGeneration {
        private int level = 0;

        public enum DropType { Equipment = 0, Item = 1, Effect = 2, Exp = 3 }

        private DropType type;
        public DropType Type { get { return type; } }

        private int expAmount = 0;
        public int ExpAmount { get { return expAmount; } }

        private Item generatedItem = null;
        public Item GeneratedItem { get { return generatedItem; } }

        private Effect generatedEffect;
        public Effect GeneratedEffect { get { return generatedEffect; } }

        private Equipment generatedEquip;
        public Equipment GeneratedEquip { get { return generatedEquip; } }

        public LootGeneration(int level) { this.level = level; }

        public void Generate() {
            int roll = Global.rnd.Next(0, 100);
            if (roll + 1 >= 1 && roll + 1 <= 40) { type = DropType.Exp; expAmount = Global.rnd.Next(0, 300); } else if (roll + 1 <= 65) {
                type = DropType.Effect;
                generatedEffect = Effect.GetEffect(level);
            } else if (roll + 1 <= 85) {
                type = DropType.Item;
                generatedItem = Item.GetItem();
            } else if (roll + 1 <= 100) {
                type = DropType.Equipment;
                generatedEquip = Equipment.GetEquipment(level);
            }
        }
    }

    public class Item {
        public enum ItemType { Smoke = 0, ArtShield = 1, MagicArrow = 2, InstHealPotion = 3, HealPotion = 4, Stimulants = 5 }
        private ItemType type;
        public ItemType Type { get { return type; } }

        private static int IDGenerator = 0;
        private int id = 0;
        public int ID { get { return id; } }

        public bool UsableOutOfCombat => usableOutOfCombat;
        private bool usableOutOfCombat = true;
        public bool UsableInCombat => usableInCombat;
        private bool usableInCombat = false;

        public string Name {
            get {
                switch (type) {
                    case ItemType.Smoke:
                        return "Granata fumogena";
                    case ItemType.ArtShield:
                        return "Artefatto scudo";
                    case ItemType.MagicArrow:
                        return "Freccia magica";
                    case ItemType.InstHealPotion:
                        return "Pozione di cura (istantanea)";
                    case ItemType.HealPotion:
                        return "Pozione di cura";
                    case ItemType.Stimulants:
                        return "Stimolanti";
                    default: return "";
                }
            }
        }

        protected int duration = 0;
        protected int strength = 0;

        public int Duration { get { return duration; } }
        public int Strength { get { return strength; } }

        public Item(ItemType type) { this.type = type; 
            IDGenerator++; 
            id = IDGenerator;
            int roll = 0;
            if (Type == ItemType.Stimulants) {
                duration = 10;
                strength = 3;
            } else if (Type == Item.ItemType.HealPotion) {
                roll = Global.rnd.Next(4, 10);
                duration = roll;
                strength = 100 / roll;
            } else if (Type == Item.ItemType.Smoke) duration = 4;
        }

        public static Item GetItem() {
            int roll = Global.rnd.Next(0, 100);
            int minRange = 3;
            int maxRange = 6;
            if (roll >= 60) { minRange = 0; maxRange = 3; }

            roll = Global.rnd.Next(minRange, maxRange);
            Item item = new Item((ItemType)roll);

            item.usableOutOfCombat = item.Type != ItemType.ArtShield && item.Type != ItemType.MagicArrow;
            item.usableInCombat = item.Type != ItemType.Smoke;
            return item;
        }
    }

    public class Equipment {
        public enum EquipmentType { ChestPlate = 0, Leggings = 1, Helmet = 2, Shoes = 3, Gloves = 4, Ring = 5, Necklace = 6, Mantle = 7 }
        private EquipmentType type;
        public EquipmentType Type => type;

        protected int equipAgi = 0;
        protected int equipDef = 0;
        protected int equipAcc = 0;
        protected int equipStr = 0;
        protected int equipWis = 0;
        protected int equipVit = 0;

        public int EquipAgi => equipAgi;
        public int EquipDef => equipDef;
        public int EquipAcc => equipAcc;
        public int EquipStr => equipStr;
        public int EquipWis => equipWis;
        public int EquipVit => equipVit;

        protected string name = "";
        public string Name => name;

        public Equipment(EquipmentType type) { this.type = type; }
        public static Equipment GetEquipment(int level) {
            int roll = Global.rnd.Next(0, 8);
            Equipment eq = new Equipment((EquipmentType)roll);

            int attr = 0;
            if (eq.type == EquipmentType.ChestPlate) attr = (int)Math.Pow(level, 2) * 3;
            else if (eq.Type != EquipmentType.Ring && eq.Type != EquipmentType.Necklace && eq.Type != EquipmentType.Mantle) attr = (int)Math.Pow(level, 2) * 2;
            else attr = (int)Math.Pow(level, 2);

            switch (eq.Type) {
                case EquipmentType.ChestPlate:
                    eq.name = "Corazza";
                    break;
                case EquipmentType.Leggings:
                    eq.name = "Pantaloni";
                    break;
                case EquipmentType.Helmet:
                    eq.name = "Elmo";
                    break;
                case EquipmentType.Shoes:
                    eq.name = "Scarpe";
                    break;
                case EquipmentType.Gloves:
                    eq.name = "Guanti";
                    break;
                case EquipmentType.Ring:
                    eq.name = "Anello";
                    break;
                case EquipmentType.Necklace:
                    eq.name = "Collana";
                    break;
                case EquipmentType.Mantle:
                    eq.name = "Mantello";
                    break;
            }

            roll = Global.rnd.Next(0, 6);
            if (roll == 0) { eq.equipAgi = attr; eq.name += " dell'agilità"; } else if (roll == 1) { eq.equipDef = attr; eq.name += " di acciaio"; } else if (roll == 2) { eq.equipAcc = attr; eq.name += " da arcere"; } else if (roll == 3) { eq.equipStr = attr; eq.name += " da combattimento"; } else if (roll == 4) { eq.equipWis = attr; eq.name += " del Saggio"; } else if (roll == 5) { eq.equipVit = attr; eq.name += " del Druido"; }

            if (level - 1 > 0) eq.name += $"(+{level})";
            return eq;
        }
    }

    public class Effect {
        public enum EffectType {
            Heal = 0,
            InstantHeal = 1,
            Stealth = 2,
            Agility = 3,
            Accuracy = 4,
            Strength = 5,
            Defence = 6,
            Vitality = 7,
            Perception = 8,
            CurseAgility = 9,
            CurseAccuracy = 10,
            CurseStrength = 11,
            CurseDefence = 12,
            CurseVitality = 13,
            CursePerception = 14,
            Speed = 15
        }
        public int RemainingDuration = 0;

        private int strength = 0;
        public int Strength { get { return strength; } }

        private EffectType type;
        public EffectType Type { get { return type; } }

        public string Name {
            get {
                switch (Type) {
                    case EffectType.Heal: return "Cura";
                    case EffectType.InstantHeal: return "Cura istantanea del 100%";
                    case EffectType.Stealth: return "Invisibilità";
                    case EffectType.Agility: return "Potenziamento agilità";
                    case EffectType.Accuracy: return "Potenziamento precisione";
                    case EffectType.Strength: return "Potenziamento forza";
                    case EffectType.Defence: return "Potenziamento difesa";
                    case EffectType.Vitality: return "Potenziamento vitalità";
                    case EffectType.Perception: return "Potenziamento percezione";
                    case EffectType.CurseAgility: return "Maledizione agilità";
                    case EffectType.CurseAccuracy: return "Maledizione precisione";
                    case EffectType.CurseStrength: return "Maledizione forza";
                    case EffectType.CurseDefence: return "Maledizione difesa";
                    case EffectType.CurseVitality: return "Maledizione vitalità";
                    case EffectType.CursePerception: return "Maledizione percezione";
                    case EffectType.Speed: return "Velocità";
                    default: return "Effetto sconosciuto: " + ((int)Type).ToString();
                }
            }
        }

        public Effect(EffectType t, int strength, int remainingDuration) {
            type = t;
            this.strength = strength;
            this.RemainingDuration = remainingDuration;
        }

        public static Effect GetEffect(int level) {
            List<int> avEffects = new List<int>();
            int iEffect = -1;
            if (level <= 1) {
                do { iEffect = Global.rnd.Next(0, 16); } while (iEffect == 1 || iEffect == 2 || iEffect == 15);
            } else if (level <= 2) {
                do { iEffect = Global.rnd.Next(0, 16); } while (iEffect == 1 || iEffect == 2);
            } else if (level <= 3) {
                iEffect = Global.rnd.Next(0, 16);
            }

            EffectType t = (EffectType)iEffect;
            int str = 0;
            int remDur = 0;
            if (t == EffectType.InstantHeal) return new Effect(EffectType.InstantHeal, 0, 0);
            if (t == EffectType.Heal) {
                str = Global.rnd.Next(7, 21) * level;
            } else if (t == EffectType.Stealth) {
                remDur = Global.rnd.Next(level - 1, level + 2);
            } else if (t == EffectType.Speed) {
                remDur = Global.rnd.Next(level, level + 3);
            } else if (iEffect >= 3 && iEffect <= 8) {
                str = Global.rnd.Next(1, 5) * level;
                remDur = Global.rnd.Next(5, 10);
            } else if (iEffect > 8) {
                str = Global.rnd.Next(1, 3) * level;
                remDur = Global.rnd.Next(5, 10);
            }

            return new Effect(t, str, remDur);
        }
    }
}
