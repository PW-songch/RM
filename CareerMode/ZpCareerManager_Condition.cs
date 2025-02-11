using UnityEngine;
using System.Collections.Generic;

public partial class ZpCareerManager : ZpSingleton<ZpCareerManager>
{
#region condition data class

    public abstract class cCareerConditionData
    {
        protected ConditionType m_eConditionType;
        protected float m_fCheckTime;			    //	check time
        protected float m_fDelay;				    //	check delay
        protected float m_fStartCheckDelay;		    //	check start delay
        protected float m_fShowTimerDelay;	        //	show timer delay
        protected float m_fWorldDistance;		    //	check distance
        protected Vector3 m_vTargetWorldPos;	    //	current target world position
        protected bool m_bInverse;				    //	역으로 계산
        protected bool m_bAdd;					    //	값을 더해서 적용 할 것인지
        protected bool m_bCheckEnd;				    //	조건 체크 종료 여부
        protected bool[] m_arrayCompleted;          //  완료 여부
        protected bool m_bForceFailed;			    //	강제 실패
        private bool m_bShowGuide;			        //	가이드 노출 여부

        public float GetSetCheckTime
        {
            get { return m_fCheckTime; }
            set { m_fCheckTime = value; }
        }

        public float GetSetDelay
        {
            get { return m_fDelay; }
            set { m_fDelay = value; }
        }

        public float GetSetStartCheckDelay
        {
            get { return m_fStartCheckDelay; }
            set { m_fStartCheckDelay = value; }
        }

        public float GetSetShowTimerDelay
        {
            get { return m_fShowTimerDelay; }
            set { m_fShowTimerDelay = value; }
        }

        public bool GetSetIsCheckEnd
        {
            get { return m_bCheckEnd; }
            set
            {
                m_bCheckEnd = value;
                if (m_bCheckEnd == true)
                {
                    ZpCareerManager.instance.DisableHighLightPointObj(m_eConditionType);
                    ZpCareerManager.instance.RemoveConditionGuide(m_eConditionType);
                }
            }
        }

        public bool GetIsInverse
        {
            get { return m_bInverse; }
        }

        public bool[] GetCompletedList
        {
            get { return m_arrayCompleted; }
        }

        public cCareerConditionData(ConditionType _eConditionType, float _fDelay = 0.0f, float _fWorldDistance = 0.0f,
            bool _bAdd = true, bool _bReverse = false)
        {
            m_eConditionType = _eConditionType;
            m_fDelay = _fDelay;
            m_fWorldDistance = _fWorldDistance;
            m_bAdd = _bAdd;
            m_bInverse = _bReverse;
            Reset();
        }

        public abstract void ResetValue();
        public abstract bool AddValue(bool _bSetForced = false);
        public abstract bool AddValue(float _fDuration, bool _bSetForced = false);
        public abstract bool AddValue(Vector3 _vTargetPos, bool _bSetForced = false);
        public abstract int GetResult(bool _bForcedGet = false);
        public abstract bool IsCheckEnd();
        public abstract bool IsComplete();
        public abstract void SetCompletedResult();
        public abstract void SetCompletedResult(bool[] _arrayCompleted);
        public abstract void SetFailedResult();
        public abstract object GetGoalValueList();
        public abstract void SetGoalValueList(object _GoalValueList);
        public abstract int GetGoalValueListCount();
        protected virtual void Reset()
        {
            m_fCheckTime = 0.0f;
            m_vTargetWorldPos = Vector3.zero;
            m_bForceFailed = false;
            GetSetIsCheckEnd = false;
            m_bShowGuide = ZpCareerManager.instance.IsShowAbleGuide(m_eConditionType);
#if UNITY_EDITOR
            ZpLog.Normal(ZpLog.E_Category.Careeer, "[cCareerConditionData] Reset() : Show guide - " + m_bShowGuide);
#endif
        }

        public bool IsCompleted()
        {
            return m_arrayCompleted[0];
        }

        public bool IsCompletedCheckEnd()
        {
            return m_arrayCompleted[m_arrayCompleted.Length - 1];
        }

        public bool IsShowGuide()
        {
            return m_bShowGuide == true && GetSetIsCheckEnd == false;
        }
    }

    public abstract class cCareerConditionVauleData<T> : cCareerConditionData
    {
        protected T m_Value;
        protected T m_ResetValue;
        protected T m_BestValue;
        protected List<T> m_GoalValueList;

        public cCareerConditionVauleData(ConditionType _eCondtionType, T _value, T _resetValue, List<T> _goalValueList,
            bool _bReverse, bool _bAdd, float _fDelay = 0.0f, float _fWorldDistance = 0.0f)
            : base(_eCondtionType, _fDelay, _fWorldDistance, _bAdd, _bReverse)
        {
            m_Value = _value;
            m_ResetValue = _resetValue;
            UpdateBestValue();

            if (_goalValueList != null)
            {
                SetGoalValueList(_goalValueList);
                m_arrayCompleted = new bool[m_GoalValueList.Count];
            }
#if UNITY_EDITOR
            else
                ZpLog.Err("[cCareerConditionData] - Not exist _GoalValue");
#endif
        }

        protected abstract void UpdateBestValue();
        public abstract void SetBestValue(T _nValue, bool _bForce = false);
        public abstract int GetResult(List<T> _arrayGoalValue, bool _bForcedGet = false);

        public T GetSetValue
        {
            get { return m_Value; }
            set
            {
                m_Value = value;
                UpdateBestValue();
            }
        }

        public T GetBestValue()
        {
            return m_BestValue;
        }

        public override int GetGoalValueListCount()
        {
            return m_GoalValueList.Count;
        }

        public override object GetGoalValueList()
        {
            return m_GoalValueList;
        }

        protected override void Reset()
        {
            base.Reset();

            //  song2201 레이스 시작시 기존 완료 정보 초기화
            if (m_arrayCompleted != null)
            {
                for (int i = 0 ; i < m_arrayCompleted.Length ; ++i)
                    m_arrayCompleted[i] = false;
            }
        }

        public void ResetValue(T _value)
        {
            Reset();
            m_Value = _value;
            m_BestValue = m_Value;

            //  song2201 레이스 시작시 기존 완료 정보 초기화 위해 기존 완료 정보 유지하던 코드 주석처리
            //if (!IsCompletedCheckEnd())
            //{
            //    m_Value = _value;

            //    int nLength = m_arrayCompleted.Length - 1;
            //    for (int i = nLength; i >= 0; --i)
            //    {
            //        if (m_arrayCompleted[nLength - i])
            //            m_Value = m_GoalValueList[i];
            //    }

            //    m_bCheckEnd = false;
            //}
            //else
            //    m_bCheckEnd = true;
        }

