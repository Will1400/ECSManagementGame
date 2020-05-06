using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using Unity.Entities;

public class PauseMenuUIManager : MonoBehaviour
{
    public static PauseMenuUIManager Instance;

    [SerializeField]
    GameObject pauseMenuPanel;
    [SerializeField]
    GameObject menuPanel;
    [SerializeField]
    Image backgroundOverlay;

    SelectionWindowUISystem windowSystem;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        windowSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SelectionWindowUISystem>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !BuildUIManager.Instance.IsActive &&
            (GameManager.Instance.CursorState == CursorState.None || GameManager.Instance.CursorState == CursorState.Menu))
        {
            Debug.Log(BuildUIManager.Instance.IsActive);
            ToggleWindow();
        }
    }


    public void CloseWindow()
    {
        GameManager.Instance.CursorState = CursorState.None;
        menuPanel.transform.DOScale(.1f, .1f);
        pauseMenuPanel.SetActive(false);
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
