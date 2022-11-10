using Discord.WebSocket;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Feline.Data;
using Newtonsoft.Json;
using System.Threading;
using System.Runtime.CompilerServices;

namespace Feline
{
    public class Friday
    {
        private DiscordSocketClient _client;

        private const string token = "Your token here";

        public Friday()
        {

        }

        public async void Awake()
        {
            _client = new DiscordSocketClient();
            _client.MessageReceived += CommandHandler;
            _client.Log += Log;
            _client.Ready += Client_Ready;
            _client.SlashCommandExecuted += SlashCommandHandler;
            _client.UserJoined += JoinedUser;

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();
        }

        private Task Log(LogMessage arg)
        {
            Console.WriteLine(arg.ToString());
            return Task.CompletedTask;
        }

        private async Task JoinedUser(SocketGuildUser arg)
        {
            SocketTextChannel channel = arg.Guild.SystemChannel;
            await channel.SendMessageAsync($"Welcome {arg.Mention} to {channel.Guild.Name}");
        }

        private async Task Client_Ready()
        {
            var guild = _client.GetGuild(943136532347359242);
            List<SlashCommandBuilder> cm = new List<SlashCommandBuilder>();
            var dbt = new SlashCommandBuilder()
                .WithName("huntbounty")
                .WithDescription("ล่าสมบัติ ใช้ได้ทุกๆ 30 นาที");
            cm.Add(dbt);

            var inv = new SlashCommandBuilder()
                .WithName("inventory")
                .WithDescription("เปิดกระเป๋า");
            cm.Add(inv);

            var crf = new SlashCommandBuilder()
                .WithName("craft")
                .WithDescription("คราฟไอเทม ")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("ticket_self")
                    .WithDescription("คราฟ ตั๋วสำหรับใช้เอง : Ticket[Gift] x3")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("ticket_gift")
                    .WithDescription("คราฟ ตั๋วสำหรับให้คนอื่น : Platinum x25")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("platinum")
                    .WithDescription("คราฟ platinum : Gold x30")
                    .WithType(ApplicationCommandOptionType.SubCommand))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("gold")
                    .WithDescription("คราฟ gold : Silver x40")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                );
            cm.Add(crf);

            var gift = new SlashCommandBuilder()
                .WithName("use_ticket_gift")
                .WithDescription("ใช้ตั๋วให้คนอื่น")
                .AddOption("user", ApplicationCommandOptionType.User, "เลือกคนที่จะให้", isRequired: true);
            cm.Add(gift);

            var self = new SlashCommandBuilder()
                .WithName("use_ticket_self")
                .WithDescription("ใช้ตั๋วให้ตัวเอง");
            cm.Add(self);

            var adtool1 = new SlashCommandBuilder()
                .WithName("dctouser")
                .WithDescription("admin tool ja :P")
                .AddOption("user", ApplicationCommandOptionType.User, "dc", isRequired: true);
            cm.Add(adtool1);

