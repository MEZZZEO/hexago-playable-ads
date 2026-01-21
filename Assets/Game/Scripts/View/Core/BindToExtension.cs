using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Game.Scripts.View.Core
{
    public static class BindToExtension
    {
        public static void BindTo(this GameObject go, Lifetime lifetime)
        {
            lifetime.Bracket(
                opening: () => go.SetActive(true),
                closing: () => go.SetActive(false)
            );
        }

        public static void BindTo(this TMP_InputField input, Lifetime lifetime, IViewableProperty<string> property)
        {
            lifetime.Bracket(
                opening: () => input.onValueChanged.AddListener(ProxyValueToProperty),
                closing: () => input.onValueChanged.RemoveListener(ProxyValueToProperty)
            );

            void ProxyValueToProperty(string value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => input.text = value);
        }

        public static void BindTo(this TMP_InputField input, Lifetime lifetime, Action<string> handler)
        {
            lifetime.Bracket(
                opening: () => input.onValueChanged.AddListener(ProxyFunc),
                closing: () => input.onValueChanged.RemoveListener(ProxyFunc)
            );

            void ProxyFunc(string value)
            {
                handler?.Invoke(value);
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, ISignal<Unit> signal)
        {
            lifetime.Bracket(
                opening: () => button.onClick.AddListener(Fire),
                closing: () => button.onClick.RemoveListener(Fire)
            );

            void Fire()
            {
                signal.Fire();
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, Command command,
            Action<bool> interactionHandler = null,
            Action<bool> activeHandler = null)
        {
            command.IsInteractable.View(lifetime, (lifetime, isInteractable) =>
            {
                button.interactable = isInteractable;
                if (isInteractable)
                {
                    button.BindTo(lifetime, command.ExecuteCommand);
                }

                interactionHandler?.Invoke(isInteractable);
            });

            command.IsEnabled.Advise(lifetime, isEnabled =>
            {
                button.gameObject.SetActive(isEnabled);
                activeHandler?.Invoke(isEnabled);
            });
        }

        public static void BindTo(this Button button, Lifetime lifetime, ISignal<int> signal, int index)
        {
            lifetime.TryBracket(
                opening: () => button.onClick.AddListener(Fire),
                closing: () => button.onClick.RemoveListener(Fire)
            );

            void Fire()
            {
                signal.Fire(index);
            }
        }

        public static void BindTo(this Button button, Lifetime lifetime, Action handler)
        {
            lifetime.Bracket(
                opening: () => button.onClick.AddListener(handler.Invoke),
                closing: () => button.onClick.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Toggle toggle, Lifetime lifetime, Action<bool> handler)
        {
            lifetime.Bracket(
                opening: () => toggle.onValueChanged.AddListener(handler.Invoke),
                closing: () => toggle.onValueChanged.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Slider slider, Lifetime lifetime, Action<float> handler)
        {
            lifetime.Bracket(
                opening: () => slider.onValueChanged.AddListener(handler.Invoke),
                closing: () => slider.onValueChanged.RemoveListener(handler.Invoke)
            );
        }

        public static void BindTo(this Slider slider, Lifetime lifetime, IViewableProperty<float> property)
        {
            lifetime.Bracket(
                opening: () => slider.onValueChanged.AddListener(OnSliderValueChanged),
                closing: () => slider.onValueChanged.RemoveListener(OnSliderValueChanged)
            );

            void OnSliderValueChanged(float value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => slider.value = value);
        }

        public static void BindTo(this Toggle toggle, Lifetime lifetime, IViewableProperty<bool> property)
        {
            lifetime.Bracket(
                opening: () => toggle.onValueChanged.AddListener(OnToggleValueChanged),
                closing: () => toggle.onValueChanged.RemoveListener(OnToggleValueChanged)
            );

            void OnToggleValueChanged(bool value)
            {
                property.Value = value;
            }

            property.Advise(lifetime, value => { toggle.isOn = value; });
        }

        public static void BindTo(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<string> property)
        {
            property.Advise(lifetime, value => label.text = value);
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> property)
        {
            property.Advise(lifetime, value => label.SetText(value?.ToString()));
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> property,
            string format)
        {
            property.Advise(lifetime, value => label.SetText(string.Format(format, value)));
        }

        public static void BindTo<T>(this TMP_Text label, Lifetime lifetime, IReadonlyProperty<T> first,
            IReadonlyProperty<T> second,
            string format)
        {
            first.Compose(lifetime, second, (x, y) => label.SetText(string.Format(format, x, y)));
        }

        public static void BindTo(this Image filler, Lifetime lifetime, IReadonlyProperty<float> progressGetter)
        {
            progressGetter.Advise(lifetime, progress => filler.fillAmount = progress);
        }

        public static void BindTo(this IViewableProperty<GameObject> target, Lifetime lifetime,
            GameObject reference)
        {
            lifetime.Bracket(
                () => target.Value = Object.Instantiate(reference),
                () => target.Value = null
            );
        }

        public static void BindTo(this Image image, Lifetime lifetime, Sprite sprite)
        {
            var colorLoading = new Color(0.5f, 0.5f, 0.5f, 0.1f);
            lifetime.Bracket(() =>
                {
                    image.color = colorLoading;
                    image.sprite = null;
                    image.color = Color.white;
                    image.sprite = sprite;
                    return Task.CompletedTask;
                },
                () =>
                {
                    image.color = colorLoading;
                    image.sprite = null;
                }
            );
        }

        public static void BindTo(this TMP_Dropdown dropdown, Lifetime lifetime, List<string> variants,
            IViewableProperty<int> selected)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(variants);
            dropdown.value = selected.Maybe.ValueOrDefault;

            var onValueChanged = new UnityAction<int>(OnValueChanged);

            dropdown.onValueChanged.AddListener(onValueChanged);
            lifetime.OnTermination(() => dropdown.onValueChanged.RemoveListener(onValueChanged));

            dropdown.RefreshShownValue();

            void OnValueChanged(int value)
            {
                selected.Value = value;
            }
        }

        public static void InteractableWhileTrue(this Toggle toggle, Lifetime lifetime,
            IReadonlyProperty<bool> property)
        {
            property.Advise(lifetime, value => toggle.interactable = value);
        }
    }
}
