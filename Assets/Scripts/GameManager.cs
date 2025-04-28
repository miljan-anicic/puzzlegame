using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject gameCanvas;
    public GameObject handPanel;       // UI panel to hold sliced pieces
    public GameObject tutorialPanel;   // UI panel to display the tutorial
    public TextMeshProUGUI tutorialText; // Text component inside the tutorial panel

    public Sprite img1;
    public Sprite img2;
    public Sprite img3;

    public GameObject piecePrefab;

    private int gridRows;
    private int gridCols;

    void Start()
    {
        menuCanvas.SetActive(true);
        gameCanvas.SetActive(false);
        tutorialPanel.SetActive(false);
    }

    public void StartEasyGame()
    {
        SetupGame(3, 4, showCorrect: true, showIncorrect: true, penaltyTime: 0.5f, allowRotation: false);
    }

    public void StartMediumGame()
    {
        SetupGame(4, 5, showCorrect: false, showIncorrect: true, penaltyTime: 1f, allowRotation: false);
    }

    public void StartHardGame()
    {
        SetupGame(5, 6, showCorrect: false, showIncorrect: false, penaltyTime: 2f, allowRotation: true);
    }

    private void SetupGame(int rows, int cols, bool showCorrect, bool showIncorrect, float penaltyTime, bool allowRotation)
    {
        menuCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        tutorialPanel.SetActive(true);

        // Set mode-specific tutorial message.
        if (showCorrect && showIncorrect)
            tutorialText.text = "Easy: Correct slots light up green,  wrong ones red. Press Space to start.";
        else if (!showCorrect && showIncorrect)
            tutorialText.text = "Medium: Only wrong moves are highlighted. Press Space to begin.";
        else if (!showCorrect && !showIncorrect)
            tutorialText.text = "Hard: No visual feedback. Use rotations wisely. Press Space to start.";

        gridRows = rows;
        gridCols = cols;

        Sprite selectedImage = new[] { img1, img2, img3 }[Random.Range(0, 3)];
        Sprite[,] slicedPieces = SliceImage(selectedImage, rows, cols);
        SpawnPieces(slicedPieces);
    }

    private Sprite[,] SliceImage(Sprite image, int rows, int cols)
    {
        Texture2D texture = image.texture;
        int pieceWidth = texture.width / cols;
        int pieceHeight = texture.height / rows;
        Sprite[,] pieces = new Sprite[rows, cols];

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Rect rect = new Rect(x * pieceWidth, y * pieceHeight, pieceWidth, pieceHeight);
                Vector2 pivot = new Vector2(0.5f, 0.5f);
                pieces[y, x] = Sprite.Create(texture, rect, pivot);
            }
        }
        return pieces;
    }

    private void SpawnPieces(Sprite[,] pieces)
    {
        // Clear previous pieces if any.
        foreach (Transform child in handPanel.transform)
            Destroy(child.gameObject);

        for (int y = 0; y < pieces.GetLength(0); y++)
        {
            for (int x = 0; x < pieces.GetLength(1); x++)
            {
                GameObject piece = Instantiate(piecePrefab, handPanel.transform);
                piece.GetComponent<Image>().sprite = pieces[y, x];
                // Optionally store puzzle piece info like original grid position.
            }
        }
    }

    void Update()
    {
        if (tutorialPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            tutorialPanel.SetActive(false);
            // The game officially starts now.
        }
    }
}
