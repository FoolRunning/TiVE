namespace ProdigalSoftware.TiVE
{
    /// <summary>
    /// Interface for getting data about the current keyboard state
    /// </summary>
    internal interface IKeyboard
    {
        /// <summary>
        /// Updates the state of the keyboard
        /// </summary>
        void Update();

        /// <summary>
        /// Gets whether the specified key is currently pressed
        /// </summary>
        bool IsKeyPressed(Keys key);
    }
}
