using UnityEngine;
using System.Collections.Generic;
using KMNetClass;

public partial class ZpCareerManager : ZpSingleton<ZpCareerManager>
{
#region career episod stage result info

    public class cCareerStageResult
    {
        public int stage;
        public int ownRank;
        public bool[] arrayCompleted;

        public cCareerStageResult() { }
        public cCareerStageResult(KMCareerStageInfo _StageInfo)
        {
            stage = GetStageWithIndex(_StageInfo.StageNumber);
            int nStarCount = GetStarRankCountByCompletedList(_StageInfo.Complete);
            if (ZpCareerManager.IsFavorStage(stage) && nStarCount != RANK_MIN)
                ownRank = RANK_MIN;
            else
                ownRank = nStarCount;

            SetCompletedCondition(_StageInfo.Complete);
        }

        public void SetCompletedCondition(bool[] _arrayCompleted)
        {
            arrayCompleted = new bool[_arrayCompleted.Length];
            _arrayCompleted.CopyTo(arrayCompleted, 0);
        }

        public void UpdateCompletedCondition(bool[] _arrayCompleted)
        {
            if (arrayCompleted == null)
                arrayCompleted = new bool[_arrayCompleted.Length];

            if (arrayCompleted.Length != _arrayCompleted.Length)
            {
                ZpLog.Err("[CareerManager] SetCompletedCondition length different");
                return;
            }

            int nRank = 0;
            for (int i = 0 ; i < arrayCompleted.Length ; ++i)
            {
                bool bPrev = arrayCompleted[i];
                bool bCurrent = _arrayCompleted[i];
                if (bPrev == false && bCurrent == true)
                    arrayCompleted[i] = bCurrent;
                if (bPrev == true || bCurrent == true)
                    nRank++;
            }

            ownRank = Mathf.Clamp(nRank, RANK_MIN, RANK_MAX);
        }
    }

    public class cCareerPlayCountLimitedStageResult : cCareerStageResult
    {
        private int nLimitedCount;
        public int GetLimitedCount { get { return nLimitedCount; } }
        private int nPossiblePlayCount;
        public int GetPossiblePlayCount { get { return nPossiblePlayCount; } }

        private int nPossiblePlayCountFromServer;
        public int SetPossiblePlayCountFromServer { set { nPossiblePlayCountFromServer = value; } }

        public cCareerPlayCountLimitedStageResult(int _nLimitedCount, KMCareerStageInfo _StageInfo)
            : base(_StageInfo)
        {
            nLimitedCount = _nLimitedCount;
            nPossiblePlayCountFromServer = int.MinValue;
        }

        public void SetPlayCountLimitedStageInfo(int _nPossiblePlayCount = -1)
        {
            SetPossiblePlayCount(_nPossiblePlayCount == -1 ? nPossiblePlayCountFromServer : _nPossiblePlayCount);
        }

        public void SetPossiblePlayCount(int _nPossiblePlayCount)
        {
            nPossiblePlayCount = _nPossiblePlayCount;
        }

        public void Play()
        {
            SetPossiblePlayCount(Mathf.Clamp(nPossiblePlayCount - 1, 0, nLimitedCount));
        }

        public bool IsPossiblePlay()
        {
            return nPossiblePlayCount > 0;
        }

        public bool IsSetPlayCountLimitedStageInfoFromServer()
        {
            return nPossiblePlayCountFromServer != int.MinValue;
        }
    }

    public class cCareerEpisodResult
    {
        public int episod;
        public bool bOpenedHidden;
        public bool bPlayHidden;
        public bool bOpenedFavor;
        public bool bPossibleOpenNextEpisod;
        public Dictionary<int, cCareerStageResult> stageResultList; // tagCareerStageResult[STAGE_MAX]

        public cCareerEpisodResult(int _episod)
        {
            episod = _episod;
            bPlayHidden = false;
            bOpenedHidden = false;
            bOpenedFavor = false;
            bPossibleOpenNextEpisod = false;
            stageResultList = new Dictionary<int, cCareerStageResult>();
        }

        public bool IsPossibleOpenHiddenStage()
        {
            if (/*!bPlayHidden && */stageResultList.Count >= (int)StageType.BOSS)
            {
                foreach (cCareerStageResult stage in stageResultList.Values)
                {
                    if (stage.stage <= (int)StageType.BOSS && stage.ownRank < RANK_MAX)
                        return false;
                }

                return true;
            }

            return false;
        }

        public void AddStage(int _nStage, cCareerStageResult _stage)
        {
            if (stageResultList.ContainsKey(_nStage) == true)
                stageResultList[_nStage] = _stage;
            else
                stageResultList.Add(_nStage, _stage);
        }

        public void RemoveHiddenStage()
        {
            if (stageResultList.ContainsKey((int)StageType.HIDDEN) == true)
                stageResultList.Remove((int)StageType.HIDDEN);
        }

        public bool IsPossibleOpenFavorStage()
        {
            return bOpenedFavor;
        }

        public bool IsPossibleOpenNextEpisod()
        {
            return bPossibleOpenNextEpisod;
        }

        public void RemoveFavorStage()
        {
            if (stageResultList.ContainsKey((int)StageType.FAVOR) == true)
            {
                stageResultList.Remove((int)StageType.FAVOR);
                bOpenedFavor = false;
            }
        }

        public cCareerPlayCountLimitedStageResult GetPlayCountLimitedStage(int _nStage)
        {
            cCareerStageResult stage;
            if (stageResultList.TryGetValue(_nStage, out stage) == true)
                return stage as cCareerPlayCountLimitedStageResult;
            return null;
        }

