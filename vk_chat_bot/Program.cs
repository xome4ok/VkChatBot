using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;


namespace vk_chat_bot
{
    class Program
    {
        static void Main(string[] args)
        {
            ulong appID = ;                      // ID приложения
            string email = "";         // email или телефон
            string pass = "";               // пароль для авторизации

            var peerId = 4586435;

            var bot = new VkBot(appID, email, pass, peerId, botCallback);

            bot.RegisterRules(new List<BotRule>
            {
                //new BotRule(
                //    match: "",
                //    description: "",
                //    act: x => 
                //    ),

                new BotRule(
                    match: "/chartest",
                    description: "test characters",
                    act: x => bot.Say("ひらがな\nカタカナ\n漢字\nlatin\nкириллица\n123")
                    ),

                new BotRule(
                    match: "/echo",
                    description: "says back your words",
                    act: x => bot.Say("Echo command recieved: " + x.text)),

                new BotRule(
                    match: "/answer",
                    description: "recites your message",
                    act: x => bot.Say("What's up?", x.messageId)
                    ),

                #region help rule

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
