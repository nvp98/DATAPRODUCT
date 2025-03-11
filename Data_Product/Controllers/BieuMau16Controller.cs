using Data_Product.Models;
using Data_Product.Repositorys;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using System.Security.Claims;

namespace Data_Product.Controllers
{
    public class BieuMau16Controller : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;

        public BieuMau16Controller(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }
        public ActionResult TaoPhieu()
        {
            DateTime DayNow = DateTime.Now;
            String Day = DayNow.ToString("dd/MM/yyyy");
            DateTime NgayLamViec = DateTime.ParseExact(Day, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None);

            var TenTaiKhoan = User.FindFirstValue(ClaimTypes.Name);
            var TaiKhoan = _context.Tbl_TaiKhoan.Where(x => x.TenTaiKhoan == TenTaiKhoan).FirstOrDefault();
            var PhongBan = _context.Tbl_PhongBan.Where(x => x.ID_PhongBan == TaiKhoan.ID_PhongBan).FirstOrDefault();
            string TenBP = PhongBan.TenNgan.ToString();

            List<Tbl_NhomVatTu> vt = _context.Tbl_NhomVatTu.ToList();
            ViewBag.VTList = new SelectList(vt, "ID_NhomVatTu", "TenNhomVatTu");


            List<Tbl_PhongBan> pb = _context.Tbl_PhongBan.ToList();
            ViewBag.ID_PhongBan = new SelectList(pb, "ID_PhongBan", "TenPhongBan");

            var NhanVien = (from a in _context.Tbl_TaiKhoan
                                  select new Tbl_TaiKhoan
                                  {
                                      ID_TaiKhoan = a.ID_TaiKhoan,
                                      HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                                  }).ToList();

            ViewBag.IDTaiKhoan = new SelectList(NhanVien, "ID_TaiKhoan", "HoVaTen");

            var VatTu = (from a in _context.Tbl_VatTu.Where(x => x.PhongBan.Contains(TenBP) && x.ID_TrangThai == 1)
                               select new Tbl_VatTu
                               {
                                   ID_VatTu = a.ID_VatTu,
                                   TenVatTu = a.TenVatTu
                               }).ToList();

            ViewBag.VTList = new SelectList(VatTu, "ID_VatTu", "TenVatTu");

            var MaLo = (from a in _context.Tbl_MaLo
                              select new Tbl_MaLo
                              {
                                  ID_MaLo = a.ID_MaLo,
                                  TenMaLo = a.TenMaLo
                              }).ToList();

            ViewBag.MLList = new SelectList(MaLo, "ID_MaLo", "TenMaLo");

            var CaKip = (from a in _context.Tbl_Kip.Where(x => x.NgayLamViec == NgayLamViec)
                               select new Tbl_Kip
                               {
                                   ID_Kip = a.ID_Kip,
                                   TenCa = a.TenCa
                               }).ToList();
            ViewBag.IDKip = new SelectList(CaKip, "ID_Kip", "TenCa");



            var NV = (from a in _context.Tbl_TaiKhoan
                            select new Tbl_TaiKhoan
                            {
                                ID_TaiKhoan = a.ID_TaiKhoan,
                                HoVaTen = a.TenTaiKhoan + " - " + a.HoVaTen
                            }).ToList();

            ViewBag.IDNhanVien = new SelectList(NV, "ID_TaiKhoan", "HoVaTen");
            return PartialView();
        }
    }
}
