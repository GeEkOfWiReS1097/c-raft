﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCactus : BlockBase, IBlockGrowable
    {
        public BlockCactus()
        {
            Name = "Cactus";
            Type = BlockData.Blocks.Cactus;
            IsSolid = true;
            Opacity = 0x0;
            LootTable.Add(new ItemStack((short)Type, 1));
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            // Cactus can only be placed on the top of the sand block or on the top of the other cactus
            if ((targetBlock.Type != (byte)BlockData.Blocks.Sand && targetBlock.Type != (byte)BlockData.Blocks.Cactus) || targetSide != BlockFace.Up)
                return false;
            // Can be placed only if North/West/East/South is clear
            bool isAir = true;
            block.Chunk.ForNSEW(block.Coords,
                delegate(UniversalCoords uc)
                {
                    if (block.World.GetBlockId(uc) != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });
            if (!isAir)
                return false;

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        public void Grow(StructBlock block)
        {
            // BlockMeta = 0x0 is a freshly planted cactus.
            // The data value is incremented at random intervals.
            // When it becomes 15, a new cactus block is created on top as long as the total height does not exceed 3.
            int maxHeight = 3;

            if (block.Coords.WorldY == 127)
                return;
            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            byte blockId = block.World.GetBlockId(oneUp);
            if (blockId != (byte)BlockData.Blocks.Air || blockId == (byte)BlockData.Blocks.Cactus)
                return;
            if (block.Coords.WorldY > maxHeight - 1)
                if (block.World.GetBlockId(UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY - maxHeight, block.Coords.WorldZ)) == (byte)BlockData.Blocks.Cactus)
                    return;

            bool isAir = true;
            block.Chunk.ForNSEW(oneUp,
                delegate(UniversalCoords uc)
                {
                    if (block.World.GetBlockId(uc) != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });
            if (!isAir)
                return;

            bool willGrow = (block.World.Server.Rand.Next(60) == 0);
            if (block.MetaData < 0xe) // 14
            {
                if (willGrow)
                {
                    Console.WriteLine("Growing cactus..");
                    block.Chunk.SetData(block.Coords, block.MetaData++, false);
                }
                return;
            }
            else
            {
                if (!willGrow)
                    return;
                Console.WriteLine("Creating new cactus");
                block.World.SetBlockData(block.Coords, 0);
                block.World.SetBlockAndData(oneUp, (byte)BlockData.Blocks.Cactus, 0x0);
            }
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            base.DropItems(entity, block);

            // TODO: If the top block is a cactus as well - destroy it
        }
    }
}
