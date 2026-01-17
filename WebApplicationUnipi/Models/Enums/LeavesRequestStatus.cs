using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplicationUnipi.Models.Enums
{
    public enum LeavesRequestStatus
    {
        Requested = 1,
        Approved = 2,
        Deleted = 3,
        Rejected = 4,
        Cancelled = 5
    }
}