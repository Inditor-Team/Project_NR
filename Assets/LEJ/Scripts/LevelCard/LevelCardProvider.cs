using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어에게 레벨 카드를 제공합니다
/// </summary>
public class LevelCardProvider : MonoBehaviour
{
    [SerializeField] LevelCardUI ui;
    PlayerStat stat; 

    private int cardCount = 3;

    private void Start()
    {
        stat = GameManager.Instance.Player.GetComponent<PlayerController>().Stat;
        SectorManager.Instance.OnSectorClear += ProvideByUI;
    }

    private void OnDestroy()
    {
        if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= ProvideByUI;
    }

    void ProvideByUI(SectorSO.SectorType type)
    {
        ProvideByUI();
    }

    public void ProvideByUI()
    {
        if (ui == null)
            return;

        //카드 데이터 중 3개의 카드를 선점
        LevelCardSO[] choosen = ChooseLevelCard(cardCount);

        //선점된 카드를 ui 로 제공
        if (ui == null)
            return;

        for (int i = 0; i < choosen.Length; i++)
        {
            Action onClickAction = null;
            int index = i;

            //버튼을 눌렀을 때 InventoryManager 에 카드 획득 등록 및 UI 종료
            onClickAction += () => {
                InventoryManager.Instance.GetCard(choosen[index].Id);
                ui.CloseUI();
            }; 

            //ui 에게 설정을 명령
            ui.SetUIElement(choosen[i], i, onClickAction);
        }

        GameManager.Instance.RequestPause();// Pause(true);

        ui.ShowUI();
    }

    /// <summary>
    /// 확률에 따라 레벨 카드를 제공
    /// </summary>
    LevelCardSO[] ChooseLevelCard(int count)
    {
        //뽑을 수 있는 카드 목록
        List<LevelCardSO> candidates = new List<LevelCardSO>(LevelCardData.Instance.LevelCards);

        //요청 개수가 카드 개수보다 많아도 안전하게 처리
        int resultCount = Mathf.Min(count, candidates.Count);

        List<LevelCardSO> result = new List<LevelCardSO>();

        for (int i = 0; i < resultCount; i++)
        {
            //남은 카드들의 전체 가중치
            float totalWeight = 0f;

            foreach (LevelCardSO card in candidates)
            {
                if (card.Weight > 0)
                    totalWeight += card.Weight;
            }

            //가중치가 남은 카드가 없으면 종료
            if (totalWeight <= 0f)
                break;

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            LevelCardSO selectedCard = null;

            foreach (LevelCardSO card in candidates)
            {
                if (card.Weight <= 0)
                    continue;

                accumulatedWeight += card.Weight;

                if (randomValue <= accumulatedWeight)
                {
                    selectedCard = card;
                    break;
                }
            }

            //선택되지 않았다면 종료
            if (selectedCard == null)
                break;

            result.Add(selectedCard);

            //중복 방지를 위해 후보 목록에서 제거
            candidates.Remove(selectedCard);
        }

        return result.ToArray();
    }
}
