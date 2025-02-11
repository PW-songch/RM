using UnityEngine;
using System.Collections.Generic;
using KMNetClass;
using eSkillType = ZpTool_SkillInfo.eSkillType;

public class ZpTool_SkillInfo : MonoBehaviour
{
    [System.Serializable]
    public struct tagSkillInfoInEdit
    {
        public eSkillType skillType;
        public PARTS_FUNC skillFuncType;
        public ItemClass carPartsClass;
        public int skillID;
        public int level;
        public int maxLevel;

        public tagSkillInfoInEdit(eSkillType _eSkillType, tagSkillInfo _skillInfo, ItemClass _eCarPartsClass = ItemClass.None)
        {
            skillType = eSkillType.END;
            skillFuncType = PARTS_FUNC.NONE;
            carPartsClass = ItemClass.None;
            skillID = 0;
            level = 0;
            maxLevel = 0;

            SetSkillInfo(_eSkillType, _eCarPartsClass, _skillInfo.nSkillID, _skillInfo.nLevel);
        }

        private void SetSkillInfo(eSkillType _skillType, ItemClass _itemClass, int _skillID, int _level)
        {
            skillType = _skillType;
            skillFuncType = (PARTS_FUNC)ZpTool_SkillTest.SkillTable.GetAffectTypeFromAffectID(_skillID);
            carPartsClass = _itemClass;
            skillID = _skillID;
            level = _level;
            maxLevel = _skillType == eSkillType.CHARACTER ? ZpTool_SkillTest.CharacterSkillTable.GetMaxLevel() : 0;
        }
    }

    public enum eSkillType
    {
        CAR_PARTS,
        CHARACTER,
        END
    }

    public int m_playerSlotNum;
    public string m_playerName;
    public List<tagSkillInfoInEdit> m_partsSkillList = new List<tagSkillInfoInEdit>();
    public List<tagSkillInfoInEdit> m_characterSkillList = new List<tagSkillInfoInEdit>();

    //  editor info
    public bool m_isRootFoldout = false;
    public List<bool> m_isSkillFoldoutList = new List<bool>();
    public List<int> m_skillSizeList = new List<int>();

    public void SetSkillInfo(int _slotNum, string _playerName)
    {
        SetSkill(_slotNum, _playerName);
    }

    public void SetSkillInfo(ZpTool_SkillInfo _skill)
    {
        if (_skill != null)
        {
            m_partsSkillList = new List<tagSkillInfoInEdit>(_skill.m_partsSkillList);
            m_characterSkillList = new List<tagSkillInfoInEdit>(_skill.m_characterSkillList);
            SetSkill(_skill.m_playerSlotNum, _skill.m_playerName);
            SetEditorInfo(_skill);            
        }
    }

    public void UpdateSkillList(eSkillType _eSkillType, List<tagSkillInfoInEdit> _skillList)
    {
        switch (_eSkillType)
        {
            default:
            case eSkillType.CAR_PARTS:
                m_partsSkillList = new List<tagSkillInfoInEdit>(_skillList);
                break;
            case eSkillType.CHARACTER:
                m_characterSkillList = new List<tagSkillInfoInEdit>(_skillList);
                break;
        }
    }

    public void ResetSkillInfo()
    {
        for (int i = 0 ; i < (int)eSkillType.END ; ++i)
            GetSkillList(i).Clear();
        if (m_isSkillFoldoutList != null)
            m_isSkillFoldoutList.Clear();
        if (m_skillSizeList != null)
            m_skillSizeList.Clear();

        for (int i = 0 ; i < (int)eSkillType.END ; ++i)
        {
            if (m_isSkillFoldoutList != null)
                m_isSkillFoldoutList.Add(false);
            if (m_skillSizeList != null)
                m_skillSizeList.Add(0);
        }
    }

    public List<tagSkillInfoInEdit> GetSkillList(eSkillType _eSkillType)
    {
        switch (_eSkillType)
        {
            default:
            case eSkillType.CAR_PARTS:
                return m_partsSkillList;
            case eSkillType.CHARACTER:
                return m_characterSkillList;
        }
    }

    public List<tagSkillInfoInEdit> GetSkillList(int _nSkillType)
    {
        return GetSkillList((eSkillType)_nSkillType);
    }

