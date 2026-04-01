using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "new ItemData", menuName = "PixelLand/Data/Item", order = 0)]
public class ItemData : ScriptableObject
{
    [SerializeField]
    ItemInfo _itemInfo;
    public ItemInfo ItemInfo => _itemInfo;

    [SerializeField]
    ItemData[] _combiantionData = new ItemData[9];
    public ItemData[] CombiantionData => _combiantionData;

    [SerializeField]
    SkillData _skillData;
    public SkillData SkillData => _skillData;

    public void ApplyDirty()
    {
        EditorUtility.SetDirty(this);
    }
}

[CustomEditor(typeof(ItemData))]
public class ItemDataInspector : Editor
{
    private ItemData _data;
    private string _itemTypeHelp;
    private SerializedProperty _dataProperty, _skillProperty;

    public void OnEnable()
    {
        _data = (ItemData)target;

        StringBuilder _stringBuilder = new StringBuilder();
        _stringBuilder.AppendLine("==타입== : 대분류");
        _stringBuilder.AppendLine("<<타입>> : 소분류");
        _itemTypeHelp = _stringBuilder.ToString();
        _dataProperty = serializedObject.FindProperty("_combiantionData");
        _skillProperty = serializedObject.FindProperty("_skillData");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        _data.ItemInfo.ID = EditorGUILayout.TextField("아이템 ID", _data.ItemInfo.ID);
        _data.ItemInfo.DisplayName = EditorGUILayout.TextField("표시 이름", _data.ItemInfo.DisplayName);
        _data.ItemInfo.Sprite = EditorGUILayout.ObjectField("아이템 이미지", _data.ItemInfo.Sprite, typeof(Sprite), false) as Sprite;

        EditorGUILayout.LabelField("아이템 설명");
        _data.ItemInfo.Description = EditorGUILayout.TextArea(_data.ItemInfo.Description, GUILayout.Height(50));
        _data.ItemInfo.ItemType = (ItemType)EditorGUILayout.EnumPopup("아이템 타입", _data.ItemInfo.ItemType);
        EditorGUILayout.LabelField(_itemTypeHelp, GUILayout.Height(40));

        _data.ItemInfo.Value = EditorGUILayout.FloatField("수치", _data.ItemInfo.Value);

        _data.ItemInfo.MaxStackCount = EditorGUILayout.IntField("최대소지개수", _data.ItemInfo.MaxStackCount);
        _data.ItemInfo.IsSellable = EditorGUILayout.Toggle("판매 가능", _data.ItemInfo.IsSellable);
        _data.ItemInfo.IsDroppable = EditorGUILayout.Toggle("인벤토리 드롭가능", _data.ItemInfo.IsDroppable);

        EditorGUILayout.PropertyField(_dataProperty, new GUIContent("조합식 아이템"));
        for (int i = 0; i < _data.ItemInfo.CombinationInfo.Length; i++)
        {
            if (_data.CombiantionData[i] != null)
                _data.ItemInfo.CombinationInfo[i] = _data.CombiantionData[i].ItemInfo.ID;
            else
                _data.ItemInfo.CombinationInfo[i] = null;
        }

        if (_data.ItemInfo.ItemType >= (ItemType)1000 && _data.ItemInfo.ItemType < (ItemType)2000)
        {
            _data.ItemInfo.Elemental = (ElementalProperties)EditorGUILayout.EnumPopup("아이템 속성", _data.ItemInfo.Elemental);
            _data.ItemInfo.Durability = EditorGUILayout.FloatField("최대 내구도", _data.ItemInfo.Durability);
            _data.ItemInfo.IsRepairable = EditorGUILayout.Toggle("수리 가능", _data.ItemInfo.IsRepairable);

            EditorGUILayout.PropertyField(_skillProperty, new GUIContent("아이템 사용 스킬"));
            if(_data.SkillData != null)
                _data.ItemInfo.ItemSkill = _data.SkillData.Skill;
        }

        if (GUILayout.Button("저장"))
        {
            _data.ApplyDirty();
            Debug.Log("아이템 데이터 정보 저장 완료");
        }

        serializedObject.ApplyModifiedProperties();
    }
}

