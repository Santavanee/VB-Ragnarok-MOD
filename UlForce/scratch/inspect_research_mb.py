import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'MonoBehaviour':
        data = obj.read()
        script = getattr(data, 'm_Script', None)
        if script:
            try:
                script_data = script.read()
                if script_data.m_Name in ['ResearchControl', 'ResearchListHandler']:
                    print(f"=== MonoBehaviour: {script_data.m_Name} ===")
                    raw = data.get_raw_data()
                    # print type tree / keys if available
                    try:
                        tree = data.read_typetree()
                        for k, v in tree.items():
                            print(f"  {k} = {v}")
                    except Exception as e:
                        print(f"  typetree error: {e}")
            except Exception as e:
                pass