        protected bool SetValue(T _value, bool _bSetForced = false)
        {
            float fDuration = Time.time - m_fCheckTime;
            if (m_bCheckEnd || !_bSetForced && fDuration < m_fDelay)
                return false;

            m_Value = _value;
            UpdateBestValue();
            m_fCheckTime = Time.time;

            return true;
        }

        public override void SetCompletedResult()
        {
            for (int i = 0 ; i < m_arrayCompleted.Length ; ++i)
                m_arrayCompleted[i] = true;

            GetSetIsCheckEnd = true;
            m_Value = m_GoalValueList[m_GoalValueList.Count - 1];
            UpdateBestValue();
        }

        public override void SetCompletedResult(bool[] _arrayCompleted)
        {
            int nCompletedCount = 0;
            for (int i = 0 ; i < m_arrayCompleted.Length ; ++i)
            {
                if (i < _arrayCompleted.Length && _arrayCompleted[i] == true)
                {
                    m_arrayCompleted[i] = _arrayCompleted[i];
                    m_Value = m_GoalValueList[i];
                    UpdateBestValue();

                    nCompletedCount++;
                    if (nCompletedCount == m_GoalValueList.Count)
                        GetSetIsCheckEnd = true;
                }
                else
                    m_arrayCompleted[i] = false;
            }
        }

        public override void SetFailedResult()
        {
            m_bForceFailed = true;
        }

        public void RemoveGoalValue(int _nIndex)
        {
            m_GoalValueList.RemoveAt(_nIndex);
        }

        public override int GetResult(bool _bForcedGet = false)
        {
            return GetResult(m_GoalValueList, _bForcedGet);
        }

        public override bool IsComplete()
        {
            return GetResult() > RANK_MIN;
        }
    }

    public class cCareerIntegerTypeConditionData : cCareerConditionVauleData<int>
    {
        public const int RESET_VALUE = int.MinValue;

        public cCareerIntegerTypeConditionData(ConditionType _eCondtionType, int _nValue, int _nResetValue,
            List<int> _goalValueList, bool _bReverse, bool _bAdd, float _fDelay = 0.0f, float _fWorldDistance = 0.0f)
            : base(_eCondtionType, _nValue, _nResetValue, _goalValueList, _bReverse, _bAdd, _fDelay, _fWorldDistance) { }

        protected override void UpdateBestValue()
        {
            if (m_Value > m_BestValue)
                m_BestValue = m_Value;
        }

        public override void SetBestValue(int _nValue, bool _bForce = false)
        {
            if (_nValue > m_BestValue || _bForce == true)
                m_BestValue = _nValue;
        }

        public override void SetGoalValueList(object _GoalValueList)
        {
            m_GoalValueList = _GoalValueList as List<int>;
            m_GoalValueList.Sort(delegate(int a, int b)
            {
                if (m_bInverse)
                {
                    if (a > b)
                        return -1;
                    else if (a < b)
                        return 1;
                    return 0;
                }
                else
                {
                    if (a > b)
                        return 1;
                    else if (a < b)
                        return -1;
                    return 0;
                }
            });
        }

        public override void ResetValue()
        {
            ResetValue(m_ResetValue != RESET_VALUE ? m_ResetValue : RANK_MIN);
        }

        public override bool AddValue(bool _bSetForced = false)
        {
            return AddValue(m_bInverse ? -1 : 1, _bSetForced);
        }

        public bool AddValue(int _value, bool _bSetForced = false)
        {
            int nValue = GetSetValue;
            return SetValue(m_bInverse ? (m_bAdd ? nValue - _value : _value) :
                (m_bAdd ? nValue + _value : _value), _bSetForced);
        }

        public override bool AddValue(float _fDuration, bool _bSetForced = false)
        {
            return AddValue(_fDuration, m_bInverse ? -1 : 1, _bSetForced);
        }

        public bool AddValue(float _fDuration, int _value, bool _bSetForced = false)
        {
            return _fDuration > m_fDelay ? AddValue(_value, _bSetForced) : false;
        }

        public override bool AddValue(Vector3 _vTargetPos, bool _bSetForced = false)
        {
            return AddValue(_vTargetPos, m_bInverse ? -1 : 1, _bSetForced);
        }

        public bool AddValue(Vector3 _vTargetPos, int _value, bool _bSetForced = false)
        {
            float fDistance = Mathf.Abs((m_vTargetWorldPos - _vTargetPos).sqrMagnitude);
            if (_bSetForced || m_fWorldDistance < fDistance)
            {
                m_vTargetWorldPos = _vTargetPos;
#if UNITY_EDITOR
                ZpLog.Normal(ZpLog.E_Category.Careeer, "@@ Career AddValue Distance : ", fDistance.ToString());
#endif
                return AddValue(_value, _bSetForced);
            }
            return false;
        }

        public override int GetResult(List<int> _arrayGoalValue, bool _bForcedGet = false)
        {
            if (!_bForcedGet && (m_bForceFailed || _arrayGoalValue == null))
                return ZpCareerManager.RANK_MIN;

            int nResult = ZpCareerManager.RANK_MIN;

            for (int i = _arrayGoalValue.Count - 1 ; i >= 0 ; --i)
            {
                if (m_bInverse)
                {
                    if (_arrayGoalValue[i] >= m_Value)
                    {
                        nResult = i + 1;
                        for (int j = 0 ; j <= i ; ++j)
                            m_arrayCompleted[j] = true;
                        break;
                    }
                }
                else
                {
                    if (_arrayGoalValue[i] <= m_Value)
                    {
                        nResult = i + 1;
                        for (int j = 0 ; j <= i ; ++j)
                            m_arrayCompleted[j] = true;
                        break;
                    }
                }
            }

            return nResult;
        }

