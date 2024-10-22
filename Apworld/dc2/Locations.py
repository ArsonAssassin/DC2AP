from enum import IntEnum
from typing import Optional, NamedTuple, Dict

from BaseClasses import Location, Region


class DC2LocationCategory(IntEnum):
    FLOOR = 0
    DUNGEON = 1
    RECRUIT = 2
    GEORAMA = 3
    MIRACLE_CHEST = 4
    BOSS = 5
    MISC = 6
    GEOSTONE = 7
    EVENT = 8
    SKIP = 9


class DC2LocationData(NamedTuple):
    name: str
    default_item: str
    category: DC2LocationCategory


class DarkCloud2Location(Location):
    game: str = "Dark Cloud 2"
    category: DC2LocationCategory
    default_item_name: str

    def __init__(
            self,
            player: int,
            name: str,
            category: DC2LocationCategory,
            default_item_name: str,
            address: Optional[int] = None,
            parent: Optional[Region] = None):
        super().__init__(player, name, address, parent)
        self.default_item_name = default_item_name
        self.category = category

    @staticmethod
    def get_name_to_id() -> dict:
        base_id = 694201000
        table_offset = 1000

        table_order = [
            "Palm Brinks",
            "Underground Water Channel",
            "Sindain",
            "Jurak Mall",
            "Rainbow Butterfly Wood",
            "Balance Valley",
            "Starlight Temple",
            "Starlight Canyon",
            "Veniccio",
            "Lunatic Wisdom Laboratories",
            "Ocean's Roar Cave",
            "Heimrada",
            "Gundorada Workshop",
            "Mount Gundor",
            "Moon Flower Palace"
        ]

        output = {}
        for i, region_name in enumerate(table_order):
            if len(location_tables[region_name]) > table_offset:
                raise Exception("A location table has {} entries, that is more than {} entries (table #{})".format(len(location_tables[region_name]), table_offset, i))

            output.update({location_data.name: id for id, location_data in enumerate(location_tables[region_name], base_id + (table_offset * i))})

        return output


