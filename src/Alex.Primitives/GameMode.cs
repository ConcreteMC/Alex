namespace Alex.Interfaces
{
    public enum GameMode
    {
        /// <summary>
        ///     Players fight against the enviornment, mobs, and players
        ///     with limited resources.
        /// </summary>
        Survival = 0,
        S = 0,

        /// <summary>
        ///     Players are given unlimited resources, flying, and
        ///     invulnerability.
        /// </summary>
        Creative = 1,
        C = 1,

        /// <summary>
        ///     Similar to survival, with the exception that players may
        ///     not place or remove blocks.
        /// </summary>
        Adventure = 2,

        /// <summary>
        ///     Similar to creative, with the exception that players may
        ///     not place or remove blocks.
        /// </summary>
        Spectator = 3
    }
}