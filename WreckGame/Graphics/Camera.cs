using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WreckGame.Graphics
{
    public class Camera
    {
        private Vector2 _position;
        private Rectangle _viewportBounds;
        private const float FIXED_ZOOM = 1.0f;
        
        public Matrix TransformMatrix
        {
            get
            {
                // First translate to negative camera position
                // Then apply zoom scaling
                // Finally translate to center of viewport
                return Matrix.CreateTranslation(new Vector3(-_position.X, -_position.Y, 0)) *
                    Matrix.CreateScale(FIXED_ZOOM) *
                    Matrix.CreateTranslation(new Vector3(_viewportBounds.Width / 2, _viewportBounds.Height / 2, 0));
            }
        }
        
        public Camera(Viewport viewport)
        {
            _position = Vector2.Zero;
            _viewportBounds = viewport.Bounds;
        }
        
        public void UpdateViewport(Viewport viewport)
        {
            _viewportBounds = viewport.Bounds;
        }
        
        public void SetPosition(Vector2 position)
        {
            _position = position;
        }
        
        public void MoveCamera(Vector2 direction, float speed)
        {
            _position += direction * speed;
        }
        
        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(TransformMatrix));
        }
        
        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return Vector2.Transform(worldPosition, TransformMatrix);
        }
    }
}