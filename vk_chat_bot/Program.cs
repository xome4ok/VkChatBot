using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using WarodaiWrapper;
using VkChatBot;

namespace JapaneseChatBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var wa = new Warodai(".\\dict\\ewarodaiedict.txt");
            var edict2 = new Warodai(".\\dict\\edict2.txt");
            ulong appID = 0;
            int peerId = 0;
            string token = "";

            try
            {
                if (args.Length < 3)
                    throw new ArgumentException();

                if (args.Length == 3 && args[2] == "\\email")
                {
                    appID = ulong.Parse(args[0]);

                    peerId = int.Parse(args[1]); // my own id
                                                 //var peerId = 2000000000 + 139; // japanese chat id
                }
                else if (args.Length == 4 && args[2] == "\\token")
                {
                    appID = ulong.Parse(args[0]);

                    peerId = int.Parse(args[1]); // my own id
                                                 //var peerId = 2000000000 + 139; // japanese chat 
                    token = args[3];
                }
                else
                {
                    throw new ArgumentException();
                }


            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't parse command line arguments. Usage: vk_chat_bot.exe appId peerId \\email\n Usage: vk_chat_bot.exe appId peerId \\token token");
                Console.ReadKey();
                return;
            }

            var bot = token == "" ? new VkBot(appID, botCallback, peerId) : new VkBot(appID, token, botCallback, peerId);

            bot.Say("Привет! Словарный бот снова онлайн!", peerId);

            Console.WriteLine(string.Format("Successfully started with:\nappid: {0}\npeerid: {1}",appID,peerId));

            bot.RegisterRules(new List<BotRule>
            {
                //new BotRule(
                //    match: "",
                //    description: "",
                //    act: x => 
                //    ),
                
                new BotRule(
                    match: "/warodai",
                    description: "искать в словаре warodai. Формат: /warodai слово",
                    act: x =>
                    {
                        try {
                            var q = x.text.Split(new char[] {' ', '\u3000'})[1];
                            var res = wa.Lookup(q, 5);
                            var answer = string.Join("\n",res.Select(r => r.ToString()));
                            bot.Say(answer != "" ? answer : "Ничего не нашлось.",x.peerId);
                            }
                        catch(IndexOutOfRangeException)
                        {
                            bot.Say("Неверный синтаксис. Надо так: /warodai слово",x.peerId);
                        }
                    }
                    ),

                new BotRule(
                    match: "/edict",
                    description: "искать в словаре edict. Формат: /warodai слово",
                    act: x =>
                    {
                        try {
                            var q = x.text.Split(new char[] {' ', '\u3000'})[1];
                            var res = edict2.Lookup(q, 5);
                            var answer = string.Join("\n",res.Select(r => r.ToString()));
                            bot.Say(answer != "" ? answer : "Ничего не нашлось.", x.peerId);
                            }
                        catch(IndexOutOfRangeException)
                        {
                            bot.Say("Неверный синтаксис. Надо так: /edict слово", x.peerId);
                        }
                    }
                    ),
                
                #region WIP, commented. /gootran
                //new BotRule(
                //    match: "/gootran",
                //    description: "translate using google translator, use syntax: /gootran lang1 lang2 sentence",
                //    act: x =>
                //    {
                //        try
                //        {
                //            byte[] bytes = Encoding.Default.GetBytes(x.text);
                //            var incomingString = Encoding.UTF8.GetString(bytes);

                //            var splitted = incomingString.Split(' ');
                //            var sl = splitted[1];
                //            var tl = splitted[2];
                //            var sentence = string.Join(" ", splitted.Skip(3));
                //            var escapedSentence = Uri.EscapeUriString(sentence);
                //            Console.WriteLine(escapedSentence);

                //            var url = string.Format("https://translate.googleapis.com/translate_a/single?client=gtx&sl={0}&tl={1}&dt=t&q={2}",
                //                sl,tl,escapedSentence);


                //            WebClient client = new WebClient();
                //            //client.Encoding = Encoding.UTF8;
                //            var googleResponsebytes = client.DownloadData(url);
                //            var googleResponse = Encoding.UTF8.GetString(googleResponsebytes);
                //            Console.WriteLine(googleResponse);
                //            var normalizedResponse = googleResponse.Replace(",,",",\"\",").Replace(",,",",\"\",");
                //            Console.WriteLine(normalizedResponse);
                //            var strippedResponse = JObject.Parse("{\"r\":" + normalizedResponse + "}")["r"][0][0][0];
                //            bot.Say(strippedResponse.ToString());
                //        }
                //        catch (IndexOutOfRangeException)
                //        {
                //            bot.Say("I didn't understand you. Syntax is: /gootran lang1 lang2 sentence", x.messageId);
                //        }
                //    }
                //    ),
                #endregion

                #region /chartest rule
                new BotRule(
                    match: "/chartest",
                    description: "test characters",
                    act: x => bot.Say("ひらがな\nカタカナ\n漢字\nlatin\nкириллица\n123", peerId)
                    ),

                #endregion

                #region /echo rule COMMENTED
                //new BotRule(
                //    match: "/echo",
                //    description: "повторяет сказанное",
                //    act: x => bot.Say("Вот что : " + x.text)),

#endregion

                #region /answer rule COMMENTED
                //new BotRule(
                //    match: "/answer",
                //    description: "recites your message",
                //    act: x => bot.Say("What's up?", x.messageId)
                //    ),
#endregion
                
                #region /help rule
                new BotRule(
                    match: "/help",
                    act: x => bot.Say("Доступные команды:\n" + string.Join("\n",bot.Rules.Where(r => !r.Contains(x.text))), x.peerId)
                    ),
                #endregion

            });


            while (true)
                Thread.Sleep(Timeout.Infinite);
        }

        static void botCallback(VkBot bot, PollServerResponse r)
        {
            Console.WriteLine(r.ToString());
        }



    }
}
