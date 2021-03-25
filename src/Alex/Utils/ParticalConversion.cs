using MiNET.Entities;
using MiNET.Particles;

namespace Alex.Utils
{
	public class ParticleConversion
	{
		public static string ConvertToBedrock(string java)
		{
			switch (java)
			{
				case "minecraft:smoke":                 return "minecraft:basic_smoke_particle";
				case "minecraft:dust":                  return "minecraft:redstone_wire_dust_particle";
				case "minecraft:ambient_entity_effect": return "minecraft:mobspell_emitter";
				case "minecraft:angry_villager":        return "minecraft:villager_angry";
				case "minecraft:bubble":                return "minecraft:basic_bubble_particle_manual";
				case "minecraft:bubble_column_up":      return "minecraft:bubble_column_up_particle";
				case "minecraft:bubble_pop":            return "minecraft:bubble_column_particle";
				case "minecraft:campfire_cosy_smoke":   return "minecraft:campfire_smoke_particle";
				case "minecraft:campfire_signal_smoke": return "minecraft:campfire_tall_smoke_particle";
				case "minecraft:cloud":                 return "minecraft:water_evaporation_bucket_emitter";
				case "minecraft:composter":             return "minecraft:villager_happy";
				case "minecraft:crit":                  return "minecraft:critical_hit_emitter";
				case "minecraft:current_down":          return "minecraft:bubble_column_down_particle";
				case "minecraft:dolphin":               return "minecraft:dolphin_move_particle";
				case "minecraft:dragon_breath":         return "minecraft:dragon_breath_lingering";
				case "minecraft:dripping_lava":         return "minecraft:lava_drip_particle";
				case "minecraft:dripping_water":        return "minecraft:water_drip_particle";
				case "minecraft:effect":                return "minecraft:splash_spell_emitter";
				case "minecraft:enchant":               return "minecraft:enchanting_table_particle";
				case "minecraft:end_rod":               return "minecraft:endrod";
				case "minecraft:entity_effect":         return "minecraft:evoker_spell";
				case "minecraft:explosion":             return "minecraft:large_explosion";
				case "minecraft:explosion_emitter":     return "minecraft:huge_explosion_emitter";
				case "minecraft:falling_lava":          return "minecraft:lava_drip_particle";
				case "minecraft:falling_water":         return "minecraft:water_splash_particle";
				case "minecraft:firework":              return "minecraft:sparkler_emitter";
				case "minecraft:fishing":               return "minecraft:water_wake_particle";
				case "minecraft:flame":                 return "minecraft:basic_flame_particle";
				case "minecraft:happy_villager":        return "minecraft:villager_happy";
				case "minecraft:heart":                 return "minecraft:heart_particle";
				case "minecraft:instant_effect":        return "minecraft:mobspell_emitter";
				case "minecraft:item":                  return "minecraft:breaking_item_icon";
				case "minecraft:large_smoke":           return "minecraft:water_evaporation_actor_emitter";
				case "minecraft:lava":                  return "minecraft:lava_particle";
				case "minecraft:mycelium":              return "minecraft:mycelium_dust_particle";
				case "minecraft:nautilus":              return "minecraft:nautilus";
				case "minecraft:note":                  return "minecraft:note_particle";
				case "minecraft:poof":                  return "minecraft:explode";
				case "minecraft:portal":                return "minecraft:mob_portal";
				case "minecraft:squid_ink":             return "minecraft:ink";
				case "minecraft:splash":                return "minecraft:water_splash_particle_manual";
				case "minecraft:spit":                  return "minecraft:llama_spit_smoke";
				case "minecraft:totem_of_undying":      return "minecraft:totem_particle";
				case "minecraft:witch":                 return "minecraft:mobspell_emitter";
			}

			return java;
		}
	}
}