using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ZpPatternBinder : ZpPatternItemBase
{
    public enum State
    {
        THROW,
        RECEIVE,
        BIND,
        FAIL,
        DISAPEAR,
    }

    public GameObject m_BindEffect;
    public GameObject m_DisapearEffect;
    public string m_AnimThrow;
    public string m_AnimReceive;
    public string m_AnimBind;
    public string m_AnimFail;
    public float m_DisapearDuration = 0.4f;

    private float m_fDuration = 5.0f;                   //  효과 지속 시간
    //private float m_fBuildupValue = 0.0f;
    private float m_fSlowdownPercent = 0.0f;
    private float m_fSlowdownPerHour = 0.0f;
    private bool m_bSuccess = false;

    // ------------------------------------------------------------------------------------------------
    public static void Spawn(GameObject _prefab, ZpVehicleBase _fromVehicle, ZpVehicleBase _targetVehicle,
        float _fDuration, float _fBuildupValue, float _fSlowdownPercent, float _fSlowdownPerHour, bool bIsSkill = false)
    {
        if (_prefab != null && _fromVehicle != null && _targetVehicle != null)
        {
            GameObject bindTargetEffect = (GameObject)Instantiate(
                _prefab, _fromVehicle.transform.position, _fromVehicle.transform.rotation);

            ZpPatternBinder binder = bindTargetEffect.GetComponent<ZpPatternBinder>();
            if (binder != null)
            {
                binder.m_ItemType = BMItemType.BIND_TARGET;
                binder.FromVehicle = _fromVehicle;
                binder.TargetVehicle = _targetVehicle;
                binder.m_fDuration = _fDuration;
                binder.m_buildup_value = _fBuildupValue;
                binder.m_fSlowdownPercent = _fSlowdownPercent;
                binder.m_fSlowdownPerHour = _fSlowdownPerHour;
                binder.m_bIsActiveSkill = bIsSkill;

                if (_targetVehicle.IsVehicleLocal())
                    ZpEventListener.Broadcast("EVENT_UI_WARNING", _fromVehicle.m_PlayerSlot, false, BMItemType.BIND_TARGET);
            }
            else
            {
                ZpLog.Error(ZpLog.E_Category.None, "ZpPatternBinder::Spawn Error!!");
                Destroy(bindTargetEffect);
            }
        }
    }

    // -------------------------------------------------------------------------------------------------
    protected override void Start()
    {
        if (FromVehicle != null)
        {
            if (TargetVehicle != null)
                TargetVehicle.AddPattern(this);

            //if (GlobalsPlayTool.bToolState)
            //{
            //    SetNextState(State.BIND);
            //    NextState();
            //    return;
            //}

            SetNextState(State.THROW);
            NextState();
            return;
        }

        Destroy(this.gameObject);
    }

    // -------------------------------------------------------------------------------------------------
    public override void OnStop()
    {
        if ((State)m_eState == State.DISAPEAR)
            return;

        string MethodName = m_eState.ToString() + "_STATE";
        StopCoroutine(MethodName);

        if (TargetVehicle != null)
            TargetVehicle.StopVehicleFlatten();

        m_bSuccess = false;
        SetNextState(State.DISAPEAR);
        NextState();
    }

    // -------------------------------------------------------------------------------------------------
    IEnumerator THROW_STATE()
    {
        if (TargetVehicle && TargetVehicle.IsVehicleLocal())
        {
            if (ZpGameGlobals.m_ScriptSC)
                ZpGameGlobals.m_ScriptSC.PlaySoundFx(SOUND_FX.HAMMER_FLY);
        }

        if (m_BindEffect != null)
            m_BindEffect.SetActive(false);

        if (m_DisapearEffect != null)
            m_DisapearEffect.SetActive(false);

        ZpUtility.EnableRendererRecursive(this.gameObject, true);
        AttachFromVehicle(this.gameObject);
        ZpUtility.ChangeLayer(this.gameObject, ZpGameGlobals.m_LayerVehicle);

        if (TargetVehicle != null && TargetVehicle.IsVehicleLocal())
        {
            ZpVehicleNotify noti = new ZpVehicleNotify();
            noti.m_GameObject = TargetVehicle.gameObject;
            noti.m_Event = ZpVehicleEvent.Create(ZpVehicleEvent.Type.ITEM_ANI_APPROACH);
            ZpNotificationCenter.instance.PostNotification("VEHICLE_NOTIFICATION", noti);
        }

        animation.CrossFade(m_AnimThrow);

        float fAnimTime = animation[m_AnimThrow].length;
        float StartTime = Time.realtimeSinceStartup - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

        while ((State)m_eState == State.THROW)
        {
            if (fAnimTime < Time.realtimeSinceStartup - StartTime - 
                (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f))
            {
                if (TargetVehicle != null)
                    SetNextState(State.RECEIVE);
                else
                {
                    SetNextState(State.DISAPEAR);

                    if (FromVehicle != null)
                    {
                        ZpVehicleNotify noti = new ZpVehicleNotify();
                        noti.m_GameObject = FromVehicle.gameObject;
                        noti.m_Event = ZpVehicleEvent.Create(ZpVehicleEvent.Type.ITEM_ANI_RESULT_FAIL1);
                        ZpNotificationCenter.instance.PostNotification("VEHICLE_NOTIFICATION", noti);
                    }
                }

                break;
            }

            yield return null;
        }

        DetachFromVehicle(gameObject);
        NextState();
    }

    // -------------------------------------------------------------------------------------------------
    IEnumerator RECEIVE_STATE()
    {
        AttachTargetVehicle(this.gameObject);
        animation.CrossFade(m_AnimReceive, 0.0f);

        float fAnimTime = animation[m_AnimReceive].length;
        float StartTime = Time.realtimeSinceStartup - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

        while ((State)m_eState == State.RECEIVE)
        {
            if (TargetVehicle.IsGuardItem() == true)
            {
                ZpVehicleNotify.SendNotify(ZpVehicleEvent.Type.ITEM_BIND_TARGET_HITTED,
                    TargetVehicle.m_GameObj, this, m_fDuration, m_buildup_value, m_fSlowdownPercent, m_fSlowdownPerHour);
                SetNextState(State.FAIL);
                break;
            }

            float fTime = Time.realtimeSinceStartup - StartTime - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);
            if (fAnimTime < fTime)
            {
                SetNextState(State.BIND);
                break;
            }

            yield return null;
        }

        NextState();
    }

    // -------------------------------------------------------------------------------------------------
    IEnumerator BIND_STATE()
    {
        if (TargetVehicle != null)
        {
            m_bSuccess = true;
            ZpEventListener.AddListener("SetIsItemDeffence", this);
            ZpVehicleNotify.SendNotify(ZpVehicleEvent.Type.ITEM_BIND_TARGET_HITTED,
                TargetVehicle.m_GameObj, this, m_fDuration, m_buildup_value, m_fSlowdownPercent, m_fSlowdownPerHour);
            ZpEventListener.RemoveListener("SetIsItemDeffence", this);

            if (m_IsDefence == false)
            {
                if (m_BindEffect != null)
                {
                    m_BindEffect.SetActive(true);
                    ZpUtility.ShurikenEmitChildren(m_BindEffect, true);
                }

                if (FromVehicle && FromVehicle.IsVehicleLocal())
                    SendSuccessItemMessage(BMItemType.BIND_TARGET, TargetVehicle, m_Upgrade, this);
                if (FromVehicle && FromVehicle.IsLocalSimulate())
                    ZpVehicleNotify.SendNotify(ZpVehicleEvent.Type.ITEM_ANI_RESULT_SUCCESS1, FromVehicle.m_GameObj);
            }
            else
                SetNextState(State.FAIL);

            //if (ZpGameGlobals.m_ScriptSC && FromVehicle && FromVehicle.IsVehicleLocal())
            //    ZpGameGlobals.m_ScriptSC.PlaySoundFx(SOUND_FX.HAMMER_USE, ZpItemHammer.m_DurationStop);
        }

        animation.CrossFade(m_AnimBind, 0.0f);
        float StartTime = Time.realtimeSinceStartup - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

        while ((State)m_eState == State.BIND)
        {
            float fTime = Time.realtimeSinceStartup - StartTime - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);
            if (fTime > m_fDuration)
            {
                SetNextState(State.DISAPEAR);
                animation.Stop();
                break;
            }

            yield return null;
        }

        NextState();
    }

    // -------------------------------------------------------------------------------------------------
    IEnumerator FAIL_STATE()
    {
        m_bSuccess = false;
        animation.CrossFade(m_AnimFail, 0.1f);

        float AnimTime = animation[m_AnimFail].length;
        float StartTime = Time.realtimeSinceStartup - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

        StopCoroutine("CoroutineApplyTransparency");
        yield return StartCoroutine("CoroutineApplyTransparency", AnimTime);

        SetNextState(State.DISAPEAR);
        NextState();
    }

    // -------------------------------------------------------------------------------------------------
    IEnumerator DISAPEAR_STATE()
    {
        if (m_BindEffect != null)
            m_BindEffect.SetActive(false);

        if (m_bSuccess == true)
        {
            if (m_DisapearEffect != null)
                m_DisapearEffect.SetActive(true);

            StopCoroutine("CoroutineApplyTransparency");
            yield return StartCoroutine("CoroutineApplyTransparency", m_DisapearDuration);
        }

        DetachTargetVehicle(this.gameObject);
		if (TargetVehicle != null)
			TargetVehicle.RemovePattern(this);
        Destroy(this.gameObject);
        yield break;
    }

    //
    IEnumerator CoroutineApplyTransparency(float _fDuration)
    {
        MeshRenderer[] arrayMeshRenderer = GetComponentsInChildren<MeshRenderer>();
        SkinnedMeshRenderer[] arraySkinnedMeshRenderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Renderer> arrayRenderer = null;
        Color[] arrayMatColor = null;

        if ((arrayMeshRenderer != null && arrayMeshRenderer.Length > 0) || (arraySkinnedMeshRenderer != null && arraySkinnedMeshRenderer.Length > 0))
        {
            Shader transparentShader = Shader.Find("RM/VertexLitAlpha");
            if (transparentShader != null)
            {
                arrayRenderer = new List<Renderer>();
                foreach (Renderer renderer in arrayRenderer)
                    arrayRenderer.Add(renderer);
                foreach (Renderer renderer in arraySkinnedMeshRenderer)
                    arrayRenderer.Add(renderer);

                arrayMatColor = new Color[arrayRenderer.Count];
                for (int i = 0 ; i < arrayRenderer.Count ; ++i)
                {
                    Renderer renderer = arrayRenderer[i];
                    renderer.material.shader = transparentShader;
                    arrayMatColor[i] = renderer.material.color;
                }
            }
        }

        float StartTime = Time.realtimeSinceStartup - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

        while (true)
        {
            float fTime = Time.realtimeSinceStartup - StartTime - (ZpGameGlobals.m_ScriptGM != null ? ZpGameGlobals.m_ScriptGM.GetPausedTime() : 0.0f);

            if (arrayRenderer != null && arrayRenderer.Count > 0)
            {
                for (int i = 0 ; i < arrayRenderer.Count ; ++i)
                {
                    Renderer renderer = arrayRenderer[i];
                    Color matColor = arrayMatColor[i];
                    matColor.a = Mathf.Clamp(1.0f - fTime / _fDuration, 0.0f, 1.0f);
                    renderer.material.color = matColor;
                }
            }

            if (_fDuration < fTime)
                break;

            yield return null;
        }
    }
}
