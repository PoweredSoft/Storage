using PoweredSoft.Storage.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.Storage.S3
{
    public interface IS3FileWriteOptions
    {
        public string Acl { get; }
    }

    public class S3FileWriteOptions : DefaultWriteOptions, IS3FileWriteOptions
    {
        public string Acl { get; set; }
    }
}
