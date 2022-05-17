using Celeste;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VivTestMod.Entities
{
    class CustomGroupFloatySpaceBlock : FloatySpaceBlock
    {
        public string customGroup;
        public CustomGroupFloatySpaceBlock(EntityData data, Vector2 offset)
           : base(data, offset)
        { customGroup = data.Attr("CustomGroup", null) }

        public override Load() { On.Celeste.FloatySpaceBlock.Awake += modCustomGroup; }
        public override Unload() { On.Celeste.FloatySpaceBlock.Awake -= modCustomGroup; }

        public void modCustomGroup() {
            foreach (Entity entity in base.Scene.Tracker.GetEntities<FloatySpaceBlock>())
            {
                FloatySpaceBlock floatySpaceBlock = (FloatySpaceBlock)entity;
                if (!floatySpaceBlock.HasGroup && floatySpaceBlock.tileType == this.tileType &&
                    !(base.Scene.CollideCheck(new Rectangle((int)from.X - 1, (int)from.Y, (int)from.Width + 2, (int)from.Height), floatySpaceBlock)
                    || base.Scene.CollideCheck(new Rectangle((int)from.X, (int)from.Y - 1, (int)from.Width, (int)from.Height + 2), floatySpaceBlock)))
                    && !(floatySpaceBlock.Group. = group) &&
                        ((floatySpaceBlock is CustomGroupFloatySpaceBlock) && (CustomGroupFloatySpaceBlock) floatySpaceBlock.customGroup))
  {
                    this.AddToGroupAndFindChildren(floatySpaceBlock);
                }
            }
        }
    }
}
