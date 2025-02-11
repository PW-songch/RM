using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZpMoveMenuUI : MonoBehaviour
{
    [Tooltip("TweenPosition용 오브젝트")]
    public GameObject m_objMove;
    public GameObject MoveObj { get { return m_objMove != null ? m_objMove : this.gameObject; } }

    [HideInInspector] public Vector3 m_ShowPos;     //  show 위치
    [Tooltip("hide 위치")]
    public Vector3 m_HidePos;
    [Tooltip("생성 후 hide 여부")]
    public bool m_isStartHide = false;

    protected bool m_bIgnoreTimeScale = true;
    protected bool m_bEnableUI = true;

    protected Color m_EnableLabelColor;
    protected Color m_DisableLabelColor;

    protected string m_strShowFinishedCallFunction = "ShowEnd";
    public string GetShowFinishedCallFunction { get { return m_strShowFinishedCallFunction; } }
    protected string m_strHideFinishedCallFunction = "HideEnd";
    public string GetHideFinishedCallFunction { get { return m_strHideFinishedCallFunction; } }

	protected bool mb_ShowEnd;
	public bool GetShowEnd
	{
		get { return mb_ShowEnd; }
	}

    protected virtual void Awake()
    {
        m_ShowPos = MoveObj.transform.localPosition;
        if (m_isStartHide == true)
            MoveObj.transform.localPosition = m_HidePos;

        m_DisableLabelColor = new Color(92.0f / 255.0f, 102.0f / 255.0f, 121.0f / 255.0f);
    }

    protected virtual void OnDestroy()
    {
        ZpEventListener.RemoveListener(this);
    }

    public virtual void Show(bool _bShow, float _fDuration, float _fDelay, GameObject _Target = null,
        GameObject _Recieve = null, string _Function = null)
    {
        if (m_objMove == null)
            m_objMove = _Target == null ? this.gameObject : _Target;

        if (_bShow)
            NGUITools.SetActiveSelf(m_objMove, true);

        if (!ZpGlobals.GlobalGUIIsNull())
        {
            string strFunction = string.IsNullOrEmpty(_Function) == false ? _Function :
                (_bShow == true ? GetShowFinishedCallFunction : GetHideFinishedCallFunction);
            ZpGlobals.GlobalGUI.ShowMenu(_bShow, m_objMove, _fDuration, _fDelay, m_objMove.transform.localPosition,
                _bShow ? m_ShowPos : m_HidePos, m_bIgnoreTimeScale, _Recieve != null ? _Recieve : this.gameObject, strFunction);
        }
    }

    public virtual void Show(bool _bShow, float _fDuration, float _fDelay, GameObject _Target, Vector3 _vFrom,
        UITweener.Method _Method, GameObject _Recieve = null, string _Function = null)
    {
        m_objMove = _Target == null ? this.gameObject : _Target;

        if (_bShow)
            NGUITools.SetActiveSelf(m_objMove, true);

        if (!ZpGlobals.GlobalGUIIsNull())
        {
            string strFunction = string.IsNullOrEmpty(_Function) == false ? _Function :
                (_bShow == true ? GetShowFinishedCallFunction : GetHideFinishedCallFunction);
            ZpGlobals.GlobalGUI.ShowMenu(_bShow, m_objMove, _fDuration, _fDelay, _vFrom, _bShow ? m_ShowPos : m_HidePos,
                _Method, m_bIgnoreTimeScale, _Recieve != null ? _Recieve : this.gameObject, strFunction);
        }
    }

    public virtual void Hide(float _fDuration, float _fDelay)
    {
        Show(false, _fDuration, _fDelay, m_objMove);
    }

    public void ImmediatelyMoveShowPosition()
    {
        MoveObj.transform.localPosition = m_ShowPos;
    }

    public void ImmediatelyMoveHidePosition()
    {
        MoveObj.transform.localPosition = m_HidePos;
    }

    public virtual void TweenAlphaAnimation(GameObject _target, float _fTartgetAlpha, float _fDuration, float _fDelay, 
        GameObject _recieve = null, string _strFunction = null)
    {
        if (_target == null && m_objMove == null)
            return;

        if (!ZpGlobals.GlobalGUIIsNull())
            ZpGlobals.GlobalGUI.TweenAlphaAnimation(_target == null ? m_objMove : _target, _fTartgetAlpha,
                _fDuration, _fDelay, UITweener.Method.EaseInOut, _recieve != null ? _recieve : this.gameObject, _strFunction);
    }

    public void FadeIn(GameObject _target, float _fDuration, float _fDelay,
        GameObject _recieve = null, string _strFunction = null)
    {
        TweenAlphaAnimation(_target, 1.0f, _fDuration, _fDelay, _recieve, _strFunction);
    }

    public void FadeOut(GameObject _target, float _fDuration, float _fDelay,
        GameObject _recieve = null, string _strFunction = null)
    {
        TweenAlphaAnimation(_target, 0.0f, _fDuration, _fDelay, _recieve, _strFunction);
    }

    public void SetIgnoreTimeScale(bool _bIgnoreTimeScale)
    {
        m_bIgnoreTimeScale = _bIgnoreTimeScale;
    }

    public virtual bool IsPossibleActive()
    {
        if (m_objMove)
        {
            Vector3 localPos = m_objMove.transform.localPosition;
            Vector3 localScale = m_objMove.transform.localScale;
            if ((localPos == m_ShowPos && localScale == Vector3.one) ||
                (localPos == m_HidePos && localScale == Vector3.zero))
                return true;
        }

        return false;
    }

    public virtual bool IsPossibleActiveCheckPosition()
    {
        if (m_objMove)
        {
            Vector3 localPos = m_objMove.transform.localPosition;
            if (localPos == m_ShowPos || localPos == m_HidePos)
                return true;
        }

        return false;
    }

    public virtual void UpdateHidePos()
    {
    }

    public virtual bool IsShowState()
    {
        if (m_objMove)
        {
            if (m_objMove.transform.localPosition == m_ShowPos && m_objMove.transform.localScale == Vector3.one)
                return true;
        }

        return false;
    }

    public virtual bool IsHideState()
    {
        if (m_objMove)
        {
            if (NGUITools.GetActive(m_objMove) == false || 
                m_objMove.transform.localPosition == m_HidePos || 
                m_objMove.transform.localScale == Vector3.zero)
                return true;
        }

        return false;
    }

    protected virtual void ShowEnd()
    {
        mb_ShowEnd = true;
        SetEnableUI(true);
        SetButtonEnable(true);
    }

    protected virtual void HideEnd()
    {
        mb_ShowEnd = false;
        NGUITools.SetActiveSelf(m_objMove, false);
    }

    public void SetEnableUI(bool _bEnable)
    {
        m_bEnableUI = _bEnable;
        UICamera[] arrayUICam = GetComponentsInChildren<UICamera>(true);
        for (int i = 0 ; i < arrayUICam.Length ; ++i)
            arrayUICam[i].enabled = _bEnable;
    }

    public virtual void SetButtonEnable(bool _bEnable)
    {
        if (_bEnable == true && m_bEnableUI == false)
            return;

        GameObject obj = m_objMove == null ? this.gameObject : m_objMove;
        Collider[] arrayCollider = obj.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in arrayCollider)
        {
            if (collider != null)
            {
                collider.enabled = _bEnable;
                if (_bEnable == true)
                {
                    UIButtonColorBase buttonColor = collider.GetComponent<UIButtonColorBase>();
                    if (buttonColor != null)
                        buttonColor.OnHover(false);
                }
            }
        }
    }

    public void EnableButton()
    {
        SetButtonEnable(true);
    }

    protected void SetLabelDisableState(bool _bEnabel, UILabel _Label, Color _Color = new Color(), Vector3 _Scale = new Vector3(), float _fRatio = 0.85f)
    {
        if (_Color == Color.clear)
            _Color = _bEnabel ? m_EnableLabelColor : m_DisableLabelColor;

        _Label.color = _Color;
        _Label.effectColor = _Color;

        if (_Scale != Vector3.zero)
        {
            Vector3 vScale = _bEnabel ? _Scale : _Scale * _fRatio;
            _Label.transform.localScale = vScale;
        }
    }
}
