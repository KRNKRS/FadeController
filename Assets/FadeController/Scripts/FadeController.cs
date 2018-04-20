using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class FadeController : MonoBehaviour {

    private const string COROUTINE_NAME = "AlphaTransition";

    [SerializeField]
    private Color m_color = Color.black;
    private UnityEvent m_callBack;
    private static FadeController s_instance;
    private static Image s_fadePanel;
    private static Canvas s_canvasComp;
    private bool m_isCallBackValid;

    private FadeController(){
        IsFinish = false;
        m_callBack = new UnityEvent();
        m_isCallBackValid = false;
    }

    public void FadeIn(float _fadeTime) {
        SetColor(Color.black);
        StartCoroutine(COROUTINE_NAME, -(_fadeTime));
    }

    public void FadeOut(float _fadeTime) {
        SetColor(Color.black);
        StartCoroutine(COROUTINE_NAME, _fadeTime);
    }

    public void FadeIn(float _fadeTime, Color _color) {
        SetColor(_color);
        FadeIn(_fadeTime);
    }

    public void FadeOut(float _fadeTime, Color _color) {
        SetColor(_color);
        FadeOut(_fadeTime);
    }

    public void FadeIn(float _fadeTime, UnityAction _finishedCallBack) {
        m_callBack.AddListener(_finishedCallBack);
        m_isCallBackValid = true;
        FadeIn(_fadeTime);
    }

    public void FadeOut(float _fadeTime, UnityAction _finishedCallBack) {
        m_callBack.AddListener(_finishedCallBack);
        m_isCallBackValid = true;
        FadeOut(_fadeTime);
    }

    public void FadeIn(float _fadeTime, Color _color, UnityAction _finishedCallBack) {
        SetColor(_color);
        FadeIn(_fadeTime, _finishedCallBack);
    }

    public void FadeOut(float _fadeTime, Color _color, UnityAction _finishedCallBack) {
        SetColor(_color);
        FadeOut(_fadeTime, _finishedCallBack);
    }

    public void SetColor(Color _color) {
        m_color = _color;
    }

    public void SetSortingOrder(int _sortingOrder) {
        s_canvasComp.sortingOrder = _sortingOrder;
    }

    private IEnumerator AlphaTransition(float _fadeTime) {
        float alfa = _fadeTime < 0 ? 1.0f : 0.0f;
        IsFinish = false;

        while (0.0f <= alfa && alfa <= 1.0f) {
            float delta = Time.deltaTime;
            alfa += delta / _fadeTime;
            s_fadePanel.color = new Color(m_color.r, m_color.g, m_color.b, alfa);
            yield return new WaitForSeconds(delta);
        }

        IsFinish = true;
        
        if (m_isCallBackValid) {
            m_callBack.Invoke();
            m_callBack.RemoveAllListeners();
            m_isCallBackValid = false;
        }
    }

    public bool IsFinish { private set; get; }

    public static FadeController Instance {
        get {
            if (s_instance == null) {

                GameObject controllerObj = new GameObject("FadeControllerObject");
                s_instance = controllerObj.AddComponent<FadeController>();
                DontDestroyOnLoad(controllerObj);

                GameObject canvasObj = new GameObject("FadeCnavas");
                s_canvasComp = canvasObj.AddComponent<Canvas>();
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                canvasObj.transform.SetParent(controllerObj.transform);

                GameObject fadePanelObj = new GameObject("FadePanel");
                fadePanelObj.AddComponent<CanvasRenderer>();
                fadePanelObj.transform.SetParent(canvasObj.transform);
                s_fadePanel = fadePanelObj.AddComponent<Image>();

                s_canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
                s_fadePanel.rectTransform.anchorMin = Vector2.zero;
                s_fadePanel.rectTransform.anchorMax = Vector2.one;
                s_fadePanel.rectTransform.sizeDelta = Vector2.zero;
            }
            return s_instance;
        }
    }
}