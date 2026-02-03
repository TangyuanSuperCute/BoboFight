using DG.Tweening;
using TMPro;
using UnityEngine;

namespace BattleScene.UI
{
    public class MainBattlePanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text textBeat;
        
        [SerializeField] private EnemyInfoPanel enemyInfo;
        [SerializeField] private PlayerInfoPanel playerInfo;

        [SerializeField] private BattleManager battleManager;

        [SerializeField] private Color clapColor = Color.white;
        [SerializeField] private Color decisionColor = new Color(1f, 0.85f, 0.45f, 1f);
        [SerializeField] private Color beatWarningColor = new Color(1f, 0.35f, 0.35f, 1f);

        private const string DefaultDecisionAction = "攒";
        private const string BeatToken = "啪";
        private const string SpiritNotEnoughText = "没气了";
        private const string DodgeNotEnoughText = "没防了";
        private string _decisionActionName = DefaultDecisionAction;
        private Tween _textTween;
        private bool _suppressDecisionText;

        private void OnEnable()
        {
            if (battleManager == null)
            {
                battleManager = FindObjectOfType<BattleManager>();
            }

            if (battleManager != null)
            {
                battleManager.OnBeat += HandleBeat;
                battleManager.OnDecisionStart += HandleDecisionStart;
                battleManager.OnDecisionEnd += HandleDecisionEnd;
                battleManager.OnBattleInfoChanged += RefreshInfo;
                battleManager.OnDecisionActionChanged += HandleDecisionActionChanged;
                battleManager.OnActionRejected += HandleActionRejected;
            }

            RefreshInfo();
        }

        private void OnDisable()
        {
            if (battleManager == null) return;

            battleManager.OnBeat -= HandleBeat;
            battleManager.OnDecisionStart -= HandleDecisionStart;
            battleManager.OnDecisionEnd -= HandleDecisionEnd;
            battleManager.OnBattleInfoChanged -= RefreshInfo;
            battleManager.OnDecisionActionChanged -= HandleDecisionActionChanged;
            battleManager.OnActionRejected -= HandleActionRejected;
        }

        private void HandleBeat(int beatIndex)
        {
            PlayBeatText(BeatToken, GetBeatHoldSeconds(), clapColor);
        }

        private void HandleDecisionStart()
        {
            _suppressDecisionText = false;
            _decisionActionName = DefaultDecisionAction;
        }

        private void HandleDecisionEnd()
        {
            if (!_suppressDecisionText)
            {
                PlayBeatText(_decisionActionName, GetBeatHoldSeconds(), decisionColor);
            }
            _decisionActionName = DefaultDecisionAction;
            _suppressDecisionText = false;
        }

        private void HandleDecisionActionChanged(string actionName)
        {
            _decisionActionName = string.IsNullOrEmpty(actionName) ? DefaultDecisionAction : actionName;
        }

        private void RefreshInfo()
        {
            if (battleManager == null) return;

            if (enemyInfo != null)
            {
                enemyInfo.SetInfo(battleManager.EnemyInfo);
            }

            if (playerInfo != null)
            {
                playerInfo.SetInfo(battleManager.PlayerInfo);
            }
        }

        private void HandleActionRejected(ActionRejectReason reason)
        {
            if (reason == ActionRejectReason.SpiritNotEnough)
            {
                PlayBeatText(SpiritNotEnoughText, 0.5f, beatWarningColor);
                playerInfo?.SkillPanel?.PlaySpiritNotEnough();
                _suppressDecisionText = true;
            }
            else if (reason == ActionRejectReason.DodgeNotEnough)
            {
                PlayBeatText(DodgeNotEnoughText, 0.5f, beatWarningColor);
                playerInfo?.PlayDodgeNotEnough();
                _suppressDecisionText = true;
            }
        }

        private void PlayBeatText(string text, float holdSeconds, Color color)
        {
            if (textBeat == null) return;

            textBeat.text = text;
            textBeat.color = color;
            textBeat.alpha = 0f;
            textBeat.rectTransform.localScale = Vector3.one * 0.85f;

            _textTween?.Kill();

            var appear = 0.12f;
            var disappear = 0.12f;
            var hold = Mathf.Max(0f, holdSeconds - appear - disappear);

            _textTween = DOTween.Sequence()
                .Append(textBeat.DOFade(1f, appear))
                .Join(textBeat.rectTransform.DOScale(1f, appear))
                .AppendInterval(hold)
                .Append(textBeat.DOFade(0f, disappear))
                .Join(textBeat.rectTransform.DOScale(0.9f, disappear));
        }

        private float GetBeatHoldSeconds()
        {
            return battleManager != null ? battleManager.beatIntervalSeconds : 0.5f;
        }

        private float GetDecisionHoldSeconds()
        {
            return battleManager != null ? battleManager.decisionTimeSeconds : 0.6f;
        }

        private void OnDestroy()
        {
            _textTween?.Kill();
        }
    }
}
 