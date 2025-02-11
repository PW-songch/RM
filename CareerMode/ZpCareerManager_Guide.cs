using UnityEngine;
using System.Collections.Generic;

public partial class ZpCareerManager : ZpSingleton<ZpCareerManager>
{
#if UNITY_EDITOR
    public static bool m_bCareerGuideTest = false;
#endif

    const string m_strGuideShowCountKey = "CAREER_GUIDE_SHOW_COUNT_{0}";

    private void SaveLocalDataCareerModeGuideShowCount()
    {
        if (m_CurrentPlayCareerInfo == null || CurrentPlayCareerInfo.IsUseGuide() == false)
            return;

        ZpPlayerPrefs.SetUserUIDWithKey(m_strGuideShowCountKey);
        ZpPlayerPrefs.SetInt(string.Format(m_strGuideShowCountKey, CurrentPlayCareerInfo.stageIndex),
            Mathf.Min(++CurrentPlayCareerInfo.guideShowCount, CurrentPlayCareerInfo.guideShowMaxCount));
        ZpPlayerPrefs.Save();

        #if UNITY_EDITOR
        ZpLog.Normal(ZpLog.E_Category.Careeer, "[SaveLocalDataCareerModeGuideShowCount] - " + CurrentPlayCareerInfo.stageIndex);
        #endif
    }

    private void SetCareerModeGuideShowCountFromLocalData()
    {
        if (m_CurrentPlayCareerInfo == null)
            return;

        if (CurrentPlayCareerInfo.IsUseGuide() == true && ZpPlayerPrefs.IsSameSaveUserUID(m_strGuideShowCountKey) == true)
        {
#if UNITY_EDITOR
            CurrentPlayCareerInfo.guideShowCount = m_bCareerGuideTest == true ? 0 : 
                ZpPlayerPrefs.GetInt(string.Format(m_strGuideShowCountKey, CurrentPlayCareerInfo.stageIndex));
#else
            CurrentPlayCareerInfo.guideShowCount = ZpPlayerPrefs.GetInt(string.Format(m_strGuideShowCountKey, CurrentPlayCareerInfo.stageIndex));
#endif
        }
    }

    public bool IsShowAbleGuide(ConditionType _eConditionType)
    {
        if (m_CurrentPlayCareerInfo == null || CurrentPlayCareerInfo.IsShowAbleGuide() == false)
            return false;
        return  m_CareerInfo.IsUseGuide(_eConditionType);
    }

    public bool IsShowAbleGuideInPlayRace(ConditionType _eConditionType)
    {
        cCareerConditionData condition = GetConditionData(_eConditionType);
        if (condition != null)
            return condition.IsShowGuide();
        return false;
    }

    public bool IsShowAbleGuideInPlayRace(ConditionType[] _arrayConditionType)
    {
        for (int i = 0 ; i < _arrayConditionType.Length ; ++i)
        {
            if (IsShowAbleGuideInPlayRace(_arrayConditionType[i]) == true)
                return true;
        }

        return false;
    }

#region Highlight Point Obj

    private Dictionary<ZpHighLightPointObjTarget, ZpHighLightPointObj> m_dicHighLightPointOb;
    public const string m_strPrefabName_HighLightPointObj = "prefabHighLightPoint";

