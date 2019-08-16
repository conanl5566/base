﻿using dotNET.Domain.Entities;
using dotNET.Domain.Entities.Sys;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace dotNET.Domain
{
 
    public class UserConfiguration : EntityMappingConfiguration<User>
    {
        public override void Map(EntityTypeBuilder<User> b)
        {
            b.ToTable("User")
                .HasKey(p => p.Id);
        }
    }

}