        public override bool IsCheckEnd()
        {
            if (m_bCheckEnd || m_GoalValueList.Count < 1)
                return true;

            if (m_GoalValueList == null)
                return false;

            bool bComplete = false;
            bool bCheckEnd = true;

            if (m_bInverse)
            {
                for (int i = m_GoalValueList.Count - 1 ; i >= 0 ; --i)
                {
                    if (m_GoalValueList[i] < m_Value)
                    {
                        bComplete = true;
                        for (int j = 0 ; j <= i ; ++j)
                            m_arrayCompleted[j] = false;
                    }
                    else
                    {
                        bComplete = false;
                        break;
                    }
                }

                bCheckEnd = bComplete;
            }
            else
            {
                for (int i = 0 ; i < m_arrayCompleted.Length ; ++i)
                {
                    if (m_GoalValueList[i] <= m_Value)
                    {
                        bComplete = true;
                        m_arrayCompleted[i] = true;
                    }
                }

                bCheckEnd = m_GoalValueList[m_GoalValueList.Count - 1] <= m_Value;
            }

            if (bComplete && !m_bCheckEnd && bCheckEnd)
            {
                GetSetIsCheckEnd = true;
                return false;
            }

            return bCheckEnd;
        }
    }

    //	grand prix
    public class cCareerGrandPrixConditionData : cCareerIntegerTypeConditionData
    {
        private int m_nMineSlot;
        private int m_nRaceCount;
        private int m_nRaceTotalCount;
        private Dictionary<int, KeyValuePair<int, float>> m_dicPlayerScoreRecord;	    //	slot, <score, record>
        private Dictionary<int, KeyValuePair<int, float>> m_dicPrevPlayerScoreRecord;	//	slot, <score, record>

        public cCareerGrandPrixConditionData(ConditionType _eCondtionType, int _nValue, List<int> _goalValueList, int _nRaceTotalCount)
            : base(_eCondtionType, _nValue, cCareerIntegerTypeConditionData.RESET_VALUE, _goalValueList, true, false)
        {
            m_nRaceCount = 0;
            m_nRaceTotalCount = _nRaceTotalCount;
            m_dicPlayerScoreRecord = new Dictionary<int, KeyValuePair<int, float>>();
            m_dicPrevPlayerScoreRecord = new Dictionary<int, KeyValuePair<int, float>>();
        }

        public void SetPlayerList(int _nMineSlot, List<int> _PlayerList)
        {
            m_nMineSlot = _nMineSlot;

            m_nRaceCount = 0;
            m_dicPlayerScoreRecord.Clear();
            m_dicPrevPlayerScoreRecord.Clear();

            for (int i = 0 ; i < _PlayerList.Count ; ++i)
                m_dicPlayerScoreRecord.Add(_PlayerList[i], new KeyValuePair<int, float>(0, 0.0f));
        }

        public bool AddScore(Dictionary<int, KeyValuePair<int, float>> _RankList)
        {
            //	slot, <score, record>
            int nTotalPlayerCount = !ZpGlobals.PlayerManagerIsNull() ?
                ZpGlobals.PlayerM.GetCountPlayer() : 4;
            foreach (int nSlot in _RankList.Keys)
            {
                if (m_dicPlayerScoreRecord.ContainsKey(nSlot))
                {
                    int nRank = _RankList[nSlot].Key;
                    float fRecord = _RankList[nSlot].Value;

                    if (m_dicPrevPlayerScoreRecord.ContainsKey(nSlot))
                    {
                        m_dicPrevPlayerScoreRecord[nSlot] = new KeyValuePair<int, float>(
                            m_dicPlayerScoreRecord[nSlot].Key, m_dicPlayerScoreRecord[nSlot].Value);
                    }
                    else
                    {
                        m_dicPrevPlayerScoreRecord.Add(nSlot, new KeyValuePair<int, float>(
                            GetScorePoint(nTotalPlayerCount, nRank), fRecord));
                    }

                    m_dicPlayerScoreRecord[nSlot] = new KeyValuePair<int, float>(
                        m_dicPlayerScoreRecord[nSlot].Key + GetScorePoint(nTotalPlayerCount, nRank),
                        m_dicPlayerScoreRecord[nSlot].Value + fRecord);
                }
                else
                    return false;
            }

            return ++m_nRaceCount >= m_nRaceTotalCount;
        }

        public void RedoAddCurrentScore()
        {
            foreach (int nSlot in m_dicPrevPlayerScoreRecord.Keys)
            {
                if (m_dicPrevPlayerScoreRecord.ContainsKey(nSlot))
                {
                    m_dicPlayerScoreRecord[nSlot] = new KeyValuePair<int, float>(
                        m_dicPlayerScoreRecord[nSlot].Key - m_dicPrevPlayerScoreRecord[nSlot].Key,
                    m_dicPlayerScoreRecord[nSlot].Value - m_dicPrevPlayerScoreRecord[nSlot].Value);
                }
            }
        }

        int GetScorePoint(int _nTotalPlayerCount, int _nRank)
        {
            return (_nTotalPlayerCount - _nRank) * 2;
        }

        public Dictionary<int, KeyValuePair<int, float>> GetScoreResult()
        {
            return m_dicPlayerScoreRecord;
        }

        public Dictionary<int, KeyValuePair<int, float>> GetPrevScoreResult()
        {
            return m_dicPrevPlayerScoreRecord;
        }

        public KeyValuePair<int, float> GetScoreResultByPlayerSlot(int _nSlot)
        {
            if (m_dicPlayerScoreRecord.ContainsKey(_nSlot))
                return m_dicPlayerScoreRecord[_nSlot];
            return new KeyValuePair<int, float>();
        }

        public override int GetResult(bool _bForcedGet = false)
        {
            if (!_bForcedGet && (m_bForceFailed || !m_bCheckEnd))
                return ZpCareerManager.RANK_MIN;

            int nResult = ZpCareerManager.RANK_MIN;
            int nMineScore = m_dicPlayerScoreRecord[m_nMineSlot].Key;
            float fMineRecord = m_dicPlayerScoreRecord[m_nMineSlot].Value;

            KeyValuePair<int, float> minScore = m_dicPlayerScoreRecord[m_nMineSlot];
            m_dicPlayerScoreRecord.Remove(m_nMineSlot);

            foreach (KeyValuePair<int, float> nScore in m_dicPlayerScoreRecord.Values)
            {
                if (nMineScore > nScore.Key)
                    nResult++;
                else if (nMineScore == nScore.Key)
                {
                    //	score 동점일 경우 record가 더 작으면 랭크 상승
                    if (fMineRecord <= nScore.Value)
                        nResult++;
                }
            }

            if (m_nRaceCount >= m_nRaceTotalCount || m_bCheckEnd == true)
            {
                int nfinalRank = m_dicPlayerScoreRecord.Count - nResult + 1;
                nResult = 0;

                for (int i = 0 ; i < m_GoalValueList.Count ; ++i)
                {
                    if (nfinalRank <= m_GoalValueList[i])
                    {
                        nResult = Mathf.Clamp(m_dicPlayerScoreRecord.Count - i, 0, m_arrayCompleted.Length);
                        break;
                    }
                }

                for (int i = 0 ; i < nResult ; ++i)
                    m_arrayCompleted[i] = true;
            }

            m_dicPlayerScoreRecord.Add(m_nMineSlot, minScore);

            return Mathf.Clamp(nResult, RANK_MIN, RANK_MAX);
        }

