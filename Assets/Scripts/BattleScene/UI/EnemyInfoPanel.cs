using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleScene.UI
{
    public class EnemyInfoPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text textName;
        [SerializeField] private TMP_Text textHp;
        [SerializeField] private TMP_Text textDodge;
        [SerializeField] private TMP_Text textSpirit;
        [SerializeField] private Image imageAvatar;
        [SerializeField] private Image imageHp;
        [SerializeField] private List<Image> skillIcons;
        [SerializeField] private List<TMP_Text> skillNames;

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
                textDodge.text = FormatCount(info.dodgeCount, info.dodgeMax);
            }

            if (textSpirit != null)
            {
                textSpirit.text = FormatSpirit(info.spiritCount);
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

            UpdateSkills(info.skills);
        }

        private void UpdateSkills(List<SkillModel> skills)
        {
            if (skillIcons == null && skillNames == null) return;

            var filtered = FilterEnemySkills(skills);
            var count = filtered.Count;

            if (skillIcons != null)
            {
                for (var i = 0; i < skillIcons.Count; i++)
                {
                    if (skillIcons[i] == null) continue;
                    skillIcons[i].enabled = i < count;
                }
            }

            if (skillNames != null)
            {
                for (var i = 0; i < skillNames.Count; i++)
                {
                    if (skillNames[i] == null) continue;
                    if (i < count)
                    {
                        skillNames[i].text = filtered[i].name;
                    }
                    else
                    {
                        skillNames[i].text = string.Empty;
                    }
                }
            }
        }

        private static List<SkillModel> FilterEnemySkills(List<SkillModel> skills)
        {
            var result = new List<SkillModel>();
            if (skills == null) return result;

            foreach (var skill in skills)
            {
                if (skill == null) continue;
                if (skill.type is SkillType.Dodge or SkillType.NormalAttack) continue;
                result.Add(skill);
            }

            return result;
        }

        private static string FormatCount(int current, int max)
        {
            return max > 0 ? $"{current}/{max}" : $"{current}";
        }

        private static string FormatSpirit(int current)
        {
            return $"{current}";
        }
    }
}