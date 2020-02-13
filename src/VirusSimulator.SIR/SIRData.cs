using System;
using System.Collections;
using VirusSimulator.Core;

namespace VirusSimulator.SIR
{

    
    
    public struct SIRData
    {
        public byte Status { get; set; }

        public float InfectionRate { get; set; }

        public TimeSpan GroundCountdown { get; set; }
        public TimeSpan CureCountdown { get; set; }

        #region Consts
        public const byte CleanupFlags = 0b_11111_000;
        /// <summary>
        /// 已经被感染
        /// </summary>
        public const byte Infected = 0b_001;
        /// <summary>
        /// 可以被感染
        /// </summary>
        public const byte CanInfect = 0b_010;
        /// <summary>
        /// 可以感染他人
        /// </summary>
        public const byte CanInfectOthers = 0b_100;
        /// <summary>
        /// 更新掩码-易感人群（未感染，可以被传染，无法传染他人）
        /// </summary>
        public const byte Person_Susceptible = CanInfect;
        /// <summary>
        /// 更新掩码-传染者（已感染，无法被传染， 可以传染他人）
        /// </summary>
        public const byte Person_Infective = Infected|CanInfectOthers;
        /// <summary>
        /// 更新掩码-恢复者（未感染，无法被传染，无法传染他人）
        /// </summary>
        public const byte Person_Recovered = 0;
        /// <summary>
        /// 更新掩码-被隔离，（已传染，无法被传染，无法传染他人）
        /// </summary>
        public const byte Person_Grounded = Infected;
        
    }

    public interface ISIRContext
    {
        public DataBuffer<SIRData> SIRInfo { get;  }
    }
}
