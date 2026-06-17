using UnityEngine;

public class ConveyorScroller : MonoBehaviour
{
    [Tooltip("Speed and direction of the conveyor belt. Use negative values to reverse.")]
    public float scrollSpeed = 0.5f;

    private Renderer rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the renderer attached to this object
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // Scroll the texture based on the scroll speed and axis
        // Calculate the offset based on time and speed
        float offset = Time.time * scrollSpeed;

        // Apply the offset to the material's main texture and scroll the texture
        rend.material.mainTextureOffset = new Vector2(0, offset);
    }
}
