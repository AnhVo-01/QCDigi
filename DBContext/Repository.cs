using Microsoft.Data.SqlClient;
using TcclPdfTool.Model;
using TcclPdfTool.Config;

namespace TcclPdfTool.DBContext
{
    public sealed class Repository
    {
        private readonly DbTargets _db;

        public Repository(DbTargets targets) => _db = targets;

        /// <summary>Tiện cho gọi nhanh khi chỉ có một chuỗi kết nối.</summary>
        public Repository(string connectionString)
            : this(new DbTargets { SalesConnectionString = connectionString }) { }

        private async Task<SqlConnection> OpenAsync(CancellationToken ct)
        {
            var cn = new SqlConnection(_db.SalesConnectionString);
            await cn.OpenAsync(ct).ConfigureAwait(false);
            return cn;
        }

        /// <summary>
        /// Mở kết nối tới một CSDL trên SERVER KHÁC (ACPThongBaoSX, DBVatTu...).
        /// Dữ liệu lấy từ đây không join chéo được với Sales - phải ghép trong C#.
        /// </summary>
        public async Task<SqlConnection> OpenOtherAsync(string key, CancellationToken ct = default)
        {
            var cs = _db.Connection(key)
                     ?? throw new InvalidOperationException($"Chưa cấu hình chuỗi kết nối cho CSDL '{key}'.");
            var cn = new SqlConnection(cs);
            await cn.OpenAsync(ct).ConfigureAwait(false);
            return cn;
        }

        public async Task<string> TestConnectionAsync(CancellationToken ct = default)
        {
            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            using var cmd = new SqlCommand("SELECT DB_NAME() + ' @ ' + @@SERVERNAME", cn);
            var v = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            var lines = new List<string> { "Sales: " + (v?.ToString() ?? "") };

            // Kiểm tra luôn các CSDL cùng instance: có thật và login có quyền đọc không.
            foreach (var key in _db.SameServerDbNames.Keys)
            {
                var prefix = _db.Prefix(key);
                if (prefix is null) continue;
                try
                {
                    using var probe = new SqlCommand($"SELECT TOP 1 1 FROM {prefix}sysobjects", cn);
                    probe.CommandTimeout = 10;
                    await probe.ExecuteScalarAsync(ct).ConfigureAwait(false);
                    lines.Add($"{key}: đọc được ({_db.SameServerDbNames[key]})");
                }
                catch (Exception ex)
                {
                    lines.Add($"{key}: KHÔNG đọc được - {ex.Message}");
                }
            }

            // Các CSDL trên server khác phải mở kết nối riêng.
            foreach (var key in _db.OtherServerConnections.Keys)
            {
                try
                {
                    using var other = await OpenOtherAsync(key, ct).ConfigureAwait(false);
                    using var probe = new SqlCommand("SELECT DB_NAME() + ' @ ' + @@SERVERNAME", other);
                    probe.CommandTimeout = 10;
                    var r = await probe.ExecuteScalarAsync(ct).ConfigureAwait(false);
                    lines.Add($"{key}: {r}");
                }
                catch (Exception ex)
                {
                    lines.Add($"{key}: KHÔNG kết nối được - {ex.Message}");
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        // ------------------------------------------------------------- danh mục

        private bool? _hasXxthId;

        /// <summary>
        /// TieuChiChatLuong đã có cột XXTHId chưa? Tiêu chí giờ chỉ gắn theo (XXTHId, SanPhamId),
        /// không còn đi qua hợp đồng - thiếu cột này thì không ghi được gì.
        /// </summary>
        public async Task<bool> HasXxthColumnAsync(CancellationToken ct = default)
        {
            if (_hasXxthId is bool b) return b;
            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            await LoadSchemaAsync(cn, ct).ConfigureAwait(false);
            return _hasXxthId!.Value;
        }

        private async Task LoadSchemaAsync(SqlConnection cn, CancellationToken ct)
        {
            _hasXxthId ??= await ColumnExistsAsync(cn, "TieuChiChatLuong", "XXTHId", ct).ConfigureAwait(false);
        }

        private void EnsureXxthColumn()
        {
            if (_hasXxthId != true)
                throw new InvalidOperationException(
                    "Bảng TieuChiChatLuong chưa có cột XXTHId - tiêu chí giờ gắn theo (XXTHId, SanPhamId).");
        }

        private async Task<bool> ColumnExistsAsync(SqlConnection cn, string table, string column, CancellationToken ct)
        {
            using var cmd = new SqlCommand(
                "SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME=@t AND COLUMN_NAME=@c", cn);
            cmd.Parameters.AddWithValue("@t", table);
            cmd.Parameters.AddWithValue("@c", column);
            return await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false) is not null;
        }

        /// <summary>
        /// Lấy danh mục tiêu chí. Điều kiện lọc bám đúng GetDanhSachTieuChi() phía server:
        /// cùng công ty, và FormType khớp HOẶC để trống (tiêu chí dùng chung mọi biểu mẫu).
        /// </summary>
        public async Task<List<DmTieuChi>> GetCatalogAsync(int congTyId, CancellationToken ct = default)
        {
            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            bool hasTol = await ColumnExistsAsync(cn, "DM_TieuChi", "MinTolerance", ct).ConfigureAwait(false);

            var tolCols = hasTol ? "MinTolerance, MaxTolerance" : "CAST(NULL AS DECIMAL(18,4)) AS MinTolerance, CAST(NULL AS DECIMAL(18,4)) AS MaxTolerance";
            var sql = $@"
                        SELECT Id, MaTieuChi, TieuChi, FormType, Tolerance, CongTyId, IsGroup, GroupId, Func, {tolCols}
                        FROM dbo.DM_TieuChi
                        WHERE CongTyId = @congTyId
                        ORDER BY GroupId, Id";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@congTyId", congTyId);

            var list = new List<DmTieuChi>();
            using var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await rd.ReadAsync(ct).ConfigureAwait(false))
            {
                list.Add(new DmTieuChi
                {
                    Id = rd.GetInt32(0),
                    MaTieuChi = Str(rd, 1) ?? "",
                    TieuChi = Str(rd, 2) ?? "",
                    FormType = Str(rd, 3),
                    Tolerance = Str(rd, 4),
                    CongTyId = rd.GetInt32(5),
                    IsGroup = !rd.IsDBNull(6) && rd.GetBoolean(6),
                    GroupId = rd.IsDBNull(7) ? null : rd.GetInt32(7),
                    Func = Str(rd, 8),
                    MinTolerance = Dec(rd, 9),
                    MaxTolerance = Dec(rd, 10)
                });
            }
            return list;
        }

