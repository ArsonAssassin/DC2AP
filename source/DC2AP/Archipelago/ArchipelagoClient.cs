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
        public event EventHandler ItemReceived;
        public ArchipelagoSession CurrentSession { get; set; }
        public ArchipelagoOptions Options { get; set; }
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
            var loginResult = await CurrentSession.LoginAsync("Dark Cloud 2", playerName, ItemsHandlingFlags.AllItems, Version.Parse("4.6"), password: password, requestSlotData: true);
            Console.WriteLine($"Login Result: {(loginResult.Successful ? "Success" : "Failed")}");
            if (loginResult.Successful)
            {
                Console.WriteLine($"Connected as Player: {playerName} playing Dark Cloud 2");
            }
            var currentSlot = CurrentSession.ConnectionInfo.Slot;
            var slotData = await CurrentSession.DataStorage.GetSlotDataAsync( currentSlot );
            var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(slotData["options"].ToString());

            Options = new ArchipelagoOptions()
            {
                ExpMultiplier = (int)options["abs_multiplier"],
                GoldMultiplier = (int)options["gilda_multiplier"]
            };

            CurrentSession.Items.ItemReceived += (helper) =>
            {
                Console.WriteLine("Item received");
                ItemReceived?.Invoke(this, new EventArgs());
            };
            return;
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