    public ZpHighLightPointObj LinkHighLightPointObj(ZpHighLightPointObjTarget _target, string _strPrefabName)
    {
        if (_target == null || _target.IsVisible == false)
            return null;

        if (m_dicHighLightPointOb == null)
            m_dicHighLightPointOb = new Dictionary<ZpHighLightPointObjTarget, ZpHighLightPointObj>();

        if (m_dicHighLightPointOb.ContainsKey(_target) == false)
        {
            foreach (KeyValuePair<ZpHighLightPointObjTarget, ZpHighLightPointObj> data in m_dicHighLightPointOb)
            {
                if (data.Key.IsVisible == false && data.Key.IsContainConditionType(_target.ConditionTypeList) == true)
                {
                    //  시야 밖에 있는 것으로 연결
                    data.Value.Target = _target.transform;
                    m_dicHighLightPointOb.Remove(data.Key);
                    m_dicHighLightPointOb.Add(_target, data.Value);

                    #if UNITY_EDITOR
                    ZpLog.Normal(ZpLog.E_Category.Load, "[LinkHighLightPointObj] Change highlight obj - " + _strPrefabName);
                    #endif
                    return data.Value;
                }
            }

            //  새로 추가
            GameObject obj = zpPrefabLoadUtil.PrefabLoad(null, zpPrefabLoadUtil.kPathPrefabs + _strPrefabName);
            if (obj != null)
            {
                ZpHighLightPointObj highLight = obj.GetComponent<ZpHighLightPointObj>();
                if (highLight != null)
                {
                    highLight.Target = _target.transform;
                    m_dicHighLightPointOb.Add(_target, highLight);

                    #if UNITY_EDITOR
                    ZpLog.Normal(ZpLog.E_Category.Load, "[LinkHighLightPointObj] Add highlight obj - " + _strPrefabName);
                    ZpLog.Error(ZpLog.E_Category.Load, "[LinkHighLightPointObj] highlight obj count - " + m_dicHighLightPointOb.Count);
                    #endif
                    return highLight;
                }
            }
        }
        else
            return m_dicHighLightPointOb[_target];

        return null;
    }

    public void DisableHighLightPointObj(ConditionType _eConditionType)
    {
        if (m_dicHighLightPointOb != null)
        {
            List<ZpHighLightPointObjTarget> removeList = new List<ZpHighLightPointObjTarget>();
            foreach (ZpHighLightPointObjTarget obj in m_dicHighLightPointOb.Keys)
            {
                if (obj != null && obj.IsContainConditionType(_eConditionType) == true)
                {
                    if (obj.SetDisable(_eConditionType, true) == true)
                        removeList.Add(obj);
                }
            }

            for (int i = 0 ; i < removeList.Count ; ++i)
                m_dicHighLightPointOb.Remove(removeList[i]);

            ZpEventListener.Broadcast("RemoveConditionTypeInHighLightPointObj", _eConditionType);
        }
    }

    public void HideAllHighLightPointObj()
    {
        if (m_dicHighLightPointOb != null)
        {
            foreach (ZpHighLightPointObjTarget obj in m_dicHighLightPointOb.Keys)
                obj.SetDisable(true);
        }

        RemoveAllConditionGuide();
    }

    public void RemoveAllHighLightPointObj()
    {
        if (m_dicHighLightPointOb != null)
        {
            foreach (KeyValuePair<ZpHighLightPointObjTarget, ZpHighLightPointObj> data in m_dicHighLightPointOb)
                data.Key.SetDisable(true, true);

            m_dicHighLightPointOb.Clear();
        }

        RemoveAllConditionGuide();
    }

#endregion

#region UI Guide

    private List<ZpCareerMode_ConditionGuide> m_conditionGuideList;
    private List<ConditionType> m_ableGuideConditionTypeList;
    private Dictionary<ConditionType, GameObject[]> m_dicConditionGuideTarget;

    private const string m_strPrefabName_ConditionGuide_Left = "prefabCareerMode_ConditionGuide_Left";
    private const string m_strPrefabName_ConditionGuide_Right = "prefabCareerMode_ConditionGuide_Right";

    public ZpCareerMode_ConditionGuide InstantiateCareerModeConditionGuide(bool _bRight, GameObject _objTarget, List<ConditionType> _conditionTypeList)
    {
        if (IsContainConditionGuide(_objTarget, _conditionTypeList.ToArray()) == false)
        {
            if (m_conditionGuideList == null)
                m_conditionGuideList = new List<ZpCareerMode_ConditionGuide>();

            string strPrefabName = _bRight == true ? m_strPrefabName_ConditionGuide_Right : m_strPrefabName_ConditionGuide_Left;
            GameObject obj = zpPrefabLoadUtil.PrefabLoad(_objTarget.transform.parent.gameObject, zpPrefabLoadUtil.kPathPrefabs + strPrefabName);
            if (obj != null)
            {
                ZpCareerMode_ConditionGuide guide = obj.GetComponent<ZpCareerMode_ConditionGuide>();
                if (guide != null)
                {
                    guide.InitializeConditionGuide(_conditionTypeList, _objTarget, m_CareerInfo.GetGuideText(_conditionTypeList[0]));
                    m_conditionGuideList.Add(guide);
                    return guide;
                }
            }
        }

        return null;
    }

