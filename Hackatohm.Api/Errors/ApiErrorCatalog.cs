using Microsoft.AspNetCore.Http;

namespace Hackatohm.Api.Errors;

public static class ApiErrorCatalog
{
    public static readonly ApiErrorDefinition InvalidPayload =
        new(
            "invalid_payload",
            StatusCodes.Status400BadRequest,
            "Dados inválidos",
            "A requisição contém dados ausentes ou inválidos.");

    public static readonly ApiErrorDefinition SensorNotRegistered =
        new(
            "sensor_not_registered",
            StatusCodes.Status409Conflict,
            "Sensor não cadastrado",
            "Um ou mais sensores enviados não estão cadastrados.");

    public static readonly ApiErrorDefinition SensorAlreadyRegistered =
        new(
            "sensor_already_registered",
            StatusCodes.Status409Conflict,
            "Sensor já cadastrado",
            "Já existe um sensor cadastrado com esse identificador.");

    public static readonly ApiErrorDefinition SensorsAlreadyRegistered =
        new(
            "sensors_already_registered",
            StatusCodes.Status409Conflict,
            "Sensores já cadastrados",
            "Um ou mais sensores já estão associados a outro cadastro.");

    public static readonly ApiErrorDefinition MotorNotFound =
        new(
            "motor_not_found",
            StatusCodes.Status404NotFound,
            "Motor não encontrado",
            "O motor solicitado não foi encontrado.");

    public static readonly ApiErrorDefinition MotorReferenceNotFound =
        new(
            "motor_not_found",
            StatusCodes.Status400BadRequest,
            "Motor inválido",
            "O motor informado não existe.");

    public static readonly ApiErrorDefinition SensorNotFound =
        new(
            "sensor_not_found",
            StatusCodes.Status404NotFound,
            "Sensor não encontrado",
            "O sensor solicitado não foi encontrado.");

    public static readonly ApiErrorDefinition ReadingNotFound =
        new(
            "reading_not_found",
            StatusCodes.Status404NotFound,
            "Leitura não encontrada",
            "Nenhuma leitura foi encontrada para o sensor solicitado.");

    public static readonly ApiErrorDefinition RateLimited =
        new(
            "rate_limited",
            StatusCodes.Status429TooManyRequests,
            "Limite de requisições excedido",
            "Aguarde antes de enviar outra requisição.");

    public static readonly ApiErrorDefinition InternalError =
        new(
            "internal_error",
            StatusCodes.Status500InternalServerError,
            "Erro interno",
            "Ocorreu um erro inesperado ao processar a requisição.");

    public static readonly ApiErrorDefinition ServiceUnavailable =
        new(
            "service_unavailable",
            StatusCodes.Status503ServiceUnavailable,
            "Serviço indisponível",
            "O serviço está temporariamente indisponível.");

    public static readonly ApiErrorDefinition QualityRuleNotFound =
        new(
            "quality_rule_not_found",
            StatusCodes.Status404NotFound,
            "Regra de qualidade não encontrada",
            "A regra de qualidade solicitada não foi encontrada.");

    public static readonly ApiErrorDefinition QualityRuleLinkNotFound =
        new(
            "quality_rule_link_not_found",
            StatusCodes.Status404NotFound,
            "Vínculo de regra não encontrado",
            "O vínculo entre o motor e a regra de qualidade não foi encontrado.");

    public static readonly ApiErrorDefinition QualityRuleAlreadyLinked =
        new(
            "quality_rule_already_linked",
            StatusCodes.Status409Conflict,
            "Regra já vinculada",
            "A regra de qualidade já está vinculada a este motor.");
}
