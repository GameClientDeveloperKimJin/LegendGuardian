using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardSO",menuName = "SO/RewardSO")]
public class RewardSO : ScriptableObject
{
    public enum RewardType
    {
        Object,
        Food,
        Badge,
    }
    public RewardType Type; //보상 타입 -> 물건, 음식 , 뱃지

    public string RewardName; // 보상 이름

    public int RequiredScore; //보상을 받기 위한 점수
}
