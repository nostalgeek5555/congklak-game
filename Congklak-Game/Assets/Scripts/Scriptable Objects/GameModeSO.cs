using UnityEngine;

[CreateAssetMenu(fileName = "Mode SO", menuName = "Scriptable Objects/Mode")]
public class GameModeSO : ScriptableObject
{
    public PlayerModeType playerModeType;
    public ModeType modeType;
    public GameObject playerGO;
    public GameObject enemyAiGO;

    public enum PlayerModeType
    {
        SinglePlayer = 0,
        Multiplayer = 1
    }

    public enum ModeType
    {
        Offline = 0,
        Online = 1
    }
}
