using UnityEngine;
using Unity.Cinemachine;

public class CameraLookahead : MonoBehaviour
{
    public PlayerBehaviour player;
    public CinemachinePositionComposer composer;

    [Tooltip("How far left/right the player is offset from screen center (0 = center, 0.3 = 30% off-center)")]
    public float screenOffsetX = 0.3f;
    [Tooltip("How fast the camera shifts sides when the player changes direction")]
    public float shiftSpeed = 2f;

    private float targetScreenX = 0f;

    void Update()
    {
        if (player == null || composer == null) return;

        float move = Input.GetAxisRaw("Horizontal");
        if (move > 0.1f)       targetScreenX = -screenOffsetX; // player on left, see right
        else if (move < -0.1f) targetScreenX =  screenOffsetX; // player on right, see left

        var comp = composer.Composition;
        comp.ScreenPosition.x = Mathf.Lerp(comp.ScreenPosition.x, targetScreenX, Time.deltaTime * shiftSpeed);
        composer.Composition = comp;
    }
}
