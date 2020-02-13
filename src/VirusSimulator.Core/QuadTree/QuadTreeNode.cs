using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.QuadTree
{
    public class QuadTreeNode<T>
    {
        public readonly RectangleF Range;
        public Lazy<List<T>> Items=new Lazy<List<T>>();
        private readonly int capacity;
        private Vector2 center;
        public List<QuadTreeNode<T>> Children;

        public delegate Vector2 GetPositionDelegate(ref T value);

        private GetPositionDelegate getPosition;

        private int level;
        public QuadTreeNode(RectangleF range, GetPositionDelegate getPositionCallback, int Capacity=10,int MaxLevels=100)
        {
            level = MaxLevels;
            if (Capacity<=0)
            {
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must larger than 0");
            }
            Range = range;
            capacity = Capacity;
            center = new Vector2(Range.X + Range.Width / 2, Range.Top + Range.Height / 2);
            getPosition = getPositionCallback;
        }
        public void AddItem(T newItem)
        {
            if (Children!=null)//try insert into children
            {
                foreach (var item in Children)
                {
                    if (item.IsInRange(getPosition(ref newItem)))
                    {
                        item.AddItem(newItem);
                        return;
                    }
                }
            }
            else
            {
                if (Items.Value.Count==capacity && level>0)
                {
                    //capacity full, split into new children
                    splitRange();
                    //move items to children
                    foreach (var item in Items.Value)
                    {
                        AddItem(item);
                    }
                    Items.Value.Clear();
                    //don't forget add current item
                    AddItem(newItem);
                }
                else
                {
                    //always add current item
                    Items.Value.Add(newItem);
                }
                
            }
        }

        public bool IsLeaf => Children == null;
        private void splitRange()
        {
            Children = new List<QuadTreeNode<T>>(4);
            int newLevel = level - 1;
            Children.Add(new QuadTreeNode<T>(RectangleF.FromLTRB(Range.Left, Range.Top, center.X, center.Y),getPosition, capacity, newLevel));//top left
            Children.Add(new QuadTreeNode<T>(RectangleF.FromLTRB(center.X, Range.Top, Range.Right, center.Y), getPosition, capacity, newLevel));//top right
            Children.Add(new QuadTreeNode<T>(RectangleF.FromLTRB(Range.Left, center.Y, center.X, Range.Bottom), getPosition, capacity, newLevel));//bottom left
            Children.Add(new QuadTreeNode<T>(RectangleF.FromLTRB(center.X, center.Y, Range.Right, Range.Bottom), getPosition, capacity, newLevel));//bottom right
        }
        public bool IsInRange(Vector2 v)
        {
            return Range.Contains(v.X, v.Y);
        }

        public void Clear()
        {
            Items = new Lazy<List<T>>();
            Children = null;
        }

        public bool IsIntersectWithCircle(Vector2 circleCenter,float radius)
        {
            return IsIntersectWithCircleInternal(circleCenter, radius * radius);
        }

        private bool IsIntersectWithCircleInternal(Vector2 circleCenter,float radiusSqr)
        {
            if (Range.Contains(circleCenter.X, circleCenter.Y)) //check if the center inside the range
            {
                return true;
            }
            //algorithum reference: https://blog.csdn.net/noahzuo/article/details/52037151
            Vector2 a = Vector2.Abs(circleCenter - center);
            Vector2 e = Vector2.Max(a - (new Vector2(Range.Right, Range.Top) - center), Vector2.Zero);
            return Vector2.Dot(e, e) <= radiusSqr;//use dot instead of lengthsquart to prevent boxing
        }

        public IEnumerable<T> GetItemsInDistance(Vector2 position,float distance)
        {
            return GetItemInDistanceInternal(position,distance * distance);
        }

        private IEnumerable<T> GetItemInDistanceInternal(Vector2 position, float distanceSqr)
        {
            
            if (IsLeaf)
            {
                return Items.Value.Where(x=>Vector2.DistanceSquared(position, getPosition(ref x))<= distanceSqr);
            }
            else
            {
                IEnumerable<T> result = null;
                foreach (var item in Children)
                {
                    
                    if (item.IsIntersectWithCircleInternal(position,distanceSqr))
                    {
                        if (result==null)
                        {
                            result = item.GetItemInDistanceInternal(position, distanceSqr);
                        }
                        else
                        {
                            result = result.Concat(item.GetItemInDistanceInternal(position, distanceSqr));
                        }
                        
                    }
                }
                return result?.Where(x => Vector2.DistanceSquared(position, getPosition(ref x)) <= distanceSqr);
            }
        }


        
    }
}
