using UnityEngine;
using UnityEngine.UI;

public class MuteButton : MonoBehaviour
{
    public Sprite muteSprite;
    public Sprite unmuteSprite;
    private Image buttonImage;

    void Start()
    {
        buttonImage = GetComponent<Image>();
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    void OnClick()
    {
        AudioListener.volume = AudioListener.volume > 0f ? 0f : 1f;
        if (buttonImage != null)
            buttonImage.sprite = AudioListener.volume > 0f ? unmuteSprite : muteSprite; // ikonu güncelle
    }
    public void UpdateSprite()
    {
        if (buttonImage != null)
            buttonImage.sprite = AudioListener.volume > 0f ? unmuteSprite : muteSprite;
    }
}