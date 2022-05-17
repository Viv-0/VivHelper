using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Celeste;
using Microsoft.Xna.Framework;
using Monocle;
using Celeste.Mod.Entities;

namespace VivHelper.Entities
{
	
	[CustomEntity("VivHelper/CornerBoostDreamBlock")]
	public class CornerBoostDreamBlock : DreamBlock
	{
		public CornerBoostDreamBlock(EntityData data, Vector2 offset) : base(data, offset) { }
	}
}
