using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
public class FadeCanvas : MonoBehaviour
{
    public Image fadeImage;
    [Header("事件监听")]
    public FadeEventSO fadeEvent;
    private void OnEnable()
    {
        fadeEvent.OnEventRaised += OnFadeEvent;
    }
    private void OnDisable()
    {
        fadeEvent.OnEventRaised -= OnFadeEvent;
    }
    private void OnFadeEvent(Color targetColor, float duration, bool isFadeIn)
    {
        fadeImage.DOBlendableColor(targetColor, duration);
    }
}
