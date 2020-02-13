using System;
using System.Collections;

namespace VirusSimulator.SIR
{
    [Flags]
    public enum SIRStatus : int
    {
        /// <summary>
        /// 是否已被感染
        /// </summary>
        Infected =  0b_0000_0001, 
        /// <summary>
        /// 是否可以被感染
        /// </summary>
        CanInfect = 0b_0000_0010,
        /// <summary>
        /// 是否可以感染他人
        /// </summary>
        CanInfectOthers=0b_0000_0100,


        /// <summary>
        /// 易感人群（未感染，可以被传染）
        /// </summary>
        Susceptible=0b_0000_0010,
        /// <summary>
        /// 传染者（已感染，可以传染）
        /// </summary>
        Infectives=0b_0000_0101,
        /// <summary>
        /// 恢复者（未感染，免疫感染）
        /// </summary>
        Recovered=0b_0000_0000,


    }
    public struct SIRData
    {
        public SIRStatus Status { get; set; }
        public float InfectionRate { get; set; }
    }
}
