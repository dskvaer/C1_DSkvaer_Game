#if UNITY_EDITOR
using C1.Platformer.Characters;
using C1.Platformer.Characters.RPG;
using C1.Platformer.Characters.Settings;
using Spine.Unity;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace C1.Platformer.Characters.EditorTools {
    public static class PlatformerCharacterSetupTool {
        private const string ProfilePath = "Assets/C1/Scripts/Platformer/Characters/Settings/DefaultPlatformerMovementProfile.asset";
        private const string InputActionsPath = "Assets/InputSystem_Actions.inputactions";
        private const string SkeletonDataPath = "Assets/C1/Animation/Player_0/Player_SkeletonData.asset";
        private const string PrefabPath = "Assets/C1/Prefabs/Player/Platformer/PlatformerHero.prefab";
        private const string PrefabSettingsFolder = "Assets/C1/Prefabs/Player/Platformer/Settings";
        private const string DefaultClassPath = PrefabSettingsFolder + "/Hero_DefaultClass.asset";
        private const string DefaultSkillTreePath = PrefabSettingsFolder + "/Hero_CommonSkillTree.asset";

        [MenuItem("C1/Platformer/Create Default Movement Profile")]
        public static void CreateDefaultMovementProfileAsset()
        {
            CreateOrLoadDefaultProfile();
        }

        [MenuItem("C1/Platformer/Create Ready Hero Prefab")]
        public static void CreateReadyHeroPrefab()
        {
            CreateDefaultPlatformerHeroPrefab();
        }

        public static void CreateDefaultPlatformerHeroPrefab()
        {
            EnsureFolder("Assets/C1/Prefabs/Player/Platformer");
            EnsureFolder(PrefabSettingsFolder);

            PlatformerMovementProfile movementProfile = CreateOrLoadDefaultProfile();
            PlatformerCharacterClassDefinition defaultClass = CreateOrLoadDefaultClass();
            PlatformerSkillTreeDefinition skillTree = CreateOrLoadDefaultSkillTree();

            GameObject root = new GameObject("PlatformerHero");
            root.layer = LayerMask.NameToLayer("Player");
            Rigidbody2D rigidbody = root.AddComponent<Rigidbody2D>();
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

            CapsuleCollider2D bodyCollider = root.AddComponent<CapsuleCollider2D>();
            bodyCollider.size = new Vector2(0.7f, 1.8f);
            bodyCollider.offset = new Vector2(0f, 0.9f);

            Transform feetProbe = CreateChild(root.transform, "FeetProbe", new Vector3(0f, 0.05f, 0f));
            Transform chestProbe = CreateChild(root.transform, "ChestProbe", new Vector3(0f, 0.9f, 0f));
            Transform headProbe = CreateChild(root.transform, "HeadProbe", new Vector3(0f, 1.75f, 0f));
            Transform weaponSocket = CreateChild(root.transform, "WeaponSocket", new Vector3(0.35f, 1.15f, 0f));
            Transform projectileOrigin = CreateChild(root.transform, "ProjectileOrigin", new Vector3(0.55f, 1.15f, 0f));

            PlayerInput playerInput = root.AddComponent<PlayerInput>();
            InputActionAsset inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(InputActionsPath);
            if (inputActions != null)
            {
                playerInput.actions = inputActions;
                playerInput.defaultActionMap = "Player";
            }

            PlayerPlatformerInputSource playerInputSource = root.AddComponent<PlayerPlatformerInputSource>();
            PlatformerTouchInputSource touchInputSource = root.AddComponent<PlatformerTouchInputSource>();
            touchInputSource.enabled = false;

            PlatformerCharacterMotor motor = root.AddComponent<PlatformerCharacterMotor>();
            AssignObject(motor, "profile", movementProfile);
            AssignObject(motor, "inputSourceComponent", playerInputSource);
            AssignObject(motor, "bodyCollider", bodyCollider);
            AssignObject(motor, "feetProbe", feetProbe);
            AssignObject(motor, "chestProbe", chestProbe);
            AssignObject(motor, "headProbe", headProbe);

            PlatformerCharacterVitals vitals = root.AddComponent<PlatformerCharacterVitals>();
            PlatformerCharacterProgression progression = root.AddComponent<PlatformerCharacterProgression>();
            AssignObject(progression, "characterClass", defaultClass);
            AssignObjectArray(progression, "skillTrees", skillTree);

            PlatformerInteractionSensor interactionSensor = root.AddComponent<PlatformerInteractionSensor>();
            AssignObject(interactionSensor, "motor", motor);
            AssignObject(interactionSensor, "inputSourceComponent", playerInputSource);

            PlatformerWeaponController weaponController = root.AddComponent<PlatformerWeaponController>();
            AssignObject(weaponController, "motor", motor);
            AssignObject(weaponController, "vitals", vitals);
            AssignObject(weaponController, "inputSourceComponent", playerInputSource);
            AssignObject(weaponController, "defaultWeaponSocket", weaponSocket);
            AssignObject(weaponController, "projectileOrigin", projectileOrigin);

            root.AddComponent<PlatformerCharacterRigNotes>();

            GameObject visual = new GameObject("Visual_Spine_4_3");
            visual.transform.SetParent(root.transform, false);
            SkeletonAnimation skeletonAnimation = visual.AddComponent<SkeletonAnimation>();
            SkeletonDataAsset skeletonData = AssetDatabase.LoadAssetAtPath<SkeletonDataAsset>(SkeletonDataPath);
            if (skeletonData != null)
            {
                skeletonAnimation.skeletonDataAsset = skeletonData;
                skeletonAnimation.initialSkinName = "default";
                skeletonAnimation.AnimationName = "Standart_Idle";
            }

            PlatformerSpineAnimator spineAnimator = visual.AddComponent<PlatformerSpineAnimator>();
            AssignObject(spineAnimator, "motor", motor);
            AssignObject(spineAnimator, "weaponController", weaponController);
            AssignObject(spineAnimator, "skeletonAnimation", skeletonAnimation);
            AssignString(spineAnimator, "idle", "Standart_Idle");
            AssignString(spineAnimator, "walk", "Standart_Run");
            AssignString(spineAnimator, "run", "Standart_Run");
            AssignString(spineAnimator, "push", "Standart_Push");
            AssignString(spineAnimator, "ledgeHang", "Jump_Hanging_on_a_cliff");
            AssignString(spineAnimator, "jumpRise", "Jump_Jump");
            AssignString(spineAnimator, "jumpFall", "Jump_Jump2");
            AssignString(spineAnimator, "armedIdle", "Attack_Idle_Armed");
            AssignString(spineAnimator, "armedWalk", "Attack_Run_Armed");

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();
            Debug.Log($"Ready platformer hero prefab created: {PrefabPath}", prefab);
        }

        [MenuItem("C1/Platformer/Setup Selected Character")]
        public static void SetupSelectedCharacter()
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("Platformer setup: select a character GameObject first.");
                return;
            }

            SetupCharacter(Selection.activeGameObject);
        }

        public static void SetupCharacter(GameObject root)
        {
            if (root == null)
            {
                return;
            }

            Undo.RegisterFullObjectHierarchyUndo(root, "Setup Platformer Character");

            PlatformerMovementProfile movementProfile = CreateOrLoadDefaultProfile();
            Rigidbody2D rigidbody = root.GetComponent<Rigidbody2D>() ?? Undo.AddComponent<Rigidbody2D>(root);
            rigidbody.freezeRotation = true;
            rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;

            Collider2D bodyCollider = root.GetComponent<Collider2D>();
            if (bodyCollider == null)
            {
                CapsuleCollider2D capsule = Undo.AddComponent<CapsuleCollider2D>(root);
                capsule.size = new Vector2(0.7f, 1.8f);
                bodyCollider = capsule;
            }

            MonoBehaviour inputSource = EnsureInputSource(root);
            PlatformerCharacterMotor motor = root.GetComponent<PlatformerCharacterMotor>() ?? Undo.AddComponent<PlatformerCharacterMotor>(root);
            AssignObject(motor, "profile", movementProfile);
            AssignObject(motor, "inputSourceComponent", inputSource);
            AssignObject(motor, "bodyCollider", bodyCollider);

            PlatformerCharacterVitals vitals = root.GetComponent<PlatformerCharacterVitals>() ?? Undo.AddComponent<PlatformerCharacterVitals>(root);

            PlatformerInteractionSensor interactionSensor = root.GetComponent<PlatformerInteractionSensor>() ?? Undo.AddComponent<PlatformerInteractionSensor>(root);
            AssignObject(interactionSensor, "motor", motor);
            AssignObject(interactionSensor, "inputSourceComponent", inputSource);
            AssignLayerMask(interactionSensor, "interactableLayers", "Interactable");

            PlatformerWeaponController weaponController = root.GetComponent<PlatformerWeaponController>() ?? Undo.AddComponent<PlatformerWeaponController>(root);
            AssignObject(weaponController, "motor", motor);
            AssignObject(weaponController, "vitals", vitals);
            AssignObject(weaponController, "inputSourceComponent", inputSource);
            AssignObject(weaponController, "defaultWeaponSocket", FindDeepChild(root.transform, "WeaponSocket", "WeaponPoint", "RightHand", "Hand_R"));
            AssignObject(weaponController, "projectileOrigin", FindDeepChild(root.transform, "ProjectileOrigin", "Muzzle", "WeaponPoint"));

            PlatformerCharacterProgression progression = root.GetComponent<PlatformerCharacterProgression>() ?? Undo.AddComponent<PlatformerCharacterProgression>(root);
            AssignObject(progression, "characterClass", CreateOrLoadDefaultClass());
            AssignObjectArray(progression, "skillTrees", CreateOrLoadDefaultSkillTree());

            if (root.GetComponent<PlatformerCharacterRigNotes>() == null)
            {
                Undo.AddComponent<PlatformerCharacterRigNotes>(root);
            }

            SkeletonAnimation skeletonAnimation = root.GetComponentInChildren<SkeletonAnimation>(true);
            if (skeletonAnimation != null)
            {
                PlatformerSpineAnimator spineAnimator = skeletonAnimation.GetComponent<PlatformerSpineAnimator>() ?? Undo.AddComponent<PlatformerSpineAnimator>(skeletonAnimation.gameObject);
                AssignObject(spineAnimator, "motor", motor);
                AssignObject(spineAnimator, "weaponController", weaponController);
                AssignObject(spineAnimator, "skeletonAnimation", skeletonAnimation);
            }

            EditorUtility.SetDirty(root);
            Debug.Log($"Platformer setup complete for {root.name}.", root);
        }

        private static MonoBehaviour EnsureInputSource(GameObject root)
        {
            PlayerPlatformerInputSource playerSource = root.GetComponent<PlayerPlatformerInputSource>();
            if (playerSource != null)
            {
                return playerSource;
            }

            ScriptedPlatformerInputSource scriptedSource = root.GetComponent<ScriptedPlatformerInputSource>();
            if (scriptedSource != null)
            {
                return scriptedSource;
            }

            return root.GetComponent<PlayerInput>() != null
                ? Undo.AddComponent<PlayerPlatformerInputSource>(root)
                : Undo.AddComponent<ScriptedPlatformerInputSource>(root);
        }

        private static PlatformerMovementProfile CreateOrLoadDefaultProfile()
        {
            PlatformerMovementProfile profile = AssetDatabase.LoadAssetAtPath<PlatformerMovementProfile>(ProfilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PlatformerMovementProfile>();
                AssetDatabase.CreateAsset(profile, ProfilePath);
            }

            SerializedObject serializedProfile = new SerializedObject(profile);
            SetLayerMask(serializedProfile, "groundLayers", "Ground");
            SetLayerMask(serializedProfile, "pushableLayers", "Pushable");
            SetLayerMask(serializedProfile, "waterLayers", "Water");
            SetLayerMask(serializedProfile, "ladderLayers", "Ladder");
            SetLayerMask(serializedProfile, "interactableLayers", "Interactable");
            serializedProfile.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            Debug.Log($"Default platformer movement profile is ready: {ProfilePath}", profile);
            return profile;
        }

        private static PlatformerCharacterClassDefinition CreateOrLoadDefaultClass()
        {
            EnsureFolder(PrefabSettingsFolder);
            PlatformerCharacterClassDefinition characterClass = AssetDatabase.LoadAssetAtPath<PlatformerCharacterClassDefinition>(DefaultClassPath);
            if (characterClass == null)
            {
                characterClass = ScriptableObject.CreateInstance<PlatformerCharacterClassDefinition>();
                AssetDatabase.CreateAsset(characterClass, DefaultClassPath);
            }

            EditorUtility.SetDirty(characterClass);
            AssetDatabase.SaveAssets();
            return characterClass;
        }

        private static PlatformerSkillTreeDefinition CreateOrLoadDefaultSkillTree()
        {
            EnsureFolder(PrefabSettingsFolder);
            PlatformerSkillTreeDefinition skillTree = AssetDatabase.LoadAssetAtPath<PlatformerSkillTreeDefinition>(DefaultSkillTreePath);
            if (skillTree == null)
            {
                skillTree = ScriptableObject.CreateInstance<PlatformerSkillTreeDefinition>();
                AssetDatabase.CreateAsset(skillTree, DefaultSkillTreePath);
            }

            EditorUtility.SetDirty(skillTree);
            AssetDatabase.SaveAssets();
            return skillTree;
        }

        private static void AssignObject(Object target, string propertyName, Object value)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void AssignObjectArray(Object target, string propertyName, params Object[] values)
        {
            if (target == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null || !property.isArray)
            {
                return;
            }

            property.arraySize = values != null ? values.Length : 0;
            for (int i = 0; values != null && i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void AssignString(Object target, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void AssignLayerMask(Object target, string propertyName, params string[] layerNames)
        {
            SerializedObject serializedObject = new SerializedObject(target);
            SetLayerMask(serializedObject, propertyName, layerNames);
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(target);
        }

        private static void SetLayerMask(SerializedObject serializedObject, string propertyName, params string[] layerNames)
        {
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            int mask = 0;
            for (int i = 0; i < layerNames.Length; i++)
            {
                int layer = LayerMask.NameToLayer(layerNames[i]);
                if (layer >= 0)
                {
                    mask |= 1 << layer;
                }
            }

            property.intValue = mask;
        }

        private static Transform FindDeepChild(Transform root, params string[] names)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < names.Length; i++)
            {
                Transform found = FindDeepChild(root, names[i]);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform FindDeepChild(Transform root, string childName)
        {
            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindDeepChild(root.GetChild(i), childName);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        private static Transform CreateChild(Transform parent, string name, Vector3 localPosition)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.transform.localPosition = localPosition;
            return child.transform;
        }

        private static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = System.IO.Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string leaf = System.IO.Path.GetFileName(folderPath);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, leaf);
            }
        }
    }
}
#endif
