namespace ProductService.Domain.Products;

public enum ProductStatus
{
    Draft = 0,       // черновик, ещё не продаётся
    Active = 1,      // опубликован, доступен к покупке
    Suspended = 2,   // приостановлен 
    Archived = 3     // архив
}