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

[HarmonyPatch(typeof(Backflip), "OnPlay")]
public static class BackflipInjection
{
	private static Tween? _tween;

	public static void Postfix(Backflip __instance)
	{
		onBackFlip();
	}

	public static void onBackFlip()
	{
		NCreature? playerNode = NCombatRoom.Instance?.CreatureNodes.FirstOrDefault(c => LocalContext.IsMe(c.Entity));

		if (playerNode != null)
		{
			NCreatureVisuals playerImage = playerNode.Visuals;

			Node2D body = playerImage.GetCurrentBody();
			Vector2 pivot = playerImage.VfxSpawnPosition.Position;
			Vector2 offset = body.Position - pivot;

			_tween?.Kill();
			_tween = body.CreateTween();
			float jumpHeight = 200f;
			_tween.TweenMethod(Callable.From((float angle) =>
			{
				float t = Mathf.Abs(angle) / Mathf.Tau;
				float arc = -4f * jumpHeight * t * (1f - t);
				body.Position = pivot + offset.Rotated(angle) + new Vector2(0, arc);
				body.Rotation = angle;
			}), 0f, -Mathf.Tau, 0.4f);
			_tween.TweenCallback(Callable.From(() =>
			{
				body.Position = pivot + offset;
				body.Rotation = 0f;
			}));
		}
	}
}
