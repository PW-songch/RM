using UnityEngine;
using System.Collections.Generic;
using KMNetClass;
using tagSkillInfoInEdit = ZpTool_SkillInfo.tagSkillInfoInEdit;
using eSkillType = ZpTool_SkillInfo.eSkillType;

public class ZpTool_SkillTest : MonoBehaviour
{
    public ZpGM m_GM;
    public GameObject m_objApplySkillRoot;
    public GameObject m_objEditorSkillRoot;
    public bool m_UseSkillTool = false;

    [HideInInspector] public int m_nPlayerCount = 0;
    [HideInInspector] public bool m_bEditorSkillFoldout = false;

    private List<ZpTool_SkillInfo> m_applySkillInfoList = new List<ZpTool_SkillInfo>();
    public List<ZpTool_SkillInfo> ApplySkillInfoList { get { return m_applySkillInfoList; } }
    private List<ZpTool_SkillInfo> m_editorSkillInfoList = new List<ZpTool_SkillInfo>();
    public List<ZpTool_SkillInfo> EditorSkillInfoList { get { return m_editorSkillInfoList; } }

    private ZpCSVAffectSkillUpgradeInfo m_skillUpgradeInfoTable;
    private ZpCSVAffectSkillUpgradeInfo SkillUpgradeInfoTable
    {
        get
        {
            if (m_skillUpgradeInfoTable == null)
            {
                m_skillUpgradeInfoTable = ZpGlobals.s_ScriptCSVDataPool.IfAffectSkillUpgradeInfo != null ?
                    ZpGlobals.s_ScriptCSVDataPool.IfAffectSkillUpgradeInfo : new ZpCSVAffectSkillUpgradeInfo();
            }

            return m_skillUpgradeInfoTable;
        }
    }

    static private ZpCSVAffect m_skillTable = null;
    static public ZpCSVAffect SkillTable
    {
        get
        {
            if (m_skillTable == null)
            {
                m_skillTable = ZpGlobals.s_ScriptCSVDataPool.IfAffect != null ?
                    ZpGlobals.s_ScriptCSVDataPool.IfAffect : new ZpCSVAffect();
            }

            return m_skillTable;
        }
    }

    static private ZpCSVAffectInfo m_skillInfoTable;
    static public ZpCSVAffectInfo SkillInfoTable
    {
        get
        {
            if (m_skillInfoTable == null)
            {
                m_skillInfoTable = ZpGlobals.s_ScriptCSVDataPool.IfAffectInfo != null ?
                    ZpGlobals.s_ScriptCSVDataPool.IfAffectInfo : new ZpCSVAffectInfo();
            }

            return m_skillInfoTable;
        }
    }

    static private ZpCSVAffectSkillExpInfo m_characterSkillTable;
    static public ZpCSVAffectSkillExpInfo CharacterSkillTable
    {
        get
        {
            if (m_characterSkillTable == null)
            {
                m_characterSkillTable = ZpGlobals.s_ScriptCSVDataPool.IfAffectSkillExpInfo != null ?
                    ZpGlobals.s_ScriptCSVDataPool.IfAffectSkillExpInfo : new ZpCSVAffectSkillExpInfo();
            }

            return m_characterSkillTable;
        }
    }

    void Awake()
    {
        if (m_GM == null)
        {
            if (this.transform.parent != null)
                m_GM = this.transform.parent.GetComponentInChildren<ZpGameMaster>();
            else
                m_GM = FindObjectOfType<ZpGameMaster>();
        }
    }

    private void UpdateApplySkillList()
    {
        if (m_applySkillInfoList.Count == 0 || Application.isPlaying == true)
            m_applySkillInfoList = new List<ZpTool_SkillInfo>(m_objApplySkillRoot.GetComponentsInChildren<ZpTool_SkillInfo>());
    }

    private void UpdateEditorSkillList()
    {
        if (m_editorSkillInfoList.Count == 0 || Application.isPlaying == true)
            m_editorSkillInfoList = new List<ZpTool_SkillInfo>(m_objEditorSkillRoot.GetComponentsInChildren<ZpTool_SkillInfo>());
    }

    private GameObject SetSkillInfoRootObj(GameObject _objRoot)
    {
        if (_objRoot == null)
        {
            _objRoot = new GameObject();
            _objRoot.transform.parent = this.transform;
        }

        return _objRoot;
    }

    public ZpTool_SkillInfo CreateEditSkill(GameObject _root, ZpTool_SkillInfo _skill, int _nSlotNum, int _nCount)
    {
        ZpTool_SkillInfo editSkill = _root.AddComponent<ZpTool_SkillInfo>();
        editSkill.ResetSkillInfo();

        if (_skill == null)
            editSkill.SetSkillInfo(_nSlotNum, _nCount == 0 ? "@LOCAL" : "@AI_" + _nSlotNum);
        else
            editSkill.SetSkillInfo(_skill);
        return editSkill;
    }

