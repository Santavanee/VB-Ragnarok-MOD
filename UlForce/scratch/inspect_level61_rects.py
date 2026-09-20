import UnityPy

env = UnityPy.load(r"E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\level61")

for obj in env.objects:
    if "GameObject" in str(obj.type):
        data = obj.read()
        name = getattr(data, "m_Name", "")
        if name in ["EquipmentList", "ListPanel", "List", "Scrollbar", "eqitem", "eqitemToggle"]:
            for c in data.m_Components:
                try:
                    cr = c.read()
                    if "RectTransform" in str(type(cr)):
                        print(f"{name}: pos=({cr.m_AnchoredPosition.x}, {cr.m_AnchoredPosition.y}) size=({cr.m_SizeDelta.x}, {cr.m_SizeDelta.y}) min=({cr.m_AnchorMin.x}, {cr.m_AnchorMin.y}) max=({cr.m_AnchorMax.x}, {cr.m_AnchorMax.y}) pivot=({cr.m_Pivot.x}, {cr.m_Pivot.y})")
                except Exception as e:
                    pass

