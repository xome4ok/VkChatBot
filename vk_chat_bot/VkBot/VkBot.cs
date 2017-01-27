using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model.RequestParams;

namespace vk_chat_bot
{
    public class VkBot
    {
        private LongPollServer server;
        private VkApi vk;
        public int peerId;
        private List<BotRule> rules = new List<BotRule>();

        public delegate void IncomingMessageCallback(VkBot b, PollServerResponse r);

        public List<string> Rules
        {
            get { return rules.Select(rule => string.Join(" - ", rule.match, rule.description) ).ToList(); }
        }

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
                var newMsgs = ((JArray)x["updates"]).Where(t => (int)t[0] == 4 && (int)t[3] == peerId).Select(c => c).ToList();

                if (newMsgs.Count != 0)
                {
                    try
                    {
                        var psr = new PollServerResponse(x);
                        rules.ForEach( rule => rule.Apply(psr) );
                        callback(this, psr);
                    }
                    catch (FormatException)
                    {
                        Console.WriteLine("Error! Couldn't parse server response to struct.\nServer said:\n" + x.ToString());
                    }
                }
            });
        }

        public void RegisterRules(IEnumerable<BotRule> rs)
        {
            rules.AddRange(rs);
        }

        public void Say(string s)
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = s
            });
        }

        public void Say(string s, long messageWhichToAnswer) // overload to answer to message
        {
            var id = vk.Messages.Send(new MessagesSendParams
            {
                PeerId = peerId,
                Message = s,
                ForwardMessages = new [] { messageWhichToAnswer }
            });
        }
    }
}
