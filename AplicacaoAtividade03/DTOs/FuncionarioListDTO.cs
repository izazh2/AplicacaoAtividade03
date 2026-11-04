namespace AplicacaoAtividade03.DTOs
{
    // DTO DE SAIDA: FUNCIONARIO COM OS DADOS DA PESSOA E OS ENDERECOS
    public class FuncionarioListDTO
    {
        public int IdPessoa { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string CPF { get; set; } = string.Empty;
        public DateOnly DataNascimento { get; set; }
        public string Sexo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string AtivoPessoa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public DateOnly DataAdmissao { get; set; }

        public List<EnderecoListDTO> Enderecos { get; set; } = new();
    }
}