location_tables = {
    "Palm Brinks": [
        DC2LocationData("PB: Miracle chest 1",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("PB: Miracle chest 2",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("PB: Miracle chest 3",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
    ],
    "Underground Water Channel": [
        DC2LocationData("UWC: Floor 1 - To the Outside World",      "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Floor 2 - Battle with Rats",          "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Floor 3 - Ghosts in the Channel",     "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Pump Room",                           "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Linda",                               "null",                             DC2LocationCategory.BOSS),
        DC2LocationData("UWC: Floor 4 - Steve's Battle",            "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Floor 5 - Ghost in the Channel",      "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Halloween",                           "null",                             DC2LocationCategory.BOSS),
        DC2LocationData("UWC: Chapter 1 Complete",                  "null",                             DC2LocationCategory.EVENT),
    ],
    "Sindain": [
        DC2LocationData("S: Grape Juice",                           "Grape Juice",                      DC2LocationCategory.MISC),
    ],
    "Jurak Mall": [
        DC2LocationData("JM: Miracle chest 1",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 2",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 3",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 4",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 5",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 6",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 7",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 8",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 9",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 10",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 11",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 12",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 13",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 14",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 15",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("JM: Miracle chest 16",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),

        DC2LocationData("JM: Lafrescia Seed",                        "Lafrescia Seed",                  DC2LocationCategory.MISC),
    ],
    "Rainbow Butterfly Wood": [    
        DC2LocationData("RBW: Floor 1 - Frightening Forest",        "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 2 - Strange Tree",              "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 3 - Rolling Shells",            "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Fish Monster Swamp",                  "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 4 - This is a Geostone?",       "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 5 - Noise in the Forest",       "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 6 - I'm a Pixie",               "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 7 - Legendary Killer Snake",    "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 8 - Grotesque Spider Lady",     "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Rainbow Butterfly",                   "null",                             DC2LocationCategory.BOSS),
        DC2LocationData("RBW: Chapter 2 Complete",                  "null",                             DC2LocationCategory.EVENT),

       # Star Floors
        DC2LocationData("RBW: Floor 9 - Looking for the Earth Gem", "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 10 - Something Rare Here!",      "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Floor 11 - Scary Tree",                "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("RBW: Trentos",                              "null",                            DC2LocationCategory.BOSS),

        DC2LocationData("RBW: Fishing Rod",                         "Fishing Rod",                      DC2LocationCategory.MISC),
        DC2LocationData("RBW: Earth Gem",                           "Earth Gem",                        DC2LocationCategory.MISC),
    ],
    "Balance Valley": [

    ],
    "Starlight Temple": [
        DC2LocationData("ST: Miracle chest 1",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 2",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),

        # Missable
        DC2LocationData("ST: Miracle chest 3",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 4",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 5",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 6",                      "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 7",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 8",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 9",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 10",                     "Emerald",                          DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 11",                     "Pearl",                            DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 12",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 13",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 14",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 15",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 16",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 17",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 18",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 19",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 20",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 21",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("ST: Miracle chest 22",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),

        DC2LocationData("ST: Starglass",                            "Starglass",                        DC2LocationCategory.MISC),
        DC2LocationData("ST: Miracle Dumplings",                    "Miracle Dumplings",                DC2LocationCategory.MISC),
    ],
    "Starlight Canyon": [
        DC2LocationData("SC: Floor 1 - Headlong Dash",               "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 2 - Fire and Ice Don't Mix",      "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 3 - Earth-Shaking Demon",         "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 4 - Powerful Yo-Yo Robot",        "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 5 - Elephant Army in the Valley", "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 6 - Dangerous Treasure Chest",    "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 7 - Little Dragon Counterattack", "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Barga's Valley",                        "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 8 - Warrior in Starlight Canyon", "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 9 - Smiling Fairy Village",       "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 10 - Cursed Mask",                "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 11 - We're the Roly-Poly Brothers", "null",                          DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 12 - Dragon Slayer",              "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Memo Eater",                            "null",                            DC2LocationCategory.BOSS),
        DC2LocationData("SC: Floor 13 - Rama Priests Like Cheese",   "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 14 - Nature's Threat",            "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 15 - Moon Baron",                 "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 16 - Lighthouse Appears",         "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Evil Flame and Gaspard",                "null",                            DC2LocationCategory.BOSS),
        DC2LocationData("SC: Chapter 3 Complete",                    "null",                            DC2LocationCategory.EVENT),

        # Star path floors
        DC2LocationData("SC: Floor 17 - Looking for the Wind Gem",   "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 18 - Evil Spirit in the Valley",  "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Floor 19 - Brave Warriors in the Valley", "null",                          DC2LocationCategory.FLOOR),
        DC2LocationData("SC: Lapis Garter",                           "null",                           DC2LocationCategory.BOSS),

        DC2LocationData("SC: White Windflower",                      "White Windflower",                DC2LocationCategory.MISC),
        DC2LocationData("SC: Wind Gem",                              "Wind Gem",                        DC2LocationCategory.MISC),
    ],
    "Veniccio": [

    ],
    "Lunatic Wisdom Laboratories": [
        DC2LocationData("LWL: Miracle chest 1",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 2",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 3",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 4",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 5",                      "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 6",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 7",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 8",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 9",                      "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 10",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 11",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 12",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 13",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 14",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 15",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 16",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 17",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 18",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 19",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 20",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 21",                     "Ruby",                            DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 22",                     "Peridot",                         DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("LWL: Miracle chest 23",                     "Sapphire",                        DC2LocationCategory.MIRACLE_CHEST),

        DC2LocationData("LWL: Aquarium",                            "Aquarium",                          DC2LocationCategory.MISC),
        DC2LocationData("LWL: Electric Worm",                       "Electric Worm",                     DC2LocationCategory.MISC),
        DC2LocationData("LWL: Shell Talkie",                        "Shell Talkie",                      DC2LocationCategory.MISC),
    ],
    "Ocean's Roar Cave": [
        DC2LocationData("ORC: Floor 1 - Pirates!",                  "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 2 - Tons of Fish",              "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 3 - Tank and Boss",             "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 4 - Water Monster",             "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 5 - Scary Auntie Medusa",       "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 6 - Sand Molers",               "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 7 - Bat Den",                   "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 8 - Pirates' Hideout",          "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Cave of Ancient Murals",              "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 9 - Wandering Zappy",           "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 10 - Banquet of the Dead",      "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 11 - Improvements",             "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 12 - Return of the Serpent",    "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 13 - Cursed Sea",               "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 14 - Sea of Atrocity",          "null",                              DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Dr. Jaming",                          "null",                              DC2LocationCategory.BOSS),
        DC2LocationData("ORC: Chapter 4 Complete",                  "null",                              DC2LocationCategory.EVENT),

        # Star path floors
        DC2LocationData("ORC: Floor 15 - Looking for the Water Gem", "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 16 - Pirates' Revenge",          "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Floor 17 - Death Ocean",               "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("ORC: Sea Dragon",                           "null",                             DC2LocationCategory.BOSS),

        DC2LocationData("ORC: Secret Dragon Remedy",                 "Secret Dragon Remedy",             DC2LocationCategory.MISC),
        DC2LocationData("ORC: Water Gem",                            "Water Gem",                        DC2LocationCategory.MISC),
    ],
    "Heim Rada": [

    ],
    "Gundorada Workshop": [
        DC2LocationData("GW: Miracle chest 1",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 2",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 3",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 4",                      "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 5",                      "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 6",                      "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 7",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 8",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 9",                      "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 10",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 11",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 12",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 13",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 14",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 15",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 16",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 17",                     "Potato Pie",                       DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 18",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 19",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 20",                     "Fruit of Eden",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 21",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 22",                     "Witch Parfait",                    DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 23",                     "Diamond",                          DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 24",                     "Turquoise",                        DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("GW: Miracle chest 25",                     "Topaz",                            DC2LocationCategory.MIRACLE_CHEST),

        DC2LocationData("GW: Time Bomb",                            "Time Bomb",                        DC2LocationCategory.MISC),
        DC2LocationData("GW: Fire Horn",                            "Fire Horn",                        DC2LocationCategory.MISC)
    ],
    "Mount Gundor": [
        DC2LocationData("MG: Floor 1 - Battle with Griffon's Army", "null",                             DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 2 - Mt. Gundor Wind",             "null",                            DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 3 - Little Dragons on the Mountain", "null",                         DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 4 - Steam Goyone",                 "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 5 - Mountain Baddie Appears",      "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 6 - Magmanoff",                    "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 7 - Danger Zone",                  "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 8 - Secret of Fire Mountain",      "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 9 - Deathtrap",                    "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 10 - Desperation on the Mountain", "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 11 - Pains in the Neck",           "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Mount Gundor Peak",                      "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 12 - Walking the Path of Flames",  "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 13 - Burning Undead",              "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 14 - Fire Dragon",                 "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 15 - Treasure Chest Danger Zone",  "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 16 - Road to the River of Flames", "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Gaspard",                                "null",                           DC2LocationCategory.BOSS),
        DC2LocationData("MG: Chapter 5 Complete",                     "null",                           DC2LocationCategory.EVENT),

        # Star path floors
        DC2LocationData("MG: Floor 17 - Looking for the Fire Gem",    "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 18 - Explosive Hot Spring",        "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Floor 19 - Crazy Mountain",              "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MG: Inferno",                                "null",                           DC2LocationCategory.BOSS),

        DC2LocationData("MG: Fire Gem",                               "Fire Gem",                       DC2LocationCategory.MISC),
    ],
    "Moon Flower Palace": [
        DC2LocationData("MFP: Miracle chest 1",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 2",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 3",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 4",                      "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 5",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 6",                      "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 7",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 8",                      "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 9",                      "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 10",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 11",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 12",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 13",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 14",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 15",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 16",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 17",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 18",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 19",                     "Fruit of Eden",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 20",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 21",                     "Potato Pie",                      DC2LocationCategory.MIRACLE_CHEST),
        DC2LocationData("MFP: Miracle chest 22",                     "Witch Parfait",                   DC2LocationCategory.MIRACLE_CHEST),

        DC2LocationData("MFP: Floor 1 - Ancient Wind",               "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 2 - Card Warriors Gather",       "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 3 - Dangerous Treasure",         "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 4 - Zombie Zone",                "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 5 - Feeling Out of Place",       "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 6 - Living Statue",              "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 7 - Danger Zone",                "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 8 - Scary Women",                "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 9 - Hell Elephant",              "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 10 - Crush the Undead",          "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Garden",                               "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 11 - Missing Gem Dealer",        "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 12 - Max's Longest Day",         "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 13 - Hell's Corridor",           "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 14 - Monica All Alone",          "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 15 - Raging Spirits",            "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 16 - Lonely Machine",            "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 17 - Nobility",                  "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 18 - Palace Watchdog",           "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 19 - Road to Memories",          "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Alexandra's Room",                     "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 20 - Final Trump Card",          "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 21 - Elemental Party",           "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 22 - Wandering Knight's Soul",   "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 23 - Beware Carelessness",       "null",                           DC2LocationCategory.FLOOR),
        DC2LocationData("MFP: Floor 24 - Final Battle",              "null",                           DC2LocationCategory.FLOOR)
    ],
    
}

location_dictionary: Dict[str, DC2LocationData] = {}
for location_table in location_tables.values():
    location_dictionary.update({location_data.name: location_data for location_data in location_table})