        // --------------------------------------------------------- file đính kèm

        /// <summary>
        /// Liệt kê file TCCL từ [ThongBaoSX].dbo.MucDinhKem (MarketType = 'TCCL').
        ///
        /// Chuỗi liên kết, tất cả nằm cùng một SQL Server nên join chéo được:
        ///   MucDinhKem.SanPhamId -> DM_SanPham.Id   (lấy MaSPNB)
        ///   Market_TBSX.XXTHId   -> XXThucHien.Id   (lấy MaXXTH, CongTyId)
        /// Khoá ghi vào TieuChiChatLuong là (XXTHId, SanPhamId) - không cần hợp đồng.
        ///
        /// Cố ý dùng LEFT JOIN toàn bộ: file nào đứt chuỗi vẫn hiện ra để người xử lý tay,
        /// thay vì biến mất khỏi danh sách mà không ai biết.
        /// </summary>
        /// <param name="maXxthFilter">
        /// Danh sách mã XXTH cần lấy (khớp chính xác). Null hoặc rỗng thì lấy tất cả.
        /// Lọc theo mã nào thì file chưa gắn được XXTH sẽ không xuất hiện - đúng ý nghĩa,
        /// vì không gắn XXTH thì không thuộc đơn hàng nào cả.
        /// </param>
        public async Task<List<FileJob>> GetCandidateFilesAsync(
            int congTyId, string? fileNameFilter, bool onlyNotImported, int maxRows,
            string marketType = "TCCL", IReadOnlyList<string>? maXxthFilter = null,
            CancellationToken ct = default)
        {
            var tbsx = _db.Prefix("ThongBaoSX")
                ?? throw new InvalidOperationException(
                    "Chưa cấu hình tên CSDL ThongBaoSX (mục sameServerDbNames trong appsettings.json) - " +
                    "không đọc được bảng MucDinhKem.");

            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            await LoadSchemaAsync(cn, ct).ConfigureAwait(false);
            EnsureXxthColumn();

            // Lọc theo đơn hàng: dựng tham số riêng cho từng mã, không nối chuỗi vào SQL.
            var xxthNames = new List<string>();
            if (maXxthFilter is { Count: > 0 })
                for (int i = 0; i < maXxthFilter.Count; i++) xxthNames.Add("@xx" + i);
            var xxthClause = xxthNames.Count == 0
                ? ""
                : $"AND xx.MaXXTH IN ({string.Join(", ", xxthNames)})";

            // "Đã import" tra đúng cách CRM tự tra ở GetTieuChiByXXTH: theo (XXTHId, SanPhamId).
            var sql = $@"
WITH src AS (
    SELECT  m.Id, m.FileName, m.Path, m.CreatedDate, m.MarketType,
            m.SanPhamId      AS SanPhamId,
            sp.MaSPNB        AS MaSPNB,
            xx.Id            AS XxthId,
            xx.MaXXTH        AS MaXXTH,
            ISNULL(xx.CongTyId, 0) AS CongTyId,
            CASE WHEN xx.Id IS NOT NULL AND m.SanPhamId IS NOT NULL AND EXISTS (
                      SELECT 1 FROM dbo.TieuChiChatLuong x
                      WHERE x.XXTHId = xx.Id AND x.SanPhamId = m.SanPhamId)
                 THEN 1 ELSE 0 END AS AlreadyImported,
            COUNT(xx.Id) OVER (PARTITION BY m.Id) AS MatchCount,
            ROW_NUMBER() OVER (PARTITION BY m.Id
                               ORDER BY CASE WHEN xx.Id IS NULL THEN 1 ELSE 0 END, xx.Id DESC) AS rn
    FROM        {tbsx}MucDinhKem      m
    LEFT JOIN   {tbsx}Market_TBSX     mt  ON mt.MucDinhKem = m.Id AND mt.DeletedDate IS NULL
    LEFT JOIN   dbo.DM_SanPham        sp  ON sp.Id = m.SanPhamId
    LEFT JOIN   dbo.XXThucHien        xx  ON xx.Id = mt.XXTHId AND xx.DeletedDate IS NULL
    WHERE   m.MarketType = @marketType
      AND   m.DeletedDate IS NULL
      AND   (m.FileName LIKE '%.pdf' OR m.FileType = 'application/pdf')
      AND   (@nameFilter IS NULL OR m.FileName LIKE @nameFilter)
      {xxthClause}
)
SELECT TOP (@maxRows)
       Id, FileName, Path, CreatedDate, MarketType, SanPhamId, MaSPNB, XxthId, MaXXTH,
       CongTyId, AlreadyImported, MatchCount
FROM   src
WHERE  rn = 1
  AND  (@congTyId = 0 OR CongTyId = @congTyId OR CongTyId = 0)
  AND  (@onlyNotImported = 0 OR AlreadyImported = 0)
ORDER BY CreatedDate DESC";

            using var cmd = new SqlCommand(sql, cn);
            cmd.CommandTimeout = 120;
            cmd.Parameters.AddWithValue("@congTyId", congTyId);
            cmd.Parameters.AddWithValue("@marketType", marketType);
            cmd.Parameters.AddWithValue("@nameFilter",
                string.IsNullOrWhiteSpace(fileNameFilter) ? DBNull.Value : (object)$"%{fileNameFilter}%");
            cmd.Parameters.AddWithValue("@onlyNotImported", onlyNotImported);
            cmd.Parameters.AddWithValue("@maxRows", maxRows > 0 ? maxRows : int.MaxValue);
            if (maXxthFilter is { Count: > 0 })
                for (int i = 0; i < maXxthFilter.Count; i++)
                    cmd.Parameters.AddWithValue("@xx" + i, maXxthFilter[i]);

            var list = new List<FileJob>();
            using var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            while (await rd.ReadAsync(ct).ConfigureAwait(false))
            {
                list.Add(new FileJob
                {
                    DinhKemId = rd.GetInt32(0),
                    FileName = Str(rd, 1) ?? "",
                    RelativePath = Str(rd, 2) ?? "",
                    CreatedDate = rd.IsDBNull(3) ? null : rd.GetDateTime(3),
                    MarketType = Str(rd, 4),
                    SanPhamId = rd.IsDBNull(5) ? null : rd.GetInt32(5),
                    MaSPNB = Str(rd, 6),
                    XxthId = rd.IsDBNull(7) ? null : rd.GetInt32(7),
                    MaXXTH = Str(rd, 8),
                    CongTyId = rd.IsDBNull(9) ? 0 : rd.GetInt32(9),
                    AlreadyImported = Convert.ToInt32(rd.GetValue(10)) == 1,
                    MatchCount = rd.IsDBNull(11) ? 0 : Convert.ToInt32(rd.GetValue(11))
                });
            }
            return list;
        }

