public interface ICreatable
{
    DateTime CreatedAt { get; set; }
}

public interface IUpdatable
{
    DateTime UpdatedAt { get; set; }
}

public interface IDeletable
{
    bool Active { get; set; }
    DateTime? DeletedAt { get; set; }
}

public interface IOwnable
{
    int UserId { get; set; }
}

public interface IAccountable : ICreatable, IUpdatable, IDeletable
{

}

public interface IOwnableAccountable : IAccountable, IOwnable
{

}

