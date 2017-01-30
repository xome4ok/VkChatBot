using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace VkChatBot
{
    /// <summary>
    /// Bot for vk.com chats
    /// </summary>
    public class VkBot
    {
        public bool answerAll = true; // when false, answers only to himself
        private int exclusivePeerId = 0; // if not 0, bot will listen exclusively to this peer
        private LongPollServer server;
        private VkApi vk;
        private List<BotRule> rules = new List<BotRule>();

        // callback mechanism is deprecated, but can be used for debugging
        public delegate void IncomingMessageCallback(VkBot b, PollServerResponse r);

        /// <summary>
        /// Constructor, which uses
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="key"></param>
        /// <param name="callback"></param>
        public VkBot(ulong appId, string token, IncomingMessageCallback callback, int exclusivePeerId=0)
        {
            ApiAuthParams apiParams = new ApiAuthParams
            {
                ApplicationId = appId,
                
                Settings = Settings.Messages
            };

            Initialize(token, appId, callback,exclusivePeerId);
        }

        /// <summary>
        /// Constructor. Inputs vk.com account credentials from console
        /// </summary>
        /// <param name="appId">application id provided by vk.com</param>
        /// <param name="callback">Delegate. Executed every time bot recieves incoming message</param>
        public VkBot(ulong appId, IncomingMessageCallback callback, int exclusivePeerId = 0)
        {
            Console.WriteLine("Logging in to vk.com");

            Console.Write("E-mail: ");
            string email = Console.ReadLine();

            string pass = "";
            #region inputing password
            Console.Write("Enter your password: ");
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass = pass.Substring(0, (pass.Length - 1));
                        Console.Write("\b \b");
                    }
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);
            Console.WriteLine();
            #endregion

            ApiAuthParams apiParams = new ApiAuthParams
            {
                ApplicationId = appId,
                Login = email,
                Password = pass,
                Settings = Settings.Messages
            };

            Initialize(apiParams, appId, callback, exclusivePeerId);

        }

        /// <summary>
        /// Simple constructor for a new bot.
        /// </summary>
        /// <param name="appId">application id provided by vk.com</param>
        /// <param name="email">vk.com login</param>
        /// <param name="pass">vk.com password</param>
        /// <param name="callback">Delegate. Executed every time bot recieves incoming message</param>
        public VkBot(ulong appId, string email, string pass, IncomingMessageCallback callback, int exclusivePeerId = 0)
        {
            ApiAuthParams apiParams = new ApiAuthParams
            {
                ApplicationId = appId,
                Login = email,
                Password = pass,
                Settings = Settings.Messages
            };

            Initialize(apiParams, appId, callback, exclusivePeerId);
        }

        private void Initialize(dynamic apiParams, ulong appId, IncomingMessageCallback callback, int exclusivePeerId)
        {
            vk = new VkApi();
            vk.Authorize(apiParams);

            var firstResponse = vk.Messages.GetLongPollServer();

            //Console.WriteLine("key: " + firstResponse.Key + "\nserver: " + firstResponse.Server + "\nts: " + firstResponse.Ts);
            Console.WriteLine("Ready.");

            server = new LongPollServer(firstResponse);

            if (exclusivePeerId != 0)
                this.exclusivePeerId = exclusivePeerId;

            server.StartPollingAsync(x =>
            {

                //get new messages, detailed info on format: https://vk.com/dev/using_longpoll
                var newMsgs = ((JArray)x["updates"])
                                .Where(t => (int)t[0] == 4
                                    && (answerAll ? // if it answers all
                                        (int)t[3] != vk.UserId : // then answer everybody except yourself
                                        (int)t[3] == vk.UserId // else answer only yourself
                                        )
                                    && (exclusivePeerId == 0 ?
                                        true : (int)t[3] == exclusivePeerId) //TODO: probably logic bug here. needs fix
                                )
                                .Select(c => c).ToList();

                //actually got new messages
                if (newMsgs.Count != 0)
                {
                    try
                    {
                        var psr = new PollServerResponse(x);
                        // apply rules one by one looking for one, which applies here
                        rules.ForEach(rule => rule.Apply(psr));
                        callback(this, psr);
                    }
                    catch (FormatException)
                    {
                        //sometimes it happens. just nevermind :)
                        Console.WriteLine("Known issue. Couldn't parse server response to struct.\n");
                    }
                }
            });
        }

        /// <summary>
        /// Register new rules for this bot. Doesn't change any ond rules
        /// </summary>
        /// <param name="newRules">new collection of rules</param>
        public void RegisterRules(IEnumerable<BotRule> newRules)
        {
            rules.AddRange(newRules);
        }

        /// <summary>
        /// Rules, registered for this bot, pretty formatted
        /// </summary>
        public List<string> Rules
        {
            get { return rules.Select(rule => string.Join(" - ", rule.match, rule.description)).ToList(); }
        }

        /// <summary>
        /// Sends plain text message to peer
        /// </summary>
        public void Say(string text, int peerId)
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = "<Bot>: " + text
            });
        }

        /// <summary>
        /// Sends text message to peer, reciting other message
        /// </summary>
        /// <param name="text">plain text of message</param>
        /// <param name="messageWhichToAnswer">id of message to recite</param>
        public void Say(string text, long messageWhichToAnswer, int peerId)
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = "<Bot>: " + text,
                ForwardMessages = new[] { messageWhichToAnswer }
            });
        }

        public VkApi GetApi()
        {
            return vk;
        }
    }
}
