using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FisherUi: MonoBehaviour
{
    [SerializeField] private Image fishProcessImage;
    [SerializeField] private Image topLineImage;
    [SerializeField] private Image bottomLineImage;
    [SerializeField] private Gradient alarmGradient;

    public ReactiveProperty<float> FishingProcess { get; } = new ReactiveProperty<float>();
    public ReactiveProperty<float> CurrentFishingTargetTop { get; } = new ReactiveProperty<float>();
    public ReactiveProperty<float> CurrentFishingTargetBottom { get; } = new ReactiveProperty<float>();
    public ReactiveProperty<float> Alarm { get; } = new ReactiveProperty<float>();
    public ReactiveProperty<bool> Active { get; } = new ReactiveProperty<bool>();


    public void Awake()
    {
        FishingProcess.Subscribe(process => fishProcessImage.fillAmount = process * .25f).AddTo(this);
        CurrentFishingTargetTop.Subscribe(rotation =>
            topLineImage.transform.rotation = Quaternion.Euler(0, 0, -90 * Mathf.Clamp01(rotation))).AddTo(this);
        CurrentFishingTargetBottom.Subscribe(rotation =>
            bottomLineImage.transform.rotation = Quaternion.Euler(0, 0, -90 * Mathf.Clamp01(rotation))).AddTo(this);
        Alarm.Subscribe(alarm => fishProcessImage.color = alarmGradient.Evaluate(alarm)).AddTo(this);

        Active.Subscribe(active =>
        {
            fishProcessImage.enabled = active;
            topLineImage.enabled = active;
            bottomLineImage.enabled = active;
        }).AddTo(this);
    }
}