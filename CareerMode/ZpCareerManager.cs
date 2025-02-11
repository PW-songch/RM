using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using KMNetClass;

#region Career enum type

public enum StageType
{
    NORMAL = 0,
    BOSS = 21,
    FAVOR = 22,
    HIDDEN = 23,
    DAILY_LIMITED = 24,
    END
}

public enum CareerGameMode
{
    ITEM = 1,
    SPEED = 2,
    ITEMTEAM = 3,
    GRANDPRIX_ITEM = 11,
    GRANDPRIX_SPEED = 12,
    END
}

public enum ConditionDataType
{
    INTEGER = 0,
    END
}

public enum ConditionType
{
    TUTORIAL = 0,                               //  0
    RANK = 1,                                   //  1
    //	drift boost
    DRIFTBOOST_COUNT,                           //  2
    FIRST_DRIFTBOOST_COUNT,                     //  3
    SECOND_DRIFTBOOST_COUNT,                    //  4
    THIRD_DRIFTBOOST_COUNT,                     //  5
    //	slipstereem
    SLIPSTREEM_COUNT,                           //  6
    //	jump
    JUMP_COUNT,                                 //  7
    //	fence collide
    FENCE_COLLIDE_COUNT,                        //  8
    //	item get
    ITEM_GET_COUNT,                             //  9
    //	defence item success
    DEFENCEITEM_SUCCESS_COUNT,                  //  10
    //	attack item success
    ITEMATTACK_SUCCESS_COUNT,                   //  11
    MISSILE_ITEMATTACK_SUCCESS_COUNT,           //  12
    HAMMER_ITEMATTACK_SUCCESS_COUNT,            //  13
    ICEBALL_ITEMATTACK_SUCCESS_COUNT,           //  14
    TURTLE_ITEMATTACK_SUCCESS_COUNT,            //  15
    BANANA_ITEMATTACK_SUCCESS_COUNT,            //  16
    THUNDER_ITEMATTACK_SUCCESS_COUNT,           //  17
    WARP_ITEMATTACK_SUCCESS_COUNT,              //  18
    //	item use
    ITEM_USE_COUNT,                             //  19
    MISSILE_ITEM_USE_COUNT,                     //  20
    HAMMER_ITEM_USE_COUNT,                      //  21
    ICEBALL_ITEM_USE_COUNT,                     //  22
    TURTLE_ITEM_USE_COUNT,                      //  23
    BANANA_ITEM_USE_COUNT,                      //  24
    THUNDER_ITEM_USE_COUNT,                     //  25
    WARP_ITEM_USE_COUNT,                        //  26
    GUARD_ITEM_USE_COUNT,                       //  27
    NITRO_ITEM_USE_COUNT,                       //  28
    //	speed mode boost use
    FIRST_NITRO_USE_COUNT,                      //  29
    SECOND_NITRO_USE_COUNT,                     //  30
    THIRD_NITRO_USE_COUNT,                      //  31
    START_BOOST_USE_COUNT,                      //  32
    //	boostzone
    BOOSTZONE_USE_COUNT,                        //  33
    RED_BOOSTZONE_USE_COUNT,                    //  34
    BLUE_BOOSTZONE_USE_COUNT,                   //  35
    GREEN_BOOSTZONE_USE_COUNT,                  //  36
    //	reset
    RACE_RESET_COUNT,                           //  37
    //	remain rank in second
    REMAIN_RANK_IN_SECOND,                      //  38
    //	time attack
    TIME_LIMITED,                               //  39
    //	suddendeath
    TIME_LIMITED_SUDDENDEATH,                   //  40
    LAPCOUNT_LIMITED_SUDDENDEATH,               //  41
    //	grand prix
    GRAND_PRIX,                                 //  42
    //  control direction
    CONTROL_DIRECTION_COUNT,                    //  43

    END
}

#endregion

public partial class ZpCareerManager : ZpSingleton<ZpCareerManager>
{
    private int m_nMaxEpisodCount;

    private int m_nCurrentPlayEpisod = 0;
    private int m_nCurrentPlayStage = 0;
    private int m_nCurrentPlayEpisodStage = 0;

    private int m_nLastOpenEpisod = 0;
    private int m_nLastOpenStage = 0;
    private int m_nLastOpenEpisodStage = 0;

    private int m_nCurrentPlayStagePrevRank = 0;

    private float m_fCareerTimeCheck = 0.0f;

    private bool m_bCareerModeScene = false;
    private bool m_bConditionMinimumDescSetCompleted = false;
    public bool IsConditionMinimumDescSetCompleted
    {
        get { return m_bConditionMinimumDescSetCompleted; }
        set { m_bConditionMinimumDescSetCompleted = value; }
    }

    private OpenedStageInfo m_openedStageInfo;
    public OpenedStageInfo GetOpenedStageInfo
    {
        get { return m_openedStageInfo; }
    }

    private ZpCareerMode m_CareerMode = null;
    private CareerStageInfo m_CurrentPlayCareerInfo = null;
    private Dictionary<int, cCareerEpisodResult> m_Result = null;

    private Dictionary<ConditionType, KeyValuePair<ConditionDataType, cCareerConditionData>> m_ConditionList = null;
    private Dictionary<ConditionType, KeyValuePair<int, string>> m_ConditionResultDesc = null;

    private ZpCSVCareer m_CareerInfo = null;
    public ZpCSVCareer GetCareerInfo
    {
        get { return m_CareerInfo; }
    }

    private ZpCSVCareerCutScene m_CareerCutSceneInfo = null;
    public ZpCSVCareerCutScene GetCareerCutSceneInfo
    {
        get { return m_CareerCutSceneInfo; }
    }

    private int m_nShowNextEpisodStage = 0;
    private bool m_bShowNextEpisodStage = false;
    private bool m_bShowPopupWillUpdateNextEpisod = false;
    public bool IsShowPopupWillUpdateNextEpisod
    {
        get { return m_bShowPopupWillUpdateNextEpisod; }
        set { m_bShowPopupWillUpdateNextEpisod = value; }
    }

    public const int MAX_MOVE_ABLE_STAGE = (int)StageType.FAVOR;
    public const int RANK_MIN = 0;
    public const int RANK_MAX = 3;