        public override bool IsCheckEnd()
        {
            if (m_nRaceCount >= m_nRaceTotalCount)
                GetSetIsCheckEnd = true;
            else if (m_bCheckEnd)
                return true;

            if (m_bCheckEnd)
            {
                int nResult = GetResult(true);
                for (int i = 0 ; i < nResult ; ++i)
                    m_arrayCompleted[i] = true;
            }

            return false;
        }

        public void SetEndRace()
        {
            m_nRaceCount = m_nRaceTotalCount;
        }
    }

#endregion

    public void ResetCareerCondition()
    {
        //SetConditionCompleted();

        //	career game unfinished
        foreach (KeyValuePair<ConditionDataType, cCareerConditionData> conditionData in m_ConditionList.Values)
        {
            //	conditionlist reset
            cCareerConditionData data = conditionData.Value;
            if (data != null)
                data.ResetValue();
        }

        //	set suddendeath condition
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(ConditionType.GRAND_PRIX, out condition))
        {
            cCareerGrandPrixConditionData data = CastingConditionDataClassFromType(
                condition.Key, condition.Value) as cCareerGrandPrixConditionData;
            if (data != null)
            {
                List<int> listPlayer = new List<int>();
                int nPlayerCount = ZpGlobals.PlayerM.GetCountPlayer();
                int nMine = !ZpGlobals.PlayerManagerIsNull() ? ZpGlobals.PlayerM.GetRoomPlayerLocal().m_Slot : 0;
                for (int i = 0 ; i < nPlayerCount ; ++i)
                    listPlayer.Add(i);
                data.SetPlayerList(nMine, listPlayer);
            }
        }
        else if (m_ConditionList.TryGetValue(ConditionType.TIME_LIMITED_SUDDENDEATH, out condition))
        {
            //cCareerConditionData data = CastingConditionDataClassFromType(condition.Key, condition.Value);
            //List<int> goalValueList = new List<int>();
            //goalValueList.Add(1); goalValueList.Add(1); goalValueList.Add(1);
            //data.SetGoalValueList(goalValueList);
        }
    }

    void SetConditionCompleted()
    {
        cCareerStageResult stageResult;
        if (GetCareerStageResult(m_CurrentPlayCareerInfo.stageIndex, out stageResult))
        {
            int i = 0;
            bool[] arrayCompleted = stageResult.arrayCompleted;

            if (arrayCompleted != null)
            {
                foreach (ConditionType conditionType in m_ConditionList.Keys)
                {
                    cCareerConditionData data = m_ConditionList[conditionType].Value;
                    if (data != null)
                    {
                        //  동일 조건인 경우
                        int nCount = data.GetGoalValueListCount();
                        if (nCount > 1)
                        {
                            bool[] arrayCompletedTemp = new bool[nCount];
                            for (int j = 0 ; j < nCount ; ++j)
                            {
                                arrayCompletedTemp[j] = arrayCompleted[i];
                                ++i;
                            }

                            data.SetCompletedResult(arrayCompletedTemp);
                        }
                        //  다른 조건인 경우
                        else if (i < arrayCompleted.Length)
                        {
                            data.SetCompletedResult(new bool[] { arrayCompleted[i] });
                            ++i;
                        }
                    }
                }
            }
        }
#if UNITY_EDITOR
        else
            ZpLog.Normal(ZpLog.E_Category.Careeer, "# Career condition completed list length different");
#endif
    }

    public bool IsCompletedCondition(ConditionType _type)
    {
        bool bCompleted = false;
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(_type, out condition))
        {
            cCareerConditionData conditionData = condition.Value;
            if (conditionData != null)
                bCompleted = conditionData.IsCompleted();
        }

        return bCompleted;
    }

    public bool GetConditionIsCheckEndAndCompleted(ConditionType _type)
    {
        bool checkEnd = false;
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(_type, out condition))
        {
            cCareerConditionData conditionData = condition.Value;
            if (conditionData != null)
            {
                checkEnd = conditionData.GetSetIsCheckEnd;
                if (!checkEnd)
                    checkEnd = conditionData.IsCompletedCheckEnd();
            }
        }

        return checkEnd;
    }

    public bool IsContainCondition(ConditionType _type)
    {
        return m_ConditionList.ContainsKey(_type);
    }

    public bool IsSuddenDeathCondition()
    {
        return IsContainCondition(ConditionType.TIME_LIMITED_SUDDENDEATH) ||
                IsContainCondition(ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH);
    }

    public object GetConditionGoalValue(ConditionType _type)
    {
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(_type, out condition))
        {
            switch (condition.Key)
            {
                default:
                case ConditionDataType.INTEGER:
                    return CastingIntegerTypeConditionDataClass(condition.Value).GetGoalValueList();
            }
        }

        return null;
    }

    public float GetConditionCheckTime()
    {
        return m_fCareerTimeCheck;
    }

    public cCareerConditionData GetConditionData(ConditionType _type)
    {
        if (IsContainCondition(_type) == true)
            return m_ConditionList[_type].Value;
        return null;
    }

    public cCareerGrandPrixConditionData GetGrandPrixConditionData()
    {
        KeyValuePair<ConditionDataType, cCareerConditionData> conditionData;
        if (m_ConditionList.TryGetValue(ConditionType.GRAND_PRIX, out conditionData))
        {
            cCareerGrandPrixConditionData data = CastingConditionDataClassFromType(conditionData.Key, conditionData.Value)
                as cCareerGrandPrixConditionData;
            return data;
        }

        return null;
    }

    public void SetCompletedAllCondition()
    {
        if (m_ConditionList == null || m_ConditionList.Count < 1)
            return;

        foreach (var condition in m_ConditionList)
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> conditionData = condition.Value;
            cCareerConditionData data = conditionData.Value;
            if (data != null)
                data.SetCompletedResult();
        }
    }

    public void SetFailedAllCondition()
    {
        if (m_ConditionList == null || m_ConditionList.Count < 1)
            return;

        foreach (var condition in m_ConditionList)
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> conditionData = condition.Value;
            cCareerConditionData data = conditionData.Value;
            if (data != null)
                data.SetFailedResult();
        }
    }

    public ConditionType[] GetCurrentCareerResultRankAndCompletedCondition(out int _nRank)
    {
        _nRank = RANK_MIN;
        if (m_ConditionList == null)
            return null;

        List<int> result = new List<int>();
        List<ConditionType> completedList = new List<ConditionType>();
        foreach (ConditionType conditionType in m_ConditionList.Keys)
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> conditionData;
            if (m_ConditionList.TryGetValue(conditionType, out conditionData))
            {
                cCareerConditionData data = conditionData.Value;
                if (data != null)
                {
                    int nResult = data.GetResult();
                    result.Add(nResult);
                    if (nResult > RANK_MIN)
                        completedList.Add(conditionType);
                }
            }
        }

        if (m_ConditionList.Count == 1 && result.Count > 0)
        {
            result.Sort();
            _nRank = result[0];
        }
        else
        {
            for (int i = 0 ; i < result.Count ; ++i)
                _nRank += result[i];
        }

        if (m_nTestRank != 0)
        {
            _nRank = m_nTestRank;

            int nIndex = 0;
            int nCount = 0;
            foreach (ConditionType conditionType in m_ConditionList.Keys)
            {
                KeyValuePair<ConditionDataType, cCareerConditionData> conditionData = m_ConditionList[conditionType];
                cCareerConditionData data = conditionData.Value;
                if (data != null)
                {
                    nIndex++;
                    bool[] arrayCompleted = new bool[Mathf.Min(_nRank - nCount, data.GetGoalValueListCount())];
                    for (int i = 0 ; i < arrayCompleted.Length ; ++i)
                        arrayCompleted[i] = nCount++ < _nRank ? true : false;
                    data.SetCompletedResult(arrayCompleted);
                }
            }
        }

        _nRank = Mathf.Clamp(_nRank, RANK_MIN, RANK_MAX);

        return completedList.ToArray();
    }

    public ConditionType[] GetCurrentCareerResultRankAndCompletedCondition()
    {
        if (m_ConditionList == null)
            return null;

        List<ConditionType> completedList = new List<ConditionType>();
        foreach (ConditionType conditionType in m_ConditionList.Keys)
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> conditionData;
            if (m_ConditionList.TryGetValue(conditionType, out conditionData))
            {
                cCareerConditionData data = conditionData.Value;
                if (data != null)
                {
                    if (data.IsCompleted())
                        completedList.Add(conditionType);
                }
            }
        }

        return completedList.ToArray();
    }

    public bool[] GetCurrentCareerResultCompletedList(bool _bRetired = false)
    {
        List<bool> completedList = new List<bool>();
        foreach (ConditionType conditionType in m_ConditionList.Keys)
        {
            KeyValuePair<ConditionDataType, cCareerConditionData> conditionData = m_ConditionList[conditionType];
            cCareerConditionData data = conditionData.Value;
            for (int i = 0 ; i < data.GetCompletedList.Length ; ++i)
                completedList.Add(_bRetired ? false : data.GetCompletedList[i]);
        }

        return completedList.ToArray();
    }

    public int GetConditionRank(ConditionType _type)
    {
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(_type, out condition))
        {
            switch (condition.Key)
            {
                default:
                case ConditionDataType.INTEGER:
                    return CastingIntegerTypeConditionDataClass(condition.Value).GetResult();
            }
        }

        return 0;
    }

    cCareerConditionData CastingConditionDataClassFromType(ConditionDataType _type, object _Data)
    {
        switch (_type)
        {
            default:
            case ConditionDataType.INTEGER:
                return CastingIntegerTypeConditionDataClass(_Data);
        }
    }

    cCareerIntegerTypeConditionData CastingIntegerTypeConditionDataClass(object _Data)
    {
        if (_Data is cCareerIntegerTypeConditionData)
            return _Data as cCareerIntegerTypeConditionData;
        else if (_Data is cCareerGrandPrixConditionData)
            return _Data as cCareerGrandPrixConditionData;
        return null;
    }

    cCareerConditionData CreateConditionList(CareerStageCondition _Condition)
    {
        if (!IsCareerModeScene())
            return null;

        cCareerConditionData conditionList = null;
        switch (GetCareerInfo.GetConditionDataType(_Condition.conditionType))
        {
            default:
            case ConditionDataType.INTEGER:
                {
                    int nInitValue = RANK_MIN;
                    int nResetValue = cCareerIntegerTypeConditionData.RESET_VALUE;
                    bool bReverse = _Condition.conditionValue[0] < 0 ? true : false;

                    for (int i = 0 ; i < _Condition.conditionValue.Count ; ++i)
                    {
                        //	음수 값 양수로
                        if (_Condition.conditionValue[i] < 0)
                            _Condition.conditionValue[i] *= -1;
                    }

                    List<int> goalValueList = new List<int>(_Condition.conditionValue);

                    cCareerIntegerTypeConditionData conditionData = null;

                    switch (_Condition.conditionType)
                    {
                        case ConditionType.RANK:
                            {
                                nResetValue = m_CurrentPlayCareerInfo.ai.aiID.Length + 1;
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, true, false);
                            }
                            break;

                        case ConditionType.GRAND_PRIX:
                            {
                                conditionData = new cCareerGrandPrixConditionData(_Condition.conditionType, nInitValue, goalValueList, m_CurrentPlayCareerInfo.track.GetTrackLength());
                            }
                            break;

                        case ConditionType.DRIFTBOOST_COUNT:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, true, 1.0f);
                            }
                            break;

                        case ConditionType.FENCE_COLLIDE_COUNT:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, true, true, 0.5f);
                            }
                            break;

                        case ConditionType.START_BOOST_USE_COUNT:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, true);
                            }
                            break;

                        case ConditionType.BOOSTZONE_USE_COUNT:
                        case ConditionType.GREEN_BOOSTZONE_USE_COUNT:
                        case ConditionType.BLUE_BOOSTZONE_USE_COUNT:
                        case ConditionType.RED_BOOSTZONE_USE_COUNT:
                        case ConditionType.ITEM_GET_COUNT:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, true, 0.0f, 1000.0f);
                            }
                            break;

                        case ConditionType.CONTROL_DIRECTION_COUNT:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, true, 0.0f, 1.0f);
                            }
                            break;

                        case ConditionType.REMAIN_RANK_IN_SECOND:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, false, (float)goalValueList[0]);
                                conditionData.GetSetStartCheckDelay = 5.0f;
                                conditionData.GetSetShowTimerDelay = 0.0f;
                            }
                            break;

                        case ConditionType.TIME_LIMITED:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, true, false);
                            }
                            break;

                        case ConditionType.TIME_LIMITED_SUDDENDEATH:
                        case ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH:
                            {
                                if (_Condition.conditionType == ConditionType.TIME_LIMITED_SUDDENDEATH)
                                    m_fCareerTimeCheck = (float)goalValueList[0];

                                int nCount = goalValueList.Count;
                                goalValueList.Clear();

                                for (int i = 0 ; i < nCount ; ++i)
                                    goalValueList.Add(1);
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, false, true);
                                conditionData.GetSetStartCheckDelay = ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("SuddenDeath_CheckStart_DelayTime"); ;
                                conditionData.GetSetShowTimerDelay = ZpGlobals.s_ScriptCSVDataPool.IfConstValueInfo.GetValue("SuddenDeath_ShowTimer_DelayTime"); ;
                            }
                            break;

                        default:
                            {
                                conditionData = new cCareerIntegerTypeConditionData(_Condition.conditionType, nInitValue, nResetValue, goalValueList, bReverse, true);
                            }
                            break;
                    }

                    conditionList = conditionData;
                }
                break;
        }

        return conditionList;
    }

