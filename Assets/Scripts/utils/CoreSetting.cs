using UnityEngine;
using Common;
public class CoreSetting : MonoSingleton<CoreSetting>
{
    public Camera UICamera;
    public Sprite[] UI_Quality_bg_sprites;
    public Sprite UI_UnKnow_sprite;
    public Vector2 Normalpos;
    public Vector2 Poppos;
    public Transform LastPanel;
    public Transform NormalParent;
    public Transform PopParent;
    public Transform MovePos;
    public Sprite GetQualityBgSprite(ObjectQuality quality)
    {
        switch (quality)
        {
            case ObjectQuality.White:
                return UI_Quality_bg_sprites[0];
            case ObjectQuality.Blue:
                return UI_Quality_bg_sprites[1];
            case ObjectQuality.Purple:
                return UI_Quality_bg_sprites[2];
            case ObjectQuality.Gold:
                return UI_Quality_bg_sprites[3];
        }
        return null;
    }
}
