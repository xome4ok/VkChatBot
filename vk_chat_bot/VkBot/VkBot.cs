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
        private LongPollServer server;
        private VkApi vk;
        public int peerId; // peerId to listen. Currently one bot instance can listen to only one peer at the moment.
        private List<BotRule> rules = new List<BotRule>();

        // callback mechanism is deprecated, but can be used for debugging
        public delegate void IncomingMessageCallback(VkBot b, PollServerResponse r); 
        
        /// <summary>
        /// Rules, registered for this bot, pretty formatted
        /// </summary>
        public List<string> Rules
        {
            get { return rules.Select(rule => string.Join(" - ", rule.match, rule.description) ).ToList(); }
        }

        /// <summary>
        /// Constructor for a new bot.
        /// </summary>
        /// <param name="appId">application id provided by vk.com</param>
        /// <param name="email">vk.com login</param>
        /// <param name="pass">vk.com password</param>
        /// <param name="_peerId">Id of peer to listen. Look https://vk.com/dev/messages.send for detailed info on format.
        /// Currently one bot instance can listen to only one peer at the moment.</param>
        /// <param name="callback">Delegate. Executed every time bot recieves incoming message</param>
        public VkBot(ulong appId, string email, string pass, int _peerId, IncomingMessageCallback callback)
        {
            peerId = _peerId;

            Settings settings = Settings.Messages;

            vk = new VkApi();

            vk.Authorize(new ApiAuthParams
            {
                ApplicationId = appId,
                Login = email,
                Password = pass,
                Settings = settings,
            });

            var firstResponse = vk.Messages.GetLongPollServer();

            //Console.WriteLine("key: " + firstResponse.Key + "\nserver: " + firstResponse.Server + "\nts: " + firstResponse.Ts);
            Console.WriteLine("Ready.");

            server = new LongPollServer(firstResponse);

            server.StartPollingAsync(x => {

                //get new incoming messages, detailed info on format: https://vk.com/dev/using_longpoll
                var newMsgs = ((JArray)x["updates"])
                                .Where(t => (int)t[0] == 4 && (int)t[3] == peerId) // incoming message from chosen peer
                                .Select(c => c).ToList();

                //actually got new messages
                if (newMsgs.Count != 0)
                {
                    try
                    {
                        var psr = new PollServerResponse(x);
                        // apply rules one by one looking for one, which applies here
                        rules.ForEach( rule => rule.Apply(psr) );
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
        /// Sends plain text message
        /// </summary>
        public void Say(string text)
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = "<Bot>: " + text
            });
        }

        /// <summary>
        /// Sends text message, reciting other message
        /// </summary>
        /// <param name="text">plain text of message</param>
        /// <param name="messageWhichToAnswer">id of message to recite</param>
        public void Say(string text, long messageWhichToAnswer)
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = "<Bot>: " + text,
                ForwardMessages = new [] { messageWhichToAnswer }
            });
        }
    }
}
