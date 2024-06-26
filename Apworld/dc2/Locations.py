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
        base_id = 100000
        table_offset = 100

        table_order = [
            "Palm Brinks",
            "Underground Water Channel",
            "Sindain",
            "Rainbow Butterfly Wood"
        ]

        output = {}
        for i, region_name in enumerate(table_order):
            if len(location_tables[region_name]) > table_offset:
                raise Exception("A location table has {} entries, that is more than {} entries (table #{})".format(len(location_tables[region_name]), table_offset, i))

            output.update({location_data.name: id for id, location_data in enumerate(location_tables[region_name], base_id + (table_offset * i))})

        return output


location_tables = {
    "Palm Brinks": [
        DC2LocationData("PB: Wrench",                               "Wrench",                           DC2LocationCategory.MISC),
        DC2LocationData("PB: Circus Ticket",                        "Circus Ticket",                    DC2LocationCategory.SKIP),
        DC2LocationData("PB: Battle Wrench",                        "Battle Wrench",                    DC2LocationCategory.MISC),
    ],
    "Underground Water Channel": [
        DC2LocationData("UWC: Floor 1",                             "Dungeon 0 Floor 0 Complete",       DC2LocationCategory.FLOOR),
        DC2LocationData("UWC: Chapter 1 Complete",                  "Chapter 1 Complete",               DC2LocationCategory.EVENT),
    ],
    "Sindain": [
        DC2LocationData("S: Grape Juice",                           "Grape Juice",                        DC2LocationCategory.MISC),
    ],
    "Rainbow Butterfly Wood": [    
        DC2LocationData("RBW: Chapter 2 Complete",                  "Chapter 2 Complete",               DC2LocationCategory.EVENT),
    ],
    
}

location_dictionary: Dict[str, DC2LocationData] = {}
for location_table in location_tables.values():
    location_dictionary.update({location_data.name: location_data for location_data in location_table})
