﻿using Discord.WebSocket;
using Discord;
using System;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Diagnostics;
using System.Timers;

#pragma warning disable CS1998

namespace Discord_Bot
{
    public class Program
    {
        static void Main(string[] args)
        => new Program().StartAsync().GetAwaiter().GetResult();
        DiscordSocketClient _client;
        CommandHandler _handler;
        public string version = Utilities.GetAlert("VERSION");

        public async Task StartAsync()
        {
            string name = Environment.UserName; // Greets user in console
            string message = Utilities.GetFormattedAlert("WELCOME_&NAME_&VERSION", name, version);
            Console.WriteLine(message);
            if (Config.bot.token == "" || Config.bot.token == null && Config.bot.cmdPrefix == "" || Config.bot.cmdPrefix == null) //default values in config.json when first launched, first time setup essentially.
            {
                Console.WriteLine("Bot token not found. Get your bot's token from the Discord Developer portal and paste it here: ");
                string token = Console.ReadLine();
                Console.Write("Command prefix not found. What would you like it to be?" +
                    "\n(This is typically one symbol, such as \"!, #, $, %, etc.\": ");
                string prefix = Console.ReadLine();
                BotConfig bot = new BotConfig
                {
                    token = token,
                    cmdPrefix = prefix
                };
                string json = JsonConvert.SerializeObject(bot, Formatting.Indented);
                File.WriteAllText("Resources" + "/" + "config.json", json);
                Console.WriteLine("Confirmed. Restarting in 5 seconds...(If app doesn't restart, close and open again.)");
                var filePath = Assembly.GetExecutingAssembly().Location;
                await Task.Delay(5000);
                Process.Start(filePath);
                Environment.Exit(0);
            }
            try
            {
                _client = new DiscordSocketClient(new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Verbose,
                    MessageCacheSize = 250
                });
                _client.Log += Log;
                try
                {
                    await _client.LoginAsync(TokenType.Bot, Config.bot.token);
                }
                catch (System.Net.Http.HttpRequestException)
                {
                    Console.WriteLine("Error: Could not successfully connect. Do you have a stable internet connection?");
                    await Task.Delay(10000);
                    Console.WriteLine("Exiting...");
                    await Task.Delay(2000);
                    Environment.Exit(0);
                    return;
                }
                await _client.StartAsync();
                ResetTimer();
                Global.Client = _client;
                _handler = new CommandHandler();
                await _handler.InitializeAsync(_client);
                await Task.Delay(-1);
                
            }
            catch(Discord.Net.HttpException)
            {
                Console.WriteLine("You have an invalid bot token. Edit /Resources/config.json and supply the proper token.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        }

        private static void ResetTimer() //Causes application to restart every 6 hours.
        {
            Timer timer = new Timer();
            timer.Interval = 21600000;
            timer.Start();
            timer.Elapsed += Timer_Elapsed;
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Process.Start("Discord Bot.exe");
            Environment.Exit(0);
        }
    }
}
