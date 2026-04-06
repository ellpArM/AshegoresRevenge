using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSelector : MonoBehaviour
{
    public BaseMonsterSkill SelectedSkill { get; set; }
    public List<FightingEntity> SelectedTargets { get; set; } = new();

    private FightingEntity owner;

    private void Awake()
    {
        owner = GetComponent<FightingEntity>();
    }

    public virtual void PreselectIntent(List<FightingEntity> playerCards, List<FightingEntity> enemyCards)
    {
        ClearIntent();

        BaseMonsterSkill[] skills =
            GetComponents<BaseMonsterSkill>();

        skills = System.Array.FindAll(skills, s => s.enabled);

        if (skills.Length == 0 || playerCards.Count == 0)
            return;

        SelectedSkill = skills[Random.Range(0, skills.Length)];

        SelectedTargets = SelectedSkill.SelectTargets(playerCards, enemyCards);
    }
    public IEnumerator ExecuteIntent()
    {
        if (SelectedSkill == null)
            yield break;

        yield return SelectedSkill.ExecuteWithTargets(SelectedTargets);
        yield return SelectedSkill.PostExecute();

        ClearIntent();
    }

    public void ClearIntent()
    {
        if (SelectedSkill != null)
            SelectedSkill.HideIntentIndicators();

        SelectedSkill = null;
        SelectedTargets.Clear();
    }
    public void ShowIntent()
    {
        if (SelectedSkill == null)
            return;

        SelectedSkill.ShowIntentIndicators(SelectedTargets);
    }

    public void HideIntent()
    {
        if (SelectedSkill == null)
            return;

        SelectedSkill.HideIntentIndicators();
    }
}
