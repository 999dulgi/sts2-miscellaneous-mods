using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

[ModInitializer("ModLoaded")]
public class ModInit
{
	public static void ModLoaded()
	{
		new Harmony("makeblurblur").PatchAll();
	}
}

public static class BlurShader
{
	public static readonly ShaderMaterial Material = new()
	{
		Shader = new Shader()
		{
			Code = """
			shader_type canvas_item;
			render_mode unshaded;

			uniform float blur_amount: hint_range(0.0, 10.0) = 1.5;

			void fragment() {
				vec4 color = vec4(0.0);
				float total = 0.0;
				vec2 ps = TEXTURE_PIXEL_SIZE * blur_amount;
				for (int x = -3; x <= 3; x++) {
					for (int y = -3; y <= 3; y++) {
						float fx = float(x);
						float fy = float(y);
						float w = exp(-(fx * fx + fy * fy) / 8.0);
						color += texture(TEXTURE, UV + vec2(fx, fy) * ps) * w;
						total += w;
					}
				}
				COLOR = color / total;
			}
			"""
		}
	};
}

[HarmonyPatch(
	"MegaCrit.Sts2.Core.Nodes.Cards.Holders.NCardHolder",
	"SetCard")]
public static class NCardHolder_SetCard_Patch
{
	public static void Postfix(object __instance, CanvasItem node)
	{
		if (__instance is not Control holder) return;

		var model = node.GetType().GetProperty("Model")?.GetValue(node);
		if (model?.GetType().Name != "Blur") return;

		Control hitbox = null;
		foreach (var child in holder.GetChildren())
			if (child.GetType().Name == "NCardHolderHitbox" && child is Control c) { hitbox = c; break; }
		if (hitbox == null) return;

		var viewport = new SubViewport()
		{
			TransparentBg = true,
			RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
		};

		var container = new SubViewportContainer()
		{
			Stretch = true,
			MouseFilter = Control.MouseFilterEnum.Ignore,
			Material = BlurShader.Material,
		};

		holder.AddChild(container);
		container.AddChild(viewport);
		node.Reparent(viewport, false);

		const float padding = 40f;
		void SyncSize()
		{
			var padded = hitbox.Size + Vector2.One * padding * 2f;
			viewport.Size = new Vector2I((int)padded.X, (int)padded.Y);
			container.Size = padded;
			container.Position = hitbox.Position - Vector2.One * padding;
			viewport.CanvasTransform = new Transform2D(0, padded / 2f);
		}
		Callable.From(SyncSize).CallDeferred();
		hitbox.Resized += SyncSize;
	}
}
