using UnityEngine;

/// <summary>
/// UI Image 레이캐스트용 1x1 스프라이트.
/// </summary>
public static class MapUiSpriteUtil
{
    static Sprite whiteSprite;

    public static Sprite White =>
        whiteSprite != null ? whiteSprite : whiteSprite = CreateWhiteSprite();

    static Sprite CreateWhiteSprite()
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}
