namespace AplicacaoAtividade03.DTOs
{
    public class AtendimentoItemPutDto
    {
        public int IdAtendiemnto { get; set; }
        public int IdItem { get; set; }
        public int IdFuncionario { get; set; }
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }

    }
}
