namespace AplicacaoAtividade03.DTOs
{
    public class AtendimentoListDTO
    {
        public int IdAtendimento { get; set; }
        public int IdPessoa { get; set; }
        public string NomePessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }

        public List<AtendimentoItemListDTO> Itens { get; set; } = new();
    }
}
