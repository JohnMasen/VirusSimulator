using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;

namespace VirusSimulator.Core.QuadTree
{
    public class QuadTreeNode
    {
        public readonly RectangleF Range;
        public Lazy<List<Person>> Items=new Lazy<List<Person>>();
        private int capacity;
        private Vector2 center;
        public List<QuadTreeNode> Children;
        public QuadTreeNode(RectangleF range,int Capacity=10)
        {
            if (Capacity<=0)
            {
                throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must larger than 0");
            }
            Range = range;
            capacity = Capacity;
            center = new Vector2(Range.X + Range.Width / 2, Range.Top + Range.Height / 2);
        }
        public void AddItem(Person person)
        {
            if (Children!=null)//try insert into children
            {
                foreach (var item in Children)
                {
                    if (item.IsInRange(person.Position))
                    {
                        item.AddItem(person);
                        return;
                    }
                }
            }
            else
            {
                if (Items.Value.Count==capacity)
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
                    AddItem(person);
                }
                else
                {
                    //always add current item
                    Items.Value.Add(person);
                }
                
            }
        }

        public bool IsLeaf => Children == null;
        private void splitRange()
        {
            Children = new List<QuadTreeNode>(4);
            
            Children.Add(new QuadTreeNode(RectangleF.FromLTRB(Range.Left, Range.Top, center.X, center.Y), capacity));//top left
            Children.Add(new QuadTreeNode(RectangleF.FromLTRB(center.X, Range.Top, Range.Right, center.Y), capacity));//top right
            Children.Add(new QuadTreeNode(RectangleF.FromLTRB(Range.Left, center.Y, center.X, Range.Bottom), capacity));//bottom left
            Children.Add(new QuadTreeNode(RectangleF.FromLTRB(center.X, center.Y, Range.Right, Range.Bottom), capacity));//bottom right
        }
        public bool IsInRange(Vector2 v)
        {
            return Range.Contains(v.X, v.Y);
        }

        public bool IsIntersectWithCircle(Vector2 circleCenter,float radius)
        {
            if (Range.Contains(circleCenter.X,circleCenter.Y)) //check if the center inside the range
            {
                return true;
            }
            //algorithum reference: https://blog.csdn.net/noahzuo/article/details/52037151
            Vector2 a = Vector2.Abs(circleCenter - center);
            Vector2 e = Vector2.Max(a - (new Vector2(Range.Right, Range.Top)-center), Vector2.Zero);
            return Vector2.Dot(e,e) <= radius * radius;//use dot instead of lengthsquart to prevent boxing
        }

        public IEnumerable<Person> GetPersonInDistance(Vector2 position, float distance)
        {
            float d2 = distance * distance;
            if (IsLeaf)
            {
                return Items.Value.Where(x=>Vector2.DistanceSquared(position,x.Position)<=d2);
            }
            else
            {
                IEnumerable<Person> result = null;
                foreach (var item in Children)
                {
                    
                    if (item.IsIntersectWithCircle(position,distance))
                    {
                        if (result==null)
                        {
                            result = item.GetPersonInDistance(position, distance);
                        }
                        else
                        {
                            result = result.Concat(item.GetPersonInDistance(position,distance));
                        }
                        
                    }
                }
                return result?.Where(x => Vector2.DistanceSquared(position, x.Position) <= d2);
            }
        }
        
    }
}