#region update condition data

    public void UpdateConditionData(ConditionType _ConditionType, object _Value = null, bool _bForce = false)
    {
        if (_bForce == false && (ZpGameGlobals.m_ScriptGM == null ||
            ZpGameGlobals.m_ScriptGM.IsOnGame() == false || ZpGameGlobals.m_ScriptGM.IsPause() == true))
            return;

        //	condition data update
        KeyValuePair<ConditionDataType, cCareerConditionData> condition;
        if (m_ConditionList.TryGetValue(_ConditionType, out condition))
        {
            bool bSuccess = false;
            switch (condition.Key)
            {
                default:
                case ConditionDataType.INTEGER:
                    {
                        cCareerIntegerTypeConditionData conditionData = CastingIntegerTypeConditionDataClass(condition.Value);
                        if (conditionData == null)
                        {
#if UNITY_EDITOR
                            ZpLog.Err("[ZpCareerManager] - (UpdateConditionData) Not exist condition data");
#endif
                            return;
                        }

                        switch (_ConditionType)
                        {
                            case ConditionType.RANK:
                                {
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        bSuccess = conditionData.AddValue((int)_Value);

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()) + _Value.ToString());
#endif
                                    }
                                }
                                break;

                            case ConditionType.DRIFTBOOST_COUNT:
                                {
                                    //	value = drift start or end
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        if ((bool)_Value)
                                        {
                                            conditionData.GetSetCheckTime = Time.time;
                                            return;
                                        }

                                        bSuccess = conditionData.AddValue();

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()));
#endif
                                    }
                                }
                                break;

                            case ConditionType.START_BOOST_USE_COUNT:
                                {
                                    //	value = true or false
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        conditionData.AddValue((bool)_Value ? 1 : 0);
                                        bSuccess = true;

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()));
#endif
                                    }
                                }
                                break;

                            case ConditionType.BOOSTZONE_USE_COUNT:
                            case ConditionType.GREEN_BOOSTZONE_USE_COUNT:
                            case ConditionType.BLUE_BOOSTZONE_USE_COUNT:
                            case ConditionType.RED_BOOSTZONE_USE_COUNT:
                            case ConditionType.ITEM_GET_COUNT:
                                {
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        ZpVehicleBase localVehicle = ZpGameGlobals.GetVehicleLOCAL();
                                        Vector3 vTartgetWorldPos = (Vector3)_Value +
                                            Vector3.one * Mathf.Max(1.0f, ((float)localVehicle.m_CurrentLap * localVehicle.m_CurrentDistance));
                                        bSuccess = conditionData.AddValue(vTartgetWorldPos);

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : Add",
                                                _ConditionType.ToString()));
