namespace AplicacaoAtividade03.DTOs
{
    public class AtendimentoPostDTO
    {
        public int IdPessoa { get; set; }
        public DateOnly DataAtendimento { get; set; }
        public string StatusAtendimento { get; set; }

        public List<AtendimentoItemPostDTO> Itens { get; set; } = new();
    }
}
