namespace AplicacaoAtividade03.DTOs
{
    // DTO DE ENTRADA (POST E PUT) DO FUNCIONARIO
    // REUNE DADOS DAS TABELAS PESSOA, FUNCIONARIO E ENDERECO NUM UNICO OBJETIVO
    public class FuncionarioDTO
    {
        // DADOS TABELA PESSOA
        public string? Nome { get; set; }  

        //CPF E TELEFONE COMO TEXTO : NO BANCO SÃO BIGINT E PERDERIAM O ZERO Á ESQUEDA
        public string? CPF { get; set; }

        //DateOnly?  PERMITE INDENTIFICAR QUANDO O CAMPO NÃO FOI ENVIADO(null)
        public DateOnly? DataNascimento { get; set; }
        public string? Sexo { get; set; }
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? AtivoPessoa { get; set; }

        //DADOS TABELA  FUNCIONARIO
        public string? Cargo { get; set; }
        public DateOnly? DataAdmissao { get; set; }

        //DADOS TABELA ENDERECO (UMA PESSOA PODE TER VARIOS)
        public List<EnderecoDTO> Enderecos { get; set; } = new();

    }
}
