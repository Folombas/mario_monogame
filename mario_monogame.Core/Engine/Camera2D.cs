using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace mario_monogame.Core.Engine
{
    /// <summary>
    /// 2D камера с эффектами.
    /// </summary>
    public class Camera2D
    {
        private Vector2 _position;
        private float _zoom;
        private float _rotation;
        private Matrix _transformMatrix;
        private Matrix _inverseTransformMatrix;

        private float _shakeIntensity;
        private float _shakeTimer;
        private Vector2 _shakeOffset;

        private Viewport _viewport;

        public Vector2 Position { get => _position; set => _position = value; }
        public float Zoom { get => _zoom; set => _zoom = MathHelper.Clamp(value, 0.1f, 5f); }
        public float Rotation { get => _rotation; set => _rotation = value; }

        public Rectangle Bounds => new Rectangle(
            (int)(_position.X - _viewport.Width / (2f * _zoom)),
            (int)(_position.Y - _viewport.Height / (2f * _zoom)),
            (int)(_viewport.Width / _zoom),
            (int)(_viewport.Height / _zoom)
        );

        public Vector2 ScreenCenter => new Vector2(_viewport.Width / 2f, _viewport.Height / 2f);

        public Camera2D(GraphicsDevice graphicsDevice)
        {
            _viewport = graphicsDevice.Viewport;
            _position = Vector2.Zero;
            _zoom = 1f;
            _rotation = 0f;
        }

        public void Update(float deltaTime)
        {
            if (_shakeTimer > 0f)
            {
                _shakeTimer -= deltaTime;
                if (_shakeTimer <= 0f)
                {
                    _shakeOffset = Vector2.Zero;
                    _shakeIntensity = 0f;
                }
                else
                {
                    _shakeOffset = new Vector2(
                        (float)(Random.Shared.NextDouble() * 2 - 1) * _shakeIntensity,
                        (float)(Random.Shared.NextDouble() * 2 - 1) * _shakeIntensity
                    );
                }
            }

            _transformMatrix = Matrix.CreateTranslation(new Vector3(-_position.X + _shakeOffset.X, -_position.Y + _shakeOffset.Y, 0)) *
                               Matrix.CreateRotationZ(_rotation) *
                               Matrix.CreateScale(_zoom, _zoom, 1) *
                               Matrix.CreateTranslation(new Vector3(ScreenCenter.X, ScreenCenter.Y, 0));

            _inverseTransformMatrix = Matrix.Invert(_transformMatrix);
        }

        public Matrix GetViewMatrix() => _transformMatrix;
        public Matrix GetInverseViewMatrix() => _inverseTransformMatrix;

        public Vector2 ScreenToWorld(Vector2 screenPosition) => Vector2.Transform(screenPosition, _inverseTransformMatrix);
        public Vector2 WorldToScreen(Vector2 worldPosition) => Vector2.Transform(worldPosition, _transformMatrix);

        public bool IsVisible(Rectangle bounds) => bounds.Right > Bounds.Left && bounds.Left < Bounds.Right && bounds.Bottom > Bounds.Top && bounds.Top < Bounds.Bottom;
        public bool IsVisible(Vector2 position) => position.X > Bounds.Left && position.X < Bounds.Right && position.Y > Bounds.Top && position.Y < Bounds.Bottom;

        public void Shake(float intensity = 10f, float duration = 0.5f)
        {
            _shakeIntensity = intensity;
            _shakeTimer = duration;
        }

        public void Follow(Vector2 target, float smoothness = 5f, float deltaTime = 0.016f)
        {
            _position = Vector2.Lerp(_position, target, smoothness * deltaTime);
        }

        public void FollowWithDeadzone(Vector2 target, Vector2 deadzoneSize, float deltaTime = 0.016f)
        {
            Rectangle deadzone = new Rectangle(
                (int)(_position.X - deadzoneSize.X / 2f),
                (int)(_position.Y - deadzoneSize.Y / 2f),
                (int)deadzoneSize.X,
                (int)deadzoneSize.Y
            );

            if (target.X < deadzone.Left)
                _position.X = MathHelper.Lerp(_position.X, target.X + deadzoneSize.X / 2f, 5f * deltaTime);
            else if (target.X > deadzone.Right)
                _position.X = MathHelper.Lerp(_position.X, target.X - deadzoneSize.X / 2f, 5f * deltaTime);

            if (target.Y < deadzone.Top)
                _position.Y = MathHelper.Lerp(_position.Y, target.Y + deadzoneSize.Y / 2f, 5f * deltaTime);
            else if (target.Y > deadzone.Bottom)
                _position.Y = MathHelper.Lerp(_position.Y, target.Y - deadzoneSize.Y / 2f, 5f * deltaTime);
        }

        public void SetBounds(Rectangle levelBounds)
        {
            _position.X = MathHelper.Clamp(_position.X, levelBounds.X + ScreenCenter.X / _zoom, levelBounds.Right - ScreenCenter.X / _zoom);
            _position.Y = MathHelper.Clamp(_position.Y, levelBounds.Y + ScreenCenter.Y / _zoom, levelBounds.Bottom - ScreenCenter.Y / _zoom);
        }

        public void Reset()
        {
            _position = Vector2.Zero;
            _zoom = 1f;
            _rotation = 0f;
            _shakeOffset = Vector2.Zero;
            _shakeTimer = 0f;
            _shakeIntensity = 0f;
        }
    }
}
