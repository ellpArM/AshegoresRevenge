using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class TroopsField : MonoBehaviour
{
    [Header("Field Settings")]
    public List<Transform> fieldPositions = new List<Transform>();
    public Transform spawnPoint;
    public float moveDuration = 0.5f;

    private List<FightingEntity> entitiesOnField = new List<FightingEntity>();
    private Dictionary<FightingEntity, Transform> cardToPosition = new Dictionary<FightingEntity, Transform>();
    private HashSet<Transform> occupiedPositions = new HashSet<Transform>();

    public IEnumerator AddEntity(FightingEntity entity, bool setPosition = true)
    {
        if (entity == null)
            yield break;

        Transform freeSlot = FindFreePosition();
        if (freeSlot == null)
        {
            Debug.LogWarning("No free position available on TroopsField!");
            yield break;
        }

        entitiesOnField.Add(entity);
        cardToPosition[entity] = freeSlot;
        occupiedPositions.Add(freeSlot);

        // Place card at spawn point and animate move to target
        if(setPosition)
            entity.transform.position = spawnPoint.position;
        yield return StartCoroutine(MoveCardToPosition(entity, freeSlot.position));
        entity.troopsField = this;
    }
    public void SetPositions(bool forPlayer)
    {
        SpawnPointEntity[] spawners = FindObjectsByType<SpawnPointEntity>(FindObjectsSortMode.None);
        FieldPositionEntity[] positions = FindObjectsByType<FieldPositionEntity>(FindObjectsSortMode.None);
        if (forPlayer)
        {
            spawnPoint = spawners.Where(x => x.team == SpawnPointEntity.Team.Player).FirstOrDefault().transform;
            fieldPositions = positions.Where(x => x.team == FieldPositionEntity.Team.Player).OrderBy(x => x.transform.position.x).Select(x => x.transform).ToList();
        }
        else
        {
            spawnPoint = spawners.Where(x => x.team == SpawnPointEntity.Team.Enemy).FirstOrDefault().transform;
            fieldPositions = positions.Where(x => x.team == FieldPositionEntity.Team.Enemy).OrderByDescending(x => x.transform.position.x).Select(x => x.transform).ToList();
        }
    }
    public IEnumerator ReasignPositions(FightingEntity card)
    {
        Transform freeSlot = FindFreePosition();
        if (freeSlot == null)
        {
            Debug.LogWarning("No free position available on TroopsField!");
            yield break;
        }
        cardToPosition[card] = freeSlot;
        occupiedPositions.Add(freeSlot);
        card.transform.position = spawnPoint.position;
        yield return StartCoroutine(MoveCardToPosition(card, freeSlot.position));
    }

    internal void AddSummonedCard(FightingEntity minionCard)
    {
        entitiesOnField.Add(minionCard);
        minionCard.troopsField = this;
    }

    public void RemoveCard(FightingEntity card)
    {
        if (card == null)
            return;

        if (cardToPosition.ContainsKey(card))
        {
            occupiedPositions.Remove(cardToPosition[card]);
            cardToPosition.Remove(card);
        }

        entitiesOnField.Remove(card);
    }
    private Transform FindFreePosition()
    {
        foreach (var pos in fieldPositions)
        {
            if (!occupiedPositions.Contains(pos))
                return pos;
        }
        return null;
    }

    private IEnumerator MoveCardToPosition(FightingEntity card, Vector3 targetPosition)
    {
        Vector3 startPosition = card.transform.position;
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            yield return null;
        }

        card.transform.position = targetPosition;
    }

    public List<FightingEntity> GetEntities()
    {
        return entitiesOnField;
    }

    private IEnumerator MoveCard(FightingEntity card, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = card.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            card.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        card.transform.position = targetPosition;
    }

    internal void ClearPositions()
    {
        occupiedPositions.Clear();
    }

    public bool AllHeroesDefeated()
    {
        // Extract only the HeroInstances
        var heroes = entitiesOnField
            .Select(c => c as HeroEntity)
            .Where(h => h != null)
            .ToList();

        if (heroes.Count == 0)
            return false;

        return heroes.All(h => h.isDefeated);

        //return cardsOnField.Count > 0 && cardsOnField.Where(x => x as HeroCard && !(x as HeroInstance).isDefeated).Count() == 0;
    }
}
