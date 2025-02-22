﻿
using System;
using System.Collections.Generic;
using AutoMapper;
using CourseLibrary.API.Models;
using CourseLibrary.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CourseLibrary.API.Controllers
{
    [ApiController]
    [Route("api/authors/{authorId}/courses")]
    [ResponseCache(CacheProfileName = "240SecondsCacheProfile")] //for controller wide caching
    public class CoursesController : ControllerBase
    {
        private readonly ICourseLibraryRepository _courseLibraryRepository;
        private readonly IMapper _mapper;

        public CoursesController(ICourseLibraryRepository courseLibraryRepository, IMapper mapper) {
            _courseLibraryRepository = courseLibraryRepository ?? throw new ArgumentNullException(nameof(courseLibraryRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        }

        [HttpGet]
        public ActionResult<IEnumerable<CourseDto>> GetCoursesForAuthor(Guid authorId) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }

            var coursesFroAuthorFromRepo = _courseLibraryRepository.GetCourses(authorId);
            return Ok(_mapper.Map<IEnumerable<CourseDto>>(coursesFroAuthorFromRepo));
        }

        [HttpGet("{courseId}", Name = "GetCourseForAuthor")]
        [ResponseCache(Duration =120, Location = ResponseCacheLocation.Any)] // for action method level only caching
        public ActionResult<CourseDto> GetCourseForAuthor(Guid authorId, Guid courseId) {

            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null) {
                return NotFound();
            }

            return Ok(_mapper.Map<CourseDto>(courseForAuthorFromRepo));
        }

        [HttpPost]
        public ActionResult<CourseDto> CreateCourseForAuthor(Guid authorId, CourseForCreationDto course) {
            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseEntity = _mapper.Map<Entities.Course>(course);
            _courseLibraryRepository.AddCourse(authorId, courseEntity);
            _courseLibraryRepository.Save();

            var courseToReturn = _mapper.Map<CourseDto>(courseEntity);
            return CreatedAtRoute("GetCourseForAuthor", new { authorId = authorId, courseId = courseToReturn.Id },
                courseToReturn);

        }

        [HttpPut("{courseId}")]
        public IActionResult UpdateCourseForAuthor(Guid authorId, Guid courseId, CourseForUpdateDto course) {

            if (!_courseLibraryRepository.AuthorExists(authorId))
            {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);

            if (courseForAuthorFromRepo == null) {
                //upsert course
                var courseToAdd = _mapper.Map<Entities.Course>(course);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();
                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor", new { authorId, courseId = courseToReturn.Id },
                courseToReturn);
            }

            _mapper.Map(course, courseForAuthorFromRepo);
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();

        }

        [HttpDelete("{courseId}")]
        public IActionResult DeleteCourseForAuthor(Guid authorId, Guid courseId) {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseForAuthorFromRepo == null) {
                return NotFound();
            }

            _courseLibraryRepository.DeleteCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();
        }

        [HttpPatch("{courseId}")]
        public ActionResult PartiallyUpdateCourseForAuthor(Guid authorId, Guid courseId,
            JsonPatchDocument<CourseForUpdateDto> patchDocument)
        {
            if (!_courseLibraryRepository.AuthorExists(authorId)) {
                return NotFound();
            }

            var courseForAuthorFromRepo = _courseLibraryRepository.GetCourse(authorId, courseId);
            if (courseForAuthorFromRepo == null) {
                //upsert patch effort if no course if found.
                var courseDto = new CourseForUpdateDto();
                patchDocument.ApplyTo(courseDto, ModelState);

                if (!TryValidateModel(courseDto)) {
                    return ValidationProblem(ModelState);
                }

                var courseToAdd = _mapper.Map<Entities.Course>(courseDto);
                courseToAdd.Id = courseId;
                _courseLibraryRepository.AddCourse(authorId, courseToAdd);
                _courseLibraryRepository.Save();

                var courseToReturn = _mapper.Map<CourseDto>(courseToAdd);
                return CreatedAtRoute("GetCourseForAuthor",
                    new { authorId, courseId = courseToReturn.Id }, courseToReturn);
                
            }
            var courseToPatch = _mapper.Map<CourseForUpdateDto>(courseForAuthorFromRepo);
            //add validation
            patchDocument.ApplyTo(courseToPatch , ModelState);
            if (!TryValidateModel(courseToPatch)) {
                return ValidationProblem(ModelState);
            };

            _mapper.Map(courseToPatch, courseForAuthorFromRepo);
            _courseLibraryRepository.UpdateCourse(courseForAuthorFromRepo);
            _courseLibraryRepository.Save();
            return NoContent();


        }

        public override ActionResult ValidationProblem([ActionResultObjectValue]ModelStateDictionary modelStateDictionary)
        {
            var options = HttpContext.RequestServices.GetRequiredService<IOptions<ApiBehaviorOptions>>();
            return (ActionResult)options.Value.InvalidModelStateResponseFactory(ControllerContext);
        }

    }
}
