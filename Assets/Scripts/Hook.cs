using System;
using UniRx;
using UnityEngine;

public class Hook : MonoBehaviour
{
    private readonly ReactiveCommand<FishMovement> _fish = new ReactiveCommand<FishMovement>();
    public IObservable<FishMovement> Fish => _fish;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var fish = other.GetComponent<FishMovement>();
        if(!fish) return;
        _fish.Execute(fish);
    }
}