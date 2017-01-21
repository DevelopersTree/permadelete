using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Permadelete.Enums
{
    public enum FileSystemType
    {
        /// <summary>
        /// New Technology File System.
        /// </summary>
        NTFS,
        /// <summary>
        /// File Allocation Table.
        /// </summary>
        FAT32,
        /// <summary>
        /// Unknown File System.
        /// </summary>
        Unknown,
        /// <summary>
        /// Used with ExpanDrive.
        /// </summary>
        EXFS
    }
}
