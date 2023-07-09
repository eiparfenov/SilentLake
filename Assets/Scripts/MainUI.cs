using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class MainUI: MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform mainCameraMenuPosition;
    [SerializeField] private Transform mainCameraGamePlayPosition;
    [Space]
    [SerializeField] private Button startButton;
    [SerializeField] private Transform buttonStdPosition;
    [SerializeField] private Transform buttonUpPosition;
    [SerializeField] private float downAnimationTime;

    private readonly Subject<Unit> _gameStarted = new Subject<Unit>();

    public IObservable<Unit> GameStarted => _gameStarted;

    private void Awake()
    {
        startButton.onClick.AsObservable().Subscribe(_ => StartGame()).AddTo(this);
    }

    private async void StartGame()
    {
        var progress = 0f;
        while (progress <= 1f)
        {
            await UniTask.Yield(PlayerLoopTiming.Update);
            progress += Time.deltaTime / downAnimationTime;
            
            mainCamera.transform.position = Vector3.Lerp(mainCameraMenuPosition.position, mainCameraGamePlayPosition.position,
                (Mathf.Sin((progress - .5f) * Mathf.PI) + 1) / 2);
            startButton.transform.position = Vector3.Lerp(buttonStdPosition.position, buttonUpPosition.position, progress);
        }
        _gameStarted.OnNext(default);
    }
}