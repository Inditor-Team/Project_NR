using System;
using System.Collections.Generic;
using System.Linq;
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
        //SectorManager.Instance.OnSectorClear += ProvideByUI;
    }

    private void OnDestroy()
    {
        /* if (SectorManager.Instance != null)
            SectorManager.Instance.OnSectorClear -= ProvideByUI; */
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
        //후보자 및 판별 기준 설정
        List<int> candidates = LevelCardData.Instance.LevelCardDic.Keys.ToList(); //전체 카드
        int[] myCards = InventoryManager.Instance.MyCards; //갖고 있는 카드

        //2버전 카드 목록 (2버전 id - 1 한 게 1버전입니다)
        int[] versionTwoIds = new int[4]
        {
            (int)LevelCardSO.LevelCardType.BioreinforcementB,
            (int)LevelCardSO.LevelCardType.EvasionB,
            (int)LevelCardSO.LevelCardType.NeuralAccelerationB,
            (int)LevelCardSO.LevelCardType.RecoveryAlgorithmB
        };

        //처음에는 2버전 카드 제거
        for (int i = 0; i < versionTwoIds.Length; i++)
            candidates.Remove(versionTwoIds[i]);

        //후보자 제거
        for (int i = 0; i < myCards.Length; i++)
        {
            int myCard = myCards[i];

            //1버전일 경우
            //해당 카드의 2버전 ID가 존재한다면
            if (versionTwoIds.Contains(myCard + 1))
            {
                candidates.Remove(myCard); //1버전 제거
                candidates.Add(myCard + 1); //2버전 포함
            }
            //2버전일 경우
            else if (versionTwoIds.Contains(myCard))
            {
                candidates.Remove(myCard - 1); //1버전 제거
                candidates.Remove(myCard); //2버전 제거
            }
            //일반 카드이고 이미 갖고 있다면 제거
            else
                candidates.Remove(myCard);
        }

        //요청 개수가 카드 개수보다 많을 경우 대비
        int resultCount = Mathf.Min(count, candidates.Count);

        List<LevelCardSO> result = new List<LevelCardSO>();

        for (int i = 0; i < resultCount; i++)
        {
            //남은 카드들의 전체 가중치
            float totalWeight = 0f;

            foreach (int element in candidates)
            {
                LevelCardSO curCard = LevelCardData.Instance.LevelCardDic[element];

                if (curCard.Weight > 0)
                    totalWeight += curCard.Weight;
            }

            //가중치가 남은 카드가 없으면 종료
            if (totalWeight <= 0f)
                break;

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float accumulatedWeight = 0f;

            int selectedCard = -1;

            foreach (int element in candidates)
            {
                LevelCardSO curCard = LevelCardData.Instance.LevelCardDic[element];

                if (curCard.Weight <= 0)
                    continue;

                accumulatedWeight += curCard.Weight;

                if (randomValue <= accumulatedWeight)
                {
                    selectedCard = element;
                    break;
                }
            }

            //선택되지 않았다면 종료
            if (selectedCard == -1)
                break;

            result.Add(LevelCardData.Instance.LevelCardDic[selectedCard]);

            //중복 방지를 위해 후보 목록에서 제거
            candidates.Remove(selectedCard);
        }

        return result.ToArray();
    }
}