    public void UpdateEditorSkillList(List<ZpTool_SkillInfo> _fromSkillList, int _nGap = 0)
    {
        m_objEditorSkillRoot = SetSkillInfoRootObj(m_objEditorSkillRoot);
        UpdateSkillList(m_objEditorSkillRoot, m_editorSkillInfoList, _fromSkillList, _nGap);
    }

    public void UpdateApplySkillList(List<ZpTool_SkillInfo> _fromSkillList, int _nGap = 0)
    {
        m_objApplySkillRoot = SetSkillInfoRootObj(m_objApplySkillRoot);
        UpdateSkillList(m_objApplySkillRoot, m_applySkillInfoList, _fromSkillList, _nGap);

        if (m_GM != null && Application.isPlaying == true)
            m_GM.ApplySkill();
    }

    public void UpdateSkillList(GameObject _root, List<ZpTool_SkillInfo> _skillList, 
        List<ZpTool_SkillInfo> _fromSkillList, int _nGap = 0)
    {
        if (_nGap > 0)
            AddSkillList(_root, _skillList, _fromSkillList, Mathf.Abs(_nGap));
        else if (_nGap < 0)
            RemoveSkillList(_skillList, Mathf.Abs(_nGap));
        
        foreach (ZpTool_SkillInfo skill in _fromSkillList)
            SetSkill(_skillList, skill);
    }

    private void SetSkill(List<ZpTool_SkillInfo> _skillList, ZpTool_SkillInfo _skill)
    {
        if (_skillList != null && _skill != null)
        {
            for (int i = 0 ; i < _skillList.Count ; ++i)
            {
                ZpTool_SkillInfo skill = _skillList[i];
                if (skill != null && skill.m_playerSlotNum == _skill.m_playerSlotNum)
                {
                    _skillList[i].SetSkillInfo(_skill);
                    _skill = null;
                    break;
                }
            }

            AddSkill(_skillList, _skill);
        }
    }

    public void AddSkill(List<ZpTool_SkillInfo> _skillList, ZpTool_SkillInfo _skill)
    {
        if (_skillList != null && _skill != null)
            _skillList.Add(_skill);
    }

    public void AddSkillList(GameObject _root, List<ZpTool_SkillInfo> _skillList, List<ZpTool_SkillInfo> _fromSkillList, int _nCount)
    {
        if (_skillList != null && _fromSkillList != null)
        {
            bool bNew = _skillList.Count != _fromSkillList.Count;
            int nCount = _skillList.Count;
            _nCount += nCount;

            for (int i = nCount ; i < _nCount ; ++i)
                AddSkill(_skillList, CreateEditSkill(_root, bNew == true && i < _fromSkillList.Count ? 
                    _fromSkillList[i] : null, i, _skillList.Count));
        }
    }

    public bool RemoveSkillList(List<ZpTool_SkillInfo> _skillList, ZpTool_SkillInfo _skill)
    {
        if (_skillList != null && _skill != null)
        {
            _skillList.Remove(_skill);
            DestroyImmediate(_skill);
            return true;
        }

        return false;
    }

    public void RemoveSkillList(List<ZpTool_SkillInfo> _skillList, int _nCount)
    {
        if (_skillList != null)
        {
            _nCount = Mathf.Clamp(_nCount, 0, _skillList.Count);
            for (int i = _skillList.Count - _nCount ; i < _skillList.Count ; )
            {
                if (RemoveSkillList(_skillList, _skillList[i]) == false)
                    i++;
            }
        }
    }

    public void ResetSkill()
    {
        RemoveSkillList(m_editorSkillInfoList, m_editorSkillInfoList.Count);
        m_nPlayerCount = m_editorSkillInfoList.Count;
    }

    public void SetIsSkillFoldout(List<ZpTool_SkillInfo> _skillList, int _nSlotNum, bool _bFoldout)
    {
        if (_skillList != null)
        {
            foreach (ZpTool_SkillInfo skill in _skillList)
            {
                if (skill.m_playerSlotNum == _nSlotNum)
                {
                    skill.SetIsSkillFoldout(_bFoldout);
                    break;
                }
            }
        }
    }

    public void SetAllFoldout(List<ZpTool_SkillInfo> _skillList, bool _bFoldout)
    {
        m_bEditorSkillFoldout = _bFoldout;
        if (_skillList != null)
        {
            foreach (ZpTool_SkillInfo skill in _skillList)
                skill.SetAllFoldout(_bFoldout);
        }
    }

