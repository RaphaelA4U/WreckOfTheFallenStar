using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.GameStates
{
    public class GameStateManager
    {
        private Dictionary<Type, GameState> _states;
        private GameState _currentState;
        
        public GameStateManager()
        {
            _states = new Dictionary<Type, GameState>();
        }
        
        public void RegisterState<T>(T state) where T : GameState
        {
            _states[typeof(T)] = state;
        }
        
        public void SwitchState<T>() where T : GameState
        {
            if (_currentState != null)
            {
                _currentState.OnStateExit();
            }
            
            if (_states.TryGetValue(typeof(T), out GameState newState))
            {
                _currentState = newState;
                _currentState.OnStateEnter();
            }
        }
        
        public void Update(GameTime gameTime)
        {
            _currentState?.Update(gameTime);
        }
        
        public void Draw(SpriteBatch spriteBatch)
        {
            _currentState?.Draw(spriteBatch);
        }

        public GameState GetCurrentState()
        {
            return _currentState;
        }
    }
}