            try
            {
                foreach (SlashCommandBuilder command in cm)
                {
                    await guild.CreateApplicationCommandAsync(command.Build());
                }
            }
            catch (Exception exception)
            {
                var json = JsonConvert.SerializeObject(exception, Formatting.Indented);

                Console.WriteLine(json);
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand arg)
        {
            //Console.WriteLine(JsonConvert.SerializeObject(arg, Formatting.Indented));

            SocketSlashCommandDataOption[] collection = arg.Data.Options.ToArray();
            var guildUser = arg.User;
            EmbedBuilder embedBuiler = createNewEmbed(guildUser);
            var guildSocket = _client.GetGuild(943136532347359242);
            User user = User.getUserData(guildUser);
            if (user == null)
            {
                embedBuiler.WithDescription("User Data Error : user = null.");
                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                return;
            }

            foreach (var member in guildSocket.Users)
            {
                if (guildUser.Id == member.Id)
                {
                    switch (arg.CommandName)
                    {
                        case "huntbounty":
                            if(DateTime.Now < user.LastHuntTime.AddMinutes(20))
                            {
                                var timeleft = (user.LastHuntTime.AddMinutes(20) - DateTime.Now).TotalMinutes;
                                embedBuiler.WithDescription("ติด Cooldown : " + ((int)timeleft).ToString() + " นาที");
                                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                break;
                            }

                            user.LastHuntTime = DateTime.Now;
                            string reward = huntBounty();
                            user.addItem(reward, 1);
                            user.saveUserData();

                            embedBuiler.WithDescription("ขุดสำเร็จ ! \nได้รับ : " + reward);
                            string filename = @"Data\Image\" + reward + ".png";
                            embedBuiler.WithImageUrl($"attachment://{filename}");
                            await arg.RespondWithFileAsync(filename, embed: embedBuiler.Build(), ephemeral: false);
                            break;
                        case "inventory":
                            embedBuiler.WithDescription("Inventory : \nSilver x" + user.inventory[0].num + "\nGold x" + user.inventory[1].num + "\nPlatinum x" + user.inventory[2].num + "\nTicket[Gift] x" + user.inventory[3].num + "\nTicket[Self] x" + user.inventory[4].num);
                            await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                            break;
                        case "craft":
                            switch(arg.Data.Options.First().Name)
                            {
                                case "ticket_self":
                                    if (user.inventory[3].num < 3)
                                    {
                                        embedBuiler.WithDescription("ไอเทมในกระเป๋าไม่เพียงพอ !\nต้องการ Ticket[Gift] x3");
                                        await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                        return;
                                    }
                                    user.inventory[3].num -= 3;
                                    user.addItem("TC_Self", 1);
                                    user.saveUserData();
                                    embedBuiler.WithDescription("คราฟสำเร็จ !\nได้รับ : Ticket[Self] x1");
                                    await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                    break;
                                case "ticket_gift":
                                    if (user.inventory[2].num < 25)
                                    {
                                        embedBuiler.WithDescription("ไอเทมในกระเป๋าไม่เพียงพอ !\nต้องการ Platinum x25");
                                        await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                        return;
                                    }
                                    user.inventory[2].num -= 25;
                                    user.addItem("TC_Give", 1);
                                    user.saveUserData();
                                    embedBuiler.WithDescription("คราฟสำเร็จ !\nได้รับ : Ticket[Gift] x1");
                                    await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                    break;
                                case "platinum":
                                    if (user.inventory[1].num < 30)
                                    {
                                        embedBuiler.WithDescription("ไอเทมในกระเป๋าไม่เพียงพอ !\nต้องการ Gold x30");
                                        await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                        return;
                                    }
                                    user.inventory[1].num -= 30;
                                    user.addItem("Platinum", 1);
                                    user.saveUserData();
                                    embedBuiler.WithDescription("คราฟสำเร็จ !\nได้รับ : Platinum x1");
                                    await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                    break;
                                case "gold":
                                    if (user.inventory[0].num < 40)
                                    {
                                        embedBuiler.WithDescription("ไอเทมในกระเป๋าไม่เพียงพอ !\nต้องการ Silver x40");
                                        await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                        return;
                                    }
                                    user.inventory[0].num -= 40;
                                    user.addItem("Gold", 1);
                                    user.saveUserData();
                                    embedBuiler.WithDescription("คราฟสำเร็จ !\nได้รับ : Gold x1");
                                    await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                    break;
                                default:
                                    break;
                            }
                            break;
                        case "use_ticket_gift":
                            if (collection[0].Value == guildUser)
                            {
                                embedBuiler.WithDescription("ใช้กับตัวเองไม่ได้นะตั๋วนี้");
                                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                return;
                            }
                            if (user.inventory[3].num == 0)
                            {
                                embedBuiler.WithDescription("ไม่มี Ticket[Gift] x1");
                                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                return;
                            }

                            user.inventory[3].num -= 1;
                            user.saveUserData();

                            User target = User.getUserData((SocketUser)collection[0].Value);
                            target.inventory[4].num += 1;
                            target.saveUserData();

                            embedBuiler.WithDescription("ใช้ไอเทมสำเร็จ ใช้ /inventory เพือตรวจสอบ");
                            await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                            break;
                        case "use_ticket_self":
                            if (user.inventory[4].num == 0)
                            {
                                embedBuiler.WithDescription("ไม่มี Ticket[Self] x1");
                                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                return;
                            }

                            break;
                        case "dctouser":
                            if(guildUser.Id != 526012903623491584)
                            {
                                embedBuiler.WithDescription("อย่าๆๆๆๆๆๆๆ");
                                await arg.RespondAsync(embed: embedBuiler.Build(), ephemeral: true);
                                return;
                            }
                            User tg = User.getUserData((SocketUser)collection[0].Value);
                            EmbedBuilder eb = createNewEmbed((SocketUser)collection[0].Value);
                            eb.WithDescription("Inventory : \nSilver x" + tg.inventory[0].num + "\nGold x" + tg.inventory[1].num + "\nPlatinum x" + tg.inventory[2].num + "\nTicket[Gift] x" + tg.inventory[3].num + "\nTicket[Self] x" + tg.inventory[4].num);
                            await arg.RespondAsync(embed: eb.Build(), ephemeral: true);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private Task CommandHandler(SocketMessage arg)
        {
            return Task.CompletedTask;
        }

        private EmbedBuilder createNewEmbed(SocketUser agr)
        {
            var embedBuiler = new EmbedBuilder();
            embedBuiler.WithAuthor(agr.ToString(), agr.GetAvatarUrl() ?? agr.GetDefaultAvatarUrl());
            embedBuiler.WithTitle("Zrace Bounty Hunt Game");
            embedBuiler.WithDescription("");
            embedBuiler.WithColor(Color.Green);
            embedBuiler.WithCurrentTimestamp();
            return embedBuiler;
        }

        public static string huntBounty()
        {
            int num = new Random(Guid.NewGuid().GetHashCode()).Next(0, 1000);
            string result = "Silver";
            if (num <= 600)
            {
                result = "Silver";
            }
            if (num > 600)
            {
                result = "Gold";
            }
            if (num > 875)
            {
                result = "Platinum";
            }
            if (num > 990)
            {
                result = "TC_Give";
            }
            if (num > 995)
            {
                result = "TC_Self";
            }
            return result;
        }
    }
}
