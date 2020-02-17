using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;
using VirusSimulator.Core;
using System.Linq;
using System.Net.Mime;
using System.Resources;
using VirusSimulator.Core.QuadTree;

namespace VirusSimulator.Processor
{
    public class POIProcessor<T> : ProcessorBase<T> where T : RunContext, IPersonMoveContext,IPOIContext
    {
        public List<Vector2> POIList { get; private set; } = new List<Vector2>();
        //public float Radius { get; set; } = 300f;
        Func<T, IEnumerable<Vector2>> create;
        public float Activity { get; set; } = 0.01f;

        public float POIScanRadiusLarge { get; set; }

        public float POIScanRadiusSmall { get; set; }
        public QuadTreeNode<Vector2> POIIndex ;

        public POIProcessor(Func<T, IEnumerable<Vector2>> createPOI)
        {
            create = createPOI;
        }
        public override void Init(T context)
        {
            POIList.AddRange(create(context));
            POIIndex = new QuadTreeNode<Vector2>(new RectangleF(0,0,context.Size.Width,context.Size.Height), (ref Vector2 x) =>
               {
                   return x;
               });
            create = null;//release creation function to reduce other object reference
            foreach (var item in POIList)
            {
                POIIndex.AddItem(item);
            }
            //init home position
            context.Persons.ForAllParallelWtihReference(context.POIData, (ref PositionItem pos, ref POIInfo poi) =>
             {
                 poi.POIStatus = POIStatusEnum.AtHome;
                 poi.HomePosition = pos.Position;
             });
        }

        public override void Process(T context, TimeSpan span)
        {
            context.MoveStatus.ForAllParallelWtihReference(context.POIData, (int index,ref MoveStatus m,ref POIInfo poi) =>
            {
                if (m.IsMovingToTarget == MovingStatusEnum.Idle)
                {
                    ProcessStatus(index, ref m, ref poi,span);
                }
            });
        }

        protected virtual void ProcessStatus(int index, ref MoveStatus m,ref POIInfo poi,TimeSpan span)
        {
            switch (poi.POIStatus)
            {
                case POIStatusEnum.AtHome:
                    if (Helper.RandomFloat(1) < Activity)//let's shopping
                    {
                        navigateToNewPOI(ref m, poi.HomePosition,  POIScanRadiusLarge);
                        poi.POIStatus = POIStatusEnum.FromHomeToPOI;
                    }
                    break;
                case POIStatusEnum.FromHomeToPOI:
                case POIStatusEnum.FromPOIToPOI:
                    if (Helper.RandomFloat(1) < Activity) //continue shopping
                    {
                        navigateToNewPOI(ref m, m.CurrentTarget,  POIScanRadiusSmall);
                        poi.POIStatus = POIStatusEnum.FromPOIToPOI;
                    }
                    else
                    {
                        navigateToHome(ref m, ref poi);
                        poi.POIStatus = POIStatusEnum.GoHome;
                    }
                    break;
                case POIStatusEnum.GoHome:
                    poi.POIStatus=POIStatusEnum.AtHome;
                    break;
                default:
                    break;
            }
        }


        private void navigateToHome(ref MoveStatus m,ref POIInfo poi)
        {
            m.CurrentTarget = poi.HomePosition;
            m.IsMovingToTarget = MovingStatusEnum.Moving;
        }

        private void navigateToNewPOI(ref MoveStatus m, Vector2 referencePosition,float radius)
        {
            var newPosition = findRandomPOI(referencePosition, radius);
            if (newPosition.HasValue)//find one more POI
            {
                m.CurrentTarget = newPosition.Value;
                m.IsMovingToTarget = MovingStatusEnum.Moving;
                
            }
        }

        protected Vector2? findRandomPOI(Vector2 pos, float distance)
        {
            var tmp = POIIndex.GetItemsInDistance(pos, distance);
            if (tmp!=null)
            {
                var data = tmp.ToList();
                if (data.Count>0)
                {
                    return data[Helper.RandomInt(data.Count)];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public static POIProcessor<T> CreateFromPoints(IEnumerable<Vector2> data)
        {
            return new POIProcessor<T>(_ => data);
        }

        public static POIProcessor<T> CreateRandomPOI(int count,float activity) 
        {
            return new POIProcessor<T>(context => createRandomPoints(count, context)) { Activity = activity }; ;

        }
        private static IEnumerable<Vector2> createRandomPoints<TContext>(int count, TContext context) where TContext : RunContext, IPersonMoveContext
        {
            for (int i = 0; i < count; i++)
            {
                yield return new Vector2(Helper.RandomFloat(context.Size.Width), Helper.RandomFloat(context.Size.Height));
            }
        }
    }
}