    public readonly Color CODITION_NORMAL_COLOR = Color.white;
    public readonly Color CODITION_SUCCESS_COLOR = new Color(44.0f / 255.0f, 56.0f / 255.0f, 1.0f / 255.0f);
    public readonly Color CODITION_SUCCESS_COLOR_INGAME = new Color(0.0f, 186.0f / 255.0f, 1.0f);
    public readonly Color CODITION_FAILED_COLOR = new Color(1.0f, 78.0f / 255.0f, 0.0f);

    private int m_nTestRank = 0;

    #region (getter and setter)
    public ZpCareerMode GetSetCareerMode
    {
        get { return m_CareerMode; }
        set { m_CareerMode = value; }
    }

    public int MaxEpisodCount
    {
        get { return m_nMaxEpisodCount; }
        set { m_nMaxEpisodCount = value; }
    }

    public int CurrentPlayEpisod
    {
        get { return m_nCurrentPlayEpisod; }
        set
        {
            value = Mathf.Clamp(value, 1, MaxEpisodCount);
            m_nCurrentPlayEpisod = value;
            m_nCurrentPlayEpisodStage = GetEpisodStageWithIndex(m_nCurrentPlayEpisod, m_nCurrentPlayStage);
        }
    }

    public int CurrentPlayStage
    {
        get { return m_nCurrentPlayStage; }
        set
        {
            value = Mathf.Clamp(value, 0, (int)StageType.END - 1);
            m_nCurrentPlayStage = value;
            m_nCurrentPlayEpisodStage = GetEpisodStageWithIndex(m_nCurrentPlayEpisod, m_nCurrentPlayStage);
        }
    }

    public int CurrentPlayEpisodStage
    {
        get { return m_nCurrentPlayEpisodStage; }
        set
        {
            if (value == 0)
            {
                m_nCurrentPlayEpisod = 0;
                m_nCurrentPlayStage = 0;
            }
            else
            {
                CurrentPlayEpisod = GetEpisodWithIndex(value);
                CurrentPlayStage = GetStageWithIndex(value);
            }
        }
    }

    public int LastOpenEpisod
    {
        get { return m_nLastOpenEpisod; }
        set
        {
            value = Mathf.Clamp(value, 1, MaxEpisodCount);
            m_nLastOpenEpisod = value;
            m_nLastOpenEpisodStage = GetEpisodStageWithIndex(m_nLastOpenEpisod, m_nLastOpenStage);
        }
    }

    public int LastOpenStage
    {
        get { return m_nLastOpenStage; }
        set
        {
            value = Mathf.Clamp(value, 1, LastOpenEpisod == MaxEpisodCount ?
                (int)StageType.BOSS : (int)StageType.FAVOR);
            m_nLastOpenStage = value;
            m_nLastOpenEpisodStage = GetEpisodStageWithIndex(m_nLastOpenEpisod, m_nLastOpenStage);
        }
    }

    public int LastOpenEpisodStage
    {
        get { return m_nLastOpenEpisodStage; }
        set
        {
            if (value == 100)
            {
                m_nLastOpenEpisod = GetEpisodWithIndex(value);
                m_nLastOpenStage = GetStageWithIndex(value);
                m_nLastOpenEpisodStage = value;
            }
            else
            {
                LastOpenEpisod = GetEpisodWithIndex(value);
                LastOpenStage = GetStageWithIndex(value);
            }
        }
    }

    public int CurrentPlayStagePrevRank
    {
        get { return m_nCurrentPlayStagePrevRank; }
        set { m_nCurrentPlayStagePrevRank = value; }
    }

    public CareerStageInfo CurrentPlayCareerInfo
    {
        get
        {
            if (m_CurrentPlayCareerInfo == null)
                m_CurrentPlayCareerInfo = new CareerStageInfo();
            return m_CurrentPlayCareerInfo;
        }
        set
        {
            m_CurrentPlayCareerInfo = value;
            if (m_CurrentPlayCareerInfo == null)
                return;

            //condition list 추가
            if (m_ConditionList == null)
                m_ConditionList = new Dictionary<ConditionType, KeyValuePair<ConditionDataType, cCareerConditionData>>();
            else
                m_ConditionList.Clear();

            if (m_CurrentPlayCareerInfo.stageIndex <= 0)
                m_CurrentPlayCareerInfo.stageIndex = 101;

            m_CurrentPlayCareerInfo.myRank = GetCareerStageMyRank(m_CurrentPlayCareerInfo.stageIndex);

            //  test
            //if (ZpUITest.m_sbCareerTest)
            //{
            //    CareerCondition condition = null;
            //    Dictionary<int, CareerCondition> testConditionList = new Dictionary<int,CareerCondition>();
            //    for (int i = 0 ; i < ZpUITest.m_sConditionID.Length ; ++i)
            //    {
            //        if (testConditionList.ContainsKey(ZpUITest.m_sConditionID[i]))
            //            testConditionList[ZpUITest.m_sConditionID[i]].conditionValue.Add(ZpUITest.m_sConditionValue[i]);
            //        else
            //        {
            //            condition = new CareerCondition((ConditionType)ZpUITest.m_sConditionID[i]);
            //            condition.conditionValue.Add(ZpUITest.m_sConditionValue[i]);
            //            testConditionList.Add(ZpUITest.m_sConditionID[i], condition);
            //        }   
            //    }

            //    m_CurrentPlayCareerInfo.conditionList = new List<CareerCondition>(testConditionList.Values);

            //    bool bGrandprix = false;
            //    foreach (CareerCondition conditionValue in testConditionList.Values)
            //    {
            //        if (conditionValue.conditionType == ConditionType.GRAND_PRIX)
            //            bGrandprix = true;
            //    }


            //    int trackID = m_CurrentPlayCareerInfo.track.trackID[0];
            //    if (bGrandprix)
            //    {
            //        m_CurrentPlayCareerInfo.track.trackID = new int[] { trackID, 80000003, 80100003 };
            //        m_CurrentPlayCareerInfo.track.SetGameMode(121);
            //    }
            //    else
            //    {
            //        m_CurrentPlayCareerInfo.track.trackID = new int[] { trackID };
            //        m_CurrentPlayCareerInfo.track.SetGameMode((int)CareerGameMode.ITEM);
            //    }
            //}

            SetCareerModeGuideShowCountFromLocalData();

            List<CareerStageCondition> conditionList = m_CurrentPlayCareerInfo.conditionList;
            for (int i = 0 ; i < conditionList.Count ; ++i)
            {
                ConditionType conditionType = conditionList[i].conditionType;
                if (!m_ConditionList.ContainsKey(conditionType))
                {
                    ConditionDataType dataType = GetCareerInfo.GetConditionDataType(conditionType);
                    cCareerConditionData condition = CreateConditionList(conditionList[i]);
                    m_ConditionList.Add(conditionList[i].conditionType, new KeyValuePair<ConditionDataType, cCareerConditionData>(dataType, condition));

                    switch (dataType)
                    {
                        default:
                        case ConditionDataType.INTEGER:
                            if (IsSuddenDeathCondition() == false)
                            {
                                m_CurrentPlayCareerInfo.conditionList[i].conditionValue =
                                    (condition as cCareerConditionData).GetGoalValueList() as List<int>;
                            }
                            break;
                    }

#if UNITY_EDITOR
                    ZpLog.Normal(ZpLog.E_Category.Careeer, "# AddCareerCondition : " + conditionList[i].conditionType.ToString());
#endif
                }
                else
                {
                    switch (GetCareerInfo.GetConditionDataType(conditionType))
                    {
                        default:
                        case ConditionDataType.INTEGER:
                            cCareerIntegerTypeConditionData integerData = CastingIntegerTypeConditionDataClass(m_ConditionList[conditionType].Value);
                            if (integerData != null)
                            {
                                List<int> goalValueList = integerData.GetGoalValueList() as List<int>;
                                for (int j = 0 ; j < conditionList[i].conditionValue.Count ; ++j)
                                    goalValueList.Add(conditionList[i].conditionValue[j]);

                                integerData.SetGoalValueList(goalValueList);
                            }
                            break;
                    }
                }
            }

            SetConditionCompleted();
        }
    }
    #endregion

