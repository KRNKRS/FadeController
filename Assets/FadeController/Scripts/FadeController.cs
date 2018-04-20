using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Assertions;

public class FadeController : MonoBehaviour {

    private UnityEvent m_callBack;
    private static Image s_fadePanel;
    private static Canvas s_canvasComp;
    private static bool s_isValid = false;
    private static bool s_isCreate = false;
    private bool m_isCallBackValid;

    private delegate void SetColorQueue(Color _color);
    private SetColorQueue m_setColorQueue;
    private Color m_colorBuff;

    private delegate void SetSortingOrderQueue(int _sortingOrder);
    private SetSortingOrderQueue m_setSortingOrderQueue;
    private int m_sortingOrderBuff;

    private static Action s_queue;

    private FadeController() { }

    private void Start() {
        IsFinish = false;
        m_callBack = new UnityEvent();
        m_isCallBackValid = false;
        s_queue = QueueInvoke;
    }

    public void FadeIn(float _fadeTime) {
        StartCoroutine(AlphaTransition(-(_fadeTime)));
    }

    public void FadeOut(float _fadeTime) {
        StartCoroutine(AlphaTransition(_fadeTime));
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
        if (s_fadePanel == null) {
            m_colorBuff = _color;
            m_setColorQueue = SetColor;
        } else {
            s_fadePanel.color = _color;
        }
    }

    public void SetSortingOrder(int _sortingOrder) {
        if (s_canvasComp == null) {
            m_sortingOrderBuff = _sortingOrder;
            m_setSortingOrderQueue = SetSortingOrder;
        } else {
            s_canvasComp.sortingOrder = _sortingOrder;
        }
    }

    public static void CreateInstance(MonoBehaviour _caller) {
        Assert.IsFalse(s_isCreate, "Instance is already exsists. Multiple create is not allow.");
        if (!s_isCreate) {
            GameObject controllerObj = new GameObject("FadeControllerObject");
            Instance = controllerObj.AddComponent<FadeController>();
            DontDestroyOnLoad(controllerObj);
            s_isCreate = true;
            _caller.StartCoroutine(CreateCanvasObjects(controllerObj));
        }
    }

    public static void DestroyInstance() {
        Assert.IsTrue(s_isValid, "Instance is null. You need create instance by CreateInstance(MonoBehaviour _caller)");
        if (s_isValid) {
            GameObject obj = Instance.gameObject;
            Instance = null;
            s_isValid = false;
            Destroy(obj);
        }
    }

    private void QueueInvoke() {
        if (m_setColorQueue != null) {
            m_setColorQueue.Invoke(m_colorBuff);
            m_setColorQueue = null;
        }
        if (m_setSortingOrderQueue != null) {
            m_setSortingOrderQueue.Invoke(m_sortingOrderBuff);
            m_setSortingOrderQueue = null;
        }
    }

    public static FadeController Instance {
        private set; get;
    }

    private static IEnumerator CreateCanvasObjects(GameObject _parnet) {
        yield return null;

        GameObject canvasObj = new GameObject("FadeCanvas");
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        canvasObj.transform.SetParent(_parnet.transform);
        s_canvasComp = canvasObj.GetComponent<Canvas>();
        s_canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
        yield return null;

        GameObject fadePanelObj = new GameObject("FadePanel");
        fadePanelObj.AddComponent<CanvasRenderer>();
        fadePanelObj.transform.SetParent(canvasObj.transform);
        s_fadePanel = fadePanelObj.AddComponent<Image>();
        s_fadePanel.rectTransform.anchorMin = Vector2.zero;
        s_fadePanel.rectTransform.anchorMax = Vector2.one;
        s_fadePanel.rectTransform.sizeDelta = Vector2.zero;
        s_fadePanel.rectTransform.anchoredPosition = Vector3.zero;
        Color panelColor = s_fadePanel.color;
        s_fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0);

        s_isValid = true;
        s_queue.Invoke();
    }

    private IEnumerator AlphaTransition(float _fadeTime) {

        while (!s_isValid) { yield return new WaitForEndOfFrame(); }

        float alfa = _fadeTime < 0 ? 1.0f : 0.0f;
        IsFinish = false;

        Color color = s_fadePanel.color;
        while (0.0f <= alfa && alfa <= 1.0f) {
            alfa += Time.deltaTime / _fadeTime;
            s_fadePanel.color = new Color(color.r, color.g, color.b, alfa);
            yield return new WaitForEndOfFrame();
        }

        IsFinish = true;

        if (m_isCallBackValid) {
            m_callBack.Invoke();
            m_callBack.RemoveAllListeners();
            m_isCallBackValid = false;
        }
    }

    public bool IsFinish { private set; get; }
}