    public void InstantiateCareerModeConditionGuide(Camera _cam, Dictionary<GameObject, List<ConditionType>> _dicConditionGuideInfo)
    {
        foreach (KeyValuePair<GameObject, List<ConditionType>> data in _dicConditionGuideInfo)
            InstantiateCareerModeConditionGuide(ZpUtility.IsRightPositionInScreen(_cam, data.Key), data.Key, data.Value);
    }

    private ZpCareerMode_ConditionGuide ShowConditionGuide(bool _bShow, GameObject _objTarget, List<ConditionType> _conditionTypeList)
    {
        if (m_conditionGuideList != null && m_conditionGuideList.Count > 0)
        {
            for (int i = 0 ; i < m_conditionGuideList.Count ; ++i)
            {
                ZpCareerMode_ConditionGuide guide = m_conditionGuideList[i];
                if (guide != null && guide.IsSame(_objTarget, _conditionTypeList.ToArray()) == true)
                {
                    if (_bShow == false)
                        guide.ShowPopupAnimation(_bShow, false);
                    else
                    {
                        for (int j = 0 ; j < _conditionTypeList.Count ; ++j)
                        {
                            if (IsShowAbleGuideInPlayRace(_conditionTypeList[j]) == true)
                            {
                                guide.ShowPopupAnimation(_bShow, false);
                                break;
                            }
                        }
                    }

                    return guide;
                }
            }
        }

        return null;
    }

    public List<ZpCareerMode_ConditionGuide> ShowCareerModeConditionGuide(bool _bShow, Dictionary<GameObject, List<ConditionType>> _dicConditionGuideInfo)
    {
        List<ZpCareerMode_ConditionGuide> guideList = new List<ZpCareerMode_ConditionGuide>();
        foreach (KeyValuePair<GameObject, List<ConditionType>> data in _dicConditionGuideInfo)
        {
            ZpCareerMode_ConditionGuide guide = ShowConditionGuide(_bShow, data.Key, data.Value);
            if (guide != null)
                guideList.Add(guide);
        }

        return guideList;
    }

    public List<ZpCareerMode_ConditionGuide> ShowCareerModeConditionGuide(bool _bShow, List<ConditionType> _conditionList)
    {
        if (_conditionList != null && _conditionList.Count > 0)
        {
            List<ConditionType> conditionList = GetShowAbleUIGuideConditionType();
            if (conditionList != null)
            {
                List<ConditionType> showConditionList = new List<ConditionType>();
                for (int i = 0 ; i < _conditionList.Count ; ++i)
                {
                    ConditionType conditionType = _conditionList[i];
                    if (conditionList.Contains(conditionType) == true)
                        showConditionList.Add(conditionType);
                }

                if (showConditionList.Count > 0)
                {
                    List<ZpCareerMode_ConditionGuide> guideList = ShowCareerModeConditionGuide(_bShow, 
                        GetCareerModeConditionGuideInfo(showConditionList, false, true));
                    return guideList;
                }
            }
        }

        return null;
    }

    private bool IsContainConditionGuide(GameObject _objTarget, ConditionType[] _arrayConditionType)
    {
        if (m_conditionGuideList != null && m_conditionGuideList.Count > 0)
        {
            for (int i = 0 ; i < m_conditionGuideList.Count ; ++i)
            {
                ZpCareerMode_ConditionGuide guide = m_conditionGuideList[i];
                if (guide.IsSame(_objTarget, _arrayConditionType) == true)
                    return true;
            }
        }

        return false;
    }

    public void AddCareerModeConditionTarget(ConditionType _eConditionType, GameObject[] _arrayTarget)
    {
        if (_arrayTarget == null || _arrayTarget.Length == 0)
            return;

        if (m_dicConditionGuideTarget == null)
            m_dicConditionGuideTarget = new Dictionary<ConditionType, GameObject[]>();

        if (m_dicConditionGuideTarget.ContainsKey(_eConditionType) == false)
            m_dicConditionGuideTarget.Add(_eConditionType, _arrayTarget);
    }

