namespace AplicacaoAtividade03.DTOs
{
    public class AtendimentoItemPostDTO
    {
        public int IdItem { get; set; }
        public int IdFuncionario { get; set; }
        public int Quantidade { get; set; }
        public Decimal ValorUnitario { get; set; }
    }
}
