using OpenTK.Input;

namespace ProdigalSoftware.TiVE.Core.Backend.OpenTKImpl
{
    internal class KeyboardImpl : IKeyboard
    {
        private KeyboardState currentState;

        public void Update()
        {
            currentState = Keyboard.GetState();
        }

        public bool IsKeyPressed(Keys key)
        {
            return currentState[(Key)(uint)key];
        }
    }
}
