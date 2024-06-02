using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Archipelago.PCSX2.Util
{
    public static class Helpers
    {

        public static T Random<T>(this IEnumerable<T> list) where T : struct
        {
            return list.ToList()[new Random().Next(0, list.Count())];
        }
        public static string OpenEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string jsonFile = reader.ReadToEnd();
                return jsonFile;
            }
        }
        public static async Task MonitorAddress(int address)
        {
            var initialValue = Memory.ReadByte(address);
            var currentValue = initialValue;
            while (initialValue == currentValue)
            {
                currentValue = Memory.ReadByte(address);
                Thread.Sleep(10);
            }
            Console.WriteLine($"Memory value changed at address {address.ToString("X8")}");
        }
        public static async Task MonitorAddress(int address, int valueToCheck)
        {
            var currentValue = Memory.ReadByte(address);
            while (currentValue != valueToCheck)
            {
                currentValue = Memory.ReadByte(address);
                Thread.Sleep(10);
            }
        }
        public static async Task MonitorAddressBit(int address, int bit)
        {
            byte initialValue = Memory.ReadByte(address);
            byte currentValue = initialValue;
            bool initialBitValue = GetBitValue(initialValue, bit);
            bool currentBitValue = initialBitValue;

            while (!currentBitValue)
            {
                currentValue = Memory.ReadByte(address);
                currentBitValue = GetBitValue(currentValue, bit);
                Thread.Sleep(10);
            }

            Console.WriteLine($"Memory value changed at address {address.ToString("X8")}, bit {bit}");
        }
        private static bool GetBitValue(byte value, int bitIndex)
        {
            return (value & (1 << bitIndex)) != 0;
        }
 
    }
}
