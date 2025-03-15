using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WreckGame.Managers
{
    public class InputManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        private MouseState _currentMouseState;
        private MouseState _previousMouseState;

        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
            _previousMouseState = _currentMouseState;
            _currentMouseState = Mouse.GetState();
        }

        public bool IsKeyPressed(Keys key) => _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        public bool IsKeyDown(Keys key) => _currentKeyboardState.IsKeyDown(key);
        public bool IsLeftMousePressed() => _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        public bool IsLeftMouseDown() => _currentMouseState.LeftButton == ButtonState.Pressed;
        public bool IsRightMousePressed() => _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released;
        public Point GetMousePosition() => _currentMouseState.Position;
        public int GetScrollWheelValue() => _currentMouseState.ScrollWheelValue;
        public int GetPreviousScrollWheelValue() => _previousMouseState.ScrollWheelValue;
        public MouseState GetCurrentMouseState() => _currentMouseState;
        public MouseState GetPreviousMouseState() => _previousMouseState;
    }
}