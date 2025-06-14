﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MRT.Domain.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        public DateTime CreationDate { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModificationDate { get; set; }

        public Guid? ModificationBy { get; set; }

        public DateTime? DeletionDate { get; set; }

        public Guid? DeleteBy { get; set; }

        public bool IsDeleted { get; set; }
    }
}
