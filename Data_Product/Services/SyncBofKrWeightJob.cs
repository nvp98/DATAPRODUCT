using Quartz;

namespace Data_Product.Services
{
    [DisallowConcurrentExecution]
    public class SyncBofKrWeightJob : IJob
    {
        private readonly IBofKrWeightSyncService _syncService;
        private readonly ILogger<SyncBofKrWeightJob> _logger;

        public SyncBofKrWeightJob(IBofKrWeightSyncService syncService, ILogger<SyncBofKrWeightJob> logger)
        {
            _syncService = syncService;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            // Kéo cửa sổ [today-1, today] để bắt các bản ghi nhà máy cập nhật/hiệu chỉnh trễ qua ngày.
            var to = DateTime.Today;
            var from = to.AddDays(-1);

            try
            {
                var count = await _syncService.SyncAsync(from, to, context.CancellationToken);
                _logger.LogInformation("Đồng bộ BOF/KR weight: {Count} bản ghi ({From:yyyy-MM-dd} - {To:yyyy-MM-dd})", count, from, to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi đồng bộ dữ liệu cân BOF/KR");
            }
        }
    }
}
