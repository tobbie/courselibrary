using System;
using System.ComponentModel.DataAnnotations;
using CourseLibrary.API.ValidationAttributes;

namespace CourseLibrary.API.Models
{
    [CourseTitleMustBeDifferentFromDescription(ErrorMessage = "Title must be different from description")]
    public abstract class CourseForManipulationDto //: IValidatableObject
    {
        [Required(ErrorMessage = "The title field is required")]
        [MaxLength(100, ErrorMessage = "The maximum character length of this field is 100")]
        public string Title { get; set; }

        [MaxLength(1500, ErrorMessage = "The maximum character length of this field is 100")]
        public virtual string  Description { get; set; }

        /**
       public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
       {
           if (Title == Description) {
               yield return new ValidationResult("The provided description should be different from the title",
                   new[] {"CourseForCreationDto" });
           }
       }
       **/
    }
}
