namespace AplicacaoAtividade03.DTOs
{
    public class ClienteDTO
    {
        // Dados pessoais
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public DateOnly? DataNascimento { get; set; }
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }

        // Endereço do cliente
        public List<EnderecoDTO> Enderecos { get; set; } = new();
    }
}