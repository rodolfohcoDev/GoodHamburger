using GoodHamburger.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace GoodHamburger.Application.Dtos;

public record CreateMenuItemRequest(
    [property: Required(ErrorMessage = "Código é obrigatório.")]
    [property: MaxLength(50)]
    string Code,

    [property: Required(ErrorMessage = "Nome é obrigatório.")]
    [property: MaxLength(200)]
    string Name,

    [property: Range(0.01, 9999.99, ErrorMessage = "Preço deve ser maior que zero.")]
    decimal Price,

    ProductCategory Category);
