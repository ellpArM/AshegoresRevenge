using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RedMageEnrage : BaseSkill
{
    public int incValue = 2;
    public AudioClip effectSound;
    public GameObject effectPrefab;
    HeroEntity mainHero;
    private void Start()
    {
        mainHero = transform.root.GetComponent<HeroEntity>();
    }
    public override IEnumerator Execute()
    {
        yield return BattleManagerNew.Instance.StartCoroutine(ApplyRageBuff());
        BattleManagerNew.Instance.NotifyPlayerSkillUsed();
    }
    private IEnumerator ApplyRageBuff()
    {
        BattleManagerNew.Instance.SetPlayerInput(false);

        InfoPanel.instance.ShowMessage("Empowering...");

        InfoPanel.instance.Hide();

        yield return new WaitForSeconds(0.1f);

        // Apply passive to all allied heroes
        List<HeroEntity> heroes = BattleManagerNew.Instance.PlayerHeroes;

        RagePassive existing = mainHero.GetComponent<RagePassive>();
        if (existing == null)
        {
            existing = mainHero.AddComponent<RagePassive>();
        }
        existing.AddRage(incValue);

        GameObject effectObj = Instantiate(effectPrefab, mainHero.transform.position, Quaternion.identity);
        Destroy(effectObj , 1);

        if (effectSound != null)
            EffectsManager.instance.CreateSoundEffect(effectSound, mergePoint);

        BattleManagerNew.Instance.SetPlayerInput(true);
    }
}
