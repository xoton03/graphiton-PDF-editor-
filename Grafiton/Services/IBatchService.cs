using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Grafiton.Models;

namespace Grafiton.Services;

public interface IBatchService
{
    Task ProcessBatchAsync(List<BatchJob> jobs, IProgress<double>? overallProgress = null, CancellationToken cancellationToken = default);
}
