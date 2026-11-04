namespace AplicacaoAtividade03.DTOs
{
    // DTO DE ENTRADA: ENDERECO RECEBIDO NO CADASTRO E NA ALTERACAO DO FUNCIONARIO
    public class EnderecoDTO
    {
        // NA ALTERACAO: COM ID = ATUALIZA O ENDERECO / SEM ID = ENDERECO NOVO
        // NO CADASTRO O ID E IGNORADO
        public int? IdEndereco { get; set; }
        public string? CEP { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
    }
}
