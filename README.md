# DC2AP

Working on this in 2 parts. First is a class library to manage communication with Archipelago and PCSX2. Second is the actual Dark Cloud 2 implementation. The idea is that other PS2 games might be adapted using the class library.

# Setup
Reference the class library in your project, then use the following template to get started

```
PCSX2Client client = new PCSX2Client();
var pcsx2Connected = client.Connect();
 
Client = new ArchipelagoClient();
await Client.Connect("localhost:38281", "Dark Cloud 2");
var locations = Helpers.GetLocations();
Client.PopulateLocations(locations);
await Client.Login(playerName, password);
Client.ItemReceived += (e, args) =>
{
   //item received logic
};

...

Client.SendLocation(locationId);
```

The PCSX2Client doesn't do much at the moment beyond ensuring that PCSX2 is running and can be reached. I recommend that after connecting successfully, you read a known value in memory to check the game is loaded. For example in Dark Cloud 2 i check the Region with the following line

GameVersion = Memory.ReadInt(0x203694D0) == 1701667175 ? "PAL" : Memory.ReadInt(0x20364BD0) == 1701667175 ? "US" : "";

If the GameVersion is empty, i know that the game is not loaded.

The ArchipelagoClient is the main part of the class library you'll care about.

Create an instance of the ArchipelagoClient and call Connect(archipelagoHost, gameName).
Populate the locations before you Login to ensure the monitor gets set up correctly.
Login using the playerName and password set up in your AP instance.
Handle the ItemReceived event to add your logic for what should be done when an item is received from AP.
Call Client.SendLocation(locationId) when you want to manually trigger a location to be sent, otherwise this will be handled automatically by the location list you populated earlier.

## Locations
Locations is a list of a Location POCO which must match those in the APWorld. The format is as seen below

```
        public int Address { get; set; }
        public int AddressBit { get; set; }
        public string Name { get; set; }
        public int Id { get; set; }
        public LocationCheckType CheckType { get; set; }
        public string CheckValue { get; set; }
```
LocationCheckType lets you define what type of memory address value we listen for changes on. for example, a CheckType of Bit means we are looking for an individual bit to be set to 1. a CheckType of Int means we are looking at an Integer in memory and waiting for the value to match CheckValue. CheckValue is a string so that other types can be used in the future, but only Bit and Int are implemented currently.

## Options
If you have options defined in your APWorld that you want to use, these are exposed via a Dictionary<string, object> as ArchipelagoClient.Options

#Reading Memory
You can use the library to read memory addresses by calling Archipelago.PCSX2.Memory functions. For example:

int myValue = Memory.ReadInt(address);
string myString = Memory.ReadString(address, length)
