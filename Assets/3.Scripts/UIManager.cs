using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UIManager : MonoBehaviour
{
    // 버튼을 연결할 변수
    public Button startButton;
    public Button optionButton;
    public Button shopButton;
    private UnityAction action;
    void Start ()
    {
        // UnityAction
        action = () => OnStartClick();
        startButton.onClick.AddListener(action);
        // 무명메서드
        optionButton.onClick.AddListener(delegate {OnButtonClick(optionButton.name);});
        // 람다식
        shopButton.onClick.AddListener(() => OnButtonClick(shopButton.name));
    }

    public void OnButtonClick(string msg)
    {
        Debug.Log("Click Button : {msg}");
    }

    #region Button Event
    public void OnButtonClick2(string msg)
    {
        Debug.Log($"Click Button : {msg}");
    }
    public void OnButtonClick3(int idx)
    {
        Debug.Log($"Click Button : {idx}");
    }
    public void OnButtonClick4(RectTransform rt)
    {
        Debug.Log($"Click Butto : {rt.localScale.x}");
    }
    #endregion
    public void OnStartClick()
    {
        SceneManager.LoadScene("Level_01");
        SceneManager.LoadScene("Play", LoadSceneMode.Additive);
    }
}