    public List<ZpTool_SkillInfo> GetEditorSkillList(bool _bUpdate)
    {
        UpdateApplySkillList();
        UpdateEditorSkillList();

        if (_bUpdate == true)
        {
            foreach (ZpTool_SkillInfo skill in m_editorSkillInfoList)
            {
                if (skill != null)
                    skill.UpdateSkillInfo();
            }
        }

        return new List<ZpTool_SkillInfo>(m_editorSkillInfoList);
    }

    public List<tagSkillInfo> GetCarPartsSkillList(int _nSlotNum)
    {
        UpdateApplySkillList();

        List<tagSkillInfo> skillList = new List<tagSkillInfo>();
        if (m_applySkillInfoList != null && m_applySkillInfoList.Count > 0)
        {
            foreach (ZpTool_SkillInfo skill in m_applySkillInfoList)
            {
                if (skill.m_playerSlotNum == _nSlotNum)
                {
                    List<tagSkillInfoInEdit> partsSkillList = skill.GetSkillList(eSkillType.CAR_PARTS);
                    if (partsSkillList != null)
                    {
                        foreach (tagSkillInfoInEdit partsSkill in partsSkillList)
                        {
                            if (partsSkill.skillType != eSkillType.END && partsSkill.skillFuncType != PARTS_FUNC.NONE
                                && partsSkill.skillFuncType != PARTS_FUNC.END
                                && SkillTable.IsContainAffectID(partsSkill.skillID) == true)
                            {
                                tagSkillInfo skillInfo = new tagSkillInfo();
                                skillInfo.SetVehiclePartsSkill(partsSkill.skillID, partsSkill.level, partsSkill.carPartsClass);
                                skillList.Add(skillInfo);
                            }
                        }
                    }
                }
            }
        }

        return skillList;
    }

    public List<KMPropertyInfo> GetCharacterSkillList(int _nSlotNum)
    {
        UpdateApplySkillList();

        List<KMPropertyInfo> skillList = new List<KMPropertyInfo>();
        if (m_applySkillInfoList != null && m_applySkillInfoList.Count > 0)
        {
            foreach (ZpTool_SkillInfo skill in m_applySkillInfoList)
            {
                if (skill.m_playerSlotNum == _nSlotNum)
                {
                    List<tagSkillInfoInEdit> characterSkillList = skill.GetSkillList(eSkillType.CHARACTER);
                    if (characterSkillList != null)
                    {
                        foreach (tagSkillInfoInEdit characterSkill in characterSkillList)
                        {
                            if (characterSkill.skillType != eSkillType.END && characterSkill.skillFuncType != PARTS_FUNC.NONE
                                && characterSkill.skillFuncType != PARTS_FUNC.END
                                && SkillTable.IsContainAffectID(characterSkill.skillID) == true)
                                skillList.Add(new KMPropertyInfo(characterSkill.skillID, 0, (byte)characterSkill.level, false));
                        }
                    }
                }
            }
        }

        return skillList;
    }

