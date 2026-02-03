using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleScene.UI
{
    public class PlayerSkillPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text textSpirit;
        [SerializeField] private List<Button> skillButtons;
        [SerializeField] private List<TMP_Text> skillTexts;
        [SerializeField] private float spiritPulseScale = 1.15f;
        [SerializeField] private float spiritNotEnoughShake = 12f;
        [SerializeField] private float spiritFlickerAlpha = 0.6f;
        [SerializeField] private float spiritIncreaseNudge = 14f;
        [SerializeField] private float spiritDecreaseNudge = -10f;

        private Tween _spiritTween;
        private Tween _spiritStyleTween;
        private int _displaySpirit;
        private Color _baseSpiritColor = Color.white;
        private Vector3 _baseSpiritScale = Vector3.one;

        private void Awake()
        {
            if (textSpirit != null)
            {
                _baseSpiritColor = textSpirit.color;
                _baseSpiritScale = textSpirit.rectTransform.localScale;
            }
        }

        public void SetSkills(BattleInfoModel info)
        {
            if (info == null) return;

            var skills = info.skills;
            var count = skills.Count;

            if (textSpirit != null)
            {
                UpdateSpirit(info.spiritCount);
            }
            
            if (skillTexts != null)
            {
                for (var i = 0; i < skillTexts.Count; i++)
                {
                    if (skillTexts[i] == null) continue;
                    if (i < count && skills[i] != null)
                    {
                        skillTexts[i].text = skills[i].name;
                    }
                    else
                    {
                        skillTexts[i].text = string.Empty;
                    }
                }
            }

            if (skillButtons != null)
            {
                for (var i = 0; i < skillButtons.Count; i++)
                {
                    if (skillButtons[i] == null) continue;
                    skillButtons[i].interactable = i < count;
                }
            }
        }
        
        private static string FormatCount(int current, int max)
        {
            return max > 0 ? $"{current}/{max}" : $"{current}";
        }

        private static string FormatSpirit(int current)
        {
            return $"{current}";
        }

        private void UpdateSpirit(int targetSpirit)
        {
            if (textSpirit == null) return;

            _spiritTween?.Kill();
            _spiritStyleTween?.Kill();

            var startValue = _displaySpirit;
            _displaySpirit = targetSpirit;
            textSpirit.text = FormatSpirit(_displaySpirit);

            if (startValue == targetSpirit)
            {
                return;
            }

            if (targetSpirit > startValue)
            {
                PlayIncreaseStyle();
            }
            else
            {
                PlayDecreaseStyle();
            }
        }

        private void OnDestroy()
        {
            _spiritTween?.Kill();
            _spiritStyleTween?.Kill();
        }

        public void PlaySpiritNotEnough()
        {
            if (textSpirit == null) return;
            _spiritTween?.Kill();
            _spiritStyleTween?.Kill();

            PlayNotEnoughStyle();
        }

        private void PlayIncreaseStyle()
        {
            var rect = textSpirit.rectTransform;
            var targetScale = _baseSpiritScale * spiritPulseScale;
            _spiritStyleTween = DOTween.Sequence()
                .Append(rect.DOScale(targetScale, 0.08f))
                .Join(rect.DOAnchorPosY(rect.anchoredPosition.y + spiritIncreaseNudge, 0.08f))
                .Join(textSpirit.DOFade(1f, 0.06f))
                .Append(textSpirit.DOFade(spiritFlickerAlpha, 0.03f))
                .Append(textSpirit.DOFade(_baseSpiritColor.a, 0.05f))
                .Join(rect.DOAnchorPosY(rect.anchoredPosition.y, 0.08f))
                .Join(rect.DOScale(_baseSpiritScale, 0.1f));
        }

        private void PlayDecreaseStyle()
        {
            var rect = textSpirit.rectTransform;
            _spiritStyleTween = DOTween.Sequence()
                .Append(rect.DOAnchorPosY(rect.anchoredPosition.y + spiritDecreaseNudge, 0.08f))
                .Join(textSpirit.DOFade(spiritFlickerAlpha, 0.05f))
                .Join(rect.DOShakeAnchorPos(0.16f, spiritNotEnoughShake, 16, 90, false, true))
                .Append(textSpirit.DOFade(_baseSpiritColor.a, 0.06f))
                .Join(rect.DOAnchorPosY(rect.anchoredPosition.y, 0.1f));
        }

        private void PlayNotEnoughStyle()
        {
            var rect = textSpirit.rectTransform;
            _spiritStyleTween = DOTween.Sequence()
                .Append(textSpirit.DOFade(spiritFlickerAlpha, 0.05f))
                .Join(rect.DOShakeAnchorPos(0.25f, spiritNotEnoughShake, 22, 90, false, true))
                .Append(textSpirit.DOFade(_baseSpiritColor.a, 0.05f))
                .Join(rect.DOAnchorPos(Vector2.zero, 0.01f));
        }
    }
}