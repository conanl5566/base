﻿using System;

namespace CompanyName.ProjectName.ICommonServer
{
    /// <summary>
    ///
    /// </summary>
    public class WebConfigDto : UpdateWebConfigDto
    {
        /// <summary>
        ///
        /// </summary>
        public DateTime CreatorTime { get; set; }

        /// <summary>
        ///
        /// </summary>
        public long? CreatorUserId { get; set; }
    }
}