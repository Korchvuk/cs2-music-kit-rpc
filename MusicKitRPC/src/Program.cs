using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DiscordRPC;
using DiscordRPC.Logging;

namespace CS2MusicKitRPC
{
    public class AppConfig
    {
        public string DiscordClientId { get; set; } = "123456789012345678"; 
        public int MusicKitId { get; set; } = 82;
    }

    public class MusicKitScanner
    {
        private static DiscordRpcClient client;
        private static AppConfig config;

        private static readonly Dictionary<int, string> Kits = new Dictionary<int, string>
        {
            { 0, "Default Kit" }, { 3, "Kelly Bailey - Valve" }, { 4, "Daniel Sadowski - Total Domination" },
            { 5, "Noisia - Sharpened" }, { 6, "Robert Allaire - Insurgency" }, { 7, "Sean Murray - A*D*8" },
            { 8, "Feed Me - High Noon" }, { 9, "DRE - Death's Approach" }, { 10, "Austin Wintory - Desert Fire" },
            { 11, "Sasha - LNOE" }, { 12, "Skog - Metal" }, { 13, "Midnight Riders - All I Want for Christmas" },
            { 14, "Beartooth - Disgusting" }, { 15, "Daniel Sadowski - Crimson Assault" }, { 16, "Mord Fustang - Diamonds" },
            { 17, "Michael Bross - Invasion!" }, { 18, "Ian Hultquist - Lion's Mouth" }, { 19, "New World Orchestra - Uber Blasto Phone" },
            { 20, "Sasha - Prism" }, { 21, "Skog - II-Headshot" }, { 22, "Ki:Theory - MOLOTV" },
            { 23, "Lennie Moore - Java Havana Funkwhale" }, { 24, "Proxy - Cl0ze" }, { 25, "Troels Folmann - For No Mankind" },
            { 26, "Austin Wintory - Bach RAM" }, { 27, "Damjan Mravunac - The Talos Principle" }, { 28, "Daniel Sadowski - The 8-Bit Kit" },
            { 29, "Darren Korb - Hades" }, { 30, "Amon Tobin - All for Dust" }, { 31, "Chris Meer - Hazardous Environments" },
            { 32, "Mateo Messina - For Eternal" }, { 33, "Scarlxrd - Chain$aw" }, { 34, "Life0ne - Under Sea" },
            { 35, "Half-Life: Alyx" }, { 36, "Austin Wintory - Mocha Petal" }, { 37, "Denzel Curry - Ultimate" },
            { 38, "The Verkkars - EZ4ENCE" }, { 39, "Team Spirit - Work Hard, Play Hard" }, { 40, "Neck Deep - Life's Not Out To Get You" },
            { 41, "Scarlxrd - King, Scar" }, { 42, "bbno$ - u mad!" }, { 43, "Darren Korb - Hades" },
            { 44, "The Living Tombstone - My Everything" }, { 45, "Amon Tobin - All for Dust" }, { 46, "3DNELU - Mocha Petal" },
            { 47, "Knock2 - dashstar*" }, { 48, "Sullivan King - Lock It Up" }, { 49, "Amon Tobin - All for Dust" },
            { 50, "Human Fall Flat" }, { 51, "Gojira - Mea Culpa" }, { 52, "Dreams & Nightmares" },
            { 53, "Denzel Curry - Walkin" }, { 54, "Ultimate Music Kit" }, { 55, "Hades II Music Kit" },
            { 56, "3DNELU - On The Loose" }, { 57, "The Verkkars & n0thing - Flashbang Dance" }, { 58, "Dr. Disrespect - Gillette" },
            { 59, "Hundredth - FREE" }, { 60, "Hyper Potions - M0u53" }, { 61, "Amon Tobin - All for Dust" },
            { 62, "Darren Korb - Bastion" }, { 63, "Kelly Bailey - Valve (Legacy)" }, { 64, "Austin Wintory - Desert Fire (StatTrak)" },
            { 65, "The Verkkars - Kolmesta" }, { 66, "Scurros - Aggressive Action" }, { 67, "Skelly - Dashstar (V2)" },
            { 68, "Mord Fustang - Diamonds (StatTrak)" }, { 69, "Sean Murray - A*D*8 (StatTrak)" }, { 70, "Damjan Mravunac - Serious Sam" },
            { 71, "Daniel Sadowski - Crimson Assault (StatTrak)" }, { 72, "Noisia - Sharpened (StatTrak)" }, { 73, "Robert Allaire - Insurgency (StatTrak)" },
            { 74, "Beartooth - Aggressive" }, { 75, "Hundredth - RARE" }, { 76, "Neck Deep - The Peace and The Panic" },
            { 77, "Roam - Backbone" }, { 78, "Twin Atlantic - GLA" }, { 79, "Frank Klepacki - I Am The One" },
            { 80, "Inon Zur - Fallout" }, { 81, "Lena Raine - Celeste" }, { 82, "Various Artists - Hotline Miami" },
            { 83, "Tree Adams - Halo Master Chief" }, { 84, "Austin Wintory - Antlion" }, { 85, "Michiel van den Bos - Unreal Tournament" }
        };