    public void SetSkill(int _slotNum, string _playerName)
    {
        m_playerName = _playerName;
        m_playerSlotNum = _slotNum;
    }

    public void UpdateSkill(eSkillType _eSkillType, int _nGap)
    {
        if (_nGap > 0)
            AddSkill(_eSkillType, Mathf.Abs(_nGap));
        else if (_nGap < 0)
            RemoveSkill(_eSkillType, Mathf.Abs(_nGap));

        List<tagSkillInfoInEdit> skillList = GetSkillList(_eSkillType);
        if (skillList != null)
            SetSkillSize(_eSkillType, skillList.Count);
    }

    public void UpdateSkillInfo()
    {
        if (m_skillSizeList != null)
        {
            for (int i = 0 ; i < (int)eSkillType.END ; ++i)
            {
                List<tagSkillInfoInEdit> skillList = GetSkillList(i);
                if (skillList != null)
                    m_skillSizeList[i] = skillList.Count;
            }
        }
    }

    public void AddSkill(eSkillType _eSkillType, tagSkillInfo _skillInfo, ItemClass _eCarPartsClass = ItemClass.None)
    {
        List<tagSkillInfoInEdit> skillList = GetSkillList(_eSkillType);
        if (skillList != null)
        {
            skillList.Add(new tagSkillInfoInEdit(_eSkillType, _skillInfo, _eCarPartsClass));
            SetSkillSize(_eSkillType, skillList.Count);
        }
    }

    public void AddSkill(eSkillType _eSkillType)
    {
        AddSkill(_eSkillType, new tagSkillInfo(ZpTool_SkillTest.SkillTable.GetAffectIDFromAffectType((int)PARTS_FUNC.NONE)));
    }

    public void AddSkill(eSkillType _eSkillType, int _nCount)
    {
        for (int i = 0 ; i < _nCount ; ++i)
            AddSkill(_eSkillType);
    }

    public void RemoveSkill(eSkillType _eSkillType, int _nCount)
    {
        List<tagSkillInfoInEdit> skillList = GetSkillList(_eSkillType);
        if (skillList != null)
        {
            _nCount = Mathf.Clamp(_nCount, 0, skillList.Count);
            skillList.RemoveRange(skillList.Count - _nCount, _nCount);
        }
    }

    public void SetEditorInfo(ZpTool_SkillInfo _skill)
    {
        if (_skill != null)
        {
            m_isRootFoldout = _skill.m_isRootFoldout;
            m_isSkillFoldoutList = new List<bool>(_skill.m_isSkillFoldoutList);
            m_skillSizeList = new List<int>(_skill.m_skillSizeList);
        }
    }

    public bool IsSkillFoldout(eSkillType _eSkillType)
    {
        if (m_isSkillFoldoutList != null && (int)_eSkillType < m_isSkillFoldoutList.Count)
            return m_isSkillFoldoutList[(int)_eSkillType];
        return false;
    }

    public void SetIsSkillFoldout(eSkillType _eSkillType, bool _bFoldout)
    {
        if (m_isSkillFoldoutList != null && (int)_eSkillType < m_isSkillFoldoutList.Count)
            m_isSkillFoldoutList[(int)_eSkillType] = _bFoldout;
    }

    public void SetAllFoldout(bool _bFoldout)
    {
        m_isRootFoldout = _bFoldout;
        SetIsSkillFoldout(_bFoldout);
    }

    public void SetIsSkillFoldout(bool _bFoldout)
    {
        if (m_isSkillFoldoutList != null)
        {
            for (int i = 0 ; i < m_isSkillFoldoutList.Count ; ++i)
                m_isSkillFoldoutList[i] = _bFoldout;
        }
    }

    public int SkillSize(eSkillType _eSkillType)
    {
        if (m_skillSizeList != null && (int)_eSkillType < m_skillSizeList.Count)
            return m_skillSizeList[(int)_eSkillType];
        return 0;
    }

    public void SetSkillSize(eSkillType _eSkillType, int _nSize)
    {
        if (m_skillSizeList != null && (int)_eSkillType < m_skillSizeList.Count)
            m_skillSizeList[(int)_eSkillType] = _nSize;
    }
}
