using Shared.Abstractions;

namespace ProductService.Domain.Categories;

public class Category : Entity<Guid>
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public Guid ParentCategoryId { get; private set; }
    
    private Category() : base(Guid.NewGuid()) {} // MongoDb
    
    private Category(Guid id, string name, string description, Guid parentCategoryId) : base(id)
    {
        Name = name;
        Description = description;
        ParentCategoryId = parentCategoryId;
    }

    public static Category Create(Guid id, string name, string description, Guid parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        return new Category(Guid.NewGuid(), name, description, parentCategoryId);
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        Name = name;
        Description = description;
    }
}