#endif
                                    }
                                }
                                break;

                            case ConditionType.CONTROL_DIRECTION_COUNT:
                                {
                                    //  value = button screen position
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        Vector3 vScreenPos = (Vector3)_Value;
                                        bSuccess = conditionData.AddValue(vScreenPos);
                                        if (bSuccess == true && conditionData.IsComplete() == false)
                                        {
                                            GameObject[] arrayTarget = GetConditionGuideTargetObj(_ConditionType);
                                            if (arrayTarget != null && arrayTarget.Length > 1)
                                            {
                                                bool bRight = ZpUtility.IsRightPositionInScreen(vScreenPos);
                                                ShowConditionGuide(false, bRight == true ? arrayTarget[1] : arrayTarget[0],
                                                    new List<ConditionType>() { _ConditionType });
                                                ShowConditionGuide(true, bRight == true ? arrayTarget[0] : arrayTarget[1],
                                                    new List<ConditionType>() { _ConditionType });
                                            }
                                        }

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : Add",
                                                _ConditionType.ToString()));
#endif
                                    }
                                }
                                break;

                            case ConditionType.REMAIN_RANK_IN_SECOND:
                                {
                                    //	value = my rank
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        float fRaceTime = ZpGameGlobals.m_ScriptGM.GetRaceTime();
                                        if (conditionData.GetSetStartCheckDelay > fRaceTime)
                                            return;

                                        if ((int)_Value != 1)
                                        {
                                            //	1등 벗어남
                                            if (conditionData.GetSetCheckTime != 0.0f)
                                            {
                                                int nTime = Mathf.FloorToInt(Time.time - conditionData.GetSetCheckTime);
                                                conditionData.SetBestValue(nTime, true);
                                                ZpEventListener.Broadcast("EVENT_UI_CAREER_TIMECOUNT", false, 0, ConditionType.END);
                                            }

                                            conditionData.GetSetCheckTime = 0.0f;
                                        }
                                        else
                                        {
                                            //	1등 진입
                                            if (conditionData.GetSetCheckTime == 0.0f)
                                            {
                                                int nRank = conditionData.GetResult();
                                                List<int> listGoalValue = conditionData.GetGoalValueList() as List<int>;
                                                int nIndex = Mathf.Clamp(nRank, 0, listGoalValue.Count - 1);
                                                conditionData.GetSetValue = nRank == 0 ? 0 : listGoalValue[nRank - 1];
                                                conditionData.GetSetCheckTime = Time.time;
                                                conditionData.SetBestValue(0, true);

                                                conditionData.GetSetDelay = listGoalValue[nIndex];

                                                ZpEventListener.Broadcast("EVENT_UI_CAREER_TIMECOUNT", true, conditionData.GetSetDelay, _ConditionType);
                                            }
                                            else
                                            {
                                                int nTime = Mathf.FloorToInt(Time.time - conditionData.GetSetCheckTime);

                                                int nRank = conditionData.GetResult();
                                                List<int> listGoalValue = conditionData.GetGoalValueList() as List<int>;
                                                int nIndex = Mathf.Clamp(nRank, 0, listGoalValue.Count - 1);
                                                if (listGoalValue[nIndex] == conditionData.GetSetDelay)
                                                {
                                                    //  새로 1등 진입
                                                    bSuccess = conditionData.AddValue(nTime);
                                                }
                                                else
                                                {
                                                    //  1등 유지
                                                    bSuccess = conditionData.AddValue(conditionData.GetSetValue + nTime);
                                                }

                                                conditionData.SetBestValue(nTime, true);

                                                conditionData.GetSetIsCheckEnd = false;
                                                if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                                {
                                                    nRank = conditionData.GetResult();
                                                    nIndex = Mathf.Clamp(nRank, 0, listGoalValue.Count - 1);
                                                    if (nIndex - 1 > 0)
                                                        conditionData.GetSetDelay = nRank > 0 ? listGoalValue[nIndex] - listGoalValue[nIndex - 1] : listGoalValue[nIndex];

                                                    if (listGoalValue.Count > 1 && nRank < listGoalValue.Count)
                                                        ZpEventListener.Broadcast("EVENT_UI_CAREER_TIMECOUNT", true, conditionData.GetSetDelay, _ConditionType);

#if UNITY_EDITOR
                                                    ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                        _ConditionType.ToString()) + nTime.ToString());
#endif
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                            case ConditionType.TIME_LIMITED:
                                {
                                    //	value = race time
                                    if (_Value != null && !conditionData.IsCompletedCheckEnd())
                                    {
                                        bSuccess = conditionData.AddValue(Mathf.CeilToInt((float)_Value));

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()) + _Value.ToString());
#endif
                                    }
                                }
                                break;

                            case ConditionType.TIME_LIMITED_SUDDENDEATH:
                                {
                                    //	value = remain player count
                                    if (_Value != null && !ZpGlobals.PlayerManagerIsNull() && (int)_Value > 1)
                                    {
                                        int nTotalPlayerCount = ZpGlobals.PlayerM.GetCountPlayer();
                                        float fCheckTime = (float)(m_fCareerTimeCheck * (++nTotalPlayerCount - (int)_Value) + conditionData.GetSetStartCheckDelay);
                                        float fRaceTime = ZpGameGlobals.m_ScriptGM.GetRaceTime();

                                        //	update
                                        if (fCheckTime <= fRaceTime)
                                        {
                                            bSuccess = true;

                                            //	success
                                            if (ZpGameGlobals.GetVehicleLOCAL().m_RankCurrent < (int)_Value)
                                            {
                                                if ((int)_Value == 2)
                                                {
                                                    conditionData.AddValue();
                                                    conditionData.SetCompletedResult(new bool[] { true, true, true });
                                                }

                                                conditionData.GetSetIsCheckEnd = false;

                                                //	last rank AI out
                                                ZpEventListener.Broadcast("RemoveLastRankAI");
                                            }
                                            //	failed
                                            else
                                            {
                                                conditionData.GetSetIsCheckEnd = true;
                                                ZpEventListener.Broadcast("TimeLimitedSuddenDeathConditionFailed");
                                            }

#if UNITY_EDITOR
                                            if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                                ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                    _ConditionType.ToString()) + _Value.ToString());
#endif
                                        }
                                    }
                                    return;
                                }

                            case ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH:
                                {
                                    if (_Value != null && !ZpGlobals.PlayerManagerIsNull() && (int)_Value > 1)
                                    {
                                        bSuccess = true;

                                        //	success
                                        if (ZpGameGlobals.GetVehicleLOCAL().m_RankCurrent < (int)_Value)
                                        {
                                            if ((int)_Value == 2)
                                            {
                                                conditionData.AddValue();
                                                conditionData.SetCompletedResult(new bool[] { true, true, true });
                                            }

                                            conditionData.GetSetIsCheckEnd = false;

                                            //	last rank AI out
                                            ZpEventListener.Broadcast("RemoveLastRankAI");
                                        }
                                        //	failed
                                        else
                                            ZpEventListener.Broadcast("TimeLimitedSuddenDeathConditionFailed");

#if UNITY_EDITOR
                                        if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()) + _Value.ToString());
