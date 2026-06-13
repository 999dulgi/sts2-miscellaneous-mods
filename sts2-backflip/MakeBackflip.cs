#nullable enable
using System.Linq;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

[ModInitializer("ModInit")]
public static class ModStart
{
	public static void ModInit()
	{
		Harmony harmony = new Harmony("sts2-backflip");
		harmony.PatchAll();
	}
}

public static class CardFlipHelper
{
	private static Tween? _tween;
	private static Vector2 _restOffset;

	public static void DoFlip(float targetAngle, float jumpHeight)
	{
		NCreature? playerNode = NCombatRoom.Instance?.CreatureNodes.FirstOrDefault(c => LocalContext.IsMe(c.Entity));
		if (playerNode == null)
			return;

		NCreatureVisuals playerImage = playerNode.Visuals;
		Node2D body = playerNode.Body;
		Vector2 pivot = playerImage.VfxSpawnPosition.Position;

		if (_tween == null || !_tween.IsRunning())
			_restOffset = body.Position - pivot;

		Vector2 offset = _restOffset;

		_tween?.Kill();
		body.Position = pivot + offset;
		body.Rotation = 0f;
		_tween = body.CreateTween();

		_tween.TweenMethod(Callable.From((float angle) =>
		{
			float t = Mathf.Abs(angle) / Mathf.Tau;
			float arc = -4f * jumpHeight * t * (1f - t);
			body.Position = pivot + offset.Rotated(angle) + new Vector2(0, arc);
			body.Rotation = angle;
		}), 0f, targetAngle, 0.4f);
		_tween.TweenCallback(Callable.From(() =>
		{
			body.Position = pivot + offset;
			body.Rotation = 0f;
		}));
	}
}

[HarmonyPatch(typeof(Backflip), "OnPlay")]
public static class BackflipInjection
{
	public static void Postfix(Backflip __instance)
	{
		if (!LocalContext.IsMe(__instance.Owner))
			return;
		CardFlipHelper.DoFlip(-Mathf.Tau, 200f);
	}
}

[HarmonyPatch(typeof(Acrobatics), "OnPlay")]
public static class AcrobaticsInjection
{
	public static void Postfix(Acrobatics __instance)
	{
		if (!LocalContext.IsMe(__instance.Owner))
			return;
		CardFlipHelper.DoFlip(Mathf.Tau, 200f);
	}
}

[HarmonyPatch(typeof(DodgeAndRoll), "OnPlay")]
public static class DodgeAndRollInjection
{
	public static void Postfix(DodgeAndRoll __instance)
	{
		if (!LocalContext.IsMe(__instance.Owner))
			return;
		CardFlipHelper.DoFlip(Mathf.Tau, 0f);
	}
}
