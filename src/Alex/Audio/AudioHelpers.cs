namespace Alex.Audio
{
	public static class AudioHelpers
	{
		public static bool TryConvertSoundIdToMapping(uint soundId, out string sound)
		{
			switch (soundId)
			{
				case 0:
				{
					sound = "ITEM_USE_ON";

					return true;
				}

				case 1:
				{
					sound = "HIT";

					return true;
				}

				case 2:
				{
					sound = "STEP";

					return true;
				}

				case 3:
				{
					sound = "FLY";

					return true;
				}

				case 4:
				{
					sound = "JUMP";

					return true;
				}

				case 5:
				{
					sound = "BREAK";

					return true;
				}

				case 6:
				{
					sound = "PLACE";

					return true;
				}

				case 7:
				{
					sound = "HEAVY_STEP";

					return true;
				}

				case 8:
				{
					sound = "GALLOP";

					return true;
				}

				case 9:
				{
					sound = "FALL";

					return true;
				}

				case 10:
				{
					sound = "AMBIENT";

					return true;
				}

				case 11:
				{
					sound = "AMBIENT_BABY";

					return true;
				}

				case 12:
				{
					sound = "AMBIENT_IN_WATER";

					return true;
				}

				case 13:
				{
					sound = "BREATHE";

					return true;
				}

				case 14:
				{
					sound = "DEATH";

					return true;
				}

				case 15:
				{
					sound = "DEATH_IN_WATER";

					return true;
				}

				case 16:
				{
					sound = "DEATH_TO_ZOMBIE";

					return true;
				}

				case 17:
				{
					sound = "HURT";

					return true;
				}

				case 18:
				{
					sound = "HURT_IN_WATER";

					return true;
				}

				case 19:
				{
					sound = "MAD";

					return true;
				}

				case 20:
				{
					sound = "BOOST";

					return true;
				}

				case 21:
				{
					sound = "BOW";

					return true;
				}

				case 22:
				{
					sound = "SQUISH_BIG";

					return true;
				}

				case 23:
				{
					sound = "SQUISH_SMALL";

					return true;
				}

				case 24:
				{
					sound = "FALL_BIG";

					return true;
				}

				case 25:
				{
					sound = "FALL_SMALL";

					return true;
				}

				case 26:
				{
					sound = "SPLASH";

					return true;
				}

				case 27:
				{
					sound = "FIZZ";

					return true;
				}

				case 28:
				{
					sound = "FLAP";

					return true;
				}

				case 29:
				{
					sound = "SWIM";

					return true;
				}

				case 30:
				{
					sound = "DRINK";

					return true;
				}

				case 31:
				{
					sound = "EAT";

					return true;
				}

				case 32:
				{
					sound = "TAKEOFF";

					return true;
				}

				case 33:
				{
					sound = "SHAKE";

					return true;
				}

				case 34:
				{
					sound = "PLOP";

					return true;
				}

				case 35:
				{
					sound = "LAND";

					return true;
				}

				case 36:
				{
					sound = "SADDLE";

					return true;
				}

				case 37:
				{
					sound = "ARMOR";

					return true;
				}

				case 38:
				{
					sound = "MOB_ARMOR_STAND_PLACE";

					return true;
				}

				case 39:
				{
					sound = "ADD_CHEST";

					return true;
				}

				case 40:
				{
					sound = "THROW";

					return true;
				}

				case 41:
				{
					sound = "ATTACK";

					return true;
				}

				case 42:
				{
					sound = "ATTACK_NODAMAGE";

					return true;
				}

				case 43:
				{
					sound = "ATTACK_STRONG";

					return true;
				}

				case 44:
				{
					sound = "WARN";

					return true;
				}

				case 45:
				{
					sound = "SHEAR";

					return true;
				}

				case 46:
				{
					sound = "MILK";

					return true;
				}

				case 47:
				{
					sound = "THUNDER";

					return true;
				}

				case 48:
				{
					sound = "EXPLODE";

					return true;
				}

				case 49:
				{
					sound = "FIRE";

					return true;
				}

				case 50:
				{
					sound = "IGNITE";

					return true;
				}

				case 51:
				{
					sound = "FUSE";

					return true;
				}

				case 52:
				{
					sound = "STARE";

					return true;
				}

				case 53:
				{
					sound = "SPAWN";

					return true;
				}

				case 54:
				{
					sound = "SHOOT";

					return true;
				}

				case 55:
				{
					sound = "BREAK_BLOCK";

					return true;
				}

				case 56:
				{
					sound = "LAUNCH";

					return true;
				}

				case 57:
				{
					sound = "BLAST";

					return true;
				}

				case 58:
				{
					sound = "LARGE_BLAST";

					return true;
				}

				case 59:
				{
					sound = "TWINKLE";

					return true;
				}

				case 60:
				{
					sound = "REMEDY";

					return true;
				}

				case 61:
				{
					sound = "UNFECT";

					return true;
				}

				case 62:
				{
					sound = "LEVELUP";

					return true;
				}

				case 63:
				{
					sound = "BOW_HIT";

					return true;
				}

				case 64:
				{
					sound = "BULLET_HIT";

					return true;
				}

				case 65:
				{
					sound = "EXTINGUISH_FIRE";

					return true;
				}

				case 66:
				{
					sound = "ITEM_FIZZ";

					return true;
				}

				case 67:
				{
					sound = "CHEST_OPEN";

					return true;
				}

				case 68:
				{
					sound = "CHEST_CLOSED";

					return true;
				}

				case 69:
				{
					sound = "SHULKERBOX_OPEN";

					return true;
				}

				case 70:
				{
					sound = "SHULKERBOX_CLOSED";

					return true;
				}

				case 71:
				{
					sound = "ENDERCHEST_OPEN";

					return true;
				}

				case 72:
				{
					sound = "ENDERCHEST_CLOSED";

					return true;
				}

				case 73:
				{
					sound = "POWER_ON";

					return true;
				}

				case 74:
				{
					sound = "POWER_OFF";

					return true;
				}

				case 75:
				{
					sound = "ATTACH";

					return true;
				}

				case 76:
				{
					sound = "DETACH";

					return true;
				}

				case 77:
				{
					sound = "DENY";

					return true;
				}

				case 78:
				{
					sound = "TRIPOD";

					return true;
				}

				case 79:
				{
					sound = "POP";

					return true;
				}

				case 80:
				{
					sound = "DROP_SLOT";

					return true;
				}

				case 81:
				{
					sound = "NOTE";

					return true;
				}

				case 82:
				{
					sound = "THORNS";

					return true;
				}

				case 83:
				{
					sound = "PISTON_IN";

					return true;
				}

				case 84:
				{
					sound = "PISTON_OUT";

					return true;
				}

				case 85:
				{
					sound = "PORTAL";

					return true;
				}

				case 86:
				{
					sound = "WATER";

					return true;
				}

				case 87:
				{
					sound = "LAVA_POP";

					return true;
				}

				case 88:
				{
					sound = "LAVA";

					return true;
				}

				case 89:
				{
					sound = "BURP";

					return true;
				}

				case 90:
				{
					sound = "BUCKET_FILL_WATER";

					return true;
				}

				case 91:
				{
					sound = "BUCKET_FILL_LAVA";

					return true;
				}

				case 92:
				{
					sound = "BUCKET_EMPTY_WATER";

					return true;
				}

				case 93:
				{
					sound = "BUCKET_EMPTY_LAVA";

					return true;
				}

				case 94:
				{
					sound = "ARMOR_EQUIP_CHAIN";

					return true;
				}

				case 95:
				{
					sound = "ARMOR_EQUIP_DIAMOND";

					return true;
				}

				case 96:
				{
					sound = "ARMOR_EQUIP_GENERIC";

					return true;
				}

				case 97:
				{
					sound = "ARMOR_EQUIP_GOLD";

					return true;
				}

				case 98:
				{
					sound = "ARMOR_EQUIP_IRON";

					return true;
				}

				case 99:
				{
					sound = "ARMOR_EQUIP_LEATHER";

					return true;
				}

				case 100:
				{
					sound = "ARMOR_EQUIP_ELYTRA";

					return true;
				}

				case 101:
				{
					sound = "RECORD_";

					return true;
				}

				case 102:
				{
					sound = "RECORD_CAT";

					return true;
				}

				case 103:
				{
					sound = "RECORD_BLOCKS";

					return true;
				}

				case 104:
				{
					sound = "RECORD_CHIRP";

					return true;
				}

				case 105:
				{
					sound = "RECORD_FAR";

					return true;
				}

				case 106:
				{
					sound = "RECORD_MALL";

					return true;
				}

				case 107:
				{
					sound = "RECORD_MELLOHI";

					return true;
				}

				case 108:
				{
					sound = "RECORD_STAL";

					return true;
				}

				case 109:
				{
					sound = "RECORD_STRAD";

					return true;
				}

				case 110:
				{
					sound = "RECORD_WARD";

					return true;
				}

				case 111:
				{
					sound = "RECORD_";

					return true;
				}

				case 112:
				{
					sound = "RECORD_WAIT";

					return true;
				}

				case 113:
				{
					sound = "STOP_RECORD";

					return true;
				}

				case 114:
				{
					sound = "FLOP";

					return true;
				}

				case 115:
				{
					sound = "ELDERGUARDIAN_CURSE";

					return true;
				}

				case 116:
				{
					sound = "MOB_WARNING";

					return true;
				}

				case 117:
				{
					sound = "MOB_WARNING_BABY";

					return true;
				}

				case 118:
				{
					sound = "TELEPORT";

					return true;
				}

				case 119:
				{
					sound = "SHULKER_OPEN";

					return true;
				}

				case 120:
				{
					sound = "SHULKER_CLOSE";

					return true;
				}

				case 121:
				{
					sound = "HAGGLE";

					return true;
				}

				case 122:
				{
					sound = "HAGGLE_YES";

					return true;
				}

				case 123:
				{
					sound = "HAGGLE_NO";

					return true;
				}

				case 124:
				{
					sound = "HAGGLE_IDLE";

					return true;
				}

				case 125:
				{
					sound = "CHORUSGROW";

					return true;
				}

				case 126:
				{
					sound = "CHORUSDEATH";

					return true;
				}

				case 127:
				{
					sound = "GLASS";

					return true;
				}

				case 128:
				{
					sound = "POTION_BREWED";

					return true;
				}

				case 129:
				{
					sound = "CAST_SPELL";

					return true;
				}

				case 130:
				{
					sound = "PREPARE_ATTACK";

					return true;
				}

				case 131:
				{
					sound = "PREPARE_SUMMON";

					return true;
				}

				case 132:
				{
					sound = "PREPARE_WOLOLO";

					return true;
				}

				case 133:
				{
					sound = "FANG";

					return true;
				}

				case 134:
				{
					sound = "CHARGE";

					return true;
				}

				case 135:
				{
					sound = "CAMERA_TAKE_PICTURE";

					return true;
				}

				case 136:
				{
					sound = "LEASHKNOT_PLACE";

					return true;
				}

				case 137:
				{
					sound = "LEASHKNOT_BREAK";

					return true;
				}

				case 138:
				{
					sound = "GROWL";

					return true;
				}

				case 139:
				{
					sound = "WHINE";

					return true;
				}

				case 140:
				{
					sound = "PANT";

					return true;
				}

				case 141:
				{
					sound = "PURR";

					return true;
				}

				case 142:
				{
					sound = "PURREOW";

					return true;
				}

				case 143:
				{
					sound = "DEATH_MIN_VOLUME";

					return true;
				}

				case 144:
				{
					sound = "DEATH_MID_VOLUME";

					return true;
				}

				case 145:
				{
					sound = "IMITATE_BLAZE";

					return true;
				}

				case 146:
				{
					sound = "IMITATE_CAVE_SPIDER";

					return true;
				}

				case 147:
				{
					sound = "IMITATE_CREEPER";

					return true;
				}

				case 148:
				{
					sound = "IMITATE_ELDER_GUARDIAN";

					return true;
				}

				case 149:
				{
					sound = "IMITATE_ENDER_DRAGON";

					return true;
				}

				case 150:
				{
					sound = "IMITATE_ENDERMAN";

					return true;
				}

				case 152:
				{
					sound = "IMITATE_EVOCATION_ILLAGER";

					return true;
				}

				case 153:
				{
					sound = "IMITATE_GHAST";

					return true;
				}

				case 154:
				{
					sound = "IMITATE_HUSK";

					return true;
				}

				case 155:
				{
					sound = "IMITATE_ILLUSION_ILLAGER";

					return true;
				}

				case 156:
				{
					sound = "IMITATE_MAGMA_CUBE";

					return true;
				}

				case 157:
				{
					sound = "IMITATE_POLAR_BEAR";

					return true;
				}

				case 158:
				{
					sound = "IMITATE_SHULKER";

					return true;
				}

				case 159:
				{
					sound = "IMITATE_SILVERFISH";

					return true;
				}

				case 160:
				{
					sound = "IMITATE_SKELETON";

					return true;
				}

				case 161:
				{
					sound = "IMITATE_SLIME";

					return true;
				}

				case 162:
				{
					sound = "IMITATE_SPIDER";

					return true;
				}

				case 163:
				{
					sound = "IMITATE_STRAY";

					return true;
				}

				case 164:
				{
					sound = "IMITATE_VEX";

					return true;
				}

				case 165:
				{
					sound = "IMITATE_VINDICATION_ILLAGER";

					return true;
				}

				case 166:
				{
					sound = "IMITATE_WITCH";

					return true;
				}

				case 167:
				{
					sound = "IMITATE_WITHER";

					return true;
				}

				case 168:
				{
					sound = "IMITATE_WITHER_SKELETON";

					return true;
				}

				case 169:
				{
					sound = "IMITATE_WOLF";

					return true;
				}

				case 170:
				{
					sound = "IMITATE_ZOMBIE";

					return true;
				}

				case 171:
				{
					sound = "IMITATE_ZOMBIE_PIGMAN";

					return true;
				}

				case 172:
				{
					sound = "IMITATE_ZOMBIE_VILLAGER";

					return true;
				}

				case 173:
				{
					sound = "BLOCK_END_PORTAL_FRAME_FILL";

					return true;
				}

				case 174:
				{
					sound = "BLOCK_END_PORTAL_SPAWN";

					return true;
				}

				case 175:
				{
					sound = "RANDOM_ANVIL_USE";

					return true;
				}

				case 176:
				{
					sound = "BOTTLE_DRAGONBREATH";

					return true;
				}

				case 177:
				{
					sound = "PORTAL_TRAVEL";

					return true;
				}

				case 178:
				{
					sound = "ITEM_TRIDENT_HIT";

					return true;
				}

				case 179:
				{
					sound = "ITEM_TRIDENT_RETURN";

					return true;
				}

				case 180:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 181:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 182:
				{
					sound = "ITEM_TRIDENT_RIPTIDE_";

					return true;
				}

				case 183:
				{
					sound = "ITEM_TRIDENT_THROW";

					return true;
				}

				case 184:
				{
					sound = "ITEM_TRIDENT_THUNDER";

					return true;
				}

				case 185:
				{
					sound = "ITEM_TRIDENT_HIT_GROUND";

					return true;
				}

				case 186:
				{
					sound = "DEFAULT";

					return true;
				}

				case 187:
				{
					sound = "BLOCK_FLETCHING_TABLE_USE";

					return true;
				}

				case 188:
				{
					sound = "ELEMCONSTRUCT_OPEN";

					return true;
				}

				case 189:
				{
					sound = "ICEBOMB_HIT";

					return true;
				}

				case 190:
				{
					sound = "BALLOONPOP";

					return true;
				}

				case 191:
				{
					sound = "LT_REACTION_ICEBOMB";

					return true;
				}

				case 192:
				{
					sound = "LT_REACTION_BLEACH";

					return true;
				}

				case 193:
				{
					sound = "LT_REACTION_EPASTE";

					return true;
				}

				case 194:
				{
					sound = "LT_REACTION_EPASTE";

					return true;
				}

				case 199:
				{
					sound = "LT_REACTION_FERTILIZER";

					return true;
				}

				case 200:
				{
					sound = "LT_REACTION_FIREBALL";

					return true;
				}

				case 201:
				{
					sound = "LT_REACTION_MGSALT";

					return true;
				}

				case 202:
				{
					sound = "LT_REACTION_MISCFIRE";

					return true;
				}

				case 203:
				{
					sound = "LT_REACTION_FIRE";

					return true;
				}

				case 204:
				{
					sound = "LT_REACTION_MISCEXPLOSION";

					return true;
				}

				case 205:
				{
					sound = "LT_REACTION_MISCMYSTICAL";

					return true;
				}

				case 206:
				{
					sound = "LT_REACTION_MISCMYSTICAL";

					return true;
				}

				case 207:
				{
					sound = "LT_REACTION_PRODUCT";

					return true;
				}

				case 208:
				{
					sound = "SPARKLER_USE";

					return true;
				}

				case 209:
				{
					sound = "GLOWSTICK_USE";

					return true;
				}

				case 210:
				{
					sound = "SPARKLER_ACTIVE";

					return true;
				}

				case 211:
				{
					sound = "CONVERT_TO_DROWNED";

					return true;
				}

				case 212:
				{
					sound = "BUCKET_FILL_FISH";

					return true;
				}

				case 213:
				{
					sound = "BUCKET_EMPTY_FISH";

					return true;
				}

				case 214:
				{
					sound = "BUBBLE_UP";

					return true;
				}

				case 215:
				{
					sound = "BUBBLE_DOWN";

					return true;
				}

				case 216:
				{
					sound = "BUBBLE_POP";

					return true;
				}

				case 217:
				{
					sound = "BUBBLE_UPINSIDE";

					return true;
				}

				case 218:
				{
					sound = "BUBBLE_DOWNINSIDE";

					return true;
				}

				case 219:
				{
					sound = "HURT_BABY";

					return true;
				}

				case 220:
				{
					sound = "DEATH_BABY";

					return true;
				}

				case 221:
				{
					sound = "STEP_BABY";

					return true;
				}

				case 223:
				{
					sound = "BORN";

					return true;
				}

				case 224:
				{
					sound = "BLOCK_TURTLE_EGG_BREAK";

					return true;
				}

				case 225:
				{
					sound = "BLOCK_TURTLE_EGG_CRACK";

					return true;
				}

				case 226:
				{
					sound = "BLOCK_TURTLE_EGG_HATCH";

					return true;
				}

				case 227:
				{
					sound = "LAY_EGG";

					return true;
				}

				case 228:
				{
					sound = "BLOCK_TURTLE_EGG_ATTACK";

					return true;
				}

				case 229:
				{
					sound = "BEACON_ACTIVATE";

					return true;
				}

				case 230:
				{
					sound = "BEACON_AMBIENT";

					return true;
				}

				case 231:
				{
					sound = "BEACON_DEACTIVATE";

					return true;
				}

				case 232:
				{
					sound = "BEACON_POWER";

					return true;
				}

				case 233:
				{
					sound = "CONDUIT_ACTIVATE";

					return true;
				}

				case 234:
				{
					sound = "CONDUIT_AMBIENT";

					return true;
				}

				case 235:
				{
					sound = "CONDUIT_ATTACK";

					return true;
				}

				case 236:
				{
					sound = "CONDUIT_DEACTIVATE";

					return true;
				}

				case 237:
				{
					sound = "CONDUIT_SHORT";

					return true;
				}

				case 238:
				{
					sound = "SWOOP";

					return true;
				}

				case 239:
				{
					sound = "BLOCK_BAMBOO_SAPLING_PLACE";

					return true;
				}

				case 240:
				{
					sound = "PRESNEEZE";

					return true;
				}

				case 241:
				{
					sound = "SNEEZE";

					return true;
				}

				case 242:
				{
					sound = "AMBIENT_TAME";

					return true;
				}

				case 243:
				{
					sound = "SCARED";

					return true;
				}

				case 244:
				{
					sound = "BLOCK_SCAFFOLDING_CLIMB";

					return true;
				}

				case 245:
				{
					sound = "CROSSBOW_LOADING_START";

					return true;
				}

				case 246:
				{
					sound = "CROSSBOW_LOADING_MIDDLE";

					return true;
				}

				case 247:
				{
					sound = "CROSSBOW_LOADING_END";

					return true;
				}

				case 248:
				{
					sound = "CROSSBOW_SHOOT";

					return true;
				}

				case 249:
				{
					sound = "CROSSBOW_QUICK_CHARGE_START";

					return true;
				}

				case 250:
				{
					sound = "CROSSBOW_QUICK_CHARGE_MIDDLE";

					return true;
				}

				case 251:
				{
					sound = "CROSSBOW_QUICK_CHARGE_END";

					return true;
				}

				case 252:
				{
					sound = "AMBIENT_AGGRESSIVE";

					return true;
				}

				case 253:
				{
					sound = "AMBIENT_WORRIED";

					return true;
				}

				case 254:
				{
					sound = "CANT_BREED";

					return true;
				}

				case 255:
				{
					sound = "ITEM_SHIELD_BLOCK";

					return true;
				}

				case 256:
				{
					sound = "ITEM_BOOK_PUT";

					return true;
				}

				case 257:
				{
					sound = "BLOCK_GRINDSTONE_USE";

					return true;
				}

				case 258:
				{
					sound = "BLOCK_BELL_HIT";

					return true;
				}

				case 259:
				{
					sound = "BLOCK_CAMPFIRE_CRACKLE";

					return true;
				}

				case 260:
				{
					sound = "ROAR";

					return true;
				}

				case 261:
				{
					sound = "STUN";

					return true;
				}

				case 262:
				{
					sound = "BLOCK_SWEET_BERRY_BUSH_HURT";

					return true;
				}

				case 263:
				{
					sound = "BLOCK_SWEET_BERRY_BUSH_PICK";

					return true;
				}

				case 264:
				{
					sound = "BLOCK_CARTOGRAPHY_TABLE_USE";

					return true;
				}

				case 265:
				{
					sound = "BLOCK_STONECUTTER_USE";

					return true;
				}

				case 266:
				{
					sound = "BLOCK_COMPOSTER_EMPTY";

					return true;
				}

				case 267:
				{
					sound = "BLOCK_COMPOSTER_FILL";

					return true;
				}

				case 268:
				{
					sound = "BLOCK_COMPOSTER_FILL_SUCCESS";

					return true;
				}

				case 269:
				{
					sound = "BLOCK_COMPOSTER_READY";

					return true;
				}

				case 270:
				{
					sound = "BLOCK_BARREL_OPEN";

					return true;
				}

				case 271:
				{
					sound = "BLOCK_BARREL_CLOSE";

					return true;
				}

				case 272:
				{
					sound = "RAID_HORN";

					return true;
				}

				case 273:
				{
					sound = "BLOCK_LOOM_USE";

					return true;
				}

				case 274:
				{
					sound = "AMBIENT_IN_RAID";

					return true;
				}

				case 275:
				{
					sound = "UI_CARTOGRAPHY_TABLE_TAKE_RESULT";

					return true;
				}

				case 276:
				{
					sound = "UI_STONECUTTER_TAKE_RESULT";

					return true;
				}

				case 277:
				{
					sound = "UI_LOOM_TAKE_RESULT";

					return true;
				}

				case 278:
				{
					sound = "BLOCK_SMOKER_SMOKE";

					return true;
				}

				case 279:
				{
					sound = "BLOCK_BLASTFURNACE_FIRE_CRACKLE";

					return true;
				}

				case 280:
				{
					sound = "BLOCK_SMITHING_TABLE_USE";

					return true;
				}

				case 281:
				{
					sound = "SCREECH";

					return true;
				}

				case 282:
				{
					sound = "SLEEP";

					return true;
				}

				case 283:
				{
					sound = "BLOCK_FURNACE_LIT";

					return true;
				}

				case 284:
				{
					sound = "CONVERT_MOOSHROOM";

					return true;
				}

				case 285:
				{
					sound = "MILK_SUSPICIOUSLY";

					return true;
				}

				case 286:
				{
					sound = "CELEBRATE";

					return true;
				}

				case 287:
				{
					sound = "JUMP_PREVENT";

					return true;
				}

				case 288:
				{
					sound = "AMBIENT_POLLINATE";

					return true;
				}

				case 289:
				{
					sound = "BLOCK_BEEHIVE_DRIP";

					return true;
				}

				case 290:
				{
					sound = "BLOCK_BEEHIVE_ENTER";

					return true;
				}

				case 291:
				{
					sound = "BLOCK_BEEHIVE_EXIT";

					return true;
				}

				case 292:
				{
					sound = "BLOCK_BEEHIVE_WORK";

					return true;
				}

				case 293:
				{
					sound = "BLOCK_BEEHIVE_SHEAR";

					return true;
				}

				case 294:
				{
					sound = "DRINK_HONEY";

					return true;
				}

				case 295:
				{
					sound = "AMBIENT_CAVE";

					return true;
				}

				case 296:
				{
					sound = "RETREAT";

					return true;
				}

				case 297:
				{
					sound = "CONVERTED_TO_ZOMBIFIED";

					return true;
				}

				case 298:
				{
					sound = "ADMIRE";

					return true;
				}

				case 299:
				{
					sound = "STEP_LAVA";

					return true;
				}

				case 300:
				{
					sound = "TEMPT";

					return true;
				}

				case 301:
				{
					sound = "PANIC";

					return true;
				}

				case 302:
				{
					sound = "ANGRY";

					return true;
				}

				case 303:
				{
					sound = "AMBIENT_WARPED_FOREST_MOOD";

					return true;
				}

				case 304:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_MOOD";

					return true;
				}

				case 305:
				{
					sound = "AMBIENT_NETHER_WASTES_MOOD";

					return true;
				}

				case 306:
				{
					sound = "RESPAWN_ANCHOR_BASALT_DELTAS_MOOD";

					return true;
				}

				case 307:
				{
					sound = "AMBIENT_CRIMSON_FOREST_MOOD";

					return true;
				}

				case 308:
				{
					sound = "RESPAWN_ANCHOR_CHARGE";

					return true;
				}

				case 309:
				{
					sound = "RESPAWN_ANCHOR_DEPLETE";

					return true;
				}

				case 310:
				{
					sound = "RESPAWN_ANCHOR_SET_SPAWN";

					return true;
				}

				case 311:
				{
					sound = "RESPAWN_ANCHOR_AMBIENT";

					return true;
				}

				case 312:
				{
					sound = "PARTICLE_SOUL_ESCAPE_QUIET";

					return true;
				}

				case 313:
				{
					sound = "PARTICLE_SOUL_ESCAPE_LOUD";

					return true;
				}

				case 314:
				{
					sound = "RECORD_PIGSTEP";

					return true;
				}

				case 315:
				{
					sound = "LODESTONE_COMPASS_LINK_COMPASS_TO_LODESTONE";

					return true;
				}

				case 316:
				{
					sound = "SMITHING_TABLE_USE";

					return true;
				}

				case 317:
				{
					sound = "ARMOR_EQUIP_NETHERITE";

					return true;
				}

				case 318:
				{
					sound = "AMBIENT_WARPED_FOREST_LOOP";

					return true;
				}

				case 319:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_LOOP";

					return true;
				}

				case 320:
				{
					sound = "AMBIENT_NETHER_WASTES_LOOP";

					return true;
				}

				case 321:
				{
					sound = "AMBIENT_BASALT_DELTAS_LOOP";

					return true;
				}

				case 322:
				{
					sound = "AMBIENT_CRIMSON_FOREST_LOOP";

					return true;
				}

				case 323:
				{
					sound = "AMBIENT_WARPED_FOREST_ADDITIONS";

					return true;
				}

				case 324:
				{
					sound = "AMBIENT_SOULSAND_VALLEY_ADDITIONS";

					return true;
				}

				case 325:
				{
					sound = "AMBIENT_NETHER_WASTES_ADDITIONS";

					return true;
				}

				case 326:
				{
					sound = "AMBIENT_BASALT_DELTAS_ADDITIONS";

					return true;
				}

				case 327:
				{
					sound = "AMBIENT_CRIMSON_FOREST_ADDITIONS";

					return true;
				}

				case 328:
				{
					sound = "BUCKET_FILL_POWDER_SNOW";

					return true;
				}

				case 329:
				{
					sound = "BUCKET_EMPTY_POWDER_SNOW";

					return true;
				}

				case 330:
				{
					sound = "UNDEFINED";

					return true;
				}
			}

			sound = null;
			return false;
		}
	}
}