        public void UpdatePlayCountLimitedStageInfo(int _nStage)
        {
            if (IsSetPlayCountLimitedStageInfoFromServer(_nStage) == true)
                SetPlayCountLimitedStageInfo(_nStage);
        }

        public void SetPlayCountLimitedStageInfo(int _nStage, int _nPossiblePlayCount = -1)
        {
            cCareerPlayCountLimitedStageResult stage = GetPlayCountLimitedStage(_nStage);
            if (stage != null)
                stage.SetPlayCountLimitedStageInfo(_nPossiblePlayCount);
        }

        public bool IsPossiblePlayCountLimitedStage(int _nStage)
        {
            cCareerPlayCountLimitedStageResult stage = GetPlayCountLimitedStage(_nStage);
            if (stage != null)
                return stage.IsPossiblePlay();
            return false;
        }

        public void SetPossiblePlayCountFromServer(int _nStage, int _nPossiblePlayCount)
        {
            cCareerPlayCountLimitedStageResult stage = GetPlayCountLimitedStage(_nStage);
            if (stage != null)
                stage.SetPossiblePlayCountFromServer = _nPossiblePlayCount;
        }

        public bool IsSetPlayCountLimitedStageInfoFromServer(int _nStage)
        {
            cCareerPlayCountLimitedStageResult stage = GetPlayCountLimitedStage(_nStage);
            if (stage != null)
                return stage.IsSetPlayCountLimitedStageInfoFromServer();
            return false;
        }
    }

    public class OpenedStageInfo
    {
        public int nLastPlayedEpisodStage;
        public int nOpenedHiddenEpisodStage;
        private int nOpenedEpisodStage;
        private int nOpenedFavorEpisodStage;
        private bool bUpdateLastPlayedEpisodStageRank;

        const string strLastPlayedEpisodStageKey = "CareerModeLastPlayedEpisodStage";
        const string strOpenedEpisodStageKey = "CareerModeOpenedEpisodStage";
        const string strOpenedFavorEpisodStageKey = "CareerModeOpenedFavorEpisodStage";
        const string strOpenedHiddenEpisodStageKey = "CareerModeOpenedHiddenEpisodStage";
        const string strIsUpdateLastPlayedEpisodStageKey = "CareerModeIsUpdateLastPlayedEpisodStage";

        public void ResetValue()
        {
            bUpdateLastPlayedEpisodStageRank = false;
            nLastPlayedEpisodStage = 0;
            nOpenedEpisodStage = 0;
            nOpenedFavorEpisodStage = 0;
            nOpenedHiddenEpisodStage = 0;

            ZpPlayerPrefs.SetUserUIDWithKey(strIsUpdateLastPlayedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strIsUpdateLastPlayedEpisodStageKey, 0);
            ZpPlayerPrefs.SetUserUIDWithKey(strLastPlayedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strLastPlayedEpisodStageKey, 0);
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedEpisodStageKey, 0);
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedFavorEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedFavorEpisodStageKey, 0);
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedHiddenEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedHiddenEpisodStageKey, 0);
            ZpPlayerPrefs.Save();
        }

        public void GetValue()
        {
            if (ZpPlayerPrefs.IsSameSaveUserUID(strIsUpdateLastPlayedEpisodStageKey) == true)
                bUpdateLastPlayedEpisodStageRank = ZpPlayerPrefs.GetInt(strIsUpdateLastPlayedEpisodStageKey) == 1 ? true : false;
            if (ZpPlayerPrefs.IsSameSaveUserUID(strLastPlayedEpisodStageKey) == true)
                nLastPlayedEpisodStage = ZpPlayerPrefs.GetInt(strLastPlayedEpisodStageKey);
            if (IsOpenedNormalStage() == false && ZpPlayerPrefs.IsSameSaveUserUID(strOpenedEpisodStageKey) == true)
                nOpenedEpisodStage = ZpPlayerPrefs.GetInt(strOpenedEpisodStageKey);
            if (IsOpenedFavorStage() == false && ZpPlayerPrefs.IsSameSaveUserUID(strOpenedFavorEpisodStageKey) == true)
                nOpenedFavorEpisodStage = ZpPlayerPrefs.GetInt(strOpenedFavorEpisodStageKey);
            if (IsOpenedHiddenStage() == false && ZpPlayerPrefs.IsSameSaveUserUID(strOpenedHiddenEpisodStageKey) == true)
                nOpenedHiddenEpisodStage = ZpPlayerPrefs.GetInt(strOpenedHiddenEpisodStageKey);
        }

        public void SetLastPlayedEpisodStage(int _nLastPlayedEpisodStage)
        {
            nLastPlayedEpisodStage = _nLastPlayedEpisodStage;
            ZpPlayerPrefs.SetUserUIDWithKey(strLastPlayedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strLastPlayedEpisodStageKey, _nLastPlayedEpisodStage);
            ZpPlayerPrefs.Save();
        }

        public void SetOpenedEpisodStage(int _nOpenedEpisodStage)
        {
            nOpenedEpisodStage = _nOpenedEpisodStage;
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedEpisodStageKey, _nOpenedEpisodStage);
            ZpPlayerPrefs.Save();
        }

        public void SetOpenedFavorEpisodStage(int _nOpenedFavorEpisodStage)
        {
            nOpenedFavorEpisodStage = _nOpenedFavorEpisodStage;
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedFavorEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedFavorEpisodStageKey, _nOpenedFavorEpisodStage);
            ZpPlayerPrefs.Save();
        }

        public void SetOpenedHiddenEpisodStage(int _nOpenedHiddenEpisodStage)
        {
            nOpenedHiddenEpisodStage = _nOpenedHiddenEpisodStage;
            ZpPlayerPrefs.SetUserUIDWithKey(strOpenedHiddenEpisodStageKey);
            ZpPlayerPrefs.SetInt(strOpenedHiddenEpisodStageKey, _nOpenedHiddenEpisodStage);
            ZpPlayerPrefs.Save();
        }

        public void SetIsUpdateLastPlayedEpisodStageRank(int _nLastPlayedEpisodStage)
        {
            SetLastPlayedEpisodStage(_nLastPlayedEpisodStage);

            nLastPlayedEpisodStage = _nLastPlayedEpisodStage;
            ZpPlayerPrefs.SetUserUIDWithKey(strIsUpdateLastPlayedEpisodStageKey);
            ZpPlayerPrefs.SetInt(strIsUpdateLastPlayedEpisodStageKey, 1);
            ZpPlayerPrefs.Save();
        }

        public bool IsOpenedNormalStage()
        {
            return nOpenedEpisodStage != 0;
        }

        public bool IsOpenedFavorStage()
        {
            return nOpenedFavorEpisodStage != 0;
        }

        public bool IsOpenedHiddenStage()
        {
            return nOpenedHiddenEpisodStage != 0;
        }

        public bool IsExistOpenedStage()
        {
            return IsOpenedNormalStage() == true || IsOpenedFavorStage() == true || IsOpenedHiddenStage() == true;
        }

        public bool IsUpdateLastPlayedEpisodStageRank()
        {
            return bUpdateLastPlayedEpisodStageRank;
        }

        public int GetLastPlayedEpisodStage()
        {
            int nEpisod = 0;
            int nStage = 0;

            if (IsOpenedNormalStage() == true)
            {
                nEpisod = GetEpisodWithIndex(nOpenedEpisodStage);
                nStage = Mathf.Max(1, GetStageWithIndex(nOpenedEpisodStage) - 1);
            }
            else if (IsOpenedFavorStage() == true)
            {
                nEpisod = GetEpisodWithIndex(nOpenedFavorEpisodStage);
                nStage = GetStageWithIndex(nOpenedFavorEpisodStage) - 1;
            }

            return GetEpisodStageWithIndex(nEpisod, nStage);
        }
    }

