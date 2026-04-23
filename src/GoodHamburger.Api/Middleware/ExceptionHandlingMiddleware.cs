using FluentValidation;
using GoodHamburger.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GoodHamburger.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        Log.Error(exception, "Exceção não tratada: {Message}", exception.Message);

        var (statusCode, title, detail) = exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Requisição inválida",
                string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),

            DuplicateItemException di => (
                StatusCodes.Status409Conflict,
                "Item duplicado",
                di.Message),

            InvalidOrderException io when io.IsNotFound => (
                StatusCodes.Status404NotFound,
                "Pedido não encontrado",
                io.Message),

            InvalidOrderException io => (
                StatusCodes.Status422UnprocessableEntity,
                "Regra de negócio violada",
                io.Message),

            InvalidDiscountRuleException dr when dr.IsNotFound => (
                StatusCodes.Status404NotFound,
                "Regra de desconto não encontrada",
                dr.Message),

            InvalidDiscountRuleException dr when dr.IsConflict => (
                StatusCodes.Status409Conflict,
                "Conflito de regra de desconto",
                dr.Message),

            InvalidDiscountRuleException dr => (
                StatusCodes.Status422UnprocessableEntity,
                "Dados inválidos para regra de desconto",
                dr.Message),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Erro interno",
                "Ocorreu um erro inesperado. Tente novamente mais tarde.")
        };

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
