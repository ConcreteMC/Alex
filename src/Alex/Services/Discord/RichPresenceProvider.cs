using System.Diagnostics.CodeAnalysis;
using DiscordRPC;

namespace Alex.Services.Discord
{

    public static class RichPresenceProvider
    {

        // ReSharper disable once MemberCanBePrivate.Global
        public const string DEFAULT_CLIENT_ID = "715598336014417982";

        private static string clientId = DEFAULT_CLIENT_ID;
        private static DiscordRpcClient client;

        public static void Initialize()
        {
            client = new DiscordRpcClient(clientId);
            client.Initialize();
            SetPresence(GenerateDefaultPresence());
        }

        /// <summary>
        /// Set the client Id of the discord client.
        /// </summary>
        /// <param name="clientIdArg">The client Id to set to.</param>
        /// <param name="initialize">Whether to initialize/reinitialize the provider.</param>
        public static void SetClientId(string clientIdArg, bool initialize = false)
        {
            clientId = clientIdArg;
            if (initialize) {
                Initialize();
            }
        }

        /// <summary>
        /// Wrapper over DiscordRpcClient.SetPresence()
        ///
        /// Set the Rich Presence.
        /// </summary>
        /// <param name="presence">The presence to set.</param>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void SetPresence(RichPresence presence)
        {
            client?.SetPresence(presence);
        }

        /// <summary>
        /// Wrapper over DiscordRpcClient.ClearPresence()
        ///
        /// Clear the Rich Presence.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public static void ClearPresence()
        {
            client?.ClearPresence();
        }

        /// <summary>
        /// Update the provider.
        ///
        /// PLEASE DON'T CALL THIS METHOD AS A PLUGIN. YOU'RE NOT SUPPOSED TO!
        /// THIS METHOD IS ONLY MEANT TO BE CALLED BY THE CORE, NOT BY PLUGINS!
        /// </summary>
        public static void Update()
        {
            client.Invoke();
        }

        private static RichPresence GenerateDefaultPresence()
        {
            return new RichPresence
            {
                Details = "Running version " + Alex.Version,
                State = "Hanging out",
                Timestamps = Timestamps.Now
            };
        }

    }

}