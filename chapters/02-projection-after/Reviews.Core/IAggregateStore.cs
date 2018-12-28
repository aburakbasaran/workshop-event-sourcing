﻿using System.Threading;
using System.Threading.Tasks;

namespace Reviews.Core
{
    public interface IAggrigateStore
    {
        Task<(long NextExceptedVersion, long LastPosition, long CommitPosition)> Save<T>(T aggregate,CancellationToken cancellationToken = default) 
            where T : Aggregate;
        
        Task<T> Load<T>(string aggregateId, CancellationToken cancellationToken = default) 
            where T : Aggregate,new();
    }
}