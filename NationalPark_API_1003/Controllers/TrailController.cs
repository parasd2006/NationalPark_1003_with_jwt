using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NationalPark_API_1003.Models;
using NationalPark_API_1003.Models.DTOs;
using NationalPark_API_1003.Repository.IRepository;

namespace NationalPark_API_1003.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrailController : Controller
    {
        private readonly ITrailRepository _trailRepository;
        private readonly IMapper _mapper;
        public TrailController(ITrailRepository trailRepository,
            IMapper mapper)
        {
            _trailRepository = trailRepository;
            _mapper = mapper;
        }
        [HttpGet]
        public IActionResult GetTrail()
        {
            return Ok(_trailRepository.GetTrails().Select(_mapper.Map<TrailDto>));
           }
        [HttpGet("{trailId:int}",Name ="GetTrails")]
        public IActionResult GrtTrails(int trailId)
        {
            var trail = _trailRepository.GetTrail(trailId);
            if (trail == null) return NotFound();
            var trailDto = _mapper.Map<Trail, TrailDto>(trail);
            if(trailDto==null) return NotFound();
            return Ok(trailDto);

        }
        [HttpPost]
        public IActionResult CreateTrail([FromBody]TrailDto trailDto)
        {
            if (trailDto == null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            if(_trailRepository.TrailExists(trailDto.Name))
            {
                ModelState.AddModelError("", "Trial in db...");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            var trail = _mapper.Map<TrailDto, Trail>(trailDto);
            if(!_trailRepository.CreateTrail(trail))
            {
                ModelState.AddModelError("", "unable to create taril..");
                return StatusCode(StatusCodes.Status500InternalServerError);

            }
            return CreatedAtRoute("GetTrails", new { trailId = trail.Id }, trail);
        }
        [HttpPut]
        public IActionResult UpdateTrail([FromBody]TrailDto trailDto)
        {
            if(trailDto==null) return BadRequest();
            if (!ModelState.IsValid) return BadRequest();
            var trail = _mapper.Map<TrailDto, Trail>(trailDto);
            if(!_trailRepository.UpdateTrail(trail))
            {
                ModelState.AddModelError("", "Unable to update...");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return NoContent();
        }
        [HttpDelete("{trailId:int}")]
        public IActionResult DeleteTrail(int trailId)
        {
            if (!_trailRepository.TrailExists(trailId))
                return NotFound();
            var trail = _trailRepository.GetTrail(trailId);
            if (trail == null) return NotFound();
            if(!_trailRepository.DeleteTrail(trail))
            {
                ModelState.AddModelError("", "Unable to delete data");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            return Ok();
        }
    }
}