        // ------------------------------------------------------------------- ghi

        /// <summary>
        /// Các bản ghi TieuChiChatLuong đã có theo khoá (SanPhamId, XXTHId) - để hỏi người dùng
        /// có ghi đè hay không trước khi ghi.
        /// </summary>
        public async Task<List<ExistingTieuChiChatLuong>> GetExistingAsync(
            IReadOnlyCollection<(int SanPhamId, int XxthId)> keys, CancellationToken ct = default)
        {
            var list = new List<ExistingTieuChiChatLuong>();
            if (keys.Count == 0) return list;

            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            await LoadSchemaAsync(cn, ct).ConfigureAwait(false);
            EnsureXxthColumn();

            // SQL Server giới hạn 2100 tham số mỗi lệnh - chia lô 500 cặp khoá.
            foreach (var chunk in keys.Distinct().Chunk(500))
            {
                using var cmd = new SqlCommand { Connection = cn };
                var conds = new List<string>(chunk.Length);
                for (int i = 0; i < chunk.Length; i++)
                {
                    conds.Add($"(SanPhamId = @sp{i} AND XXTHId = @xx{i})");
                    cmd.Parameters.AddWithValue("@sp" + i, chunk[i].SanPhamId);
                    cmd.Parameters.AddWithValue("@xx" + i, chunk[i].XxthId);
                }
                cmd.CommandText =
                    "SELECT Id, SanPhamId, XXTHId FROM dbo.TieuChiChatLuong WHERE " +
                    string.Join(" OR ", conds);

                using var rd = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
                while (await rd.ReadAsync(ct).ConfigureAwait(false))
                {
                    list.Add(new ExistingTieuChiChatLuong
                    {
                        Id = rd.GetInt32(0),
                        SanPhamId = rd.GetInt32(1),
                        XxthId = rd.GetInt32(2)
                    });
                }
            }
            return list;
        }

