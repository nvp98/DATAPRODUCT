using Data_Product.Common;
using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class XuLyPhieuController : Controller
    {
        private readonly DataContext _context;
        //private readonly BM_11Controller _11Controller;

        public XuLyPhieuController(DataContext _context)
        {
            this._context = _context;
        }
        public async Task<IActionResult> Index(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;
            //DateTime startDay = Now.AddDays(-1);
            //DateTime endDay = Now;
            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN= TaiKhoan.ID_TaiKhoan;
            //var selec = new List<Tbl_TrangThai_PheDuyet>
            //{
            //    new Tbl_TrangThai_PheDuyet { ID_TrangThai_PheDuyet = 0, TenTrangThai = "Chưa xử lý" },
            //    new Tbl_TrangThai_PheDuyet { ID_TrangThai_PheDuyet = 1, TenTrangThai = "Đã xử lý" },
            //    new Tbl_TrangThai_PheDuyet { ID_TrangThai_PheDuyet = 2, TenTrangThai = "Hủy phiếu" },
            //};
            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BN == ID_NhanVien_BN && x.ID_TrangThai_BG == 1 && !x.IsDelete)
                             select new Tbl_BienBanGiaoNhan
                             {
                                 ID_BBGN = a.ID_BBGN,
                                 ID_NhanVien_BG = a.ID_NhanVien_BG,
                                 ID_PhongBan_BG = a.ID_PhongBan_BG,
                                 ID_Xuong_BG = a.ID_Xuong_BG,
                                 ID_ViTri_BG = a.ID_ViTri_BG,
                                 ThoiGianXuLyBG = (DateTime)a.ThoiGianXuLyBG,
                                 ID_TrangThai_BG = a.ID_TrangThai_BG,
                                 ID_NhanVien_BN = a.ID_NhanVien_BN,
                                 ID_PhongBan_BN = a.ID_PhongBan_BN,
                                 ID_Xuong_BN = a.ID_Xuong_BN,
                                 ID_ViTri_BN = a.ID_ViTri_BN,
                                 ThoiGianXuLyBN = (DateTime?)a.ThoiGianXuLyBN?? default,
                                 ID_TrangThai_BN = a.ID_TrangThai_BN,
                                 SoPhieu = a.SoPhieu,
                                 ID_TrangThai_BBGN = a.ID_TrangThai_BBGN,
                                 ID_QuyTrinh = a.ID_QuyTrinh,
                                 NoiDungTrichYeu =a.NoiDungTrichYeu,
                                 NgayTao = a.NgayTao
                             }).OrderBy(x=>x.ID_TrangThai_BN).ThenByDescending(x=>x.NgayTao).ToListAsync();
            //if (res != null)
            //{
            //    foreach (var item in res)
            //    {
            //        if (item.ID_QuyTrinh != 1)
            //        {
            //            var TrinhKyBS = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == item.ID_BBGN && x.ID_TrangThai == 2).FirstOrDefault();
            //            if (TrinhKyBS == null) res = res.Where(a=>a.ID_BBGN != item.ID_BBGN).ToList();
            //        }
            //    }
            //}

            if (ID_TrangThai != null) res = res.Where(x => x.ID_TrangThai_BN == ID_TrangThai).ToList();
            if(begind != null && endd != null) res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
           
            const int pageSize = 1000;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }
        public async Task<IActionResult> Phieudentoi(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", ID_BBGN.ID_PhongBan_BN);

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", ID_BBGN.ID_NhanVien_BN);

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");
            ViewBag.Data = id;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Phieudentoi(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, int? id)
        {
            string XacNhan = "";
            string YKienHieuChinh = "";
            DateTime ThoiGianXuLyBN = DateTime.Now;
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        XacNhan = formCollection["xacnhan"];
                        YKienHieuChinh = formCollection["ykien"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan" && key.Key != "ykien" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                            KhoiLuong_BN =double.TryParse(formCollection["khoiluongbn_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                            //KL_QuyKho_BN = double.Parse(formCollection["quykhobn_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }
                }
                if (XacNhan == "0" && XacNhan != "") // BN hủy phiếu
                {
                    if(YKienHieuChinh!= "")
                    {
                        var result_ykien = _context.Database.ExecuteSqlRaw("EXEC Tbl_YeuCauHieuChinh_insert {0},{1}", YKienHieuChinh, id);
                    }
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        //double KL_QuyKhoNhan = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }
                    //var result_yeucau = _context.Database.ExecuteSqlRaw("EXEC Tbl_YeuCauHieuChinh_insert {0},{1}", YKienHieuChinh, id);

                    //var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    //Bên nhận hủy phiếu
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 2);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);
                    // đặt trạng thái hủy phiếu cho phiếu bố sung
                    //if(ID_BBGN.ID_QuyTrinh != 1)
                    //{
                    //    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    //    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 3);
                    //    var re = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, ThoiGianXuLyBN);
                    //}

                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "") // xác nhận phiếu
                {
                    var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        //double KL_QuyKhoNhan = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }
                    if (ID_BBGN.ID_QuyTrinh == 1)
                    {
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);

                        var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);

                        var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    }
                    else
                    {
                        var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);

                        var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);

                        //var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    }
                   

                    TempData["msgSuccess"] = "<script>alert('xác nhận thành công');</script>";
                }

            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
                return RedirectToAction("Phieudentoi", "XuLyPhieu", new {id = id});
            }
            return RedirectToAction("Index", "XuLyPhieu");
        }


        public async Task<IActionResult> YCauHieuChinh(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            var TrinhKy = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", ID_BBGN.ID_PhongBan_BN);

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", ID_BBGN.ID_NhanVien_BN);

            if(TrinhKy == null)
            {
                var NhanVien_TT = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                         join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");



                var NhanVien_TT_View = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN)
                                         join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen");
            }    
            else
            {
                var NhanVien_TT = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                         join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen", TrinhKy.ID_TaiKhoan);



                var NhanVien_TT_View = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == ID_BBGN.ID_Xuong_BN)
                                              join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                              select new Tbl_TaiKhoan
                                              {
                                                  ID_TaiKhoan = a.ID_TaiKhoan,
                                                  HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                              }).ToListAsync();

                ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen", TrinhKy.ID_TaiKhoan_View);
            }    


            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();
            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");


            var MaLo = await (from a in _context.Tbl_MaLo
                              select new Tbl_MaLo
                              {
                                  ID_MaLo = a.ID_MaLo,
                                  TenMaLo = a.TenMaLo
                              }).ToListAsync();


            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");
            ViewBag.Data = id;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YCauHieuChinh(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, int? id)
        {
            string XacNhan = "";
            int IDNhanVienTT = 0;
            int IDNhanVien_TT_View = 0;
            int BBGN_IDNew = 0;
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
                XacNhan = formCollection["xacnhan"];
                if (XacNhan == "0" && XacNhan != "")  // Hủy
                {
                   
                    TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
                    return RedirectToAction("Index", "BM_11");
                }
                else if (XacNhan == "1" && XacNhan != "") // Gửi
                {
                    // Thông tin phê duyệt nhân viên thống kê
                    if (formCollection["NhanVienTT"] == "")
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                        return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = id });
                    }
                    else if (formCollection["NhanVien_TT_View"] == "")
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê nhận BBGN');</script>";
                        return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = id });
                    }
                    // lấy dữ liệu từ form
                    foreach (var key in formCollection.ToList())
                    {
                        if (key.Key != "__RequestVerificationToken")
                        {
                            XacNhan = formCollection["xacnhan"];
                            IDNhanVienTT = Convert.ToInt32(formCollection["NhanVienTT"]);
                            IDNhanVien_TT_View = Convert.ToInt32(formCollection["NhanVien_TT_View"]);
                        }
                        if (key.Key != "__RequestVerificationToken" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan"
                           && key.Key != "NhanVienTT" && key.Key != "NhanVien_TT_View" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                        {
                            Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                            {
                                //ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                                ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                                MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                                DoAm_W = double.TryParse(formCollection["doam_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0,
                                KhoiLuong_BG = double.TryParse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                                //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                                GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                            });
                        }

                    }
                   

                    // Tạo phiếu hiệu chỉnh
                    // Phát sinh phiếu mới
                    var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault(); //Phiếu cũ
                    var checkSPhieu = ID_BBGN.SoPhieu.Split("_")[0];
                    var ID_BBGNHC = _context.Tbl_BienBanGiaoNhan.Where(x => x.SoPhieu.Contains(checkSPhieu + "_HC")).ToList();
                    //var ID_Kip = _context.Tbl_Kip.Where(x => x.ID_Kip == ID_BBGN.ID_Kip).FirstOrDefault();
                    // Thông tin bên giao
                    var ThongTin_BG = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BG).FirstOrDefault();
                    var ThongTin_BP_BG = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BG.ID_PhongBan).FirstOrDefault();
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == ID_BBGN.ID_NhanVien_BN).FirstOrDefault();
                    var ThongTin_BP_BN = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == ThongTin_BN.ID_PhongBan).FirstOrDefault();

                    DateTime Day = (DateTime)ID_BBGN.ThoiGianXuLyBG;
                    string Day_Convert = Day.ToString("dd-MM-yyyy");
                    DateTime ThoiGianXuLyBG = DateTime.ParseExact(Day_Convert, "dd-MM-yyyy", CultureInfo.InvariantCulture);

                    int so = ID_BBGNHC.Count() + 1;
                    string SoPhieu = checkSPhieu.Trim() + "_HC." + so;

                    var Output_ID_BBGN = new SqlParameter
                    {
                        ParameterName = "ID_BBGN",
                        SqlDbType = System.Data.SqlDbType.Int,
                        Direction = System.Data.ParameterDirection.Output,
                    };

                    //// file đính kèm
                    //if (FileDinhKem != null && FileDinhKem.Length > 0)
                    //{
                    //    // Lấy tên file gốc và phần mở rộng
                    //    var originalFileName = Path.GetFileNameWithoutExtension(FileDinhKem.FileName);
                    //    var fileExtension = Path.GetExtension(FileDinhKem.FileName);

                    //    // Tạo tên file mới với thời gian
                    //    var newFileName = $"{originalFileName}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";

                    //    // Đường dẫn lưu file (thư mục cần tồn tại trước)
                    //    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", newFileName);

                    //    // Lưu file vào server
                    //    using (var stream = new FileStream(path, FileMode.Create))
                    //    {
                    //        FileDinhKem.CopyTo(stream);
                    //    }

                    //    // Lưu path vào DB
                    //    if (path != "" && path != null)
                    //    {
                    //        // lưu vào đường dẫn tương đối
                    //        filedk = $"/uploads/{newFileName}";
                    //    }

                    //    //ViewBag.Message = "File uploaded successfully: " + fileName;
                    //}

                    var result_new = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},@ID_BBGN OUTPUT",
                                                          ThongTin_BG.ID_TaiKhoan, ThongTin_BG.ID_PhongBan, ThongTin_BG.ID_PhanXuong, ThongTin_BG.ID_ChucVu, ThoiGianXuLyBG, 1,
                                                          ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu, 0, SoPhieu, ID_BBGN.ID_Kip, 0, 3, ID_BBGN.Kip, ID_BBGN.Ca, ID_BBGN.NoiDungTrichYeu,ID_BBGN.FileDinhKem, Output_ID_BBGN);
                    BBGN_IDNew = Convert.ToInt32(Output_ID_BBGN.Value);

                    // Update ID phiếu cũ
                    var result_update = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update_ID {0},{1}", BBGN_IDNew, id);

                    // insert ChiTiet_BBGN
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {

                        if (BBGN_IDNew != 0)
                        {
                            double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                            //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                            double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                            var result_Vitri = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                       item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu, BBGN_IDNew);

                        }

                    }

                    //update BBGN cũ
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 3); // cập nhật tình trạng phiếu cũ thành ĐNHC

                    var checkBS = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == BBGN_IDNew).FirstOrDefault();


                    if (checkBS == null)
                    {
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", BBGN_IDNew, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);
                    }
                    else
                    {
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", checkBS.ID_TrinhKy, IDNhanVienTT, IDNhanVien_TT_View, 0);
                    }

                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";

                    return RedirectToAction("Index_Detai", "BM_11", new { id = BBGN_IDNew });
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
                return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new { id = id });
            }

            return RedirectToAction("YCauHieuChinh", "XuLyPhieu", new {id = id});
        }

        public async Task<IActionResult> Phieudentoi_CauHieuChinh(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", ID_BBGN.ID_PhongBan_BN);

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", ID_BBGN.ID_NhanVien_BN);

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");
            ViewBag.Data = id;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Phieudentoi_CauHieuChinh(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, int? id)
        {
            string XacNhan = "";
            string YKienHieuChinh = "";
            int BBGN_ID = 0;
            DateTime ThoiGianXuLyBN = DateTime.Now;
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        XacNhan = formCollection["xacnhan"];
                        YKienHieuChinh = formCollection["ykien"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "xacnhan" && key.Key != "ykien" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                            //KhoiLuong_BN = double.Parse(formCollection["khoiluongbn_" + key.Key.Split('_')[1]]),
                            KhoiLuong_BN = double.TryParse(formCollection["khoiluongbn_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                            //KL_QuyKho_BN = double.Parse(formCollection["quykhobn_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }

                }
                if (XacNhan == "0" && XacNhan != "") // Không xác nhận
                {
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        //double KL_QuyKhoNhan = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }

                    //var result_yeucau = _context.Database.ExecuteSqlRaw("EXEC Tbl_YeuCauHieuChinh_insert {0},{1}", YKienHieuChinh, id);

                    //Thông tin biển bản giao nhận
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 2);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);
                    var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    // đặt trạng thái hủy phiếu cho phiếu bố sung
                    if (ID_BBGN.ID_QuyTrinh != 1)
                    {
                        var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 3);
                        var re = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, ThoiGianXuLyBN);
                    }
                    if (ID_BBGN.ID_QuyTrinh == 3 && ID_BBGN.ID_BBGN_Cu != null)
                    {
                        // cập nhật phiếu cũ về trạng thái hoàn tất
                        var phieucu = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", ID_BBGN.ID_BBGN_Cu, 1);
                    }
                       
                    TempData["msgSuccess"] = "<script>alert('Thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "") // Xác nhận
                {
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        //double KL_QuyKhoNhan = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);
                    //var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);

                    TempData["msgSuccess"] = "<script>alert('xác nhận thành công');</script>";
                }
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chính sửa thất bại');</script>";
                return RedirectToAction("Phieudentoi_CauHieuChinh", "XuLyPhieu", new { id = id });
            }
            return RedirectToAction("Index", "XuLyPhieu");
        }


        public async Task<IActionResult> PhieuBoSung(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;

            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);

            var res = await (from a in _context.Tbl_TrinhKyBoSung.Where(x => x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan)
                             join bb in _context.Tbl_BienBanGiaoNhan.Where(x=>x.ID_TrangThai_BG ==1 && x.ID_TrangThai_BN ==1 && x.ID_QuyTrinh != 1 && !x.IsDelete) on a.ID_BBGN equals bb.ID_BBGN
                             select new Tbl_TrinhKyBoSung
                             {
                                 ID_TrinhKy = a.ID_TrinhKy,
                                 ID_BBGN = a.ID_BBGN,
                                 SoPhieu = bb.SoPhieu,
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                                 NgayXuLy = (DateTime)a.NgayXuLy,
                                 ID_TrangThai = a.ID_TrangThai
                             }).OrderBy(x=>x.ID_TrangThai).OrderByDescending(x => x.NgayTrinhKy).ToListAsync();

            if (ID_TrangThai != null) res = res.Where(x => x.ID_TrangThai == ID_TrangThai).ToList();
            if (begind != null && endd != null) res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay).ToList();
            //if (begind == null && endd == null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay).ToList();
            //}
            //else if (begind != null && endd != null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd).ToList();
            //}
            //else if (begind != null && endd != null && ID_TrangThai != null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.ID_TrangThai == ID_TrangThai).ToList();
            //}
            const int pageSize = 20;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }

        public async Task<IActionResult> PKHXoaPhieu(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = Now;

            if (begind != null) startDay = (DateTime)begind;
            if (endd != null) endDay = (DateTime)endd;

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);

            var res = await (from a in _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_TaiKhoanKH == TaiKhoan.ID_TaiKhoan && x.TinhTrang_BN ==1)
                             join bb in _context.Tbl_BienBanGiaoNhan.Where(x =>  x.ID_TrangThai_BBGN == 5) on a.ID_BBGN equals bb.ID_BBGN
                             select new Tbl_XuLyXoaPhieu
                             {
                                 ID = a.ID,
                                 ID_TaiKhoanBN = a.ID_TaiKhoanBN,
                                 TinhTrang_BN =a.TinhTrang_BN,
                                 ID_TaiKhoanKH =a.ID_TaiKhoanKH,
                                 TinhTrang_KH =a.TinhTrang_KH,
                                 NgayXuLy_BN =a.NgayXuLy_BN,
                                 NgayXuLy_KH =a.NgayXuLy_KH,
                                 ID_BBGN = a.ID_BBGN,
                                 ID_TrangThai = a.TinhTrang_KH
                             }).OrderBy(x=>x.TinhTrang_KH).OrderByDescending(x => x.NgayXuLy_KH).ToListAsync();

            if (ID_TrangThai != null) res = res.Where(x => x.ID_TrangThai == ID_TrangThai).ToList();
            if (begind != null && endd != null) res = res.Where(x => x.NgayXuLy_KH >= startDay && x.NgayXuLy_KH <= endDay).ToList();
            //if (begind == null && endd == null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay).ToList();
            //}
            //else if (begind != null && endd != null && ID_TrangThai == null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd).ToList();
            //}
            //else if (begind != null && endd != null && ID_TrangThai != null)
            //{
            //    res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.ID_TrangThai == ID_TrangThai).ToList();
            //}
            const int pageSize = 20;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }

        public async Task<IActionResult> Chitiet_PhieuBoSung(int id)
        {
            var res = await (from a in _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id)
                             join vt in _context.Tbl_VatTu on a.ID_VatTu equals vt.ID_VatTu
                             select new Tbl_ChiTiet_BienBanGiaoNhan
                             {
                                 ID_CT_BBGN = a.ID_CT_BBGN,
                                 ID_VatTu = a.ID_VatTu,
                                 TenVatTu = vt.TenVatTu,
                                 DonViTinh = vt.DonViTinh,
                                 MaLo = a.MaLo,
                                 DoAm_W = (double)a.DoAm_W,
                                 KhoiLuong_BG = (double)a.KhoiLuong_BG,
                                 KL_QuyKho_BG = (double)a.KL_QuyKho_BG,
                                 KhoiLuong_BN = (double)a.KhoiLuong_BN,
                                 KL_QuyKho_BN = (double)a.KL_QuyKho_BN,
                                 GhiChu = a.GhiChu,
                                 ID_BBGN = a.ID_BBGN
                             }).ToListAsync();
            ViewBag.Data = id;
            return View(res);
        }

        public async Task<IActionResult> SuaPhieu_PhieuBoSung(int? id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            var ID_BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
            var ID_TrinhKy = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();

            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan", ID_BBGN.ID_PhongBan_BN);

            var NhanVien = await (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen", ID_BBGN.ID_NhanVien_BN);

            var VatTu = await (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP))
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToListAsync();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");


            var NhanVienTT = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                     join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                     select new Tbl_TaiKhoan
                                     {
                                         ID_TaiKhoan = a.ID_TaiKhoan,
                                         HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                     }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVienTT, "ID_TaiKhoan", "HoVaTen", ID_TrinhKy.ID_TaiKhoan);


            var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                          select new Tbl_TaiKhoan
                                          {
                                              ID_TaiKhoan = a.ID_TaiKhoan,
                                              HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                          }).ToListAsync();

            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen", ID_TrinhKy.ID_TaiKhoan_View);

            var MaLo = await (from a in _context.Tbl_MaLo
                              select new Tbl_MaLo
                              {
                                  ID_MaLo = a.ID_MaLo,
                                  TenMaLo = a.TenMaLo
                              }).ToListAsync();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");



            ViewBag.Data = id;
            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SuaPhieu_PhieuBoSung(Tbl_ChiTiet_BienBanGiaoNhan _DO, IFormCollection formCollection, int? id)
        {
            int IDTaiKhoan = 0;
            int NhanVienTT = 0;
            int IDNhanVien_TT_View = 0;
            string XacNhan = "";
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
                foreach (var key in formCollection.ToList())
                {
                    if (key.Key != "__RequestVerificationToken")
                    {
                        IDTaiKhoan = Convert.ToInt32(formCollection["IDTaiKhoan"]);
                        NhanVienTT = Convert.ToInt32(formCollection["NhanVienTT"]);
                        IDNhanVien_TT_View = Convert.ToInt32(formCollection["NhanVien_TT_View"]);
                        XacNhan = formCollection["xacnhan"];
                    }
                    if (key.Key != "__RequestVerificationToken" && key.Key != "IDTaiKhoan" && key.Key != "xacnhan"
                        && key.Key != "NhanVienTT" && key.Key != "NhanVien_TT_View" && key.Key == "ghichu_" + key.Key.Split('_')[1])
                    {
                        Tbl_ChiTiet_BienBanGiaoNhan.Add(new Tbl_ChiTiet_BienBanGiaoNhan()
                        {
                            ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                            ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                            MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                            DoAm_W = double.TryParse(formCollection["doam_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var value) ? value : 0,
                            KhoiLuong_BG = double.TryParse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]], NumberStyles.Any, CultureInfo.InvariantCulture, out var s) ? s : 0,
                            //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }

                }
                // Thông tin BBGB
                var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                if (XacNhan == "0" && XacNhan != "")
                {
                   
                    // Thông tin nhân viên thống kê phê duyệt
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, NhanVienTT, IDNhanVien_TT_View, 0);
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();

                    var result_tt = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update {0},{1},{2},{3},{4}", id,
                                                            ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu);
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT_BBGN = item.ID_CT_BBGN - 100;
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == ID_CT_BBGN).FirstOrDefault();

                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);

                        if (ID_CT == null)
                        {
                            if (item.MaLo != "" && item.MaLo != null )
                            {
                                //int Ma_Lo = Convert.ToInt32(item.MaLo);
                                //var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                                item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);

                            }
                            else
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                              item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);
                            }
                           
                        }
                        else
                        {

                            if (item.ID_VatTu == 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    //int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    //var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         ID_CT.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                             

                            }
                            else
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    //int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    //var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       item.ID_VatTu,"", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                             
                            }
                        }

                    }
                    TempData["msgSuccess"] = "<script>alert('Lưu thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "") // gửi phiếu
                {
                    // Thông tin nhân viên thống kê phê duyệt
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, NhanVienTT, IDNhanVien_TT_View, 1); // gửi PKH
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();

                    var result_tt = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update {0},{1},{2},{3},{4}", id,
                                                            ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu);
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT_BBGN = item.ID_CT_BBGN - 100;
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == ID_CT_BBGN).FirstOrDefault();

                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        //double KL_QuyKho = Math.Round(QuyKho + 0.00001, 3, MidpointRounding.AwayFromZero);
                        double KL_QuyKho = Math.Round(AdjustIfLastDigitIsFive(QuyKho, 3), 3, MidpointRounding.ToEven);
                        if (ID_CT == null)
                        {
                            if (item.MaLo != "" && item.MaLo != null)
                            {

                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                            item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);

                            }
                            else
                            {
                                var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);
                            }
                        }
                        else
                        {

                            if (item.ID_VatTu == 0)
                            {
                                if (item.MaLo != "" && item.MaLo != null )
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       ID_CT.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                             

                            }
                            else
                            {
                                if (item.MaLo != "" && item.MaLo != null)
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                     item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                            
                            }
                        }

                    }
                    // update Trạng thái BG
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 1);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 0);

                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
                    return RedirectToAction("Index_Detai", "BM_11", new { id = id });
                }


            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Chỉnh sửa thất bại');</script>";
            }
            return RedirectToAction("SuaPhieu_PhieuBoSung", "XuLyPhieu", new { id = id });
        }
        public async Task<IActionResult> HuyPhieu(int id)
        {
            try
            {
                DateTime NgayXuLy = DateTime.Now;
                var ID_BB = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                
                //Thông tin nhân viên thông kê phê duyệt
                var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 2);
                var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                //Thông tin biển bản giao nhận
                //var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 4);
                //var result_BN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 4);
                var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 4);
                //Thông tin phiếu cũ
                // cập nhật phiếu cũ về trạng thái hoàn tất khi đề nghị hiệu chỉnh 
                if(ID_BB.ID_QuyTrinh == 3 && ID_BB.ID_BBGN_Cu != null)
                {
                    var phieucu = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", ID_BB.ID_BBGN_Cu, 1);
                }
                

                TempData["msgSuccess"] = "<script>alert('Hủy phiếu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy phiếu thất bại');</script>";
            }
            return RedirectToAction("PhieuBoSung", "XuLyPhieu");
        }
        public async Task<IActionResult> XacNhanPhieu(int id)
        {
            try
            {
                DateTime NgayXuLy = DateTime.Now;
                var ID_BB = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                //Thông tin nhân viên thông kê phê duyệt
                var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 1);
                var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                //Cập nhật biển bản giao nhận
                var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                //var savpdf = _11Controller.SavePdf(id);
                TempData["msgSuccess"] = "<script>alert('Xác nhận phiếu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xác nhận phiếu thất bại');</script>";
            }
            return RedirectToAction("PhieuBoSung", "XuLyPhieu");
        }

        public async Task<IActionResult> BN_XoaPhieu(int id,int tinhtrang)
        {
            try
            {
                if (tinhtrang == 0) //không xóa
                {
                    var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    _context.Tbl_XuLyXoaPhieu.Remove(xoaphieu);
                    //xoaphieu.TinhTrang_BN = 0;
                    //xoaphieu.NgayXuLy_BN = DateTime.Now;
                    _context.SaveChanges();
                }
                else
                {
                    //var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    xoaphieu.TinhTrang_BN = 1;
                    xoaphieu.NgayXuLy_BN = DateTime.Now;
                    _context.SaveChanges();
                }

                TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy phiếu thất bại');</script>";
            }
            return RedirectToAction("Index", "XuLyPhieu");
        }
        public async Task<IActionResult> KH_XoaPhieu(int id, int tinhtrang)
        {
            try
            {
                if (tinhtrang == 0) //không xóa
                {
                    var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    _context.Tbl_XuLyXoaPhieu.Remove(xoaphieu);
                    //xoaphieu.TinhTrang_KH = 0;
                    //xoaphieu.NgayXuLy_KH = DateTime.Now;
                    _context.SaveChanges();
                }
                else
                {
                    Tbl_BienBanGiaoNhan bbgn = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    bbgn.IsDelete = true;
                    //var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                    Tbl_XuLyXoaPhieu xoaphieu = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    xoaphieu.TinhTrang_KH = 1;
                    xoaphieu.NgayXuLy_KH = DateTime.Now;
                    _context.SaveChanges();
                }

                TempData["msgSuccess"] = "<script>alert('Xác nhận thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Hủy phiếu thất bại');</script>";
            }
            return RedirectToAction("PKHXoaPhieu", "XuLyPhieu");
        }

        public async Task<IActionResult> PhieuNhanThongTin(DateTime? begind, DateTime? endd, int? ID_TrangThai, int page = 1)
        {
            DateTime Now = DateTime.Now;
            DateTime startDay = new DateTime(Now.Year, Now.Month, 1);
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_TrinhKyBoSung.Where(x => x.ID_TaiKhoan_View == TaiKhoan.ID_TaiKhoan && x.ID_TrangThai != 0)
                             join bb in _context.Tbl_BienBanGiaoNhan.Where(x=>!x.IsDelete) on a.ID_BBGN equals bb.ID_BBGN
                             select new Tbl_TrinhKyBoSung
                             {
                                 ID_TrinhKy = a.ID_TrinhKy,
                                 ID_BBGN = a.ID_BBGN,
                                 SoPhieu = bb.SoPhieu,
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                                 NgayXuLy = (DateTime)a.NgayXuLy,
                                 ID_TrangThai = a.ID_TrangThai
                             }).OrderByDescending(x => x.NgayTrinhKy).ToListAsync();

            var dsPhieuXoa = await (from a in  _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_TaiKhoanKH_View == TaiKhoan.ID_TaiKhoan)
                             join b in _context.Tbl_BienBanGiaoNhan on a.ID_BBGN equals b.ID_BBGN
                             select new Tbl_TrinhKyBoSung
                             {
                                 ID_TrinhKy = a.ID,
                                 ID_BBGN = a.ID_BBGN,
                                 SoPhieu = b.SoPhieu,
                                 ID_TaiKhoan = a.ID_TaiKhoanKH,
                                 NgayTrinhKy = (DateTime)b.NgayTao,
                                 NgayXuLy = (DateTime)a.NgayXuLy_BN,
                                 ID_TrangThai = a.ID_TrangThai
                             }).OrderByDescending(x => x.NgayTrinhKy).ToListAsync();
            if(dsPhieuXoa != null)
            {
                res.AddRange(dsPhieuXoa.ToList());
            }


            if (begind == null && endd == null && ID_TrangThai == null)
            {
                res = res.Where(x => x.NgayTrinhKy >= startDay && x.NgayTrinhKy <= endDay).ToList();
            }
            else if (begind != null && endd != null && ID_TrangThai == null)
            {
                res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd).ToList();
            }
            else if (begind != null && endd != null && ID_TrangThai != null)
            {
                res = res.Where(x => x.NgayTrinhKy >= begind && x.NgayTrinhKy <= endd && x.ID_TrangThai == ID_TrangThai).ToList();
            }
            const int pageSize = 1000;
            if (page < 1)
            {
                page = 1;
            }
            int resCount = res.Count;
            var pager = new Pager(resCount, page, pageSize);
            int recSkip = (page - 1) * pageSize;
            var data = res.Skip(recSkip).Take(pager.PageSize).ToList();
            this.ViewBag.Pager = pager;
            return View(data);
        }

        [HttpPost]
        public IActionResult ProcessSelected(List<int> selectedItems)
        {
            if (selectedItems != null && selectedItems.Any())
            {
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                // Xử lý danh sách ID được chọn
                foreach (var id in selectedItems)
                {
                    // Lưu vào database
                    _context.Tbl_PKHXuLyPhieu.Add(new Tbl_PKHXuLyPhieu
                    {
                        ID_BBGN = id,
                        ID_TaiKhoan = TaiKhoan.ID_TaiKhoan,
                        NgayXuLy = DateTime.Now,
                        TinhTrang = true
                    });
                }
                _context.SaveChanges();
            }

            return RedirectToAction("PhieuNhanThongTin", "XuLyPhieu");
        }

        


        public async Task<IActionResult> HuyPhieu_PheDuyet(int id)
        {
            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var BBGN = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).FirstOrDefault();
            var NhanVien_TT = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == TaiKhoan.ID_PhanXuong)
                                     join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                     select new Tbl_TaiKhoan
                                     {
                                         ID_TaiKhoan = a.ID_TaiKhoan,
                                         HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                     }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");

            var NhanVien_TTView = await (from a in _context.Tbl_ThongKeXuong.Where(x => x.ID_Xuong == BBGN.ID_Xuong_BN)
                                     join b in _context.Tbl_TaiKhoan on a.ID_TaiKhoan equals b.ID_TaiKhoan
                                     select new Tbl_TaiKhoan
                                     {
                                         ID_TaiKhoan = a.ID_TaiKhoan,
                                         HoVaTen = b.TenTaiKhoan + " - " + b.HoVaTen
                                     }).ToListAsync();

            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TTView, "ID_TaiKhoan", "HoVaTen");
            ViewBag.NhanVienBN = new SelectList(_context.Tbl_TaiKhoan.Where(x=>x.ID_TaiKhoan == BBGN.ID_NhanVien_BN), "ID_TaiKhoan", "HoVaTen");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu_PheDuyet(Tbl_XuLyXoaPhieu _DO, int id)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                if(_DO.ID_TaiKhoanKH != 0 && _DO.ID_TaiKhoanKH_View != null)
                {
                    var check = _context.Tbl_XuLyXoaPhieu.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    if (check != null)
                    {
                        check.ID_TaiKhoanBN = _DO.ID_TaiKhoanBN;
                        check.ID_TaiKhoanKH = _DO.ID_TaiKhoanKH;
                        check.TinhTrang_BN = 0;
                        check.TinhTrang_KH = 0;
                        check.ID_BBGN = id;
                        check.ID_TrangThai = 0;
                        check.ID_TaiKhoanKH_View = _DO.ID_TaiKhoanKH_View;
                    }
                    else
                    {
                        Tbl_XuLyXoaPhieu phieu = new Tbl_XuLyXoaPhieu()
                        {
                            ID_BBGN = id,ID_TaiKhoanBN=_DO.ID_TaiKhoanBN,ID_TaiKhoanKH=_DO.ID_TaiKhoanKH,TinhTrang_BN =0,TinhTrang_KH =0,ID_TrangThai=0,ID_TaiKhoanKH_View =_DO.ID_TaiKhoanKH_View
                       };
                        _context.Tbl_XuLyXoaPhieu.Add(phieu);
                    }
                    _context.SaveChanges();
                    var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 5);
                }
                else
                {
                    TempData["msgError"] = "<script>alert('Vui lòng chọn Thống kê phê duyệt và nhận thông tin');</script>";
                    return RedirectToAction("Index_Detai", "BM_11", new {id= id });
                }
                
                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Trình ký thất bại');</script>";
            }

            return RedirectToAction("Index", "BM_11");
        }

        static double AdjustIfLastDigitIsFive(double number, int precision)
        {
            // Tăng độ chính xác (dịch dấu thập phân để lấy chữ số cuối)
            double multiplier = Math.Pow(10, precision + 1);
            double shiftedNumber = number * multiplier;

            // Lấy phần nguyên để kiểm tra chữ số cuối
            int lastDigit = (int)Math.Abs(shiftedNumber) % 10;

            // Nếu chữ số cuối là 5, cộng thêm một lượng nhỏ
            if (lastDigit == 5)
            {
                double adjustment = 1 / multiplier;
                number += adjustment; // Cộng thêm lượng nhỏ
            }

            return number;
        }

        [HttpPost]
        public IActionResult ProcessSelectedXLPhieu(List<int> selectedCheck)
        {
            if (selectedCheck != null && selectedCheck.Any())
            {
                var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
                var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
                // Xử lý danh sách ID được chọn
                foreach (var id in selectedCheck)
                {
                    // Lưu vào database
                    _context.Tbl_PKHXuLyPhieu.Add(new Tbl_PKHXuLyPhieu
                    {
                        ID_BBGN = id,
                        ID_TaiKhoan = TaiKhoan.ID_TaiKhoan,
                        NgayXuLy = DateTime.Now,
                        TinhTrang = true
                    });
                }
                _context.SaveChanges();
            }

            return RedirectToAction("PhieuBoSung", "XuLyPhieu");
        }

    }
}