    #region static function

    public static int GetEpisodWithIndex(int _nEpisodStage)
    {
        return _nEpisodStage / 100;
    }

    public static int GetStageWithIndex(int _nEpisodStage)
    {
        return _nEpisodStage % 100;
    }

    public static int GetEpisodStageWithIndex(int _nEpisodIndex, int _nStageIndex)
    {
        return _nEpisodIndex * 100 + _nStageIndex;
    }

    public static int GetStageNumberForUI(int _nEpisodStage)
    {
        return GetStageNumberForUI(GetEpisodWithIndex(_nEpisodStage), GetStageWithIndex(_nEpisodStage));
    }

    public static int GetStageNumberForUI(int _nEpisodIndex, int _nStageIndex)
    {
        return /*(_nEpisodIndex - 1) * ((int)StageType.BOSS - 1) + */_nStageIndex;
    }

    public static bool IsBossStage(int _nStage)
    {
        return _nStage == (int)StageType.BOSS;
    }

    public static bool IsBossStage(StageType _type)
    {
        return _type == StageType.BOSS;
    }

    public static bool IsHiddenStage(int _nStage)
    {
        return _nStage == (int)StageType.HIDDEN;
    }

    public static bool IsHiddenStage(StageType _type)
    {
        return _type == StageType.HIDDEN;
    }

    public static bool IsFavorStage(int _nStage)
    {
        return _nStage == (int)StageType.FAVOR;
    }

    public static bool IsFavorStage(StageType _type)
    {
        return _type == StageType.FAVOR;
    }

    public static bool IsDailyLimitedStage(int _nStage)
    {
        return _nStage == (int)StageType.DAILY_LIMITED;
    }

    public static bool IsDailyLimitedStage(StageType _type)
    {
        return _type == StageType.DAILY_LIMITED;
    }

    public static bool IsPlayCountLimitedStage(int _nStage)
    {
        return _nStage == (int)StageType.HIDDEN || _nStage == (int)StageType.DAILY_LIMITED;
    }

    static public string GetConditionDesc(ConditionType _type, int _nValue, bool _bMinimum = false)
    {
        string strDesc = _bMinimum ? ZpCareerManager.instance.GetCareerInfo.GetConditionMinimumDesc(_type) :
            ZpCareerManager.instance.GetCareerInfo.GetConditionDesc(_type);
        strDesc = strDesc.Trim();

        switch (_type)
        {
            case ConditionType.REMAIN_RANK_IN_SECOND:
            case ConditionType.TIME_LIMITED:
            case ConditionType.TIME_LIMITED_SUDDENDEATH:
                {
                    int nMinute = _nValue / 60;
                    int nSecond = _nValue % 60;
                    if (nMinute <= 0)
                    {
                        int nRemoveStartIndex = strDesc.IndexOf("{0}");

                        //	분단위 제거
                        string strTemp = strDesc;
                        if (nRemoveStartIndex >= 0)
                        {
                            strTemp = strDesc.Remove(nRemoveStartIndex, "{0}".Length + 1);
                            strTemp = strTemp.Trim();
                            strTemp = strTemp.Replace("{1}", "{0}");
                        }
                        strDesc = string.Format(strTemp, nSecond);
                    }
                    else if (nMinute >= 1 && nSecond == 0)
                    {
                        int nRemoveStartIndex = strDesc.IndexOf("{1}");
                        int nSpaceIndex = strDesc.IndexOf(" ", nRemoveStartIndex >= 0 ? nRemoveStartIndex : 0);

                        //	초단위 제거
                        string strTemp = strDesc;
                        if (nRemoveStartIndex >= 0 && nRemoveStartIndex <= nSpaceIndex)
                        {
                            strTemp = strDesc.Remove(nRemoveStartIndex, nSpaceIndex - nRemoveStartIndex);
                            strTemp = strTemp.Trim();
                        }
                        strDesc = string.Format(strTemp, nMinute);
                    }
                    else
                        strDesc = string.Format(strDesc, nMinute, nSecond);
                }
                break;

            case ConditionType.GRAND_PRIX:
                {
                    CareerStageInfo careerInfo = ZpCareerManager.instance.CurrentPlayCareerInfo;
                    if (careerInfo != null && careerInfo.track != null)
                        strDesc = string.Format(strDesc, careerInfo.track.GetTrackLength(), _nValue);
                }
                break;

            default:
                {
                    strDesc = string.Format(strDesc, _nValue);
                }
                break;
        }

        return strDesc;
    }

