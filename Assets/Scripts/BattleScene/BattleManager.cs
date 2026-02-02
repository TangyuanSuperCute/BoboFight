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

        [Header("Battle Info")]
        [SerializeField] private BattleInfoModel playerInfo;
        [SerializeField] private BattleInfoModel enemyInfo;

        public BattleState CurrentBattleState => _battleState;
        public RhythmState CurrentRhythmState => _rhythmState;
        public BattleInfoModel PlayerInfo => playerInfo;
        public BattleInfoModel EnemyInfo => enemyInfo;
        public int CurrentBeatCount => _currentBeatCount;
        public float TimeToNextBeat => Mathf.Max(0f, _nextBeatTime - Time.time);
        public float TimeToDecisionEnd => Mathf.Max(0f, _decisionEndTime - Time.time);

        public event Action<BattleState> OnBattleStateChanged;
        public event Action OnBattleInfoChanged;
        public event Action<int> OnBeat;
        public event Action OnDecisionStart;
        public event Action OnDecisionEnd;

        private BattleState _battleState = BattleState.None;
        private RhythmState _rhythmState = RhythmState.Idle;
        private int _currentBeatCount;
        private float _nextBeatTime;
        private float _decisionEndTime;

        private void Update()
        {
            if (_rhythmState == RhythmState.Beating)
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
            else if (_rhythmState == RhythmState.Decision)
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

        public void StartBattle()
        {
            SetBattleState(BattleState.Preparing);
            StartRhythm();
        }

        public void EndBattle()
        {
            StopRhythm();
            SetBattleState(BattleState.Finished);
        }

        public void StartRhythm()
        {
            StartBeating();
            SetBattleState(BattleState.Rhythm);
        }

        public void StopRhythm()
        {
            _rhythmState = RhythmState.Idle;
            _currentBeatCount = 0;
        }

        public void SetPlayerInfo(BattleInfoModel info)
        {
            playerInfo = info;
            OnBattleInfoChanged?.Invoke();
        }

        public void SetEnemyInfo(BattleInfoModel info)
        {
            enemyInfo = info;
            OnBattleInfoChanged?.Invoke();
        }

        public void UpdatePlayerStatus(int hp, int dodge, int spirit)
        {
            if (playerInfo == null) return;
            playerInfo.hp = hp;
            playerInfo.dodgeCount = dodge;
            playerInfo.spiritCount = spirit;
            OnBattleInfoChanged?.Invoke();
        }

        public void UpdateEnemyStatus(int hp, int dodge, int spirit)
        {
            if (enemyInfo == null) return;
            enemyInfo.hp = hp;
            enemyInfo.dodgeCount = dodge;
            enemyInfo.spiritCount = spirit;
            OnBattleInfoChanged?.Invoke();
        }

        private void StartBeating()
        {
            _rhythmState = RhythmState.Beating;
            _currentBeatCount = 0;
            _nextBeatTime = Time.time + beatIntervalSeconds;
        }

        private void EnterDecision()
        {
            _rhythmState = RhythmState.Decision;
            _decisionEndTime = Time.time + decisionTimeSeconds;
            OnDecisionStart?.Invoke();
            SetBattleState(BattleState.Decision);
        }

        private void SetBattleState(BattleState nextState)
        {
            if (_battleState == nextState) return;
            _battleState = nextState;
            OnBattleStateChanged?.Invoke(_battleState);
        }
    }
}