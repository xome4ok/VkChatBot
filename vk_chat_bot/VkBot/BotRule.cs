using System;

namespace VkChatBot
{
    /// <summary>
    /// Rule for VkBot
    /// </summary>
    public class BotRule
    {
        /// <summary>
        /// Rule is fired if first word in message is match
        /// </summary>
        public string match { get; }

        /// <summary>
        /// Description to provide in help
        /// </summary>
        public string description { get; }

        /// <summary>
        /// Reaction to command
        /// </summary>
        Action<PollServerResponse> act;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match">Rule is fired if first word in message is match</param>
        /// <param name="description">Description to provide in help</param>
        /// <param name="act">Reaction to command</param>
        public BotRule(string match, string description, Action<PollServerResponse> act)
        {
            this.description = description;
            this.match = match;
            this.act = act;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match">Rule is fired if first word in message is match</param>
        /// <param name="act">Reaction to command</param>
        public BotRule(string match, Action<PollServerResponse> act)
        {
            this.match = match;
            this.act = act;
        }

        /// <summary>
        /// Tries to apply rule. If first word in the message matches @match, then action is executed
        /// </summary>
        /// <param name="r">response from poll server containing the message</param>
        public void Apply(PollServerResponse r)
        {
            // \u3000 is a Japanese space character
            if (r.text.Split(new char[] {' ', '\u3000'})[0] == match)
            {
                Console.WriteLine("\nACTION > " + match + "\n");

                act(r);
            }
        }
    }
}