    public static StageType GetStageType(int _nStage)
    {
        switch (_nStage)
        {
            case (int)StageType.BOSS:
                return StageType.BOSS;
            case (int)StageType.FAVOR:
                return StageType.FAVOR;
            case (int)StageType.HIDDEN:
                return StageType.HIDDEN;
            case (int)StageType.DAILY_LIMITED:
                return StageType.DAILY_LIMITED;
            default:
                return StageType.NORMAL;
        }
    }

    public static int GetStarRankCountByCompletedList(bool[] _arrayCompletedList)
    {
        int nStarRank = RANK_MIN;
        if (_arrayCompletedList != null)
        {
            for (int i = 0 ; i < _arrayCompletedList.Length ; ++i)
            {
                if (_arrayCompletedList[i])
                    nStarRank++;
            }

            return nStarRank;
        }

        return nStarRank;
    }

    public static void SetCareerFavorRequestExistNoti(bool _bExist)
    {
        ZpPlayerPrefs.SetUserUIDWithKey("_CAREERMODE_FAVOR_REQUEST_NOTI");
        ZpPlayerPrefs.DeleteKey("_CAREERMODE_FAVOR_REQUEST_NOTI");
        ZpPlayerPrefs.SetInt("_CAREERMODE_FAVOR_REQUEST_NOTI", _bExist ? 1 : 0);
        ZpPlayerPrefs.Save();
    }

    public static bool IsCareerFavorRequestExistNoti()
    {
        return ZpPlayerPrefs.IsSameSaveUserUID("_CAREERMODE_FAVOR_REQUEST_NOTI") &&
            ZpPlayerPrefs.GetInt("_CAREERMODE_FAVOR_REQUEST_NOTI", 0) == 1;
    }

    public static void SetCareerNewEpisodNoti(int _nEpisod)
    {
        ZpPlayerPrefs.SetUserUIDWithKey("_CAREERMODE_NEW_EPISOD_NOTI");
        ZpPlayerPrefs.DeleteKey("_CAREERMODE_NEW_EPISOD_NOTI");
        ZpPlayerPrefs.SetInt("_CAREERMODE_NEW_EPISOD_NOTI", _nEpisod);
        ZpPlayerPrefs.Save();
    }

    public static bool IsExistCareerNewEpisodNoti()
    {
        return ZpPlayerPrefs.IsSameSaveUserUID("_CAREERMODE_NEW_EPISOD_NOTI") &&
            ZpPlayerPrefs.GetInt("_CAREERMODE_NEW_EPISOD_NOTI", 0) > 1;
    }

    public static int GetCareerNewEpisod()
    {
        int nEpisod = 0;
        if (ZpPlayerPrefs.IsSameSaveUserUID("_CAREERMODE_NEW_EPISOD_NOTI") == true)
            nEpisod = ZpPlayerPrefs.GetInt("_CAREERMODE_NEW_EPISOD_NOTI", 0);
        return nEpisod;
    }

    public static bool IsCareerModeScene()
    {
        if (!ZpGlobals.SceneManagerIsNull() && ZpGlobals.SceneM.CurrentScene != ZpSceneManager.RMScene.CAREER_MODE)
            return false;

        return true;
    }

    public static bool IsCareerModeGame()
    {
        if ((ZpGameGlobals.m_ScriptGM != null && ZpGameGlobals.m_ScriptGM.IsGameMode(KMGameMode.CAREER)) ||
            (!ZpGlobals.PlayerManagerIsNull() && ZpGlobals.PlayerM.GetGameMode() == KMGameMode.CAREER))
            return true;

        return false;
    }

    static public void RequestCareerStageResultInfo()
    {
        if (ZpGlobals.UsingNetwork)
            ZpGlobals.Network.SendTCP_CareerModeWorldMapStart();
    }

    #endregion

    public override void Init()
    {
        DontDestroyOnLoad(this.gameObject);
        m_openedStageInfo = new OpenedStageInfo();

        ZpEventListener.AddListener("PurchaseDailyLimitedStagePlayCount", this);
    }

    public void CareerModeLocalDataLoad()
    {
        if (m_CareerInfo == null)
            m_CareerInfo = new ZpCSVCareer();
        if (m_CareerCutSceneInfo == null)
            m_CareerCutSceneInfo = new ZpCSVCareerCutScene();
        if (m_Result == null)
            m_Result = new Dictionary<int, cCareerEpisodResult>();
    }

    public void StartCareerMode(int _nEpisodStage)
    {
        //	start careermode worldmap
        m_bCareerModeScene = IsCareerModeScene();
        if (m_bCareerModeScene)
        {
            if (_nEpisodStage > LastOpenEpisodStage && GetStageWithIndex(_nEpisodStage) <= MAX_MOVE_ABLE_STAGE)
                LastOpenEpisodStage = _nEpisodStage;
        }
        else
            EndCareerMode();
    }

    public void StartCareerModeGame(int _nEpisodStage)
    {
        //	start careermode game
        CurrentPlayEpisodStage = _nEpisodStage;
        m_nTestRank = 0;

        if (m_ConditionResultDesc == null)
            m_ConditionResultDesc = new Dictionary<ConditionType, KeyValuePair<int, string>>();
        else
            m_ConditionResultDesc.Clear();

        IsConditionMinimumDescSetCompleted = true;
    }

    public void EndCareerMode()
    {
        CurrentPlayEpisodStage = GetEpisodStageWithIndex(0, 0);

        if (m_Result != null)
            m_Result.Clear();
        if (m_ConditionList != null)
            m_ConditionList.Clear();
        if (m_ConditionResultDesc != null)
            m_ConditionResultDesc.Clear();

        CurrentPlayCareerInfo = null;
        GetSetCareerMode = null;
        ResetShowNextStage();

        ZpEventListener.RemoveListener(this);
        Destroy(this.gameObject);

        if (ZpGlobals.PlayerManagerIsNull() == false)
            ZpGlobals.PlayerM.ClearRoom();
    }

    public void SetRestartCareermodeGame()
    {
        CurrentPlayCareerInfo.track.ResetTrackIndex();

        ZpRoomData roomData = ZpGlobals.PlayerM.GetRoomData();
        if (roomData != null)
        {
            //bool bTutorial = ZpGlobals.PlayerM.IsAblePlayTutorial(KMTutorialType.CAREER_MODE_TUTORIAL);
            //if (bTutorial == true)
            //{
            //    CareerStageInfo tutorialStageInfo = null;
            //    if (ZpCareerManager.instance.GetCareerInfo.TryGetCareerInfo(100, out tutorialStageInfo) == true)
            //        roomData.m_TrackID = tutorialStageInfo.track.GetTrackID();
            //    else
            //        roomData.m_TrackID = CurrentPlayCareerInfo.track.GetTrackID();
            //}
            //else
                roomData.m_TrackID = CurrentPlayCareerInfo.track.GetTrackID();
        }
    }

