using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace VkChatBot
{
    /// <summary>
    /// Interface to vk.com LongPollServer.
    /// For detailed info look: https://vk.com/dev/using_longpoll
    /// </summary>
    public class LongPollServer
    {
        public ulong Ts;
        private string Key;
        private string Server;
        public JObject current_state;

        /// <summary>
        /// LongPollServer needs to be initiated by initial response, 
        /// obtained from vkApi.Messages.GetLongPollServer() call
        /// </summary>
        /// <param name="resp"></param>
        public LongPollServer(VkNet.Model.LongPollServerResponse resp)
        {
            this.Ts = resp.Ts;
            this.Key = resp.Key;
            this.Server = resp.Server;
        }

        public async void StartPollingAsync(Action<JObject> pollServerResponseCallback)
        {
            while (true)
            {
                var response = await this.Call();
                dynamic parsedResponse = JObject.Parse(response);
                this.current_state = parsedResponse;

                if (this.Ts != (ulong)parsedResponse.ts)
                    pollServerResponseCallback(current_state);

                this.Ts = parsedResponse.ts;
            }
        }

        public async Task<string> Call()
        {
            using (WebClient client = new WebClient())
            {
                var url = "https://" + Server + "?act=a_check&key=" + Key + "&ts=" + Ts + "&wait=25&version=1";
                var data = await client.DownloadStringTaskAsync(new Uri(url));
                return data;
            }
        }
    }
}
