using Data_Product.Repositorys;
using DocumentFormat.OpenXml.Spreadsheet;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Newtonsoft.Json;

namespace Data_Product.Controllers
{

    public enum TinhTrang
    {
        ChuaChuyen,
        ChoXuLy,
        DaNhan,
        DaChot,
        DaSuDung
    }

    public enum CaLamViec
    {
        Ngay, // 1 = "N"
        Dem, // 2 = "Đ"
    }


    public class BM_GangThoi_ThepModelView
    {
        public int Id { get; set; }
        public string GioNM { get; set; }
        public string MaThungGang { get; set; }
        public CaLamViec Ca { get; set; }
        public int LoThoi { get; set; }
        public int Locao { get; set; }
        public int ThungSo { get; set; }
        public double KLThung{ get; set; }
        public string ChuyenDen { get; set; }
        public bool? IsKR { get; set; }
        public bool IsChonThung { get; set; }
        public TinhTrang TinhTrang { get; set; }
        public List<string> HRC1 { get; set; }
        public List<string> HRC2 { get; set; }

        // kết hợp với khu vực xử lý số liệu
        public string MaThungThep { get; set; } // MaThungThep kế thừa từ MaThungGang theo format (MaThungGang + Ngày + Ca + Lò thổi + 00 (số lần nhận thùng + thêm 1) =>> EX: 17L2N001.00.17NT1.00)
        public double? KLThungVaGangLong { get; set; }
        public double? KLGangLong { get; set; }

        public int? ThungTrungGian { get; set; }
        public double? KLThungVaGangLongVaoLoThoi { get; set; }
        public double KLThungVaoLoThoi { get; set; }

        public string Methoi { get; set; }
        public string GhiChu { get; set; }

    }
    public class BM_GangThoi_ThepController : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;


        // Mockup data maaux
        private static List<BM_GangThoi_ThepModelView> allItems = new List<BM_GangThoi_ThepModelView>
        {
            new BM_GangThoi_ThepModelView { Id = 1, GioNM = "7:30", MaThungGang = "17L2N001.00", Ca = CaLamViec.Ngay, LoThoi = 1,  Locao = 2, ThungSo = 8, KLThung = 57.22, ChuyenDen = "HRC1", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.DaNhan, HRC1 = new List<string>{"Bùi Tá Trung", "Nguyễn Hoàng Hưng" }, HRC2 = new List<string>{"Đòan Văn Tây" }},
            new BM_GangThoi_ThepModelView { Id = 2, GioNM = "8:15", MaThungGang = "17L2N002.00", Ca = CaLamViec.Ngay, LoThoi = 2, Locao = 2, ThungSo = 11, KLThung = 59.22, ChuyenDen = "HRC1", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.DaNhan, HRC1 = new List<string>{"Bùi Tá Trung", "Nguyễn Hoàng Hưng" }, HRC2 = new List<string>{"Đòan Văn Tây" }},
            new BM_GangThoi_ThepModelView { Id = 3, GioNM = "9:20", MaThungGang = "17L2N003.00", Ca = CaLamViec.Dem, LoThoi = 3, Locao = 3, ThungSo = 33, KLThung = 59.22, ChuyenDen = "HRC2", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.DaNhan, HRC1 = new List<string>{ "Nguyễn Hoàng Hưng", "Nguyễn Hoàng Hưng" }, HRC2 = new List<string>{ }},
            new BM_GangThoi_ThepModelView { Id = 4, GioNM = "10:45", MaThungGang = "17L2N004.00", Ca = CaLamViec.Dem, LoThoi = 1, Locao = 4, ThungSo = 23, KLThung = 59.22, ChuyenDen = "HRC1", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.ChoXuLy, HRC1 = new List<string>{ }, HRC2 = new List<string>{ }},
            new BM_GangThoi_ThepModelView { Id = 5, GioNM = "14:20", MaThungGang = "17L2N005.00", Ca = CaLamViec.Ngay, LoThoi = 5, Locao = 1, ThungSo = 90, KLThung = 68.22, ChuyenDen = "HRC2", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.ChoXuLy, HRC1 = new List<string>{}, HRC2 = new List<string>{ }},
            new BM_GangThoi_ThepModelView { Id = 6, GioNM = "21:33", MaThungGang = "17L2N006.00", Ca = CaLamViec.Dem, LoThoi = 6, Locao = 2, ThungSo = 7, KLThung = 68.22, ChuyenDen = "HRC1", IsKR = false, IsChonThung = false, TinhTrang = TinhTrang.ChoXuLy, HRC1 = new List<string>{ }, HRC2 = new List<string>{}},

        };

        // Danh sách được nhận
        private static List<BM_GangThoi_ThepModelView> selectedItems = new();

        public BM_GangThoi_ThepController(DataContext _context, ICompositeViewEngine viewEngine)
        {
            this._context = _context;
            _viewEngine = viewEngine;
        }

        
        public IActionResult Index()
        {
            ViewBag.SelectedItems = selectedItems;
            return View(allItems);
        }

        [HttpPost]
        public JsonResult Nhan ([FromBody]List<int> selectedIds)
        {
            var selected = allItems.Where(x => selectedIds.Contains(x.Id))
            .Select(x =>
            {
                var selectedItem = JsonConvert.DeserializeObject<BM_GangThoi_ThepModelView>(
                    JsonConvert.SerializeObject(x));

                int countItem = selectedItems.Count(s => s.Id == x.Id);
                string countSelected = countItem < 10 ? "0" + countItem : countItem.ToString();

                string ca = x.Ca == CaLamViec.Ngay ? "N" : "Đ";
                string dayStr = DateTime.Now.Day.ToString("00");
                selectedItem.MaThungThep = $"{x.MaThungGang}.{dayStr}{ca}T{x.LoThoi}.{countSelected}";

                selectedItem.TinhTrang = TinhTrang.DaNhan;

                return selectedItem;
            }
            ).ToList();

            // check lại và chèn vào đúng vị trí
            foreach (var item in selected)
            {
                int existingIndex = selectedItems.FindLastIndex(x => x.Id == item.Id);
                if (existingIndex >= 0)
                {
                    selectedItems.Insert(existingIndex + 1, item);
                }
                else
                {
                    selectedItems.Add(item);
                }
            }
            return Json(selectedItems);
        }

        [HttpPost]
        public JsonResult HuyNhan([FromBody] List<int> selectedIds)
        {
            var selected = allItems
                .Where(x => selectedIds.Contains(x.Id))
                .ToList();

            foreach (var item in selected)
            {
                item.TinhTrang = TinhTrang.ChoXuLy;

                selectedItems.RemoveAll(x => x.Id == item.Id);
            }

            return Json(new { success = true, message = "Hủy nhận thành công!" });
        }
    }
}