    public int GetEnableMoveOpenNextEpisodStage(bool _bForced = false)
    {
        int nEpisodStage = LastOpenEpisodStage;
        if (_bForced == true)
            return nEpisodStage + 1;

        if (MaxEpisodCount >= LastOpenEpisod)
        {
            if (LastOpenStage < (int)StageType.FAVOR)
            {
                if (MaxEpisodCount == LastOpenEpisod && IsBossStage(LastOpenStage) == true)
                    nEpisodStage = LastOpenEpisodStage;
                else if (IsPrevPlayedCareerModeGame() == false || CurrentPlayEpisodStage == LastOpenEpisodStage)
                    nEpisodStage = LastOpenEpisodStage + 1;
            }
            else if (MaxEpisodCount > LastOpenEpisod)
            {
                if (IsPossibleOpenNextEpisod() == true)
                    nEpisodStage = GetEpisodStageWithIndex(LastOpenEpisod + 1, 1);
            }
            else
                nEpisodStage = GetEpisodStageWithIndex(LastOpenEpisod, (int)StageType.BOSS);
        }

        return nEpisodStage;
    }

    public int GetEnableFrocedOpenNextEpisodStage()
    {
        if (LastOpenStage < (int)StageType.BOSS - 1)
            return LastOpenEpisodStage + 1;
        return LastOpenEpisodStage;
    }

    public bool IsPrevPlayedCareerModeGame()
    {
        //	이전에 careermode game play 했는지
        if (CurrentPlayEpisod < 1)
            return false;
        return true;
    }

    public bool IsPossibleOpenNewStage()
    {
        //	new stage open 가능 여부
        if ((IsPrevPlayedCareerModeGame() && MaxEpisodCount >= LastOpenEpisod))
        {
            //	hidden stage play한 경우 제외
            if ((!IsHiddenStage(CurrentPlayStage) && CurrentPlayEpisodStage == LastOpenEpisodStage))
            {
                //  마지막 에피소드 boss stage play한 경우 hidden stage 안열린 상태에 open 가능하면 true
                if (MaxEpisodCount == CurrentPlayEpisod)
                {
                    if ((IsBossStage(CurrentPlayStage) == false &&
                        GetCareerStageMyRank() > RANK_MIN)
                        || (IsBossStage(CurrentPlayStage) == true &&
                        IsOpenedHiddenStage(CurrentPlayEpisod) == true &&
                        IsPossibleOpenHiddenStage(CurrentPlayEpisod) == true))
                        return true;
                }
                else
                    return GetCareerStageMyRank() > RANK_MIN;
            }
            //  favor check
            else if (IsFavorStage(LastOpenStage) && IsPossibleOpenNextEpisod())
                return true;
            else
            {
                if (IsBossStage(CurrentPlayStage) == true &&
                    IsOpenedHiddenStage(CurrentPlayEpisod) == true &&
                    IsPossibleOpenHiddenStage(CurrentPlayEpisod) == true)
                    return true;
            }
        }
        //	play하지 않고 진입시
        else
        {
            if (GetOpenedStageInfo.IsExistOpenedStage() == true)
                return true;

            //	다음 episod가 있는 경우
            if (MaxEpisodCount > LastOpenEpisod)
            {
                if (IsBossStage(m_nLastOpenStage) && GetCareerStageMyRank(LastOpenStage) > RANK_MIN)
                    return true;

                if (IsFavorStage(LastOpenStage) && IsPossibleOpenNextEpisod())
                    return true;
            }
        }

        return false;
    }