#endif
                                    }
                                    return;
                                }

                            case ConditionType.GRAND_PRIX:
                                {
                                    if (_Value != null)
                                    {
                                        cCareerGrandPrixConditionData grandPrixData = conditionData as cCareerGrandPrixConditionData;
                                        if (grandPrixData == null)
                                            return;

                                        Dictionary<int, KeyValuePair<int, float>> dicRank = _Value as Dictionary<int, KeyValuePair<int, float>>;
                                        if (dicRank != null)
                                        {
                                            bSuccess = grandPrixData.AddScore(dicRank);

#if UNITY_EDITOR
                                            ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                _ConditionType.ToString()) + dicRank[0].ToString());
#endif
                                        }
                                    }
                                }
                                break;

                            default:
                                {
                                    if (!conditionData.IsCompletedCheckEnd())
                                    {
                                        if (_Value == null)
                                        {
                                            bSuccess = conditionData.AddValue();

#if UNITY_EDITOR
                                            if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                                ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : Add",
                                                    _ConditionType.ToString()));
#endif
                                        }
                                        else
                                        {
                                            bSuccess = conditionData.AddValue((int)_Value);

#if UNITY_EDITOR
                                            if (bSuccess && !conditionData.GetSetIsCheckEnd)
                                                ZpLog.Normal(ZpLog.E_Category.Careeer, string.Format("@@ Career Condition Update [{0}] : ",
                                                    _ConditionType.ToString()) + _Value.ToString());
#endif
                                        }
                                    }
                                }
                                break;
                        }

                        //	notice integer type
                        if (bSuccess && !conditionData.IsCheckEnd())
                        {
                            string strDesc = GetNoticeConditionUpdateResultDesc(_ConditionType, conditionData);
                            ZpEventListener.Broadcast("EVENT_UI_CAREER_CONDITIONINFO", strDesc, true);
                        }
                    }
                    break;
            }
        }
    }

#endregion

