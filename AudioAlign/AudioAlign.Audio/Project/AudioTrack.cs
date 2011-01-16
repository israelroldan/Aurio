﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace AudioAlign.Audio.Project {
    public class AudioTrack : Track {

        public const string PEAKFILE_EXTENSION = ".aapeaks";

        static AudioTrack() {
            MediaType = MediaType.Audio;
        }

        public event EventHandler<ValueEventArgs<bool>> MuteChanged;
        public event EventHandler<ValueEventArgs<bool>> SoloChanged;
        public event EventHandler<ValueEventArgs<float>> VolumeChanged;
        public event EventHandler<ValueEventArgs<bool>> InvertedPhaseChanged;

        private bool mute = false;
        private bool solo = false;
        private float volume = 1.0f;
        private bool invertedPhase = false;

        public AudioTrack(FileInfo fileInfo) : base(fileInfo) {
            this.Length = CreateAudioStream().TimeLength;
        }

        public IAudioStream16 CreateAudioStream() {
            return AudioStreamFactory.FromFileInfo(FileInfo);
        }

        public FileInfo PeakFile {
            get {
                return new FileInfo(FileInfo.FullName + PEAKFILE_EXTENSION);
            }
        }

        public bool HasPeakFile {
            get {
                return PeakFile.Exists;
            }
        }

        public AudioProperties Properties {
            get {
                // TODO check how often this is called... on frequent calls cache the properties locally
                return CreateAudioStream().Properties;
            }
        }

        /// <summary>
        /// Gets or sets a value telling is this track is muted.
        /// </summary>
        public bool Mute { get { return mute; } set { mute = value; OnMuteChanged(); } }

        /// <summary>
        /// Gets or sets a value that tells is this track is to be played solo.
        /// If the solo property of at least one track in a project is set to true, only the tracks with
        /// solo set to true will be played.
        /// </summary>
        public bool Solo { get { return solo; } set { solo = value; OnSoloChanged(); } }

        /// <summary>
        /// Gets or sets the volume of this track. 0.0f equals to mute, 1.0f is the default audio 
        /// level (volume stays unchanged). 2.0f means the volume will be twice the default intensity.
        /// </summary>
        public float Volume { get { return volume; } set { volume = value; OnVolumeChanged(); } }

        /// <summary>
        /// Gets or sets a value telling is this track' audio phase is inverted.
        /// </summary>
        public bool InvertedPhase { get { return invertedPhase; } set { invertedPhase = value; OnInvertedPhaseChanged(); } }

        private void OnMuteChanged() {
            if (MuteChanged != null) {
                MuteChanged(this, new ValueEventArgs<bool>(mute));
            }
            OnPropertyChanged("Mute");
        }

        private void OnSoloChanged() {
            if (solo) {
                Mute = false;
            }

            if (SoloChanged != null) {
                SoloChanged(this, new ValueEventArgs<bool>(solo));
            }
            OnPropertyChanged("Solo");
        }

        private void OnVolumeChanged() {
            if (VolumeChanged != null) {
                VolumeChanged(this, new ValueEventArgs<float>(volume));
            }
            OnPropertyChanged("Volume");
        }

        private void OnInvertedPhaseChanged() {
            if (InvertedPhaseChanged != null) {
                InvertedPhaseChanged(this, new ValueEventArgs<bool>(invertedPhase));
            }
            OnPropertyChanged("InvertedPhase");
        }
    }
}
