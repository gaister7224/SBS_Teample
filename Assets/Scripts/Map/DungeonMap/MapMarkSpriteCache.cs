using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Resources/MapMarks 스프라이트를 한 번 로드해 캐시합니다.
/// </summary>
public static class MapMarkSpriteCache
{
    static readonly Dictionary<string, Sprite> SpritesByName = new();
    static bool loaded;

    public static Sprite Get(string fileName)
    {
        EnsureLoaded();
        return SpritesByName.TryGetValue(fileName, out var sprite) ? sprite : null;
    }

    public static void EnsureLoaded()
    {
        if (loaded)
            return;

        loaded = true;
        SpritesByName.Clear();

        foreach (var sprite in Resources.LoadAll<Sprite>("MapMarks"))
            Register(sprite.name, sprite);

        foreach (var texture in Resources.LoadAll<Texture2D>("MapMarks"))
        {
            if (SpritesByName.ContainsKey(texture.name))
                continue;

            Register(texture.name, CreateSprite(texture));
        }
    }

    static void Register(string name, Sprite sprite)
    {
        if (sprite != null && !string.IsNullOrEmpty(name))
            SpritesByName[name] = sprite;
    }

    static Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
            return null;

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
    }
}
