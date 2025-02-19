using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using nuxt_shop.Models;

namespace nuxt_shop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HangHoaController : ControllerBase
    {
        public static List<HangHoa> hangHoas = new List<HangHoa>();
        [HttpGet]
        public IActionResult GetAll()
        {
            return Ok(hangHoas);
        }
        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                // LINQ | Query
                var hangHoa = hangHoas.SingleOrDefault(hh => hh.MaHangHoa == Guid.Parse(id));
                if (hangHoa == null)
                {
                    return NotFound();
                }
                return Ok(hangHoa);

            } catch
            {
                return BadRequest();
            }
        }
        [HttpPost]
        public IActionResult Create(HangHoaVM hanghoaVM)
        {
            var hanghoa = new HangHoa
            {
                MaHangHoa = Guid.NewGuid(),
                TenHangHoa = hanghoaVM.TenHangHoa,
                DonGia = hanghoaVM.DonGia
            };
            hangHoas.Add(hanghoa);
            return Ok(new {
                Success = true,
                Data = hanghoa
            });
        }
        [HttpPut]
        public IActionResult Edit(HangHoa hangHoaEdit)
        {
            try
            {
                // LINQ [Object] Query
                var hangHoa = hangHoas.SingleOrDefault(hh => hh.MaHangHoa == hangHoaEdit.MaHangHoa);
                if (hangHoa == null)
                {
                    return NotFound();
                }
                // update
                hangHoa.TenHangHoa = hangHoaEdit.TenHangHoa;
                hangHoa.DonGia = hangHoaEdit.DonGia;
                return Ok(new {
                    Success = true,
                    Data = hangHoa
                });

            } catch
            {
               return BadRequest();
            }
        }
        [HttpDelete("{id}")]
        public IActionResult Remove(string id)
        {
            try
            {
                // LINQ | Query
                var hangHoa = hangHoas.SingleOrDefault(hh => hh.MaHangHoa == Guid.Parse(id));
                if (hangHoa == null)
                {
                    return NotFound();
                }
                hangHoas.Remove(hangHoa);
                return Ok();

            }
            catch
            {
                return BadRequest();
            }
        }
    }
}