        static void Main()
        {
            Console.Title = "CS2 Music Kit RPC by Korchvuk";

       
            config = LoadConfig();

          
            if (string.IsNullOrEmpty(config.DiscordClientId) || config.DiscordClientId == "YOUR_APP_ID_HERE")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Укажите DiscordClientId в файле config.json!");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            client = new DiscordRpcClient(config.DiscordClientId);
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            client.OnReady += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"RPC Пользователь: {e.User.Username}");
                Console.ResetColor();

            
                UpdateRPC(false, "В меню");
            };

            client.Initialize();

            
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:3000/");

            try
            {
                listener.Start();
                Console.WriteLine("CS2 порт 3000...");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Не удалось запустить сервер. Запустите от имени Администратора! Ошибка: {ex.Message}");
                return;
            }

          
            while (true)
            {
                try
                {
                    var context = listener.GetContext();
                    var request = context.Request;

                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        string json = reader.ReadToEnd();

                    
                        context.Response.StatusCode = 200;
                        context.Response.Close();

                      
                        ProcessGameState(json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка чтения: {ex.Message}");
                }
            }
        }

        private static void ProcessGameState(string json)
        {
            try
            {
                var data = JObject.Parse(json);

              
                string mapName = data["map"]?["name"]?.ToString();
                string phase = data["map"]?["phase"]?.ToString(); 

                bool inGame = !string.IsNullOrEmpty(mapName) && phase != null;

      
                string team = data["player"]?["team"]?.ToString(); 
                int ctScore = data["map"]?["team_ct"]?["score"]?.Value<int>() ?? 0;
                int tScore = data["map"]?["team_t"]?["score"]?.Value<int>() ?? 0;
                string scoreText = inGame ? $"Score: CT {ctScore} - {tScore} T" : "In Menu";

                UpdateRPC(inGame, scoreText);
            }
            catch
            {
              
            }
        }

        private static void UpdateRPC(bool inGame, string stateDetails)
        {
            int kitId = config.MusicKitId;
            Kits.TryGetValue(kitId, out string kitName);
            string kitDisplayName = kitName ?? "Unknown Kit";


            var presence = new RichPresence()
            {
                Details = inGame ? "Playing Match" : "In Main Menu",
                State = stateDetails,
                Assets = new Assets()
                {
                    LargeImageKey = "cs2",
                    LargeImageText = "Counter-Strike 2",
                    SmallImageKey = kitId.ToString(),
                    SmallImageText = kitDisplayName
                }
            };

            presence.Buttons = new Button[]
            {
                new Button() { Label = "Get This RPC", Url = "https://github.com/Korchvuk/cs2-music-kit-rpc" }
            };

            client.SetPresence(presence);
        }

        private static AppConfig LoadConfig()
        {
            string path = "config.json";
            if (!File.Exists(path))
            {
                var cfg = new AppConfig();
                File.WriteAllText(path, JsonConvert.SerializeObject(cfg, Formatting.Indented));
                Console.WriteLine("Создан файл config.json.");
                return cfg;
            }

            try
            {
                return JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(path));
            }
            catch
            {
                Console.WriteLine("Ошибка чтения конфига! Создаю новый.");
                return new AppConfig();
            }
        }
    }
}
