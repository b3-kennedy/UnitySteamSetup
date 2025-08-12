using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour
{


    public TextMeshProUGUI nameText;
    public Image avatarImage; // Drag in a UI Image from the prefab

    ulong playerID;

    public void SetPlayerID(ulong id)
    {
        playerID = id;
    }

    public ulong GetPlayerID()
    {
        return playerID;
    }

    public void UpdateName(string playerName)
    {
        nameText.text = playerName;
    }

    public void UpdateAvatar(Texture2D texture)
    {
        if (texture != null)
        {
            avatarImage.sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.Tight,
                Vector4.zero,
                false // <-- 'false' here flips the y-axis UV
            );
        }

    }
}
