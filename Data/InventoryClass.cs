using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feline.Data
{
    public class InventoryClass
    {
        public string name { get; set; }

        public int num { get; set; }

        public long owner { get; set; }

        public void reset(string Iname)
        {
            name = Iname;
            num = 0;
            owner = -1;
        }

        public void copy(InventoryClass n)
        {
            name = n.name;
            num = n.num;
            owner = n.owner;
        }

        public bool isEqual(InventoryClass n)
        {
            return name == n.name && num == n.num && owner == n.owner;
        }

        public void addInventory(int amount)
        {
            num += amount;
        }
    }
}