    public void RemoveAllConditionGuide()
    {
        if (m_conditionGuideList != null && m_conditionGuideList.Count > 0)
        {
            for (int i = 0 ; i < m_conditionGuideList.Count ; ++i)
            {
                ZpCareerMode_ConditionGuide guide = m_conditionGuideList[i];
                if (guide != null)
                    guide.ShowPopupAnimation(false, true);
            }

            m_conditionGuideList.Clear();
        }

        if (m_ableGuideConditionTypeList != null)
        {
            m_ableGuideConditionTypeList.Clear();
            m_ableGuideConditionTypeList = null;
        }

        if (m_dicConditionGuideTarget != null)
            m_dicConditionGuideTarget.Clear();
    }

    public void RemoveConditionGuide(ConditionType _eCoditionType)
    {
        if (m_conditionGuideList != null && m_conditionGuideList.Count > 0)
        {
            List<ZpCareerMode_ConditionGuide> removeList = new List<ZpCareerMode_ConditionGuide>();
            for (int i = 0 ; i < m_conditionGuideList.Count ; ++i)
            {
                ZpCareerMode_ConditionGuide guide = m_conditionGuideList[i];
                if (guide.RemoveConditionType(_eCoditionType) == true)
                    removeList.Add(guide);
            }

            for (int i = 0 ; i < removeList.Count ; ++i)
                m_conditionGuideList.Remove(removeList[i]);
        }

        if (m_ableGuideConditionTypeList != null)
        {
            m_ableGuideConditionTypeList.Remove(_eCoditionType);
            if (m_ableGuideConditionTypeList.Count == 0)
            {
                m_ableGuideConditionTypeList = null;

                if (m_dicConditionGuideTarget != null)
                    m_dicConditionGuideTarget.Clear();
            }
        }
    }

    public List<ConditionType> GetShowAbleUIGuideConditionType()
    {
        if (m_CurrentPlayCareerInfo == null || CurrentPlayCareerInfo.IsShowAbleGuide() == false)
            return null;

        if (m_ableGuideConditionTypeList == null)
        {
            List<ConditionType> conditionList = m_CareerInfo.GetShowAbleUIGuideConditionType();
            m_ableGuideConditionTypeList = new List<ConditionType>();

            for (int i = 0 ; i < conditionList.Count ; ++i)
            {
                ConditionType condition = conditionList[i];
                if (IsContainCondition(condition) == true)
                    m_ableGuideConditionTypeList.Add(condition);
            }
        }

        return m_ableGuideConditionTypeList;
    }

    public Dictionary<GameObject, List<ConditionType>> GetCareerModeConditionGuideInfo(List<ConditionType> _conditionList, bool _bInstantiateStart = false, bool _bForce = false)
    {
        Dictionary<GameObject, List<ConditionType>> dicConditionGuideInfo = new Dictionary<GameObject, List<ConditionType>>();

        for (int i = 0 ; i < _conditionList.Count ; ++i)
        {
            ConditionType conditionType = _conditionList[i];
            GameObject[] arrayTarget = GetConditionGuideTargetObj(conditionType, _bInstantiateStart, _bForce);
            if (arrayTarget != null)
            {
                for (int j = 0 ; j < arrayTarget.Length ; ++j)
                {
                    GameObject target = arrayTarget[j];
                    if (target != null && dicConditionGuideInfo.ContainsKey(target) == false)
                    {
                        List<ConditionType> ableConditionList = new List<ConditionType>();
                        ableConditionList.Add(conditionType);
                        dicConditionGuideInfo.Add(target, ableConditionList);

                        for (int k = 0 ; k < _conditionList.Count ; ++k)
                        {
                            if (i == k)
                                continue;

                            ConditionType eConditionType = _conditionList[k];
                            GameObject[] arrayObjTarget = GetConditionGuideTargetObj(eConditionType, _bInstantiateStart, _bForce);
                            if (arrayObjTarget != null)
                            {
                                for (int l = 0 ; l < arrayObjTarget.Length ; ++l)
                                {
                                    if (target == arrayObjTarget[l])
                                        ableConditionList.Add(eConditionType);
                                }
                            }
                        }
                    }
                }
            }
        }

        return dicConditionGuideInfo;
    }

