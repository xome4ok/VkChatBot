using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace VkChatBot
{
    /// <summary>
    /// Simple interface wrapper to long poll server response
    /// </summary>
    public class PollServerResponse
    {
        /// <summary>
        /// Id of message
        /// </summary>
        public int messageId;
        /// <summary>
        /// Id of peer who sent the message
        /// </summary>
        public int peerId;
        /// <summary>
        /// time of recieving
        /// </summary>
        public int timestamp;
        /// <summary>
        /// subject of message. usually "..."
        /// </summary>
        public string subject;
        /// <summary>
        /// body of message
        /// </summary>
        public string text;

        /// <summary>
        /// Creates PollServerResponse object from parsed json object
        /// </summary>
        /// <param name="jo">parsed json object</param>
        public PollServerResponse(JObject jo)
        {
            try
            {
                messageId = (int)jo["updates"][0][1];
                peerId = (int)jo["updates"][0][3];
                timestamp = (int)jo["updates"][0][4];
                subject = (string)jo["updates"][0][5];
                // in order of recieving unicode, reencode text to unicode. Without it windows tends to use win1251
                byte[] bytes = Encoding.Default.GetBytes((string)jo["updates"][0][6]);
                text = Encoding.UTF8.GetString(bytes);
            }
            catch (Exception)
            {
                throw new FormatException("Can't convert server answer from JObject to PollServerResponse object");
            }

        }

        public override string ToString()
        {
            return string.Format("messageId: {0},\npeerId: {1},\ntimestamp: {2},\nsubject: {3},\ntext: {4}\n",
                                    messageId, peerId, timestamp, subject, text);
        }
    }
}