    public List<ZpTool_SkillInfo> GetSkillListFromGM()
    {
        m_objEditorSkillRoot = SetSkillInfoRootObj(m_objEditorSkillRoot);

        if (m_GM != null)
        {
            //  local
            ZpTool_SkillInfo testSkill = CreateEditSkill(m_objEditorSkillRoot, null, m_GM.m_STANDALONE_Lane_LOCAL, 0);

            int[] arrayVehicleParts = new int[3];
            arrayVehicleParts[0] = m_GM.m_LOCAL_BODY;
            arrayVehicleParts[1] = m_GM.m_LOCAL_SPOILER;
            arrayVehicleParts[2] = m_GM.m_LOCAL_WHEEL;

            const int nSkillIDCount = (int)KEY_ITEMDB.i_AffectID3 - (int)KEY_ITEMDB.i_AffectID1;
            int nSkillType = 0;
            tagSkillInfo skillInfo;

            for (int i = 0 ; i < arrayVehicleParts.Length ; ++i)
            {
                int nPartsID = arrayVehicleParts[i];
                nSkillType = (int)KEY_ITEMDB.i_AffectID1;
                for (int j = 0 ; j <= nSkillIDCount ; ++j)
                {
                    int nSkillID = (int)ZpGlobals.s_ScriptItemDB.GetItemDB(nPartsID, (KEY_ITEMDB)nSkillType++);
                    if (SkillTable.IsContainAffectID(nSkillID) == true)
                    {
                        ItemClass itemClass = ZpItemUtil.GetItemClass(nPartsID);
                        skillInfo = new tagSkillInfo();
                        skillInfo.nSkillID = nSkillID;
                        skillInfo.nLevel = 1;
                        skillInfo.fValue = SkillUpgradeInfoTable.GetSkillStatsValue(
                            itemClass, skillInfo.nSkillID, skillInfo.nLevel);
                        testSkill.AddSkill(eSkillType.CAR_PARTS, skillInfo, itemClass);
                    }
                }
            }

            nSkillType = (int)KEY_ITEMDB.i_AffectID1;
            for (int i = 0 ; i <= nSkillIDCount ; ++i)
            {
                int nSkillID = (int)ZpGlobals.s_ScriptItemDB.GetItemDB(
                    m_GM.m_STANDALONE_CostumeIDCharacter_LOCAL, (KEY_ITEMDB)nSkillType++);
                if (SkillTable.IsContainAffectID(nSkillID) == true)
                {
                    skillInfo = new tagSkillInfo();
                    skillInfo.nSkillID = nSkillID;
                    skillInfo.nLevel = 1;
                    skillInfo.fValue = SkillTable.GetAffectValue(skillInfo.nSkillID, skillInfo.nLevel);
                    testSkill.AddSkill(eSkillType.CHARACTER, skillInfo);
                }
            }

            AddSkill(m_editorSkillInfoList, testSkill);

            //  AI
            if (m_GM.m_Array_STANDALONE_AI_ID != null)
            {
                for (int i = 0 ; i < m_GM.m_Array_STANDALONE_AI_ID.Length ; ++i)
                {
                    int aiID = m_GM.m_Array_STANDALONE_AI_ID[i];
                    arrayVehicleParts = new int[3];
                    arrayVehicleParts[0] = (int)ZpItemDB.Instance.GetItemDB(aiID, KEY_ITEMDB.i_AIBody);
                    arrayVehicleParts[1] = (int)ZpItemDB.Instance.GetItemDB(aiID, KEY_ITEMDB.i_AISpolier);
                    arrayVehicleParts[2] = (int)ZpItemDB.Instance.GetItemDB(aiID, KEY_ITEMDB.i_AIWheel);

                    int nSlot = i + 1;
                    ZpTool_SkillInfo testSkillAI = CreateEditSkill(m_objEditorSkillRoot, null, nSlot, nSlot);

                    for (int j = 0 ; j < arrayVehicleParts.Length ; ++j)
                    {
                        int nPartsID = arrayVehicleParts[j];
                        nSkillType = (int)KEY_ITEMDB.i_AffectID1;
                        for (int k = 0 ; k <= nSkillIDCount ; ++k)
                        {
                            int nSkillID = (int)ZpGlobals.s_ScriptItemDB.GetItemDB(nPartsID, (KEY_ITEMDB)nSkillType++);
                            if (SkillTable.IsContainAffectID(nSkillID) == true)
                            {
                                ItemClass itemClass = ZpItemUtil.GetItemClass(nPartsID);
                                skillInfo = new tagSkillInfo();
                                skillInfo.nSkillID = nSkillID;
                                skillInfo.nLevel = 1;
                                skillInfo.fValue = SkillUpgradeInfoTable.GetSkillStatsValue(
                                    itemClass, skillInfo.nSkillID, skillInfo.nLevel);
                                testSkillAI.AddSkill(eSkillType.CAR_PARTS, skillInfo, itemClass);
                            }
                        }
                    }

                    int nConstumeID = (int)ZpItemDB.Instance.GetItemDB(aiID, KEY_ITEMDB.i_AICostume);
                    nSkillType = (int)KEY_ITEMDB.i_AffectID1;
                    for (int j = 0 ; j <= nSkillIDCount ; ++j)
                    {
                        int nSkillID = (int)ZpGlobals.s_ScriptItemDB.GetItemDB(nConstumeID, (KEY_ITEMDB)nSkillType++);
                        if (SkillTable.IsContainAffectID(nSkillID) == true)
                        {
                            skillInfo = new tagSkillInfo();
                            skillInfo.nSkillID = nSkillID;
                            skillInfo.nLevel = 1;
                            skillInfo.fValue = SkillTable.GetAffectValue(skillInfo.nSkillID, skillInfo.nLevel);
                            testSkillAI.AddSkill(eSkillType.CHARACTER, skillInfo);
                        }
                    }

                    AddSkill(m_editorSkillInfoList, testSkillAI);
                }
            }
        }

        m_nPlayerCount = m_editorSkillInfoList.Count;
        return new List<ZpTool_SkillInfo>(m_editorSkillInfoList);
    }

    public int GetMaxSkillLevel(int _nSkillID, int _nLevel, ItemClass _partsClass)
    {
        if (SkillUpgradeInfoTable != null)
            return SkillUpgradeInfoTable.GetSkillUpgradeStats((int)_partsClass, _nSkillID, _nLevel).nLimitLevel;
        return 0;
    }

    public void ClearSkill()
    {
        m_applySkillInfoList.Clear();
        m_editorSkillInfoList.Clear();
    }
}
