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
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            int ID_NhanVien_BN= TaiKhoan.ID_TaiKhoan;

            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai.ToList(), "ID_TrangThai", "TenTrangThai", ID_TrangThai);
            var res = await (from a in _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_NhanVien_BN == ID_NhanVien_BN && x.ID_TrangThai_BG == 1)
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
                                 ID_QuyTrinh = a.ID_QuyTrinh
                             }).ToListAsync();

            if(ID_TrangThai != null) res = res.Where(x => x.ID_TrangThai_BBGN == ID_TrangThai).ToList();
            if(begind != null && endd != null) res = res.Where(x => x.ThoiGianXuLyBG >= startDay && x.ThoiGianXuLyBG <= endDay).ToList();
           
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
                            KhoiLuong_BN = double.Parse(formCollection["khoiluongbn_" + key.Key.Split('_')[1]]),
                            //KL_QuyKho_BN = double.Parse(formCollection["quykhobn_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }
                }
                if (XacNhan == "0" && XacNhan != "")
                {
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);

                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }
                    var result_yeucau = _context.Database.ExecuteSqlRaw("EXEC Tbl_YeuCauHieuChinh_insert {0},{1}", YKienHieuChinh, id);

                    //Thông tin biển bản giao nhận
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 2);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);


                    TempData["msgSuccess"] = "<script>alert('Yêu cầu hiệu chỉnh thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "")
                {
               
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BN * (100 - detail.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }    
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);

                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);

                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);

                    TempData["msgSuccess"] = "<script>alert('xác nhận thành công');</script>";
                }
                // Gọi công việc không chặn
                //_ = Task.Run(() => RedirectToAction("SavePdf", "BM_11", new { id = id }));
                //var savpdf = _11Controller.SavePdf(id);
                //return RedirectToAction("SavePdf", "BM_11",new {id = id});
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
                var NhanVien_TT = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");



                var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                              select new Tbl_TaiKhoan
                                              {
                                                  ID_TaiKhoan = a.ID_TaiKhoan,
                                                  HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                              }).ToListAsync();

                ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen");
            }    
            else
            {
                var NhanVien_TT = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                         select new Tbl_TaiKhoan
                                         {
                                             ID_TaiKhoan = a.ID_TaiKhoan,
                                             HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                         }).ToListAsync();

                ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen", TrinhKy.ID_TaiKhoan);



                var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                              select new Tbl_TaiKhoan
                                              {
                                                  ID_TaiKhoan = a.ID_TaiKhoan,
                                                  HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
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


            var MaLo = await (from a in _context.Tbl_MaLo.Where(x => x.PhongBan.Contains(TenBP.Trim()))
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
            int BBGN_ID = 0;
            List<Tbl_ChiTiet_BienBanGiaoNhan> Tbl_ChiTiet_BienBanGiaoNhan = new List<Tbl_ChiTiet_BienBanGiaoNhan>();
            try
            {
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
                            ID_CT_BBGN = Convert.ToInt32(key.Key.Split('_')[1]),
                            ID_VatTu = Convert.ToInt32(formCollection["VatTu_" + key.Key.Split('_')[1]]),
                            MaLo = formCollection["lo_" + key.Key.Split('_')[1]],
                            DoAm_W = double.Parse(formCollection["doam_" + key.Key.Split('_')[1]]),
                            KhoiLuong_BG = double.Parse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]]),
                            //KL_QuyKho_BG = double.Parse(formCollection["quykhobg_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }

                }
                if (XacNhan == "0" && XacNhan != "")
                {
                    // Thông tin phê duyệt nhân viên thống kê
                    if (IDNhanVienTT == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                        return RedirectToAction("BoSungPhieu", "BM_11");
                    }
                    else if (IDNhanVien_TT_View == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê nhận BBGN');</script>";
                        return RedirectToAction("BoSungPhieu", "BM_11");
                    }
                    else
                    {
                        var check = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                        if(check == null)
                        {
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", id, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 0);
                        }
                        else
                        {
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", check.ID_TrinhKy, IDNhanVienTT, IDNhanVien_TT_View, 0);
                        }

                    }
                    // Thông tin bên nhận

                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        if (item.ID_VatTu == 0)
                        {
                            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", item.ID_CT_BBGN,
                                                                      ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN, item.GhiChu);

                        }
                        else
                        {
                            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", item.ID_CT_BBGN,
                                                                     item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                        }
                    }

                    TempData["msgSuccess"] = "<script>alert('Lưu thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "")
                {
                    var check = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    // Thông tin phê duyệt nhân viên thống kê
                    if (IDNhanVienTT == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê phê duyệt');</script>";
                        return RedirectToAction("BoSungPhieu", "BM_11");
                    }
                    else if (IDNhanVien_TT_View == 0)
                    {
                        TempData["msgSuccess"] = "<script>alert('Vui lòng chọn nhân viên thống kê nhận BBGN');</script>";
                        return RedirectToAction("BoSungPhieu", "BM_11");
                    }
                    else
                    {
                      
                        if (check == null)
                        {
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", id, IDNhanVienTT, IDNhanVien_TT_View, DateTime.Now, 1);
                        }
                        else
                        {
                            var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", check.ID_TrinhKy, IDNhanVienTT, IDNhanVien_TT_View, 1);
                        }    
                      
                    }
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == item.ID_CT_BBGN).FirstOrDefault();
                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        if (item.ID_VatTu == 0)
                        {
                            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", item.ID_CT_BBGN,
                                                                      ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, ID_CT.KhoiLuong_BN, ID_CT.KL_QuyKho_BN,item.GhiChu);

                        }
                        else
                        {
                            var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", item.ID_CT_BBGN,
                                                                     item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                        }
                    }


                    TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";

                    return RedirectToAction("Index_Detai", "BM_11", new { id = id });
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
                            KhoiLuong_BN = double.Parse(formCollection["khoiluongbn_" + key.Key.Split('_')[1]]),
                            //KL_QuyKho_BN = double.Parse(formCollection["quykhobn_" + key.Key.Split('_')[1]]),
                            GhiChu = formCollection["ghichu_" + key.Key.Split('_')[1]]
                        });
                    }

                }
                if (XacNhan == "0" && XacNhan != "")
                {
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        double QuyKho = (item.KhoiLuong_BN * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);

                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BN, KL_QuyKho, item.GhiChu);
                    }

                    var result_yeucau = _context.Database.ExecuteSqlRaw("EXEC Tbl_YeuCauHieuChinh_insert {0},{1}", YKienHieuChinh, id);

                    //Thông tin biển bản giao nhận
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 2);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);

                    TempData["msgSuccess"] = "<script>alert('Yêu cầu hiệu chỉnh thành công');</script>";
                }
                else if (XacNhan == "1" && XacNhan != "")
                {
                    var Up_Detail = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_BBGN == id).ToList();
                    foreach (var item in Up_Detail)
                    {
                        double QuyKho = (item.KhoiLuong_BN * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);

                        var up_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update_BN {0},{1},{2},{3}", item.ID_CT_BBGN, item.KhoiLuong_BG, KL_QuyKho, item.GhiChu);
                    }
                    var result = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);
                      var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG_Date {0},{1}", id, ThoiGianXuLyBN);
                    var result_ = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);

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
            DateTime endDay = startDay.AddMonths(1).AddDays(-1);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            ViewBag.TTList = new SelectList(_context.Tbl_TrangThai_PheDuyet.ToList(), "ID_TrangThai_PheDuyet", "TenTrangThai", ID_TrangThai);

            var res = await (from a in _context.Tbl_TrinhKyBoSung.Where(x => x.ID_TaiKhoan == TaiKhoan.ID_TaiKhoan && x.ID_TrangThai != 0)
                             join bb in _context.Tbl_BienBanGiaoNhan on a.ID_BBGN equals bb.ID_BBGN
                             select new Tbl_TrinhKyBoSung
                             {
                                 ID_TrinhKy = a.ID_TrinhKy,
                                 ID_BBGN = a.ID_BBGN,
                                 SoPhieu = bb.SoPhieu,
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                                 NgayXuLy = (DateTime)a.NgayXuLy,
                                 ID_TrangThai = a.ID_TrangThai
                             }).ToListAsync();

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


            var NhanVienTT = await (from a in _context.Tbl_TaiKhoan.Where(x=>x.ID_Quyen == 3)
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVienTT, "ID_TaiKhoan", "HoVaTen", ID_TrinhKy.ID_TaiKhoan);


            var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                          select new Tbl_TaiKhoan
                                          {
                                              ID_TaiKhoan = a.ID_TaiKhoan,
                                              HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                          }).ToListAsync();

            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen", ID_TrinhKy.ID_TaiKhoan_View);

            var MaLo = await (from a in _context.Tbl_MaLo.Where(x => x.PhongBan.Contains(TenBP.Trim()))
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
                            DoAm_W = double.Parse(formCollection["doam_" + key.Key.Split('_')[1]]),
                            KhoiLuong_BG = double.Parse(formCollection["khoiluongbg_" + key.Key.Split('_')[1]]),
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
                        double KL_QuyKho = Math.Round(QuyKho, 3);

                        if (ID_CT == null)
                        {
                            if (item.MaLo != "")
                            {
                                int Ma_Lo = Convert.ToInt32(item.MaLo);
                                var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                if (IDMaLo != null)
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                              item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);
                                }

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
                                if (item.MaLo != "")
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                        ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         ID_CT.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                             

                            }
                            else
                            {
                                if (item.MaLo != "")
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                    }

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
                else if (XacNhan == "1" && XacNhan != "")
                {
                    // Thông tin nhân viên thống kê phê duyệt
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, NhanVienTT, IDNhanVien_TT_View, 1);
                    // Thông tin bên nhận
                    var ThongTin_BN = _context.Tbl_TaiKhoan.Where(x => x.ID_TaiKhoan == IDTaiKhoan).FirstOrDefault();

                    var result_tt = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_update {0},{1},{2},{3},{4}", id,
                                                            ThongTin_BN.ID_TaiKhoan, ThongTin_BN.ID_PhongBan, ThongTin_BN.ID_PhanXuong, ThongTin_BN.ID_ChucVu);
                    foreach (var item in Tbl_ChiTiet_BienBanGiaoNhan)
                    {
                        var ID_CT_BBGN = item.ID_CT_BBGN - 100;
                        var ID_CT = _context.Tbl_ChiTiet_BienBanGiaoNhan.Where(x => x.ID_CT_BBGN == ID_CT_BBGN).FirstOrDefault();

                        double QuyKho = (item.KhoiLuong_BG * (100 - item.DoAm_W) / 100);
                        double KL_QuyKho = Math.Round(QuyKho, 3);
                        if (ID_CT == null)
                        {
                            if (item.MaLo != "")
                            {
                                int Ma_Lo = Convert.ToInt32(item.MaLo);
                                var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                if (IDMaLo != null)
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_insert {0},{1},{2},{3},{4},{5},{6},{7},{8}",
                                                                           item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu, id);
                                }

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
                                if (item.MaLo != "")
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                         ID_CT.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       ID_CT.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                             

                            }
                            else
                            {
                                if (item.MaLo != "")
                                {
                                    int Ma_Lo = Convert.ToInt32(item.MaLo);
                                    var IDMaLo = _context.Tbl_MaLo.Where(x => x.ID_MaLo == Ma_Lo).FirstOrDefault();
                                    if (IDMaLo != null)
                                    {
                                        var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                       item.ID_VatTu, item.MaLo, item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                    }

                                }
                                else
                                {
                                    var result_detail = _context.Database.ExecuteSqlRaw("EXEC Tbl_ChiTiet_BienBanGiaoNhan_update {0},{1},{2},{3},{4},{5},{6},{7},{8}", ID_CT_BBGN,
                                                                     item.ID_VatTu, "", item.DoAm_W, item.KhoiLuong_BG, KL_QuyKho, 0, 0, item.GhiChu);
                                }
                            
                            }
                        }

                    }

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
                if(ID_BB.ID_TrangThai_BBGN == 5)
                {
                    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan, 3);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);

                    var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 1);
                    var result_BN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 1);
                    var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 1);
                }   
                else
                {
                    if (ID_BB.ID_QuyTrinh == 2)
                    {
                        //Thông tin nhân viên thông kê phê duyệt
                        var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan, 3);
                        var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                        //Thông tin biển bản giao nhận
                        var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 4);
                        var result_BN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 4);
                        var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 4);
                    }
                    else if (ID_BB.ID_QuyTrinh == 3)
                    {
                        //Thông tin nhân viên thông kê phê duyệt
                        var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                        var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan, 3);
                        var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                        //Thông tin biển bản giao nhận
                        var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 4);
                        var result_BN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 4);
                        var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 4);
                        //Thông tin phiếu cũ
                        var ID_BBGN_Cu = _context.Tbl_BienBanGiaoNhan.Where(x => x.ID_BBGN == ID_BB.ID_BBGN_Cu).FirstOrDefault();
                        var result_BG_Cu = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", ID_BBGN_Cu.ID_BBGN, 1);
                        var result_BN_Cu = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", ID_BBGN_Cu.ID_BBGN, 1);
                        var result_BBGN_Cu = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", ID_BBGN_Cu.ID_BBGN, 1);


                    }

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
                if (ID_BB.ID_TrangThai_BBGN == 5)
                {
                    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                }  
                else
                {
                    //Thông tin nhân viên thông kê phê duyệt
                    var ID_TK = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", ID_TK.ID_TrinhKy, ID_TK.ID_TaiKhoan, ID_TK.ID_TaiKhoan_View, 2);
                    var result_date = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update_date {0},{1}", ID_TK.ID_TrinhKy, NgayXuLy);
                    //Thông tin biển bản giao nhận
                    var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 1);
                }
                //var savpdf = _11Controller.SavePdf(id);
                TempData["msgSuccess"] = "<script>alert('Xác nhận phiếu thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Xác nhận phiếu thất bại');</script>";
            }
            return RedirectToAction("PhieuBoSung", "XuLyPhieu");
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
                             join bb in _context.Tbl_BienBanGiaoNhan on a.ID_BBGN equals bb.ID_BBGN
                             select new Tbl_TrinhKyBoSung
                             {
                                 ID_TrinhKy = a.ID_TrinhKy,
                                 ID_BBGN = a.ID_BBGN,
                                 SoPhieu = bb.SoPhieu,
                                 ID_TaiKhoan = a.ID_TaiKhoan,
                                 NgayTrinhKy = (DateTime)a.NgayTrinhKy,
                                 NgayXuLy = (DateTime)a.NgayXuLy,
                                 ID_TrangThai = a.ID_TrangThai
                             }).ToListAsync();

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


        public async Task<IActionResult> HuyPhieu_PheDuyet(int id)
        {

            var NhanVien_TT = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                     select new Tbl_TaiKhoan
                                     {
                                         ID_TaiKhoan = a.ID_TaiKhoan,
                                         HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                     }).ToListAsync();

            ViewBag.NhanVienTT = new SelectList(NhanVien_TT, "ID_TaiKhoan", "HoVaTen");



            var NhanVien_TT_View = await (from a in _context.Tbl_TaiKhoan.Where(x => x.ID_Quyen == 3)
                                          select new Tbl_TaiKhoan
                                          {
                                              ID_TaiKhoan = a.ID_TaiKhoan,
                                              HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                          }).ToListAsync();

            ViewBag.NhanVien_TT_View = new SelectList(NhanVien_TT_View, "ID_TaiKhoan", "HoVaTen");

            return PartialView();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyPhieu_PheDuyet(Tbl_TrinhKyBoSung _DO, int id)
        {
            DateTime NgayTao = DateTime.Now;
            try
            {
                var check = _context.Tbl_TrinhKyBoSung.Where(x => x.ID_BBGN == id).FirstOrDefault();
                if(check != null)
                {
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_update {0},{1},{2},{3}", check.ID_TrinhKy, _DO.ID_TaiKhoan, _DO.ID_TaiKhoan, 1);
                }
                else
                {
                    var result_nvtt = _context.Database.ExecuteSqlRaw("EXEC Tbl_TrinhKyBoSung_insert {0},{1},{2},{3},{4}", id, _DO.ID_TaiKhoan, _DO.ID_TaiKhoan_View, DateTime.Now, 1);

                }


                var result_BG = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBG {0},{1}", id, 5);
                var result_BN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBN {0},{1}", id, 0);
                var result_BBGN = _context.Database.ExecuteSqlRaw("EXEC Tbl_BienBanGiaoNhan_XacNhanBBGN {0},{1}", id, 5);
                TempData["msgSuccess"] = "<script>alert('Trình ký thành công');</script>";
            }
            catch (Exception e)
            {
                TempData["msgError"] = "<script>alert('Trình ký thất bại');</script>";
            }

            return RedirectToAction("Index", "BM_11");
        }
    }
}
