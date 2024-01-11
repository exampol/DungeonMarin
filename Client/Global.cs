using BlazorD.Client.Models;

namespace BlazorD.Client {
    public static class Global {
        public static Random rnd = new Random();

        public static List<Equipment> availableEquipment = new List<Equipment>();
        public static List<Equipment> equippedEquipment = new List<Equipment>();

        public static List<Item> availableItems = new List<Item>();
        public static List<Item> activeItems = new List<Item>();
    }
}
