using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Feline.Data
{
    public class User
    {
        public ulong Id { get; set; }

        public DateTime LastHuntTime { get; set; }

        public InventoryClass[] inventory { get; set; }

        public User()
        {
            Id = 0;
            LastHuntTime = DateTime.Now.AddMinutes(-30);
            resetInventory();
        }

        public void resetInventory()
        {
            inventory = new InventoryClass[5];
            inventory[0] = new InventoryClass()
            {
                name = "Silver",
                num = 0,
                owner = 0
            };
            inventory[1] = new InventoryClass()
            {
                name = "Gold",
                num = 0,
                owner = 0
            };
            inventory[2] = new InventoryClass()
            {
                name = "Platinum",
                num = 0,
                owner = 0
            };
            inventory[3] = new InventoryClass()
            {
                name = "TC_Give",
                num = 0,
                owner = 0
            };
            inventory[4] = new InventoryClass()
            {
                name = "TC_Self",
                num = 0,
                owner = 0
            };
        }

        public void addItem(string item, int num)
        {
            switch(item)
            {
                case "Silver":
                    inventory[0].addInventory(num);
                    break;
                case "Gold":
                    inventory[1].addInventory(num);
                    break;
                case "Platinum":
                    inventory[2].addInventory(num);
                    break;
                case "TC_Give":
                    inventory[3].addInventory(num);
                    break;
                case "TC_Self":
                    inventory[4].addInventory(num);
                    break;
                default:
                    inventory[0].addInventory(num);
                    break;
            }
        }

        public static User getUserData(SocketUser Id)
        {
            try
            {
                if(!File.Exists(@"Data\User\" + Id.Id.ToString() + ".json"))
                {
                    User newUser = new User()
                    {
                        Id = Id.Id,
                        LastHuntTime = DateTime.Now.AddMinutes(-30)
                    };
                    newUser.resetInventory();
                    string savedata = JsonConvert.SerializeObject(newUser, Formatting.Indented);
                    File.WriteAllText(@"Data\User\" + Id.Id.ToString() + ".json", savedata);
                }

                StreamReader strd = new StreamReader(@"Data\User\" + Id.Id.ToString() + ".json");
                string data = strd.ReadToEnd();
                strd.Close();
                if (data == string.Empty || data == "")
                {
                    return null;
                }
                User user = JsonConvert.DeserializeObject<User>(data);
                return user;
            }
            catch (Exception ex)
            {
                var json = JsonConvert.SerializeObject(ex, Formatting.Indented);
                Console.WriteLine(json);
                return null;
            }
        }

        public void saveUserData()
        {
            string savedata = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(@"Data\User\" + Id.ToString() + ".json", savedata);
        }
    }
}
