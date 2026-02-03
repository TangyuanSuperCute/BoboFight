using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleScene.UI
{
    public class PlayerInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text textName;
        [SerializeField] private TMP_Text textHp;
        [SerializeField] private TMP_Text textDodge;
        [SerializeField] private Image imageAvatar;
        [SerializeField] private Image imageHp;

        [SerializeField] private PlayerSkillPanel skillPanel;
        public PlayerSkillPanel SkillPanel => skillPanel;

        [Header("Dodge Effects")]
        [SerializeField] private float dodgePulseScale = 1.12f;
        [SerializeField] private float dodgeIncreaseNudge = 10f;
        [SerializeField] private float dodgeDecreaseNudge = -8f;
        [SerializeField] private float dodgeFlickerAlpha = 0.6f;
        [SerializeField] private float dodgeNotEnoughShake = 10f;

        private Tween _dodgeTween;
        private int _displayDodge;
        private Color _baseDodgeColor = Color.white;
        private Vector3 _baseDodgeScale = Vector3.one;
        private Vector2 _baseDodgePos;

        private void Awake()
        {
            if (textDodge != null)
            {
                _baseDodgeColor = textDodge.color;
                _baseDodgeScale = textDodge.rectTransform.localScale;
                _baseDodgePos = textDodge.rectTransform.anchoredPosition;
            }
        }

        public void SetInfo(BattleInfoModel info)
        {
            if (info == null) return;

            if (textName != null)
            {
                textName.text = string.IsNullOrEmpty(info.name) ? "???" : info.name;
            }

            if (textHp != null)
            {
                textHp.text = FormatCount(info.hp, info.hpMax);
            }

            if (textDodge != null)
            {
                UpdateDodge(info.dodgeCount, info.dodgeMax);
            }

            if (imageAvatar != null)
            {
                imageAvatar.sprite = info.avatar;
                imageAvatar.enabled = info.avatar != null;
            }

            if (imageHp != null)
            {
                imageHp.fillAmount = info.hpMax > 0 ? Mathf.Clamp01((float)info.hp / info.hpMax) : 0f;
            }

            if (skillPanel != null)
            {
                skillPanel.SetSkills(info);
            }
        }

        private static string FormatCount(int current, int max)
        {
            return max > 0 ? $"{current}/{max}" : $"{current}";
        }

        public void PlayDodgeNotEnough()
        {
            if (textDodge == null) return;
            _dodgeTween?.Kill();
            PlayDodgeNotEnoughStyle();
        }

        private void UpdateDodge(int current, int max)
        {
            if (textDodge == null) return;

            _dodgeTween?.Kill();

            var startValue = _displayDodge;
            _displayDodge = current;
            textDodge.text = FormatCount(current, max);

            if (startValue == current) return;

            if (current > startValue)
            {
                PlayDodgeIncreaseStyle();
            }
            else
            {
                PlayDodgeDecreaseStyle();
            }
        }

        private void PlayDodgeIncreaseStyle()
        {
            var rect = textDodge.rectTransform;
            var targetScale = _baseDodgeScale * dodgePulseScale;
            _dodgeTween = DOTween.Sequence()
                .Append(rect.DOScale(targetScale, 0.08f))
                .Join(rect.DOAnchorPosY(_baseDodgePos.y + dodgeIncreaseNudge, 0.08f))
                .Join(textDodge.DOFade(1f, 0.06f))
                .Append(textDodge.DOFade(dodgeFlickerAlpha, 0.03f))
                .Append(textDodge.DOFade(_baseDodgeColor.a, 0.05f))
                .Join(rect.DOAnchorPosY(_baseDodgePos.y, 0.08f))
                .Join(rect.DOScale(_baseDodgeScale, 0.1f));
        }

        private void PlayDodgeDecreaseStyle()
        {
            var rect = textDodge.rectTransform;
            _dodgeTween = DOTween.Sequence()
                .Append(rect.DOAnchorPosY(_baseDodgePos.y + dodgeDecreaseNudge, 0.08f))
                .Join(textDodge.DOFade(dodgeFlickerAlpha, 0.05f))
                .Join(rect.DOShakeAnchorPos(0.16f, dodgeNotEnoughShake, 16, 90, false, true))
                .Append(textDodge.DOFade(_baseDodgeColor.a, 0.06f))
                .Join(rect.DOAnchorPosY(_baseDodgePos.y, 0.1f));
        }

        private void PlayDodgeNotEnoughStyle()
        {
            var rect = textDodge.rectTransform;
            _dodgeTween = DOTween.Sequence()
                .Append(textDodge.DOFade(dodgeFlickerAlpha, 0.05f))
                .Join(rect.DOShakeAnchorPos(0.25f, dodgeNotEnoughShake, 20, 90, false, true))
                .Append(textDodge.DOFade(_baseDodgeColor.a, 0.05f))
                .Join(rect.DOAnchorPos(_baseDodgePos, 0.01f));
        }

        private void OnDestroy()
        {
            _dodgeTween?.Kill();
        }
    }
}