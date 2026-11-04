namespace AplicacaoAtividade03.DTOs
{
    // DTO DE SAIDA: ENDERECO DEVOLVIDO PELA API
    public class EnderecoListDTO
    {
        public int IdEndereco { get; set; }
        public string CEP { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string Complemento { get; set; } = string.Empty;
    }
}
