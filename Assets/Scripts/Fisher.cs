using System;
using UniRx;
using UnityEngine;

public class Fisher: MonoBehaviour
{
    [SerializeField] private float additionalVelocity;
    [SerializeField] private Hook hook;
    [SerializeField] private float targetDelta;
    [SerializeField] private float fishingProgressUpTime;
    [SerializeField] private float fishingProgressDownTime;
    [SerializeField] private float caughtDistance;
    [SerializeField] private float caughtAlarmDistance;
    [SerializeField] private FisherUi fisherUi;

    private FishMovement _currentFish;
    private float _fishingTarget;
    
    
    private void Awake()
    {
        hook.Fish.Subscribe(fish =>
        {
            _currentFish = fish;
            Destroy(hook.gameObject);
        }).AddTo(hook);
    }

    private void Update()
    {
        if (_currentFish)
        {
            var direction = _currentFish.transform.position - transform.position;
            _currentFish.AdditionalSpeed = -(direction).normalized * additionalVelocity;
            UpdateFishingRules(direction);
            if (direction.magnitude < caughtDistance)
            {
                Destroy(_currentFish.gameObject);
            }
        }
    }

    private void UpdateFishingRules(Vector2 direction)
    {
        var fishingProcess = Mathf.Clamp01(Vector2.Angle(direction, Vector2.right) / 90);
        var currentTargetDelta = targetDelta / (_fishingTarget + 1);
        if (_fishingTarget - currentTargetDelta < fishingProcess && fishingProcess < _fishingTarget + currentTargetDelta)
        {
            _fishingTarget += Time.deltaTime / fishingProgressUpTime;
        }
        else
        {
            _fishingTarget -= Time.deltaTime / fishingProgressDownTime;
        }

        _fishingTarget = Mathf.Clamp01(_fishingTarget);
        
        fisherUi.FishingProcess.Value = fishingProcess;
        fisherUi.CurrentFishingTargetTop.Value = _fishingTarget + currentTargetDelta;
        fisherUi.CurrentFishingTargetBottom.Value = _fishingTarget - currentTargetDelta;
        fisherUi.Alarm.Value = Mathf.Clamp01(Mathf.InverseLerp(caughtAlarmDistance, caughtDistance, (direction).magnitude));
    }
}