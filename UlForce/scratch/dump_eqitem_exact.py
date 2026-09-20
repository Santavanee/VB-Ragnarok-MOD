import UnityPy

env = UnityPy.load(r"E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\level61")

def dump_all(trans, indent=0):
    try:
        go = trans.m_GameObject.read()
        ap = trans.m_AnchoredPosition
        sd = trans.m_SizeDelta
        amin = trans.m_AnchorMin
        amax = trans.m_AnchorMax
        piv = trans.m_Pivot
        print("  "*indent + f"{go.m_Name}: pos=({ap.x},{ap.y}) size=({sd.x},{sd.y}) min=({amin.x},{amin.y}) max=({amax.x},{amax.y}) piv=({piv.x},{piv.y})")
        for ch in trans.m_Children:
            try:
                dump_all(ch.read(), indent + 1)
            except Exception as ex:
                pass
    except Exception as e:
        print("Error:", e)

for obj in env.objects:
    if "GameObject" in str(obj.type):
        data = obj.read()
        if data.m_Name == "eqitemToggle":
            for c in data.m_Components:
                try:
                    cr = c.read()
                    if "RectTransform" in str(type(cr)):
                        dump_all(cr)
                except:
                    pass

