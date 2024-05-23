using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.Models;
using Archipelago.PCSX2.Models;
using Archipelago.PCSX2.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archipelago.PCSX2
{
    public class ArchipelagoClient
    {
        internal bool IsConnected { get; set; }
        internal bool IsLoggedIn { get; set; }
        public event EventHandler<ItemReceivedEventArgs> ItemReceived;
        public ArchipelagoSession CurrentSession { get; set; }
        private List<Location> Locations { get; set; }
        private string GameName { get; set;}
        private Dictionary<string, object> _options;
        public Dictionary<string, object> Options { get { return _options; } }
        public async Task Connect(string host, string gameName)
        {
            CurrentSession = ArchipelagoSessionFactory.CreateSession(host);
            var roomInfo = await CurrentSession.ConnectAsync();
            Console.WriteLine($"Connected to room {roomInfo.SeedName}");
            Console.WriteLine($"Available Players: {JsonConvert.SerializeObject(roomInfo.Players)}");
            GameName = gameName;
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
            if(Locations == null || !Locations.Any())
            {
                Console.WriteLine("Please populate locations before calling Login");
                return;
            }
            var loginResult = await CurrentSession.LoginAsync(GameName, playerName, ItemsHandlingFlags.AllItems, Version.Parse("4.6.0"), password: password, requestSlotData: true);
            Console.WriteLine($"Login Result: {(loginResult.Successful ? "Success" : "Failed")}");
            if (loginResult.Successful)
            {
                Console.WriteLine($"Connected as Player: {playerName} playing {GameName}");
            }
            else
            {
                Console.WriteLine($"Login failed.");
                return;
            }
            var currentSlot = CurrentSession.ConnectionInfo.Slot;
            var slotData = await CurrentSession.DataStorage.GetSlotDataAsync(currentSlot);
            _options = JsonConvert.DeserializeObject<Dictionary<string, object>>(slotData["options"].ToString());

            MonitorLocations(Locations);

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
        public async void PopulateLocations(List<Location> locations)
        {
            Locations = locations;
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
