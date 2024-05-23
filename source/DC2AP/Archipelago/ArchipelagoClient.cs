using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using DC2AP.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DC2AP.Archipelago
{
    public class ArchipelagoClient
    {
        public bool IsConnected { get; set; }
        public bool IsLoggedIn { get; set; }
        public event EventHandler<ItemReceivedEventArgs> ItemReceived;
        public ArchipelagoSession CurrentSession { get; set; }
        public ArchipelagoOptions Options { get; set; }
        public List<Location> Locations { get; set; }
        public async Task Connect(string host)
        {
            CurrentSession = ArchipelagoSessionFactory.CreateSession(host);
            var roomInfo = await CurrentSession.ConnectAsync();
            Console.WriteLine($"Connected to room {roomInfo.SeedName}");
            Console.WriteLine($"Available Players: {JsonConvert.SerializeObject(roomInfo.Players)}");
            IsConnected = true;
        }
        public void Disconnect()
        {
            CurrentSession = null;
            IsConnected = false;
        }

        public async Task Login(string playerName, string password = null)
        {
            if (!IsConnected)
            {
                Console.WriteLine("Must be Connected before calling Login");
                return;
            }
            var loginResult = await CurrentSession.LoginAsync("Dark Cloud 2", playerName, ItemsHandlingFlags.AllItems, Version.Parse("4.6.0"), password: password, requestSlotData: true);
            Console.WriteLine($"Login Result: {(loginResult.Successful ? "Success" : "Failed")}");
            if (loginResult.Successful)
            {
                Console.WriteLine($"Connected as Player: {playerName} playing Dark Cloud 2");
            }
            var currentSlot = CurrentSession.ConnectionInfo.Slot;
            var slotData = await CurrentSession.DataStorage.GetSlotDataAsync(currentSlot);
            var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(slotData["options"].ToString());
          //  var locationIds = JsonConvert.DeserializeObject<List<int>>(slotData["locationsId"].ToString());
         //   var locationItems = JsonConvert.DeserializeObject<List<int>>(slotData["locationsItem"].ToString());
            Locations = Helpers.GetLocations();
            MonitorLocations(Locations);
            Options = new ArchipelagoOptions()
            {
                ExpMultiplier = (int)(long)options["abs_multiplier"],
                GoldMultiplier = (int)(long)options["gilda_multiplier"]
            };

            CurrentSession.Items.ItemReceived += (helper) =>
            {
                Console.WriteLine("Item received");
                var item = helper.PeekItem();
                var newItem = new Item() { Id = (int)item.Item, Quantity = 1 };
                ItemReceived?.Invoke(this, new ItemReceivedEventArgs() { Item = newItem });
                helper.DequeueItem();

            };
            IsLoggedIn = true;
            return;
        }
        private async void MonitorLocations(List<Location> locations)
        {
            foreach (var location in Locations)
            {
                if (location.CheckType == LocationCheckType.Bit)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await Helpers.MonitorAddressBit(location.Address, location.AddressBit);
                        SendLocation(location.Id);
                    });
                }
                else if(location.CheckType == LocationCheckType.Int)
                {
                    Task.Factory.StartNew(async () =>
                    {
                        await Helpers.MonitorAddress(location.Address, int.Parse(location.CheckValue));
                        SendLocation(location.Id);
                    });
                }
            }
        }
        public async void SendLocation(long id)
        {
            if (!(IsConnected && IsLoggedIn))
            {
                Console.WriteLine("Must be connected and logged in to send locations.");
                return;
            }
            await CurrentSession.Locations.CompleteLocationChecksAsync(new[] { id });
        }
    }
}
