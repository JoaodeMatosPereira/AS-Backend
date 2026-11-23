namespace Apicafe.Dtos;

// Tipo de entrada (DTO): só os campos que o cliente pode enviar no POST e no PUT.
// O Id não está aqui de propósito: quem gera o identificador é o banco.
public record CafeInputDto(string Marca = "", int Volume = 0, string Tipo = "");
