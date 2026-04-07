#nullable enable
using System.Linq;
using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;


[ModInitializer("ModInit")]
public static class ModStart
{
	public static void ModInit()
	{
		Harmony harmony = new Harmony("gigantification");
		harmony.PatchAll();
	}
}


public static class GigantificationState
{
	public static Node2D? GigantifiedBody;
	public static Vector2 OriginalScale;
	public static Tween? ActiveTween;
}

[HarmonyPatch(typeof(Creature), "ApplyPowerInternal")]
public static class MakeGigantic
{
	public static void Postfix(Creature __instance, PowerModel power)
	{
		if (power is not GigantificationPower) return;
		if (!LocalContext.IsMe(__instance)) return;
		if (GigantificationState.GigantifiedBody != null) return; // 이미 적용됨

		NCreature? playerNode = NCombatRoom.Instance?.CreatureNodes.FirstOrDefault(c => LocalContext.IsMe(c.Entity));
		if (playerNode == null) return;

		GigantificationState.GigantifiedBody = playerNode.Body;
		GigantificationState.OriginalScale = playerNode.Body.Scale;

		GigantificationState.ActiveTween?.Kill();
		GigantificationState.ActiveTween = playerNode.Body.CreateTween();
		GigantificationState.ActiveTween.TweenProperty(playerNode.Body, "scale", GigantificationState.OriginalScale * 3, 0.4f)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
	}
}

[HarmonyPatch(typeof(Creature), "RemovePowerInternal")]
public static class MakeSmall
{
	public static void Postfix(PowerModel power)
	{
		if (power is not GigantificationPower) return;
		if (GigantificationState.GigantifiedBody == null) return;

		GigantificationState.ActiveTween?.Kill();
		GigantificationState.ActiveTween = GigantificationState.GigantifiedBody.CreateTween();
		GigantificationState.ActiveTween.TweenProperty(GigantificationState.GigantifiedBody, "scale", GigantificationState.OriginalScale, 0.3f)
			.SetTrans(Tween.TransitionType.Quad)
			.SetEase(Tween.EaseType.In);
		GigantificationState.GigantifiedBody = null;
	}
}
