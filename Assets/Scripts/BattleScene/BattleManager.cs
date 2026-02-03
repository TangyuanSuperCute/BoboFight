using System;
using UnityEngine;

namespace BattleScene
{
    public class BattleManager : MonoBehaviour
    {
        [Header("Rhythm Settings")]
        [Min(1)] public int beatsBeforeDecision = 2;
        [Min(0.05f)] public float beatIntervalSeconds = 0.5f;
        [Min(0.1f)] public float decisionTimeSeconds = 0.5f;
        [Min(0.05f)] public float loopIntervalSeconds = 0.5f;
        [Min(0.1f)] public float dodgeRegenIntervalSeconds = 5f;
        [SerializeField] private bool autoStartBattle = true;

        [Header("Battle Info")]
        [SerializeField] private BattleInfoModel playerInfo;
        [SerializeField] private BattleInfoModel enemyInfo;
        
        public BattleState CurrentBattleState => _battleState;
        public RhythmState CurrentRhythmState => _rhythmState;
        public RoundPhase CurrentRoundPhase => _roundPhase;
        public BattleInfoModel PlayerInfo => playerInfo;
        public BattleInfoModel EnemyInfo => enemyInfo;
        public int CurrentBeatCount => _currentBeatCount;
        public float TimeToNextBeat => Mathf.Max(0f, _nextBeatTime - Time.time);
        public float TimeToDecisionEnd => Mathf.Max(0f, _decisionEndTime - Time.time);
        public bool IsDecisionWindow => _decisionWindowOpen;

        public event Action<BattleState> OnBattleStateChanged;
        public event Action OnBattleInfoChanged;
        public event Action<int> OnBeat;
        public event Action OnDecisionStart;
        public event Action OnDecisionEnd;
        public event Action<string> OnDecisionActionChanged;
        public event Action<ActionRejectReason> OnActionRejected;
        public event Action<RoundPhase> OnRoundPhaseChanged;

        private BattleState _battleState = BattleState.None;
        private RhythmState _rhythmState = RhythmState.Idle;
        private int _currentBeatCount;
        private float _nextBeatTime;
        private float _decisionStartTime;
        private float _decisionEndTime;
        private bool _decisionStarted;
        private bool _playerActedThisDecision;
        private bool _decisionWindowOpen;
        private bool _selectionLocked;
        private string _decisionActionName = "攒";
        private float _nextDodgeRegenTime;
        private RoundPhase _roundPhase = RoundPhase.None;

        private void Awake()
        {
            playerInfo = CreateTestPlayer();
            enemyInfo = CreateTestEnemy();
        }

        private void Start()
        {
            OnBattleInfoChanged?.Invoke();
            if (autoStartBattle)
            {
                StartBattle();
            }
        }

        private void Update()
        {
            TickDodgeRegen();

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
                if (!_decisionStarted)
                {
                    if (Time.time >= _decisionStartTime)
                    {
                        _decisionStarted = true;
                        _decisionWindowOpen = true;
                        OnDecisionStart?.Invoke();
                    }
                }
                else if (Time.time >= _decisionEndTime)
                {
                    ApplyDecisionResult();
                    _decisionWindowOpen = false;
                    SetRoundPhase(RoundPhase.Result);
                    OnDecisionEnd?.Invoke();
                    _rhythmState = RhythmState.Idle;
                    _nextBeatTime = Time.time + loopIntervalSeconds;
                    SetRoundPhase(RoundPhase.LoopDelay);
                }
            }
            else if (_rhythmState == RhythmState.Idle)
            {
                if (Time.time >= _nextBeatTime)
                {
                    StartBeating();
                }
            }
        }

        public void Configure(int beats, float intervalSeconds, float decisionSeconds, float loopSeconds)
        {
            beatsBeforeDecision = Mathf.Max(1, beats);
            beatIntervalSeconds = Mathf.Max(0.05f, intervalSeconds);
            decisionTimeSeconds = Mathf.Max(0.1f, decisionSeconds);
            loopIntervalSeconds = Mathf.Max(0.05f, loopSeconds);
        }

