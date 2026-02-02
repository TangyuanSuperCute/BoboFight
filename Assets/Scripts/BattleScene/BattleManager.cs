using System;
using UnityEngine;

namespace BattleScene
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Rhythm Settings")]
        [Min(1)] public int beatsBeforeDecision = 2;
        [Min(0.05f)] public float beatIntervalSeconds = 0.5f;
        [Min(0.1f)] public float decisionTimeSeconds = 1.0f;

        public enum RhythmState
        {
            Idle,
            Beating,
            Decision
        }

        public RhythmState State => _state;
        public int CurrentBeatCount => _currentBeatCount;
        public float TimeToNextBeat => Mathf.Max(0f, _nextBeatTime - Time.time);
        public float TimeToDecisionEnd => Mathf.Max(0f, _decisionEndTime - Time.time);

        public event Action<int> OnBeat;
        public event Action OnDecisionStart;
        public event Action OnDecisionEnd;

        private RhythmState _state = RhythmState.Idle;
        private int _currentBeatCount;
        private float _nextBeatTime;
        private float _decisionEndTime;

        private void Update()
        {
            if (_state == RhythmState.Beating)
            {
                if (Time.time >= _nextBeatTime)
                {
                    _currentBeatCount++;
                    OnBeat?.Invoke(_currentBeatCount);

                    if (_currentBeatCount >= beatsBeforeDecision)
                    {
                        EnterDecision();
                    }
                    else
                    {
                        _nextBeatTime = Time.time + beatIntervalSeconds;
                    }
                }
            }
            else if (_state == RhythmState.Decision)
            {
                if (Time.time >= _decisionEndTime)
                {
                    OnDecisionEnd?.Invoke();
                    StartBeating();
                }
            }
        }

        public void Configure(int beats, float intervalSeconds, float decisionSeconds)
        {
            beatsBeforeDecision = Mathf.Max(1, beats);
            beatIntervalSeconds = Mathf.Max(0.05f, intervalSeconds);
            decisionTimeSeconds = Mathf.Max(0.1f, decisionSeconds);
        }

        public void StartRhythm()
        {
            StartBeating();
        }

        public void StopRhythm()
        {
            _state = RhythmState.Idle;
            _currentBeatCount = 0;
        }

        private void StartBeating()
        {
            _state = RhythmState.Beating;
            _currentBeatCount = 0;
            _nextBeatTime = Time.time + beatIntervalSeconds;
        }

        private void EnterDecision()
        {
            _state = RhythmState.Decision;
            _decisionEndTime = Time.time + decisionTimeSeconds;
            OnDecisionStart?.Invoke();
        }
    }
}