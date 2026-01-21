using Game.Scripts.View.Core;
using JetBrains.Lifetimes;
using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.View.Gameplay.Packshot
{
    public class HexaPackshotPresenter : MonoPresenter
    {
        [Header("UI Elements")]
        [SerializeField] private GameObject _packshotPanel;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _backgroundButton;
        [SerializeField] private Image _logoImage;
        
        [Header("Animation Settings")]
        [SerializeField, Range(0.1f, 2f)] private float _fadeInDuration = 0.5f;
        [SerializeField, Range(0.5f, 3f)] private float _logoScaleDuration = 0.8f;

        private void Awake()
        {
            if (_packshotPanel != null)
                _packshotPanel.SetActive(false);
        }

        protected override void Setup(Lifetime lifetime)
        {
            var protocol = ProtocolDispatcher.Get<HexaPackshotProtocol>(lifetime);

            protocol.IsVisible.Advise(lifetime, isVisible =>
            {
                if (isVisible)
                {
                    ShowPackshot(lifetime, protocol);
                }
                else
                {
                    HidePackshot();
                }
            });
        }

        private void ShowPackshot(Lifetime lifetime, HexaPackshotProtocol protocol)
        {
            if (_packshotPanel == null) return;

            _packshotPanel.SetActive(true);

            // Fade in из альфы
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                Tween.Alpha(_canvasGroup, 1f, _fadeInDuration);
            }

            // Анимация логотипа
            if (_logoImage != null)
            {
                _logoImage.transform.localScale = Vector3.zero;
                Tween.Scale(
                    _logoImage.transform,
                    Vector3.one,
                    _logoScaleDuration,
                    Ease.OutBack
                );
            }

            // Привязка кнопок
            if (_playButton != null)
            {
                _playButton.BindTo(lifetime, protocol.PlayButtonCommand);
            }

            if (_backgroundButton != null)
            {
                _backgroundButton.BindTo(lifetime, protocol.AnyClickCommand);
            }
        }

        private void HidePackshot()
        {
            if (_packshotPanel != null)
                _packshotPanel.SetActive(false);
        }
    }
}