#endregion

#region Play count limited stage

    int CreatePlayCountLimitedStagePlayCount(int _nStage)
    {
        int nLimitedCount = 0;
        switch (GetStageType(_nStage))
        {
            case StageType.HIDDEN:
                nLimitedCount = (int)ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("Career_Hidden_Play_Limited_Count");
                break;
            case StageType.DAILY_LIMITED:
                nLimitedCount = (int)ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("Career_Daily_Play_Limited_Count");
                break;
        }

        return nLimitedCount;
    }

    cCareerPlayCountLimitedStageResult CreatePlayCountLimitedStageResult(KMCareerStageInfo _stageInfo, int _nLimitedCount)
    {
        return new cCareerPlayCountLimitedStageResult(_nLimitedCount, _stageInfo);
    }

    cCareerPlayCountLimitedStageResult CreatePlayCountLimitedStageResult(int _nEpisod, int _nStage, int _nLimitedCount)
    {
        int nEpisodStage = ZpCareerManager.GetEpisodStageWithIndex(_nEpisod, _nStage);
        bool[] arrayCondition = new bool[m_CareerInfo.GetConditionListCount(nEpisodStage)];
        KMCareerStageInfo stage = new KMCareerStageInfo((short)nEpisodStage, arrayCondition);
        return new cCareerPlayCountLimitedStageResult(_nLimitedCount, stage);
    }

    cCareerPlayCountLimitedStageResult CreatePlayCountLimitedStageResult(int _nEpisodStage, int _nLimitedCount)
    {
        return CreatePlayCountLimitedStageResult(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage), _nLimitedCount);
    }

    cCareerPlayCountLimitedStageResult CreatePlayCountLimitedStageResult(KMDailyStageInfo _info)
    {
        if (_info != null)
        {
            cCareerPlayCountLimitedStageResult stage = CreatePlayCountLimitedStageResult(_info.StageNumber,
                CreatePlayCountLimitedStagePlayCount(GetStageWithIndex(_info.StageNumber)));
            if (stage != null)
                stage.SetPlayCountLimitedStageInfo(_info.RemainCount);
            return stage;
        }

        return null;
    }

    private cCareerPlayCountLimitedStageResult GetPlayCountLimitedStageResult(int _nEpisod, int _nStage)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            return episodResult.GetPlayCountLimitedStage(_nStage);
        return null;
    }

    private cCareerPlayCountLimitedStageResult GetPlayCountLimitedStageResult(int _nEpisodStage)
    {
        return GetPlayCountLimitedStageResult(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage));
    }

    public void ShowDailyLimitedStagePlayCountPurchasePopup(int _nEpisodStage)
    {
        string strMoney = ZpUIHelper.GetValueToCommaNumber(
            ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("Career_Daily_Play_Price"));
        string strMessage = string.Format(ZpGlobalText.Instance.GetString(
            "Career_DailyStage_Purchase_PlayCount_Message"), strMoney);

        ZpPopupDialogBase.m_bForceStartClickPos = true;
        GameObject popup = ZpGlobals.GlobalGUI.CreateDialog(ZpGlobalGUI.DialogType.MessageBoxYesOrNo,
            "PurchaseDailyLimitedStagePlayCount", "",
            strMessage, ZpGlobalText.Instance.GetString("Career_DailyStage_Purchase_PlayCount_Title"),
            new ZpParameter(new object[] { _nEpisodStage }));
        if (popup != null)
        {
            ZpMessageBox messageBox = popup.GetComponent<ZpMessageBox>();
            if (messageBox != null)
            {
                messageBox.SetMessageBoxOption(
                    new ZpMessageBox.MessageBoxOption(false, true, false, false, true, null, null,
                        null, ZpUtility.ConvertToString(ZpMessageBox.ICON_NAME.icon_star_up), null, strMoney));
            }
        }
    }

    void PurchaseDailyLimitedStagePlayCount(ZpParameter _param)
    {
        if (ZpGlobals.UsingNetwork)
        {
            long price;
            if (ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.TryGetValue("Career_Daily_Play_Price", out price))
            {
                if (ZpItemUtil.CheckEnoughCash((int)price) == true)
                    ZpGlobals.Network.SendTCP_CareerModePurchaseDailyLimitedStagePlayCount((int)_param[0]);
                else
                    ZpGlobals.GlobalGUI.CreateTweetBox(ZpGlobals.s_ScriptCSVGlobalText.GetString("Goods_NotEnough_Gold"));
            }
        }
    }

    public void UpdatePlayCountLimitedStageInfo(int _nEpisod, int _nStage)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult) == true)
        {
            if (episodResult != null)
                episodResult.UpdatePlayCountLimitedStageInfo(_nStage);
        }
    }

    public void UpdatePlayCountLimitedStageInfo(int _nEpisodStage)
    {
        UpdatePlayCountLimitedStageInfo(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage));
    }

    public void UpdatePlayCountLimitedStageInfo(int _nEpisod, int _nStage, int _nPlayedCount, bool _bSetStageInWorld = false)
    {
        cCareerPlayCountLimitedStageResult stage = GetPlayCountLimitedStageResult(_nEpisod, _nStage);
        if (stage != null)
        {
            stage.SetPlayCountLimitedStageInfo(_nPlayedCount);

            if (GetSetCareerMode != null && _bSetStageInWorld == true)
                GetSetCareerMode.UpdateStageInfo(ZpCareerManager.GetEpisodStageWithIndex(_nEpisod, (int)StageType.DAILY_LIMITED));
        }
    }

    public void UpdatePlayCountLimitedStageInfo(int _nEpisodStage, int _nPlayedCount, bool _bSetStageInWorld)
    {
        UpdatePlayCountLimitedStageInfo(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage), _nPlayedCount, _bSetStageInWorld);
    }

    public void UpdatePlayCountLimitedStageInfo(KMDailyStageInfo[] _arrayPlayCountLimitedStageInfo)
    {
        if (_arrayPlayCountLimitedStageInfo != null)
        {
            for (int i = 0 ; i < _arrayPlayCountLimitedStageInfo.Length ; ++i)
                UpdatePlayCountLimitedStageInfo(_arrayPlayCountLimitedStageInfo[i]);
        }
    }

    public void UpdatePlayCountLimitedStageInfo(KMDailyStageInfo _playCountLimitedStageInfo)
    {
        if (_playCountLimitedStageInfo != null)
        {
            int nEpisod = GetEpisodWithIndex(_playCountLimitedStageInfo.StageNumber);
            cCareerEpisodResult episodResult;
            if (GetCareerEpisodResult(nEpisod, out episodResult) == false)
            {
                AddNewOpenStage(GetCreateStageInfo(_playCountLimitedStageInfo.StageNumber));
                if (GetCareerEpisodResult(nEpisod, out episodResult) == false)
                    return;
            }

            if (episodResult != null)
            {
                int nStage = GetStageWithIndex(_playCountLimitedStageInfo.StageNumber);
                bool bSetStageInWorld = episodResult.IsSetPlayCountLimitedStageInfoFromServer(nStage);
                episodResult.SetPossiblePlayCountFromServer(nStage, _playCountLimitedStageInfo.RemainCount);
                episodResult.UpdatePlayCountLimitedStageInfo(nStage);

                if (GetSetCareerMode != null && bSetStageInWorld == true)
                    GetSetCareerMode.UpdateStageInfo(_playCountLimitedStageInfo.StageNumber);
            }
        }
    }

    public bool IsPossiblePlayPlayCountLimitedStage(int _nEpisod, int _nStage)
    {
        cCareerEpisodResult episodResult;
        if (_nEpisod <= LastOpenEpisod && GetCareerEpisodResult(_nEpisod, out episodResult))
        {
            if (IsHiddenStage(_nStage) == true)
                return episodResult.bOpenedHidden == true && episodResult.IsPossiblePlayCountLimitedStage(_nStage) == true;
            else
                return episodResult.IsPossiblePlayCountLimitedStage(_nStage) == true;
        }
        return false;
    }

    public bool IsPossiblePlayPlayCountLimitedStage(int _nEpisodStage)
    {
        return IsPossiblePlayPlayCountLimitedStage(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage));
    }

    public string GetPlayCountLimitedStagePlayCountString(int _nEpisod, int _nStage)
    {
        if (_nEpisod <= LastOpenEpisod)
        {
            cCareerPlayCountLimitedStageResult limitedStage = GetPlayCountLimitedStageResult(_nEpisod, _nStage);
            if (limitedStage != null)
            {
                string strCount = string.Format(ZpGlobals.s_ScriptCSVGlobalText.GetString("Count_TotalCount"),
                    limitedStage.GetPossiblePlayCount, limitedStage.GetLimitedCount);
                if (limitedStage.IsPossiblePlay() == false)
                    strCount = strCount.Insert(0, "[FF0000]");
                return strCount;
            }
        }
        else
        {
            return string.Format(ZpGlobals.s_ScriptCSVGlobalText.GetString("Count_TotalCount"),
                0, ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("Career_Daily_Play_Limited_Count")).Insert(0, "[FF0000]");
        }

        return string.Empty;
    }

    public string GetPlayCountLimitedStagePlayCountString(int _nEpisodStage)
    {
        return GetPlayCountLimitedStagePlayCountString(ZpCareerManager.GetEpisodWithIndex(_nEpisodStage),
            ZpCareerManager.GetStageWithIndex(_nEpisodStage));
    }

