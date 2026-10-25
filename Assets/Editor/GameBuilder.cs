// GameBuilder.cs
// Naz Yagmur - GAME2014 Assignment 1
// editor script - menu GAME2014 > Build Game Scenes.
// it sets up the sprites, animations, prefabs and all 4 scenes so i dont have to do it by hand every time

using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class GameBuilder
{
    private const float PPU = 10f;
    private const string SceneDir = "Assets/Scenes";
    private const string PrefabDir = "Assets/Prefabs";
    private const string AnimDir = "Assets/Animations";

    private static readonly Color Yellow = new Color32(255, 205, 117, 255);
    private static readonly Color Cyan = new Color32(115, 239, 247, 255);
    private static readonly Color Orange = new Color32(239, 125, 87, 255);
    private static readonly Color Grey = new Color32(148, 176, 194, 255);
    private static readonly Color Navy = new Color32(24, 25, 46, 255);

    private static Material spriteMaterial;
    private static Font font;

    private static GameObject playerBulletPrefab, enemyBulletPrefab, explosionPrefab;
    private static GameObject chaserPrefab, shooterPrefab, coinPrefab, starPrefab;
    private static RuntimeAnimatorController playerController;

    [MenuItem("GAME2014/Build Game Scenes")]
    public static void BuildAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        ConfigureSprites();
        PrepareFolders();
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/PressStart2P-Regular.ttf");
        spriteMaterial = null;

        BuildAnimationsAndPrefabs();

        string menu = BuildMainMenu();
        string instructions = BuildInstructions();
        string play = BuildPlay();
        string gameOver = BuildGameOver();

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(menu, true),
            new EditorBuildSettingsScene(instructions, true),
            new EditorBuildSettingsScene(play, true),
            new EditorBuildSettingsScene(gameOver, true),
        };
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.productName = "Cosmic Warden";
        PlayerSettings.companyName = "Naz Yagmur";

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(menu);
        Debug.Log("<color=lime>GAME2014: all scenes built. Press Play in the MainMenu scene.</color>");
    }

    private static void ConfigureSprites()
    {
        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Sprites" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = PPU;
            ti.filterMode = FilterMode.Point; // no blur for pixel art
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            if (path.Contains("ui_button") || path.Contains("ui_panel"))
                ti.spriteBorder = new Vector4(4, 4, 4, 4); // 9 slice
            ti.SaveAndReimport();
        }
    }

    private static void PrepareFolders()
    {
        foreach (string dir in new[] { PrefabDir, AnimDir })
        {
            if (AssetDatabase.IsValidFolder(dir)) AssetDatabase.DeleteAsset(dir);
            AssetDatabase.CreateFolder("Assets", dir.Substring("Assets/".Length));
        }
        if (!AssetDatabase.IsValidFolder(SceneDir)) AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    // helpers
    private static Sprite S(string name)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/{name}.png");
        if (s == null) Debug.LogError("Missing sprite: " + name);
        return s;
    }

    private static Sprite[] Frames(string prefix, int count)
    {
        var list = new Sprite[count];
        for (int i = 0; i < count; i++) list[i] = S($"{prefix}_{i}");
        return list;
    }

    private static AudioClip A(string path)
    {
        var c = AssetDatabase.LoadAssetAtPath<AudioClip>($"Assets/Audio/{path}.wav");
        if (c == null) Debug.LogError("Missing audio: " + path);
        return c;
    }

    private static Material SpriteMat()
    {
        if (spriteMaterial == null)
        {
            spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(
                "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat");
            if (spriteMaterial == null)
                spriteMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
        }
        return spriteMaterial;
    }

    private static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        if (p == null) { Debug.LogError($"Field '{field}' not found on {target.GetType().Name}"); return; }
        switch (value)
        {
            case Object o: p.objectReferenceValue = o; break;
            case int i: p.intValue = i; break;
            case float f: p.floatValue = f; break;
            case bool b: p.boolValue = b; break;
            case string s: p.stringValue = s; break;
            case System.Enum e: p.intValue = System.Convert.ToInt32(e); break;
            case Vector2 v: p.vector2Value = v; break;
        }
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void SetArray(Object target, string field, IList<Object> values)
    {
        var so = new SerializedObject(target);
        SerializedProperty p = so.FindProperty(field);
        p.arraySize = values.Count;
        for (int i = 0; i < values.Count; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static GameObject SpriteObject(string name, Sprite sprite, int order, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = order;
        sr.sharedMaterial = SpriteMat();
        return go;
    }

    // rigidbody with no gravity, only for trigger detection
    private static void AddBody(GameObject go, bool dynamic)
    {
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = dynamic ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.None;
    }

    private static AnimationClip MakeClip(string name, Sprite[] frames, float fps, bool loop)
    {
        var clip = new AnimationClip { frameRate = fps, name = name };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = frames[i] };
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = frames[frames.Length - 1] };
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        AssetDatabase.CreateAsset(clip, $"{AnimDir}/{name}.anim");
        return clip;
    }

    private static RuntimeAnimatorController MakeController(string name, AnimationClip clip)
    {
        return AnimatorController.CreateAnimatorControllerAtPathWithClip($"{AnimDir}/{name}.controller", clip);
    }

    private static GameObject SavePrefab(GameObject go)
    {
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{go.name}.prefab");
        Object.DestroyImmediate(go);
        return prefab;
    }

    // animations and prefabs
    private static void BuildAnimationsAndPrefabs()
    {
        playerController = MakeController("Player", MakeClip("Player_Thrust", Frames("player", 3), 12, true));
        var chaserAnim = MakeController("Chaser", MakeClip("Chaser_Fly", Frames("chaser", 2), 6, true));
        var shooterAnim = MakeController("Shooter", MakeClip("Shooter_Lights", Frames("shooter", 2), 4, true));
        var explosionAnim = MakeController("Explosion", MakeClip("Explosion", Frames("explosion", 6), 14, false));
        var coinAnim = MakeController("Coin", MakeClip("Coin_Spin", Frames("coin", 4), 10, true));
        var starAnim = MakeController("Star", MakeClip("Star_Twinkle", Frames("star", 2), 5, true));

        // explosion
        var ex = SpriteObject("Explosion", S("explosion_0"), 10);
        ex.AddComponent<Animator>().runtimeAnimatorController = explosionAnim;
        Set(ex.AddComponent<AutoDestroy>(), "lifetime", 6f / 14f);
        explosionPrefab = SavePrefab(ex);

        // bullets
        var pb = SpriteObject("PlayerBullet", S("bullet_player"), 4);
        pb.AddComponent<BoxCollider2D>().isTrigger = true;
        AddBody(pb, false);
        var pbc = pb.AddComponent<Bullet>();
        Set(pbc, "owner", Bullet.Owner.Player); Set(pbc, "speed", 18f);
        playerBulletPrefab = SavePrefab(pb);

        var eb = SpriteObject("EnemyBullet", S("bullet_enemy"), 4);
        eb.AddComponent<CircleCollider2D>().isTrigger = true;
        AddBody(eb, false);
        var ebc = eb.AddComponent<Bullet>();
        Set(ebc, "owner", Bullet.Owner.Enemy); Set(ebc, "speed", 6.5f);
        enemyBulletPrefab = SavePrefab(eb);

        // enemies
        var ch = SpriteObject("Chaser", S("chaser_0"), 3);
        ch.AddComponent<Animator>().runtimeAnimatorController = chaserAnim;
        var chCol = ch.AddComponent<CircleCollider2D>(); chCol.isTrigger = true; chCol.radius *= 0.8f;
        AddBody(ch, true);
        var chc = ch.AddComponent<ChaserEnemy>();
        Set(chc, "maxHealth", 1); Set(chc, "scoreValue", 100); Set(chc, "speed", 2.6f);
        Set(chc, "detectionRadius", 5f); Set(chc, "explosionPrefab", explosionPrefab);
        Set(chc, "explosionSfx", A("SFX/sfx_explosion"));
        chaserPrefab = SavePrefab(ch);

        var sh = SpriteObject("Shooter", S("shooter_0"), 3);
        sh.AddComponent<Animator>().runtimeAnimatorController = shooterAnim;
        var shCol = sh.AddComponent<BoxCollider2D>(); shCol.isTrigger = true;
        AddBody(sh, true);
        var shc = sh.AddComponent<ShooterEnemy>();
        Set(shc, "maxHealth", 3); Set(shc, "scoreValue", 250); Set(shc, "speed", 3f);
        Set(shc, "detectionRadius", 6f); Set(shc, "explosionPrefab", explosionPrefab);
        Set(shc, "explosionSfx", A("SFX/sfx_explosion"));
        Set(shc, "shootSfx", A("SFX/sfx_enemy_shoot"));
        shooterPrefab = SavePrefab(sh);

        // pickups
        coinPrefab = MakePickup("Coin", "coin_0", coinAnim, 50, 2f, "SFX/sfx_coin");
        starPrefab = MakePickup("Star", "star_0", starAnim, 200, 1.6f, "SFX/sfx_star");
    }

    private static GameObject MakePickup(string name, string sprite, RuntimeAnimatorController anim,
                                         int points, float fall, string sfx)
    {
        var go = SpriteObject(name, S(sprite), 2);
        if (anim != null) go.AddComponent<Animator>().runtimeAnimatorController = anim;
        var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; col.radius *= 1.2f; // bigger so its easier to collect
        AddBody(go, false);
        var p = go.AddComponent<Pickup>();
        Set(p, "points", points); Set(p, "fallSpeed", fall); Set(p, "collectSfx", A(sfx));
        return SavePrefab(go);
    }

    private static void NewScene()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static string SaveScene(string name)
    {
        string path = $"{SceneDir}/{name}.unity";
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path);
        return path;
    }

    private static Camera MakeCamera()
    {
        var go = new GameObject("Main Camera") { tag = "MainCamera" };
        go.transform.position = new Vector3(0, 0, -10);
        var cam = go.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 8f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Navy;
        go.AddComponent<AudioListener>();
        return cam;
    }

    private static void MakeBackground(float speed)
    {
        var root = new GameObject("Background");
        Sprite s = S("background");
        var a = SpriteObject("Tile A", s, -100, root.transform).transform;
        var b = SpriteObject("Tile B", s, -100, root.transform).transform;
        b.localPosition = new Vector3(0, s.bounds.size.y, 0);
        var sb = root.AddComponent<ScrollingBackground>();
        SetArray(sb, "tiles", new Object[] { a, b });
        Set(sb, "speed", speed);
    }

    private static void MakeAudio(string music)
    {
        var go = new GameObject("AudioManager");
        var am = go.AddComponent<AudioManager>();
        var m = go.AddComponent<AudioSource>(); m.playOnAwake = false;
        var f = go.AddComponent<AudioSource>(); f.playOnAwake = false;
        Set(am, "musicSource", m);
        Set(am, "sfxSource", f);

        var sm = new GameObject("SceneMusic").AddComponent<SceneMusic>();
        Set(sm, "music", A("Music/" + music));
    }

    private static void MakeEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    private static Transform MakeCanvas(string name, int order)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var c = go.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = order;
        var cs = go.AddComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1080, 1920);
        cs.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go.transform;
    }

    private static RectTransform UIRect(Transform parent, string name, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = anchor;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return rt;
    }

    private static Text MakeText(Transform parent, string name, string content, int size, Color color,
                                 Vector2 anchor, Vector2 pos, Vector2 box, TextAnchor align = TextAnchor.MiddleCenter)
    {
        var t = UIRect(parent, name, anchor, pos, box).gameObject.AddComponent<Text>();
        t.font = font;
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.text = content;
        t.supportRichText = true;
        t.lineSpacing = 1.35f;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var shadow = t.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.85f);
        shadow.effectDistance = new Vector2(4, -4);
        return t;
    }

    private static Image MakeImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        var img = UIRect(parent, name, anchor, pos, size).gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.raycastTarget = false;
        return img;
    }

    private static Image PixelImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 pos, float scale)
    {
        Vector2 size = new Vector2(sprite.rect.width, sprite.rect.height) * scale;
        return MakeImage(parent, name, sprite, anchor, pos, size);
    }

    private static void Animate(Image img, Sprite[] frames, float fps)
    {
        var a = img.gameObject.AddComponent<UISpriteAnimator>();
        SetArray(a, "frames", frames);
        Set(a, "framesPerSecond", fps);
    }

    private static Button MakeButton(Transform parent, string label, Vector2 anchor, Vector2 pos, Vector2 size,
                                     SceneLoader loader, string scene)
    {
        Image img = MakeImage(parent, label + " Button", S("ui_button"), anchor, pos, size);
        img.type = Image.Type.Sliced;
        img.pixelsPerUnitMultiplier = 1.6f;
        img.raycastTarget = true;
        var btn = img.gameObject.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 1f, 1f);
        colors.pressedColor = new Color(0.6f, 0.75f, 0.9f);
        btn.colors = colors;
        MakeText(img.transform, "Label", label, 40, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0, 4), size);
        UnityEventTools.AddStringPersistentListener(btn.onClick, new UnityAction<string>(loader.LoadScene), scene);
        return btn;
    }

    private static SceneLoader MakeSceneLoader(string clickSfx = "SFX/sfx_click")
    {
        var loader = new GameObject("SceneLoader").AddComponent<SceneLoader>();
        Set(loader, "clickSfx", A(clickSfx));
        return loader;
    }

    private static void CommonScene(string music, float bgSpeed)
    {
        NewScene();
        MakeCamera();
        MakeBackground(bgSpeed);
        MakeAudio(music);
        MakeEventSystem();
    }

    // scenes
    private static readonly Vector2 Center = new Vector2(0.5f, 0.5f);
    private static readonly Vector2 Top = new Vector2(0.5f, 1f);
    private static readonly Vector2 Bottom = new Vector2(0.5f, 0f);

    private static string BuildMainMenu()
    {
        CommonScene("music_menu", 0.8f);
        SceneLoader loader = MakeSceneLoader();
        Transform ui = MakeCanvas("Canvas", 0);

        MakeText(ui, "Title", "<color=#FFCD75>COSMIC</color>\n<color=#73EFF7>WARDEN</color>", 104, Color.white,
                 Center, new Vector2(0, 450), new Vector2(1000, 320));

        MakeButton(ui, "PLAY", Center, new Vector2(0, -100), new Vector2(640, 150), loader, SceneLoader.Play);
        MakeButton(ui, "INSTRUCTIONS", Center, new Vector2(0, -290), new Vector2(640, 150), loader, SceneLoader.Instructions);
        return SaveScene("MainMenu");
    }

    private static string BuildInstructions()
    {
        CommonScene("music_menu", 0.8f);
        SceneLoader loader = MakeSceneLoader("SFX/sfx_back");
        Transform ui = MakeCanvas("Canvas", 0);

        MakeText(ui, "Title", "HOW TO PLAY", 64, Yellow, Top, new Vector2(0, -190), new Vector2(1000, 90));
        MakeText(ui, "Goal", "SURVIVE AND GET A HIGH SCORE!", 24, Cyan, Top, new Vector2(0, -290), new Vector2(1000, 50));

        // rows appear one by one
        RectTransform rows = UIRect(ui, "Rows", Center, new Vector2(0, 40), new Vector2(980, 1100));
        rows.gameObject.AddComponent<UIStaggerFadeIn>();
        float[] ys = { 420, 210, 0, -210, -420 };

        // row 1 - ship and finger move together
        RectTransform r1 = Row(rows, "Row Controls", ys[0], "HOLD & DRAG TO MOVE.\nYOUR SHIP FIRES WHILE\nYOU TOUCH THE SCREEN.");
        Image demoShip = PixelImage(r1, "Ship", S("player_0"), Center, new Vector2(-340, 25), 6f);
        Animate(demoShip, Frames("player", 3), 12);
        Set(demoShip.gameObject.AddComponent<UIFloat>(), "distance", new Vector2(45, 0));
        Image finger = PixelImage(r1, "Finger", S("icon_finger"), Center, new Vector2(-320, -65), 5f);
        Set(finger.gameObject.AddComponent<UIFloat>(), "distance", new Vector2(45, 0));

        RectTransform r2 = Row(rows, "Row Raider", ys[1], "RAIDERS CHASE YOU WHEN\nYOU GET CLOSE.\n<color=#FFCD75>100 PTS</color>");
        Animate(PixelImage(r2, "Raider", S("chaser_0"), Center, new Vector2(-340, 0), 7f), Frames("chaser", 2), 6);

        RectTransform r3 = Row(rows, "Row Saucer", ys[2], "SAUCERS SHOOT WHEN\nTHEY SEE YOU.\n<color=#FFCD75>250 PTS</color>");
        Animate(PixelImage(r3, "Saucer", S("shooter_0"), Center, new Vector2(-340, 0), 6f), Frames("shooter", 2), 4);

        RectTransform r4 = Row(rows, "Row Loot", ys[3], "COLLECT LOOT:\nCOIN  <color=#FFCD75>50 PTS</color>\nSTAR  <color=#FFCD75>200 PTS</color>");
        Animate(PixelImage(r4, "Coin", S("coin_0"), Center, new Vector2(-385, 0), 7f), Frames("coin", 4), 10);
        Animate(PixelImage(r4, "Star", S("star_0"), Center, new Vector2(-290, 0), 6f), Frames("star", 2), 5);

        RectTransform r5 = Row(rows, "Row Lives", ys[4], "YOU HAVE 3 LIVES.\nLOSE ALL LIVES AND\nIT IS GAME OVER!");
        PixelImage(r5, "Heart", S("heart"), Center, new Vector2(-340, 0), 8f);

        MakeButton(ui, "BACK", Bottom, new Vector2(0, 220), new Vector2(480, 140), loader, SceneLoader.MainMenu);
        return SaveScene("Instructions");
    }

    private static RectTransform Row(Transform parent, string name, float y, string text)
    {
        RectTransform row = UIRect(parent, name, Center, new Vector2(0, y), new Vector2(980, 190));
        Image panel = MakeImage(row, "Panel", S("ui_panel"), Center, Vector2.zero, new Vector2(980, 190));
        panel.type = Image.Type.Sliced;
        panel.pixelsPerUnitMultiplier = 2.5f;
        panel.color = new Color(1, 1, 1, 0.85f);
        MakeText(row, "Text", text, 26, Color.white, Center, new Vector2(110, 0), new Vector2(680, 170), TextAnchor.MiddleLeft);
        return row;
    }

    private static string BuildPlay()
    {
        CommonScene("music_game", 1.6f);

        var gm = new GameObject("GameManager").AddComponent<GameManager>();
        Set(gm, "gameOverSfx", A("SFX/sfx_gameover"));

        // object pools
        var playerPool = new GameObject("PlayerBulletPool").AddComponent<BulletPool>();
        Set(playerPool, "bulletPrefab", playerBulletPrefab.GetComponent<Bullet>());
        Set(playerPool, "initialSize", 40);
        var enemyPool = new GameObject("EnemyBulletPool").AddComponent<BulletPool>();
        Set(enemyPool, "bulletPrefab", enemyBulletPrefab.GetComponent<Bullet>());
        Set(enemyPool, "initialSize", 30);

        // player
        var player = SpriteObject("Player", S("player_0"), 5);
        player.transform.position = new Vector3(0, -5.5f, 0);
        player.AddComponent<Animator>().runtimeAnimatorController = playerController;
        var pCol = player.AddComponent<CircleCollider2D>(); pCol.isTrigger = true; pCol.radius = 0.45f;
        AddBody(player, true);
        var muzzle = new GameObject("Muzzle").transform;
        muzzle.SetParent(player.transform, false);
        muzzle.localPosition = new Vector3(0, 1f, 0);
        var pc = player.AddComponent<PlayerController>();
        Set(pc, "bulletPool", playerPool);
        Set(pc, "muzzle", muzzle);
        Set(pc, "shootSfx", A("SFX/sfx_shoot"));
        Set(pc, "hitSfx", A("SFX/sfx_player_hit"));
        Set(pc, "explosionPrefab", explosionPrefab);
        Set(pc, "explosionSfx", A("SFX/sfx_explosion"));

        // enemy spawner + factories
        var spawnerGO = new GameObject("EnemySpawner");
        var chaserFactory = spawnerGO.AddComponent<ChaserFactory>();
        Set(chaserFactory, "prefab", chaserPrefab.GetComponent<ChaserEnemy>());
        Set(chaserFactory, "spawnWeight", 2f);
        var shooterFactory = spawnerGO.AddComponent<ShooterFactory>();
        Set(shooterFactory, "prefab", shooterPrefab.GetComponent<ShooterEnemy>());
        Set(shooterFactory, "spawnWeight", 1f);
        var spawner = spawnerGO.AddComponent<EnemySpawner>();
        SetArray(spawner, "factories", new Object[] { chaserFactory, shooterFactory });
        Set(spawner, "player", player.transform);
        Set(spawner, "enemyBulletPool", enemyPool);

        // pickup spawner
        var ps = new GameObject("PickupSpawner").AddComponent<PickupSpawner>();
        SetArray(ps, "pickups", new Object[] { coinPrefab.GetComponent<Pickup>(), starPrefab.GetComponent<Pickup>() });

        // hud
        Transform ui = MakeCanvas("HUD", 0);
        Vector2 topLeft = new Vector2(0, 1), topRight = new Vector2(1, 1);
        MakeText(ui, "Score Label", "SCORE", 22, Grey, topLeft, new Vector2(250, -60), new Vector2(420, 30), TextAnchor.MiddleLeft);
        Text score = MakeText(ui, "Score", "000000", 48, Yellow, topLeft, new Vector2(250, -110), new Vector2(420, 60), TextAnchor.MiddleLeft);
        var hearts = new List<Object>();
        for (int i = 0; i < 3; i++)
            hearts.Add(PixelImage(ui, "Heart " + (i + 1), S("heart"), topRight, new Vector2(-60 - (2 - i) * 84, -90), 8f));
        var hud = ui.gameObject.AddComponent<HUDController>();
        Set(hud, "scoreText", score);
        SetArray(hud, "hearts", hearts);
        Set(hud, "heartFull", S("heart"));
        Set(hud, "heartEmpty", S("heart_empty"));

        return SaveScene("Play");
    }

    private static string BuildGameOver()
    {
        CommonScene("music_gameover", 0.4f);
        SceneLoader loader = MakeSceneLoader();
        Transform ui = MakeCanvas("Canvas", 0);

        MakeText(ui, "Title", "GAME\nOVER", 110, Orange, Center, new Vector2(0, 520), new Vector2(1000, 320));
        MakeText(ui, "Score Label", "YOUR SCORE", 30, Grey, Center, new Vector2(0, 250), new Vector2(1000, 50));
        Text final = MakeText(ui, "Final Score", "000000", 92, Yellow, Center, new Vector2(0, 140), new Vector2(1000, 120));

        MakeButton(ui, "PLAY AGAIN", Center, new Vector2(0, -200), new Vector2(640, 150), loader, SceneLoader.Play);
        MakeButton(ui, "MAIN MENU", Center, new Vector2(0, -390), new Vector2(640, 150), loader, SceneLoader.MainMenu);

        Set(ui.gameObject.AddComponent<GameOverUI>(), "finalScoreText", final);
        return SaveScene("GameOver");
    }
}
