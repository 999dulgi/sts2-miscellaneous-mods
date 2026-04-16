using Godot;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace GrandFinaleEffect;

[ModInitializer("ModInit")]
public static class GrandFinaleInjection
{
    public static void ModInit()
    {
        GrandFinaleOnPlayPatch.Initialize();

        var harmony = new Harmony("grand-finale-effect");
        harmony.PatchAll();
    }
}

[HarmonyPatch(typeof(GrandFinale), "OnPlay")]
public static class GrandFinaleOnPlayPatch
{
    private static Node2D? _particleInstance;
    private static bool _skipPatch = false;

    public static void Initialize() { }

    public static bool Prefix(GrandFinale __instance, PlayerChoiceContext choiceContext, CardPlay cardPlay, ref Task __result)
    {
        if (_skipPatch) return true;

        SpawnParticles();
        __result = DelayedPlay(__instance, choiceContext, cardPlay);
        return false;
    }

    private static async Task DelayedPlay(GrandFinale instance, PlayerChoiceContext ctx, CardPlay play)
    {
        await Task.Delay(1000);

        _skipPatch = true;
        try
        {
            var method = AccessTools.Method(typeof(GrandFinale), "OnPlay");
            if (method.Invoke(instance, [ctx, play]) is Task task)
                await task;
        }
        finally
        {
            _skipPatch = false;
        }

        StopParticles();
        await ThrowBouquetBox();
    }

    private static void SpawnParticles()
    {
        if (Engine.GetMainLoop() is not SceneTree sceneTree) return;

        var scene = GD.Load<PackedScene>("res://grand-finale-effect/petal_particles.tscn");
        if (scene == null) return;

        _particleInstance = scene.Instantiate<Node2D>();
        sceneTree.Root.AddChild(_particleInstance);
    }

    private static void StopParticles()
    {
        if (_particleInstance == null || !_particleInstance.IsInsideTree()) return;

        var particles = _particleInstance.GetNode<GpuParticles2D>("PetalParticles");
        particles.OneShot = true;
        particles.Finished += _particleInstance.QueueFree;
    }

    private static async Task ThrowBouquetBox()
    {
        if (Engine.GetMainLoop() is not SceneTree sceneTree) return;

        var scene = GD.Load<PackedScene>("res://grand-finale-effect/bouquet_box.tscn");
        if (scene == null)
        {
            GD.PrintErr("[GrandFinale] bouquet_box.tscn 로드 실패");
            return;
        }

        var playerNode = NCombatRoom.Instance?.CreatureNodes
            .FirstOrDefault(c => c.Entity.IsPlayer);
        var baseTarget = playerNode?.GlobalPosition ?? new Vector2(400, 800);

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        ThrowSingleBox(scene, sceneTree, baseTarget, rng);

        // 0.2~0.5초 랜덤 딜레이 후 두 번째 박스
        var delay = sceneTree.CreateTimer(rng.RandfRange(0.2f, 0.5f));
        await delay.ToSignal(delay, SceneTreeTimer.SignalName.Timeout);

        ThrowSingleBox(scene, sceneTree, baseTarget, rng);
    }

    private static readonly string[] _normalImages =
    [
        "res://grand-finale-effect/images/bing_bong.png",
        "res://grand-finale-effect/images/daughter_of_the_wind.png",
        "res://grand-finale-effect/images/happy_flower.png",
        "res://grand-finale-effect/images/mr_struggles.png",
    ];
    private const string _rareImage = "res://grand-finale-effect/images/fake_happy_flower.png";

    private static void ThrowSingleBox(PackedScene scene, SceneTree sceneTree, Vector2 baseTarget, RandomNumberGenerator rng)
    {
        var box = scene.Instantiate<Node2D>();
        sceneTree.Root.AddChild(box);

        var imagePath = rng.Randf() < 0.01f
            ? _rareImage
            : _normalImages[rng.RandiRange(0, _normalImages.Length - 1)];
        var sprite = box.GetNode<Sprite2D>("Visual");
        sprite.Texture = GD.Load<Texture2D>(imagePath);

        // 목표 위치에 랜덤 오프셋
        var target = baseTarget + new Vector2(
            rng.RandfRange(-80f, 80f),
            rng.RandfRange(-60f, 60f)
        );

        // 랜덤 시작 위치 (화면 왼쪽 위에서 시작)
        box.Position = new Vector2(rng.RandfRange(-150f, -50f), target.Y - rng.RandfRange(300f, 500f));

        float duration = rng.RandfRange(0.5f, 0.8f);
        float rotations = rng.RandfRange(1.5f, 3.0f);
        float direction = rng.Randf() > 0.5f ? 1f : -1f;

        var tween = box.CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(box, "position:x", target.X, duration)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(box, "position:y", target.Y, duration)
            .SetTrans(Tween.TransitionType.Bounce).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(box, "rotation", direction * rotations * Mathf.Tau, duration)
            .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);

        // 도착 후 1초 대기, 사라짐
        async void WaitAndFree()
        {
            var t1 = sceneTree.CreateTimer(duration);
            await t1.ToSignal(t1, SceneTreeTimer.SignalName.Timeout);
            var t2 = sceneTree.CreateTimer(1.0f);
            await t2.ToSignal(t2, SceneTreeTimer.SignalName.Timeout);
            if (!GodotObject.IsInstanceValid(box)) return;

            var fadeTween = box.CreateTween();
            fadeTween.TweenProperty(box, "modulate:a", 0f, 0.4f)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);

            var t3 = sceneTree.CreateTimer(0.4f);
            await t3.ToSignal(t3, SceneTreeTimer.SignalName.Timeout);
            if (GodotObject.IsInstanceValid(box)) box.QueueFree();
        }
        WaitAndFree();
    }
}
