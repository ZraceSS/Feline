using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Feline
{
    public class Program
    {
        public static Friday friday;

        public static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.Title = "Friday";

            friday = new Friday();
            friday.Awake();


            await Task.Delay(-1);
        }
    }
}
