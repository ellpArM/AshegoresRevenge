using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RedMageMight : BaseSkill
{
    public AudioClip soundEffect;
    public GameObject effectPrefab;
    public override IEnumerator Execute()
    {
        yield return ApplyMightBuff();
        BattleManagerNew.Instance.NotifyPlayerSkillUsed();
    }
    public void HighLightUnits(List<FightingEntity> entities)
    {
        foreach (var e in entities)
        {
            if (e != null)
                e.ShowSelector(SelectionState.Green);
        }
    }
    public void HideHighlight(List<FightingEntity> entities)
    {
        foreach (var e in entities)
        {
            if (e != null)
                e.HideSelector();
        }
    }
    private IEnumerator ApplyMightBuff()
    {
        BattleManagerNew.Instance.SetPlayerInput(false);
        List<FightingEntity> heroes = BattleManagerNew.Instance.playerField.GetEntities();

        HighLightUnits(heroes);

        BattleManagerNew.Instance.SelectedTarget = null;
        InfoPanel.instance.ShowMessage("Select friendly hero...");

        yield return new WaitUntil(() => BattleManagerNew.Instance.SelectedTarget != null);

        HideHighlight(heroes);
        InfoPanel.instance.Hide();
        FightingEntity target = BattleManagerNew.Instance.SelectedTarget;
        BattleManagerNew.Instance.SelectTarget(null);

        yield return BattleManagerNew.Instance.StartCoroutine(PerformVisuals(target));

        //HeroEntity hero = BattleManagerNew.Instance.GetHeroOfelement(damageType);
        MightPassive might = target.AddComponent<MightPassive>();
        might.duration = 3;

        if (effectPrefab)
            Instantiate(effectPrefab, target.transform.position, Quaternion.identity);

        if (soundEffect)
            EffectsManager.instance.CreateSoundEffect(soundEffect, transform.position);

        yield return StartCoroutine(target.ResolveDeathIfNeeded());
        BattleManagerNew.Instance.SetPlayerInput(true);

        yield return null;
    }
    private IEnumerator PerformVisuals(FightingEntity target)
    {
        yield return null;
    }
}
