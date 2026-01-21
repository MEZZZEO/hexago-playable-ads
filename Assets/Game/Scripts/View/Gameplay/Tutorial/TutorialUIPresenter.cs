using Game.Scripts.View.Core;
using Game.Utilities.Lifetimes.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using PrimeTween;
using R3;
using UnityEngine;

namespace Game.View.Gameplay.Tutorial
{
    public class HexaTutorialUIPresenter : MonoPresenter
    {
        [Header("Hand Elements")]
        [SerializeField] private GameObject _handContainer;
        [SerializeField] private RectTransform _handTransform;
        [SerializeField] private CanvasGroup _handCanvasGroup;
        
        [Header("Animation Settings")]
        [SerializeField, Range(0.5f, 3f)] private float _animationDuration = 1.5f;
        [SerializeField, Range(0.1f, 1f)] private float _fadeDuration = 0.3f;
        [SerializeField] private AnimationCurve _movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Camera _camera;
        private Tween _movementTween;
        private Tween _fadeTween;

        private void Awake()
        {
            _camera = Camera.main;
        }

        protected override void Setup(Lifetime lifetime)
        {
            var protocol = ProtocolDispatcher.Get<HexaTutorialUIProtocol>(lifetime);

            protocol.IsActive.WhenTrue(lifetime, tutorialLifetime =>
            {
                protocol.ShowHand.Advise(tutorialLifetime, showHand =>
                {
                    if (showHand)
                    {
                        ShowHand(tutorialLifetime, protocol);
                    }
                    else
                    {
                        HideHand();
                    }

                    tutorialLifetime.OnTermination(HideHand);
                });
            });


        }

        private void ShowHand(Lifetime lifetime, HexaTutorialUIProtocol protocol)
        {
            _handCanvasGroup.alpha = 0f;
            _fadeTween.Stop();
            _fadeTween = Tween.Alpha(_handCanvasGroup, 1f, _fadeDuration);

            StartHandAnimation(lifetime, protocol);
        }

        private void HideHand()
        {
            _movementTween.Stop();
            _fadeTween.Stop();
            
            _fadeTween = Tween.Alpha(_handCanvasGroup, 0f, _fadeDuration);
        }

        private void StartHandAnimation(Lifetime lifetime, HexaTutorialUIProtocol protocol)
        {
            Observable.EveryUpdate()
                .Subscribe(_ => UpdateHandAnimation(protocol))
                .AddTo(lifetime);
        }

        private void UpdateHandAnimation(HexaTutorialUIProtocol protocol)
        {
            if (_handTransform == null || _camera == null) return;

            var startScreenPos = _camera.WorldToScreenPoint(protocol.HandStartPosition.Value);
            var endScreenPos = _camera.WorldToScreenPoint(protocol.HandEndPosition.Value);

            var t = Mathf.PingPong(Time.time / _animationDuration, 1f);
            t = _movementCurve.Evaluate(t);

            var currentPos = Vector3.Lerp(startScreenPos, endScreenPos, t);
            _handTransform.position = currentPos;
        }
    }
}