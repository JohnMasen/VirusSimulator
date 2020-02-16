using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.QuadTree
{
    public static class QuadTreeHelper
    {
        public static void Load<T>(this QuadTreeNode<T> node, DataBuffer<T> data) where T : struct
        {
            foreach (var item in data.Items.Span)
            {
                node.AddItem(item);
            }
        }

        public static int GetItemsCountInDistance<T>(this QuadTreeNode<T> node, Vector2 position, float distance) where T:struct
        {
            var result = node.GetItemsInDistance(position, distance);
            if (result==null)
            {
                return 0;
            }
            else
            {
                return result.Count();
            }
        }
    }
}