        public void StartBattle()
        {
            SetBattleState(BattleState.Preparing);
            StartRhythm();
            _nextDodgeRegenTime = Time.time + dodgeRegenIntervalSeconds;
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
            SetRoundPhase(RoundPhase.None);
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

        public bool TrySpendPlayerSpirit(int cost)
        {
            if (playerInfo == null) return false;
            if (cost <= 0) return true;
            if (playerInfo.spiritCount < cost) return false;

            playerInfo.spiritCount -= cost;
            OnBattleInfoChanged?.Invoke();
            return true;
        }

        public bool TrySpendPlayerDodge()
        {
            if (playerInfo == null) return false;
            if (playerInfo.dodgeCount <= 0) return false;

            playerInfo.dodgeCount -= 1;
            OnBattleInfoChanged?.Invoke();
            return true;
        }

        public bool TrySelectPlayerSkill(int index, out ActionRejectReason reason)
        {
            if (!_decisionWindowOpen)
            {
                reason = ActionRejectReason.NotInDecision;
                return false;
            }

            if (_selectionLocked)
            {
                reason = ActionRejectReason.AlreadySelected;
                return false;
            }

            var skills = playerInfo?.skills;
            if (skills == null || index < 0 || index >= skills.Count)
            {
                reason = ActionRejectReason.InvalidSkill;
                return false;
            }

            var skill = skills[index];
            if (skill == null)
            {
                reason = ActionRejectReason.InvalidSkill;
                return false;
            }

            if (skill.type == SkillType.Dodge)
            {
                if (!TrySpendPlayerDodge())
                {
                    reason = ActionRejectReason.DodgeNotEnough;
                    _playerActedThisDecision = true;
                    _selectionLocked = true;
                    OnActionRejected?.Invoke(reason);
                    return false;
                }
            }
            else
            {
                if (!TrySpendPlayerSpirit(skill.cast))
                {
                    reason = ActionRejectReason.SpiritNotEnough;
                    _playerActedThisDecision = true;
                    _selectionLocked = true;
                    OnActionRejected?.Invoke(reason);
                    return false;
                }
            }

            _decisionActionName = skill.name;
            _playerActedThisDecision = true;
            _selectionLocked = true;
            reason = ActionRejectReason.None;
            OnDecisionActionChanged?.Invoke(_decisionActionName);
            return true;
        }

        private void StartBeating()
        {
            _rhythmState = RhythmState.Beating;
            _currentBeatCount = 0;
            _nextBeatTime = Time.time + beatIntervalSeconds;
            SetRoundPhase(RoundPhase.Beat);
        }

        private void EnterDecision()
        {
            _rhythmState = RhythmState.Decision;
            _decisionStarted = false;
            _playerActedThisDecision = false;
            _selectionLocked = false;
            _decisionActionName = "攒";
            _decisionStartTime = Time.time;
            _decisionEndTime = _decisionStartTime + decisionTimeSeconds;
            SetBattleState(BattleState.Decision);
            SetRoundPhase(RoundPhase.Decision);
        }

        private void SetBattleState(BattleState nextState)
        {
            if (_battleState == nextState) return;
            _battleState = nextState;
            OnBattleStateChanged?.Invoke(_battleState);
        }

        private void SetRoundPhase(RoundPhase nextPhase)
        {
            if (_roundPhase == nextPhase) return;
            _roundPhase = nextPhase;
            OnRoundPhaseChanged?.Invoke(_roundPhase);
        }

        private void TickDodgeRegen()
        {
            if (playerInfo == null) return;
            if (_battleState == BattleState.None || _battleState == BattleState.Finished) return;
            if (dodgeRegenIntervalSeconds <= 0f) return;
            if (playerInfo.dodgeMax <= 0) return;

            if (playerInfo.dodgeCount >= playerInfo.dodgeMax)
            {
                _nextDodgeRegenTime = Time.time + dodgeRegenIntervalSeconds;
                return;
            }

            if (Time.time < _nextDodgeRegenTime) return;

            playerInfo.dodgeCount += 1;
            _nextDodgeRegenTime = Time.time + dodgeRegenIntervalSeconds;
            OnBattleInfoChanged?.Invoke();
        }

        private void ApplyDecisionResult()
        {
            if (playerInfo == null) return;
            if (_playerActedThisDecision) return;

            var next = playerInfo.spiritCount + 1;
            if (playerInfo.spiritMax > 0)
            {
                next = Mathf.Min(next, playerInfo.spiritMax);
            }

            if (next == playerInfo.spiritCount) return;
            playerInfo.spiritCount = next;
            OnBattleInfoChanged?.Invoke();
        }

        private static BattleInfoModel CreateTestPlayer()
        {
            var info = new BattleInfoModel
            {
                name = "玩家",
                hpMax = 10,
                hpMin = 0,
                hp = 10,
                dodgeMax = 2,
                dodgeCount = 2,
                spiritMax = 10,
                spiritCount = 0
            };

            info.skills.Add(new SkillModel { name = "防御", cast = 0, type = SkillType.Dodge });
            info.skills.Add(new SkillModel { name = "普攻", cast = 1, type = SkillType.NormalAttack });
            info.skills.Add(new SkillModel { name = "技能1", cast = 2, type = SkillType.AttackSkill });
            info.skills.Add(new SkillModel { name = "技能2", cast = 2, type = SkillType.DefenseSkill });

            return info;
        }

        private static BattleInfoModel CreateTestEnemy()
        {
            var info = new BattleInfoModel
            {
                name = "敌人",
                hpMax = 10,
                hpMin = 0,
                hp = 10,
                dodgeMax = 2,
                dodgeCount = 2,
                spiritMax = 10,
                spiritCount = 0
            };

            info.skills.Add(new SkillModel { name = "防御", cast = 0, type = SkillType.Dodge });
            info.skills.Add(new SkillModel { name = "普攻", cast = 1, type = SkillType.NormalAttack });
            info.skills.Add(new SkillModel { name = "技能1", cast = 2, type = SkillType.AttackSkill });
            info.skills.Add(new SkillModel { name = "技能2", cast = 2, type = SkillType.DefenseSkill });

            return info;
        }
    }
}