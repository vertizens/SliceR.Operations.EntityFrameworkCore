﻿using Microsoft.EntityFrameworkCore.Metadata;

namespace Vertizens.SliceR.Operations.EntityFrameworkCore;
internal interface IDbContextStartupFactory
{
    IEnumerable<IEntityType> GetEntityTypes(IEnumerable<Type> dbContextTypes);
}