    private GameObject[] GetConditionGuideTargetObj(ConditionType _eConditionType, bool _bInstantiateStart = false, bool _bForce = false)
    {
        if (m_dicConditionGuideTarget == null)
            return null;

        GameObject[] arrayTarget;
        if (m_dicConditionGuideTarget.TryGetValue(_eConditionType, out arrayTarget) == true)
        {
            switch (_eConditionType)
            {
                case ConditionType.DRIFTBOOST_COUNT:
                case ConditionType.FIRST_DRIFTBOOST_COUNT:
                case ConditionType.SECOND_DRIFTBOOST_COUNT:
                case ConditionType.THIRD_DRIFTBOOST_COUNT:

                case ConditionType.ITEM_USE_COUNT:
                case ConditionType.BANANA_ITEM_USE_COUNT:
                case ConditionType.GUARD_ITEM_USE_COUNT:
                case ConditionType.HAMMER_ITEM_USE_COUNT:
                case ConditionType.ICEBALL_ITEM_USE_COUNT:
                case ConditionType.MISSILE_ITEM_USE_COUNT:
                case ConditionType.NITRO_ITEM_USE_COUNT:
                case ConditionType.THUNDER_ITEM_USE_COUNT:
                case ConditionType.TURTLE_ITEM_USE_COUNT:
                case ConditionType.WARP_ITEM_USE_COUNT:

                case ConditionType.FIRST_NITRO_USE_COUNT:
                case ConditionType.SECOND_NITRO_USE_COUNT:
                case ConditionType.THIRD_NITRO_USE_COUNT:
                case ConditionType.START_BOOST_USE_COUNT:

                case ConditionType.CONTROL_DIRECTION_COUNT:
                case ConditionType.TIME_LIMITED_SUDDENDEATH:
                    return arrayTarget;

                case ConditionType.GRAND_PRIX:
                    {
                        if (_bInstantiateStart == false || _bForce == true)
                            return arrayTarget;
                    }
                    break;
            }
        }

        return null;
    }

    public List<ConditionType> GetCareerConditionTypeList_Drift()
    {
        return new List<ConditionType>()
        {
            ConditionType.DRIFTBOOST_COUNT, 
            ConditionType.FIRST_DRIFTBOOST_COUNT, 
            ConditionType.SECOND_DRIFTBOOST_COUNT, 
            ConditionType.THIRD_DRIFTBOOST_COUNT
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_StartBoost()
    {
        return new List<ConditionType>()
        {
            ConditionType.START_BOOST_USE_COUNT
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_NitroBooster()
    {
        return new List<ConditionType>()
        {
            ConditionType.FIRST_NITRO_USE_COUNT,
            ConditionType.SECOND_NITRO_USE_COUNT,
            ConditionType.THIRD_NITRO_USE_COUNT
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_ItemUse()
    {
        return new List<ConditionType>()
        {
            ConditionType.ITEM_USE_COUNT,
            ConditionType.BANANA_ITEM_USE_COUNT,
            ConditionType.GUARD_ITEM_USE_COUNT,
            ConditionType.HAMMER_ITEM_USE_COUNT,
            ConditionType.ICEBALL_ITEM_USE_COUNT,
            ConditionType.MISSILE_ITEM_USE_COUNT,
            ConditionType.NITRO_ITEM_USE_COUNT,
            ConditionType.THUNDER_ITEM_USE_COUNT,
            ConditionType.TURTLE_ITEM_USE_COUNT,
            ConditionType.WARP_ITEM_USE_COUNT
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_ControlDirection()
    {
        return new List<ConditionType>()
        {
            ConditionType.CONTROL_DIRECTION_COUNT
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_SuddenDeath()
    {
        return new List<ConditionType>()
        {
            ConditionType.TIME_LIMITED_SUDDENDEATH,
            ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH
        };
    }

    public List<ConditionType> GetCareerConditionTypeList_GrandPrix()
    {
        return new List<ConditionType>()
        {
            ConditionType.GRAND_PRIX
        };
    }

#endregion
}
