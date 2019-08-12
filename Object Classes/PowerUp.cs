using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ICGGSAssignment
{
    public enum PowerUpType
    {
        Heal5,              // Instant effect +5 health
        Heal10,             // Instant effect +10 health
        Heal25,             // Instant effect +25 health
        NoChase,            // Timed effect prevents mines from chasing player
        Repel,              // Timed effect makes mines run away from player
        SlowFish,           // Timed effect slows down fish
        SlowMines           // Times effect slows down mines
    }

    public class PowerUp
    {
        private PowerUpType _type = PowerUpType.Heal5;
        public PowerUpType Type { get { return _type; } }

        private bool _isActive = false;
        public bool IsActive { get { return _isActive; } }

        // The time that the effect ends
        private DateTime _effectEnd = DateTime.Now;
        public DateTime EffectEnd { get { return _effectEnd; } }

        private Vector2 _position = Vector2.Zero;
        public Vector2 Position { get { return _position; } set { _position = value; } }

        /// <summary>
        /// Return the appropriate colour depending on the type
        /// </summary>
        public Color Color
        {
            get
            {
                switch (_type)
                {
                    // Heals all have the same colour
                    case PowerUpType.Heal5:
                    case PowerUpType.Heal10: 
                    case PowerUpType.Heal25:
                        return Color.LightGreen;
                    case PowerUpType.NoChase:
                        return Color.LightSkyBlue;
                    case PowerUpType.Repel:
                        return Color.LightPink;
                    case PowerUpType.SlowFish:
                        return Color.Yellow;
                    case PowerUpType.SlowMines:
                        return Color.Salmon;
                }
                return Color.White; // Default to Color.White if for some reason it doesn't have a type defined
            }
        }

        /// <summary>
        /// The name that is shown on the top-left corner of the power-up
        /// </summary>
        public string DisplayName
        {
            get
            {
                switch (_type)
                {
                    case PowerUpType.Heal5:
                        return "H5";
                    case PowerUpType.Heal10:
                        return "H10";
                    case PowerUpType.Heal25:
                        return "H25";
                    case PowerUpType.NoChase:
                        return "NC";
                    case PowerUpType.Repel:
                        return "RP";
                    case PowerUpType.SlowFish:
                        return "SF";
                    case PowerUpType.SlowMines:
                        return "SM";
                }
                return ""; // Return nothing if no type is defined
            }
        }

        /// <summary>
        /// Returns whether or not the power-up effect is running. (heals are instant so will always be false)
        /// </summary>
        public bool Running
        {
            get
            {
                // If type is a heal, return false
                if ((int)_type < 3) return false;
                else
                    // If the power-up hasn't ended, return true
                    if (DateTime.Now < _effectEnd && !_isActive)
                        return true;
                return false;
            }
        }

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)_position.X - 30, (int)_position.Y - 10, 60, 20);
            }
        }

        public PowerUp(PowerUpType t)
        {
            _type = t;
        }

        /// <summary>
        /// Generates a new power-up and activates it
        /// </summary>
        public void ResetAndActivate()
        {
            _type = (PowerUpType)Functions.Rand(0, 7);
            _position.Y = 15;
            _position.X = Functions.Rand(20, Functions.GameSize.X - 20);
            _isActive = true;
        }

        /// <summary>
        /// Manually assign a type
        /// </summary>
        /// <param name="t">The PowerUpType to set the power-up to</param>
        public void SetType(PowerUpType t)
        {
            _type = t;
        }

        /// <summary>
        /// Move the power-up down the screen until it reached the bottom, then deactivate it
        /// </summary>
        public void Update()
        {
            if (_isActive)
            {
                if (_position.X + 20 < 0 || _position.Y + 60 < 0 ||
                    _position.X - 20 > Functions.GameSize.X ||
                    _position.Y - 60 > Functions.GameSize.Y)
                {
                    _isActive = false;
                }
                else
                    // Move it down the screen
                    _position.Y += 1f;
            }
            else
            {
                // If _effectEnd is reached, end the power-up's effect
                if (DateTime.Now >= _effectEnd)
                    EndEffect();
            }
        }

        /// <summary>
        /// Applies the appropriate effect depending on the type and displays a message
        /// </summary>
        /// <param name="p">Piranha object to apply the effect to (only for heals)</param>
        public void ApplyEffect(Piranha p)
        {
            if (!_isActive) return; // Prevent applying the effect more than once
            switch (_type)
            {
                case PowerUpType.Heal5:
                    if (p.Health + 5 >= 100) { p.Health = 100; } else { p.Health += 5; }
                    Functions.Alert.Show("Health +5", Color.PaleGreen);
                    break;
                case PowerUpType.Heal10:
                    if (p.Health + 10 >= 100) { p.Health = 100; } else { p.Health += 10; }
                    Functions.Alert.Show("Health +10", Color.PaleGreen);
                    break;
                case PowerUpType.Heal25:
                    if (p.Health + 25 >= 100) { p.Health = 100; } else { p.Health += 25; }
                    Functions.Alert.Show("Health +25", Color.PaleGreen);
                    break;
                case PowerUpType.NoChase:
                    Functions.NoChaseActive = true;
                    Functions.Alert.Show("No-chase activated", Color.RoyalBlue);
                    break;
                case PowerUpType.Repel:
                    Functions.RepelActive = true;
                    Functions.Alert.Show("Repel activated", Color.LightPink);
                    break;
                case PowerUpType.SlowFish:
                    Functions.SlowFishActive = true;
                    Functions.Alert.Show("Slow fish activated", Color.Yellow);
                    break;
                case PowerUpType.SlowMines:
                    Functions.SlowMinesActive = true;
                    Functions.Alert.Show("Slow mines activated", Color.Salmon);
                    break;
            }
            this._isActive = false;
            _effectEnd = DateTime.Now + TimeSpan.FromSeconds(20);
        }

        /// <summary>
        /// End the power-ups effect. Heals aren't checked as they're instant
        /// </summary>
        public void EndEffect()
        {
            switch (_type)
            {
                case PowerUpType.NoChase:
                    Functions.NoChaseActive = false;
                    break;
                case PowerUpType.Repel:
                    Functions.RepelActive = false;
                    break;
                case PowerUpType.SlowFish:
                    Functions.SlowFishActive = false;
                    break;
                case PowerUpType.SlowMines:
                    Functions.SlowMinesActive = false;
                    break;
            }
            // Make sure the effect ends so that Running = false
            _effectEnd = DateTime.Now;
        }
    }
}
