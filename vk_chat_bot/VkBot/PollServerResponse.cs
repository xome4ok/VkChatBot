using Newtonsoft.Json.Linq;
using System;


namespace vk_chat_bot
{
    public class PollServerResponse
    {
        public int messageId;
        public int peerId;
        public int timestamp;
        public string subject;
        public string text;

        public PollServerResponse(JObject jo)
        {
            //Console.WriteLine(  jo.ToString() );
            try
            {
                messageId = (int)jo["updates"][0][1];
                peerId = (int)jo["updates"][0][3];
                timestamp = (int)jo["updates"][0][4];
                subject = (string)jo["updates"][0][5];
                text = (string)jo["updates"][0][6];
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
