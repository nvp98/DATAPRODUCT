using Data_Product.DTO.BM_16_DTO;
using Data_Product.Repositorys;
using Data_Product.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;

namespace Data_Product.Controllers
{
    public class BM_16_LuyenThepHRC1Controller : Controller
    {
        private readonly DataContext _context;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly IHRC1LuyenThepService _hrc1LuyenThepService;

        public BM_16_LuyenThepHRC1Controller(DataContext _context, ICompositeViewEngine viewEngine, IHRC1LuyenThepService hrc1LuyenThepService)
        {
            this._context = _context;
            _viewEngine = viewEngine;
            this._hrc1LuyenThepService = hrc1LuyenThepService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpsertMeGangVaoLo([FromBody] TaoMoiMeGangVaoLoDto payload)
        {
            try
            {
                var upsertMeGang = await this._hrc1LuyenThepService.UpsertMeGangVaoLoAsync(payload);
                return Ok(new { upsertMeGang });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChuyenMe([FromBody] ChuyenMeGangDto dto)
        {
            try
            {
                await this._hrc1LuyenThepService.ChuyenMeGangAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> ThuHoiMe([FromBody] ThuHoiMeDto dto)
        {
            try
            {
                await this._hrc1LuyenThepService.ThuHoiMeGangAsync(dto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> LayThongTinMeAT([FromBody] ThongTinMeThoiATDto payload)
        {
            try
            {
               
                var data = await this._hrc1LuyenThepService.ThongTinMeThoiAT(payload);
                return Ok(new { data });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> XoaMeTaoTay([FromBody] XoaMeTaoTayDto payload)
        {
            try
            {

                var kq = await this._hrc1LuyenThepService.XoaMeTaoTayAsync(payload);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }
}
