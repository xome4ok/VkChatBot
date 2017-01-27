using System;

namespace vk_chat_bot
{
    public class BotRule
    {
        public string match { get; }
        public string description { get; }
        Action<PollServerResponse> act;

        public BotRule(string match, string description, Action<PollServerResponse> act)
        {
            this.description = description;
            this.match = match;
            this.act = act;
        }

        public BotRule(string match, Action<PollServerResponse> act)
        {
            this.match = match;
            this.act = act;
        }

        public void Apply(PollServerResponse r)
        {
            if (r.text == match)
            {
                Console.WriteLine("\nACTION > " + match + "\n");
                act(r);
            }
        }
    }
}