#region notice condition update

    //	notice integer type
    string GetNoticeConditionUpdateResultDesc(ConditionType _type, cCareerIntegerTypeConditionData _Condition, bool _bUpdate = true)
    {
        CareerStageCondition careerCondition = m_CurrentPlayCareerInfo.getConditionFromType(_type);

        bool bComplete = false;
        System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Append(GetConditionDesc(careerCondition.conditionType, GetConditionValueForUseDesc(careerCondition)));

        int nValue = 0;

        switch (_type)
        {
            case ConditionType.TUTORIAL:
            case ConditionType.RANK:
            case ConditionType.TIME_LIMITED:
            case ConditionType.START_BOOST_USE_COUNT:
            case ConditionType.GRAND_PRIX:
                {
                    if (_bUpdate && ZpGameGlobals.m_ScriptGM && !ZpGameGlobals.m_ScriptGM.IsPause())
                        bComplete = _Condition.IsComplete();

                    nValue = bComplete ? 1 : 0;
                    if (!bComplete)
                        nValue = _Condition.IsCompleted() ? 1 : 0;
                    stringBuilder.Append(string.Format(" ({0}", nValue));
                    stringBuilder.Append(string.Format("/{0})", 1));

                    if (_bUpdate && _type == ConditionType.START_BOOST_USE_COUNT)
                        _Condition.GetSetIsCheckEnd = true;
                }
                break;

            case ConditionType.REMAIN_RANK_IN_SECOND:
                {
                    nValue = _Condition.GetBestValue();
                    stringBuilder.Append(string.Format(" ({0}", nValue));
                    stringBuilder.Append(string.Format("/{0})", careerCondition.conditionValue[0]));

                    if (!_Condition.GetIsInverse || _Condition.GetSetIsCheckEnd)
                        bComplete = _Condition.IsComplete();
                }
                break;

            case ConditionType.TIME_LIMITED_SUDDENDEATH:
            case ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH:
                {
                    nValue = _Condition.GetSetValue;
                    int nGoal = 3;
                    List<int> goalList = _Condition.GetGoalValueList() as List<int>;
                    if (goalList.Count > 0)
                        nGoal = goalList[goalList.Count - 1];

                    stringBuilder.Append(string.Format(" ({0}", nValue));
                    stringBuilder.Append(string.Format("/{0})", nGoal));

                    if (_type == ConditionType.LAPCOUNT_LIMITED_SUDDENDEATH)
                    {
                        int nCurrentLap = ZpGameGlobals.m_ScriptGM.GetVehicleFromRank(1).m_CurrentLap;
                        bComplete = nCurrentLap > 0 ? _Condition.IsComplete() : false;
                        if (nCurrentLap == 0)
                            _Condition.GetSetIsCheckEnd = false;
                        else
                        {
                            _Condition.GetSetIsCheckEnd = ZpGameGlobals.GetVehicleLOCAL().m_RankCurrent <
                                ZpGlobals.PlayerM.GetCountPlayer() - nValue ?
                                ZpGameGlobals.m_ScriptGM.m_TotalLaps == nCurrentLap : true;
                        }
                    }
                    else
                    {
                        bComplete = _Condition.IsComplete();
                        _Condition.GetSetIsCheckEnd = _bUpdate ? (ZpGameGlobals.m_ScriptGM.IsGameState(ZpGameMaster.GameState.ON_GAME) ?
                            ZpGameGlobals.m_ScriptGM.CountVehicles() == 1 : true) : false;

                        if (_Condition.GetSetIsCheckEnd)
                            ZpEventListener.Broadcast("EVENT_UI_CAREER_SUDDENDEATH_TIMECOUNT", false, 0);
                    }
                }
                break;

            //case ConditionType.GRAND_PRIX:
            //    {
            //        if (ZpGameGlobals.m_ScriptGM && !ZpGameGlobals.m_ScriptGM.IsPause())
            //            bComplete = _Condition.IsComplete();

            //        cCareerGrandPrixConditionData grandPrixData = _Condition as cCareerGrandPrixConditionData;
            //        if (grandPrixData == null)
            //            return string.Empty;

            //        int nGoal = 0;
            //        List<int> goalList = _Condition.GetGoalValueList() as List<int>;
            //        if (goalList.Count > 0)
            //            nGoal = goalList[goalList.Count - 1];

            //        nValue = Mathf.Min(nGoal, grandPrixData.GetResult());

            //        stringBuilder.Append(string.Format(" ({0}", nValue));
            //        stringBuilder.Append(string.Format("/{0})", nGoal));
            //    }
            //    break;

            default:
                {
                    nValue = _Condition.GetSetValue;
                    stringBuilder.Append(string.Format(" ({0}", nValue));
                    stringBuilder.Append(string.Format("/{0})", careerCondition.conditionValue[0]));

                    if (!_Condition.GetIsInverse || _Condition.GetSetIsCheckEnd)
                        bComplete = _Condition.IsComplete();
                }
                break;
        }

        if (!bComplete)
            bComplete = _Condition.IsCompleted();

        //	set label color
        if (_Condition.GetSetIsCheckEnd)
        {
            if (bComplete)
                stringBuilder.Insert(0, string.Format("[{0}]", ZpUtility.ConvertRGBtoHexReturnStr(CODITION_SUCCESS_COLOR)));
            else
                stringBuilder.Insert(0, string.Format("[{0}]", ZpUtility.ConvertRGBtoHexReturnStr(CODITION_FAILED_COLOR)));
        }
        else
        {
            if (bComplete)
                stringBuilder.Insert(0, string.Format("[{0}]", ZpUtility.ConvertRGBtoHexReturnStr(CODITION_SUCCESS_COLOR)));
            else
                stringBuilder.Insert(0, string.Format("[{0}]", ZpUtility.ConvertRGBtoHexReturnStr(CODITION_NORMAL_COLOR)));
        }

        //  update minimum desc
        UpdateConditionMinimumDesc(_type, _Condition.GetSetIsCheckEnd, bComplete);

        string strDesc = stringBuilder.ToString();
        if (m_ConditionResultDesc.ContainsKey(_type))
            m_ConditionResultDesc[_type] = new KeyValuePair<int, string>(nValue, strDesc);
        else
            m_ConditionResultDesc.Add(_type, new KeyValuePair<int, string>(nValue, strDesc));

        if (_type == ConditionType.GRAND_PRIX)
        {
            ZpEventListener.Broadcast("EVENT_UI_CAREER_CONDITIONINFO", strDesc);
            strDesc = string.Empty;
        }

        return strDesc;
    }

#endregion
}
