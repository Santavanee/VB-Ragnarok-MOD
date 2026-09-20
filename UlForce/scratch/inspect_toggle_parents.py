import UnityPy

env = UnityPy.load('E:/GameH/VenusBlood RAGNAROK International (US)/VBRI_Data/level62')

for obj in env.objects:
    if obj.type.name == 'MonoBehaviour':
        try:
            raw = obj.get_raw_data()
            # Search for PPtr references in raw data
            # Or read with UnityPy if possible
        except:
            pass

# Let's inspect GameObjects and their hierarchy under Canvas or Research
for obj in env.objects:
    if obj.type.name == 'GameObject':
        data = obj.read()
        name = getattr(data, 'm_Name', '')
        if name in ['Toggle6', 'Toggle7', 'Toggle8', 'pages']:
            parent = None
            for c in data.m_Components:
                try:
                    c_data = c.read()
                    if 'Transform' in c_data.type.name:
                        # find parent
                        p_ptr = getattr(c_data, 'm_Father', None)
                        if p_ptr:
                            p_trans = p_ptr.read()
                            p_go = p_trans.m_GameObject.read()
                            parent = p_go.m_Name
                except Exception as ex:
                    pass
            print(f"GO: {name} | active: {getattr(data, 'm_IsActive', True)} | parent: {parent}")

