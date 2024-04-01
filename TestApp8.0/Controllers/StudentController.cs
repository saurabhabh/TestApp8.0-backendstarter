using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApp8._0.Dto;
using TestApp8._0.Service;


namespace TestApp8._0.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    // [RequiredScope("wishlist.write")]
    public class StudentController : Controller
    {
        const string scopeRequiredByAPI = "access_as_user";

        // In-memory StudentList
        private static readonly Dictionary<int, StudentDto> StudentStore = new Dictionary<int, StudentDto>();

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IStudentService _studentService;
        private readonly IMapper _mapper;
        public StudentController(IHttpContextAccessor contextAccessor, IStudentService studentService, IMapper mapper)
        {
            _mapper = mapper;
            _studentService = studentService;
            this._contextAccessor = contextAccessor;

            // Pre-populate with sample data
            if (StudentStore.Count == 0)
            {
                StudentStore.Add(1, new StudentDto() { Id = 1, Name = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Age = "10" });
                StudentStore.Add(2, new StudentDto() { Id = 2, Name = $"{this._contextAccessor.HttpContext.User.Identity.Name}", Age = "12" });
            }
        }

        // GET: api/values
        [HttpGet]
        public List<StudentDto> Get()
        {
            var list = _studentService.GetAllEntitiesAsync().Result;
            var studentList = _mapper.Map<List<Domain.Entities.Student>, List<StudentDto>>(list);
            return studentList;

        }

        // GET: api/values
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            var d = _studentService.GetEntityByIdAsync(id);
            return Ok(d.Result);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            _studentService.DeleteEntityByID(id);
            _studentService.SaveChanges();
            return Ok("Deleted :" + id);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] StudentDto todo)
        {
           // int id = StudentStore.Values.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            StudentDto todonew = new StudentDto() { Name = todo.Name, Age = todo.Age };

            var newItem = _studentService.CreateEntityAsync<StudentDto, StudentDto>(todonew);

            return Ok(newItem.Result);
        }

        // PATCH api/values
        //[
        //    {
        //        "op": "replace",
        //        "path": "/Name",
        //        "value": "Sa1"
        //    }
        //]
        //
        //
        //

        [Consumes("application/json-patch+json")]
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<StudentDto> jsonPatchDocument)
        {
            var isExist = await _studentService.ExistAsync(x => x.Id == id);

            if (jsonPatchDocument != null && isExist)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var tododto = new StudentDto();
                jsonPatchDocument.ApplyTo(tododto);
                tododto.Id = id;
                var todoEntity = _mapper.Map<Domain.Entities.Student>(tododto);

                //partially update the chnages to the db. 
                await _studentService.UpdatePartialEntityAsync(todoEntity, jsonPatchDocument);
            }
            else
            {
                if (!isExist)
                {
                    return NotFound(ModelState);
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            return Ok();
        }
    }
}