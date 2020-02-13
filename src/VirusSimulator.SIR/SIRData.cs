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

        #region Consts
        public const byte Infected = 0b_001;
        /// <summary>
        /// 是否可以被感染
        /// </summary>
        public const byte CanInfect = 0b_010;
        /// <summary>
        /// 是否可以感染他人
        /// </summary>
        public const byte CanInfectOthers = 0b_100;
        /// <summary>
        /// 更新掩码-易感人群（未感染，可以被传染）
        /// </summary>
        public const byte Make_Susceptible = 0b_11111_010;
        /// <summary>
        /// 更新掩码-传染者（已感染，可以传染）
        /// </summary>
        public const byte Make_Infectives = 0b_11111_101;
        /// <summary>
        /// 更新掩码-恢复者（未感染，免疫感染）
        /// </summary>
        public const byte Make_Recovered = 0b_11111_000;
        /// <summary>
        /// 更新掩码-被隔离，（已传染，无法传染他人）
        /// </summary>
        public const byte Make_Grounded = 0b_11111_001;
        /// <summary>
        /// 传染者（已感染，可以传染）
        /// </summary>
        public const byte InfectedNotGrounded = Infected | CanInfectOthers;
        /// <summary>
        /// 被隔离，（已传染，无法传染他人）
        /// </summary>
        public const byte InfectedGrounded = Infected;
        #endregion

    }

    public interface ISIRContext
    {
        public DataBuffer<SIRData> SIRInfo { get;  }
    }
}
