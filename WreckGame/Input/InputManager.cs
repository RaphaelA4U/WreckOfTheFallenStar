using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WreckGame.Input
{
    public class InputManager
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        
        public InputManager()
        {
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
        }
        
        public void Update()
        {
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();
        }
        
        public bool IsKeyPressed(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyUp(key);
        }
        
        public bool IsKeyDown(Keys key)
        {
            return _currentKeyboardState.IsKeyDown(key);
        }
        
        public bool IsKeyReleased(Keys key)
        {
            return _currentKeyboardState.IsKeyUp(key) && _previousKeyboardState.IsKeyDown(key);
        }
        
        public bool IsComboPressed(params Keys[] keys)
        {
            if (keys.Length == 0) return false;
            
            // Check if all keys are down
            bool allKeysDown = true;
            foreach (var key in keys)
            {
                if (!_currentKeyboardState.IsKeyDown(key))
                {
                    allKeysDown = false;
                    break;
                }
            }
            
            // Check if at least one key was just pressed
            bool oneKeyPressed = false;
            if (allKeysDown)
            {
                foreach (var key in keys)
                {
                    if (_previousKeyboardState.IsKeyUp(key))
                    {
                        oneKeyPressed = true;
                        break;
                    }
                }
            }
            
            return allKeysDown && oneKeyPressed;
        }
        
        public Vector2 GetMovementDirection()
        {
            Vector2 direction = Vector2.Zero;
            
            if (_currentKeyboardState.IsKeyDown(Keys.A)) direction.X -= 1;
            if (_currentKeyboardState.IsKeyDown(Keys.D)) direction.X += 1;
            if (_currentKeyboardState.IsKeyDown(Keys.W)) direction.Y -= 1;
            if (_currentKeyboardState.IsKeyDown(Keys.S)) direction.Y += 1;
            
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            
            return direction;
        }
    }
}