    public bool IsPossibleOpenHiddenStage(int _nEpisod)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            return episodResult.IsPossibleOpenHiddenStage();
        return false;
    }

    public bool IsPossibleOpenFavorStage(int _nEpisod)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            return episodResult.IsPossibleOpenFavorStage();
        return false;
    }

    public bool IsOpenedHiddenStage(int _nEpisod)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            return episodResult.bOpenedHidden;
        return true;
    }

    public bool IsPrevPlayedHiddenStage()
    {
        if (IsPrevPlayedCareerModeGame() && ZpCareerManager.IsHiddenStage(CurrentPlayStage))
            return true;
        return false;
    }

    public void RemoveHiddenStage()
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(CurrentPlayEpisod, out episodResult))
            episodResult.RemoveHiddenStage();
    }

    public void RemoveFavorStage()
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(LastOpenEpisod, out episodResult))
            episodResult.RemoveFavorStage();
    }

    public bool FavorCompleteNotice(int _nEpisod)
    {
        if (_nEpisod <= MaxEpisodCount && _nEpisod == LastOpenEpisod + 1)
        {
            if (m_CareerMode != null && IsCareerModeScene())
            {
                m_CareerMode.FavorCompleteNotice(GetEpisodStageWithIndex(LastOpenEpisod, (int)StageType.FAVOR));
                return true;
            }
        }

        return false;
    }

    public void SetPossibleOpenNextEpisod()
    {
        cCareerEpisodResult episodResult;
        if (LastOpenEpisod < MaxEpisodCount && GetCareerEpisodResult(LastOpenEpisod, out episodResult))
            episodResult.bPossibleOpenNextEpisod = true;
    }

    public bool IsPossibleOpenNextEpisod()
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(LastOpenEpisod, out episodResult))
            return episodResult.IsPossibleOpenNextEpisod();
        return false;
    }

    public bool IsPossibleMoveOpenNewStage()
    {
        return LastOpenEpisodStage != GetEnableMoveOpenNextEpisodStage();
    }

    public string GetConditionResultDesc(ConditionType _type, bool _bUpdate = false)
    {
        string strDesc = string.Empty;
        if (m_ConditionResultDesc.ContainsKey(_type))
        {
            if (IsContainCondition(_type))
            {
                switch (m_ConditionList[_type].Key)
                {
                    default:
                    case ConditionDataType.INTEGER:
                        {
                            cCareerIntegerTypeConditionData condition = CastingIntegerTypeConditionDataClass(m_ConditionList[_type].Value);
                            switch (_type)
                            {
                                case ConditionType.REMAIN_RANK_IN_SECOND:
                                    {
                                        GetNoticeConditionUpdateResultDesc(_type, condition, _bUpdate);
                                        strDesc = m_ConditionResultDesc[_type].Value;
                                    }
                                    break;

                                default:
                                    {
                                        int nValue = condition.GetSetValue;
                                        if (_bUpdate || nValue != m_ConditionResultDesc[_type].Key)
                                            GetNoticeConditionUpdateResultDesc(_type, condition, _bUpdate);
                                        strDesc = m_ConditionResultDesc[_type].Value;
                                    }
                                    break;
                            }
                        }
                        break;
                }
            }
        }
        else
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> condition;
            if (m_ConditionList.TryGetValue(_type, out condition))
            {
                switch (condition.Key)
                {
                    default:
                    case ConditionDataType.INTEGER:
                        strDesc = GetNoticeConditionUpdateResultDesc(_type, CastingIntegerTypeConditionDataClass(condition.Value), _bUpdate);
                        break;
                }
            }
            else
            {
                CareerStageCondition careerCondition = m_CurrentPlayCareerInfo.getConditionFromType(_type);
                strDesc = GetConditionDesc(careerCondition.conditionType, GetConditionValueForUseDesc(careerCondition));
            }
        }

        return strDesc;
    }

    public int GetConditionValueForUseDesc(CareerStageCondition _condition)
    {
        int nValue = 0;
        if (_condition.conditionType == ConditionType.TIME_LIMITED_SUDDENDEATH)
            nValue = (int)m_fCareerTimeCheck;
        else
            nValue = _condition.conditionValue[0];

        return nValue;
    }

    string GetConditionMinimumDesc(ConditionType _type)
    {
        CareerStageCondition careerCondition = m_CurrentPlayCareerInfo.getConditionFromType(_type);
        return GetConditionDesc(careerCondition.conditionType, GetConditionValueForUseDesc(careerCondition), true);
    }

    public string GetUpdateConditionMinimumDesc(ConditionType _type)
    {
        StringBuilder strMinimumDesc = new StringBuilder(GetConditionMinimumDesc(_type));

        if (IsContainCondition(_type))
        {
            switch (m_ConditionList[_type].Key)
            {
                default:
                case ConditionDataType.INTEGER:
                    {
                        cCareerIntegerTypeConditionData condition = CastingIntegerTypeConditionDataClass(m_ConditionList[_type].Value);
                        switch (_type)
                        {
                            default:
                                {
                                    string strDesc = GetNoticeConditionUpdateResultDesc(_type, condition, false);
                                    strDesc = ZpUtility.GetLabelColorString(strDesc, CODITION_NORMAL_COLOR);
                                    strMinimumDesc.Insert(0, strDesc);
                                }
                                break;
                        }
                    }
                    break;
            }
        }

        return strMinimumDesc.ToString();
    }

    void UpdateConditionMinimumDesc(ConditionType _type, bool _bCheckEnd, bool _bCompleted)
    {
        //  update label ui
        if (_bCheckEnd || _bCompleted)
            ZpEventListener.Broadcast("UpdateCareerModeMissionList", _type, _bCompleted);
    }

    public IEnumerator CoroutineUpdateConditionMinimumDescBeforRaceStart()
    {
        //  race 시작전 minimum desc update
        List<CareerStageCondition> conditionList = CurrentPlayCareerInfo.conditionList;
        for (int i = 0 ; i < conditionList.Count ; ++i)
        {
            bool bCheckEnd = false;
            switch (m_ConditionList[conditionList[i].conditionType].Key)
            {
                default:
                case ConditionDataType.INTEGER:
                    bCheckEnd = CastingIntegerTypeConditionDataClass(
                        m_ConditionList[conditionList[i].conditionType].Value).GetSetIsCheckEnd;
                    break;
            }

            UpdateConditionMinimumDesc(conditionList[i].conditionType, bCheckEnd,
                GetConditionIsCheckEndAndCompleted(conditionList[i].conditionType));
            yield return new WaitForSeconds(0.2f);
        }

        IsConditionMinimumDescSetCompleted = true;
    }

    public bool IsRetired()
    {
        if (IsContainCondition(ConditionType.TIME_LIMITED_SUDDENDEATH))
            return GetConditionRank(ConditionType.TIME_LIMITED_SUDDENDEATH) <= RANK_MIN;

        if (IsContainCondition(ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH))
            return GetConditionRank(ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH) <= RANK_MIN;

        return ZpGameGlobals.GetVehicleLOCAL().m_FinalRank >= (int)(byte.MaxValue);
    }

    public bool IsTutorialStage(int _nEpisodStage)
    {
        return _nEpisodStage == ZpTutorial_CAREERMODE.TUTORIAL_STAGE;
    }

    public bool IsPlayingTutorial()
    {
        return false;
        //return ZpGlobals.PlayerManagerIsNull() == false
        //&& ZpGlobals.PlayerM.IsAblePlayTutorial(KMTutorialType.CAREER_MODE_TUTORIAL) == true
        //&& CurrentPlayEpisodStage == ZpTutorial_CAREERMODE.TUTORIAL_STAGE;
    }

    public bool IsPlayedTutorial()
    {
        return IsTutorialStage(CurrentPlayEpisodStage);
    }

    #region UpdateMissingCareerRaceEnd

    private void SetOpenedStageInfoFromLocalData()
    {
        KMCareerStageInfo[] arrayStageInfo = GetOpenedStageInfoInLocal();
        if (arrayStageInfo == null || arrayStageInfo.Length < 1 || m_Result == null)
        {
            GetOpenedStageInfo.ResetValue();
            return;
        }

        CurrentPlayEpisodStage = ZpPlayerPrefs.GetInt("CareerModeEpisodStage");

        //  오픈된 스테이지 정보 셋팅
        for (int i = 0 ; i < arrayStageInfo.Length ; ++i)
        {
            KMCareerStageInfo info = arrayStageInfo[i];
            if (info != null)
            {
                int nStage = GetStageWithIndex(info.StageNumber);

                //  favor episod stage
                if (IsFavorStage(nStage) == true)
                    GetOpenedStageInfo.SetOpenedFavorEpisodStage(info.StageNumber);
                else
                {
                    for (int j = 0 ; j < info.Complete.Length ; ++j)
                    {
                        if (info.Complete[j] == true)
                        {
                            GetOpenedStageInfo.SetIsUpdateLastPlayedEpisodStageRank(info.StageNumber);
                            break;
                        }

                        if (j == info.Complete.Length - 1)
                        {
                            //  hidden episod stage
                            if (IsHiddenStage(nStage) == true)
                                GetOpenedStageInfo.SetOpenedHiddenEpisodStage(info.StageNumber);
                            //  normal episod stage
                            else
                                GetOpenedStageInfo.SetOpenedEpisodStage(info.StageNumber);
                        }
                    }
                }
            }
        }

        AddNewOpenStage(arrayStageInfo);
    }

    public void InitValCareerRaceEnd()
    {
        int CareerModeCompletedLength = ZpPlayerPrefs.GetInt("CareerModeCompletedLength");

        ZpPlayerPrefs.SetInt64("CareerModePlayUserID", 0);
        ZpPlayerPrefs.SetInt("LastPlayCareerMode", 0);
        ZpPlayerPrefs.SetInt("CareerModeEpisodStage", 0);
        ZpPlayerPrefs.SetInt("CareerModeRaceRank", 0);
        ZpPlayerPrefs.SetInt("CareerModeRaceTimeRecord", 0);
        ZpPlayerPrefs.SetInt("CareerModeCompletedLength", 0);
        ZpPlayerPrefs.SetInt("LastOpeneStageInfo_Count", 0);
        ZpPlayerPrefs.SetInt64("LastOpeneStageInfo_UID", 0);

        string StrCompleted = "";
        for (int i = 0 ; i < CareerModeCompletedLength ; ++i)
        {
            StrCompleted = "CareerModeCompleted" + (i + 1);
            ZpPlayerPrefs.SetInt(StrCompleted, 0);
        }
        ZpPlayerPrefs.Save();
    }

    static public bool UpdateValCareerRaceEnd(bool _sendPacket)
    {
        int LastPlayCareerMode = ZpPlayerPrefs.GetInt("LastPlayCareerMode");

        if (LastPlayCareerMode != 1)
            return false;

        long UserID = ZpPlayerPrefs.GetInt64("CareerModePlayUserID");

        //ZpLog.Nor("tempCareerMode:" + UserID + " " + ZpGlobals.PlayerM.SocialUser.m_UserInfo.m_UserUID);
        if (UserID != ZpGlobals.PlayerM.SocialUser.m_UserInfo.m_UserUID)
            return false;

        int CareerModeEpisodStage = ZpPlayerPrefs.GetInt("CareerModeEpisodStage");
        int CareerModeRaceRank = ZpPlayerPrefs.GetInt("CareerModeRaceRank");
        int CareerModeRaceTimeRecord = ZpPlayerPrefs.GetInt("CareerModeRaceTimeRecord");

        if (CareerModeRaceRank == 255 || CareerModeRaceTimeRecord == 0)
            return false;

        if (_sendPacket == true)
        {
            List<bool> completedList = new List<bool>();
            string StrCompleted = "";
            int CareerModeCompletedNum = 0;
            int CareerModeCompletedLength = ZpPlayerPrefs.GetInt("CareerModeCompletedLength");

            for (int i = 0 ; i < CareerModeCompletedLength ; ++i)
            {
                StrCompleted = "CareerModeCompleted" + (i + 1);
                CareerModeCompletedNum = ZpPlayerPrefs.GetInt(StrCompleted);
                if (CareerModeCompletedNum == 1)
                    completedList.Add(true);
                else
                    completedList.Add(false);
            }

            //ZpLog.Nor("tempCareerMode:" + LastPlayCareerMode + " " + CareerModeEpisodStage + " " + CareerModeRaceRank + " " + CareerModeRaceTimeRecord + " " + CareerModeCompletedLength);
            //ZpLog.Nor("tempCareerMode:" + completedList[0] + " " + completedList[1] + " " + completedList[2]);

            bool[] arrayCompleted = completedList.ToArray();
            KMCareerStageInfo stageInfo = new KMCareerStageInfo((short)CareerModeEpisodStage, arrayCompleted);
            SaveLocalOpenedStageInfo(new KMCareerStageInfo[] { stageInfo }, false);

            if (ZpGlobals.UsingNetwork)
                ZpGlobals.Network.SendTCP_UpdateCareerResult(CareerModeEpisodStage, arrayCompleted, CareerModeRaceRank, CareerModeRaceTimeRecord);
        }

        return true;
    }

    static public void ReceiveCareerRaceEndReward()
    {
        ZpPlayerPrefs.SetInt("LastPlayCareerMode", 0);
    }

    static public void SaveLocalOpenedStageInfo(KMCareerStageInfo[] _arrayOpenStageInfo, bool _bFromServer)
    {
        if (_arrayOpenStageInfo == null || _arrayOpenStageInfo.Length < 1)
            return;

        //if (_bFromServer == true)
        //{
        //    KMCareerStageInfo[] arrayPrevData = GetOpenedStageInfoInLocal();
        //    if (arrayPrevData != null && arrayPrevData.Length > 0 && arrayPrevData.Length <= _arrayOpenStageInfo.Length)
        //    {
        //        for (int i = 0 ; i < _arrayOpenStageInfo.Length ; ++i)
        //        {
        //            KMCareerStageInfo prevStageInfo = arrayPrevData[i];
        //            KMCareerStageInfo stageInfo = _arrayOpenStageInfo[i];
        //            if (prevStageInfo.StageNumber == stageInfo.StageNumber)
        //            {

        //            }
        //        }
        //    }
        //}

        ZpPlayerPrefs.SetInt64("LastOpeneStageInfo_UID", ZpGlobals.PlayerM.SocialUser.m_UserInfo.m_UserUID);
        ZpPlayerPrefs.SetInt("LastOpeneStageInfo_Count", _arrayOpenStageInfo.Length);
        for (int i = 0 ; i < _arrayOpenStageInfo.Length ; ++i)
        {
            KMCareerStageInfo stageInfo = _arrayOpenStageInfo[i];
            ZpPlayerPrefs.SetInt(string.Format("LastOpeneStageInfo_EpisodStage_{0}", i), stageInfo.StageNumber);
            ZpPlayerPrefs.SetInt(string.Format("LastOpeneStageInfo_ConditionCount_{0}", i), stageInfo.Complete.Length);
            for (int j = 0 ; j < stageInfo.Complete.Length ; ++j)
            {
                ZpPlayerPrefs.SetInt(string.Format("LastOpeneStageInfo_ConditionCompleted_{0}_{1}", i, j),
                    stageInfo.Complete[j] == true ? 1 : 0);
            }
        }

        ZpPlayerPrefs.Save();
    }

    static private KMCareerStageInfo[] GetOpenedStageInfoInLocal()
    {
        long UserID = ZpPlayerPrefs.GetInt64("LastOpeneStageInfo_UID");
        if (UserID != ZpGlobals.PlayerM.SocialUser.m_UserInfo.m_UserUID)
            return null;

        int nCount = ZpPlayerPrefs.GetInt("LastOpeneStageInfo_Count");
        if (nCount < 1)
            return null;

        KMCareerStageInfo[] arrayOpenedStageInfo = new KMCareerStageInfo[nCount];

        for (int i = 0 ; i < arrayOpenedStageInfo.Length ; ++i)
        {
            int nEpisodStage = ZpPlayerPrefs.GetInt(string.Format("LastOpeneStageInfo_EpisodStage_{0}", i));
            int nConditionCount = ZpPlayerPrefs.GetInt(string.Format("LastOpeneStageInfo_ConditionCount_{0}", i));
            bool[] arrayCompleted = new bool[nConditionCount];

            for (int j = 0 ; j < arrayCompleted.Length ; ++j)
            {
                arrayCompleted[j] = ZpPlayerPrefs.GetInt(
                    string.Format("LastOpeneStageInfo_ConditionCompleted_{0}_{1}", i, j)) == 1 ? true : false;
            }

            arrayOpenedStageInfo[i] = new KMCareerStageInfo((short)nEpisodStage, arrayCompleted);
        }

        return arrayOpenedStageInfo;
    }

    #endregion

    #region Show Next Stage

    public void ResetShowNextStage()
    {
        m_nShowNextEpisodStage = 0;
        m_bShowNextEpisodStage = false;
    }

    public bool ShowNextStage()
    {
        if (IsShowNextStage() == true)
        {
            if (IsFavorStage(GetStageWithIndex(m_nShowNextEpisodStage)) == true)
                GetSetCareerMode.ShowFavorDialog(m_nShowNextEpisodStage);
            else
                GetSetCareerMode.ShowStageWindow(m_nShowNextEpisodStage);

            GetSetCareerMode.m_bPlayStageAnimation = false;
            GetSetCareerMode.m_DragCam.SetStageAnimationFlag(false);
            ResetShowNextStage();

            return true;
        }

        return false;
    }

    public void SetShowNextEpisodStage()
    {
        m_bShowNextEpisodStage = true;
    }

    private void SetShowNextStage(int _nEpisodStage, bool _bSetForce = false)
    {
        if (m_bShowNextEpisodStage == true)
            return;

        if (_nEpisodStage != 0 && (m_nShowNextEpisodStage == 0 || _bSetForce == true) && 
            _nEpisodStage != CurrentPlayEpisodStage && IsPrevPlayedCareerModeGame() == true)
        {
            m_nShowNextEpisodStage = _nEpisodStage;

#if UNITY_EDITOR
            ZpLog.Normal(ZpLog.E_Category.Careeer, "[ZpCareerManager] SetShowNextStage - " + _nEpisodStage);
#endif
        }
    }

    public void SetShowNextStage( cCareerStageResult _cCurrentStageResult = null )
    {
        if (IsPrevPlayedCareerModeGame() == true && MaxEpisodCount >= LastOpenEpisod)
        {
            int nShowNextStage = 0;

            StageType eCurrentPlayStageType = GetStageType(CurrentPlayStage);
            switch (eCurrentPlayStageType)
            {
                case StageType.NORMAL:
                    if (CurrentPlayEpisod <= LastOpenEpisod)
                    {
						if ((null != _cCurrentStageResult && 0 < _cCurrentStageResult.ownRank
                            && _cCurrentStageResult.stage == CurrentPlayStage) || 
                            (CurrentPlayStage < LastOpenStage || CurrentPlayEpisod < LastOpenEpisod))
							nShowNextStage = CurrentPlayEpisodStage + 1;
                    }
                    break;

                case StageType.BOSS:
                case StageType.HIDDEN:
                    if (LastOpenEpisod < MaxEpisodCount)
                    {
                        //  favor로 이동
                        if (IsPossibleOpenFavorStage(CurrentPlayEpisod) == true)
                        {
                            if (IsBossStage(eCurrentPlayStageType) == true)
                            {
                                int nHiddenStage = GetEpisodStageWithIndex(CurrentPlayEpisod, (int)StageType.HIDDEN);
                                if (IsPossiblePlayPlayCountLimitedStage(nHiddenStage) == true)
                                    nShowNextStage = nHiddenStage;
                                else
                                    nShowNextStage = CurrentPlayEpisodStage + 1;
                            }
                            else
                                nShowNextStage = GetEpisodStageWithIndex(CurrentPlayEpisod, (int)StageType.FAVOR);
                        }
                        //  다음 episod 1 stage로 이동
                        else if (CurrentPlayEpisod < LastOpenEpisod)
                            nShowNextStage = GetEpisodStageWithIndex(CurrentPlayEpisod + 1, 1);
                    }
                    else if (LastOpenEpisod == MaxEpisodCount)
                    {
                        //  다음 episod 1 stage로 이동
                        if (CurrentPlayEpisod < LastOpenEpisod)
                        {
                            int nHiddenStage = GetEpisodStageWithIndex(CurrentPlayEpisod, (int)StageType.HIDDEN);
                            if (IsBossStage(eCurrentPlayStageType) == true && IsPossiblePlayPlayCountLimitedStage(nHiddenStage) == true)
                                nShowNextStage = nHiddenStage;
                            else
                                nShowNextStage = GetEpisodStageWithIndex(CurrentPlayEpisod + 1, 1);
                        }
                    }
                    break;
            }

            SetShowNextStage(nShowNextStage);
        }
    }

    private bool IsShowNextStage()
    {
        return m_bShowNextEpisodStage == true && IsPossibleShowNextStage() == true;
    }

    public bool IsPossibleShowNextStage()
    {
        return m_nShowNextEpisodStage != 0;
    }

    #endregion
}
