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

            ulong appID = 5785736;                      // ID приложения
            string email = "xome4ok@yandex.kz";         // email или телефон
            string pass = "";               // пароль для авторизации

            var peerId = 4586435; // my own id
            //var peerId = 2000000000 + 139; // japanese chat id


            var bot = new VkBot(appID, email, pass, peerId, botCallback);

            bot.Say("Привет! Я снова онлайн!");

            bot.RegisterRules(new List<BotRule>
            {
                //new BotRule(
                //    match: "",
                //    description: "",
                //    act: x => 
                //    ),
                
                new BotRule(
                    match: "/warodai",
                    description: "look something up in warodai dictionary. Usage: /warodai %query%",
                    act: x =>
                    {
                        try {
                            var q = x.text.Split(new char[] {' ', '\u3000'})[1];
                            var res = wa.LookupSmart(q, 5);
                            bot.Say(string.Join("\n",res.Select(r => r.ToString())));
                            }
                        catch(IndexOutOfRangeException)
                        {
                            bot.Say("Wrong syntax. Should be: /warodai %query%");
                        }
                    }
                    ),

                #region /jedict rule COMMENTED
                //new BotRule(
                //    match: "/jedict",
                //    description: "japanese-english dictionary lookup",
                //    act: x =>
                //    {
                //        try
                //        {
                //            byte[] bytes = Encoding.Default.GetBytes(x.text);
                //            var incomingString = Encoding.UTF8.GetString(bytes);
                //            var q = incomingString.Split(' ')[1];
                //            var match = dictEntries.Find(w => w.Kanjis.Any(k => k.Text == q) || w.Readings.Any(r => r.Text == q));
                //            var rest = dictEntries.FindAll(w => w.Kanjis.Any(k => k.Text.Contains(q))
                //            || w.Readings.Any(r => r.Text.Contains(q)) 
                //            //|| w.Senses.Any(s => s.ToString().Contains(q)))
                //            ).Take(5).Select(e => e.ToString().Split(new string[] { " :: " }, StringSplitOptions.None)[1]).ToList();
                //            if (match != null)
                //            {
                //                var detailedMatch = string.Join(" | ", match.Kanjis.ToList().Select(k => k.Text))
                //                + " | " + string.Join(" | ",match.Readings.ToList().Select(r => r.Text))
                //                + " | " + string.Join("; ", match.Senses.ToList().Select(s => s.ToString().Substring(9)));

                //                rest.Add(detailedMatch);
                //                rest.Reverse();
                //            }

                //            var reply = string.Join("\n", rest); 
                //            bot.Say(reply == "" ? "Not found" : reply);
                //        }
                //        catch (IndexOutOfRangeException)
                //        {
                //            bot.Say("Wrong syntax. Should be: /dict %query%");
                //        }
                //    }
                //    ),
                #endregion

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
                    act: x => bot.Say("ひらがな\nカタカナ\n漢字\nlatin\nкириллица\n123")
                    ),

                #endregion

                #region /echo rule
                new BotRule(
                    match: "/echo",
                    description: "says back your words",
                    act: x => bot.Say("Echo command recieved: " + x.text)),

#endregion

                #region /answer rule
                new BotRule(
                    match: "/answer",
                    description: "recites your message",
                    act: x => bot.Say("What's up?", x.messageId)
                    ),
#endregion
                
                #region /help rule
                new BotRule(
                    match: "/help",
                    act: x => bot.Say("Available commands:\n" + string.Join("\n",bot.Rules.Where(r => !r.Contains(x.text))))
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