#endregion

    public bool UpdateCareerResult(float _fLapTime)
    {
        return UpdateCareerResult(m_nCurrentPlayEpisodStage, _fLapTime);
    }

    public bool UpdateCareerResult(int _nEpisod, int _nStage, float _fLapTime)
    {
        return UpdateCareerResult(ZpCareerManager.GetEpisodStageWithIndex(_nEpisod, _nStage), _fLapTime);
    }

    public bool UpdateCareerResult(int _nEpisodStage, float _fLapTime)
    {
        SaveLocalDataCareerModeGuideShowCount();
        if (_nEpisodStage == CurrentPlayEpisodStage && _fLapTime != Mathf.Infinity)
            return UpdateCurrentPlayCareerRank();
        return false;
    }

    public bool UpdateCurrentPlayCareerRank()
    {
        cCareerStageResult stageResult;
        if (GetCareerStageResult(CurrentPlayEpisodStage, out stageResult) == false)
            return false;

        stageResult.stage = CurrentPlayStage;

        int nCurrentRank;
        ConditionType[] arrayCompletedConditionType = GetCurrentCareerResultRankAndCompletedCondition(out nCurrentRank);

        //  game play condition list ui update
        foreach (ConditionType type in m_ConditionList.Keys)
        {
            bool bCompleted = false;
            for (int i = 0 ; i < arrayCompletedConditionType.Length ; ++i)
            {
                if (type == arrayCompletedConditionType[i])
                {
                    bCompleted = true;
                    break;
                }
            }

            UpdateConditionMinimumDesc(type, true, bCompleted);
        }

        //if (stageResult.ownRank < nCurrentRank)
        //{
        bool[] arrayCompleted = GetCurrentCareerResultCompletedList();
        CurrentPlayStagePrevRank = stageResult.ownRank;
        stageResult.UpdateCompletedCondition(arrayCompleted);
        m_CurrentPlayCareerInfo.myRank = stageResult.ownRank;

        //  오픈된 마지막 에피소드의 보스 스테이지 최초 클리어한 경우 다음 에피소드 업데이트 예정 팝업 show 체크
        if (CurrentPlayEpisod == MaxEpisodCount && IsBossStage(CurrentPlayStage) == true &&
            CurrentPlayStagePrevRank == RANK_MIN && stageResult.ownRank > RANK_MIN)
            IsShowPopupWillUpdateNextEpisod = true;

        SetCareerStageResult(CurrentPlayEpisodStage, stageResult);
		SetShowNextStage(stageResult);
        //#if UNITY_EDITOR
        //            ZpLog.Normal(ZpLog.E_Category.Careeer, "@@ Update Career current Rank : " + nCurrentRank.ToString());
        //#endif
        //        }
        //#if UNITY_EDITOR
        //        else
        //            ZpLog.Normal(ZpLog.E_Category.Careeer, "@@ Update Career current Rank : Update failed");
        //#endif

        return true;
    }

    public void AddCareerRank()
    {
        m_nTestRank = Mathf.Clamp(++m_nTestRank, RANK_MIN, RANK_MAX);
    }

    public bool GetCareerEpisodResult(int _nEpisod, out cCareerEpisodResult _episodResult)
    {
        return m_Result.TryGetValue(_nEpisod, out _episodResult);
    }

    public bool GetCareerStageResult(int _nEpisod, int _nStage, out cCareerStageResult _StageResult)
    {
        cCareerEpisodResult result;
        if (GetCareerEpisodResult(_nEpisod, out result))
            return result.stageResultList.TryGetValue(_nStage, out _StageResult);

        _StageResult = new cCareerStageResult();
        return false;
    }

    public bool GetCareerStageResult(int _nEpisodStage, out cCareerStageResult _StageResult)
    {
        return GetCareerStageResult(GetEpisodWithIndex(_nEpisodStage), GetStageWithIndex(_nEpisodStage), out _StageResult);
    }

    void SetCareerStageResult(int _nEpisod, int _nStage, cCareerStageResult _StageResult)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
        {
            cCareerStageResult stageResult;
            if (episodResult.stageResultList.TryGetValue(_nStage, out stageResult) == true)
            {
                stageResult = _StageResult;

#if UNITY_EDITOR
                ZpLog.Normal(ZpLog.E_Category.Careeer, "Update stage result - " + GetEpisodStageWithIndex(_nEpisod, _nStage));
#endif
            }
            else
            {
                episodResult.AddStage(_nStage, _StageResult);

#if UNITY_EDITOR
                ZpLog.Normal(ZpLog.E_Category.Careeer, "New open stage - " + GetEpisodStageWithIndex(_nEpisod, _nStage));
#endif
            }

            //  favor stage open check
            if (MaxEpisodCount > LastOpenEpisod && IsPossibleOpenNewStage() == true && (IsFavorStage(_StageResult.stage) == true ||
                (IsBossStage(_StageResult.stage) == true && _StageResult.ownRank > RANK_MIN)))
            {
                //if (ZpGlobals.UsingNetwork == true)
                //    ZpGlobals.Network.SendTCP_CareerModeAbleOpenEpisodRemainTime();

#if UNITY_EDITOR
                ZpLog.Normal(ZpLog.E_Category.Careeer, "Update stage result - Open favor stage");
#endif
            }

            //	hidden stage open check
            if (IsHiddenStage(_StageResult.stage) == true)
            {
                episodResult.bPlayHidden = false;

#if UNITY_EDITOR
                ZpLog.Normal(ZpLog.E_Category.Careeer, "Update stage result - Open hidden stage");
#endif
            }
            else
            {
                episodResult.bPlayHidden = false;

                if (IsFavorStage(_StageResult.stage) == false)
                    SetShowNextStage(GetEpisodStageWithIndex(_nEpisod, _nStage));
            }
        }
        else
        {
            episodResult = new cCareerEpisodResult(_nEpisod);
            episodResult.AddStage(_nStage, _StageResult);
            if (_nStage == 1)
                episodResult.AddStage((int)StageType.DAILY_LIMITED, CreatePlayCountLimitedStageResult(_nEpisod, (int)StageType.DAILY_LIMITED,
                    CreatePlayCountLimitedStagePlayCount((int)StageType.DAILY_LIMITED)));
            m_Result.Add(_nEpisod, episodResult);

            if (IsFavorStage(_nStage) == true)
            {

                //ZpGlobals.Network.SendTCP_CareerModeAbleOpenEpisodRemainTime();
            }
            else
                SetShowNextStage(GetEpisodStageWithIndex(_nEpisod, _nStage));

#if UNITY_EDITOR
            ZpLog.Normal(ZpLog.E_Category.Careeer, "New open episod - " + GetEpisodStageWithIndex(_nEpisod, _nStage));
#endif
        }
    }

    void SetCareerStageResult(KMCareerStageInfo _StageResult)
    {
        bool bPlayCountLimitedStage = IsPlayCountLimitedStage(GetStageWithIndex(_StageResult.StageNumber));
        cCareerStageResult stageResult = bPlayCountLimitedStage == false ? new cCareerStageResult(_StageResult) :
            CreatePlayCountLimitedStageResult(_StageResult,
            CreatePlayCountLimitedStagePlayCount(GetStageWithIndex(_StageResult.StageNumber)));
        SetCareerStageResult(_StageResult.StageNumber, stageResult);
    }

    void SetCareerStageResult(int _nEpisodStage, cCareerStageResult _StageResult)
    {
        SetCareerStageResult(GetEpisodWithIndex(_nEpisodStage), GetStageWithIndex(_nEpisodStage), _StageResult);
    }

    public void SetCareerOpenedHiddenStage(int _nEpisod, bool _bOpened)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            episodResult.bOpenedHidden = _bOpened;
    }

    public void SetCareerOpenedFavorStage(int _nEpisod, bool _bOpened)
    {
        cCareerEpisodResult episodResult;
        if (GetCareerEpisodResult(_nEpisod, out episodResult))
            episodResult.bOpenedFavor = _bOpened;
    }

    public int GetCareerStageMyRank(int _nEpisod, int _nStage)
    {
        cCareerStageResult stageResult;
        if (GetCareerStageResult(_nEpisod, _nStage, out stageResult) == true)
            return Mathf.Clamp(stageResult.ownRank, RANK_MIN, RANK_MAX);
        return RANK_MIN;
    }

    public int GetCareerStageMyRank(int _nEpisodStage)
    {
        return GetCareerStageMyRank(GetEpisodWithIndex(_nEpisodStage), GetStageWithIndex(_nEpisodStage));
    }

    public int GetCareerStageMyRank()
    {
        return GetCareerStageMyRank(CurrentPlayEpisodStage);
    }

    public void UpdateAllCareerStageResultInfo(PacketCSRsCareerEpisodeMap _stagesInfo)
    {
        //if (ZpUITest.m_sbCareerTest)
        //{
        //    _arrayStageInfo = new KMCareerStageInfo[ZpUITest.m_sConditionValue[0]];
        //    for (int i = 0; i < _arrayStageInfo.Length; ++i)
        //    {
        //        int nEpi = i / (int)StageType.HIDDEN + 1;
        //        int nStage = i % (int)StageType.HIDDEN + 1;
        //        _arrayStageInfo[i] = new KMCareerStageInfo((short)ZpCareerManager.GetEpisodStageWithIndex(nEpi, nStage),
        //            (byte)ZpUITest.m_sConditionValue[1]);
        //    }
        //}        

        KMCareerStageInfo[] arrayStageInfo = _stagesInfo.stageInfo;
        KMDailyStageInfo[] arrayPlayCountLimitedStageInfo = _stagesInfo.DailyStageList;
        MaxEpisodCount = GetCareerInfo.GetEpisodTotalCount();

        SetOpenedStageInfoFromLocalData();

        if (m_Result == null)
            m_Result = new Dictionary<int, cCareerEpisodResult>();
        else
            m_Result.Clear();

        int nEpisod = 0;
        cCareerEpisodResult episodResult = new cCareerEpisodResult(1);

        if (arrayStageInfo != null && arrayStageInfo.Length > 0)
        {
            List<KMCareerStageInfo> stageInfoList = new List<KMCareerStageInfo>();
            for (int i = 0 ; i < arrayStageInfo.Length ; ++i)
                stageInfoList.Add(arrayStageInfo[i]);

            stageInfoList.Sort(delegate(KMCareerStageInfo a, KMCareerStageInfo b)
            {
                if (a.StageNumber > b.StageNumber)
                    return 1;
                else if (a.StageNumber < b.StageNumber)
                    return -1;
                return 0;
            });

            //  오픈됐던 스테이지 정보 로드
            GetOpenedStageInfo.GetValue();

            for (int i = 0 ; i < stageInfoList.Count ; ++i)
            {
                KMCareerStageInfo stageInfo = stageInfoList[i];
                cCareerStageResult stageResult = null;

                int nStage = GetStageWithIndex(stageInfo.StageNumber);

                if (nStage == 1)
                {
                    if (++nEpisod > 1)
                        episodResult = new cCareerEpisodResult(nEpisod);

                    int nPlayCountLimitedStage = (int)StageType.DAILY_LIMITED;
                    episodResult.episod = nEpisod;
                    episodResult.AddStage(nPlayCountLimitedStage, CreatePlayCountLimitedStageResult(nEpisod, nPlayCountLimitedStage,
                        CreatePlayCountLimitedStagePlayCount(nPlayCountLimitedStage)));
                    if (arrayPlayCountLimitedStageInfo != null)
                    {
                        for (int j = 0 ; j < arrayPlayCountLimitedStageInfo.Length ; ++j)
                        {
                            KMDailyStageInfo limitStageInfo = arrayPlayCountLimitedStageInfo[j];
                            int nLimitedStage = GetStageWithIndex(limitStageInfo.StageNumber);
                            if (nEpisod == GetEpisodWithIndex(limitStageInfo.StageNumber) && nPlayCountLimitedStage == nLimitedStage)
                            {
                                episodResult.SetPossiblePlayCountFromServer(nLimitedStage, limitStageInfo.RemainCount);
                                episodResult.SetPlayCountLimitedStageInfo(nLimitedStage, limitStageInfo.RemainCount);
                            }
                        }
                    }
                }

                //	hidden stage open check
                bool bHidden = IsHiddenStage(nStage);
                if (bHidden == true)
                {
                    stageResult = CreatePlayCountLimitedStageResult(stageInfo, CreatePlayCountLimitedStagePlayCount(nStage));
                    episodResult.AddStage(nStage, stageResult);

                    if (arrayPlayCountLimitedStageInfo != null)
                    {
                        for (int j = 0 ; j < arrayPlayCountLimitedStageInfo.Length ; ++j)
                        {
                            KMDailyStageInfo limitStageInfo = arrayPlayCountLimitedStageInfo[j];
                            int nLimitedStage = GetStageWithIndex(limitStageInfo.StageNumber);
                            if (nEpisod == GetEpisodWithIndex(limitStageInfo.StageNumber) && nStage == nLimitedStage)
                            {
                                episodResult.SetPossiblePlayCountFromServer(nLimitedStage, limitStageInfo.RemainCount);
                                episodResult.SetPlayCountLimitedStageInfo(nLimitedStage, limitStageInfo.RemainCount);
                            }
                        }
                    }

                    if (GetEpisodWithIndex(GetOpenedStageInfo.nOpenedHiddenEpisodStage) == nEpisod &&
                        GetOpenedStageInfo.IsOpenedHiddenStage() == true)
                        episodResult.bOpenedHidden = false;
                    else
                        episodResult.bOpenedHidden = true;
                }
                else
                {
                    stageResult = new cCareerStageResult(stageInfo);
                    if (IsBossStage(nStage) == true && GetOpenedStageInfo.IsUpdateLastPlayedEpisodStageRank() == true &&
                        IsHiddenStage(GetStageWithIndex(GetOpenedStageInfo.nLastPlayedEpisodStage)) == true &&
                        GetEpisodWithIndex(GetOpenedStageInfo.nLastPlayedEpisodStage) == nEpisod)
                    {
                        episodResult.bOpenedHidden = true;
                        //episodResult.bPlayHidden = true;
                        episodResult.bPlayHidden = false;
                    }
                    else
                    {
                        //episodResult.bOpenedHidden = false;
                        //episodResult.bPlayHidden = true;
                        episodResult.bPlayHidden = false;
                        if (MaxEpisodCount > nEpisod && IsFavorStage(nStage) == true)
                            episodResult.bOpenedFavor = GetOpenedStageInfo.IsOpenedFavorStage() == false;
                    }
                }

                if (bHidden == false)
                    episodResult.AddStage(nStage, stageResult);

                int nIndex = i + 1;
                if (nIndex == stageInfoList.Count)
                    m_Result.Add(nEpisod, episodResult);
                else if (nIndex < stageInfoList.Count && nEpisod != GetEpisodWithIndex(stageInfoList[nIndex].StageNumber))
                    m_Result.Add(nEpisod, episodResult);
            }

            int nLastEpisodStage = GetOpenedStageInfo.GetLastPlayedEpisodStage();
            if (nLastEpisodStage > 0)
                LastOpenEpisodStage = nLastEpisodStage;
            else
            {
                nLastEpisodStage = stageInfoList[stageInfoList.Count - 1].StageNumber;

                //  favor check
                int nLastEpisod = GetEpisodWithIndex(nLastEpisodStage);
                if (GetStageWithIndex(nLastEpisodStage) == 1 && nLastEpisod > 1 && IsPossibleOpenFavorStage(nLastEpisod - 1))
                {
                    LastOpenEpisodStage = GetEpisodStageWithIndex(nLastEpisod - 1, (int)StageType.FAVOR);
                    SetPossibleOpenNextEpisod();
                }
                else
                    LastOpenEpisodStage = IsHiddenStage(GetStageWithIndex(nLastEpisodStage)) ? nLastEpisodStage - 1 : nLastEpisodStage;

                if (GetOpenedStageInfo.IsUpdateLastPlayedEpisodStageRank() == false)
                    CurrentPlayEpisodStage = 0;
            }
        }
        else
        {
            //	최초 시작 (101)
            nEpisod = 1;
            LastOpenEpisodStage = 100;

            cCareerStageResult stageResult = new cCareerStageResult();
            stageResult.stage = 1;
            stageResult.ownRank = RANK_MIN;
            episodResult.AddStage(1, stageResult);

            cCareerPlayCountLimitedStageResult stage = CreatePlayCountLimitedStageResult(nEpisod, (int)StageType.DAILY_LIMITED,
                CreatePlayCountLimitedStagePlayCount((int)StageType.DAILY_LIMITED));
            if (stage != null)
                episodResult.AddStage((int)StageType.DAILY_LIMITED, stage);
            if (arrayPlayCountLimitedStageInfo != null && arrayPlayCountLimitedStageInfo.Length > 0)
            {
                KMDailyStageInfo limitStageInfo = arrayPlayCountLimitedStageInfo[0];
                episodResult.SetPossiblePlayCountFromServer(GetStageWithIndex(limitStageInfo.StageNumber), limitStageInfo.RemainCount);
            }

            m_Result.Add(nEpisod, episodResult);
        }

        //  tutorial stage
        //if (ZpGlobals.PlayerM.IsAblePlayTutorial(KMTutorialType.CAREER_MODE_TUTORIAL) == true)
        //{
        //    nEpisod = 1;
        //    LastOpenEpisodStage = ZpTutorial_CAREERMODE.TUTORIAL_STAGE;

        //    cCareerStageResult stageResult = new cCareerStageResult();
        //    stageResult.stage = GetStageWithIndex(ZpTutorial_CAREERMODE.TUTORIAL_STAGE);
        //    stageResult.ownRank = RANK_MIN;

        //    cCareerEpisodResult epiResult;
        //    if (GetCareerEpisodResult(nEpisod, out epiResult) == false)
        //    {
        //        epiResult = new cCareerEpisodResult(nEpisod);
        //        m_Result.Add(nEpisod, epiResult);
        //    }

        //    epiResult.AddStage(stageResult.stage, stageResult);

        //    cCareerPlayCountLimitedStageResult stage = CreatePlayCountLimitedStageResult(nEpisod, (int)StageType.DAILY_LIMITED,
        //        CreatePlayCountLimitedStagePlayCount((int)StageType.DAILY_LIMITED));
        //    if (stage != null)
        //        epiResult.AddStage((int)StageType.DAILY_LIMITED, stage);
        //    if (arrayPlayCountLimitedStageInfo != null && arrayPlayCountLimitedStageInfo.Length > 0)
        //    {
        //        KMDailyStageInfo limitStageInfo = arrayPlayCountLimitedStageInfo[0];
        //        epiResult.SetPossiblePlayCountFromServer(GetStageWithIndex(limitStageInfo.StageNumber), limitStageInfo.RemainCount);
        //    }
        //}
    }

    public KMCareerStageInfo[] GetCreateStageInfo(int _nEpisodStage)
    {
        KMCareerStageInfo[] arrayStageInfo = new KMCareerStageInfo[1] { new KMCareerStageInfo() };
        arrayStageInfo[0].StageNumber = (short)_nEpisodStage;
        arrayStageInfo[0].Complete = new bool[GetCareerInfo.GetConditionListCount(_nEpisodStage)];
        return arrayStageInfo;
    }

    public void AddNewOpenStage(KMCareerStageInfo[] _arrayStageInfo)
    {
        if (_arrayStageInfo == null || _arrayStageInfo.Length < 1 || m_Result == null)
            return;

        bool bFavor = false;
        bool bHidden = false;
        for (int i = 0 ; i < _arrayStageInfo.Length ; ++i)
        {
            KMCareerStageInfo stageInfo = _arrayStageInfo[i];
            SetCareerStageResult(stageInfo);

            if (bFavor == false)
                bFavor = IsFavorStage(GetStageWithIndex(stageInfo.StageNumber));
            if (bHidden == false)
                bHidden = IsHiddenStage(GetStageWithIndex(stageInfo.StageNumber));
        }

        //  set show next stage
        if (IsPrevPlayedCareerModeGame() == true)
        {
            if (bFavor == true)
            {
                for (int i = 0 ; i < _arrayStageInfo.Length ; ++i)
                {
                    KMCareerStageInfo stageInfo = _arrayStageInfo[i];
                    if (IsFavorStage(GetStageWithIndex(stageInfo.StageNumber)) == true &&
                        IsPossiblePlayPlayCountLimitedStage(GetEpisodWithIndex(stageInfo.StageNumber), (int)StageType.HIDDEN) == false)
                    {
                        SetShowNextStage(stageInfo.StageNumber, true);
                        break;
                    }
                }
            }
            else if (bHidden == true)
                SetShowNextStage(GetEpisodStageWithIndex(CurrentPlayEpisod, (int)StageType.HIDDEN), true);
        }
    }
}