//[CustomPropertyDrawer(typeof(ItemInfo))]
//public class ItemInfoDrawer : PropertyDrawer
//{
//    private ItemInfo _itemInfo;

//    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//    {
//        using (new EditorGUI.PropertyScope(position, label, property))
//        {
//            EditorGUIUtility.labelWidth = 50;

//            position.height = EditorGUIUtility.singleLineHeight;
//            var heightInterval = EditorGUIUtility.singleLineHeight + 2;

//            var halfWidth = position.width * 0.5f;

//            var spriteRect = new Rect(position)
//            {
//                width = 64,
//                height = 64
//            };

//            var idRect = new Rect(position)
//            {
//                width = position.width - 64,
//                x = position.x + 64
//            };

//            var nameRect = new Rect(idRect)
//            {
//                y = idRect.y + heightInterval
//            };

//            var descriptionRect = new Rect(position)
//            {
//                y = spriteRect.y + heightInterval
//            };

//            var itemTypeRect = new Rect(descriptionRect)
//            {
//                y = descriptionRect.y + heightInterval
//            };

//            var durabilityRect = new Rect(itemTypeRect)
//            {
//                y = itemTypeRect.y + heightInterval
//            };

//            var maxStackRect = new Rect(durabilityRect)
//            {
//                y = durabilityRect.y + heightInterval
//            };

//            var repairableRect = new Rect(maxStackRect)
//            {
//                width = position.width * 0.5f,
//                y = maxStackRect.y + heightInterval
//            };

//            var sellableRect = new Rect(repairableRect)
//            {
//                x = repairableRect.x + repairableRect.width
//            };

//            var idProperty = property.FindPropertyRelative("ID");
//            var nameProperty = property.FindPropertyRelative("DisplayName");
//            var spriteProperty = property.FindPropertyRelative("Sprite");
//            var descriptionProperty = property.FindPropertyRelative("Description");
//            var itemTypeProperty = property.FindPropertyRelative("ItemType");
//            var durabilityProperty = property.FindPropertyRelative("Durability");
//            var stackProperty = property.FindPropertyRelative("MaxStackCount");
//            var repairableProperty = property.FindPropertyRelative("IsRepairable");
//            var sellableProperty = property.FindPropertyRelative("IsSellable");

//            spriteProperty.objectReferenceValue = EditorGUI.ObjectField(spriteRect, spriteProperty.objectReferenceValue, typeof(Texture), false);
//            idProperty.stringValue = EditorGUI.TextField(idRect, "Display Name", idProperty.stringValue);
//            nameProperty.stringValue = EditorGUI.TextField(nameRect, "ID (Name)", nameProperty.stringValue);
//            descriptionProperty.stringValue = EditorGUI.TextArea(descriptionRect, "아이템 설명", descriptionProperty.stringValue);



//            //_data.ItemInfo.ID = EditorGUILayout.TextField("ID (Name)", _data.ItemInfo.ID);
//            //_data.ItemInfo.DisplayName = EditorGUILayout.TextField("Display Name", _data.ItemInfo.DisplayName);
//            //_data.ItemInfo.Sprite = EditorGUILayout.ObjectField("아이템 이미지", _data.ItemInfo.Sprite, typeof(Sprite), false) as Sprite;
//            //EditorGUILayout.LabelField("아이템 설명");
//            //_data.ItemInfo.Description = EditorGUILayout.TextArea(_data.ItemInfo.Description, GUILayout.Height(50));

//            //_data.ItemInfo.ItemType = (Danny_ItemType)EditorGUILayout.EnumPopup("아이템 타입", _data.ItemInfo.ItemType);
//            //EditorGUILayout.LabelField(_itemTypeHelp, GUILayout.Height(40));

//            //_data.ItemInfo.Durability = EditorGUILayout.FloatField("내구도", _data.ItemInfo.Durability);
//            //_data.ItemInfo.MaxStackCount = EditorGUILayout.IntField("최대 소지개수", _data.ItemInfo.MaxStackCount);
//            //_data.ItemInfo.IsRepairable = EditorGUILayout.Toggle("수리가능", _data.ItemInfo.IsRepairable);
//            //_data.ItemInfo.IsSellable = EditorGUILayout.Toggle("판매가능", _data.ItemInfo.IsSellable);
//        }
//    }
//}