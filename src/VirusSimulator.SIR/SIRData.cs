using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using VirusSimulator.Core;

namespace VirusSimulator.SIR
{



    public struct SIRData
    {
        public int Status { get; set; }

        public float InfectionProgress { get; set; }

        public TimeSpan GroundCountdown { get; set; }
        public TimeSpan CureCountdown { get; set; }

        #region Consts
        /// <summary>
        /// 已经被感染
        /// </summary>
        private const int Infected = 0b_001;
        /// <summary>
        /// 可以被感染
        /// </summary>
        private const int CanInfect = 0b_010;
        /// <summary>
        /// 可以感染他人
        /// </summary>
        private const int CanInfectOthers = 0b_100;
        /// <summary>
        /// 易感人群（未感染，可以被传染，无法传染他人）
        /// </summary>
        public const int Susceptible = CanInfect;
        /// <summary>
        /// 传染者（已感染，无法被传染， 可以传染他人）
        /// </summary>
        public const int Infective = Infected | CanInfectOthers;
        /// <summary>
        /// 恢复者（未感染，无法被传染，无法传染他人）
        /// </summary>
        public const int Recovered = 0;
        /// <summary>
        /// 被隔离，（已传染，无法被传染，无法传染他人）
        /// </summary>
        public const int Grounded = Infected;
        #endregion
    }

    public interface ISIRContext
    {
        public DataBuffer<SIRData> SIRInfo { get; }


        public (int Susceptible, int Infective, int Grounded, int Recovered) GetCount()
        {
            int Susceptible = 0;
            int Infective = 0;
            int Grounded = 0;
            int Recovered = 0;
            SIRInfo.ForAllParallel((int idx,ref SIRData item) =>
            {
                if ((item.Status & SIRData.Susceptible) ==SIRData.Susceptible)
                {
                    Interlocked.Increment(ref Susceptible);
                    return;
                }
                if ((item.Status & SIRData.Infective)==SIRData.Infective)
                {
                    Interlocked.Increment(ref Infective);
                    return;
                }
                if ((item.Status & SIRData.Grounded)==SIRData.Grounded)
                {
                    Interlocked.Increment(ref Grounded);
                    return;
                }
                if ((item.Status & SIRData.Recovered)==SIRData.Recovered)
                {
                    Interlocked.Increment(ref Recovered);
                    return;
                }
                Debug.WriteLine($"idx={idx},value= {item.Status}");
                throw new InvalidOperationException();
            });
            return (Susceptible, Infective, Grounded, Recovered);
        }
    }
}
