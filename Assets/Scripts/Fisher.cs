using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

public class Fisher: MonoBehaviour
{
    #region Fishing
    [SerializeField] private float additionalVelocity;
    [SerializeField] private Hook hook;
    [SerializeField] private float targetDelta;
    [SerializeField] private float fishingProgressUpTime;
    [SerializeField] private float fishingProgressDownTime;
    [SerializeField] private float caughtDistance;
    [SerializeField] private float caughtAlarmDistance;
    [SerializeField] private FisherUi fisherUi;
    #endregion
    [Space]

    #region Animation
    [SerializeField] private Transform boat;
    [SerializeField] private Transform boatStdPosition;
    [SerializeField] private Transform boatDownPosition;
    [SerializeField] private Transform boatLeftPosition;
    [SerializeField] private float boatRotatePart;
    [SerializeField] private float boatDownTime;
    [SerializeField] private float boatLeftTime;
    #endregion

    private FishMovement _currentFish;
    private float _fishingTarget;

    private CompositeDisposable _fishDashSubscription;
    
    private void Awake()
    {
        hook.Fish.Subscribe(fish =>
        {
            _currentFish = fish;
            _currentFish.Dashed.Subscribe(Respawn).AddTo(_fishDashSubscription);
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

    private async void Respawn(Vector2 dashDirection)
    {
        if(Vector2.Angle(dashDirection, Vector2.down) > 5f || Mathf.Abs(_fishingTarget - 1) > .1f) return;
        
        var progress = 0f;
        while (progress <= 1f)
        {
            progress += Time.deltaTime / boatDownTime;
            boat.position = Vector3.Lerp(boatStdPosition.position, boatDownPosition.position, progress);
            boat.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 90, progress / boatRotatePart));
            await UniTask.Yield(PlayerLoopTiming.Update);
        }

        boat.rotation = Quaternion.identity;
        boat.position = boatLeftPosition.position;
        progress = 0f;
        while (progress <= 1f)
        {
            progress += Time.deltaTime / boatDownTime;
            boat.position = Vector3.Lerp(boatLeftPosition.position, boatStdPosition.position, progress);
        }
    }
}