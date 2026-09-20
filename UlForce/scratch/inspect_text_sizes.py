import UnityPy

env = UnityPy.load(r"E:\GameH\VenusBlood RAGNAROK International (US)\VBRI_Data\level61")

for obj in env.objects:
    if "GameObject" in str(obj.type):
        data = obj.read()
        if data.m_Name in ["eqitemToggle", "eqitem"]:
            print(f"=== {data.m_Name} ===")
            for c in data.m_Components:
                try:
                    cr = c.read()
                    if "Text" in str(type(cr)):
                        print(f"Text on {data.m_Name}: fontSize={getattr(cr, 'm_FontSize', None)}")
                except:
                    pass
            # check children
            tr = None
            for c in data.m_Components:
                try:
                    cr = c.read()
                    if "Transform" in str(type(cr)):
                        tr = cr
                        break
                except:
                    pass
            if tr:
                for ch in tr.m_Children:
                    ch_go = ch.read().m_GameObject.read()
                    for c in ch_go.m_Components:
                        try:
                            cr = c.read()
                            if "Text" in str(type(cr)):
                                print(f"  Text on {ch_go.m_Name}: fontSize={getattr(cr, 'm_FontSize', None)}")
                        except:
                            pass
                    # grand children
                    for gch in ch.read().m_Children:
                        gch_go = gch.read().m_GameObject.read()
                        for c in gch_go.m_Components:
                            try:
                                cr = c.read()
                                if "Text" in str(type(cr)):
                                    print(f"    Text on {gch_go.m_Name}: fontSize={getattr(cr, 'm_FontSize', None)}")
                            except:
                                pass
                        # great grand children
                        for ggch in gch.read().m_Children:
                            ggch_go = ggch.read().m_GameObject.read()
                            for c in ggch_go.m_Components:
                                try:
                                    cr = c.read()
                                    if "Text" in str(type(cr)):
                                        print(f"      Text on {ggch_go.m_Name}: fontSize={getattr(cr, 'm_FontSize', None)}")
                                except:
                                    pass

