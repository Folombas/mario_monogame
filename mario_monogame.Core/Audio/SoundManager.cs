using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace mario_monogame.Core.Audio
{
    /// <summary>
    /// Менеджер звука для управления музыкой и звуковыми эффектами.
    /// </summary>
    public class SoundManager : IDisposable
    {
        private readonly ContentManager _contentManager;
        private readonly Dictionary<string, SoundEffect> _soundEffects;
        private readonly Dictionary<string, Song> _musicTracks;
        
        private Song _currentSong;
        private float _musicVolume;
        private float _sfxVolume;
        private bool _musicEnabled;
        private bool _sfxEnabled;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = MathHelper.Clamp(value, 0f, 1f);
                MediaPlayer.Volume = _musicVolume;
            }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = MathHelper.Clamp(value, 0f, 1f);
        }

        public bool MusicEnabled
        {
            get => _musicEnabled;
            set
            {
                _musicEnabled = value;
                if (!_musicEnabled)
                {
                    MediaPlayer.Stop();
                }
            }
        }

        public bool SfxEnabled
        {
            get => _sfxEnabled;
            set => _sfxEnabled = value;
        }

        public SoundManager(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _soundEffects = new Dictionary<string, SoundEffect>();
            _musicTracks = new Dictionary<string, Song>();
            _musicVolume = 0.5f;
            _sfxVolume = 0.7f;
            _musicEnabled = true;
            _sfxEnabled = true;
        }

        public void LoadSoundEffect(string name, string path)
        {
            try
            {
                var soundEffect = _contentManager.Load<SoundEffect>(path);
                _soundEffects[name] = soundEffect;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load sound effect '{name}': {ex.Message}");
            }
        }

        public void LoadMusic(string name, string path)
        {
            try
            {
                var song = _contentManager.Load<Song>(path);
                _musicTracks[name] = song;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load music '{name}': {ex.Message}");
            }
        }

        public void PlaySound(string name)
        {
            if (!_sfxEnabled) return;
            
            if (_soundEffects.TryGetValue(name, out var soundEffect))
            {
                soundEffect.Play(_sfxVolume, 0f, 0f);
            }
        }

        public void PlayMusic(string name, bool isLooping = true)
        {
            if (!_musicEnabled) return;
            
            if (_musicTracks.TryGetValue(name, out var song))
            {
                MediaPlayer.IsRepeating = isLooping;
                MediaPlayer.Play(song);
                MediaPlayer.Volume = _musicVolume;
            }
        }

        public void StopMusic()
        {
            MediaPlayer.Stop();
        }

        public void PauseMusic()
        {
            MediaPlayer.Pause();
        }

        public void ResumeMusic()
        {
            MediaPlayer.Resume();
        }

        /// <summary>
        /// Воспроизвести звук прыжка.
        /// </summary>
        public void PlayJumpSound()
        {
            PlaySound("jump");
        }

        /// <summary>
        /// Воспроизвести звук сбора моркови.
        /// </summary>
        public void PlayCollectSound()
        {
            PlaySound("collect");
        }

        /// <summary>
        /// Воспроизвести звук сдачи моркови в домик.
        /// </summary>
        public void PlayDepositSound()
        {
            PlaySound("deposit");
        }

        /// <summary>
        /// Воспроизвести звук уровня.
        /// </summary>
        public void PlayLevelUpSound()
        {
            PlaySound("levelup");
        }

        /// <summary>
        /// Воспроизвести звук столкновения с врагом.
        /// </summary>
        public void PlayHitSound()
        {
            PlaySound("hit");
        }

        /// <summary>
        /// Воспроизвести фоновую музыку.
        /// </summary>
        public void PlayBackgroundMusic()
        {
            PlayMusic("background", true);
        }

        public void Dispose()
        {
            MediaPlayer.Stop();
            
            foreach (var soundEffect in _soundEffects.Values)
            {
                soundEffect?.Dispose();
            }
            _soundEffects.Clear();
        }
    }
}
