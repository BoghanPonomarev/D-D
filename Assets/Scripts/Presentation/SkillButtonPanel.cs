using System;
using System.Collections.Generic;
using Battle.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation
{
    public class SkillButtonPanel : MonoBehaviour
    {
        [SerializeField] private Button[] skillButtons;
        [SerializeField] private TextMeshProUGUI[] buttonLabels;

        public void Show(List<SkillDefinition> skills, Action<SkillDefinition> onSelect)
        {
            gameObject.SetActive(true);

            for (int i = 0; i < skillButtons.Length; i++)
            {
                bool hasSkill = i < skills.Count;
                skillButtons[i].gameObject.SetActive(hasSkill);
                if (!hasSkill) continue;

                var skill = skills[i];
                buttonLabels[i].text = string.IsNullOrEmpty(skill.Description)
                    ? skill.SkillName
                    : $"{skill.SkillName}\n<size=18>{skill.Description}</size>";

                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => onSelect(skill));
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
