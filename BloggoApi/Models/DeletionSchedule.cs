using BloggoApi.Models;
using Microsoft.EntityFrameworkCore;

public class DeletionSchedule
{
    public Guid DeletionId { get; set; } = Guid.CreateVersion7();

    // TODO: find a way to define an PK(this.Id, Either<BlogPost, User>) constraint
    // to avoid nullable fields. We ideally want to encode our invariants in the typesystem.
    // For now, these are considered foreign keys in a 1 <- -> 1 relation from this type's side
    public BlogPost? Post { get; set; }
    public User? User { get; set; }

    public required DateTimeOffset _when { get; set; }
}