        private bool? _hasTolColumns;

        /// <summary>
        /// Ghi một bản ghi vào TieuChiChatLuong, khoá theo (XXTHId, SanPhamId), cùng dữ liệu như file CSV.
        /// Chưa có thì thêm mới. Đã có thì chỉ cập nhật khi <paramref name="overwrite"/> = true;
        /// không ghi đè thì trả về 0.
        /// </summary>
        public async Task<int> SaveAsync(TieuChiChatLuongRecord rec, bool overwrite, CancellationToken ct = default)
        {
            using var cn = await OpenAsync(ct).ConfigureAwait(false);
            await LoadSchemaAsync(cn, ct).ConfigureAwait(false);
            EnsureXxthColumn();

            _hasTolColumns ??=
                await ColumnExistsAsync(cn, "TieuChiChatLuong", "MaxTolerance", ct).ConfigureAwait(false) &&
                await ColumnExistsAsync(cn, "TieuChiChatLuong", "MinTolerance", ct).ConfigureAwait(false);

            var tolUpdate = _hasTolColumns == true ? ", MaxTolerance = @maxTol, MinTolerance = @minTol" : "";
            var tolCols = _hasTolColumns == true ? ", MaxTolerance, MinTolerance" : "";
            var tolVals = _hasTolColumns == true ? ", @maxTol, @minTol" : "";

            var sql = $@"
MERGE dbo.TieuChiChatLuong AS tgt
USING (SELECT @xxthId AS XXTHId, @sanPhamId AS SanPhamId) AS src
   ON (tgt.XXTHId = src.XXTHId AND tgt.SanPhamId = src.SanPhamId)
WHEN MATCHED AND @overwrite = 1 THEN
    UPDATE SET FormType = @formType, FilePath = @filePath, TieuChiData = @data,
               UpdatedDate = ISNULL(@updateDate, GETDATE()){tolUpdate}
WHEN NOT MATCHED THEN
    INSERT (XXTHId, SanPhamId, FormType, FilePath, TieuChiData, UpdatedDate{tolCols})
    VALUES (@xxthId, @sanPhamId, @formType, @filePath, @data, ISNULL(@updateDate, GETDATE()){tolVals})
OUTPUT INSERTED.Id;";

            using var cmd = new SqlCommand(sql, cn);
            cmd.Parameters.AddWithValue("@xxthId", rec.XxthId);
            cmd.Parameters.AddWithValue("@sanPhamId", rec.SanPhamId);
            cmd.Parameters.AddWithValue("@overwrite", overwrite);
            cmd.Parameters.AddWithValue("@formType", rec.FormType);
            cmd.Parameters.AddWithValue("@filePath", rec.FilePath);
            cmd.Parameters.AddWithValue("@data", rec.TieuChiData);
            cmd.Parameters.Add("@updateDate", System.Data.SqlDbType.DateTime).Value =
                (object?)rec.UpdateDate ?? DBNull.Value;
            if (_hasTolColumns == true)
            {
                cmd.Parameters.AddWithValue("@maxTol", (object?)rec.MaxTolerance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@minTol", (object?)rec.MinTolerance ?? DBNull.Value);
            }

            var id = await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false);
            return id is null ? 0 : Convert.ToInt32(id);
        }

        // ---------------------------------------------------------------- tiện ích

        private static string? Str(SqlDataReader rd, int i) => rd.IsDBNull(i) ? null : rd.GetString(i);

        private static decimal? Dec(SqlDataReader rd, int i)
        {
            if (rd.IsDBNull(i)) return null;
            var v = rd.GetValue(i);
            return v is decimal d ? d : Convert.ToDecimal(v);
        }
    }
}
