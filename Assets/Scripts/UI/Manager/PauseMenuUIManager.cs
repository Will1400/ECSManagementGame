using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class PauseMenuUIManager : MonoBehaviour
{
    public static PauseMenuUIManager Instance;

    [SerializeField]
    GameObject pauseMenuPanel;
    [SerializeField]
    GameObject menuPanel;
    [SerializeField]
    Image backgroundOverlay;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !BuildUIManager.Instance.IsActive &&
            (GameManager.Instance.CursorState == CursorState.None || GameManager.Instance.CursorState == CursorState.Menu))
        {
            ToggleWindow();
        }
    }


    public void CloseWindow()
    {
        GameManager.Instance.CursorState = CursorState.None;
        pauseMenuPanel.SetActive(false);
        menuPanel.transform.DOScale(.1f, .2f);
    }

    public void OpenWindow()
    {
        GameManager.Instance.CursorState = CursorState.Menu;
        pauseMenuPanel.SetActive(true);
        menuPanel.transform.DOScale(1, .2f);
    }

    public void ToggleWindow()
    {
        if (pauseMenuPanel.activeSelf)
            CloseWindow();
        else
            OpenWindow();
    }
}
