using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    [Tooltip("Assign the TextMeshProUGUI element that shows the score.")]
    public TextMeshProUGUI scoreText;

    void Update()
    {
        if (ScoreManager.Instance != null)
            scoreText.text = $"Score: {ScoreManager.Instance.Score}";
    }
}
