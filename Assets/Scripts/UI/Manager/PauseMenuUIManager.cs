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
    GameObject graphicSettingsPanel;

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
            MenuAwareBack();
        }
    }

    public void ClosePanel(GameObject panel)
    {
        panel.transform.localScale = Vector3.zero;
        panel.SetActive(false);
    }

    public void OpenPanel(GameObject panel)
    {
        if (!pauseMenuPanel.activeSelf)
            pauseMenuPanel.SetActive(true);

        GameManager.Instance.CursorState = CursorState.Menu;
        panel.SetActive(true);
        panel.transform.DOScale(1, .2f);
    }


    void MenuAwareBack()
    {
        if (graphicSettingsPanel.activeSelf)
        {
            ClosePanel(graphicSettingsPanel);
            //ClosePanel(graphicSettingsPanel);
            OpenPanel(menuPanel);
        }
        else
        {
            ToggleWindow();
        }
    }

    public void ToggleWindow()
    {
        if (pauseMenuPanel.activeSelf)
        {
            GameManager.Instance.CursorState = CursorState.None;
            menuPanel.transform.DOScale(.1f, .1f);
            pauseMenuPanel.SetActive(false);
        }
        else
        {
            OpenPanel(menuPanel);
        }
    }
}
