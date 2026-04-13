using System.Collections;
using UnityEngine;

public class RouletteWheelController : MonoBehaviour
{
    [Header("Wheel")]
    [SerializeField] private RectTransform wheelTransform;

    [Header("Spin Settings")]
    [SerializeField] private int totalSegments = 5;
    [SerializeField] private float spinDuration = 3f;
    [SerializeField] private int extraSpins = 5;

    private bool isSpinning;

    public bool IsSpinning => isSpinning;

    public void SpinToResult(int resultIndex, System.Action onComplete = null)
    {
        if (isSpinning)
            return;

        StartCoroutine(SpinCoroutine(resultIndex, onComplete));
    }

    private IEnumerator SpinCoroutine(int resultIndex, System.Action onComplete)
    {
        isSpinning = true;

        float segmentAngle = 360f / totalSegments;

        // 결과 칸의 중심으로 멈추도록 계산
        float targetAngle = resultIndex * segmentAngle;

        // 룰렛이 여러 바퀴 돈 뒤 목표 위치에서 멈추게 설정
        float totalRotation = (360f * extraSpins) + targetAngle;

        float startZ = wheelTransform.eulerAngles.z;
        float endZ = startZ - totalRotation;

        float elapsed = 0f;

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);

            // 감속 느낌
            float easedT = 1f - Mathf.Pow(1f - t, 3f);

            float currentZ = Mathf.Lerp(startZ, endZ, easedT);
            wheelTransform.rotation = Quaternion.Euler(0f, 0f, currentZ);

            yield return null;
        }

        wheelTransform.rotation = Quaternion.Euler(0f, 0f, endZ);

        isSpinning = false;
        onComplete?.Invoke();
    }
}