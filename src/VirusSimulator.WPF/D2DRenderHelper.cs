using SeeingSharp;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing2D;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using VirusSimulator.Core;
using VirusSimulator.SIR;

namespace VirusSimulator.WPF
{
    public class D2DRenderHelper 
    {
        private List<Action<Graphics2D>> drawCommands = new List<Action<Graphics2D>>();
        public bool Enabled { get; set; } = true;
        private object drawSync = new object();
        BrushResource redBrush = new SolidBrushResource(Color4.RedColor);
        BrushResource greeBrush = new SolidBrushResource(new Color4(76,255,0));
        BrushResource yellowBrush = new SolidBrushResource(Color4.Yellow);
        public async Task Init(RenderLoop loop)
        {
            drawCommands.Clear();
            try
            {
                await loop.Register2DDrawingLayerAsync(new Custom2DDrawingLayer(drawInternal));
            }
            catch (Exception)
            {

                throw;
            }
            
            
        }

        private void drawInternal(Graphics2D graphics)
        {
            lock (drawSync)
            {
                graphics.Clear(Color4.Black);
                foreach (var item in drawCommands)
                {
                    item(graphics);
                }
            }
        }

        public void UpdateCommands(TestContext context)
        {
            if (!Enabled)
            {
                return;
            }
            List<Action<Graphics2D>> newList = new List<Action<Graphics2D>>();
            context.Persons.ForAllWtihReference(context.SIRInfo, (ref PositionItem p, ref SIRData infection) =>
             {
                 
                 if (infection.Status == SIRData.Susceptible || infection.Status == SIRData.Infective)//can infect others or can be infected
                 {
                     BrushResource c = null;
                     if (infection.Status == SIRData.Susceptible)
                     {
                         c = greeBrush;
                     }
                     else
                     {
                         if (infection.GroundCountdown == TimeSpan.Zero)
                         {
                             c = redBrush;
                         }
                         else
                         {
                             c = yellowBrush;
                         }
                     }
                     var pos = p.Position;
                     newList.Add((graph) =>
                     {
                         graph.FillRectangle(new RectangleF(pos.X,pos.Y, 2, 2), c);
                     });
                 }
                 
             });

            lock (drawSync)
            {
                drawCommands = newList;
            }
        }
    }
}
